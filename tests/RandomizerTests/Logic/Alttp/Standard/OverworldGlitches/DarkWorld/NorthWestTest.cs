namespace RandomizerTests.Logic.Alttp.Standard.OverworldGlitches.DarkWorld;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Standard.OverworldGlitches;

[TestClass]
public sealed class NorthWestTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Brewery Item", false, new string[] {  }],
        ["Brewery Item", false, new string[] { "PegasusBoots" }],
        ["Brewery Item", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["C-Shaped House Item", false, new string[] {  }],
        ["C-Shaped House Item", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["C-Shaped House Item", true, new string[] { "MagicMirror", "PegasusBoots" }],

        ["Chest Game", false, new string[] {  }],
        ["Chest Game", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Chest Game", true, new string[] { "MagicMirror", "PegasusBoots" }],

        ["Hammer Pegs Item", false, new string[] {  }],
        ["Hammer Pegs Item", false, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Hammer Pegs Item", false, new string[] { "Hammer", "PegasusBoots" }],
        ["Hammer Pegs Item", true, new string[] { "MoonPearl", "Hammer", "PegasusBoots" }],

        ["Bumper Cave Item", false, new string[] {  }],
        ["Bumper Cave Item", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Blacksmith Item", false, new string[] {  }],
        ["Blacksmith Item", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" }],
        ["Blacksmith Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Blacksmith Item", true, new string[] { "MoonPearl", "TitansMitt" }],

        ["Purple Chest Item", false, new string[] {  }],
        ["Purple Chest Item", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" }],
        ["Purple Chest Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Purple Chest Item", true, new string[] { "MoonPearl", "TitansMitt" }],
    ];
}
