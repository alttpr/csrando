namespace RandomizerTests.Logic.Standard.MajorGlitches.DarkWorld;

[TestClass]
public sealed class MireTest : StandardMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Mire Shed - Left", false, new string[] {  }],
        ["Mire Shed - Left", true, new string[] { "MoonPearl" }],
        ["Mire Shed - Left", true, new string[] { "BottleWithBee" }],
        ["Mire Shed - Left", true, new string[] { "BottleWithFairy" }],
        ["Mire Shed - Left", true, new string[] { "BottleWithRedPotion" }],
        ["Mire Shed - Left", true, new string[] { "BottleWithGreenPotion" }],
        ["Mire Shed - Left", true, new string[] { "BottleWithBluePotion" }],
        ["Mire Shed - Left", true, new string[] { "Bottle" }],
        ["Mire Shed - Left", true, new string[] { "BottleWithGoldBee" }],
        ["Mire Shed - Left", true, new string[] { "MagicMirror" }],

        ["Mire Shed - Right", false, new string[] {  }],
        ["Mire Shed - Right", true, new string[] { "MoonPearl" }],
        ["Mire Shed - Right", true, new string[] { "BottleWithBee" }],
        ["Mire Shed - Right", true, new string[] { "BottleWithFairy" }],
        ["Mire Shed - Right", true, new string[] { "BottleWithRedPotion" }],
        ["Mire Shed - Right", true, new string[] { "BottleWithGreenPotion" }],
        ["Mire Shed - Right", true, new string[] { "BottleWithBluePotion" }],
        ["Mire Shed - Right", true, new string[] { "Bottle" }],
        ["Mire Shed - Right", true, new string[] { "BottleWithGoldBee" }],
        ["Mire Shed - Right", true, new string[] { "MagicMirror" }],
    ];
}
