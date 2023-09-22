namespace RandomizerTests.Logic.Standard.OverworldGlitches.DarkWorld;

[TestClass]
public sealed class NorthWestTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Brewery", false, new string[] {  } },
        new object[] { "Brewery", false, new string[] { "PegasusBoots" } },
        new object[] { "Brewery", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "C-Shaped House", false, new string[] {  } },
        new object[] { "C-Shaped House", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "C-Shaped House", true, new string[] { "MagicMirror", "PegasusBoots" } },

        new object[] { "Chest Game", false, new string[] {  } },
        new object[] { "Chest Game", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Chest Game", true, new string[] { "MagicMirror", "PegasusBoots" } },

        new object[] { "Hammer Pegs", false, new string[] {  } },
        new object[] { "Hammer Pegs", false, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Hammer Pegs", false, new string[] { "Hammer", "PegasusBoots" } },
        new object[] { "Hammer Pegs", true, new string[] { "MoonPearl", "Hammer", "PegasusBoots" } },

        new object[] { "Bumper Cave", false, new string[] {  } },
        new object[] { "Bumper Cave", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Blacksmith", false, new string[] {  } },
        new object[] { "Blacksmith", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Blacksmith", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Blacksmith", true, new string[] { "MoonPearl", "TitansMitt" } },

        new object[] { "Purple Chest", false, new string[] {  } },
        new object[] { "Purple Chest", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Purple Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Purple Chest", true, new string[] { "MoonPearl", "TitansMitt" } },
    };
}
