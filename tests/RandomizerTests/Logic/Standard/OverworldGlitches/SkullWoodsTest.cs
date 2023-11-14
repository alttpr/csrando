namespace RandomizerTests.Logic.Standard.OverworldGlitches;

[TestClass]
public sealed class SkullWoodsTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Skull Woods - Big Chest", false, new string[] {  }],
        ["Skull Woods - Big Chest", true, new string[] { "MagicMirror", "PegasusBoots", "BigKeyD3" }],
        ["Skull Woods - Big Chest", true, new string[] { "MoonPearl", "PegasusBoots", "BigKeyD3" }],

        ["Skull Woods - Big Key Chest", false, new string[] {  }],
        ["Skull Woods - Big Key Chest", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Skull Woods - Big Key Chest", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Skull Woods - Compass Chest", false, new string[] {  }],
        ["Skull Woods - Compass Chest", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Skull Woods - Compass Chest", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Skull Woods - Map Chest", false, new string[] {  }],
        ["Skull Woods - Map Chest", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Skull Woods - Map Chest", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Skull Woods - Bridge Room", false, new string[] {  }],
        ["Skull Woods - Bridge Room", true, new string[] { "MoonPearl", "PegasusBoots", "FireRod" }],

        ["Skull Woods - Pot Prison", false, new string[] {  }],
        ["Skull Woods - Pot Prison", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Skull Woods - Pot Prison", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Skull Woods - Pinball Room", false, new string[] {  }],
        ["Skull Woods - Pinball Room", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Skull Woods - Pinball Room", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Skull Woods - Boss", false, new string[] {  }],
        ["Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "PegasusBoots", "FireRod", "UncleSword" }],
        ["Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "PegasusBoots", "FireRod", "MasterSword" }],
        ["Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "PegasusBoots", "FireRod", "L3Sword" }],
        ["Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "PegasusBoots", "FireRod", "L4Sword" }],
        ["Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "PegasusBoots", "FireRod", "ProgressiveSword" }],
    ];
}
