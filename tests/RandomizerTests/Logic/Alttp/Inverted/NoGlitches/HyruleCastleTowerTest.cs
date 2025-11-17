namespace RandomizerTests.Logic.Alttp.Inverted.NoGlitches;

using RandomizerTests.Logic.Alttp;

[TestClass]
public class HyruleCastleTowerTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Agahnims Tower - First Chest", false, new string[] {  }],
        ["Agahnims Tower - First Chest", true, new string[] { "Lamp", "ProgressiveGlove" }],
        ["Agahnims Tower - First Chest", true, new string[] { "OcarinaInactive", "TitansMitt", "MoonPearl" }],

        ["Agahnims Tower - Second Chest", false, new string[] {  }],
        ["Agahnims Tower - Second Chest", false, new string[] { "KeyA1", "OcarinaInactive", "TitansMitt", "MoonPearl" }],
        ["Agahnims Tower - Second Chest", true, new string[] { "KeyA1", "Lamp", "ProgressiveGlove" }],
        ["Agahnims Tower - Second Chest", true, new string[] { "KeyA1", "Lamp", "OcarinaInactive", "TitansMitt", "MoonPearl" }],

        ["Agahnims Tower - Boss Room - Agahnim", false, new string[] {  }],
        ["Agahnims Tower - Boss Room - Agahnim", false, new string[] { "KeyA1", "KeyA1", "ProgressiveGlove", "Lamp" }],
        ["Agahnims Tower - Boss Room - Agahnim", true, new string[] { "KeyA1", "KeyA1", "L1Sword", "ProgressiveGlove", "Lamp" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
