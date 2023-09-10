using AlttpRandomizer.Graph;

namespace RandomizerTests;

[TestClass]
public class PalaceOfDarkness
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] {"Palace of Darkness - Big Key Chest", false, new string[] { } },
        new object[] {"Palace of Darkness - Big Key Chest", false, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Hammer", "PowerGlove" } },
        new object[] {"Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Hammer", "TitansMitt" } },
        new object[] {"Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Hammer", "ProgressiveGlove" } },
        new object[] {"Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] {"Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" } },

        new object[] {"Palace of Darkness - The Arena - Ledge", false, new string[] { } },
        new object[] {"Palace of Darkness - The Arena - Ledge", false, new string[] { "MoonPearl", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MoonPearl", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MoonPearl", "Hammer", "PowerGlove" } },
        new object[] {"Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MoonPearl", "Hammer", "TitansMitt" } },
        new object[] {"Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MoonPearl", "Hammer", "ProgressiveGlove" } },
        new object[] {"Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] {"Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" } },

        new object[] {"Palace of Darkness - The Arena - Bridge", false, new string[] { } },
        new object[] {"Palace of Darkness - The Arena - Bridge", false, new string[] { "KeyD1", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MoonPearl", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MoonPearl", "Hammer", "PowerGlove" } },
        new object[] {"Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MoonPearl", "Hammer", "TitansMitt" } },
        new object[] {"Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MoonPearl", "Hammer", "ProgressiveGlove" } },
        new object[] {"Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] {"Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] {"Palace of Darkness - The Arena - Bridge", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - The Arena - Bridge", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "PowerGlove" } },
        new object[] {"Palace of Darkness - The Arena - Bridge", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "TitansMitt" } },
        new object[] {"Palace of Darkness - The Arena - Bridge", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "ProgressiveGlove" } },

        new object[] {"Palace of Darkness - Big Chest", false, new string[] { } },
        new object[] {"Palace of Darkness - Big Chest", false, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Hammer", "PowerGlove" } },
        new object[] {"Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Hammer", "TitansMitt" } },
        new object[] {"Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Hammer", "ProgressiveGlove" } },
        new object[] {"Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Flippers", "TitansMitt" } },
        new object[] {"Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Flippers", "ProgressiveGlove", "ProgressiveGlove" } },

        new object[] {"Palace of Darkness - Compass Chest", false, new string[] { } },
        new object[] {"Palace of Darkness - Compass Chest", false, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Hammer", "PowerGlove" } },
        new object[] {"Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Hammer", "TitansMitt" } },
        new object[] {"Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Hammer", "ProgressiveGlove" } },
        new object[] {"Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] {"Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" } },

        new object[] {"Palace of Darkness - Harmless Hellway Chest", false, new string[] { } },
        new object[] {"Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Hammer", "PowerGlove" } },
        new object[] {"Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Hammer", "TitansMitt" } },
        new object[] {"Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Hammer", "ProgressiveGlove" } },
        new object[] {"Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] {"Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" } },

        new object[] {"Palace of Darkness - Stalfos Basement Chest", false, new string[] { } },
        new object[] {"Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "MoonPearl", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "MoonPearl", "Hammer", "PowerGlove" } },
        new object[] {"Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "MoonPearl", "Hammer", "TitansMitt" } },
        new object[] {"Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "MoonPearl", "Hammer", "ProgressiveGlove" } },
        new object[] {"Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] {"Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] {"Palace of Darkness - Stalfos Basement Chest", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - Stalfos Basement Chest", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "PowerGlove" } },
        new object[] {"Palace of Darkness - Stalfos Basement Chest", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "TitansMitt" } },
        new object[] {"Palace of Darkness - Stalfos Basement Chest", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "ProgressiveGlove" } },

        new object[] {"Palace of Darkness - Dark Basement - Left", false, new string[] { } },
        new object[] {"Palace of Darkness - Dark Basement - Left", false, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Hammer", "PowerGlove" } },
        new object[] {"Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Hammer", "TitansMitt" } },
        new object[] {"Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Hammer", "ProgressiveGlove" } },
        new object[] {"Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] {"Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" } },

        new object[] {"Palace of Darkness - Dark Basement - Right", false, new string[] { } },
        new object[] {"Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Hammer", "PowerGlove" } },
        new object[] {"Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Hammer", "TitansMitt" } },
        new object[] {"Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Hammer", "ProgressiveGlove" } },
        new object[] {"Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] {"Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" } },

        new object[] {"Palace of Darkness - Map Chest", false, new string[] { } },
        new object[] {"Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MoonPearl", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MoonPearl", "Hammer", "PowerGlove" } },
        new object[] {"Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MoonPearl", "Hammer", "TitansMitt" } },
        new object[] {"Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MoonPearl", "Hammer", "ProgressiveGlove" } },
        new object[] {"Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] {"Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" } },

        new object[] {"Palace of Darkness - Dark Maze - Top", false, new string[] { } },
        new object[] {"Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Hammer", "PowerGlove" } },
        new object[] {"Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Hammer", "TitansMitt" } },
        new object[] {"Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Hammer", "ProgressiveGlove" } },
        new object[] {"Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Flippers", "TitansMitt" } },
        new object[] {"Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Flippers", "ProgressiveGlove", "ProgressiveGlove" } },

        new object[] {"Palace of Darkness - Dark Maze - Bottom", false, new string[] { } },
        new object[] {"Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Hammer", "PowerGlove" } },
        new object[] {"Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Hammer", "TitansMitt" } },
        new object[] {"Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Hammer", "ProgressiveGlove" } },
        new object[] {"Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Flippers", "TitansMitt" } },
        new object[] {"Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "Flippers", "ProgressiveGlove", "ProgressiveGlove" } },

        new object[] {"Palace of Darkness - Shooter Room Chest", false, new string[] { } },
        new object[] {"Palace of Darkness - Shooter Room Chest", false, new string[] { "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - Shooter Room Chest", true, new string[] { "MoonPearl", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - Shooter Room Chest", true, new string[] { "MoonPearl", "Hammer", "PowerGlove" } },
        new object[] {"Palace of Darkness - Shooter Room Chest", true, new string[] { "MoonPearl", "Hammer", "TitansMitt" } },
        new object[] {"Palace of Darkness - Shooter Room Chest", true, new string[] { "MoonPearl", "Hammer", "ProgressiveGlove" } },
        new object[] {"Palace of Darkness - Shooter Room Chest", true, new string[] { "MoonPearl", "Flippers", "TitansMitt" } },
        new object[] {"Palace of Darkness - Shooter Room Chest", true, new string[] { "MoonPearl", "Flippers", "ProgressiveGlove", "ProgressiveGlove" } },

        new object[] {"Palace of Darkness - Boss", false, new string[] { } },
        new object[] {"Palace of Darkness - Boss", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Hammer", "BowAndArrows", "AgahnimDefeated" } },
        new object[] {"Palace of Darkness - Boss", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Hammer", "BowAndArrows", "PowerGlove" } },
        new object[] {"Palace of Darkness - Boss", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Hammer", "BowAndArrows", "TitansMitt" } },
        new object[] {"Palace of Darkness - Boss", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Hammer", "BowAndArrows", "ProgressiveGlove" } },
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
