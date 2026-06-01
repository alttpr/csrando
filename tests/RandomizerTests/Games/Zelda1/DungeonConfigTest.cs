namespace RandomizerTests.Games.Zelda1;

using Randomizer.Games.Zelda1;

[TestClass]
public sealed class DungeonConfigTest
{
    [TestMethod]
    [DataRow(DungeonStyleOption.Progressive)]
    [DataRow(DungeonStyleOption.Wild)]
    [DataRow(DungeonStyleOption.Megadungeon)]
    [DataRow(DungeonStyleOption.Nightmare)]
    [DataRow(DungeonStyleOption.Minimal)]
    public void GetConfigForLevel_AllLevels_ProducesValidConfig(DungeonStyleOption style)
    {
        var rnd = new Random(42);
        for (int level = 1; level <= 9; level++)
        {
            var config = DungeonConfig.GetConfigForLevel(level, style, EnemyPlacementOption.Progressive, rnd);

            Assert.IsTrue(config.Width >= 4, $"Level {level}: Width {config.Width} too small");
            Assert.IsTrue(config.Width <= 8, $"Level {level}: Width {config.Width} too large");
            Assert.IsTrue(config.Height >= 4, $"Level {level}: Height {config.Height} too large");
            Assert.IsTrue(config.Height <= 8, $"Level {level}: Height {config.Height} too large");
            Assert.IsTrue(config.Rooms >= 6, $"Level {level}: Rooms {config.Rooms} too few");
            Assert.IsTrue(config.Rooms <= config.Width * config.Height - 2,
                $"Level {level}: Rooms {config.Rooms} exceeds grid capacity {config.Width * config.Height - 2}");
            Assert.IsTrue(config.Segments >= 1, $"Level {level}: Segments {config.Segments} too few");
            Assert.IsTrue(config.ItemCellars >= 0, $"Level {level}: ItemCellars {config.ItemCellars} negative");
        }
    }

    [TestMethod]
    public void GetConfigForLevel_Progressive_ScalesWithLevel()
    {
        var rnd = new Random(42);
        var config1 = DungeonConfig.GetConfigForLevel(1, DungeonStyleOption.Progressive, EnemyPlacementOption.Progressive, rnd);
        var config9 = DungeonConfig.GetConfigForLevel(9, DungeonStyleOption.Progressive, EnemyPlacementOption.Progressive, rnd);

        Assert.IsTrue(config9.Rooms > config1.Rooms,
            $"Level 9 should have more rooms ({config9.Rooms}) than level 1 ({config1.Rooms})");
        Assert.IsTrue(config9.Width >= config1.Width,
            $"Level 9 width ({config9.Width}) should be >= level 1 width ({config1.Width})");
    }

    [TestMethod]
    public void GetConfigForLevel_Nightmare_AlwaysMaxSize()
    {
        var rnd = new Random(42);
        for (int level = 1; level <= 9; level++)
        {
            var config = DungeonConfig.GetConfigForLevel(level, DungeonStyleOption.Nightmare, EnemyPlacementOption.Progressive, rnd);
            Assert.AreEqual(8, config.Width, $"Level {level}: Nightmare width should be 8");
            Assert.AreEqual(8, config.Height, $"Level {level}: Nightmare height should be 8");
            Assert.AreEqual(3, config.Segments, $"Level {level}: Nightmare should have 3 segments");
            Assert.IsTrue(config.Rooms >= 48, $"Level {level}: Nightmare rooms ({config.Rooms}) should be >= 48");
        }
    }

    [TestMethod]
    public void GetConfigForLevel_Nightmare_UsesRandomEnemyPlacement()
    {
        var rnd = new Random(42);
        var config = DungeonConfig.GetConfigForLevel(1, DungeonStyleOption.Nightmare, EnemyPlacementOption.Progressive, rnd);
        Assert.AreEqual(EnemyPlacementOption.Random, config.EnemyPlacement,
            "Nightmare style should override to Random enemy placement");
    }

    [TestMethod]
    public void GetConfigForLevel_NonNightmare_RespectsEnemyPlacementSetting()
    {
        var rnd = new Random(42);
        var config = DungeonConfig.GetConfigForLevel(1, DungeonStyleOption.Progressive, EnemyPlacementOption.Vanilla, rnd);
        Assert.AreEqual(EnemyPlacementOption.Vanilla, config.EnemyPlacement);
    }

    [TestMethod]
    public void GetConfigForLevel_Deterministic_SameSeedSameResult()
    {
        var rnd1 = new Random(12345);
        var rnd2 = new Random(12345);

        for (int level = 1; level <= 9; level++)
        {
            var config1 = DungeonConfig.GetConfigForLevel(level, DungeonStyleOption.Wild, EnemyPlacementOption.Progressive, rnd1);
            var config2 = DungeonConfig.GetConfigForLevel(level, DungeonStyleOption.Wild, EnemyPlacementOption.Progressive, rnd2);

            Assert.AreEqual(config1.Width, config2.Width, $"Level {level}: Width mismatch");
            Assert.AreEqual(config1.Height, config2.Height, $"Level {level}: Height mismatch");
            Assert.AreEqual(config1.Rooms, config2.Rooms, $"Level {level}: Rooms mismatch");
            Assert.AreEqual(config1.Segments, config2.Segments, $"Level {level}: Segments mismatch");
        }
    }

    [TestMethod]
    public void GetConfigForLevel_Megadungeon_FloorsAtLevel6Baseline()
    {
        var rnd = new Random(42);
        var config1 = DungeonConfig.GetConfigForLevel(1, DungeonStyleOption.Megadungeon, EnemyPlacementOption.Progressive, rnd);

        Assert.AreEqual(8, config1.Width, "Megadungeon should use 8x8 grid");
        Assert.AreEqual(8, config1.Height, "Megadungeon should use 8x8 grid");
        Assert.IsTrue(config1.Rooms >= 36, $"Megadungeon level 1 should have >= 36 rooms, got {config1.Rooms}");
        Assert.IsTrue(config1.Segments >= 2, $"Megadungeon should have >= 2 segments, got {config1.Segments}");
    }

    [TestMethod]
    public void GetConfigForLevel_Minimal_SmallestPossibleDungeons()
    {
        var rnd = new Random(42);
        for (int level = 1; level <= 9; level++)
        {
            var config = DungeonConfig.GetConfigForLevel(level, DungeonStyleOption.Minimal, EnemyPlacementOption.Progressive, rnd);
            Assert.AreEqual(4, config.Width, $"Level {level}: Minimal width should be 4");
            Assert.AreEqual(4, config.Height, $"Level {level}: Minimal height should be 4");
            Assert.AreEqual(6, config.Rooms, $"Level {level}: Minimal rooms should be 6");
            Assert.AreEqual(1, config.Segments, $"Level {level}: Minimal should have 1 segment");
            Assert.AreEqual(0, config.ItemCellars, $"Level {level}: Minimal should have 0 item cellars");
            Assert.AreEqual(0.0, config.DoorComplexity, $"Level {level}: Minimal should have 0 door complexity");
        }
    }
}
