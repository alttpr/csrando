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

    public static IEnumerable<object[]> TestData => [
        ["Superbunny Cave - Top", true, new string[] {  }],

        ["Superbunny Cave - Bottom", true, new string[] {  }],

        ["Hookshot Cave - Bottom Chest", false, new string[] {  }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "PegasusBoots", "MoonPearl" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "Hookshot", "MoonPearl" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "PegasusBoots", "BottleWithBee" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "Hookshot", "BottleWithBee" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "PegasusBoots", "BottleWithFairy" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "Hookshot", "BottleWithFairy" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "PegasusBoots", "BottleWithRedPotion" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "Hookshot", "BottleWithRedPotion" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "PegasusBoots", "BottleWithGreenPotion" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "Hookshot", "BottleWithGreenPotion" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "PegasusBoots", "BottleWithBluePotion" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "Hookshot", "BottleWithBluePotion" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "PegasusBoots", "Bottle" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "Hookshot", "Bottle" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "PegasusBoots", "BottleWithGoldBee" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "Hookshot", "BottleWithGoldBee" }],

        ["Hookshot Cave - Middle South Chest", false, new string[] {  }],
        ["Hookshot Cave - Middle South Chest", true, new string[] { "Hookshot", "MoonPearl" }],
        ["Hookshot Cave - Middle South Chest", true, new string[] { "Hookshot", "BottleWithBee" }],
        ["Hookshot Cave - Middle South Chest", true, new string[] { "Hookshot", "BottleWithFairy" }],
        ["Hookshot Cave - Middle South Chest", true, new string[] { "Hookshot", "BottleWithRedPotion" }],
        ["Hookshot Cave - Middle South Chest", true, new string[] { "Hookshot", "BottleWithGreenPotion" }],
        ["Hookshot Cave - Middle South Chest", true, new string[] { "Hookshot", "BottleWithBluePotion" }],
        ["Hookshot Cave - Middle South Chest", true, new string[] { "Hookshot", "Bottle" }],
        ["Hookshot Cave - Middle South Chest", true, new string[] { "Hookshot", "BottleWithGoldBee" }],

        ["Hookshot Cave - Middle North Chest", false, new string[] {  }],
        ["Hookshot Cave - Middle North Chest", true, new string[] { "Hookshot", "MoonPearl" }],
        ["Hookshot Cave - Middle North Chest", true, new string[] { "Hookshot", "BottleWithBee" }],
        ["Hookshot Cave - Middle North Chest", true, new string[] { "Hookshot", "BottleWithFairy" }],
        ["Hookshot Cave - Middle North Chest", true, new string[] { "Hookshot", "BottleWithRedPotion" }],
        ["Hookshot Cave - Middle North Chest", true, new string[] { "Hookshot", "BottleWithGreenPotion" }],
        ["Hookshot Cave - Middle North Chest", true, new string[] { "Hookshot", "BottleWithBluePotion" }],
        ["Hookshot Cave - Middle North Chest", true, new string[] { "Hookshot", "Bottle" }],
        ["Hookshot Cave - Middle North Chest", true, new string[] { "Hookshot", "BottleWithGoldBee" }],

        ["Hookshot Cave - Top Chest", false, new string[] {  }],
        ["Hookshot Cave - Top Chest", true, new string[] { "Hookshot", "MoonPearl" }],
        ["Hookshot Cave - Top Chest", true, new string[] { "Hookshot", "BottleWithBee" }],
        ["Hookshot Cave - Top Chest", true, new string[] { "Hookshot", "BottleWithFairy" }],
        ["Hookshot Cave - Top Chest", true, new string[] { "Hookshot", "BottleWithRedPotion" }],
        ["Hookshot Cave - Top Chest", true, new string[] { "Hookshot", "BottleWithGreenPotion" }],
        ["Hookshot Cave - Top Chest", true, new string[] { "Hookshot", "BottleWithBluePotion" }],
        ["Hookshot Cave - Top Chest", true, new string[] { "Hookshot", "Bottle" }],
        ["Hookshot Cave - Top Chest", true, new string[] { "Hookshot", "BottleWithGoldBee" }],
    ];
}
