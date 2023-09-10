using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Open.NoGlitches;

[TestClass]
public class SkullWoodsTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Skull Woods - Big Chest", false, new string[] {  } },
        new object[] { "Skull Woods - Big Chest", false, new string[] { "TitansMitt", "BigKeyD3" } },
        new object[] { "Skull Woods - Big Chest", true, new string[] { "MoonPearl", "TitansMitt", "BigKeyD3" } },
        new object[] { "Skull Woods - Big Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "BigKeyD3" } },
        new object[] { "Skull Woods - Big Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer", "BigKeyD3" } },
        new object[] { "Skull Woods - Big Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer", "BigKeyD3" } },
        new object[] { "Skull Woods - Big Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot", "BigKeyD3" } },
        new object[] { "Skull Woods - Big Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "BigKeyD3" } },
        new object[] { "Skull Woods - Big Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot", "BigKeyD3" } },
        new object[] { "Skull Woods - Big Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot", "BigKeyD3" } },

        new object[] { "Skull Woods - Big Key Chest", false, new string[] {  } },
        new object[] { "Skull Woods - Big Key Chest", true, new string[] { "MoonPearl", "TitansMitt" } },
        new object[] { "Skull Woods - Big Key Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Skull Woods - Big Key Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Skull Woods - Big Key Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Skull Woods - Big Key Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot" } },
        new object[] { "Skull Woods - Big Key Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Skull Woods - Big Key Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" } },
        new object[] { "Skull Woods - Big Key Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" } },

        new object[] { "Skull Woods - Compass Chest", false, new string[] {  } },
        new object[] { "Skull Woods - Compass Chest", true, new string[] { "MoonPearl", "TitansMitt" } },
        new object[] { "Skull Woods - Compass Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Skull Woods - Compass Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Skull Woods - Compass Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Skull Woods - Compass Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot" } },
        new object[] { "Skull Woods - Compass Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Skull Woods - Compass Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" } },
        new object[] { "Skull Woods - Compass Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" } },

        new object[] { "Skull Woods - Map Chest", false, new string[] {  } },
        new object[] { "Skull Woods - Map Chest", true, new string[] { "MoonPearl", "TitansMitt" } },
        new object[] { "Skull Woods - Map Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Skull Woods - Map Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Skull Woods - Map Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Skull Woods - Map Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot" } },
        new object[] { "Skull Woods - Map Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Skull Woods - Map Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" } },
        new object[] { "Skull Woods - Map Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" } },

        new object[] { "Skull Woods - Bridge Room Chest", false, new string[] {  } },
        new object[] { "Skull Woods - Bridge Room Chest", true, new string[] { "MoonPearl", "TitansMitt", "FireRod" } },
        new object[] { "Skull Woods - Bridge Room Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "FireRod" } },
        new object[] { "Skull Woods - Bridge Room Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer", "FireRod" } },
        new object[] { "Skull Woods - Bridge Room Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer", "FireRod" } },
        new object[] { "Skull Woods - Bridge Room Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot", "FireRod" } },
        new object[] { "Skull Woods - Bridge Room Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "FireRod" } },
        new object[] { "Skull Woods - Bridge Room Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot", "FireRod" } },
        new object[] { "Skull Woods - Bridge Room Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot", "FireRod" } },

        new object[] { "Skull Woods - Pot Prison", false, new string[] {  } },
        new object[] { "Skull Woods - Pot Prison", true, new string[] { "MoonPearl", "TitansMitt" } },
        new object[] { "Skull Woods - Pot Prison", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Skull Woods - Pot Prison", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Skull Woods - Pot Prison", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Skull Woods - Pot Prison", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot" } },
        new object[] { "Skull Woods - Pot Prison", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Skull Woods - Pot Prison", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" } },
        new object[] { "Skull Woods - Pot Prison", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" } },

        new object[] { "Skull Woods - Pinball Chest", false, new string[] {  } },
        new object[] { "Skull Woods - Pinball Chest", true, new string[] { "MoonPearl", "TitansMitt" } },
        new object[] { "Skull Woods - Pinball Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Skull Woods - Pinball Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Skull Woods - Pinball Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Skull Woods - Pinball Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot" } },
        new object[] { "Skull Woods - Pinball Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Skull Woods - Pinball Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" } },
        new object[] { "Skull Woods - Pinball Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" } },

        new object[] { "Skull Woods - Boss", false, new string[] {  } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "TitansMitt", "FireRod", "UncleSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "TitansMitt", "FireRod", "MasterSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "TitansMitt", "FireRod", "L3Sword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "TitansMitt", "FireRod", "L4Sword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "TitansMitt", "FireRod", "ProgressiveSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "FireRod", "UncleSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "FireRod", "MasterSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "FireRod", "L3Sword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "FireRod", "L4Sword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "FireRod", "ProgressiveSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "ProgressiveGlove", "Hammer", "FireRod", "UncleSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "ProgressiveGlove", "Hammer", "FireRod", "MasterSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "ProgressiveGlove", "Hammer", "FireRod", "L3Sword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "ProgressiveGlove", "Hammer", "FireRod", "L4Sword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "ProgressiveGlove", "Hammer", "FireRod", "ProgressiveSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "PowerGlove", "Hammer", "FireRod", "UncleSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "PowerGlove", "Hammer", "FireRod", "MasterSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "PowerGlove", "Hammer", "FireRod", "L3Sword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "PowerGlove", "Hammer", "FireRod", "L4Sword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "PowerGlove", "Hammer", "FireRod", "ProgressiveSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot", "FireRod", "UncleSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot", "FireRod", "MasterSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot", "FireRod", "L3Sword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot", "FireRod", "L4Sword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "Hammer", "Hookshot", "FireRod", "ProgressiveSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "FireRod", "UncleSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "FireRod", "MasterSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "FireRod", "L3Sword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "FireRod", "L4Sword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "FireRod", "ProgressiveSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot", "FireRod", "UncleSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot", "FireRod", "MasterSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot", "FireRod", "L3Sword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot", "FireRod", "L4Sword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot", "FireRod", "ProgressiveSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot", "FireRod", "UncleSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot", "FireRod", "MasterSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot", "FireRod", "L3Sword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot", "FireRod", "L4Sword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot", "FireRod", "ProgressiveSword" } },
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
