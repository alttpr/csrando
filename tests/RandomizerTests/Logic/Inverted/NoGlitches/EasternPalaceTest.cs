using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Inverted.NoGlitches;

[TestClass]
public class EasternPalaceTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Eastern Palace - Compass Chest", false, new string[] {  } },
        new object[] { "Eastern Palace - Compass Chest", false, new string[] { "AgahnimDefeated" } },
        new object[] { "Eastern Palace - Compass Chest", true, new string[] { "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Eastern Palace - Compass Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Eastern Palace - Compass Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Eastern Palace - Compass Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Eastern Palace - Compass Chest", true, new string[] { "MoonPearl", "TitansMitt" } },

        new object[] { "Eastern Palace - Cannonball Chest", false, new string[] {  } },
        new object[] { "Eastern Palace - Cannonball Chest", false, new string[] { "AgahnimDefeated", } },
        new object[] { "Eastern Palace - Cannonball Chest", true, new string[] { "MoonPearl", "AgahnimDefeated", } },
        new object[] { "Eastern Palace - Cannonball Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Eastern Palace - Cannonball Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Eastern Palace - Cannonball Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Eastern Palace - Cannonball Chest", true, new string[] { "MoonPearl", "TitansMitt" } },

        new object[] { "Eastern Palace - Big Chest", false, new string[] {  } },
        new object[] { "Eastern Palace - Big Chest", false, new string[] { "BigKeyP1", "AgahnimDefeated" } },
        new object[] { "Eastern Palace - Big Chest", false, new string[] { "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Eastern Palace - Big Chest", true, new string[] { "BigKeyP1", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Eastern Palace - Big Chest", true, new string[] { "BigKeyP1", "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Eastern Palace - Big Chest", true, new string[] { "BigKeyP1", "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Eastern Palace - Big Chest", true, new string[] { "BigKeyP1", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Eastern Palace - Big Chest", true, new string[] { "BigKeyP1", "MoonPearl", "TitansMitt" } },

        new object[] { "Eastern Palace - Map Chest", false, new string[] {  } },
        new object[] { "Eastern Palace - Map Chest", false, new string[] { "AgahnimDefeated" } },
        new object[] { "Eastern Palace - Map Chest", true, new string[] { "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Eastern Palace - Map Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Eastern Palace - Map Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Eastern Palace - Map Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Eastern Palace - Map Chest", true, new string[] { "MoonPearl", "TitansMitt" } },

        new object[] { "Eastern Palace - Big Key Chest", false, new string[] {  } },
        new object[] { "Eastern Palace - Big Key Chest", false, new string[] { "Lamp", "AgahnimDefeated" } },
        new object[] { "Eastern Palace - Big Key Chest", false, new string[] { "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Eastern Palace - Big Key Chest", true, new string[] { "Lamp", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Eastern Palace - Big Key Chest", true, new string[] { "Lamp", "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Eastern Palace - Big Key Chest", true, new string[] { "Lamp", "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Eastern Palace - Big Key Chest", true, new string[] { "Lamp", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Eastern Palace - Big Key Chest", true, new string[] { "Lamp", "MoonPearl", "TitansMitt" } },

        new object[] { "Eastern Palace - Boss", false, new string[] {  } },
        new object[] { "Eastern Palace - Boss", false, new string[] { "Lamp", "BowAndArrows", "BigKeyP1", "AgahnimDefeated" } },
        new object[] { "Eastern Palace - Boss", false, new string[] { "BowAndArrows", "BigKeyP1", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Eastern Palace - Boss", false, new string[] { "Lamp", "BigKeyP1", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Eastern Palace - Boss", false, new string[] { "Lamp", "BowAndArrows", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Eastern Palace - Boss", true, new string[] { "Lamp", "BowAndArrows", "BigKeyP1", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Eastern Palace - Boss", true, new string[] { "Lamp", "BowAndArrows", "BigKeyP1", "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Eastern Palace - Boss", true, new string[] { "Lamp", "BowAndArrows", "BigKeyP1", "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Eastern Palace - Boss", true, new string[] { "Lamp", "BowAndArrows", "BigKeyP1", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Eastern Palace - Boss", true, new string[] { "Lamp", "BowAndArrows", "BigKeyP1", "MoonPearl", "TitansMitt" } },
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
