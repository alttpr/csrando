namespace RandomizerTests.Logic.Open.OverworldGlitches;

[TestClass]
public sealed class LightWorldTest : OpenOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
            // @todo figure out where the boots clip drops one off?
            // new object[] { "Magic Bat Item", false, new string[] {  } },
            // new object[] { "Magic Bat Item", true, new string[] { "Powder", "PegasusBoots" } },

        new object[] { "Hobo", true, new string[] {  } },

        new object[] { "Bombos Tablet", false, new string[] {  } },
        new object[] { "Bombos Tablet", true, new string[] { "PegasusBoots", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },

        new object[] { "King Zora", true, new string[] {  } },


        new object[] { "Cave 45 Item", false, new string[] {  } },
        new object[] { "Cave 45 Item", true, new string[] { "PegasusBoots" } },

        new object[] { "Graveyard Cave Item", false, new string[] {  } },
        new object[] { "Graveyard Cave Item", true, new string[] { "PegasusBoots" } },

        new object[] { "Checkerboard Cave Item", false, new string[] {  } },
        new object[] { "Checkerboard Cave Item", true, new string[] { "PegasusBoots", "ProgressiveGlove" } },
        new object[] { "Checkerboard Cave Item", true, new string[] { "PegasusBoots", "PowerGlove" } },
        new object[] { "Checkerboard Cave Item", true, new string[] { "PegasusBoots", "TitansMitt" } },

        new object[] { "Maze Race", true, new string[] {  } },

        new object[] { "Desert Ledge - Item", false, new string[] {  } },
        new object[] { "Desert Ledge - Item", true, new string[] { "PegasusBoots" } },

        new object[] { "Lake Hylia Island", false, new string[] {  } },
        new object[] { "Lake Hylia Island", true, new string[] { "PegasusBoots" } },

        new object[] { "Zora's Domain Ledge Item", false, new string[] {  } },
        new object[] { "Zora's Domain Ledge Item", true, new string[] { "PegasusBoots" } },

        new object[] { "Waterfall Fairy - Left", false, new string[] {  } },
        new object[] { "Waterfall Fairy - Left", true, new string[] { "Flippers" } },
        new object[] { "Waterfall Fairy - Left", true, new string[] { "MoonPearl" } },
        new object[] { "Waterfall Fairy - Left", true, new string[] { "PegasusBoots" } },

        new object[] { "Waterfall Fairy - Right", false, new string[] {  } },
        new object[] { "Waterfall Fairy - Right", true, new string[] { "Flippers" } },
        new object[] { "Waterfall Fairy - Right", true, new string[] { "MoonPearl" } },
        new object[] { "Waterfall Fairy - Right", true, new string[] { "PegasusBoots" } },
    };
}
