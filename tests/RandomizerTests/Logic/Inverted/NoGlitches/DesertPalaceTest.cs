using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Inverted.NoGlitches;

[TestClass]
public class DeserPalaceTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Desert Palace - Map Chest", false, new string[] {  } },
        new object[] { "Desert Palace - Map Chest", false, new string[] { "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Desert Palace - Map Chest", false, new string[] { "BookOfMudora", "AgahnimDefeated" } },
        new object[] { "Desert Palace - Map Chest", true, new string[] { "BookOfMudora", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Desert Palace - Map Chest", true, new string[] { "BookOfMudora", "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Desert Palace - Map Chest", true, new string[] { "BookOfMudora", "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Desert Palace - Map Chest", true, new string[] { "BookOfMudora", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Desert Palace - Map Chest", true, new string[] { "BookOfMudora", "MoonPearl", "TitansMitt" } },

        new object[] { "Desert Palace - Big Chest", false, new string[] {  } },
        new object[] { "Desert Palace - Big Chest", false, new string[] { "BigKeyP2", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Desert Palace - Big Chest", false, new string[] { "BookOfMudora", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Desert Palace - Big Chest", false, new string[] { "BookOfMudora", "BigKeyP2", "AgahnimDefeated" } },
        new object[] { "Desert Palace - Big Chest", true, new string[] { "BookOfMudora", "BigKeyP2", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Desert Palace - Big Chest", true, new string[] { "BookOfMudora", "BigKeyP2", "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Desert Palace - Big Chest", true, new string[] { "BookOfMudora", "BigKeyP2", "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Desert Palace - Big Chest", true, new string[] { "BookOfMudora", "BigKeyP2", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Desert Palace - Big Chest", true, new string[] { "BookOfMudora", "BigKeyP2", "MoonPearl", "TitansMitt" } },

        new object[] { "Desert Palace - Torch", false, new string[] {  } },
        new object[] { "Desert Palace - Torch", false, new string[] { "PegasusBoots", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Desert Palace - Torch", false, new string[] { "BookOfMudora", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Desert Palace - Torch", false, new string[] { "BookOfMudora", "PegasusBoots", "AgahnimDefeated" } },
        new object[] { "Desert Palace - Torch", true, new string[] { "BookOfMudora", "PegasusBoots", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Desert Palace - Torch", true, new string[] { "BookOfMudora", "PegasusBoots", "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Desert Palace - Torch", true, new string[] { "BookOfMudora", "PegasusBoots", "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Desert Palace - Torch", true, new string[] { "BookOfMudora", "PegasusBoots", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Desert Palace - Torch", true, new string[] { "BookOfMudora", "PegasusBoots", "MoonPearl", "TitansMitt" } },

        new object[] { "Desert Palace - Compass Chest", false, new string[] {  } },
        new object[] { "Desert Palace - Compass Chest", false, new string[] { "KeyP2", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Desert Palace - Compass Chest", false, new string[] { "BookOfMudora", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Desert Palace - Compass Chest", false, new string[] { "BookOfMudora", "KeyP2", "AgahnimDefeated" } },
        new object[] { "Desert Palace - Compass Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Desert Palace - Compass Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Desert Palace - Compass Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Desert Palace - Compass Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Desert Palace - Compass Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "TitansMitt" } },

        new object[] { "Desert Palace - Big Key Chest", false, new string[] {  } },
        new object[] { "Desert Palace - Big Key Chest", false, new string[] { "KeyP2", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Desert Palace - Big Key Chest", false, new string[] { "BookOfMudora", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Desert Palace - Big Key Chest", false, new string[] { "BookOfMudora", "KeyP2", "AgahnimDefeated" } },
        new object[] { "Desert Palace - Big Key Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Desert Palace - Big Key Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Desert Palace - Big Key Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Desert Palace - Big Key Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Desert Palace - Big Key Chest", true, new string[] { "BookOfMudora", "KeyP2", "MoonPearl", "TitansMitt" } },

        new object[] { "Desert Palace - Boss", false, new string[] {  } },
        new object[] { "Desert Palace - Boss", false, new string[] { "UncleSword", "MoonPearl", "ProgressiveGlove", "AgahnimDefeated", "BookOfMudora", "Lamp", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", false, new string[] { "UncleSword", "MoonPearl", "ProgressiveGlove", "AgahnimDefeated", "KeyP2", "BookOfMudora", "Lamp" } },
        new object[] { "Desert Palace - Boss", false, new string[] { "UncleSword", "MoonPearl", "ProgressiveGlove", "AgahnimDefeated", "KeyP2", "Lamp", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", false, new string[] { "UncleSword", "ProgressiveGlove", "AgahnimDefeated", "KeyP2", "BookOfMudora", "Lamp", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", false, new string[] { "UncleSword", "MoonPearl", "AgahnimDefeated", "KeyP2", "BookOfMudora", "Lamp", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "MoonPearl", "ProgressiveGlove", "AgahnimDefeated", "KeyP2", "BookOfMudora", "Lamp", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "MoonPearl", "PowerGlove", "AgahnimDefeated", "KeyP2", "BookOfMudora", "Lamp", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer", "KeyP2", "BookOfMudora", "Lamp", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "MoonPearl", "PowerGlove", "Hammer", "KeyP2", "BookOfMudora", "Lamp", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "KeyP2", "BookOfMudora", "Lamp", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "MoonPearl", "TitansMitt", "KeyP2", "BookOfMudora", "Lamp", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "MoonPearl", "ProgressiveGlove", "AgahnimDefeated", "KeyP2", "BookOfMudora", "FireRod", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "MoonPearl", "PowerGlove", "AgahnimDefeated", "KeyP2", "BookOfMudora", "FireRod", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer", "KeyP2", "BookOfMudora", "FireRod", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "MoonPearl", "PowerGlove", "Hammer", "KeyP2", "BookOfMudora", "FireRod", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "KeyP2", "BookOfMudora", "FireRod", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "MoonPearl", "TitansMitt", "KeyP2", "BookOfMudora", "FireRod", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "MoonPearl", "TitansMitt", "KeyP2", "BookOfMudora", "FireRod", "BigKeyP2" } },
    };
    [TestMethod]
    [DynamicData(nameof(TestData))]
    public void TestLogic(string location, bool expected, string[] inventory)
    {
        var randomizer = new Randomizer(new[]
        {
            new RandomizerConfig
            {
                Glitches = GlitchesOption.None,
                State = StateOption.Inverted,
            }
        });
        randomizer.AssumeItems(inventory.Select(i => Item.Get(i, 0)));
        Assert.AreEqual(expected, randomizer.CanReachLocation($"{location}:0"));
    }
}
