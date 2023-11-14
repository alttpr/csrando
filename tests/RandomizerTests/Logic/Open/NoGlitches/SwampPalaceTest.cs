namespace RandomizerTests.Logic.Open.NoGlitches;

[TestClass]
public class SwampPalaceTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Swamp Palace - Entrance Chest", false, new string[] {  }],
        ["Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "TitansMitt" }],
        ["Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" }],
        ["Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" }],
        ["Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" }],
        ["Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hookshot" }],

        ["Swamp Palace - Big Chest", false, new string[] {  }],
        ["Swamp Palace - Big Chest", true, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - Big Chest", true, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" }],
        ["Swamp Palace - Big Chest", true, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" }],
        ["Swamp Palace - Big Chest", true, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" }],

        ["Swamp Palace - Big Key Chest", false, new string[] {  }],
        ["Swamp Palace - Big Key Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - Big Key Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" }],
        ["Swamp Palace - Big Key Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" }],
        ["Swamp Palace - Big Key Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" }],

        ["Swamp Palace - Map Chest", false, new string[] {  }],
        ["Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt" }],
        ["Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" }],
        ["Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" }],
        ["Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" }],
        ["Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hookshot" }],

        ["Swamp Palace - West Chest", false, new string[] {  }],
        ["Swamp Palace - West Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - West Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" }],
        ["Swamp Palace - West Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" }],
        ["Swamp Palace - West Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" }],

        ["Swamp Palace - Compass Chest", false, new string[] {  }],
        ["Swamp Palace - Compass Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - Compass Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" }],
        ["Swamp Palace - Compass Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" }],
        ["Swamp Palace - Compass Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" }],

        ["Swamp Palace - Flooded Room - Left", false, new string[] {  }],
        ["Swamp Palace - Flooded Room - Left", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Left", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Left", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Left", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer", "Hookshot" }],

        ["Swamp Palace - Flooded Room - Right", false, new string[] {  }],
        ["Swamp Palace - Flooded Room - Right", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Right", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Right", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Right", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer", "Hookshot" }],

        ["Swamp Palace - Waterfall Room Chest", false, new string[] {  }],
        ["Swamp Palace - Waterfall Room Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" }],
        ["Swamp Palace - Waterfall Room Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer", "Hookshot" }],
        ["Swamp Palace - Waterfall Room Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer", "Hookshot" }],
        ["Swamp Palace - Waterfall Room Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer", "Hookshot" }],

        ["Swamp Palace - Boss", false, new string[] {  }],
        ["Swamp Palace - Boss", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" }],
        ["Swamp Palace - Boss", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer", "Hookshot" }],
        ["Swamp Palace - Boss", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer", "Hookshot" }],
        ["Swamp Palace - Boss", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer", "Hookshot" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
