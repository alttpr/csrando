namespace RandomizerTests.Logic.Inverted.MajorGlitches.DarkWorld;

[TestClass]
public sealed class NorthEastTest : InvertedMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Catfish", true, new string[] {  } },

        new object[] { "Pyramid", true, new string[] {  } },

        new object[] { "Pyramid Fairy - Left", false, new string[] {  } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "BigRedBomb", "MagicMirror" } },

        new object[] { "Pyramid Fairy - Right", false, new string[] {  } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "BigRedBomb", "MagicMirror" } },
    };
}
