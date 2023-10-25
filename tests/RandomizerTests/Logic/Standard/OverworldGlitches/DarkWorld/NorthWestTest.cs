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
        new object[] { "Brewery Item", false, new string[] {  } },
        new object[] { "Brewery Item", false, new string[] { "PegasusBoots" } },
        new object[] { "Brewery Item", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "C-Shaped House Item", false, new string[] {  } },
        new object[] { "C-Shaped House Item", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "C-Shaped House Item", true, new string[] { "MagicMirror", "PegasusBoots" } },

        new object[] { "Chest Game", false, new string[] {  } },
        new object[] { "Chest Game", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Chest Game", true, new string[] { "MagicMirror", "PegasusBoots" } },

        new object[] { "Hammer Pegs Item", false, new string[] {  } },
        new object[] { "Hammer Pegs Item", false, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Hammer Pegs Item", false, new string[] { "Hammer", "PegasusBoots" } },
        new object[] { "Hammer Pegs Item", true, new string[] { "MoonPearl", "Hammer", "PegasusBoots" } },

        new object[] { "Bumper Cave Item", false, new string[] {  } },
        new object[] { "Bumper Cave Item", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Blacksmith Item", false, new string[] {  } },
        new object[] { "Blacksmith Item", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Blacksmith Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Blacksmith Item", true, new string[] { "MoonPearl", "TitansMitt" } },

        new object[] { "Purple Chest Item", false, new string[] {  } },
        new object[] { "Purple Chest Item", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Purple Chest Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Purple Chest Item", true, new string[] { "MoonPearl", "TitansMitt" } },
    };
}
