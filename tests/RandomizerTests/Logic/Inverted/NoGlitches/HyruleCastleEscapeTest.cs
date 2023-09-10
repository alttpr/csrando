using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Inverted.NoGlitches;

[TestClass]
public class HyruleCastleEscapeTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Sanctuary Chest", false, new string[] {  } },
        new object[] { "Sanctuary Chest", false, new string[] { "AgahnimDefeated" } },
        new object[] { "Sanctuary Chest", true, new string[] { "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Sanctuary Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Sanctuary Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Sanctuary Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Sanctuary Chest", true, new string[] { "MoonPearl", "TitansMitt" } },

        new object[] { "Sewers - Secret Room - Left", false, new string[] {  } },
        new object[] { "Sewers - Secret Room - Left", false, new string[] { "ProgressiveGlove", "AgahnimDefeated" } },
        new object[] { "Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "ProgressiveGlove", "AgahnimDefeated" } },
        new object[] { "Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "PowerGlove", "AgahnimDefeated" } },
        new object[] { "Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "TitansMitt" } },
        new object[] { "Sewers - Secret Room - Left", true, new string[] { "MoonPearl", "AgahnimDefeated", "Lamp", "KeyH2" } },

        new object[] { "Sewers - Secret Room - Middle", false, new string[] {  } },
        new object[] { "Sewers - Secret Room - Middle", false, new string[] { "ProgressiveGlove", "AgahnimDefeated" } },
        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "ProgressiveGlove", "AgahnimDefeated" } },
        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "PowerGlove", "AgahnimDefeated" } },
        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "TitansMitt" } },
        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "MoonPearl", "AgahnimDefeated", "Lamp", "KeyH2" } },

        new object[] { "Sewers - Secret Room - Right", false, new string[] {  } },
        new object[] { "Sewers - Secret Room - Right", false, new string[] { "ProgressiveGlove", "AgahnimDefeated" } },
        new object[] { "Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "AgahnimDefeated" } },
        new object[] { "Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "PowerGlove", "AgahnimDefeated" } },
        new object[] { "Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "TitansMitt" } },
        new object[] { "Sewers - Secret Room - Right", true, new string[] { "MoonPearl", "AgahnimDefeated", "Lamp", "KeyH2" } },

        new object[] { "Sewers - Dark Cross Chest", false, new string[] {  } },
        new object[] { "Sewers - Dark Cross Chest", false, new string[] { "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Sewers - Dark Cross Chest", false, new string[] { "Lamp", "AgahnimDefeated" } },
        new object[] { "Sewers - Dark Cross Chest", true, new string[] { "Lamp", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Sewers - Dark Cross Chest", true, new string[] { "Lamp", "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Sewers - Dark Cross Chest", true, new string[] { "Lamp", "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Sewers - Dark Cross Chest", true, new string[] { "Lamp", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Sewers - Dark Cross Chest", true, new string[] { "Lamp", "MoonPearl", "TitansMitt" } },

        new object[] { "Hyrule Castle - Boomerang Chest", false, new string[] {  } },
        new object[] { "Hyrule Castle - Boomerang Chest", false, new string[] { "MoonPearl", "Lamp", "AgahnimDefeated" } },
        new object[] { "Hyrule Castle - Boomerang Chest", false, new string[] { "KeyH2", "AgahnimDefeated" } },
        new object[] { "Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2", "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2", "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2", "MoonPearl", "TitansMitt" } },

        new object[] { "Hyrule Castle - Map Chest", false, new string[] {  } },
        new object[] { "Hyrule Castle - Map Chest", false, new string[] { "AgahnimDefeated" } },
        new object[] { "Hyrule Castle - Map Chest", true, new string[] { "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Hyrule Castle - Map Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Hyrule Castle - Map Chest", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Hyrule Castle - Map Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Hyrule Castle - Map Chest", true, new string[] { "MoonPearl", "TitansMitt" } },

        new object[] { "Hyrule Castle - Zelda's Cell", false, new string[] {  } },
        new object[] { "Hyrule Castle - Zelda's Cell", false, new string[] { "MoonPearl", "Lamp", "AgahnimDefeated" } },
        new object[] { "Hyrule Castle - Zelda's Cell", false, new string[] { "KeyH2", "AgahnimDefeated" } },
        new object[] { "Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2", "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2", "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2", "MoonPearl", "TitansMitt" } },
    };
    [TestMethod]
    [DynamicData(nameof(TestData))]
    public void TestLogic(string location, bool expected, string[] inventory)
    {
        var randomizer = new Randomizer(new[]
        {
            new Dictionary<string, object>()
            {
                { "mode.state", "inverted" },
                { "logic", "NoGlitches" },
            }
        });
        randomizer.AssumeItems(inventory.Select(i => Item.Get(i, 0)));
        Assert.AreEqual(expected, randomizer.CanReachLocation($"{location}:0"));
    }
}
