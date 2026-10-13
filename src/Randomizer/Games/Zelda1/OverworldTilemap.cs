namespace Randomizer.Games.Zelda1;

using System;
using System.Collections.Generic;

/// <summary>
/// Decodes a Zelda 1 overworld screen layout from the vanilla (PRG0) ROM into a
/// 16-wide × 11-tall grid of "squares", and derives a per-square walkability grid.
///
/// This is the source-of-truth model of a screen the NES engine itself uses
/// (column-directory + run-length-encoded square descriptors). Walkability is
/// derived from it; the same square model can later be re-encoded back into
/// RoomLayoutsOW to support full overworld-layout shuffling.
///
/// Verified against zelda1prg0.nes and the disassembly routine LayoutRoomOrCaveOW
/// (Z_05.asm). All offsets below are PRG offsets (no iNES header); callers pass
/// the headerless PRG image. See the z1-overworld-screen-tilemap-decode memory.
/// </summary>
internal static class OverworldTilemap
{
    public const int Columns = 16;   // square columns across the play area
    public const int Rows = 11;      // square rows ($0B) down the play area

    // ─── PRG0 data offsets (headerless image) ───
    // 16 column-descriptor bytes per screen, indexed by unique room id.
    private const int RoomLayoutsOW = 0x15418;
    // 16 pointers (32 bytes) to the column tables; NES bank-5 addresses.
    private const int ColumnDirectoryOW = 0x19D0F;
    // 64-byte tables mapping a 6-bit square index to its top tile.
    private const int PrimarySquaresOW = 0x1697C;
    // The first column-directory pointer is runtime-patched to the OW column heap,
    // which in the overworld is the static value below (== ColumnHeapOW0).
    private const int ColumnHeapOW0Nes = 0x9BD8;

    // Bank 5 ($8000-$BFFF) maps to PRG file 0x14000.
    private const int Bank5FileBase = 0x14000;
    private const int Bank5NesBase = 0x8000;

    // Link's walkability on the overworld: a tile is walkable if it is in the WalkableTiles
    // list (those get normalized to the open-ground tile $26 before the test) OR its raw value
    // is below the unwalkable threshold. (GetCollidingTileMoving + the ObjectFirstUnwalkableTile
    // compare, Z_07.asm:2272-2292 / 2407.) $8C is the default overworld threshold — it keeps
    // ground ($24/$26), sand ($84/$89) and the listed tiles walkable while blocking trees,
    // rocks, walls and water. (The < $84 edge-spawn rule in Z_05.asm:3554 is a stricter
    // monster-only rule and is NOT what gates Link.)
    public const int FirstBlockingTile = 0x8C;

    // Tiles the engine normalizes to walkable ($26) for Link before the threshold compare
    // (WalkableTiles, Z_07.asm:2132).
    private static readonly HashSet<int> AlwaysWalkableTiles =
        new() { 0x8D, 0x91, 0x9C, 0xAC, 0xAD, 0xCC, 0xD2, 0xD5, 0xDF };

    private static bool IsTileWalkable(int tile) => AlwaysWalkableTiles.Contains(tile) || tile < FirstBlockingTile;

    // Entrance squares produced by the secret/cave layout transform (Z_05.asm:5916-5942):
    // tree $E7 → stairs, rock wall $E6 → cave, special armos $EA → stairs. $F3 is the
    // baked dungeon/cave-entrance primary tile.
    private static readonly HashSet<int> EntrancePrimaryTiles = new() { 0x70, 0xF3, 0xE7, 0xE6, 0xEA };

    private static int Bank5NesToFile(int nesAddr) => Bank5FileBase + (nesAddr - Bank5NesBase);

    /// <summary>
    /// Decodes the 16×11 grid of square indices for one screen (the unique room id, i.e.
    /// the low 6 bits of the screen's LevelBlockAttrs D byte — the YAML `screen` field).
    /// </summary>
    public static int[,] DecodeSquares(byte[] prg, int uniqueRoomId)
    {
        // Resolve the 16 column-table pointers; entry 0 is the OW column heap.
        var tableFile = new int[16];
        for (int i = 0; i < 16; i++)
        {
            int nes = prg[ColumnDirectoryOW + i * 2] | (prg[ColumnDirectoryOW + i * 2 + 1] << 8);
            if (i == 0)
                nes = ColumnHeapOW0Nes;
            tableFile[i] = Bank5NesToFile(nes);
        }

        var squares = new int[Columns, Rows];
        int layoutBase = RoomLayoutsOW + uniqueRoomId * 0x10;

        for (int col = 0; col < Columns; col++)
        {
            byte descriptor = prg[layoutBase + col];
            int tableIndex = (descriptor & 0xF0) >> 4;
            int columnIndex = descriptor & 0x0F;
            int tableOffset = tableFile[tableIndex];

            // Skip to the columnIndex-th column start (bit7 marks a column start).
            int idx = -1;
            int remaining = columnIndex;
            while (true)
            {
                idx++;
                if ((prg[tableOffset + idx] & 0x80) != 0)
                {
                    if (remaining == 0)
                        break;
                    remaining--;
                }
            }

            // Emit exactly Rows squares. Bit6 ($40) repeats the descriptor once.
            int p = tableOffset + idx;
            int toggle = 0;
            for (int row = 0; row < Rows; row++)
            {
                byte sq = prg[p];
                squares[col, row] = sq & 0x3F;

                if ((sq & 0x40) != 0)
                {
                    toggle ^= 0x40;
                    if (toggle == 0)
                        p++; // second emission done; advance
                }
                else
                {
                    p++;
                }
            }
        }

        return squares;
    }

    /// <summary>Returns the primary (top) tile id for each square in the grid.</summary>
    public static int[,] DecodePrimaryTiles(byte[] prg, int uniqueRoomId)
    {
        var squares = DecodeSquares(prg, uniqueRoomId);
        var tiles = new int[Columns, Rows];
        for (int c = 0; c < Columns; c++)
        {
            for (int r = 0; r < Rows; r++)
                tiles[c, r] = prg[PrimarySquaresOW + squares[c, r]];
        }

        return tiles;
    }

    /// <summary>True where Link can stand: the square's primary tile is open ground (&lt; $84).</summary>
    public static bool[,] DecodeWalkable(byte[] prg, int uniqueRoomId)
    {
        var tiles = DecodePrimaryTiles(prg, uniqueRoomId);
        var walk = new bool[Columns, Rows];
        for (int c = 0; c < Columns; c++)
        {
            for (int r = 0; r < Rows; r++)
                walk[c, r] = IsTileWalkable(tiles[c, r]);
        }

        return walk;
    }

    /// <summary>
    /// Square positions where an entrance (stairs/cave/dungeon) is or can be drawn on this
    /// screen — baked $F3 entrances plus tree/rock-wall/armos squares the secret transform
    /// converts into entrances. Used to align exit positions with the visible entrance.
    /// </summary>
    public static List<(int col, int row)> FindEntranceSquares(byte[] prg, int uniqueRoomId)
    {
        var tiles = DecodePrimaryTiles(prg, uniqueRoomId);
        var result = new List<(int, int)>();
        for (int c = 0; c < Columns; c++)
        {
            for (int r = 0; r < Rows; r++)
            {
                if (EntrancePrimaryTiles.Contains(tiles[c, r]))
                    result.Add((c, r));
            }
        }

        return result;
    }

    /// <summary>
    /// Loads the headerless PRG image from a .nes file (strips the 16-byte iNES header).
    /// </summary>
    public static byte[] LoadPrg(string nesPath)
    {
        var rom = System.IO.File.ReadAllBytes(nesPath);
        if (rom.Length < 16 || rom[0] != 'N' || rom[1] != 'E' || rom[2] != 'S')
            throw new InvalidOperationException($"Not an iNES ROM: {nesPath}");
        var prg = new byte[rom.Length - 16];
        Array.Copy(rom, 16, prg, 0, prg.Length);
        return prg;
    }
}
