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

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Brewery", true, new string[] {  } },

        new object[] { "C-Shaped House", true, new string[] {  } },

        new object[] { "Chest Game", true, new string[] {  } },

        new object[] { "Hammer Pegs", false, new string[] {  } },
        new object[] { "Hammer Pegs", true, new string[] { "Hammer" } },

        new object[] { "Bumper Cave", true, new string[] {  } },

        new object[] { "Blacksmith", false, new string[] {  } },
        new object[] { "Blacksmith", true, new string[] { "MagicMirror" } },
        new object[] { "Blacksmith", true, new string[] { "Bottle" } },
        new object[] { "Blacksmith", true, new string[] { "ProgressiveGlove", "ProgressiveGlove" } },

        new object[] { "Purple Chest", false, new string[] {  } },
        new object[] { "Purple Chest", true, new string[] { "MagicMirror" } },
        new object[] { "Purple Chest", true, new string[] { "Bottle" } },
        new object[] { "Purple Chest", true, new string[] { "ProgressiveGlove", "ProgressiveGlove" } },
    };
}
