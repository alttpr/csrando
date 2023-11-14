namespace RandomizerTests.Logic.Inverted.OverworldGlitches.DarkWorld;

[TestClass]
public sealed class NorthWestTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Brewery Item", true, new string[] {  }],

        ["C-Shaped House Item", true, new string[] {  }],

        ["Chest Game", true, new string[] {  }],

        ["Hammer Pegs Item", false, new string[] {  }],
        ["Hammer Pegs Item", true, new string[] { "Hammer", "PegasusBoots" }],

        ["Bumper Cave Item", false, new string[] {  }],
        ["Bumper Cave Item", true, new string[] { "PegasusBoots" }],

        ["Blacksmith Item", false, new string[] {  }],
        ["Blacksmith Item", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Blacksmith Item", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "PegasusBoots", "MoonPearl" }],

        ["Purple Chest Item", false, new string[] {  }],
        ["Purple Chest Item", true, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Purple Chest Item", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "PegasusBoots", "MoonPearl" }],
    ];
}
