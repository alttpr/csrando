namespace RandomizerTests.Logic.Alttp.Standard.OverworldGlitches.DarkWorld;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Standard.OverworldGlitches;

[TestClass]
public sealed class NorthEastTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Catfish", false, new string[] {  }],
        ["Catfish", false, new string[] { "PegasusBoots" }],
        ["Catfish", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Pyramid", false, new string[] {  }],
        ["Pyramid", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Pyramid Fairy - Left", false, new string[] {  }],
        ["Pyramid Fairy - Left", true, new string[] { "MagicMirror", "PegasusBoots" }],

        ["Pyramid Fairy - Right", false, new string[] {  }],
        ["Pyramid Fairy - Right", true, new string[] { "MagicMirror", "PegasusBoots" }],

        ["Ganon", false, new string[] {  }],
    ];
}
