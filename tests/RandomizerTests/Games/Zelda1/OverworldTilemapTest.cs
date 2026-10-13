namespace RandomizerTests.Games.Zelda1;

using System;
using System.IO;
using System.Linq;
using Randomizer.Games.Zelda1;

[TestClass]
public sealed class OverworldTilemapTest
{
    // Vanilla Z1 PRG0 ROM. Override with the Z1_PRG0_ROM environment variable.
    private static readonly string RomPath =
        Environment.GetEnvironmentVariable("Z1_PRG0_ROM")
        ?? @"D:\Misc\xkas\multirando-asm\resources\zelda1prg0.nes";

    private static byte[]? TryLoadPrg()
        => File.Exists(RomPath) ? OverworldTilemap.LoadPrg(RomPath) : null;

    [TestMethod]
    public void Decode_Level1EntranceScreen_HasExpectedWalkableLayout()
    {
        var prg = TryLoadPrg();
        if (prg == null)
        {
            Assert.Inconclusive($"PRG0 ROM not found at {RomPath}; set Z1_PRG0_ROM.");
            return;
        }

        // Screen 0x36 is the Level 1 entrance area (unique room id).
        var walk = OverworldTilemap.DecodeWalkable(prg, 0x36);

        // The dungeon entrance ($f3) sits at square (col 7, row 4) and is NOT walkable.
        Assert.IsFalse(walk[7, 4], "Entrance square (7,4) should be blocked.");
        // Link emerges and stands on the open ground just below the entrance.
        Assert.IsTrue(walk[7, 5], "Square below the entrance (7,5) should be walkable.");
        // The screen border columns are walls.
        Assert.IsFalse(walk[0, 0], "Border (0,0) should be blocked.");

        // Sanity: the interior has a substantial walkable area.
        int walkableCount = 0;
        for (int c = 0; c < OverworldTilemap.Columns; c++)
        {
            for (int r = 0; r < OverworldTilemap.Rows; r++)
                if (walk[c, r]) walkableCount++;
        }

        Assert.IsTrue(walkableCount > 40, $"Expected a sizeable walkable area, got {walkableCount}.");
    }

    [TestMethod]
    public void Decode_AllScreens_ProduceFullGrids()
    {
        var prg = TryLoadPrg();
        if (prg == null)
        {
            Assert.Inconclusive($"PRG0 ROM not found at {RomPath}; set Z1_PRG0_ROM.");
            return;
        }

        // Every unique room id 0x00-0x7B decodes into a complete 16x11 grid without throwing.
        for (int uid = 0; uid <= 0x7B; uid++)
        {
            var squares = OverworldTilemap.DecodeSquares(prg, uid);
            Assert.AreEqual(OverworldTilemap.Columns, squares.GetLength(0));
            Assert.AreEqual(OverworldTilemap.Rows, squares.GetLength(1));
        }
    }
}
