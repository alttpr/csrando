using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Open.NoGlitches;

[TestClass]
public class ThievesTownTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Thieves' Town - Attic", false, new string[] {  } },
        new object[] { "Thieves' Town - Attic", true, new string[] { "MoonPearl", "TitansMitt", "KeyD4", "BigKeyD4" } },
        new object[] { "Thieves' Town - Attic", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "KeyD4", "BigKeyD4" } },
        new object[] { "Thieves' Town - Attic", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer", "KeyD4", "BigKeyD4" } },
        new object[] { "Thieves' Town - Attic", true, new string[] { "MoonPearl", "PowerGlove", "Hammer", "KeyD4", "BigKeyD4" } },
        new object[] { "Thieves' Town - Attic", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot", "KeyD4", "BigKeyD4" } },
        new object[] { "Thieves' Town - Attic", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "KeyD4", "BigKeyD4" } },
        new object[] { "Thieves' Town - Attic", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot", "KeyD4", "BigKeyD4" } },
        new object[] { "Thieves' Town - Attic", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot", "KeyD4", "BigKeyD4" } },

        new object[] { "Thieves' Town - Big Key Chest", false, new string[] {  } },
        new object[] { "Thieves' Town - Big Key Chest", true, new string[] { "MoonPearl", "TitansMitt" } },
        new object[] { "Thieves' Town - Big Key Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Thieves' Town - Big Key Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Thieves' Town - Big Key Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Thieves' Town - Big Key Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot" } },
        new object[] { "Thieves' Town - Big Key Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Thieves' Town - Big Key Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" } },
        new object[] { "Thieves' Town - Big Key Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" } },

        new object[] { "Thieves' Town - Map Chest", false, new string[] {  } },
        new object[] { "Thieves' Town - Map Chest", true, new string[] { "MoonPearl", "TitansMitt" } },
        new object[] { "Thieves' Town - Map Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Thieves' Town - Map Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Thieves' Town - Map Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Thieves' Town - Map Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot" } },
        new object[] { "Thieves' Town - Map Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Thieves' Town - Map Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" } },
        new object[] { "Thieves' Town - Map Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" } },

        new object[] { "Thieves' Town - Compass Chest", false, new string[] {  } },
        new object[] { "Thieves' Town - Compass Chest", false, new string[] { "TitansMitt" } },
        new object[] { "Thieves' Town - Compass Chest", true, new string[] { "MoonPearl", "TitansMitt" } },
        new object[] { "Thieves' Town - Compass Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Thieves' Town - Compass Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Thieves' Town - Compass Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Thieves' Town - Compass Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot" } },
        new object[] { "Thieves' Town - Compass Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Thieves' Town - Compass Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" } },
        new object[] { "Thieves' Town - Compass Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" } },

        new object[] { "Thieves' Town - Ambush Chest", false, new string[] {  } },
        new object[] { "Thieves' Town - Ambush Chest", true, new string[] { "MoonPearl", "TitansMitt" } },
        new object[] { "Thieves' Town - Ambush Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Thieves' Town - Ambush Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Thieves' Town - Ambush Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Thieves' Town - Ambush Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot" } },
        new object[] { "Thieves' Town - Ambush Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Thieves' Town - Ambush Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" } },
        new object[] { "Thieves' Town - Ambush Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" } },

        new object[] { "Thieves' Town - Big Chest", false, new string[] {  } },
        new object[] { "Thieves' Town - Big Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer", "KeyD4", "BigKeyD4" } },
        new object[] { "Thieves' Town - Big Chest", true, new string[] { "MoonPearl", "TitansMitt", "Hammer", "KeyD4", "BigKeyD4" } },
        new object[] { "Thieves' Town - Big Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer", "KeyD4", "BigKeyD4" } },
        new object[] { "Thieves' Town - Big Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot", "KeyD4", "BigKeyD4" } },
        new object[] { "Thieves' Town - Big Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hammer", "Hookshot", "KeyD4", "BigKeyD4" } },

        new object[] { "Thieves' Town - Blind's Cell", false, new string[] {  } },
        new object[] { "Thieves' Town - Blind's Cell", true, new string[] { "MoonPearl", "TitansMitt", "BigKeyD4" } },
        new object[] { "Thieves' Town - Blind's Cell", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "BigKeyD4" } },
        new object[] { "Thieves' Town - Blind's Cell", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer", "BigKeyD4" } },
        new object[] { "Thieves' Town - Blind's Cell", true, new string[] { "MoonPearl", "PowerGlove", "Hammer", "BigKeyD4" } },
        new object[] { "Thieves' Town - Blind's Cell", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot", "BigKeyD4" } },
        new object[] { "Thieves' Town - Blind's Cell", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "BigKeyD4" } },
        new object[] { "Thieves' Town - Blind's Cell", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot", "BigKeyD4" } },
        new object[] { "Thieves' Town - Blind's Cell", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot", "BigKeyD4" } },

        new object[] { "Thieves' Town - Boss", false, new string[] {  } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "PowerGlove", "Hammer", "BigKeyD4" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot", "BigKeyD4" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "ProgressiveGlove", "Hammer", "BigKeyD4" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "TitansMitt", "BigKeyD4", "ProgressiveSword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "TitansMitt", "BigKeyD4", "UncleSword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "TitansMitt", "BigKeyD4", "MasterSword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "TitansMitt", "BigKeyD4", "L3Sword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "TitansMitt", "BigKeyD4", "L4Sword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "TitansMitt", "BigKeyD4", "CaneOfByrna" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "TitansMitt", "BigKeyD4", "CaneOfSomaria" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "TitansMitt", "BigKeyD4", "Hammer" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "BigKeyD4", "ProgressiveSword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "BigKeyD4", "UncleSword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "BigKeyD4", "MasterSword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "BigKeyD4", "L3Sword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "BigKeyD4", "L4Sword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "BigKeyD4", "CaneOfByrna" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "BigKeyD4", "CaneOfSomaria" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "BigKeyD4", "ProgressiveSword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "BigKeyD4", "UncleSword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "BigKeyD4", "MasterSword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "BigKeyD4", "L3Sword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "BigKeyD4", "L4Sword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "BigKeyD4", "CaneOfByrna" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "BigKeyD4", "CaneOfSomaria" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot", "BigKeyD4", "ProgressiveSword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot", "BigKeyD4", "UncleSword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot", "BigKeyD4", "MasterSword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot", "BigKeyD4", "L3Sword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot", "BigKeyD4", "L4Sword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot", "BigKeyD4", "CaneOfByrna" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot", "BigKeyD4", "CaneOfSomaria" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot", "BigKeyD4", "ProgressiveSword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot", "BigKeyD4", "UncleSword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot", "BigKeyD4", "MasterSword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot", "BigKeyD4", "L3Sword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot", "BigKeyD4", "L4Sword" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot", "BigKeyD4", "CaneOfByrna" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot", "BigKeyD4", "CaneOfSomaria" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot", "BigKeyD4", "Hammer" } },
    };
    [TestMethod]
    [DynamicData(nameof(TestData))]
    public void TestLogic(string location, bool expected, string[] inventory)
    {
        var randomizer = new Randomizer(new[]
        {
            new Dictionary<string, object>()
            {
                { "mode.state", "open" },
                { "logic", "NoGlitches" },
            }
        });
        randomizer.AssumeItems(inventory.Select(i => Item.Get(i, 0)));
        Assert.AreEqual(expected, randomizer.CanReachLocation($"{location}:0"));
    }
}
