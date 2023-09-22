namespace RandomizerTests.Logic.Inverted.OverworldGlitches;

[TestClass]
public sealed class PalaceOfDarknessTest : InvertedOverworldGlitchesLogicTests
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
        new object[] { "Palace of Darkness - Big Key Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1" } },

        new object[] { "Palace of Darkness - The Arena - Ledge", false, new string[] {  } },
        new object[] { "Palace of Darkness - The Arena - Ledge", true, new string[] { "BowAndArrows" } },

        new object[] { "Palace of Darkness - The Arena - Bridge", false, new string[] {  } },
        new object[] { "Palace of Darkness - The Arena - Bridge", true, new string[] { "KeyD1" } },

        new object[] { "Palace of Darkness - Big Chest", false, new string[] {  } },
        new object[] { "Palace of Darkness - Big Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "Lamp" } },

        new object[] { "Palace of Darkness - Compass Chest", false, new string[] {  } },
        new object[] { "Palace of Darkness - Compass Chest", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1" } },

        new object[] { "Palace of Darkness - Harmless Hellway", false, new string[] {  } },
        new object[] { "Palace of Darkness - Harmless Hellway", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1" } },

        new object[] { "Palace of Darkness - Stalfos Basement", false, new string[] {  } },
        new object[] { "Palace of Darkness - Stalfos Basement", true, new string[] { "KeyD1" } },

        new object[] { "Palace of Darkness - Dark Basement - Left", false, new string[] {  } },
        new object[] { "Palace of Darkness - Dark Basement - Left", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp" } },

        new object[] { "Palace of Darkness - Dark Basement - Right", false, new string[] {  } },
        new object[] { "Palace of Darkness - Dark Basement - Right", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp" } },

        new object[] { "Palace of Darkness - Map Chest", false, new string[] {  } },
        new object[] { "Palace of Darkness - Map Chest", true, new string[] { "BowAndArrows" } },

        new object[] { "Palace of Darkness - Dark Maze - Top", false, new string[] {  } },
        new object[] { "Palace of Darkness - Dark Maze - Top", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp" } },

        new object[] { "Palace of Darkness - Dark Maze - Bottom", false, new string[] {  } },
        new object[] { "Palace of Darkness - Dark Maze - Bottom", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "Lamp" } },

        new object[] { "Palace of Darkness - Shooter Room", true, new string[] {  } },

        new object[] { "Palace of Darkness - Boss", false, new string[] {  } },
        new object[] { "Palace of Darkness - Boss", true, new string[] { "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "KeyD1", "BigKeyD1", "Lamp", "Hammer", "BowAndArrows" } },
    };
}
