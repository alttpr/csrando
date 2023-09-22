namespace RandomizerTests.Logic.Inverted.MajorGlitches;

[TestClass]
public sealed class TurtleRockTest : InvertedMajorGlitchesLogicTests
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
        new object[] { "Turtle Rock - Chain Chomps", true, new string[] { "MagicMirror", "MoonPearl" } },

        new object[] { "Turtle Rock - Compass Chest", false, new string[] {  } },
        new object[] { "Turtle Rock - Compass Chest", true, new string[] { "MagicMirror", "MoonPearl", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7" } },

        new object[] { "Turtle Rock - Roller Room - Left", false, new string[] {  } },
        new object[] { "Turtle Rock - Roller Room - Left", true, new string[] { "MagicMirror", "MoonPearl", "CaneOfSomaria", "FireRod", "KeyD7", "KeyD7", "KeyD7", "KeyD7" } },

        new object[] { "Turtle Rock - Roller Room - Right", false, new string[] {  } },
        new object[] { "Turtle Rock - Roller Room - Right", true, new string[] { "MagicMirror", "MoonPearl", "CaneOfSomaria", "FireRod", "KeyD7", "KeyD7", "KeyD7", "KeyD7" } },

        new object[] { "Turtle Rock - Big Chest", false, new string[] {  } },
        new object[] { "Turtle Rock - Big Chest", true, new string[] { "MagicMirror", "MoonPearl", "Hookshot", "BigKeyD7" } },
        new object[] { "Turtle Rock - Big Chest", true, new string[] { "MagicMirror", "MoonPearl", "CaneOfSomaria", "BigKeyD7" } },

        new object[] { "Turtle Rock - Big Key Chest", false, new string[] {  } },
        new object[] { "Turtle Rock - Big Key Chest", true, new string[] { "MagicMirror", "MoonPearl", "KeyD7", "KeyD7", "KeyD7", "KeyD7" } },

        new object[] { "Turtle Rock - Crystaroller Room", false, new string[] {  } },
        new object[] { "Turtle Rock - Crystaroller Room", true, new string[] { "MagicMirror", "MoonPearl", "BigKeyD7" } },
        new object[] { "Turtle Rock - Crystaroller Room", true, new string[] { "MagicMirror", "MoonPearl", "Lamp", "CaneOfSomaria" } },

        new object[] { "Turtle Rock - Eye Bridge - Bottom Left", false, new string[] {  } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "MagicMirror", "MoonPearl" } },

        new object[] { "Turtle Rock - Eye Bridge - Bottom Right", false, new string[] {  } },
        new object[] { "Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "MagicMirror", "MoonPearl" } },

        new object[] { "Turtle Rock - Eye Bridge - Top Left", false, new string[] {  } },
        new object[] { "Turtle Rock - Eye Bridge - Top Left", true, new string[] { "MagicMirror", "MoonPearl" } },

        new object[] { "Turtle Rock - Eye Bridge - Top Right", false, new string[] {  } },
        new object[] { "Turtle Rock - Eye Bridge - Top Right", true, new string[] { "MagicMirror", "MoonPearl" } },

        new object[] { "Turtle Rock - Boss", false, new string[] {  } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "MagicMirror", "MoonPearl", "UncleSword", "Bottle", "Bottle", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "MagicMirror", "MoonPearl", "ProgressiveSword", "Bottle", "Bottle", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "MagicMirror", "MoonPearl", "MasterSword", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "MagicMirror", "MoonPearl", "ProgressiveSword", "ProgressiveSword", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "MagicMirror", "MoonPearl", "L3Sword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "MagicMirror", "MoonPearl", "L4Sword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
        new object[] { "Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "MagicMirror", "MoonPearl", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" } },
    };
}
