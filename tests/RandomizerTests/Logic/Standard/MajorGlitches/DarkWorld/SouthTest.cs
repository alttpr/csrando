namespace RandomizerTests.Logic.Standard.MajorGlitches.DarkWorld;

[TestClass]
public sealed class SouthTest : StandardMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Hype Cave - Top", false, new string[] {  } },
        new object[] { "Hype Cave - Top", true, new string[] { "MoonPearl" } },
        new object[] { "Hype Cave - Top", true, new string[] { "BottleWithBee" } },
        new object[] { "Hype Cave - Top", true, new string[] { "BottleWithFairy" } },
        new object[] { "Hype Cave - Top", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Hype Cave - Top", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Hype Cave - Top", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Hype Cave - Top", true, new string[] { "Bottle" } },
        new object[] { "Hype Cave - Top", true, new string[] { "BottleWithGoldBee" } },

        new object[] { "Hype Cave - Middle Right", false, new string[] {  } },
        new object[] { "Hype Cave - Middle Right", true, new string[] { "MoonPearl" } },
        new object[] { "Hype Cave - Middle Right", true, new string[] { "BottleWithBee" } },
        new object[] { "Hype Cave - Middle Right", true, new string[] { "BottleWithFairy" } },
        new object[] { "Hype Cave - Middle Right", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Hype Cave - Middle Right", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Hype Cave - Middle Right", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Hype Cave - Middle Right", true, new string[] { "Bottle" } },
        new object[] { "Hype Cave - Middle Right", true, new string[] { "BottleWithGoldBee" } },

        new object[] { "Hype Cave - Middle Left", false, new string[] {  } },
        new object[] { "Hype Cave - Middle Left", true, new string[] { "MoonPearl" } },
        new object[] { "Hype Cave - Middle Left", true, new string[] { "BottleWithBee" } },
        new object[] { "Hype Cave - Middle Left", true, new string[] { "BottleWithFairy" } },
        new object[] { "Hype Cave - Middle Left", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Hype Cave - Middle Left", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Hype Cave - Middle Left", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Hype Cave - Middle Left", true, new string[] { "Bottle" } },
        new object[] { "Hype Cave - Middle Left", true, new string[] { "BottleWithGoldBee" } },

        new object[] { "Hype Cave - Bottom", false, new string[] {  } },
        new object[] { "Hype Cave - Bottom", true, new string[] { "MoonPearl" } },
        new object[] { "Hype Cave - Bottom", true, new string[] { "BottleWithBee" } },
        new object[] { "Hype Cave - Bottom", true, new string[] { "BottleWithFairy" } },
        new object[] { "Hype Cave - Bottom", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Hype Cave - Bottom", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Hype Cave - Bottom", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Hype Cave - Bottom", true, new string[] { "Bottle" } },
        new object[] { "Hype Cave - Bottom", true, new string[] { "BottleWithGoldBee" } },

        new object[] { "Hype Cave - NPC Item", false, new string[] {  } },
        new object[] { "Hype Cave - NPC Item", true, new string[] { "MoonPearl" } },
        new object[] { "Hype Cave - NPC Item", true, new string[] { "BottleWithBee" } },
        new object[] { "Hype Cave - NPC Item", true, new string[] { "BottleWithFairy" } },
        new object[] { "Hype Cave - NPC Item", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Hype Cave - NPC Item", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Hype Cave - NPC Item", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Hype Cave - NPC Item", true, new string[] { "Bottle" } },
        new object[] { "Hype Cave - NPC Item", true, new string[] { "BottleWithGoldBee" } },

        new object[] { "Stumpy", false, new string[] {  } },
        new object[] { "Stumpy", true, new string[] { "MoonPearl" } },
        new object[] { "Stumpy", true, new string[] { "MagicMirror" } },
        new object[] { "Stumpy", true, new string[] { "BottleWithBee" } },
        new object[] { "Stumpy", true, new string[] { "BottleWithFairy" } },
        new object[] { "Stumpy", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Stumpy", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Stumpy", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Stumpy", true, new string[] { "Bottle" } },
        new object[] { "Stumpy", true, new string[] { "BottleWithGoldBee" } },

        new object[] { "Digging Game", false, new string[] {  } },
        new object[] { "Digging Game", true, new string[] { "MoonPearl" } },
        new object[] { "Digging Game", true, new string[] { "BottleWithBee" } },
        new object[] { "Digging Game", true, new string[] { "BottleWithFairy" } },
        new object[] { "Digging Game", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Digging Game", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Digging Game", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Digging Game", true, new string[] { "Bottle" } },
        new object[] { "Digging Game", true, new string[] { "BottleWithGoldBee" } },
    };
}
