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

    public static IEnumerable<object[]> TestData => [
        ["Brewery Item", false, new string[] {  }],
        ["Brewery Item", true, new string[] { "MoonPearl" }],
        ["Brewery Item", true, new string[] { "BottleWithBee" }],
        ["Brewery Item", true, new string[] { "BottleWithFairy" }],
        ["Brewery Item", true, new string[] { "BottleWithRedPotion" }],
        ["Brewery Item", true, new string[] { "BottleWithGreenPotion" }],
        ["Brewery Item", true, new string[] { "BottleWithBluePotion" }],
        ["Brewery Item", true, new string[] { "Bottle" }],
        ["Brewery Item", true, new string[] { "BottleWithGoldBee" }],

        ["C-Shaped House Item", true, new string[] {  }],

        ["Chest Game", true, new string[] {  }],

        ["Hammer Pegs Item", false, new string[] {  }],
        ["Hammer Pegs Item", true, new string[] { "MoonPearl", "Hammer" }],
        ["Hammer Pegs Item", true, new string[] { "BottleWithBee", "Hammer" }],
        ["Hammer Pegs Item", true, new string[] { "BottleWithFairy", "Hammer" }],
        ["Hammer Pegs Item", true, new string[] { "BottleWithRedPotion", "Hammer" }],
        ["Hammer Pegs Item", true, new string[] { "BottleWithGreenPotion", "Hammer" }],
        ["Hammer Pegs Item", true, new string[] { "BottleWithBluePotion", "Hammer" }],
        ["Hammer Pegs Item", true, new string[] { "Bottle", "Hammer" }],
        ["Hammer Pegs Item", true, new string[] { "BottleWithGoldBee", "Hammer" }],

        ["Bumper Cave Item", true, new string[] {  }],

        ["Blacksmith Item", false, new string[] {  }],
        ["Blacksmith Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Blacksmith Item", true, new string[] { "BottleWithBee" }],
        ["Blacksmith Item", true, new string[] { "BottleWithFairy" }],
        ["Blacksmith Item", true, new string[] { "BottleWithRedPotion" }],
        ["Blacksmith Item", true, new string[] { "BottleWithGreenPotion" }],
        ["Blacksmith Item", true, new string[] { "BottleWithFairy" }],
        ["Blacksmith Item", true, new string[] { "BottleWithBluePotion" }],
        ["Blacksmith Item", true, new string[] { "Bottle" }],
        ["Blacksmith Item", true, new string[] { "BottleWithGoldBee" }],

        ["Purple Chest Item", false, new string[] {  }],
        ["Purple Chest Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Purple Chest Item", true, new string[] { "BottleWithBee" }],
        ["Purple Chest Item", true, new string[] { "BottleWithFairy" }],
        ["Purple Chest Item", true, new string[] { "BottleWithRedPotion" }],
        ["Purple Chest Item", true, new string[] { "BottleWithGreenPotion" }],
        ["Purple Chest Item", true, new string[] { "BottleWithFairy" }],
        ["Purple Chest Item", true, new string[] { "BottleWithBluePotion" }],
        ["Purple Chest Item", true, new string[] { "Bottle" }],
        ["Purple Chest Item", true, new string[] { "BottleWithGoldBee" }],
    ];
}
