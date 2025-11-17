namespace RandomizerTests.Logic.Alttp.Standard.MajorGlitches.DarkWorld;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Standard.MajorGlitches;

[TestClass]
public sealed class SouthTest : StandardMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Hype Cave - Top", false, new string[] {  }],
        ["Hype Cave - Top", true, new string[] { "MoonPearl" }],
        ["Hype Cave - Top", true, new string[] { "BottleWithBee" }],
        ["Hype Cave - Top", true, new string[] { "BottleWithFairy" }],
        ["Hype Cave - Top", true, new string[] { "BottleWithRedPotion" }],
        ["Hype Cave - Top", true, new string[] { "BottleWithGreenPotion" }],
        ["Hype Cave - Top", true, new string[] { "BottleWithBluePotion" }],
        ["Hype Cave - Top", true, new string[] { "Bottle" }],
        ["Hype Cave - Top", true, new string[] { "BottleWithGoldBee" }],

        ["Hype Cave - Middle Right", false, new string[] {  }],
        ["Hype Cave - Middle Right", true, new string[] { "MoonPearl" }],
        ["Hype Cave - Middle Right", true, new string[] { "BottleWithBee" }],
        ["Hype Cave - Middle Right", true, new string[] { "BottleWithFairy" }],
        ["Hype Cave - Middle Right", true, new string[] { "BottleWithRedPotion" }],
        ["Hype Cave - Middle Right", true, new string[] { "BottleWithGreenPotion" }],
        ["Hype Cave - Middle Right", true, new string[] { "BottleWithBluePotion" }],
        ["Hype Cave - Middle Right", true, new string[] { "Bottle" }],
        ["Hype Cave - Middle Right", true, new string[] { "BottleWithGoldBee" }],

        ["Hype Cave - Middle Left", false, new string[] {  }],
        ["Hype Cave - Middle Left", true, new string[] { "MoonPearl" }],
        ["Hype Cave - Middle Left", true, new string[] { "BottleWithBee" }],
        ["Hype Cave - Middle Left", true, new string[] { "BottleWithFairy" }],
        ["Hype Cave - Middle Left", true, new string[] { "BottleWithRedPotion" }],
        ["Hype Cave - Middle Left", true, new string[] { "BottleWithGreenPotion" }],
        ["Hype Cave - Middle Left", true, new string[] { "BottleWithBluePotion" }],
        ["Hype Cave - Middle Left", true, new string[] { "Bottle" }],
        ["Hype Cave - Middle Left", true, new string[] { "BottleWithGoldBee" }],

        ["Hype Cave - Bottom", false, new string[] {  }],
        ["Hype Cave - Bottom", true, new string[] { "MoonPearl" }],
        ["Hype Cave - Bottom", true, new string[] { "BottleWithBee" }],
        ["Hype Cave - Bottom", true, new string[] { "BottleWithFairy" }],
        ["Hype Cave - Bottom", true, new string[] { "BottleWithRedPotion" }],
        ["Hype Cave - Bottom", true, new string[] { "BottleWithGreenPotion" }],
        ["Hype Cave - Bottom", true, new string[] { "BottleWithBluePotion" }],
        ["Hype Cave - Bottom", true, new string[] { "Bottle" }],
        ["Hype Cave - Bottom", true, new string[] { "BottleWithGoldBee" }],

        ["Hype Cave - NPC Item", false, new string[] {  }],
        ["Hype Cave - NPC Item", true, new string[] { "MoonPearl" }],
        ["Hype Cave - NPC Item", true, new string[] { "BottleWithBee" }],
        ["Hype Cave - NPC Item", true, new string[] { "BottleWithFairy" }],
        ["Hype Cave - NPC Item", true, new string[] { "BottleWithRedPotion" }],
        ["Hype Cave - NPC Item", true, new string[] { "BottleWithGreenPotion" }],
        ["Hype Cave - NPC Item", true, new string[] { "BottleWithBluePotion" }],
        ["Hype Cave - NPC Item", true, new string[] { "Bottle" }],
        ["Hype Cave - NPC Item", true, new string[] { "BottleWithGoldBee" }],

        ["Stumpy", false, new string[] {  }],
        ["Stumpy", true, new string[] { "MoonPearl" }],
        ["Stumpy", true, new string[] { "MagicMirror" }],
        ["Stumpy", true, new string[] { "BottleWithBee" }],
        ["Stumpy", true, new string[] { "BottleWithFairy" }],
        ["Stumpy", true, new string[] { "BottleWithRedPotion" }],
        ["Stumpy", true, new string[] { "BottleWithGreenPotion" }],
        ["Stumpy", true, new string[] { "BottleWithBluePotion" }],
        ["Stumpy", true, new string[] { "Bottle" }],
        ["Stumpy", true, new string[] { "BottleWithGoldBee" }],

        ["Digging Game", false, new string[] {  }],
        ["Digging Game", true, new string[] { "MoonPearl" }],
        ["Digging Game", true, new string[] { "BottleWithBee" }],
        ["Digging Game", true, new string[] { "BottleWithFairy" }],
        ["Digging Game", true, new string[] { "BottleWithRedPotion" }],
        ["Digging Game", true, new string[] { "BottleWithGreenPotion" }],
        ["Digging Game", true, new string[] { "BottleWithBluePotion" }],
        ["Digging Game", true, new string[] { "Bottle" }],
        ["Digging Game", true, new string[] { "BottleWithGoldBee" }],
    ];
}
