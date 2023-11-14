namespace RandomizerTests.Logic.Standard.OverworldGlitches;

[TestClass]
public sealed class PalaceOfDarknessTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Palace of Darkness - Big Key Chest", false, new string[] {  }],
        ["Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "PegasusBoots" }],
        ["Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "TitansMitt" }],

        ["Palace of Darkness - The Arena - Ledge", false, new string[] {  }],
        ["Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MoonPearl", "PegasusBoots" }],
        ["Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MoonPearl", "TitansMitt" }],

        ["Palace of Darkness - The Arena - Bridge", false, new string[] {  }],
        ["Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MoonPearl", "PegasusBoots" }],
        ["Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MoonPearl", "TitansMitt" }],
        ["Palace of Darkness - The Arena - Bridge", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "PegasusBoots" }],

        ["Palace of Darkness - Big Chest", false, new string[] {  }],
        ["Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "PegasusBoots" }],
        ["Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "TitansMitt" }],

        ["Palace of Darkness - Compass Chest", false, new string[] {  }],
        ["Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "PegasusBoots" }],
        ["Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "TitansMitt" }],

        ["Palace of Darkness - Harmless Hellway", false, new string[] {  }],
        ["Palace of Darkness - Harmless Hellway", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "PegasusBoots" }],
        ["Palace of Darkness - Harmless Hellway", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Palace of Darkness - Harmless Hellway", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "TitansMitt" }],

        ["Palace of Darkness - Stalfos Basement", false, new string[] {  }],
        ["Palace of Darkness - Stalfos Basement", true, new string[] { "KeyD1", "MoonPearl", "PegasusBoots" }],
        ["Palace of Darkness - Stalfos Basement", true, new string[] { "KeyD1", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Palace of Darkness - Stalfos Basement", true, new string[] { "KeyD1", "MoonPearl", "TitansMitt" }],
        ["Palace of Darkness - Stalfos Basement", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "PegasusBoots" }],

        ["Palace of Darkness - Dark Basement - Left", false, new string[] {  }],
        ["Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "PegasusBoots" }],
        ["Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "TitansMitt" }],

        ["Palace of Darkness - Dark Basement - Right", false, new string[] {  }],
        ["Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "PegasusBoots" }],
        ["Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "TitansMitt" }],

        ["Palace of Darkness - Map Chest", false, new string[] {  }],
        ["Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MoonPearl", "PegasusBoots" }],
        ["Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MoonPearl", "TitansMitt" }],

        ["Palace of Darkness - Dark Maze - Top", false, new string[] {  }],
        ["Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "PegasusBoots" }],
        ["Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "TitansMitt" }],

        ["Palace of Darkness - Dark Maze - Bottom", false, new string[] {  }],
        ["Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "PegasusBoots" }],
        ["Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "TitansMitt" }],

        ["Palace of Darkness - Shooter Room", false, new string[] {  }],
        ["Palace of Darkness - Shooter Room", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Palace of Darkness - Shooter Room", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Palace of Darkness - Shooter Room", true, new string[] { "MoonPearl", "TitansMitt" }],

        ["Palace of Darkness - Boss", false, new string[] {  }],
        ["Palace of Darkness - Boss", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Hammer", "BowAndArrows", "PegasusBoots" }],
    ];
}
