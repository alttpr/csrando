namespace RandomizerTests.Logic.Inverted.OverworldGlitches;

[TestClass]
public sealed class EasternPalaceTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Eastern Palace - Compass Chest", false, new string[] {  }],
        ["Eastern Palace - Compass Chest", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Eastern Palace - Compass Chest", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Eastern Palace - Compass Chest", true, new string[] { "DefeatAgahnim" }],

        ["Eastern Palace - Cannonball Chest", false, new string[] {  }],
        ["Eastern Palace - Cannonball Chest", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Eastern Palace - Cannonball Chest", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Eastern Palace - Cannonball Chest", true, new string[] { "DefeatAgahnim" }],

        ["Eastern Palace - Big Chest", false, new string[] {  }],
        ["Eastern Palace - Big Chest", true, new string[] { "BigKeyP1", "MoonPearl", "PegasusBoots" }],
        ["Eastern Palace - Big Chest", true, new string[] { "BigKeyP1", "MagicMirror", "PegasusBoots" }],
        ["Eastern Palace - Big Chest", true, new string[] { "BigKeyP1", "DefeatAgahnim" }],

        ["Eastern Palace - Map Chest", false, new string[] {  }],
        ["Eastern Palace - Map Chest", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Eastern Palace - Map Chest", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Eastern Palace - Map Chest", true, new string[] { "DefeatAgahnim" }],

        ["Eastern Palace - Big Key Chest", false, new string[] {  }],
        ["Eastern Palace - Big Key Chest", true, new string[] { "Lamp", "MoonPearl", "PegasusBoots" }],
        ["Eastern Palace - Big Key Chest", true, new string[] { "Lamp", "MagicMirror", "PegasusBoots" }],
        ["Eastern Palace - Big Key Chest", true, new string[] { "Lamp", "DefeatAgahnim" }],

        ["Eastern Palace - Boss", false, new string[] {  }],
        ["Eastern Palace - Boss", true, new string[] { "Lamp", "BowAndArrows", "BigKeyP1", "MoonPearl", "PegasusBoots" }],
        ["Eastern Palace - Boss", true, new string[] { "Lamp", "BowAndArrows", "BigKeyP1", "MagicMirror", "PegasusBoots" }],
        ["Eastern Palace - Boss", true, new string[] { "Lamp", "BowAndArrows", "BigKeyP1", "DefeatAgahnim" }],
    ];
}
