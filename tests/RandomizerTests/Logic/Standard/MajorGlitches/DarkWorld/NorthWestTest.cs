namespace RandomizerTests.Logic.Standard.MajorGlitches.DarkWorld;

[TestClass]
public sealed class NorthWestTest : StandardMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Brewery", false, new string[] {  } },
        new object[] { "Brewery", true, new string[] { "MoonPearl" } },
        new object[] { "Brewery", true, new string[] { "BottleWithBee" } },
        new object[] { "Brewery", true, new string[] { "BottleWithFairy" } },
        new object[] { "Brewery", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Brewery", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Brewery", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Brewery", true, new string[] { "Bottle" } },
        new object[] { "Brewery", true, new string[] { "BottleWithGoldBee" } },

        new object[] { "C-Shaped House", true, new string[] {  } },

        new object[] { "Chest Game", true, new string[] {  } },

        new object[] { "Hammer Pegs Item", false, new string[] {  } },
        new object[] { "Hammer Pegs Item", true, new string[] { "MoonPearl", "Hammer" } },
        new object[] { "Hammer Pegs Item", true, new string[] { "BottleWithBee", "Hammer" } },
        new object[] { "Hammer Pegs Item", true, new string[] { "BottleWithFairy", "Hammer" } },
        new object[] { "Hammer Pegs Item", true, new string[] { "BottleWithRedPotion", "Hammer" } },
        new object[] { "Hammer Pegs Item", true, new string[] { "BottleWithGreenPotion", "Hammer" } },
        new object[] { "Hammer Pegs Item", true, new string[] { "BottleWithBluePotion", "Hammer" } },
        new object[] { "Hammer Pegs Item", true, new string[] { "Bottle", "Hammer" } },
        new object[] { "Hammer Pegs Item", true, new string[] { "BottleWithGoldBee", "Hammer" } },

        new object[] { "Bumper Cave Item", true, new string[] {  } },

        new object[] { "Blacksmith", false, new string[] {  } },
        new object[] { "Blacksmith", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Blacksmith", true, new string[] { "BottleWithBee" } },
        new object[] { "Blacksmith", true, new string[] { "BottleWithFairy" } },
        new object[] { "Blacksmith", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Blacksmith", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Blacksmith", true, new string[] { "BottleWithFairy" } },
        new object[] { "Blacksmith", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Blacksmith", true, new string[] { "Bottle" } },
        new object[] { "Blacksmith", true, new string[] { "BottleWithGoldBee" } },

        new object[] { "Purple Chest", false, new string[] {  } },
        new object[] { "Purple Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Purple Chest", true, new string[] { "BottleWithBee" } },
        new object[] { "Purple Chest", true, new string[] { "BottleWithFairy" } },
        new object[] { "Purple Chest", true, new string[] { "BottleWithRedPotion" } },
        new object[] { "Purple Chest", true, new string[] { "BottleWithGreenPotion" } },
        new object[] { "Purple Chest", true, new string[] { "BottleWithFairy" } },
        new object[] { "Purple Chest", true, new string[] { "BottleWithBluePotion" } },
        new object[] { "Purple Chest", true, new string[] { "Bottle" } },
        new object[] { "Purple Chest", true, new string[] { "BottleWithGoldBee" } },
    };
}
