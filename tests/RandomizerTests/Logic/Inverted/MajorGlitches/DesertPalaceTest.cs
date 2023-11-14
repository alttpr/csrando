namespace RandomizerTests.Logic.Inverted.MajorGlitches;

[TestClass]
public sealed class DesertPalaceTest : InvertedMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Desert Palace - Map Chest", true, new string[] {  }],

        ["Desert Palace - Big Chest", false, new string[] {  }],
        ["Desert Palace - Big Chest", true, new string[] { "BigKeyP2" }],

        ["Desert Palace - Torch", false, new string[] {  }],
        ["Desert Palace - Torch", true, new string[] { "PegasusBoots" }],

        ["Desert Palace - Compass Chest", false, new string[] {  }],
        ["Desert Palace - Compass Chest", true, new string[] { "KeyP2" }],

        ["Desert Palace - Big Key Chest", false, new string[] {  }],
        ["Desert Palace - Big Key Chest", true, new string[] { "KeyP2" }],

        ["Desert Palace - Boss", false, new string[] {  }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "BigKeyP2", "Lamp" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "BigKeyP2", "FireRod" }],
    ];
}
