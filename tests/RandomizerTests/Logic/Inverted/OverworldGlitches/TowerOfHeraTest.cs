namespace RandomizerTests.Logic.Inverted.OverworldGlitches;

[TestClass]
public sealed class TowerOfHeraTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Tower Of Hera - Big Key Chest", false, new string[] {  }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "PegasusBoots", "MoonPearl", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "FireRod", "PegasusBoots", "MoonPearl", "KeyP3" }],

        ["Tower Of Hera - Basement Cage", false, new string[] {  }],
        ["Tower Of Hera - Basement Cage", true, new string[] { "PegasusBoots", "MoonPearl" }],

        ["Tower Of Hera - Map Chest", false, new string[] {  }],
        ["Tower Of Hera - Map Chest", true, new string[] { "PegasusBoots", "MoonPearl" }],

        ["Tower Of Hera - Compass Chest", false, new string[] {  }],
        ["Tower Of Hera - Compass Chest", true, new string[] { "PegasusBoots", "MoonPearl", "BigKeyP3" }],

        ["Tower Of Hera - Big Chest", false, new string[] {  }],
        ["Tower Of Hera - Big Chest", true, new string[] { "PegasusBoots", "MoonPearl", "BigKeyP3" }],

        ["Tower Of Hera - Boss", false, new string[] {  }],
        ["Tower Of Hera - Boss", true, new string[] { "PegasusBoots", "MoonPearl", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Boss", true, new string[] { "PegasusBoots", "MoonPearl", "UncleSword", "BigKeyP3" }],
        ["Tower Of Hera - Boss", true, new string[] { "PegasusBoots", "MoonPearl", "ProgressiveSword", "BigKeyP3" }],
        ["Tower Of Hera - Boss", true, new string[] { "PegasusBoots", "MoonPearl", "MasterSword", "BigKeyP3" }],
        ["Tower Of Hera - Boss", true, new string[] { "PegasusBoots", "MoonPearl", "L3Sword", "BigKeyP3" }],
        ["Tower Of Hera - Boss", true, new string[] { "PegasusBoots", "MoonPearl", "L4Sword", "BigKeyP3" }],
    ];
}
