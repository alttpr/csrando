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

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Spiral Cave Item", false, new string[] {  } },
        new object[] { "Spiral Cave Item", true, new string[] { "MagicMirror", "ProgressiveGlove", "ProgressiveGlove", "Lamp", "UncleSword" } },
        new object[] { "Spiral Cave Item", true, new string[] { "MagicMirror", "TitansMitt", "Lamp", "UncleSword" } },
        new object[] { "Spiral Cave Item", true, new string[] { "MagicMirror", "ProgressiveGlove", "ProgressiveGlove", "PegasusBoots", "UncleSword" } },
        new object[] { "Spiral Cave Item", true, new string[] { "MagicMirror", "TitansMitt", "PegasusBoots", "UncleSword" } },
        new object[] { "Spiral Cave Item", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Paradox Cave Lower - Far Left", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "BottleWithFairy" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "Bottle" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "BottleWithGoldBee" } },

        new object[] { "Paradox Cave Lower - Left", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "BottleWithBee" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "BottleWithFairy" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "Bottle" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "BottleWithGoldBee" } },

        new object[] { "Paradox Cave Lower - Middle", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "BottleWithBee" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "BottleWithFairy" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "Bottle" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "BottleWithGoldBee" } },

        new object[] { "Paradox Cave Lower - Right", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "BottleWithBee" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "BottleWithFairy" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "Bottle" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "BottleWithGoldBee" } },

        new object[] { "Paradox Cave Lower - Far Right", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "BottleWithBee" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "BottleWithFairy" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "Bottle" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "BottleWithGoldBee" } },

        new object[] { "Paradox Cave Upper - Left", false, new string[] {  } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "MoonPearl" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "BottleWithBee" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "BottleWithFairy" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "Bottle" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "BottleWithGoldBee" } },

        new object[] { "Paradox Cave Upper - Right", false, new string[] {  } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "MoonPearl" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "BottleWithBee" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "BottleWithFairy" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "Bottle" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "BottleWithGoldBee" } },

        new object[] { "Mimic Cave", false, new string[] {  } },
        new object[] { "Mimic Cave", true, new string[] { "MoonPearl", "Hammer" } },
        new object[] { "Mimic Cave", true, new string[] { "BottleWithBee", "Hammer" } },
        new object[] { "Mimic Cave", true, new string[] { "BottleWithFairy", "Hammer" } },
        new object[] { "Mimic Cave", true, new string[] { "BottleWithRedPotion", "Hammer" } },
        new object[] { "Mimic Cave", true, new string[] { "BottleWithGreenPotion", "Hammer" } },
        new object[] { "Mimic Cave", true, new string[] { "BottleWithBluePotion", "Hammer" } },
        new object[] { "Mimic Cave", true, new string[] { "Bottle", "Hammer" } },
        new object[] { "Mimic Cave", true, new string[] { "BottleWithGoldBee", "Hammer" } },

        new object[] { "Ether Tablet", false, new string[] {  } },
        new object[] { "Ether Tablet", true, new string[] { "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "BookOfMudora", "L4Sword" } },

        new object[] { "Spectacle Rock", true, new string[] {  } },
    };
}
