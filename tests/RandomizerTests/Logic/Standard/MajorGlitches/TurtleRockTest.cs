namespace RandomizerTests.Logic.Standard.MajorGlitches;

[TestClass]
public sealed class TurtleRockTest : StandardMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory.Concat(new[] { "TurtleRockEntryQuake" }).ToArray());
    }

    public static IEnumerable<object[]> TestData => [
        ["Turtle Rock - Chain Chomps", true, new string[] {  }],

        ["Turtle Rock - Compass Chest", false, new string[] {  }],
        ["Turtle Rock - Compass Chest", true, new string[] { "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7" }],
        ["Turtle Rock - Compass Chest", true, new string[] { "CaneOfSomaria", "MoonPearl", "Quake", "UncleSword" }],
        ["Turtle Rock - Compass Chest", true, new string[] { "CaneOfSomaria", "Bottle", "Quake", "UncleSword" }],

        ["Turtle Rock - Roller Room - Left", false, new string[] {  }],
        ["Turtle Rock - Roller Room - Left", true, new string[] { "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "FireRod" }],
        ["Turtle Rock - Roller Room - Left", true, new string[] { "CaneOfSomaria", "MoonPearl", "Quake", "UncleSword", "FireRod" }],
        ["Turtle Rock - Roller Room - Left", true, new string[] { "CaneOfSomaria", "Bottle", "Quake", "UncleSword", "FireRod" }],

        ["Turtle Rock - Roller Room - Right", false, new string[] {  }],
        ["Turtle Rock - Roller Room - Right", true, new string[] { "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "FireRod" }],
        ["Turtle Rock - Roller Room - Right", true, new string[] { "CaneOfSomaria", "MoonPearl", "Quake", "UncleSword", "FireRod" }],
        ["Turtle Rock - Roller Room - Right", true, new string[] { "CaneOfSomaria", "Bottle", "Quake", "UncleSword", "FireRod" }],

        ["Turtle Rock - Big Chest", false, new string[] {  }],
        ["Turtle Rock - Big Chest", true, new string[] { "CaneOfSomaria", "BigKeyD7" }],
        ["Turtle Rock - Big Chest", true, new string[] { "Hookshot", "BigKeyD7" }],

        ["Turtle Rock - Big Key Chest", false, new string[] {  }],
        ["Turtle Rock - Big Key Chest", true, new string[] { "KeyD7", "KeyD7", "KeyD7", "KeyD7" }],

        ["Turtle Rock - Crystaroller Room", false, new string[] {  }],
        ["Turtle Rock - Crystaroller Room", true, new string[] { "BigKeyD7" }],
        ["Turtle Rock - Crystaroller Room", true, new string[] { "Lamp", "CaneOfSomaria", "MagicMirror", "MoonPearl" }],
        ["Turtle Rock - Crystaroller Room", true, new string[] { "Lamp", "CaneOfSomaria", "MagicMirror", "Bottle" }],

        ["Turtle Rock - Eye Bridge - Bottom Left", false, new string[] {  }],
        ["Turtle Rock - Eye Bridge - Bottom Left", false, new string[] { "MagicMirror" }],
        ["Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "Lamp", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "MagicMirror", "MoonPearl" }],
        ["Turtle Rock - Eye Bridge - Bottom Left", true, new string[] { "MagicMirror", "Bottle" }],

        ["Turtle Rock - Eye Bridge - Bottom Right", false, new string[] {  }],
        ["Turtle Rock - Eye Bridge - Bottom Right", false, new string[] { "MagicMirror" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "Lamp", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "MagicMirror", "MoonPearl" }],
        ["Turtle Rock - Eye Bridge - Bottom Right", true, new string[] { "MagicMirror", "Bottle" }],

        ["Turtle Rock - Eye Bridge - Top Left", false, new string[] {  }],
        ["Turtle Rock - Eye Bridge - Top Left", false, new string[] { "MagicMirror" }],
        ["Turtle Rock - Eye Bridge - Top Left", true, new string[] { "Lamp", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Left", true, new string[] { "MagicMirror", "MoonPearl" }],
        ["Turtle Rock - Eye Bridge - Top Left", true, new string[] { "MagicMirror", "Bottle" }],

        ["Turtle Rock - Eye Bridge - Top Right", false, new string[] {  }],
        ["Turtle Rock - Eye Bridge - Top Right", false, new string[] { "MagicMirror" }],
        ["Turtle Rock - Eye Bridge - Top Right", true, new string[] { "Lamp", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Eye Bridge - Top Right", true, new string[] { "MagicMirror", "MoonPearl" }],
        ["Turtle Rock - Eye Bridge - Top Right", true, new string[] { "MagicMirror", "Bottle" }],

        ["Turtle Rock - Boss", false, new string[] {  }],
        ["Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "UncleSword", "Bottle", "Bottle", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "ProgressiveSword", "Bottle", "Bottle", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "MasterSword", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "ProgressiveSword", "ProgressiveSword", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "L3Sword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "L4Sword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "Lamp", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "MagicMirror", "UncleSword", "Bottle", "Bottle", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "MagicMirror", "ProgressiveSword", "Bottle", "Bottle", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "MagicMirror", "MasterSword", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "MagicMirror", "ProgressiveSword", "ProgressiveSword", "Bottle", "Bottle", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "MagicMirror", "MoonPearl", "L3Sword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "MagicMirror", "MoonPearl", "L4Sword", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
        ["Turtle Rock - Boss", true, new string[] { "IceRod", "FireRod", "MagicMirror", "MoonPearl", "Hammer", "CaneOfSomaria", "KeyD7", "KeyD7", "KeyD7", "KeyD7", "BigKeyD7" }],
    ];
}
