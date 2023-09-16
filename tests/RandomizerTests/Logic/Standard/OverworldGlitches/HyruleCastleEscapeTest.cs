namespace RandomizerTests.Logic.Standard.OverworldGlitches;

[TestClass]
public sealed class HyruleCastleEscapeTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Sanctuary", true, new string[] { "UncleSword", "KeyH2" } },

        new object[] { "Sewers - Secret Room - Left", true, new string[] { "UncleSword", "KeyH2" } },

        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "UncleSword", "KeyH2" } },

        new object[] { "Sewers - Secret Room - Right", true, new string[] { "UncleSword", "KeyH2" } },

        new object[] { "Sewers - Dark Cross", true, new string[] { "UncleSword" } },

        new object[] { "Hyrule Castle - Boomerang Chest", true, new string[] { "UncleSword" } },

        new object[] { "Hyrule Castle - Map Chest", true, new string[] { "UncleSword" } },

        new object[] { "Hyrule Castle - Zelda's Cell", true, new string[] { "UncleSword" } },
    };
}
