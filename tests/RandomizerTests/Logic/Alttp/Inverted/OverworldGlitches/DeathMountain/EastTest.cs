namespace RandomizerTests.Logic.Alttp.Inverted.OverworldGlitches.DeathMountain;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Inverted.OverworldGlitches;

[TestClass]
public sealed class EastTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Spiral Cave Item", false, new string[] {  }],
        ["Spiral Cave Item", true, new string[] { "MagicMirror", "ProgressiveGlove", "ProgressiveGlove", "Lamp", "UncleSword" }],
        ["Spiral Cave Item", true, new string[] { "MagicMirror", "TitansMitt", "Lamp", "UncleSword" }],
        ["Spiral Cave Item", true, new string[] { "MagicMirror", "ProgressiveGlove", "ProgressiveGlove", "PegasusBoots", "UncleSword" }],
        ["Spiral Cave Item", true, new string[] { "MagicMirror", "TitansMitt", "PegasusBoots", "UncleSword" }],
        ["Spiral Cave Item", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Paradox Cave Lower - Far Left", false, new string[] {  }],
        ["Paradox Cave Lower - Far Left", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Paradox Cave Lower - Left", false, new string[] {  }],
        ["Paradox Cave Lower - Left", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Paradox Cave Lower - Middle", false, new string[] {  }],
        ["Paradox Cave Lower - Middle", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Paradox Cave Lower - Right", false, new string[] {  }],
        ["Paradox Cave Lower - Right", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Paradox Cave Lower - Far Right", false, new string[] {  }],
        ["Paradox Cave Lower - Far Right", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Paradox Cave Upper - Left", false, new string[] {  }],
        ["Paradox Cave Upper - Left", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Paradox Cave Upper - Right", false, new string[] {  }],
        ["Paradox Cave Upper - Right", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Mimic Cave", false, new string[] {  }],
        ["Mimic Cave", true, new string[] { "MoonPearl", "Hammer", "PegasusBoots" }],

        ["Ether Tablet", false, new string[] {  }],
        ["Ether Tablet", true, new string[] { "PegasusBoots", "MoonPearl", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" }],
        ["Ether Tablet", true, new string[] { "PegasusBoots", "MoonPearl", "BookOfMudora", "L2Sword" }],
        ["Ether Tablet", true, new string[] { "PegasusBoots", "MoonPearl", "BookOfMudora", "L3Sword" }],
        ["Ether Tablet", true, new string[] { "PegasusBoots", "MoonPearl", "BookOfMudora", "L4Sword" }],

        ["Spectacle Rock", false, new string[] {  }],
        ["Spectacle Rock", true, new string[] { "MoonPearl", "PegasusBoots" }],
    ];
}
