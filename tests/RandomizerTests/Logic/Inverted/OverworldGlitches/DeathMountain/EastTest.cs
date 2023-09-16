namespace RandomizerTests.Logic.Inverted.OverworldGlitches.DeathMountain;

[TestClass]
public sealed class EastTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Spiral Cave", false, new string[] {  } },
        new object[] { "Spiral Cave", true, new string[] { "MagicMirror", "ProgressiveGlove", "ProgressiveGlove", "Lamp", "UncleSword" } },
        new object[] { "Spiral Cave", true, new string[] { "MagicMirror", "TitansMitt", "Lamp", "UncleSword" } },
        new object[] { "Spiral Cave", true, new string[] { "MagicMirror", "ProgressiveGlove", "ProgressiveGlove", "PegasusBoots", "UncleSword" } },
        new object[] { "Spiral Cave", true, new string[] { "MagicMirror", "TitansMitt", "PegasusBoots", "UncleSword" } },
        new object[] { "Spiral Cave", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Paradox Cave Lower - Far Left", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Paradox Cave Lower - Left", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Paradox Cave Lower - Middle", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Paradox Cave Lower - Right", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Paradox Cave Lower - Far Right", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Paradox Cave Upper - Left", false, new string[] {  } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Paradox Cave Upper - Right", false, new string[] {  } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Mimic Cave", false, new string[] {  } },
        new object[] { "Mimic Cave", true, new string[] { "MoonPearl", "Hammer", "PegasusBoots" } },

        new object[] { "Ether Tablet", false, new string[] {  } },
        new object[] { "Ether Tablet", true, new string[] { "PegasusBoots", "MoonPearl", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "PegasusBoots", "MoonPearl", "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "PegasusBoots", "MoonPearl", "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "PegasusBoots", "MoonPearl", "BookOfMudora", "L4Sword" } },

        new object[] { "Spectacle Rock", false, new string[] {  } },
        new object[] { "Spectacle Rock", true, new string[] { "MoonPearl", "PegasusBoots" } },
    };
}
