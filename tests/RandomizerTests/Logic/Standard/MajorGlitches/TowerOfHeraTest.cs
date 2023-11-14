namespace RandomizerTests.Logic.Standard.MajorGlitches;

[TestClass]
public sealed class TowerOfHeraTest : StandardMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Tower Of Hera - Big Key Chest", false, new string[] {  }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "FireRod", "KeyP3" }],

        ["Tower Of Hera - Basement Cage", true, new string[] {  }],

        ["Tower Of Hera - Map Chest", true, new string[] {  }],

        ["Tower Of Hera - Compass Chest", false, new string[] {  }],
        ["Tower Of Hera - Compass Chest", true, new string[] { "BigKeyP3" }],

        ["Tower Of Hera - Big Chest", false, new string[] {  }],
        ["Tower Of Hera - Big Chest", true, new string[] { "BigKeyP3" }],

        ["Tower Of Hera - Boss", false, new string[] {  }],
        ["Tower Of Hera - Boss", true, new string[] { "BigKeyP3", "ProgressiveSword" }],
        ["Tower Of Hera - Boss", true, new string[] { "BigKeyP3", "UncleSword" }],
        ["Tower Of Hera - Boss", true, new string[] { "BigKeyP3", "MasterSword" }],
        ["Tower Of Hera - Boss", true, new string[] { "BigKeyP3", "L3Sword" }],
        ["Tower Of Hera - Boss", true, new string[] { "BigKeyP3", "L4Sword" }],
        ["Tower Of Hera - Boss", true, new string[] { "BigKeyP3", "Hammer" }],
    ];
}
