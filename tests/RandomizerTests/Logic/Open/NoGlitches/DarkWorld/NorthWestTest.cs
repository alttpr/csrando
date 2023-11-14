namespace RandomizerTests.Logic.Open.NoGlitches.DarkWorld;

[TestClass]
public class NorthWestTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Brewery Item", false, new string[] {  }],
        ["Brewery Item", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" }],
        ["Brewery Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Brewery Item", true, new string[] { "MoonPearl", "TitansMitt" }],
        ["Brewery Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Brewery Item", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Brewery Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" }],
        ["Brewery Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" }],
        ["Brewery Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" }],

        ["C-Shaped House Item", false, new string[] {  }],
        ["C-Shaped House Item", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" }],
        ["C-Shaped House Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["C-Shaped House Item", true, new string[] { "MoonPearl", "TitansMitt" }],
        ["C-Shaped House Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["C-Shaped House Item", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["C-Shaped House Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" }],
        ["C-Shaped House Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" }],
        ["C-Shaped House Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" }],

        ["Chest Game", false, new string[] {  }],
        ["Chest Game", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" }],
        ["Chest Game", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Chest Game", true, new string[] { "MoonPearl", "TitansMitt" }],
        ["Chest Game", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Chest Game", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Chest Game", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" }],
        ["Chest Game", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" }],
        ["Chest Game", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" }],

        ["Hammer Pegs Item", false, new string[] {  }],
        ["Hammer Pegs Item", false, new string[] { "Hammer", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Hammer Pegs Item", false, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Hammer Pegs Item", false, new string[] { "MoonPearl", "Hammer", "ProgressiveGlove" }],
        ["Hammer Pegs Item", true, new string[] { "MoonPearl", "Hammer", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Hammer Pegs Item", true, new string[] { "MoonPearl", "Hammer", "TitansMitt" }],

        ["Bumper Cave Item", false, new string[] {  }],
        ["Bumper Cave Item", false, new string[] { "Cape", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Bumper Cave Item", false, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Bumper Cave Item", false, new string[] { "MoonPearl", "Cape", "ProgressiveGlove" }],
        ["Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "TitansMitt" }],
        ["Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "ProgressiveGlove", "Hammer" }],
        ["Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "PowerGlove", "Hammer" }],
        ["Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" }],
        ["Bumper Cave Item", true, new string[] { "MoonPearl", "Cape", "AgahnimDefeated", "PowerGlove", "Hookshot" }],

        ["Blacksmith Item", false, new string[] {  }],
        ["Blacksmith Item", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" }],
        ["Blacksmith Item", false, new string[] { "MoonPearl", "ProgressiveGlove" }],
        ["Blacksmith Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Blacksmith Item", true, new string[] { "MoonPearl", "TitansMitt" }],

        ["Purple Chest Item", false, new string[] {  }],
        ["Purple Chest Item", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" }],
        ["Purple Chest Item", false, new string[] { "MoonPearl", "ProgressiveGlove" }],
        ["Purple Chest Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Purple Chest Item", true, new string[] { "MoonPearl", "TitansMitt" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
