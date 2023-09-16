namespace RandomizerTests.Logic.Standard.OverworldGlitches;

[TestClass]
public sealed class SwampPalaceTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Swamp Palace - Entrance Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - Entrance Chest", true, new string[] { "MagicMirror", "MoonPearl", "Flippers", "PegasusBoots" } },

        new object[] { "Swamp Palace - Big Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - Big Chest", true, new string[] { "BigKeyD2", "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PegasusBoots", "Hammer" } },

        new object[] { "Swamp Palace - Big Key Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - Big Key Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PegasusBoots", "Hammer" } },

        new object[] { "Swamp Palace - Map Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - Map Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PegasusBoots" } },

        new object[] { "Swamp Palace - West Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - West Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PegasusBoots", "Hammer" } },

        new object[] { "Swamp Palace - Compass Chest", false, new string[] {  } },
        new object[] { "Swamp Palace - Compass Chest", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PegasusBoots", "Hammer" } },

        new object[] { "Swamp Palace - Flooded Room - Left", false, new string[] {  } },
        new object[] { "Swamp Palace - Flooded Room - Left", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PegasusBoots", "Hammer", "Hookshot" } },

        new object[] { "Swamp Palace - Flooded Room - Right", false, new string[] {  } },
        new object[] { "Swamp Palace - Flooded Room - Right", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PegasusBoots", "Hammer", "Hookshot" } },

        new object[] { "Swamp Palace - Waterfall Room", false, new string[] {  } },
        new object[] { "Swamp Palace - Waterfall Room", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PegasusBoots", "Hammer", "Hookshot" } },

        new object[] { "Swamp Palace - Boss", false, new string[] {  } },
        new object[] { "Swamp Palace - Boss", true, new string[] { "KeyD2", "MagicMirror", "MoonPearl", "Flippers", "PegasusBoots", "Hammer", "Hookshot" } },
    };
}
