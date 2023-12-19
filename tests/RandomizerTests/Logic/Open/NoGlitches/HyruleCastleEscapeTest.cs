namespace RandomizerTests.Logic.Open.NoGlitches;

[TestClass]
public class HyruleCastleEscapeTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Sanctuary Chest", true, new string[] {  }],

        ["Sewers - Secret Room - Left", false, new string[] {  }],
        ["Sewers - Secret Room - Left", true, new string[] { "ProgressiveGlove" }],
        ["Sewers - Secret Room - Left", true, new string[] { "PowerGlove" }],
        ["Sewers - Secret Room - Left", true, new string[] { "TitansMitt" }],
        ["Sewers - Secret Room - Left", true, new string[] { "Lamp", "KeyH2" }],

        ["Sewers - Secret Room - Middle", false, new string[] {  }],
        ["Sewers - Secret Room - Middle", true, new string[] { "ProgressiveGlove" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "PowerGlove" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "TitansMitt" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "Lamp", "KeyH2" }],

        ["Sewers - Secret Room - Right", false, new string[] {  }],
        ["Sewers - Secret Room - Right", true, new string[] { "ProgressiveGlove" }],
        ["Sewers - Secret Room - Right", true, new string[] { "PowerGlove" }],
        ["Sewers - Secret Room - Right", true, new string[] { "TitansMitt" }],
        ["Sewers - Secret Room - Right", true, new string[] { "Lamp", "KeyH2" }],

        ["Sewers - Dark Cross Chest", false, new string[] {  }],
        ["Sewers - Dark Cross Chest", true, new string[] { "Lamp" }],

        ["Hyrule Castle - Boomerang Chest", false, new string[] { "Lamp" }],
        ["Hyrule Castle - Boomerang Chest", false, new string[] { "PowerGlove" }],
        ["Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2" }],

        ["Hyrule Castle - Map Chest", true, new string[] {  }],

        ["Hyrule Castle - Zelda's Cell", false, new string[] { "Lamp" }],
        ["Hyrule Castle - Zelda's Cell", false, new string[] { "PowerGlove" }],
        ["Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2" }],

        ["Link's Uncle", true, new string[] {  }],

        ["Secret Passage", true, new string[] {  }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
