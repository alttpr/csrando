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

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Mire Shed - Left", false, new string[] {  } },
        new object[] { "Mire Shed - Left", true, new string[] { "MoonPearl" } },
        new object[] { "Mire Shed - Left", true, new string[] { "BottleWithBee" } },
        new object[] { "Mire Shed - Left", true, new string[] { "BottleWithFairy" } },
        new object[] { "Mire Shed - Left", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Mire Shed - Left", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Mire Shed - Left", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Mire Shed - Left", true, new string[] { "Bottle" } },
        new object[] { "Mire Shed - Left", true, new string[] { "BottleWithGoldBee" } },
        new object[] { "Mire Shed - Left", true, new string[] { "MagicMirror" } },

        new object[] { "Mire Shed - Right", false, new string[] {  } },
        new object[] { "Mire Shed - Right", true, new string[] { "MoonPearl" } },
        new object[] { "Mire Shed - Right", true, new string[] { "BottleWithBee" } },
        new object[] { "Mire Shed - Right", true, new string[] { "BottleWithFairy" } },
        new object[] { "Mire Shed - Right", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Mire Shed - Right", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Mire Shed - Right", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Mire Shed - Right", true, new string[] { "Bottle" } },
        new object[] { "Mire Shed - Right", true, new string[] { "BottleWithGoldBee" } },
        new object[] { "Mire Shed - Right", true, new string[] { "MagicMirror" } },
    };
}
