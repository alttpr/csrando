namespace RandomizerTests.Logic.Alttp.Inverted.NoGlitches;

using RandomizerTests.Logic.Alttp;

[TestClass]
public class DesertPalaceTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Desert Palace - Map Chest", false, new string[] {  }],
        ["Desert Palace - Map Chest", false, new string[] { "MoonPearl", "AgahnimDefeated" }],
        ["Desert Palace - Map Chest", false, new string[] { "BookOfMudora", "AgahnimDefeated" }],
        ["Desert Palace - Map Chest", true, new string[] { "BookOfMudora", "MoonPearl", "AgahnimDefeated" }],
        ["Desert Palace - Map Chest", true, new string[] { "BookOfMudora", "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Desert Palace - Map Chest", true, new string[] { "BookOfMudora", "MoonPearl", "PowerGlove", "Hammer" }],
        ["Desert Palace - Map Chest", true, new string[] { "BookOfMudora", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Desert Palace - Map Chest", true, new string[] { "BookOfMudora", "MoonPearl", "TitansMitt" }],

        ["Desert Palace - Big Chest", false, new string[] {  }],
        ["Desert Palace - Big Chest", false, new string[] { "BigKeyP2", "MoonPearl", "AgahnimDefeated" }],
        ["Desert Palace - Big Chest", false, new string[] { "BookOfMudora", "MoonPearl", "AgahnimDefeated" }],
        ["Desert Palace - Big Chest", false, new string[] { "BookOfMudora", "BigKeyP2", "AgahnimDefeated" }],
        ["Desert Palace - Big Chest", true, new string[] { "BookOfMudora", "BigKeyP2", "MoonPearl", "AgahnimDefeated" }],
        ["Desert Palace - Big Chest", true, new string[] { "BookOfMudora", "BigKeyP2", "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Desert Palace - Big Chest", true, new string[] { "BookOfMudora", "BigKeyP2", "MoonPearl", "PowerGlove", "Hammer" }],
        ["Desert Palace - Big Chest", true, new string[] { "BookOfMudora", "BigKeyP2", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Desert Palace - Big Chest", true, new string[] { "BookOfMudora", "BigKeyP2", "MoonPearl", "TitansMitt" }],

        ["Desert Palace - Torch", false, new string[] {  }],
        ["Desert Palace - Torch", false, new string[] { "PegasusBoots", "MoonPearl", "AgahnimDefeated" }],
        ["Desert Palace - Torch", false, new string[] { "BookOfMudora", "MoonPearl", "AgahnimDefeated" }],
        ["Desert Palace - Torch", false, new string[] { "BookOfMudora", "PegasusBoots", "AgahnimDefeated" }],
        ["Desert Palace - Torch", true, new string[] { "BookOfMudora", "PegasusBoots", "MoonPearl", "AgahnimDefeated" }],
        ["Desert Palace - Torch", true, new string[] { "BookOfMudora", "PegasusBoots", "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Desert Palace - Torch", true, new string[] { "BookOfMudora", "PegasusBoots", "MoonPearl", "PowerGlove", "Hammer" }],
        ["Desert Palace - Torch", true, new string[] { "BookOfMudora", "PegasusBoots", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Desert Palace - Torch", true, new string[] { "BookOfMudora", "PegasusBoots", "MoonPearl", "TitansMitt" }],

        ["Desert Palace - Compass Chest", false, new string[] {  }],
        ["Desert Palace - Compass Chest", false, new string[] { "KeyP2", "MoonPearl", "AgahnimDefeated" }],
        ["Desert Palace - Compass Chest", false, new string[] { "BookOfMudora", "MoonPearl", "AgahnimDefeated" }],
        ["Desert Palace - Compass Chest", false, new string[] { "BookOfMudora", "KeyP2", "AgahnimDefeated" }],
        ["Desert Palace - Compass Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "AgahnimDefeated" }],
        ["Desert Palace - Compass Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Desert Palace - Compass Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "PowerGlove", "Hammer" }],
        ["Desert Palace - Compass Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Desert Palace - Compass Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "TitansMitt" }],

        ["Desert Palace - Big Key Chest", false, new string[] {  }],
        ["Desert Palace - Big Key Chest", false, new string[] { "KeyP2", "MoonPearl", "AgahnimDefeated" }],
        ["Desert Palace - Big Key Chest", false, new string[] { "BookOfMudora", "MoonPearl", "AgahnimDefeated" }],
        ["Desert Palace - Big Key Chest", false, new string[] { "BookOfMudora", "KeyP2", "AgahnimDefeated" }],
        ["Desert Palace - Big Key Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "AgahnimDefeated" }],
        ["Desert Palace - Big Key Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Desert Palace - Big Key Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "PowerGlove", "Hammer" }],
        ["Desert Palace - Big Key Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Desert Palace - Big Key Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "TitansMitt" }],

        ["Desert Palace - Boss", false, new string[] {  }],
        ["Desert Palace - Boss", false, new string[] { "UncleSword", "MoonPearl", "ProgressiveGlove", "AgahnimDefeated", "BookOfMudora", "Lamp", "BigKeyP2" }],
        ["Desert Palace - Boss", false, new string[] { "UncleSword", "MoonPearl", "ProgressiveGlove", "AgahnimDefeated", "KeyP2", "BookOfMudora", "Lamp" }],
        ["Desert Palace - Boss", false, new string[] { "UncleSword", "MoonPearl", "ProgressiveGlove", "AgahnimDefeated", "KeyP2", "Lamp", "BigKeyP2" }],
        ["Desert Palace - Boss", false, new string[] { "UncleSword", "ProgressiveGlove", "AgahnimDefeated", "KeyP2", "BookOfMudora", "Lamp", "BigKeyP2" }],
        ["Desert Palace - Boss", false, new string[] { "UncleSword", "MoonPearl", "AgahnimDefeated", "KeyP2", "BookOfMudora", "Lamp", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "MoonPearl", "ProgressiveGlove", "AgahnimDefeated", "KeyP2", "BookOfMudora", "Lamp", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "MoonPearl", "PowerGlove", "AgahnimDefeated", "KeyP2", "BookOfMudora", "Lamp", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer", "KeyP2", "BookOfMudora", "Lamp", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "MoonPearl", "PowerGlove", "Hammer", "KeyP2", "BookOfMudora", "Lamp", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "KeyP2", "BookOfMudora", "Lamp", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "MoonPearl", "TitansMitt", "KeyP2", "BookOfMudora", "Lamp", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "MoonPearl", "ProgressiveGlove", "AgahnimDefeated", "KeyP2", "BookOfMudora", "FireRod", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "MoonPearl", "PowerGlove", "AgahnimDefeated", "KeyP2", "BookOfMudora", "FireRod", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer", "KeyP2", "BookOfMudora", "FireRod", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "MoonPearl", "PowerGlove", "Hammer", "KeyP2", "BookOfMudora", "FireRod", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "KeyP2", "BookOfMudora", "FireRod", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "MoonPearl", "TitansMitt", "KeyP2", "BookOfMudora", "FireRod", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "MoonPearl", "TitansMitt", "KeyP2", "BookOfMudora", "FireRod", "BigKeyP2" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
