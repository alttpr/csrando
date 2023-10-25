namespace RandomizerTests.Logic.Inverted.OverworldGlitches.DarkWorld;

[TestClass]
public sealed class NorthWestTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Brewery Item", true, new string[] {  } },

        new object[] { "C-Shaped House Item", true, new string[] {  } },

        new object[] { "Chest Game", true, new string[] {  } },

        new object[] { "Hammer Pegs Item", false, new string[] {  } },
        new object[] { "Hammer Pegs Item", true, new string[] { "Hammer", "PegasusBoots" } },

        new object[] { "Bumper Cave Item", false, new string[] {  } },
        new object[] { "Bumper Cave Item", true, new string[] { "PegasusBoots" } },

        new object[] { "Blacksmith Item", false, new string[] {  } },
        new object[] { "Blacksmith Item", true, new string[] { "MagicMirror", "PegasusBoots" } },
        new object[] { "Blacksmith Item", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "PegasusBoots", "MoonPearl" } },

        new object[] { "Purple Chest Item", false, new string[] {  } },
        new object[] { "Purple Chest Item", true, new string[] { "MagicMirror", "PegasusBoots" } },
        new object[] { "Purple Chest Item", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "PegasusBoots", "MoonPearl" } },
    };
}
