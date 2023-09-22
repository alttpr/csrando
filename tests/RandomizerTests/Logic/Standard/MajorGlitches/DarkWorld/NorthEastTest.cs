namespace RandomizerTests.Logic.Standard.MajorGlitches.DarkWorld;

[TestClass]
public sealed class NorthEastTest : StandardMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Catfish", false, new string[] {  } },
        new object[] { "Catfish", true, new string[] { "MoonPearl" } },
        new object[] { "Catfish", true, new string[] { "BottleWithBee" } },
        new object[] { "Catfish", true, new string[] { "BottleWithFairy" } },
        new object[] { "Catfish", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Catfish", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Catfish", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Catfish", true, new string[] { "Bottle" } },
        new object[] { "Catfish", true, new string[] { "BottleWithGoldBee" } },

        new object[] { "Pyramid", true, new string[] {  } },

        new object[] { "Pyramid Fairy - Left", false, new string[] {  } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "MagicMirror" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "Crystal5", "Crystal6", "MoonPearl", "Hammer" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "Crystal5", "Crystal6", "BottleWithBee" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "Crystal5", "Crystal6", "BottleWithFairy" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "Crystal5", "Crystal6", "BottleWithRedPotion" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "Crystal5", "Crystal6", "BottleWithGreenPotion" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "Crystal5", "Crystal6", "BottleWithBluePotion" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "Crystal5", "Crystal6", "Bottle" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "Crystal5", "Crystal6", "BottleWithGoldBee" } },

        new object[] { "Pyramid Fairy - Right", false, new string[] {  } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "MagicMirror" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "Crystal5", "Crystal6", "MoonPearl", "Hammer" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "Crystal5", "Crystal6", "BottleWithBee" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "Crystal5", "Crystal6", "BottleWithFairy" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "Crystal5", "Crystal6", "BottleWithRedPotion" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "Crystal5", "Crystal6", "BottleWithGreenPotion" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "Crystal5", "Crystal6", "BottleWithBluePotion" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "Crystal5", "Crystal6", "Bottle" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "Crystal5", "Crystal6", "BottleWithGoldBee" } },

        new object[] { "Ganon", false, new string[] {  } },
    };
}
