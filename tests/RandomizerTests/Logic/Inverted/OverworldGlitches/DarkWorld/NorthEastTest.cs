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

    public static IEnumerable<object[]> TestData => [
        ["Catfish", false, new string[] {  }],
        ["Catfish", true, new string[] { "PegasusBoots" }],

        ["Pyramid", true, new string[] {  }],

        ["Pyramid Fairy - Left", false, new string[] {  }],
        ["Pyramid Fairy - Left", true, new string[] { "BigRedBomb", "MagicMirror", "PegasusBoots" }],

        ["Pyramid Fairy - Right", false, new string[] {  }],
        ["Pyramid Fairy - Right", true, new string[] { "BigRedBomb", "MagicMirror", "PegasusBoots" }],
    ];
}
