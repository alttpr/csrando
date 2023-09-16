namespace RandomizerTests.Logic.Standard.OverworldGlitches.DarkWorld.DeathMountain;

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
        new object[] { "Superbunny Cave - Top", false, new string[] {  } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "PegasusBoots" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "TitansMitt", "PegasusBoots" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "Hammer", "PegasusBoots" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Superbunny Cave - Bottom", false, new string[] {  } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "PegasusBoots" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "TitansMitt", "PegasusBoots" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "Hammer", "PegasusBoots" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Hookshot Cave - Bottom Right", false, new string[] {  } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Hookshot Cave - Bottom Left", false, new string[] {  } },
        new object[] { "Hookshot Cave - Bottom Left", true, new string[] { "MoonPearl", "PegasusBoots", "Hookshot" } },

        new object[] { "Hookshot Cave - Top Left", false, new string[] {  } },
        new object[] { "Hookshot Cave - Top Left", true, new string[] { "MoonPearl", "PegasusBoots", "Hookshot" } },

        new object[] { "Hookshot Cave - Top Right", false, new string[] {  } },
        new object[] { "Hookshot Cave - Top Right", true, new string[] { "MoonPearl", "PegasusBoots", "Hookshot" } },
    };
}
