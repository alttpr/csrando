namespace RandomizerTests.Logic.Standard.MajorGlitches;

[TestClass]
public sealed class SkullWoodsTest : StandardMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Skull Woods - Big Chest", false, new string[] {  }],
        ["Skull Woods - Big Chest", true, new string[] { "BigKeyD3" }],

        ["Skull Woods - Big Key Chest", true, new string[] {  }],

        ["Skull Woods - Compass Chest", true, new string[] {  }],

        ["Skull Woods - Map Chest", true, new string[] {  }],

        ["Skull Woods - Bridge Room", false, new string[] {  }],
        ["Skull Woods - Bridge Room", true, new string[] { "FireRod", "MoonPearl" }],

        ["Skull Woods - Pot Prison", true, new string[] {  }],

        ["Skull Woods - Pinball Room", true, new string[] {  }],

        ["Skull Woods - Boss", false, new string[] {  }],
        ["Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "FireRod", "MoonPearl", "UncleSword" }],
        ["Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "FireRod", "MoonPearl", "MasterSword" }],
        ["Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "FireRod", "MoonPearl", "L3Sword" }],
        ["Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "FireRod", "MoonPearl", "L4Sword" }],
        ["Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "FireRod", "MoonPearl", "ProgressiveSword" }],
    ];
}
