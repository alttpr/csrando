namespace RandomizerTests.Logic.Inverted.OverworldGlitches;

[TestClass]
public sealed class SwampPalaceTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Swamp Palace - Entrance Chest", false, new string[] {  }],
        ["Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "Flippers", "PegasusBoots" }],
        ["Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "Flippers", "DefeatAgahnim" }],

        ["Swamp Palace - Big Chest", false, new string[] {  }],
        ["Swamp Palace - Big Chest", true, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "Flippers", "PegasusBoots", "Hammer" }],
        ["Swamp Palace - Big Chest", true, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "Flippers", "DefeatAgahnim", "Hammer" }],

        ["Swamp Palace - Big Key Chest", false, new string[] {  }],
        ["Swamp Palace - Big Key Chest", true, new string[] { "KeyD2", "MagicMirror", "Flippers", "PegasusBoots", "Hammer" }],
        ["Swamp Palace - Big Key Chest", true, new string[] { "KeyD2", "MagicMirror", "Flippers", "DefeatAgahnim", "Hammer" }],

        ["Swamp Palace - Map Chest", false, new string[] {  }],
        ["Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "Flippers", "PegasusBoots" }],
        ["Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "Flippers", "DefeatAgahnim", "ProgressiveGlove" }],
        ["Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "Flippers", "DefeatAgahnim", "Hammer" }],

        ["Swamp Palace - West Chest", false, new string[] {  }],
        ["Swamp Palace - West Chest", true, new string[] { "KeyD2", "MagicMirror", "Flippers", "PegasusBoots", "Hammer" }],
        ["Swamp Palace - West Chest", true, new string[] { "KeyD2", "MagicMirror", "Flippers", "DefeatAgahnim", "Hammer" }],

        ["Swamp Palace - Compass Chest", false, new string[] {  }],
        ["Swamp Palace - Compass Chest", true, new string[] { "KeyD2", "MagicMirror", "Flippers", "PegasusBoots", "Hammer" }],
        ["Swamp Palace - Compass Chest", true, new string[] { "KeyD2", "MagicMirror", "Flippers", "DefeatAgahnim", "Hammer" }],

        ["Swamp Palace - Flooded Room - Left", false, new string[] {  }],
        ["Swamp Palace - Flooded Room - Left", true, new string[] { "KeyD2", "MagicMirror", "Flippers", "PegasusBoots", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Left", true, new string[] { "KeyD2", "MagicMirror", "Flippers", "DefeatAgahnim", "Hammer", "Hookshot" }],

        ["Swamp Palace - Flooded Room - Right", false, new string[] {  }],
        ["Swamp Palace - Flooded Room - Right", true, new string[] { "KeyD2", "MagicMirror", "Flippers", "PegasusBoots", "Hammer", "Hookshot" }],
        ["Swamp Palace - Flooded Room - Right", true, new string[] { "KeyD2", "MagicMirror", "Flippers", "DefeatAgahnim", "Hammer", "Hookshot" }],

        ["Swamp Palace - Waterfall Room", false, new string[] {  }],
        ["Swamp Palace - Waterfall Room", true, new string[] { "KeyD2", "MagicMirror", "Flippers", "PegasusBoots", "Hammer", "Hookshot" }],
        ["Swamp Palace - Waterfall Room", true, new string[] { "KeyD2", "MagicMirror", "Flippers", "DefeatAgahnim", "Hammer", "Hookshot" }],

        ["Swamp Palace - Boss", false, new string[] {  }],
        ["Swamp Palace - Boss", true, new string[] { "KeyD2", "MagicMirror", "Flippers", "PegasusBoots", "Hammer", "Hookshot" }],
        ["Swamp Palace - Boss", true, new string[] { "KeyD2", "MagicMirror", "Flippers", "DefeatAgahnim", "Hammer", "Hookshot" }],
    ];
}
