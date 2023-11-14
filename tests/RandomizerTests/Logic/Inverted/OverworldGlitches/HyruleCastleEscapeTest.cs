namespace RandomizerTests.Logic.Inverted.OverworldGlitches;

[TestClass]
public sealed class HyruleCastleEscapeTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Sanctuary", false, new string[] {  }],
        ["Sanctuary", true, new string[] { "MagicMirror", "DefeatAgahnim" }],
        ["Sanctuary", true, new string[] { "Lamp", "DefeatAgahnim", "KeyH2" }],
        ["Sanctuary", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Sanctuary", true, new string[] { "MagicMirror", "PegasusBoots" }],

        ["Sewers - Secret Room - Left", false, new string[] {  }],
        ["Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "ProgressiveGlove", "PegasusBoots" }],
        ["Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "PegasusBoots", "Lamp", "KeyH2" }],
        ["Sewers - Secret Room - Left", true, new string[] { "MagicMirror", "PegasusBoots", "Lamp", "KeyH2" }],
        ["Sewers - Secret Room - Left", true, new string[] { "DefeatAgahnim", "Lamp", "KeyH2" }],

        ["Sewers - Secret Room - Middle", false, new string[] {  }],
        ["Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "ProgressiveGlove", "PegasusBoots" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "PegasusBoots", "Lamp", "KeyH2" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "MagicMirror", "PegasusBoots", "Lamp", "KeyH2" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "DefeatAgahnim", "Lamp", "KeyH2" }],

        ["Sewers - Secret Room - Right", false, new string[] {  }],
        ["Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "PegasusBoots" }],
        ["Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "PegasusBoots", "Lamp", "KeyH2" }],
        ["Sewers - Secret Room - Right", true, new string[] { "MagicMirror", "PegasusBoots", "Lamp", "KeyH2" }],
        ["Sewers - Secret Room - Right", true, new string[] { "DefeatAgahnim", "Lamp", "KeyH2" }],

        ["Sewers - Dark Cross", false, new string[] {  }],
        ["Sewers - Dark Cross", true, new string[] { "Lamp", "DefeatAgahnim" }],
        ["Sewers - Dark Cross", true, new string[] { "Lamp", "MoonPearl", "PegasusBoots" }],
        ["Sewers - Dark Cross", true, new string[] { "Lamp", "MagicMirror", "PegasusBoots" }],

        ["Hyrule Castle - Boomerang Chest", false, new string[] {  }],
        ["Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2", "DefeatAgahnim" }],
        ["Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2", "MoonPearl", "PegasusBoots" }],
        ["Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2", "MagicMirror", "PegasusBoots" }],

        ["Hyrule Castle - Map Chest", false, new string[] {  }],
        ["Hyrule Castle - Map Chest", true, new string[] { "DefeatAgahnim" }],
        ["Hyrule Castle - Map Chest", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Hyrule Castle - Map Chest", true, new string[] { "MagicMirror", "PegasusBoots" }],

        ["Hyrule Castle - Zelda's Cell", false, new string[] {  }],
        ["Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2", "DefeatAgahnim" }],
        ["Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2", "MoonPearl", "PegasusBoots" }],
        ["Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2", "MagicMirror", "PegasusBoots" }],
    ];
}
