namespace RandomizerTests.Logic.Open.NoGlitches.DarkWorld;

[TestClass]
public class NorthWestTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Brewery Item", false, new string[] {  } },
        new object[] { "Brewery Item", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Brewery Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Brewery Item", true, new string[] { "MoonPearl", "TitansMitt" } },
        new object[] { "Brewery Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Brewery Item", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Brewery Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Brewery Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" } },
        new object[] { "Brewery Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" } },

        new object[] { "C-Shaped House Item", false, new string[] {  } },
        new object[] { "C-Shaped House Item", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "C-Shaped House Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "C-Shaped House Item", true, new string[] { "MoonPearl", "TitansMitt" } },
        new object[] { "C-Shaped House Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "C-Shaped House Item", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "C-Shaped House Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "C-Shaped House Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" } },
        new object[] { "C-Shaped House Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" } },

        new object[] { "Chest Game", false, new string[] {  } },
        new object[] { "Chest Game", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Chest Game", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Chest Game", true, new string[] { "MoonPearl", "TitansMitt" } },
        new object[] { "Chest Game", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Chest Game", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Chest Game", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Chest Game", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" } },
        new object[] { "Chest Game", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" } },

        new object[] { "Hammer Pegs Item", false, new string[] {  } },
        new object[] { "Hammer Pegs Item", false, new string[] { "Hammer", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Hammer Pegs Item", false, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Hammer Pegs Item", false, new string[] { "MoonPearl", "Hammer", "ProgressiveGlove" } },
        new object[] { "Hammer Pegs Item", true, new string[] { "MoonPearl", "Hammer", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Hammer Pegs Item", true, new string[] { "MoonPearl", "Hammer", "TitansMitt" } },

        new object[] { "Bumper Cave Item", false, new string[] {  } },
        new object[] { "Bumper Cave Item", false, new string[] { "Cape", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Bumper Cave Item", false, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Bumper Cave Item", false, new string[] { "MoonPearl", "Cape", "ProgressiveGlove" } },
        new object[] { "Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "TitansMitt" } },
        new object[] { "Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "ProgressiveGlove", "Hammer" } },
        new object[] { "Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "PowerGlove", "Hammer" } },
        new object[] { "Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "AgahnimDefeated", "PowerGlove", "Hookshot" } },

        new object[] { "Blacksmith Item", false, new string[] {  } },
        new object[] { "Blacksmith Item", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Blacksmith Item", false, new string[] { "MoonPearl", "ProgressiveGlove" } },
        new object[] { "Blacksmith Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Blacksmith Item", true, new string[] { "MoonPearl", "TitansMitt" } },

        new object[] { "Purple Chest Item", false, new string[] {  } },
        new object[] { "Purple Chest Item", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Purple Chest Item", false, new string[] { "MoonPearl", "ProgressiveGlove" } },
        new object[] { "Purple Chest Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Purple Chest Item", true, new string[] { "MoonPearl", "TitansMitt" } },
    };

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
