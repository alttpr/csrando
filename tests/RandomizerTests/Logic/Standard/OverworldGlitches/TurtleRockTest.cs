namespace RandomizerTests.Logic.Standard.OverworldGlitches;

[TestClass]
public sealed class TurtleRockTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory.Concat(new[] { "TurtleRockEntryQuake" }).ToArray());
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Turtle Rock - Chain Chomps", false, new string[] {  } },
        new object[] { "Turtle Rock - Chain Chomps", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Turtle Rock - Chain Chomps", true, new string[] { "PegasusBoots", "MagicMirror", "Hammer" } },
        new object[] { "Turtle Rock - Chain Chomps", true, new string[] { "PegasusBoots", "MagicMirror", "TitansMitt" } },

        new object[] { "Turtle Rock - Compass Chest", false, new string[] {  } },
        new object[] { "Turtle Rock - Compass Chest", true, new string[] { "MoonPearl", "PegasusBoots", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7" } },
        new object[] { "Turtle Rock - Compass Chest", true, new string[] { "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7" } },
        new object[] { "Turtle Rock - Compass Chest", true, new string[] { "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7" } },

        new object[] { "Turtle Rock - Roller Room - Left", false, new string[] {  } },
        new object[] { "Turtle Rock - Roller Room - Left", true, new string[] { "MoonPearl", "PegasusBoots", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "FireRod" } },
        new object[] { "Turtle Rock - Roller Room - Left", true, new string[] { "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "FireRod" } },
        new object[] { "Turtle Rock - Roller Room - Left", true, new string[] { "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "FireRod" } },

        new object[] { "Turtle Rock - Roller Room - Right", false, new string[] {  } },
        new object[] { "Turtle Rock - Roller Room - Right", true, new string[] { "MoonPearl", "PegasusBoots", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "FireRod" } },
        new object[] { "Turtle Rock - Roller Room - Right", true, new string[] { "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "FireRod" } },
        new object[] { "Turtle Rock - Roller Room - Right", true, new string[] { "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "FireRod" } },

        new object[] { "Turtle Rock - Big Chest", false, new string[] {  } },
        new object[] { "Turtle Rock - Big Chest", true, new string[] { "MoonPearl", "PegasusBoots", "CaneOfSomaria", "BigKeyD7" } },
        new object[] { "Turtle Rock - Big Chest", true, new string[] { "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "BigKeyD7" } },
        new object[] { "Turtle Rock - Big Chest", true, new string[] { "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "BigKeyD7" } },
        new object[] { "Turtle Rock - Big Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Hookshot", "BigKeyD7" } },
        new object[] { "Turtle Rock - Big Chest", true, new string[] { "PegasusBoots", "MagicMirror", "Hammer", "Hookshot", "BigKeyD7" } },
        new object[] { "Turtle Rock - Big Chest", true, new string[] { "PegasusBoots", "MagicMirror", "TitansMitt", "Hookshot", "BigKeyD7" } },

        new object[] { "Turtle Rock - Big Key Chest", false, new string[] {  } },
        new object[] { "Turtle Rock - Big Key Chest", true, new string[] { "MoonPearl", "PegasusBoots", "KeyD7", "KeyD7", "KeyD7", "KeyD7" } },
        new object[] { "Turtle Rock - Big Key Chest", true, new string[] { "PegasusBoots", "MagicMirror", "Hammer", "KeyD7", "KeyD7", "KeyD7", "KeyD7" } },
        new object[] { "Turtle Rock - Big Key Chest", true, new string[] { "PegasusBoots", "MagicMirror", "TitansMitt", "KeyD7", "KeyD7", "KeyD7", "KeyD7" } },

        new object[] { "Turtle Rock - Crystaroller Room", false, new string[] {  } },
        new object[] { "Turtle Rock - Crystaroller Room", true, new string[] { "MoonPearl", "PegasusBoots", "BigKeyD7" } },
        new object[] { "Turtle Rock - Crystaroller Room", true, new string[] { "PegasusBoots", "MagicMirror", "Hammer", "BigKeyD7" } },
        new object[] { "Turtle Rock - Crystaroller Room", true, new string[] { "PegasusBoots", "MagicMirror", "TitansMitt", "BigKeyD7" } },

        new object[] { "Turtle Rock - Eye Bridge - Bottom Left", false, new string[] {  } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "Lamp", "Cape", "MoonPearl", "PegasusBoots", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "Lamp", "Cape", "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "Lamp", "Cape", "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "Lamp", "CaneOfByrna", "MoonPearl", "PegasusBoots", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "Lamp", "CaneOfByrna", "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "Lamp", "CaneOfByrna", "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "Lamp", "MirrorShield", "MoonPearl", "PegasusBoots", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "Lamp", "MirrorShield", "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "Lamp", "MirrorShield", "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "MoonPearl", "PegasusBoots", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },

        new object[] { "Turtle Rock - Eye Bridge - Bottom Right", false, new string[] {  } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "Lamp", "Cape", "MoonPearl", "PegasusBoots", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "Lamp", "Cape", "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "Lamp", "Cape", "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "Lamp", "CaneOfByrna", "MoonPearl", "PegasusBoots", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "Lamp", "CaneOfByrna", "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "Lamp", "CaneOfByrna", "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "Lamp", "MirrorShield", "MoonPearl", "PegasusBoots", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "Lamp", "MirrorShield", "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "Lamp", "MirrorShield", "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "MoonPearl", "PegasusBoots", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },

        new object[] { "Turtle Rock - Eye Bridge - Top Left", false, new string[] {  } },
        new object[] { "Turtle Rock - Eye Bridge - Top Left", true, new string[] { "Lamp", "Cape", "MoonPearl", "PegasusBoots", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Left", true, new string[] { "Lamp", "Cape", "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Left", true, new string[] { "Lamp", "Cape", "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Left", true, new string[] { "Lamp", "CaneOfByrna", "MoonPearl", "PegasusBoots", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Left", true, new string[] { "Lamp", "CaneOfByrna", "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Left", true, new string[] { "Lamp", "CaneOfByrna", "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Left", true, new string[] { "Lamp", "MirrorShield", "MoonPearl", "PegasusBoots", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Left", true, new string[] { "Lamp", "MirrorShield", "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Left", true, new string[] { "Lamp", "MirrorShield", "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Left", true, new string[] { "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "MoonPearl", "PegasusBoots", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Left", true, new string[] { "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Left", true, new string[] { "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },

        new object[] { "Turtle Rock - Eye Bridge - Top Right", false, new string[] {  } },
        new object[] { "Turtle Rock - Eye Bridge - Top Right", true, new string[] { "Lamp", "Cape", "MoonPearl", "PegasusBoots", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Right", true, new string[] { "Lamp", "Cape", "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Right", true, new string[] { "Lamp", "Cape", "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Right", true, new string[] { "Lamp", "CaneOfByrna", "MoonPearl", "PegasusBoots", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Right", true, new string[] { "Lamp", "CaneOfByrna", "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Right", true, new string[] { "Lamp", "CaneOfByrna", "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Right", true, new string[] { "Lamp", "MirrorShield", "MoonPearl", "PegasusBoots", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Right", true, new string[] { "Lamp", "MirrorShield", "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Right", true, new string[] { "Lamp", "MirrorShield", "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Right", true, new string[] { "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "MoonPearl", "PegasusBoots", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Right", true, new string[] { "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Eye Bridge - Top Right", true, new string[] { "Lamp", "ProgressiveShield", "ProgressiveShield", "ProgressiveShield", "PegasusBoots", "MagicMirror", "TitansMitt", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },

        new object[] { "Turtle Rock - Boss", false, new string[] {  } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "MoonPearl", "PegasusBoots", "UncleSword", "Bottle", "Bottle", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "MagicMirror", "TitansMitt", "UncleSword", "Bottle", "Bottle", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "MoonPearl", "PegasusBoots", "ProgressiveSword", "Bottle", "Bottle", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "MagicMirror", "TitansMitt", "ProgressiveSword", "Bottle", "Bottle", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "MoonPearl", "PegasusBoots", "MasterSword", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "MagicMirror", "TitansMitt", "MasterSword", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "MoonPearl", "PegasusBoots", "ProgressiveSword", "ProgressiveSword", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "MagicMirror", "TitansMitt", "ProgressiveSword", "ProgressiveSword", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "MoonPearl", "PegasusBoots", "L3Sword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "MagicMirror", "TitansMitt", "L3Sword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "MoonPearl", "PegasusBoots", "L4Sword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "MagicMirror", "TitansMitt", "L4Sword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "MoonPearl", "PegasusBoots", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "PegasusBoots", "MagicMirror", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
    };
}
