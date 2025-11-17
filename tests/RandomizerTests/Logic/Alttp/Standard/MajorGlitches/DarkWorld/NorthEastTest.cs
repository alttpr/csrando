namespace RandomizerTests.Logic.Alttp.Standard.MajorGlitches.DarkWorld;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Standard.MajorGlitches;

[TestClass]
public sealed class NorthEastTest : StandardMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Catfish", false, new string[] {  }],
        ["Catfish", true, new string[] { "MoonPearl" }],
        ["Catfish", true, new string[] { "BottleWithBee" }],
        ["Catfish", true, new string[] { "BottleWithFairy" }],
        ["Catfish", true, new string[] { "BottleWithRedPotion" }],
        ["Catfish", true, new string[] { "BottleWithGreenPotion" }],
        ["Catfish", true, new string[] { "BottleWithBluePotion" }],
        ["Catfish", true, new string[] { "Bottle" }],
        ["Catfish", true, new string[] { "BottleWithGoldBee" }],

        ["Pyramid", true, new string[] {  }],

        ["Pyramid Fairy - Left", false, new string[] {  }],
        ["Pyramid Fairy - Left", true, new string[] { "MagicMirror" }],
        ["Pyramid Fairy - Left", true, new string[] { "Crystal5", "Crystal6", "MoonPearl", "Hammer" }],
        ["Pyramid Fairy - Left", true, new string[] { "Crystal5", "Crystal6", "BottleWithBee" }],
        ["Pyramid Fairy - Left", true, new string[] { "Crystal5", "Crystal6", "BottleWithFairy" }],
        ["Pyramid Fairy - Left", true, new string[] { "Crystal5", "Crystal6", "BottleWithRedPotion" }],
        ["Pyramid Fairy - Left", true, new string[] { "Crystal5", "Crystal6", "BottleWithGreenPotion" }],
        ["Pyramid Fairy - Left", true, new string[] { "Crystal5", "Crystal6", "BottleWithBluePotion" }],
        ["Pyramid Fairy - Left", true, new string[] { "Crystal5", "Crystal6", "Bottle" }],
        ["Pyramid Fairy - Left", true, new string[] { "Crystal5", "Crystal6", "BottleWithGoldBee" }],

        ["Pyramid Fairy - Right", false, new string[] {  }],
        ["Pyramid Fairy - Right", true, new string[] { "MagicMirror" }],
        ["Pyramid Fairy - Right", true, new string[] { "Crystal5", "Crystal6", "MoonPearl", "Hammer" }],
        ["Pyramid Fairy - Right", true, new string[] { "Crystal5", "Crystal6", "BottleWithBee" }],
        ["Pyramid Fairy - Right", true, new string[] { "Crystal5", "Crystal6", "BottleWithFairy" }],
        ["Pyramid Fairy - Right", true, new string[] { "Crystal5", "Crystal6", "BottleWithRedPotion" }],
        ["Pyramid Fairy - Right", true, new string[] { "Crystal5", "Crystal6", "BottleWithGreenPotion" }],
        ["Pyramid Fairy - Right", true, new string[] { "Crystal5", "Crystal6", "BottleWithBluePotion" }],
        ["Pyramid Fairy - Right", true, new string[] { "Crystal5", "Crystal6", "Bottle" }],
        ["Pyramid Fairy - Right", true, new string[] { "Crystal5", "Crystal6", "BottleWithGoldBee" }],

        ["Ganon", false, new string[] {  }],
    ];
}
