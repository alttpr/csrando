namespace RandomizerTests.Logic.Alttp.Inverted.MajorGlitches;

using RandomizerTests.Logic.Alttp;

[TestClass]
public sealed class HyruleCastleEscapeTest : InvertedMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Sanctuary", false, new string[] {  }],
        ["Sanctuary", true, new string[] { "Lamp", "KeyH2" }],
        ["Sanctuary", true, new string[] { "MoonPearl" }],
        ["Sanctuary", true, new string[] { "MagicMirror" }],
        ["Sanctuary", true, new string[] { "BottleWithBee" }],
        ["Sanctuary", true, new string[] { "BottleWithFairy" }],
        ["Sanctuary", true, new string[] { "BottleWithRedPotion" }],
        ["Sanctuary", true, new string[] { "BottleWithGreenPotion" }],
        ["Sanctuary", true, new string[] { "BottleWithBluePotion" }],
        ["Sanctuary", true, new string[] { "Bottle" }],
        ["Sanctuary", true, new string[] { "BottleWithGoldBee" }],

        ["Sewers - Secret Room - Left", false, new string[] {  }],
        ["Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "PowerGlove" }],
        ["Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "TitansMitt" }],
        ["Sewers - Secret Room - Left", true, new string[] { "BottleWithBee", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Left", true, new string[] { "BottleWithBee", "PowerGlove" }],
        ["Sewers - Secret Room - Left", true, new string[] { "BottleWithBee", "TitansMitt" }],
        ["Sewers - Secret Room - Left", true, new string[] { "BottleWithFairy", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Left", true, new string[] { "BottleWithFairy", "PowerGlove" }],
        ["Sewers - Secret Room - Left", true, new string[] { "BottleWithFairy", "TitansMitt" }],
        ["Sewers - Secret Room - Left", true, new string[] { "BottleWithRedPotion", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Left", true, new string[] { "BottleWithRedPotion", "PowerGlove" }],
        ["Sewers - Secret Room - Left", true, new string[] { "BottleWithRedPotion", "TitansMitt" }],
        ["Sewers - Secret Room - Left", true, new string[] { "BottleWithGreenPotion", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Left", true, new string[] { "BottleWithGreenPotion", "PowerGlove" }],
        ["Sewers - Secret Room - Left", true, new string[] { "BottleWithGreenPotion", "TitansMitt" }],
        ["Sewers - Secret Room - Left", true, new string[] { "BottleWithBluePotion", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Left", true, new string[] { "BottleWithBluePotion", "PowerGlove" }],
        ["Sewers - Secret Room - Left", true, new string[] { "BottleWithBluePotion", "TitansMitt" }],
        ["Sewers - Secret Room - Left", true, new string[] { "Bottle", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Left", true, new string[] { "Bottle", "PowerGlove" }],
        ["Sewers - Secret Room - Left", true, new string[] { "Bottle", "TitansMitt" }],
        ["Sewers - Secret Room - Left", true, new string[] { "BottleWithGoldBee", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Left", true, new string[] { "BottleWithGoldBee", "PowerGlove" }],
        ["Sewers - Secret Room - Left", true, new string[] { "BottleWithGoldBee", "TitansMitt" }],
        ["Sewers - Secret Room - Left", true, new string[] { "Lamp", "KeyH2" }],

        ["Sewers - Secret Room - Middle", false, new string[] {  }],
        ["Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "PowerGlove" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "TitansMitt" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "BottleWithBee", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "BottleWithBee", "PowerGlove" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "BottleWithBee", "TitansMitt" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "BottleWithFairy", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "BottleWithFairy", "PowerGlove" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "BottleWithFairy", "TitansMitt" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "BottleWithRedPotion", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "BottleWithRedPotion", "PowerGlove" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "BottleWithRedPotion", "TitansMitt" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "BottleWithGreenPotion", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "BottleWithGreenPotion", "PowerGlove" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "BottleWithGreenPotion", "TitansMitt" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "BottleWithBluePotion", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "BottleWithBluePotion", "PowerGlove" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "BottleWithBluePotion", "TitansMitt" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "Bottle", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "Bottle", "PowerGlove" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "Bottle", "TitansMitt" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "BottleWithGoldBee", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "BottleWithGoldBee", "PowerGlove" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "BottleWithGoldBee", "TitansMitt" }],
        ["Sewers - Secret Room - Middle", true, new string[] { "Lamp", "KeyH2" }],

        ["Sewers - Secret Room - Right", false, new string[] {  }],
        ["Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "PowerGlove" }],
        ["Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "TitansMitt" }],
        ["Sewers - Secret Room - Right", true, new string[] { "BottleWithBee", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Right", true, new string[] { "BottleWithBee", "PowerGlove" }],
        ["Sewers - Secret Room - Right", true, new string[] { "BottleWithBee", "TitansMitt" }],
        ["Sewers - Secret Room - Right", true, new string[] { "BottleWithFairy", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Right", true, new string[] { "BottleWithFairy", "PowerGlove" }],
        ["Sewers - Secret Room - Right", true, new string[] { "BottleWithFairy", "TitansMitt" }],
        ["Sewers - Secret Room - Right", true, new string[] { "BottleWithRedPotion", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Right", true, new string[] { "BottleWithRedPotion", "PowerGlove" }],
        ["Sewers - Secret Room - Right", true, new string[] { "BottleWithRedPotion", "TitansMitt" }],
        ["Sewers - Secret Room - Right", true, new string[] { "BottleWithGreenPotion", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Right", true, new string[] { "BottleWithGreenPotion", "PowerGlove" }],
        ["Sewers - Secret Room - Right", true, new string[] { "BottleWithGreenPotion", "TitansMitt" }],
        ["Sewers - Secret Room - Right", true, new string[] { "BottleWithBluePotion", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Right", true, new string[] { "BottleWithBluePotion", "PowerGlove" }],
        ["Sewers - Secret Room - Right", true, new string[] { "BottleWithBluePotion", "TitansMitt" }],
        ["Sewers - Secret Room - Right", true, new string[] { "Bottle", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Right", true, new string[] { "Bottle", "PowerGlove" }],
        ["Sewers - Secret Room - Right", true, new string[] { "Bottle", "TitansMitt" }],
        ["Sewers - Secret Room - Right", true, new string[] { "BottleWithGoldBee", "ProgressiveGlove" }],
        ["Sewers - Secret Room - Right", true, new string[] { "BottleWithGoldBee", "PowerGlove" }],
        ["Sewers - Secret Room - Right", true, new string[] { "BottleWithGoldBee", "TitansMitt" }],
        ["Sewers - Secret Room - Right", true, new string[] { "Lamp", "KeyH2" }],

        ["Sewers - Dark Cross", false, new string[] {  }],
        ["Sewers - Dark Cross", true, new string[] { "Lamp" }],

        ["Hyrule Castle - Boomerang Chest", false, new string[] {  }],
        ["Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2" }],

        ["Hyrule Castle - Map Chest", true, new string[] {  }],
        ["Hyrule Castle - Map Chest", true, new string[] { "DefeatAgahnim" }],

        ["Hyrule Castle - Zelda's Cell", false, new string[] {  }],
        ["Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2" }],
    ];
}
