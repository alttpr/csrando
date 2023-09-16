namespace RandomizerTests.Logic.Inverted.OverworldGlitches;

[TestClass]
public sealed class HyruleCastleEscapeTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Sanctuary", false, new string[] {  } },
        new object[] { "Sanctuary", true, new string[] { "MagicMirror", "DefeatAgahnim" } },
        new object[] { "Sanctuary", true, new string[] { "Lamp", "DefeatAgahnim", "KeyH2" } },
        new object[] { "Sanctuary", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Sanctuary", true, new string[] { "MagicMirror", "PegasusBoots" } },

        new object[] { "Sewers - Secret Room - Left", false, new string[] {  } },
        new object[] { "Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "ProgressiveGlove", "PegasusBoots" } },
        new object[] { "Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "PegasusBoots", "Lamp", "KeyH2" } },
        new object[] { "Sewers - Secret Room - Left", true, new string[] { "MagicMirror", "PegasusBoots", "Lamp", "KeyH2" } },
        new object[] { "Sewers - Secret Room - Left", true, new string[] { "DefeatAgahnim", "Lamp", "KeyH2" } },

        new object[] { "Sewers - Secret Room - Middle", false, new string[] {  } },
        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "ProgressiveGlove", "PegasusBoots" } },
        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "PegasusBoots", "Lamp", "KeyH2" } },
        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "MagicMirror", "PegasusBoots", "Lamp", "KeyH2" } },
        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "DefeatAgahnim", "Lamp", "KeyH2" } },

        new object[] { "Sewers - Secret Room - Right", false, new string[] {  } },
        new object[] { "Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "PegasusBoots" } },
        new object[] { "Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "PegasusBoots", "Lamp", "KeyH2" } },
        new object[] { "Sewers - Secret Room - Right", true, new string[] { "MagicMirror", "PegasusBoots", "Lamp", "KeyH2" } },
        new object[] { "Sewers - Secret Room - Right", true, new string[] { "DefeatAgahnim", "Lamp", "KeyH2" } },

        new object[] { "Sewers - Dark Cross", false, new string[] {  } },
        new object[] { "Sewers - Dark Cross", true, new string[] { "Lamp", "DefeatAgahnim" } },
        new object[] { "Sewers - Dark Cross", true, new string[] { "Lamp", "MoonPearl", "PegasusBoots" } },
        new object[] { "Sewers - Dark Cross", true, new string[] { "Lamp", "MagicMirror", "PegasusBoots" } },

        new object[] { "Hyrule Castle - Boomerang Chest", false, new string[] {  } },
        new object[] { "Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2", "DefeatAgahnim" } },
        new object[] { "Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2", "MoonPearl", "PegasusBoots" } },
        new object[] { "Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2", "MagicMirror", "PegasusBoots" } },

        new object[] { "Hyrule Castle - Map Chest", false, new string[] {  } },
        new object[] { "Hyrule Castle - Map Chest", true, new string[] { "DefeatAgahnim" } },
        new object[] { "Hyrule Castle - Map Chest", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Hyrule Castle - Map Chest", true, new string[] { "MagicMirror", "PegasusBoots" } },

        new object[] { "Hyrule Castle - Zelda's Cell", false, new string[] {  } },
        new object[] { "Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2", "DefeatAgahnim" } },
        new object[] { "Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2", "MoonPearl", "PegasusBoots" } },
        new object[] { "Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2", "MagicMirror", "PegasusBoots" } },
    };
}
