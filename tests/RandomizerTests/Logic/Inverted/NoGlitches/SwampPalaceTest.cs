using Randomizer.Graph;

namespace RandomizerTests.Logic.Inverted.NoGlitches;

[TestClass]
public class SwampPalaceTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Swamp Palace - Entrance Chest", false, new string[] {  }],
        ["Swamp Palace - Entrance Chest", false, new string[] { "MoonPearl", "Flippers", "TitansMitt" }],
        ["Swamp Palace - Entrance Chest", false, new string[] { "MagicMirror", "Flippers", "TitansMitt" }],
        ["Swamp Palace - Entrance Chest", false, new string[] { "MagicMirror", "MoonPearl", "TitansMitt" }],
        ["Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "TitansMitt" }],
        ["Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" }],
        ["Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" }],
        ["Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated" }],

        ["Swamp Palace - Big Chest", false, new string[] {  }],
        ["Swamp Palace - Big Chest", false, new string[] { "BigKeyD2", "KeyD2", "MoonPearl", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - Big Chest", false, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - Big Chest", false, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer" }],
        ["Swamp Palace - Big Chest", false, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt" }],
        ["Swamp Palace - Big Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - Big Chest", true, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - Big Chest", true, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" }],
        ["Swamp Palace - Big Chest", true, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" }],
        ["Swamp Palace - Big Chest", true, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" }],

        ["Swamp Palace - Big Key Chest", false, new string[] {  }],
        ["Swamp Palace - Big Key Chest", false, new string[] { "KeyD2", "MoonPearl", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - Big Key Chest", false, new string[] { "KeyD2", "MagicMirror", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - Big Key Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer" }],
        ["Swamp Palace - Big Key Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt" }],
        ["Swamp Palace - Big Key Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - Big Key Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" }],
        ["Swamp Palace - Big Key Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" }],
        ["Swamp Palace - Big Key Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" }],

        ["Swamp Palace - Map Chest", false, new string[] {  }],
        ["Swamp Palace - Map Chest", false, new string[] { "KeyD2", "MoonPearl", "Flippers", "TitansMitt" }],
        ["Swamp Palace - Map Chest", false, new string[] { "KeyD2", "MagicMirror", "Flippers", "TitansMitt" }],
        ["Swamp Palace - Map Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "TitansMitt" }],
        ["Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt" }],
        ["Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" }],
        ["Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" }],
        ["Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" }],
        ["Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hookshot" }],

        ["Swamp Palace - West Chest", false, new string[] {  }],
        ["Swamp Palace - West Chest", false, new string[] { "KeyD2", "MoonPearl", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - West Chest", false, new string[] { "KeyD2", "MagicMirror", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - West Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer" }],
        ["Swamp Palace - West Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt" }],
        ["Swamp Palace - West Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - West Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" }],
        ["Swamp Palace - West Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" }],
        ["Swamp Palace - West Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" }],

        ["Swamp Palace - Compass Chest", false, new string[] {  }],
        ["Swamp Palace - Compass Chest", false, new string[] { "KeyD2", "MoonPearl", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - Compass Chest", false, new string[] { "KeyD2", "MagicMirror", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - Compass Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer" }],
        ["Swamp Palace - Compass Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt" }],
        ["Swamp Palace - Compass Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - Compass Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" }],
        ["Swamp Palace - Compass Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" }],
        ["Swamp Palace - Compass Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" }],

        ["Swamp Palace - Flooded Room - Left", false, new string[] {  }],
        ["Swamp Palace - Flooded Room - Left", false, new string[] { "KeyD2", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Left", false, new string[] { "KeyD2", "MagicMirror", "Flippers", "TitansMitt", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Left", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Left", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Left", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - Flooded Room - Left", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Left", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Left", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Left", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer", "Hookshot" }],

        ["Swamp Palace - Flooded Room - Right", false, new string[] {  }],
        ["Swamp Palace - Flooded Room - Right", false, new string[] { "KeyD2", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Right", false, new string[] { "KeyD2", "MagicMirror", "Flippers", "TitansMitt", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Right", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Right", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Right", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - Flooded Room - Right", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Right", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Right", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Right", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer", "Hookshot" }],

        ["Swamp Palace - Waterfall Room Chest", false, new string[] {  }],
        ["Swamp Palace - Waterfall Room Chest", false, new string[] { "KeyD2", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" }],
        ["Swamp Palace - Waterfall Room Chest", false, new string[] { "KeyD2", "MagicMirror", "Flippers", "TitansMitt", "Hammer", "Hookshot" }],
        ["Swamp Palace - Waterfall Room Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Hookshot" }],
        ["Swamp Palace - Waterfall Room Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hookshot" }],
        ["Swamp Palace - Waterfall Room Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" }],
        ["Swamp Palace - Waterfall Room Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" }],
        ["Swamp Palace - Waterfall Room Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer", "Hookshot" }],
        ["Swamp Palace - Waterfall Room Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer", "Hookshot" }],
        ["Swamp Palace - Waterfall Room Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer", "Hookshot" }],

        ["Swamp Palace - Boss", false, new string[] {  }],
        ["Swamp Palace - Boss", false, new string[] { "KeyD2", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" }],
        ["Swamp Palace - Boss", false, new string[] { "KeyD2", "MagicMirror", "Flippers", "TitansMitt", "Hammer", "Hookshot" }],
        ["Swamp Palace - Boss", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Hookshot" }],
        ["Swamp Palace - Boss", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hookshot" }],
        ["Swamp Palace - Boss", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" }],
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
