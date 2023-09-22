namespace RandomizerTests.Logic.Open.OverworldGlitches;

[TestClass]
public sealed class EscapeTest : OpenOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Sanctuary", true, new string[] {  } },

        new object[] { "Sewers - Secret Room - Left", false, new string[] {  } },
        new object[] { "Sewers - Secret Room - Left", true, new string[] { "ProgressiveGlove" } },
        new object[] { "Sewers - Secret Room - Left", true, new string[] { "PowerGlove" } },
        new object[] { "Sewers - Secret Room - Left", true, new string[] { "TitansMitt" } },
        new object[] { "Sewers - Secret Room - Left", true, new string[] { "Lamp", "KeyH2" } },

        new object[] { "Sewers - Secret Room - Middle", false, new string[] {  } },
        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "ProgressiveGlove" } },
        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "PowerGlove" } },
        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "TitansMitt" } },
        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "Lamp", "KeyH2" } },

        new object[] { "Sewers - Secret Room - Right", false, new string[] {  } },
        new object[] { "Sewers - Secret Room - Right", true, new string[] { "ProgressiveGlove" } },
        new object[] { "Sewers - Secret Room - Right", true, new string[] { "PowerGlove" } },
        new object[] { "Sewers - Secret Room - Right", true, new string[] { "TitansMitt" } },
        new object[] { "Sewers - Secret Room - Right", true, new string[] { "Lamp", "KeyH2" } },

        new object[] { "Sewers - Dark Cross", true, new string[] { "Lamp" } },

        new object[] { "Hyrule Castle - Boomerang Chest", false, new string[] {  } },
        new object[] { "Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2" } },

        new object[] { "Hyrule Castle - Map Chest", true, new string[] {  } },

        new object[] { "Hyrule Castle - Zelda's Cell", false, new string[] {  } },
        new object[] { "Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2" } },

        new object[] { "Link's Uncle", true, new string[] {  } },

        new object[] { "Secret Passage", true, new string[] {  } },
    };
}
