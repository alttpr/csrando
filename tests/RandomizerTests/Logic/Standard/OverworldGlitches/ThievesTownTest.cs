namespace RandomizerTests.Logic.Standard.OverworldGlitches;

[TestClass]
public sealed class ThievesTownTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Thieves' Town - Attic", false, new string[] {  }],
        ["Thieves' Town - Attic", true, new string[] { "MoonPearl", "PegasusBoots", "KeyD4", "BigKeyD4" }],

        ["Thieves' Town - Big Key Chest", false, new string[] {  }],
        ["Thieves' Town - Big Key Chest", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Thieves' Town - Map Chest", false, new string[] {  }],
        ["Thieves' Town - Map Chest", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Thieves' Town - Compass Chest", false, new string[] {  }],
        ["Thieves' Town - Compass Chest", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Thieves' Town - Ambush Chest", false, new string[] {  }],
        ["Thieves' Town - Ambush Chest", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Thieves' Town - Big Chest", false, new string[] {  }],
        ["Thieves' Town - Big Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Hammer", "KeyD4", "BigKeyD4" }],

        ["Thieves' Town - Blind's Cell", false, new string[] {  }],
        ["Thieves' Town - Blind's Cell", true, new string[] { "MoonPearl", "PegasusBoots", "BigKeyD4" }],

        ["Thieves' Town - Boss", false, new string[] {  }],
        ["Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "PegasusBoots", "BigKeyD4", "ProgressiveSword" }],
        ["Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "PegasusBoots", "BigKeyD4", "UncleSword" }],
        ["Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "PegasusBoots", "BigKeyD4", "MasterSword" }],
        ["Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "PegasusBoots", "BigKeyD4", "L3Sword" }],
        ["Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "PegasusBoots", "BigKeyD4", "L4Sword" }],
        ["Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "PegasusBoots", "BigKeyD4", "CaneOfByrna" }],
        ["Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "PegasusBoots", "BigKeyD4", "CaneOfSomaria" }],
        ["Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "PegasusBoots", "BigKeyD4", "Hammer" }],
    ];
}
