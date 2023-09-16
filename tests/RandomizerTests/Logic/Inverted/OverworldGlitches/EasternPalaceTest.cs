namespace RandomizerTests.Logic.Inverted.OverworldGlitches;

[TestClass]
public sealed class EasternPalaceTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Eastern Palace - Compass Chest", false, new string[] {  } },
        new object[] { "Eastern Palace - Compass Chest", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Eastern Palace - Compass Chest", true, new string[] { "MagicMirror", "PegasusBoots" } },
        new object[] { "Eastern Palace - Compass Chest", true, new string[] { "DefeatAgahnim" } },

        new object[] { "Eastern Palace - Cannonball Chest", false, new string[] {  } },
        new object[] { "Eastern Palace - Cannonball Chest", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Eastern Palace - Cannonball Chest", true, new string[] { "MagicMirror", "PegasusBoots" } },
        new object[] { "Eastern Palace - Cannonball Chest", true, new string[] { "DefeatAgahnim" } },

        new object[] { "Eastern Palace - Big Chest", false, new string[] {  } },
        new object[] { "Eastern Palace - Big Chest", true, new string[] { "BigKeyP1", "MoonPearl", "PegasusBoots" } },
        new object[] { "Eastern Palace - Big Chest", true, new string[] { "BigKeyP1", "MagicMirror", "PegasusBoots" } },
        new object[] { "Eastern Palace - Big Chest", true, new string[] { "BigKeyP1", "DefeatAgahnim" } },

        new object[] { "Eastern Palace - Map Chest", false, new string[] {  } },
        new object[] { "Eastern Palace - Map Chest", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Eastern Palace - Map Chest", true, new string[] { "MagicMirror", "PegasusBoots" } },
        new object[] { "Eastern Palace - Map Chest", true, new string[] { "DefeatAgahnim" } },

        new object[] { "Eastern Palace - Big Key Chest", false, new string[] {  } },
        new object[] { "Eastern Palace - Big Key Chest", true, new string[] { "Lamp", "MoonPearl", "PegasusBoots" } },
        new object[] { "Eastern Palace - Big Key Chest", true, new string[] { "Lamp", "MagicMirror", "PegasusBoots" } },
        new object[] { "Eastern Palace - Big Key Chest", true, new string[] { "Lamp", "DefeatAgahnim" } },

        new object[] { "Eastern Palace - Boss", false, new string[] {  } },
        new object[] { "Eastern Palace - Boss", true, new string[] { "Lamp", "BowAndArrows", "BigKeyP1", "MoonPearl", "PegasusBoots" } },
        new object[] { "Eastern Palace - Boss", true, new string[] { "Lamp", "BowAndArrows", "BigKeyP1", "MagicMirror", "PegasusBoots" } },
        new object[] { "Eastern Palace - Boss", true, new string[] { "Lamp", "BowAndArrows", "BigKeyP1", "DefeatAgahnim" } },
    };
}
