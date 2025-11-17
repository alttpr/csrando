namespace RandomizerTests.Logic.Alttp.Inverted.NoGlitches;

using RandomizerTests.Logic.Alttp;

[TestClass]
public class EasternPalaceTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Eastern Palace - Compass Chest", false, new string[] {  }],
        ["Eastern Palace - Compass Chest", false, new string[] { "AgahnimDefeated" }],
        ["Eastern Palace - Compass Chest", true, new string[] { "MoonPearl", "AgahnimDefeated" }],
        ["Eastern Palace - Compass Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Eastern Palace - Compass Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Eastern Palace - Compass Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Eastern Palace - Compass Chest", true, new string[] { "MoonPearl", "TitansMitt" }],

        ["Eastern Palace - Cannonball Chest", false, new string[] {  }],
        ["Eastern Palace - Cannonball Chest", false, new string[] { "AgahnimDefeated", }],
        ["Eastern Palace - Cannonball Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", }],
        ["Eastern Palace - Cannonball Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Eastern Palace - Cannonball Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Eastern Palace - Cannonball Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Eastern Palace - Cannonball Chest", true, new string[] { "MoonPearl", "TitansMitt" }],

        ["Eastern Palace - Big Chest", false, new string[] {  }],
        ["Eastern Palace - Big Chest", false, new string[] { "BigKeyP1", "AgahnimDefeated" }],
        ["Eastern Palace - Big Chest", false, new string[] { "MoonPearl", "AgahnimDefeated" }],
        ["Eastern Palace - Big Chest", true, new string[] { "BigKeyP1", "MoonPearl", "AgahnimDefeated" }],
        ["Eastern Palace - Big Chest", true, new string[] { "BigKeyP1", "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Eastern Palace - Big Chest", true, new string[] { "BigKeyP1", "MoonPearl", "PowerGlove", "Hammer" }],
        ["Eastern Palace - Big Chest", true, new string[] { "BigKeyP1", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Eastern Palace - Big Chest", true, new string[] { "BigKeyP1", "MoonPearl", "TitansMitt" }],

        ["Eastern Palace - Map Chest", false, new string[] {  }],
        ["Eastern Palace - Map Chest", false, new string[] { "AgahnimDefeated" }],
        ["Eastern Palace - Map Chest", true, new string[] { "MoonPearl", "AgahnimDefeated" }],
        ["Eastern Palace - Map Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Eastern Palace - Map Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Eastern Palace - Map Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Eastern Palace - Map Chest", true, new string[] { "MoonPearl", "TitansMitt" }],

        ["Eastern Palace - Big Key Chest", false, new string[] {  }],
        ["Eastern Palace - Big Key Chest", false, new string[] { "Lamp", "AgahnimDefeated" }],
        ["Eastern Palace - Big Key Chest", false, new string[] { "MoonPearl", "AgahnimDefeated" }],
        ["Eastern Palace - Big Key Chest", true, new string[] { "Lamp", "MoonPearl", "AgahnimDefeated" }],
        ["Eastern Palace - Big Key Chest", true, new string[] { "Lamp", "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Eastern Palace - Big Key Chest", true, new string[] { "Lamp", "MoonPearl", "PowerGlove", "Hammer" }],
        ["Eastern Palace - Big Key Chest", true, new string[] { "Lamp", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Eastern Palace - Big Key Chest", true, new string[] { "Lamp", "MoonPearl", "TitansMitt" }],

        ["Eastern Palace - Boss", false, new string[] {  }],
        ["Eastern Palace - Boss", false, new string[] { "Lamp", "BowAndArrows", "BigKeyP1", "AgahnimDefeated" }],
        ["Eastern Palace - Boss", false, new string[] { "BowAndArrows", "BigKeyP1", "MoonPearl", "AgahnimDefeated" }],
        ["Eastern Palace - Boss", false, new string[] { "Lamp", "BigKeyP1", "MoonPearl", "AgahnimDefeated" }],
        ["Eastern Palace - Boss", false, new string[] { "Lamp", "BowAndArrows", "MoonPearl", "AgahnimDefeated" }],
        ["Eastern Palace - Boss", true, new string[] { "Lamp", "BowAndArrows", "BigKeyP1", "MoonPearl", "AgahnimDefeated" }],
        ["Eastern Palace - Boss", true, new string[] { "Lamp", "BowAndArrows", "BigKeyP1", "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Eastern Palace - Boss", true, new string[] { "Lamp", "BowAndArrows", "BigKeyP1", "MoonPearl", "PowerGlove", "Hammer" }],
        ["Eastern Palace - Boss", true, new string[] { "Lamp", "BowAndArrows", "BigKeyP1", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Eastern Palace - Boss", true, new string[] { "Lamp", "BowAndArrows", "BigKeyP1", "MoonPearl", "TitansMitt" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
