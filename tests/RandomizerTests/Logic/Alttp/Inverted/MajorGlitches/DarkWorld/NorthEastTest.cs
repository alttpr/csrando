namespace RandomizerTests.Logic.Alttp.Inverted.MajorGlitches.DarkWorld;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Inverted.MajorGlitches;

[TestClass]
public sealed class NorthEastTest : InvertedMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Catfish", true, new string[] {  }],

        ["Pyramid", true, new string[] {  }],

        ["Pyramid Fairy - Left", false, new string[] {  }],
        ["Pyramid Fairy - Left", true, new string[] { "BigRedBomb", "MagicMirror" }],

        ["Pyramid Fairy - Right", false, new string[] {  }],
        ["Pyramid Fairy - Right", true, new string[] { "BigRedBomb", "MagicMirror" }],
    ];
}
