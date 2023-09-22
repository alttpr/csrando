namespace RandomizerTests.Logic.Standard.MajorGlitches.DarkWorld.DeathMountain;

[TestClass]
public sealed class EastTest : StandardMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Superbunny Cave - Top", true, new string[] {  } },

        new object[] { "Superbunny Cave - Bottom", true, new string[] {  } },

        new object[] { "Hookshot Cave - Bottom Right", false, new string[] {  } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "PegasusBoots", "MoonPearl" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "Hookshot", "MoonPearl" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "PegasusBoots", "BottleWithBee" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "Hookshot", "BottleWithBee" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "PegasusBoots", "BottleWithFairy" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "Hookshot", "BottleWithFairy" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "PegasusBoots", "BottleWithRedPotion" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "Hookshot", "BottleWithRedPotion" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "PegasusBoots", "BottleWithGreenPotion" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "Hookshot", "BottleWithGreenPotion" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "PegasusBoots", "BottleWithBluePotion" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "Hookshot", "BottleWithBluePotion" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "PegasusBoots", "Bottle" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "Hookshot", "Bottle" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "PegasusBoots", "BottleWithGoldBee" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "Hookshot", "BottleWithGoldBee" } },

        new object[] { "Hookshot Cave - Bottom Left", false, new string[] {  } },
        new object[] { "Hookshot Cave - Bottom Left", true, new string[] { "Hookshot", "MoonPearl" } },
        new object[] { "Hookshot Cave - Bottom Left", true, new string[] { "Hookshot", "BottleWithBee" } },
        new object[] { "Hookshot Cave - Bottom Left", true, new string[] { "Hookshot", "BottleWithFairy" } },
        new object[] { "Hookshot Cave - Bottom Left", true, new string[] { "Hookshot", "BottleWithRedPotion" } },
        new object[] { "Hookshot Cave - Bottom Left", true, new string[] { "Hookshot", "BottleWithGreenPotion" } },
        new object[] { "Hookshot Cave - Bottom Left", true, new string[] { "Hookshot", "BottleWithBluePotion" } },
        new object[] { "Hookshot Cave - Bottom Left", true, new string[] { "Hookshot", "Bottle" } },
        new object[] { "Hookshot Cave - Bottom Left", true, new string[] { "Hookshot", "BottleWithGoldBee" } },

        new object[] { "Hookshot Cave - Top Left", false, new string[] {  } },
        new object[] { "Hookshot Cave - Top Left", true, new string[] { "Hookshot", "MoonPearl" } },
        new object[] { "Hookshot Cave - Top Left", true, new string[] { "Hookshot", "BottleWithBee" } },
        new object[] { "Hookshot Cave - Top Left", true, new string[] { "Hookshot", "BottleWithFairy" } },
        new object[] { "Hookshot Cave - Top Left", true, new string[] { "Hookshot", "BottleWithRedPotion" } },
        new object[] { "Hookshot Cave - Top Left", true, new string[] { "Hookshot", "BottleWithGreenPotion" } },
        new object[] { "Hookshot Cave - Top Left", true, new string[] { "Hookshot", "BottleWithBluePotion" } },
        new object[] { "Hookshot Cave - Top Left", true, new string[] { "Hookshot", "Bottle" } },
        new object[] { "Hookshot Cave - Top Left", true, new string[] { "Hookshot", "BottleWithGoldBee" } },

        new object[] { "Hookshot Cave - Top Right", false, new string[] {  } },
        new object[] { "Hookshot Cave - Top Right", true, new string[] { "Hookshot", "MoonPearl" } },
        new object[] { "Hookshot Cave - Top Right", true, new string[] { "Hookshot", "BottleWithBee" } },
        new object[] { "Hookshot Cave - Top Right", true, new string[] { "Hookshot", "BottleWithFairy" } },
        new object[] { "Hookshot Cave - Top Right", true, new string[] { "Hookshot", "BottleWithRedPotion" } },
        new object[] { "Hookshot Cave - Top Right", true, new string[] { "Hookshot", "BottleWithGreenPotion" } },
        new object[] { "Hookshot Cave - Top Right", true, new string[] { "Hookshot", "BottleWithBluePotion" } },
        new object[] { "Hookshot Cave - Top Right", true, new string[] { "Hookshot", "Bottle" } },
        new object[] { "Hookshot Cave - Top Right", true, new string[] { "Hookshot", "BottleWithGoldBee" } },
    };
}
