namespace RandomizerTests.Logic.Alttp.Open.OverworldGlitches;

using RandomizerTests.Logic.Alttp;

[TestClass]
public sealed class LightWorldTest : OpenOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
            // TODO: figure out where the boots clip drops one off?
            // new object[] { "Magic Bat Item", false, new string[] {  } },
            // new object[] { "Magic Bat Item", true, new string[] { "Powder", "PegasusBoots" } },

        ["Hobo", true, new string[] {  }],

        ["Bombos Tablet", false, new string[] {  }],
        ["Bombos Tablet", true, new string[] { "PegasusBoots", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" }],

        ["King Zora", true, new string[] {  }],


        ["Cave 45 Item", false, new string[] {  }],
        ["Cave 45 Item", true, new string[] { "PegasusBoots" }],

        ["Graveyard Cave Item", false, new string[] {  }],
        ["Graveyard Cave Item", true, new string[] { "PegasusBoots" }],

        ["Checkerboard Cave Item", false, new string[] {  }],
        ["Checkerboard Cave Item", true, new string[] { "PegasusBoots", "ProgressiveGlove" }],
        ["Checkerboard Cave Item", true, new string[] { "PegasusBoots", "PowerGlove" }],
        ["Checkerboard Cave Item", true, new string[] { "PegasusBoots", "TitansMitt" }],

        ["Maze Race", true, new string[] {  }],

        ["Desert Ledge - Item", false, new string[] {  }],
        ["Desert Ledge - Item", true, new string[] { "PegasusBoots" }],

        ["Lake Hylia Island", false, new string[] {  }],
        ["Lake Hylia Island", true, new string[] { "PegasusBoots" }],

        ["Zora's Domain Ledge Item", false, new string[] {  }],
        ["Zora's Domain Ledge Item", true, new string[] { "PegasusBoots" }],

        ["Waterfall Fairy - Left", false, new string[] {  }],
        ["Waterfall Fairy - Left", true, new string[] { "Flippers" }],
        ["Waterfall Fairy - Left", true, new string[] { "MoonPearl" }],
        ["Waterfall Fairy - Left", true, new string[] { "PegasusBoots" }],

        ["Waterfall Fairy - Right", false, new string[] {  }],
        ["Waterfall Fairy - Right", true, new string[] { "Flippers" }],
        ["Waterfall Fairy - Right", true, new string[] { "MoonPearl" }],
        ["Waterfall Fairy - Right", true, new string[] { "PegasusBoots" }],
    ];
}
