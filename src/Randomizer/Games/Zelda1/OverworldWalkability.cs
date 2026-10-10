namespace Randomizer.Games.Zelda1;

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Reads the per-screen overworld walkability grid (committed in the screen YAMLs, generated
/// offline by OverworldTilemap — the randomizer has no ROM at generation time) and answers the
/// flute (whirlwind) landing question the entrance shuffler needs.
///
/// Grid coordinates: 16 square columns (0-15) × 11 square rows (0-10). A square col c maps to
/// pixel X = c*$10; a square row r maps to ObjY = r*$10 + $4D (see InitMode_EnterRoom /
/// InitWhirlwind, Z_05.asm:1577 / Z_01.asm).
/// </summary>
internal static class OverworldWalkability
{
    public const int Columns = OverworldTilemap.Columns; // 16
    public const int Rows = OverworldTilemap.Rows;       // 11

    /// <summary>Parses a screen's `walkable` grid into a [col,row] bool array, or null if absent.</summary>
    public static bool[,]? Parse(YamlReader.Screen? screen)
    {
        var rows = screen?.walkable;
        if (rows == null || rows.Count == 0)
            return null;

        var grid = new bool[Columns, Rows];
        for (int r = 0; r < Rows && r < rows.Count; r++)
        {
            var line = rows[r];
            for (int c = 0; c < Columns && c < line.Length; c++)
                grid[c, r] = line[c] == '.';
        }
        return grid;
    }

    private static bool IsWalkable(bool[,] grid, int col, int row)
        => col >= 0 && col < Columns && row >= 0 && row < Rows && grid[col, row];

    // The whirlwind drops Link when it reaches screen X = $80, i.e. square column 8
    // (UpdateWhirlwind_Full, Z_01.asm:1798). So the flute lands him around column 8.
    private const int FluteDropColumn = 8;

    /// <summary>
    /// Computes the flute landing ObjY for a dungeon-entrance screen. The whirlwind drops Link at
    /// the level entrance (screen X ≈ $80), exactly as vanilla does — vanilla recorder_y_pos points
    /// Link right at the entrance square (often the cave tile itself), not at empty ground, and the
    /// engine lets him step in/out from there. So this returns the row of the entrance nearest
    /// column 8. Falls back to a column-8 walkable row, then null (caller keeps the vanilla value).
    /// </summary>
    public static int? ComputeFluteY(YamlReader.Screen screen)
    {
        var grid = Parse(screen);
        if (grid == null)
            return null;

        // 1. Land at the dungeon/cave entrance, like vanilla. Pick the entrance closest to the
        //    drop column so Link arrives where the engine scrolls him to.
        var entrance = (screen.entrances ?? Enumerable.Empty<int[]>())
            .OrderBy(e => System.Math.Abs(e[0] - FluteDropColumn))
            .FirstOrDefault();
        if (entrance != null)
            return RowToObjY(entrance[1]);

        // 2. No baked entrance square (e.g. a hidden/secret cave whose entrance is only drawn at
        //    runtime): land Link on the nearest walkable ground to the drop column so he is never
        //    stuck in a wall. Prefer the drop column, then widen outward.
        for (int dx = 0; dx < Columns; dx++)
        {
            foreach (int col in new[] { FluteDropColumn - dx, FluteDropColumn + dx })
            {
                if (col < 0 || col >= Columns) continue;
                foreach (int row in RowsByDistanceFrom(5))
                {
                    if (IsWalkable(grid, col, row))
                        return RowToObjY(row);
                }
            }
        }

        return null;
    }

    // Flute drop ObjY for a square row. recorder_y_pos is a full Y byte (vanilla uses $5D/$8D/$AD,
    // i.e. rows 1/4/6), so unlike the 3-bit cave exit row it is NOT masked to 0-7.
    private static int RowToObjY(int row) => (row << 4) + 0x4D;

    // Row indices ordered by distance from a centre row, centre first.
    private static IEnumerable<int> RowsByDistanceFrom(int centre)
        => Enumerable.Range(0, Rows).OrderBy(r => System.Math.Abs(r - centre));
}
