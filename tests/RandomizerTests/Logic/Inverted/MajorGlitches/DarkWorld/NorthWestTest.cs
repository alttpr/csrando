namespace RandomizerTests.Logic.Inverted.MajorGlitches.DarkWorld;

[TestClass]
public sealed class NorthWestTest : InvertedMajorGlitchesLogicTests
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
        ["Hammer Pegs Item", true, new string[] { "Hammer" }],

        ["Bumper Cave Item", true, new string[] {  }],

        ["Blacksmith Item", false, new string[] {  }],
        ["Blacksmith Item", true, new string[] { "MagicMirror" }],
        ["Blacksmith Item", true, new string[] { "Bottle" }],
        ["Blacksmith Item", true, new string[] { "ProgressiveGlove", "ProgressiveGlove" }],

        ["Purple Chest Item", false, new string[] {  }],
        ["Purple Chest Item", true, new string[] { "MagicMirror" }],
        ["Purple Chest Item", true, new string[] { "Bottle" }],
        ["Purple Chest Item", true, new string[] { "ProgressiveGlove", "ProgressiveGlove" }],
    ];
}
