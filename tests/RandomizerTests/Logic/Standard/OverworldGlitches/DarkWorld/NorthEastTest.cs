namespace RandomizerTests.Logic.Standard.OverworldGlitches.DarkWorld;

[TestClass]
public sealed class NorthEastTest : StandardOverworldGlitchesLogicTests
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
        new object[] { "Catfish", false, new string[] { "PegasusBoots" } },
        new object[] { "Catfish", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Pyramid", false, new string[] {  } },
        new object[] { "Pyramid", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Pyramid Fairy - Left", false, new string[] {  } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "MagicMirror", "PegasusBoots" } },

        new object[] { "Pyramid Fairy - Right", false, new string[] {  } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "MagicMirror", "PegasusBoots" } },

        new object[] { "Ganon", false, new string[] {  } },
    };
}
