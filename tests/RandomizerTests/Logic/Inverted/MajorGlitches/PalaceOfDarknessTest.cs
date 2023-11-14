namespace RandomizerTests.Logic.Inverted.MajorGlitches;

[TestClass]
public sealed class PalaceOfDarknessTest : InvertedMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Palace of Darkness - Big Key Chest", false, new string[] {  }],
        ["Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1" }],

        ["Palace of Darkness - The Arena - Ledge", false, new string[] {  }],
        ["Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows" }],

        ["Palace of Darkness - The Arena - Bridge", false, new string[] {  }],
        ["Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1" }],

        ["Palace of Darkness - Big Chest", false, new string[] {  }],
        ["Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "Lamp" }],

        ["Palace of Darkness - Compass Chest", false, new string[] {  }],
        ["Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1" }],

        ["Palace of Darkness - Harmless Hellway", false, new string[] {  }],
        ["Palace of Darkness - Harmless Hellway", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1" }],

        ["Palace of Darkness - Stalfos Basement", false, new string[] {  }],
        ["Palace of Darkness - Stalfos Basement", true, new string[] { "KeyD1" }],

        ["Palace of Darkness - Dark Basement - Left", false, new string[] {  }],
        ["Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp" }],

        ["Palace of Darkness - Dark Basement - Right", false, new string[] {  }],
        ["Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp" }],

        ["Palace of Darkness - Map Chest", false, new string[] {  }],
        ["Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows" }],

        ["Palace of Darkness - Dark Maze - Top", false, new string[] {  }],
        ["Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp" }],

        ["Palace of Darkness - Dark Maze - Bottom", false, new string[] {  }],
        ["Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp" }],

        ["Palace of Darkness - Shooter Room", true, new string[] {  }],

        ["Palace of Darkness - Boss", false, new string[] {  }],
        ["Palace of Darkness - Boss", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "Lamp", "Hammer", "BowAndArrows" }],
    ];
}
