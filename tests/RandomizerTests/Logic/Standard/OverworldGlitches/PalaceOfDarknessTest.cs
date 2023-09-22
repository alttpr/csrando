namespace RandomizerTests.Logic.Standard.OverworldGlitches;

[TestClass]
public sealed class PalaceOfDarknessTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Palace of Darkness - Big Key Chest", false, new string[] {  } },
        new object[] { "Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "PegasusBoots" } },
        new object[] { "Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "TitansMitt" } },

        new object[] { "Palace of Darkness - The Arena - Ledge", false, new string[] {  } },
        new object[] { "Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MoonPearl", "PegasusBoots" } },
        new object[] { "Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows", "MoonPearl", "TitansMitt" } },

        new object[] { "Palace of Darkness - The Arena - Bridge", false, new string[] {  } },
        new object[] { "Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MoonPearl", "PegasusBoots" } },
        new object[] { "Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - The Arena - Bridge", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "PegasusBoots" } },

        new object[] { "Palace of Darkness - Big Chest", false, new string[] {  } },
        new object[] { "Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "PegasusBoots" } },
        new object[] { "Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "TitansMitt" } },

        new object[] { "Palace of Darkness - Compass Chest", false, new string[] {  } },
        new object[] { "Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "PegasusBoots" } },
        new object[] { "Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "TitansMitt" } },

        new object[] { "Palace of Darkness - Harmless Hellway", false, new string[] {  } },
        new object[] { "Palace of Darkness - Harmless Hellway", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "PegasusBoots" } },
        new object[] { "Palace of Darkness - Harmless Hellway", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Harmless Hellway", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "TitansMitt" } },

        new object[] { "Palace of Darkness - Stalfos Basement", false, new string[] {  } },
        new object[] { "Palace of Darkness - Stalfos Basement", true, new string[] { "KeyD1", "MoonPearl", "PegasusBoots" } },
        new object[] { "Palace of Darkness - Stalfos Basement", true, new string[] { "KeyD1", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Stalfos Basement", true, new string[] { "KeyD1", "MoonPearl", "TitansMitt" } },
        new object[] { "Palace of Darkness - Stalfos Basement", true, new string[] { "BowAndArrows", "Hammer", "MoonPearl", "PegasusBoots" } },

        new object[] { "Palace of Darkness - Dark Basement - Left", false, new string[] {  } },
        new object[] { "Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "PegasusBoots" } },
        new object[] { "Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "TitansMitt" } },

        new object[] { "Palace of Darkness - Dark Basement - Right", false, new string[] {  } },
        new object[] { "Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "PegasusBoots" } },
        new object[] { "Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp", "MoonPearl", "TitansMitt" } },

        new object[] { "Palace of Darkness - Map Chest", false, new string[] {  } },
        new object[] { "Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MoonPearl", "PegasusBoots" } },
        new object[] { "Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows", "MoonPearl", "TitansMitt" } },

        new object[] { "Palace of Darkness - Dark Maze - Top", false, new string[] {  } },
        new object[] { "Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "PegasusBoots" } },
        new object[] { "Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "TitansMitt" } },

        new object[] { "Palace of Darkness - Dark Maze - Bottom", false, new string[] {  } },
        new object[] { "Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "PegasusBoots" } },
        new object[] { "Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "MoonPearl", "Lamp", "TitansMitt" } },

        new object[] { "Palace of Darkness - Shooter Room", false, new string[] {  } },
        new object[] { "Palace of Darkness - Shooter Room", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Palace of Darkness - Shooter Room", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Palace of Darkness - Shooter Room", true, new string[] { "MoonPearl", "TitansMitt" } },

        new object[] { "Palace of Darkness - Boss", false, new string[] {  } },
        new object[] { "Palace of Darkness - Boss", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "MoonPearl", "Lamp", "Hammer", "BowAndArrows", "PegasusBoots" } },
    };
}
