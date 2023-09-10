using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Open.NoGlitches;

[TestClass]
public class SwampPalaceTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Swamp Palace - Entrance Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] { "Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" } },
        new object[] { "Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" } },
        new object[] { "Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" } },
        new object[] { "Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hookshot" } },

        new object[] { "Swamp Palace - Big Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - Big Chest", true, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - Big Chest", true, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" } },
        new object[] { "Swamp Palace - Big Chest", true, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" } },
        new object[] { "Swamp Palace - Big Chest", true, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" } },

        new object[] { "Swamp Palace - Big Key Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - Big Key Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - Big Key Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" } },
        new object[] { "Swamp Palace - Big Key Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" } },
        new object[] { "Swamp Palace - Big Key Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" } },

        new object[] { "Swamp Palace - Map Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] { "Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" } },
        new object[] { "Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" } },
        new object[] { "Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" } },
        new object[] { "Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hookshot" } },

        new object[] { "Swamp Palace - West Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - West Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - West Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" } },
        new object[] { "Swamp Palace - West Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" } },
        new object[] { "Swamp Palace - West Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" } },

        new object[] { "Swamp Palace - Compass Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - Compass Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - Compass Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" } },
        new object[] { "Swamp Palace - Compass Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" } },
        new object[] { "Swamp Palace - Compass Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" } },

        new object[] { "Swamp Palace - Flooded Room - Left", false, new string[] {  } },
        new object[] { "Swamp Palace - Flooded Room - Left", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Left", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Left", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Left", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer", "Hookshot" } },

        new object[] { "Swamp Palace - Flooded Room - Right", false, new string[] {  } },
        new object[] { "Swamp Palace - Flooded Room - Right", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Right", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Right", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Right", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer", "Hookshot" } },

        new object[] { "Swamp Palace - Waterfall Room Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - Waterfall Room Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Waterfall Room Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Waterfall Room Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Waterfall Room Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer", "Hookshot" } },

        new object[] { "Swamp Palace - Boss", false, new string[] {  } },
        new object[] { "Swamp Palace - Boss", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Boss", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Boss", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Boss", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer", "Hookshot" } },
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
