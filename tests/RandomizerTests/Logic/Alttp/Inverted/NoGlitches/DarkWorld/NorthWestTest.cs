namespace RandomizerTests.Logic.Alttp.Inverted.NoGlitches.DarkWorld;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Inverted.NoGlitches;

[TestClass]
public class NorthWestTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Brewery Item", true, new string[] {  }],

        ["C-Shaped House Item", true, new string[] {  }],

        ["Chest Game", true, new string[] {  }],

        ["Hammer Pegs Item", false, new string[] {  }],
        ["Hammer Pegs Item", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" }],
        ["Hammer Pegs Item", true, new string[] { "Hammer", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Hammer Pegs Item", true, new string[] { "Hammer", "TitansMitt" }],
        ["Hammer Pegs Item", true, new string[] { "Hammer", "ProgressiveGlove", "MagicMirror", "MoonPearl" }],
        ["Hammer Pegs Item", true, new string[] { "Hammer", "AgahnimDefeated", "MagicMirror" }],

        ["Bumper Cave Item", false, new string[] {  }],
        ["Bumper Cave Item", false, new string[] { "Cape", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Bumper Cave Item", false, new string[] { "MoonPearl", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Bumper Cave Item", false, new string[] { "MoonPearl", "Cape", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Bumper Cave Item", false, new string[] { "MoonPearl", "Cape", "MagicMirror", "Hammer" }],
        ["Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "MagicMirror", "ProgressiveGlove", "Hammer" }],
        ["Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "MagicMirror", "TitansMitt" }],
        ["Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "MagicMirror", "PowerGlove", "Hammer" }],
        ["Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "MagicMirror", "AgahnimDefeated", "ProgressiveGlove" }],
        ["Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "MagicMirror", "AgahnimDefeated", "PowerGlove" }],

        ["Blacksmith Item", false, new string[] {  }],
        ["Blacksmith Item", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl" }],
        ["Blacksmith Item", true, new string[] { "TitansMitt", "MoonPearl" }],
        ["Blacksmith Item", true, new string[] { "AgahnimDefeated", "MagicMirror" }],
        ["Blacksmith Item", true, new string[] { "ProgressiveGlove", "Hammer", "MagicMirror", "MoonPearl" }],

        ["Purple Chest Item", false, new string[] {  }],
        ["Purple Chest Item", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl" }],
        ["Purple Chest Item", true, new string[] { "TitansMitt", "MoonPearl" }],
        ["Purple Chest Item", true, new string[] { "AgahnimDefeated", "MagicMirror" }],
        ["Purple Chest Item", true, new string[] { "ProgressiveGlove", "Hammer", "MagicMirror", "MoonPearl" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
