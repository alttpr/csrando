namespace RandomizerTests.Logic.Inverted.NoGlitches.DarkWorld;

[TestClass]
public class NorthWestTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Brewery", true, new string[] {  } },

        new object[] { "C-Shaped House", true, new string[] {  } },

        new object[] { "Chest Game", true, new string[] {  } },

        new object[] { "Hammer Pegs Item", false, new string[] {  } },
        new object[] { "Hammer Pegs Item", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Hammer Pegs Item", true, new string[] { "Hammer", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Hammer Pegs Item", true, new string[] { "Hammer", "TitansMitt" } },
        new object[] { "Hammer Pegs Item", true, new string[] { "Hammer", "ProgressiveGlove", "MagicMirror", "MoonPearl" } },
        new object[] { "Hammer Pegs Item", true, new string[] { "Hammer", "AgahnimDefeated", "MagicMirror" } },

        new object[] { "Bumper Cave Item", false, new string[] {  } },
        new object[] { "Bumper Cave Item", false, new string[] { "Cape", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Bumper Cave Item", false, new string[] { "MoonPearl", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Bumper Cave Item", false, new string[] { "MoonPearl", "Cape", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Bumper Cave Item", false, new string[] { "MoonPearl", "Cape", "MagicMirror", "Hammer" } },
        new object[] { "Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "MagicMirror", "ProgressiveGlove", "Hammer" } },
        new object[] { "Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "MagicMirror", "TitansMitt" } },
        new object[] { "Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "MagicMirror", "PowerGlove", "Hammer" } },
        new object[] { "Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "MagicMirror", "AgahnimDefeated", "ProgressiveGlove" } },
        new object[] { "Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "MagicMirror", "AgahnimDefeated", "PowerGlove" } },

        new object[] { "Blacksmith", false, new string[] {  } },
        new object[] { "Blacksmith", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl" } },
        new object[] { "Blacksmith", true, new string[] { "TitansMitt", "MoonPearl" } },
        new object[] { "Blacksmith", true, new string[] { "AgahnimDefeated", "MagicMirror" } },
        new object[] { "Blacksmith", true, new string[] { "ProgressiveGlove", "Hammer", "MagicMirror", "MoonPearl" } },

        new object[] { "Purple Chest", false, new string[] {  } },
        new object[] { "Purple Chest", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl" } },
        new object[] { "Purple Chest", true, new string[] { "TitansMitt", "MoonPearl" } },
        new object[] { "Purple Chest", true, new string[] { "AgahnimDefeated", "MagicMirror" } },
        new object[] { "Purple Chest", true, new string[] { "ProgressiveGlove", "Hammer", "MagicMirror", "MoonPearl" } },
    };

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
