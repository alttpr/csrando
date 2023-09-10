using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Inverted.NoGlitches;

[TestClass]
public class SwampPalaceTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Swamp Palace - Entrance Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - Entrance Chest", false, new string[] { "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] { "Swamp Palace - Entrance Chest", false, new string[] { "MagicMirror", "Flippers", "TitansMitt" } },
        new object[] { "Swamp Palace - Entrance Chest", false, new string[] { "MagicMirror", "MoonPearl", "TitansMitt" } },
        new object[] { "Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] { "Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" } },
        new object[] { "Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" } },
        new object[] { "Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated" } },

        new object[] { "Swamp Palace - Big Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - Big Chest", false, new string[] { "BigKeyD2", "KeyD2", "MoonPearl", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - Big Chest", false, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - Big Chest", false, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - Big Chest", false, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] { "Swamp Palace - Big Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - Big Chest", true, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - Big Chest", true, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" } },
        new object[] { "Swamp Palace - Big Chest", true, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" } },
        new object[] { "Swamp Palace - Big Chest", true, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" } },

        new object[] { "Swamp Palace - Big Key Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - Big Key Chest", false, new string[] { "KeyD2", "MoonPearl", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - Big Key Chest", false, new string[] { "KeyD2", "MagicMirror", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - Big Key Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - Big Key Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] { "Swamp Palace - Big Key Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - Big Key Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" } },
        new object[] { "Swamp Palace - Big Key Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" } },
        new object[] { "Swamp Palace - Big Key Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" } },

        new object[] { "Swamp Palace - Map Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - Map Chest", false, new string[] { "KeyD2", "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] { "Swamp Palace - Map Chest", false, new string[] { "KeyD2", "MagicMirror", "Flippers", "TitansMitt" } },
        new object[] { "Swamp Palace - Map Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "TitansMitt" } },
        new object[] { "Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] { "Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" } },
        new object[] { "Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" } },
        new object[] { "Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" } },
        new object[] { "Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hookshot" } },

        new object[] { "Swamp Palace - West Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - West Chest", false, new string[] { "KeyD2", "MoonPearl", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - West Chest", false, new string[] { "KeyD2", "MagicMirror", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - West Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - West Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] { "Swamp Palace - West Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - West Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" } },
        new object[] { "Swamp Palace - West Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" } },
        new object[] { "Swamp Palace - West Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" } },

        new object[] { "Swamp Palace - Compass Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - Compass Chest", false, new string[] { "KeyD2", "MoonPearl", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - Compass Chest", false, new string[] { "KeyD2", "MagicMirror", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - Compass Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - Compass Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] { "Swamp Palace - Compass Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - Compass Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer" } },
        new object[] { "Swamp Palace - Compass Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer" } },
        new object[] { "Swamp Palace - Compass Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer" } },

        new object[] { "Swamp Palace - Flooded Room - Left", false, new string[] {  } },
        new object[] { "Swamp Palace - Flooded Room - Left", false, new string[] { "KeyD2", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Left", false, new string[] { "KeyD2", "MagicMirror", "Flippers", "TitansMitt", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Left", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Left", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Left", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - Flooded Room - Left", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Left", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Left", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Left", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer", "Hookshot" } },

        new object[] { "Swamp Palace - Flooded Room - Right", false, new string[] {  } },
        new object[] { "Swamp Palace - Flooded Room - Right", false, new string[] { "KeyD2", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Right", false, new string[] { "KeyD2", "MagicMirror", "Flippers", "TitansMitt", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Right", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Right", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Right", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - Flooded Room - Right", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Right", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Right", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Flooded Room - Right", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer", "Hookshot" } },

        new object[] { "Swamp Palace - Waterfall Room Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - Waterfall Room Chest", false, new string[] { "KeyD2", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Waterfall Room Chest", false, new string[] { "KeyD2", "MagicMirror", "Flippers", "TitansMitt", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Waterfall Room Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Waterfall Room Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hookshot" } },
        new object[] { "Swamp Palace - Waterfall Room Chest", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" } },
        new object[] { "Swamp Palace - Waterfall Room Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Waterfall Room Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "ProgressiveGlove", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Waterfall Room Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PowerGlove", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Waterfall Room Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "AgahnimDefeated", "Hammer", "Hookshot" } },

        new object[] { "Swamp Palace - Boss", false, new string[] {  } },
        new object[] { "Swamp Palace - Boss", false, new string[] { "KeyD2", "MoonPearl", "Flippers", "TitansMitt", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Boss", false, new string[] { "KeyD2", "MagicMirror", "Flippers", "TitansMitt", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Boss", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "TitansMitt", "Hammer", "Hookshot" } },
        new object[] { "Swamp Palace - Boss", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hookshot" } },
        new object[] { "Swamp Palace - Boss", false, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" } },
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
            new RandomizerConfig
            {
                Glitches = GlitchesOption.None,
                State = StateOption.Inverted,
            }
        });
        randomizer.AssumeItems(inventory.Select(i => Item.Get(i, 0)));
        Assert.AreEqual(expected, randomizer.CanReachLocation($"{location}:0"));
    }
    [TestMethod]
    public void TestKeyForKey()
    {
        var randomizer = new Randomizer(new[]
        {
            new RandomizerConfig
            {
                Accessibility = AccessibilityOption.Items,
                Glitches = GlitchesOption.None,
                State = StateOption.Inverted,
            }
        });
        randomizer.AssumeItems(new[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "TitansMitt", "Hammer" }.Select(i => Item.Get(i, 0)));
        Assert.IsTrue(randomizer.CanReachLocation("Swamp Palace - Big Chest:0"));
    }
}
