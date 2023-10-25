namespace RandomizerTests.Logic.Standard.OverworldGlitches.DeathMountain;

[TestClass]
public sealed class EastTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Spiral Cave Item", false, new string[] {  } },
        new object[] { "Spiral Cave Item", true, new string[] { "PegasusBoots" } },

        new object[] { "Paradox Cave Lower - Far Left", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "PegasusBoots" } },

        new object[] { "Paradox Cave Lower - Left", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "PegasusBoots" } },

        new object[] { "Paradox Cave Lower - Middle", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "PegasusBoots" } },

        new object[] { "Paradox Cave Lower - Right", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "PegasusBoots" } },

        new object[] { "Paradox Cave Lower - Far Right", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "PegasusBoots" } },

        new object[] { "Paradox Cave Upper - Left", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "PegasusBoots" } },

        new object[] { "Paradox Cave Upper - Right", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "PegasusBoots" } },

        new object[] { "Mimic Cave", false, new string[] {  } },
        new object[] { "Mimic Cave", false, new string[] { "Hammer", "PegasusBoots" } },
        new object[] { "Mimic Cave", false, new string[] { "MagicMirror", "PegasusBoots" } },
        new object[] { "Mimic Cave", true, new string[] { "MagicMirror", "Hammer", "PegasusBoots" } },
        new object[] { "Mimic Cave", true, new string[] { "MagicMirror", "Hammer", "ProgressiveGlove", "Lamp" } },
        new object[] { "Mimic Cave", true, new string[] { "MagicMirror", "Hammer", "PowerGlove", "Lamp" } },
        new object[] { "Mimic Cave", true, new string[] { "MagicMirror", "Hammer", "TitansMitt", "Lamp" } },
        new object[] { "Mimic Cave", true, new string[] { "MagicMirror", "Hammer", "Flute" } },
    };
}
