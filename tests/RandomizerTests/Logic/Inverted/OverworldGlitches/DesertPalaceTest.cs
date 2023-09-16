namespace RandomizerTests.Logic.Inverted.OverworldGlitches;

[TestClass]
public sealed class DesertPalaceTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Desert Palace - Map Chest", false, new string[] {  } },
        new object[] { "Desert Palace - Map Chest", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Desert Palace - Map Chest", true, new string[] { "BookOfMudora", "MagicMirror", "PegasusBoots" } },

        new object[] { "Desert Palace - Big Chest", false, new string[] {  } },
        new object[] { "Desert Palace - Big Chest", true, new string[] { "MoonPearl", "PegasusBoots", "BigKeyP2" } },
        new object[] { "Desert Palace - Big Chest", true, new string[] { "BookOfMudora", "MagicMirror", "PegasusBoots", "BigKeyP2" } },

        new object[] { "Desert Palace - Torch", false, new string[] {  } },
        new object[] { "Desert Palace - Torch", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Desert Palace - Torch", true, new string[] { "BookOfMudora", "MagicMirror", "PegasusBoots" } },

        new object[] { "Desert Palace - Compass Chest", false, new string[] {  } },
        new object[] { "Desert Palace - Compass Chest", true, new string[] { "MoonPearl", "PegasusBoots", "KeyP2" } },
        new object[] { "Desert Palace - Compass Chest", true, new string[] { "BookOfMudora", "MagicMirror", "PegasusBoots", "KeyP2" } },

        new object[] { "Desert Palace - Big Key Chest", false, new string[] {  } },
        new object[] { "Desert Palace - Big Key Chest", true, new string[] { "MoonPearl", "PegasusBoots", "KeyP2" } },
        new object[] { "Desert Palace - Big Key Chest", true, new string[] { "BookOfMudora", "MagicMirror", "PegasusBoots", "KeyP2" } },

        new object[] { "Desert Palace - Boss", false, new string[] {  } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "BigKeyP2", "MoonPearl", "PegasusBoots", "Lamp" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "BigKeyP2", "MoonPearl", "PegasusBoots", "FireRod" } },
    };
}
