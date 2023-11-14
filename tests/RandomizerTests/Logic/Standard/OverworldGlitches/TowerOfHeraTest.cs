namespace RandomizerTests.Logic.Standard.OverworldGlitches;

[TestClass]
public sealed class TowerOfHeraTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Tower Of Hera - Big Key Chest", false, new string[] {  }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "PegasusBoots", "KeyP3" }],

        ["Tower Of Hera - Basement Cage", false, new string[] {  }],
        ["Tower Of Hera - Basement Cage", true, new string[] { "PegasusBoots" }],

        ["Tower Of Hera - Map Chest", false, new string[] {  }],
        ["Tower Of Hera - Map Chest", true, new string[] { "PegasusBoots" }],

        ["Tower Of Hera - Compass Chest", false, new string[] {  }],
        ["Tower Of Hera - Compass Chest", true, new string[] { "PegasusBoots", "BigKeyP3" }],

        ["Tower Of Hera - Big Chest", false, new string[] {  }],
        ["Tower Of Hera - Big Chest", true, new string[] { "PegasusBoots", "BigKeyP3" }],

        ["Tower Of Hera - Boss", false, new string[] {  }],
        ["Tower Of Hera - Boss", true, new string[] { "PegasusBoots", "BigKeyP3", "ProgressiveSword" }],
        ["Tower Of Hera - Boss", true, new string[] { "PegasusBoots", "BigKeyP3", "UncleSword" }],
        ["Tower Of Hera - Boss", true, new string[] { "PegasusBoots", "BigKeyP3", "MasterSword" }],
        ["Tower Of Hera - Boss", true, new string[] { "PegasusBoots", "BigKeyP3", "L3Sword" }],
        ["Tower Of Hera - Boss", true, new string[] { "PegasusBoots", "BigKeyP3", "L4Sword" }],
        ["Tower Of Hera - Boss", true, new string[] { "PegasusBoots", "BigKeyP3", "Hammer" }],
    ];
}
