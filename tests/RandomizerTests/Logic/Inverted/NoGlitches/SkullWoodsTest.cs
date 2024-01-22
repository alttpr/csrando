using Randomizer.Graph;

namespace RandomizerTests.Logic.Inverted.NoGlitches;

[TestClass]
public class SkullWoodsTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Skull Woods - Big Chest", false, new string[] {  }],
        ["Skull Woods - Big Chest", true, new string[] { "BigKeyD3" }],

        ["Skull Woods - Big Key Chest", true, new string[] {  }],

        ["Skull Woods - Compass Chest", true, new string[] {  }],

        ["Skull Woods - Map Chest", true, new string[] {  }],

        ["Skull Woods - Bridge Room Chest", false, new string[] {  }],
        ["Skull Woods - Bridge Room Chest", true, new string[] { "FireRod" }],

        ["Skull Woods - Pot Prison", true, new string[] {  }],

        ["Skull Woods - Pinball Chest", true, new string[] {  }],

        ["Skull Woods - Boss", false, new string[] {  }],
        ["Skull Woods - Boss", false, new string[] { "KeyD3", "KeyD3", "KeyD3", "UncleSword" }],
        ["Skull Woods - Boss", false, new string[] { "KeyD3", "KeyD3", "KeyD3", "FireRod" }],
        ["Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "FireRod", "UncleSword" }],
        ["Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "FireRod", "MasterSword" }],
        ["Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "FireRod", "L3Sword" }],
        ["Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "FireRod", "L4Sword" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
