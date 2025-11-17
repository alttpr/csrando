namespace RandomizerTests.Logic.Alttp.Inverted.NoGlitches;

using RandomizerTests.Logic.Alttp;

[TestClass]
public class HyruleCastleEscapeTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Sanctuary Chest", false, new string[] {  }],
        ["Sanctuary Chest", false, new string[] { "AgahnimDefeated" }],
        ["Sanctuary Chest", true, new string[] { "MoonPearl", "AgahnimDefeated" }],
        ["Sanctuary Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Sanctuary Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Sanctuary Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Sanctuary Chest", true, new string[] { "MoonPearl", "TitansMitt" }],

        ["Sewers - Secret Room - Left", false, new string[] {  }],
        ["Sewers - Secret Room - Left", false, new string[] { "ProgressiveGlove", "AgahnimDefeated" }],
        ["Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "ProgressiveGlove", "AgahnimDefeated" }],
        ["Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "PowerGlove", "AgahnimDefeated" }],
        ["Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "TitansMitt" }],
        ["Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "AgahnimDefeated", "Lamp", "KeyH2" }],

        ["Sewers - Secret Room - Middle", false, new string[] {  }],
        ["Sewers - Secret Room - Middle", false, new string[] { "ProgressiveGlove", "AgahnimDefeated" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "ProgressiveGlove", "AgahnimDefeated" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "PowerGlove", "AgahnimDefeated" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "TitansMitt" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "AgahnimDefeated", "Lamp", "KeyH2" }],

        ["Sewers - Secret Room - Right", false, new string[] {  }],
        ["Sewers - Secret Room - Right", false, new string[] { "ProgressiveGlove", "AgahnimDefeated" }],
        ["Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "AgahnimDefeated" }],
        ["Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "PowerGlove", "AgahnimDefeated" }],
        ["Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "TitansMitt" }],
        ["Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "AgahnimDefeated", "Lamp", "KeyH2" }],

        ["Sewers - Dark Cross Chest", false, new string[] {  }],
        ["Sewers - Dark Cross Chest", false, new string[] { "MoonPearl", "AgahnimDefeated" }],
        ["Sewers - Dark Cross Chest", false, new string[] { "Lamp", "AgahnimDefeated" }],
        ["Sewers - Dark Cross Chest", true, new string[] { "Lamp", "MoonPearl", "AgahnimDefeated" }],
        ["Sewers - Dark Cross Chest", true, new string[] { "Lamp", "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Sewers - Dark Cross Chest", true, new string[] { "Lamp", "MoonPearl", "PowerGlove", "Hammer" }],
        ["Sewers - Dark Cross Chest", true, new string[] { "Lamp", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Sewers - Dark Cross Chest", true, new string[] { "Lamp", "MoonPearl", "TitansMitt" }],

        ["Hyrule Castle - Boomerang Chest", false, new string[] {  }],
        ["Hyrule Castle - Boomerang Chest", false, new string[] { "MoonPearl", "Lamp", "AgahnimDefeated" }],
        ["Hyrule Castle - Boomerang Chest", false, new string[] { "KeyH2", "AgahnimDefeated" }],
        ["Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2", "MoonPearl", "AgahnimDefeated" }],
        ["Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2", "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2", "MoonPearl", "PowerGlove", "Hammer" }],
        ["Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2", "MoonPearl", "TitansMitt" }],

        ["Hyrule Castle - Map Chest", false, new string[] {  }],
        ["Hyrule Castle - Map Chest", false, new string[] { "AgahnimDefeated" }],
        ["Hyrule Castle - Map Chest", true, new string[] { "MoonPearl", "AgahnimDefeated" }],
        ["Hyrule Castle - Map Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Hyrule Castle - Map Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Hyrule Castle - Map Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Hyrule Castle - Map Chest", true, new string[] { "MoonPearl", "TitansMitt" }],

        ["Hyrule Castle - Zelda's Cell", false, new string[] {  }],
        ["Hyrule Castle - Zelda's Cell", false, new string[] { "MoonPearl", "Lamp", "AgahnimDefeated" }],
        ["Hyrule Castle - Zelda's Cell", false, new string[] { "KeyH2", "AgahnimDefeated" }],
        ["Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2", "MoonPearl", "AgahnimDefeated" }],
        ["Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2", "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2", "MoonPearl", "PowerGlove", "Hammer" }],
        ["Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2", "MoonPearl", "TitansMitt" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
