namespace RandomizerTests.Logic.Inverted.MajorGlitches.DeathMountain;

[TestClass]
public sealed class EastTest : InvertedMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Spiral Cave Item", false, new string[] {  }],
        ["Spiral Cave Item", true, new string[] { "MagicMirror", "ProgressiveGlove", "ProgressiveGlove", "Lamp", "UncleSword" }],
        ["Spiral Cave Item", true, new string[] { "MagicMirror", "TitansMitt", "Lamp", "UncleSword" }],
        ["Spiral Cave Item", true, new string[] { "MagicMirror", "ProgressiveGlove", "ProgressiveGlove", "PegasusBoots", "UncleSword" }],
        ["Spiral Cave Item", true, new string[] { "MagicMirror", "TitansMitt", "PegasusBoots", "UncleSword" }],
        ["Spiral Cave Item", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Paradox Cave Lower - Far Left", false, new string[] {  }],
        ["Paradox Cave Lower - Far Left", true, new string[] { "MoonPearl" }],
        ["Paradox Cave Lower - Far Left", true, new string[] { "BottleWithFairy" }],
        ["Paradox Cave Lower - Far Left", true, new string[] { "BottleWithRedPotion" }],
        ["Paradox Cave Lower - Far Left", true, new string[] { "BottleWithGreenPotion" }],
        ["Paradox Cave Lower - Far Left", true, new string[] { "BottleWithBluePotion" }],
        ["Paradox Cave Lower - Far Left", true, new string[] { "Bottle" }],
        ["Paradox Cave Lower - Far Left", true, new string[] { "BottleWithGoldBee" }],

        ["Paradox Cave Lower - Left", false, new string[] {  }],
        ["Paradox Cave Lower - Left", true, new string[] { "MoonPearl" }],
        ["Paradox Cave Lower - Left", true, new string[] { "BottleWithBee" }],
        ["Paradox Cave Lower - Left", true, new string[] { "BottleWithFairy" }],
        ["Paradox Cave Lower - Left", true, new string[] { "BottleWithRedPotion" }],
        ["Paradox Cave Lower - Left", true, new string[] { "BottleWithGreenPotion" }],
        ["Paradox Cave Lower - Left", true, new string[] { "BottleWithBluePotion" }],
        ["Paradox Cave Lower - Left", true, new string[] { "Bottle" }],
        ["Paradox Cave Lower - Left", true, new string[] { "BottleWithGoldBee" }],

        ["Paradox Cave Lower - Middle", false, new string[] {  }],
        ["Paradox Cave Lower - Middle", true, new string[] { "MoonPearl" }],
        ["Paradox Cave Lower - Middle", true, new string[] { "BottleWithBee" }],
        ["Paradox Cave Lower - Middle", true, new string[] { "BottleWithFairy" }],
        ["Paradox Cave Lower - Middle", true, new string[] { "BottleWithRedPotion" }],
        ["Paradox Cave Lower - Middle", true, new string[] { "BottleWithGreenPotion" }],
        ["Paradox Cave Lower - Middle", true, new string[] { "BottleWithBluePotion" }],
        ["Paradox Cave Lower - Middle", true, new string[] { "Bottle" }],
        ["Paradox Cave Lower - Middle", true, new string[] { "BottleWithGoldBee" }],

        ["Paradox Cave Lower - Right", false, new string[] {  }],
        ["Paradox Cave Lower - Right", true, new string[] { "MoonPearl" }],
        ["Paradox Cave Lower - Right", true, new string[] { "BottleWithBee" }],
        ["Paradox Cave Lower - Right", true, new string[] { "BottleWithFairy" }],
        ["Paradox Cave Lower - Right", true, new string[] { "BottleWithRedPotion" }],
        ["Paradox Cave Lower - Right", true, new string[] { "BottleWithGreenPotion" }],
        ["Paradox Cave Lower - Right", true, new string[] { "BottleWithBluePotion" }],
        ["Paradox Cave Lower - Right", true, new string[] { "Bottle" }],
        ["Paradox Cave Lower - Right", true, new string[] { "BottleWithGoldBee" }],

        ["Paradox Cave Lower - Far Right", false, new string[] {  }],
        ["Paradox Cave Lower - Far Right", true, new string[] { "MoonPearl" }],
        ["Paradox Cave Lower - Far Right", true, new string[] { "BottleWithBee" }],
        ["Paradox Cave Lower - Far Right", true, new string[] { "BottleWithFairy" }],
        ["Paradox Cave Lower - Far Right", true, new string[] { "BottleWithRedPotion" }],
        ["Paradox Cave Lower - Far Right", true, new string[] { "BottleWithGreenPotion" }],
        ["Paradox Cave Lower - Far Right", true, new string[] { "BottleWithBluePotion" }],
        ["Paradox Cave Lower - Far Right", true, new string[] { "Bottle" }],
        ["Paradox Cave Lower - Far Right", true, new string[] { "BottleWithGoldBee" }],

        ["Paradox Cave Upper - Left", false, new string[] {  }],
        ["Paradox Cave Upper - Left", true, new string[] { "MoonPearl" }],
        ["Paradox Cave Upper - Left", true, new string[] { "BottleWithBee" }],
        ["Paradox Cave Upper - Left", true, new string[] { "BottleWithFairy" }],
        ["Paradox Cave Upper - Left", true, new string[] { "BottleWithRedPotion" }],
        ["Paradox Cave Upper - Left", true, new string[] { "BottleWithGreenPotion" }],
        ["Paradox Cave Upper - Left", true, new string[] { "BottleWithBluePotion" }],
        ["Paradox Cave Upper - Left", true, new string[] { "Bottle" }],
        ["Paradox Cave Upper - Left", true, new string[] { "BottleWithGoldBee" }],

        ["Paradox Cave Upper - Right", false, new string[] {  }],
        ["Paradox Cave Upper - Right", true, new string[] { "MoonPearl" }],
        ["Paradox Cave Upper - Right", true, new string[] { "BottleWithBee" }],
        ["Paradox Cave Upper - Right", true, new string[] { "BottleWithFairy" }],
        ["Paradox Cave Upper - Right", true, new string[] { "BottleWithRedPotion" }],
        ["Paradox Cave Upper - Right", true, new string[] { "BottleWithGreenPotion" }],
        ["Paradox Cave Upper - Right", true, new string[] { "BottleWithBluePotion" }],
        ["Paradox Cave Upper - Right", true, new string[] { "Bottle" }],
        ["Paradox Cave Upper - Right", true, new string[] { "BottleWithGoldBee" }],

        ["Mimic Cave", false, new string[] {  }],
        ["Mimic Cave", true, new string[] { "MoonPearl", "Hammer" }],
        ["Mimic Cave", true, new string[] { "BottleWithBee", "Hammer" }],
        ["Mimic Cave", true, new string[] { "BottleWithFairy", "Hammer" }],
        ["Mimic Cave", true, new string[] { "BottleWithRedPotion", "Hammer" }],
        ["Mimic Cave", true, new string[] { "BottleWithGreenPotion", "Hammer" }],
        ["Mimic Cave", true, new string[] { "BottleWithBluePotion", "Hammer" }],
        ["Mimic Cave", true, new string[] { "Bottle", "Hammer" }],
        ["Mimic Cave", true, new string[] { "BottleWithGoldBee", "Hammer" }],

        ["Ether Tablet", false, new string[] {  }],
        ["Ether Tablet", true, new string[] { "BookOfMudora", "ProgressiveSword", "ProgressiveSword" }],
        ["Ether Tablet", true, new string[] { "BookOfMudora", "L2Sword" }],
        ["Ether Tablet", true, new string[] { "BookOfMudora", "L3Sword" }],
        ["Ether Tablet", true, new string[] { "BookOfMudora", "L4Sword" }],

        ["Spectacle Rock", true, new string[] {  }],
    ];
}
