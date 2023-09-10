using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Inverted.NoGlitches;

[TestClass]
public class PalaceOfDarknessTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Palace of Darkness - Big Key Chest", false, new string[] {  } },
        new object[] { "Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Flippers" } },
        new object[] { "Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Hammer" } },
        new object[] { "Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "OcarinaInactive", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "OcarinaInactive", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MagicMirror", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MagicMirror", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MagicMirror", "AgahnimDefeated" } },

        new object[] { "Palace of Darkness - The Arena - Ledge", false, new string[] {  } },
        new object[] { "Palace of Darkness - The Arena - Ledge", false, new string[] { "Flippers" } },
        new object[] { "Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "Flippers" } },
        new object[] { "Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "Hammer" } },
        new object[] { "Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "OcarinaInactive", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "OcarinaInactive", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MagicMirror", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MagicMirror", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MagicMirror", "AgahnimDefeated" } },

        new object[] { "Palace of Darkness - The Arena - Bridge", false, new string[] {  } },
        new object[] { "Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "Flippers" } },
        new object[] { "Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "Hammer" } },
        new object[] { "Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "OcarinaInactive", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "OcarinaInactive", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MagicMirror", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MagicMirror", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MagicMirror", "AgahnimDefeated" } },
        new object[] { "Palace of Darkness - The Arena - Bridge", true, new string[] { "BowAndArrows", "Hammer" } },

        new object[] { "Palace of Darkness - Big Chest", false, new string[] {  } },
        new object[] { "Palace of Darkness - Big Chest", false, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "Flippers" } },
        new object[] { "Palace of Darkness - Big Chest", false, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "Flippers" } },
        new object[] { "Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "Lamp", "Flippers" } },
        new object[] { "Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "Lamp", "Hammer" } },
        new object[] { "Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "Lamp", "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "Lamp", "OcarinaInactive", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "Lamp", "OcarinaInactive", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "Lamp", "MagicMirror", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "Lamp", "MagicMirror", "AgahnimDefeated" } },

        new object[] { "Palace of Darkness - Compass Chest", false, new string[] {  } },
        new object[] { "Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Flippers" } },
        new object[] { "Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Hammer" } },
        new object[] { "Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "OcarinaInactive", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "OcarinaInactive", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MagicMirror", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MagicMirror", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MagicMirror", "AgahnimDefeated" } },

        new object[] { "Palace of Darkness - Harmless Hellway Chest", false, new string[] {  } },
        new object[] { "Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Flippers" } },
        new object[] { "Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Hammer" } },
        new object[] { "Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "OcarinaInactive", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "OcarinaInactive", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MagicMirror", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MagicMirror", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Harmless Hellway Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MagicMirror", "AgahnimDefeated" } },

        new object[] { "Palace of Darkness - Stalfos Basement Chest", false, new string[] {  } },
        new object[] { "Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "Flippers" } },
        new object[] { "Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "Hammer" } },
        new object[] { "Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "OcarinaInactive", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "OcarinaInactive", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "MagicMirror", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "MagicMirror", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Stalfos Basement Chest", true, new string[] { "KeyD1", "MagicMirror", "AgahnimDefeated" } },
        new object[] { "Palace of Darkness - Stalfos Basement Chest", true, new string[] { "BowAndArrows", "Hammer" } },

        new object[] { "Palace of Darkness - Dark Basement - Left", false, new string[] {  } },
        new object[] { "Palace of Darkness - Dark Basement - Left", false, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Flippers" } },
        new object[] { "Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "Flippers" } },
        new object[] { "Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "Hammer" } },
        new object[] { "Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "OcarinaInactive", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "OcarinaInactive", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MagicMirror", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MagicMirror", "AgahnimDefeated" } },

        new object[] { "Palace of Darkness - Dark Basement - Right", false, new string[] {  } },
        new object[] { "Palace of Darkness - Dark Basement - Right", false, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Flippers" } },
        new object[] { "Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "Flippers" } },
        new object[] { "Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "Hammer" } },
        new object[] { "Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "OcarinaInactive", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "OcarinaInactive", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MagicMirror", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MagicMirror", "AgahnimDefeated" } },

        new object[] { "Palace of Darkness - Map Chest", false, new string[] {  } },
        new object[] { "Palace of Darkness - Map Chest", false, new string[] { "Flippers" } },
        new object[] { "Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "Flippers" } },
        new object[] { "Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "Hammer" } },
        new object[] { "Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "OcarinaInactive", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "OcarinaInactive", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MagicMirror", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MagicMirror", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MagicMirror", "AgahnimDefeated" } },

        new object[] { "Palace of Darkness - Dark Maze - Top", false, new string[] {  } },
        new object[] { "Palace of Darkness - Dark Maze - Top", false, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Flippers" } },
        new object[] { "Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "Flippers" } },
        new object[] { "Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "Hammer" } },
        new object[] { "Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "OcarinaInactive", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "OcarinaInactive", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MagicMirror", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MagicMirror", "AgahnimDefeated" } },

        new object[] { "Palace of Darkness - Dark Maze - Bottom", false, new string[] {  } },
        new object[] { "Palace of Darkness - Dark Maze - Bottom", false, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Flippers" } },
        new object[] { "Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "Flippers" } },
        new object[] { "Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "Hammer" } },
        new object[] { "Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "OcarinaInactive", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "OcarinaInactive", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MagicMirror", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MagicMirror", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MagicMirror", "AgahnimDefeated" } },

        new object[] { "Palace of Darkness - Shooter Room Chest", false, new string[] {  } },
        new object[] { "Palace of Darkness - Shooter Room Chest", true, new string[] { "Flippers" } },
        new object[] { "Palace of Darkness - Shooter Room Chest", true, new string[] { "Hammer" } },
        new object[] { "Palace of Darkness - Shooter Room Chest", true, new string[] { "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Shooter Room Chest", true, new string[] { "OcarinaInactive", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Shooter Room Chest", true, new string[] { "OcarinaInactive", "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Palace of Darkness - Shooter Room Chest", true, new string[] { "MagicMirror", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Shooter Room Chest", true, new string[] { "MagicMirror", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Shooter Room Chest", true, new string[] { "MagicMirror", "AgahnimDefeated" } },

        new object[] { "Palace of Darkness - Boss", false, new string[] {  } },
        new object[] { "Palace of Darkness - Boss", false, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "Lamp", "Hammer" } },
        new object[] { "Palace of Darkness - Boss", false, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "Lamp", "BowAndArrows" } },
        new object[] { "Palace of Darkness - Boss", false, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "Hammer", "BowAndArrows" } },
        new object[] { "Palace of Darkness - Boss", false, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "Hammer", "BowAndArrows" } },
        new object[] { "Palace of Darkness - Boss", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "Lamp", "Hammer", "BowAndArrows" } },
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
