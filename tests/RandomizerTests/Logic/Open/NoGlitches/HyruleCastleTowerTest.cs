namespace RandomizerTests.Logic.Open.NoGlitches;

[TestClass]
public class HyruleCastleTowerTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Agahnims Tower - First Chest", false, new string[] {  }],
        ["Agahnims Tower - First Chest", true, new string[] { "L2Sword" }],
        ["Agahnims Tower - First Chest", true, new string[] { "Cape" }],

        ["Agahnims Tower - Second Chest", false, new string[] {  }],
        ["Agahnims Tower - Second Chest", false, new string[] { "L2Sword" }],
        ["Agahnims Tower - Second Chest", true, new string[] { "KeyA1", "L2Sword", "Lamp" }],
        ["Agahnims Tower - Second Chest", true, new string[] { "KeyA1", "Cape", "Lamp" }],

        ["Agahnims Tower - Boss Room - Agahnim", false, new string[] {  }],
        ["Agahnims Tower - Boss Room - Agahnim", false, new string[] { "KeyA1", "KeyA1", "Cape", "Lamp" }],
        ["Agahnims Tower - Boss Room - Agahnim", true, new string[] { "KeyA1", "KeyA1", "L2Sword", "Lamp" }],
        ["Agahnims Tower - Boss Room - Agahnim", true, new string[] { "KeyA1", "KeyA1", "L1Sword", "Cape", "Lamp" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
