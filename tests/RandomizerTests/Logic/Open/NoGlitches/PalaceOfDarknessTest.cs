namespace RandomizerTests.Logic.Open.NoGlitches;

[TestClass]
public class PalaceOfDarknessTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Palace of Darkness - Big Key Chest", false, new string[] {  }],
        ["Palace of Darkness - Big Key Chest", false, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "AgahnimDefeated" }],
        ["Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "AgahnimDefeated" }],
        ["Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Hammer", "PowerGlove" }],
        ["Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Hammer", "TitansMitt" }],
        ["Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Hammer", "ProgressiveGlove" }],
        ["Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Flippers", "TitansMitt" }],
        ["Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" }],

        ["Palace of Darkness - The Arena - Ledge", false, new string[] {  }],
        ["Palace of Darkness - The Arena - Ledge", false, new string[] { "MoonPearl", "AgahnimDefeated" }],
        ["Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MoonPearl", "AgahnimDefeated" }],
        ["Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MoonPearl", "Hammer", "PowerGlove" }],
        ["Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MoonPearl", "Hammer", "TitansMitt" }],
        ["Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MoonPearl", "Hammer", "ProgressiveGlove" }],
        ["Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MoonPearl", "Flippers", "TitansMitt" }],
        ["Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" }],

        ["Palace of Darkness - The Arena - Bridge", false, new string[] {  }],
        ["Palace of Darkness - The Arena - Bridge", false, new string[] { "KeyD1", "AgahnimDefeated" }],
        ["Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MoonPearl", "AgahnimDefeated" }],
        ["Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MoonPearl", "Hammer", "PowerGlove" }],
        ["Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MoonPearl", "Hammer", "TitansMitt" }],
        ["Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MoonPearl", "Hammer", "ProgressiveGlove" }],
        ["Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MoonPearl", "Flippers", "TitansMitt" }],
        ["Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Palace of Darkness - The Arena - Bridge", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "AgahnimDefeated" }],
        ["Palace of Darkness - The Arena - Bridge", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "PowerGlove" }],
        ["Palace of Darkness - The Arena - Bridge", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "TitansMitt" }],
        ["Palace of Darkness - The Arena - Bridge", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "ProgressiveGlove" }],

        ["Palace of Darkness - Big Chest", false, new string[] {  }],
        ["Palace of Darkness - Big Chest", false, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "AgahnimDefeated" }],
        ["Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "AgahnimDefeated" }],
        ["Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Hammer", "PowerGlove" }],
        ["Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Hammer", "TitansMitt" }],
        ["Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Hammer", "ProgressiveGlove" }],
        ["Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Flippers", "TitansMitt" }],
        ["Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Flippers", "ProgressiveGlove", "ProgressiveGlove" }],

        ["Palace of Darkness - Compass Chest", false, new string[] {  }],
        ["Palace of Darkness - Compass Chest", false, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "AgahnimDefeated" }],
        ["Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "AgahnimDefeated" }],
        ["Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Hammer", "PowerGlove" }],
        ["Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Hammer", "TitansMitt" }],
        ["Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Hammer", "ProgressiveGlove" }],
        ["Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Flippers", "TitansMitt" }],
        ["Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" }],

        ["Palace of Darkness - Harmless Hellway Chest", false, new string[] {  }],
        ["Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "AgahnimDefeated" }],
        ["Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Hammer", "PowerGlove" }],
        ["Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Hammer", "TitansMitt" }],
        ["Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Hammer", "ProgressiveGlove" }],
        ["Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Flippers", "TitansMitt" }],
        ["Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" }],

        ["Palace of Darkness - Stalfos Basement Chest", false, new string[] {  }],
        ["Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "MoonPearl", "AgahnimDefeated" }],
        ["Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "MoonPearl", "Hammer", "PowerGlove" }],
        ["Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "MoonPearl", "Hammer", "TitansMitt" }],
        ["Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "MoonPearl", "Hammer", "ProgressiveGlove" }],
        ["Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "MoonPearl", "Flippers", "TitansMitt" }],
        ["Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Palace of Darkness - Stalfos Basement Chest", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "AgahnimDefeated" }],
        ["Palace of Darkness - Stalfos Basement Chest", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "PowerGlove" }],
        ["Palace of Darkness - Stalfos Basement Chest", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "TitansMitt" }],
        ["Palace of Darkness - Stalfos Basement Chest", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "ProgressiveGlove" }],

        ["Palace of Darkness - Dark Basement - Left", false, new string[] {  }],
        ["Palace of Darkness - Dark Basement - Left", false, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "AgahnimDefeated" }],
        ["Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "AgahnimDefeated" }],
        ["Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Hammer", "PowerGlove" }],
        ["Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Hammer", "TitansMitt" }],
        ["Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Hammer", "ProgressiveGlove" }],
        ["Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Flippers", "TitansMitt" }],
        ["Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" }],

        ["Palace of Darkness - Dark Basement - Right", false, new string[] {  }],
        ["Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "AgahnimDefeated" }],
        ["Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Hammer", "PowerGlove" }],
        ["Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Hammer", "TitansMitt" }],
        ["Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Hammer", "ProgressiveGlove" }],
        ["Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Flippers", "TitansMitt" }],
        ["Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" }],

        ["Palace of Darkness - Map Chest", false, new string[] {  }],
        ["Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MoonPearl", "AgahnimDefeated" }],
        ["Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MoonPearl", "Hammer", "PowerGlove" }],
        ["Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MoonPearl", "Hammer", "TitansMitt" }],
        ["Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MoonPearl", "Hammer", "ProgressiveGlove" }],
        ["Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MoonPearl", "Flippers", "TitansMitt" }],
        ["Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" }],

        ["Palace of Darkness - Dark Maze - Top", false, new string[] {  }],
        ["Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "AgahnimDefeated" }],
        ["Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Hammer", "PowerGlove" }],
        ["Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Hammer", "TitansMitt" }],
        ["Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Hammer", "ProgressiveGlove" }],
        ["Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Flippers", "TitansMitt" }],
        ["Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Flippers", "ProgressiveGlove", "ProgressiveGlove" }],

        ["Palace of Darkness - Dark Maze - Bottom", false, new string[] {  }],
        ["Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "AgahnimDefeated" }],
        ["Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Hammer", "PowerGlove" }],
        ["Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Hammer", "TitansMitt" }],
        ["Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Hammer", "ProgressiveGlove" }],
        ["Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Flippers", "TitansMitt" }],
        ["Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Flippers", "ProgressiveGlove", "ProgressiveGlove" }],

        ["Palace of Darkness - Shooter Room Chest", false, new string[] {  }],
        ["Palace of Darkness - Shooter Room Chest", false, new string[] { "AgahnimDefeated" }],
        ["Palace of Darkness - Shooter Room Chest", true, new string[] { "MoonPearl", "AgahnimDefeated" }],
        ["Palace of Darkness - Shooter Room Chest", true, new string[] { "MoonPearl", "Hammer", "PowerGlove" }],
        ["Palace of Darkness - Shooter Room Chest", true, new string[] { "MoonPearl", "Hammer", "TitansMitt" }],
        ["Palace of Darkness - Shooter Room Chest", true, new string[] { "MoonPearl", "Hammer", "ProgressiveGlove" }],
        ["Palace of Darkness - Shooter Room Chest", true, new string[] { "MoonPearl", "Flippers", "TitansMitt" }],
        ["Palace of Darkness - Shooter Room Chest", true, new string[] { "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" }],

        ["Palace of Darkness - Boss", false, new string[] {  }],
        ["Palace of Darkness - Boss", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Hammer", "BowAndArrows", "AgahnimDefeated" }],
        ["Palace of Darkness - Boss", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Hammer", "BowAndArrows", "PowerGlove" }],
        ["Palace of Darkness - Boss", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Hammer", "BowAndArrows", "TitansMitt" }],
        ["Palace of Darkness - Boss", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Hammer", "BowAndArrows", "ProgressiveGlove" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
