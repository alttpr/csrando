namespace RandomizerTests.Logic.Alttp.Standard.OverworldGlitches;

using RandomizerTests.Logic.Alttp;

[TestClass]
public sealed class EasternPalaceTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Eastern Palace - Compass Chest", true, new string[] {  }],

        ["Eastern Palace - Cannonball Chest", true, new string[] {  }],

        ["Eastern Palace - Big Chest", false, new string[] {  }],
        ["Eastern Palace - Big Chest", true, new string[] { "BigKeyP1" }],

        ["Eastern Palace - Map Chest", true, new string[] {  }],

        ["Eastern Palace - Big Key Chest", false, new string[] {  }],
        ["Eastern Palace - Big Key Chest", true, new string[] { "Lamp" }],

        ["Eastern Palace - Boss", false, new string[] {  }],
        ["Eastern Palace - Boss", true, new string[] { "Lamp", "BowAndArrows", "BigKeyP1" }],
    ];
}
