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

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Thieves' Town - Attic", false, new string[] {  } },
        new object[] { "Thieves' Town - Attic", true, new string[] { "MoonPearl", "PegasusBoots", "KeyD4", "BigKeyD4" } },

        new object[] { "Thieves' Town - Big Key Chest", false, new string[] {  } },
        new object[] { "Thieves' Town - Big Key Chest", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Thieves' Town - Map Chest", false, new string[] {  } },
        new object[] { "Thieves' Town - Map Chest", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Thieves' Town - Compass Chest", false, new string[] {  } },
        new object[] { "Thieves' Town - Compass Chest", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Thieves' Town - Ambush Chest", false, new string[] {  } },
        new object[] { "Thieves' Town - Ambush Chest", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Thieves' Town - Big Chest", false, new string[] {  } },
        new object[] { "Thieves' Town - Big Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Hammer", "KeyD4", "BigKeyD4" } },

        new object[] { "Thieves' Town - Blind's Cell", false, new string[] {  } },
        new object[] { "Thieves' Town - Blind's Cell", true, new string[] { "MoonPearl", "PegasusBoots", "BigKeyD4" } },

        new object[] { "Thieves' Town - Boss", false, new string[] {  } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "PegasusBoots", "BigKeyD4", "ProgressiveSword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "PegasusBoots", "BigKeyD4", "UncleSword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "PegasusBoots", "BigKeyD4", "MasterSword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "PegasusBoots", "BigKeyD4", "L3Sword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "PegasusBoots", "BigKeyD4", "L4Sword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "PegasusBoots", "BigKeyD4", "CaneOfByrna" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "PegasusBoots", "BigKeyD4", "CaneOfSomaria" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "PegasusBoots", "BigKeyD4", "Hammer" } },
    };
}
