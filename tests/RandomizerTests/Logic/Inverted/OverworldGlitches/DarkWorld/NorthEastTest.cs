namespace RandomizerTests.Logic.Inverted.OverworldGlitches.DarkWorld;

[TestClass]
public sealed class NorthEastTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Catfish", false, new string[] {  } },
        new object[] { "Catfish", true, new string[] { "PegasusBoots" } },

        new object[] { "Pyramid", true, new string[] {  } },

        new object[] { "Pyramid Fairy - Left", false, new string[] {  } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "BigRedBomb", "MagicMirror", "PegasusBoots" } },

        new object[] { "Pyramid Fairy - Right", false, new string[] {  } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "BigRedBomb", "MagicMirror", "PegasusBoots" } },
    };
}
