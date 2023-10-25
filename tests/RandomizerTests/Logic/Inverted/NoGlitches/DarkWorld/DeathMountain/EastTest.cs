namespace RandomizerTests.Logic.Inverted.NoGlitches.DarkWorld.DeathMountain;

[TestClass]
public class EastTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Superbunny Cave - Top", false, new string[] {  } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "ProgressiveGlove", "Lamp" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "Hammer", "ProgressiveGlove", "MoonPearl", "OcarinaInactive" } },

        new object[] { "Superbunny Cave - Bottom", false, new string[] {  } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "ProgressiveGlove", "Lamp" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "Hammer", "ProgressiveGlove", "MoonPearl", "OcarinaInactive" } },

        new object[] { "Hookshot Cave - Bottom Chest", false, new string[] {  } },
        new object[] { "Hookshot Cave - Bottom Chest", true, new string[] { "ProgressiveGlove", "Lamp", "PegasusBoots" } },
        new object[] { "Hookshot Cave - Bottom Chest", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive", "PegasusBoots" } },
        new object[] { "Hookshot Cave - Bottom Chest", true, new string[] { "ProgressiveGlove", "Hammer", "MoonPearl", "OcarinaInactive", "PegasusBoots" } },
        new object[] { "Hookshot Cave - Bottom Chest", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" } },
        new object[] { "Hookshot Cave - Bottom Chest", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive", "Hookshot" } },
        new object[] { "Hookshot Cave - Bottom Chest", true, new string[] { "ProgressiveGlove", "Hammer", "MoonPearl", "OcarinaInactive", "Hookshot" } },

        new object[] { "Hookshot Cave - Middle South Chest", false, new string[] {  } },
        new object[] { "Hookshot Cave - Middle South Chest", false, new string[] { "ProgressiveGlove", "Lamp" } },
        new object[] { "Hookshot Cave - Middle South Chest", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" } },
        new object[] { "Hookshot Cave - Middle South Chest", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive", "Hookshot" } },
        new object[] { "Hookshot Cave - Middle South Chest", true, new string[] { "ProgressiveGlove", "Hammer", "MoonPearl", "OcarinaInactive", "Hookshot" } },

        new object[] { "Hookshot Cave - Middle North Chest", false, new string[] {  } },
        new object[] { "Hookshot Cave - Middle North Chest", false, new string[] { "ProgressiveGlove", "Lamp" } },
        new object[] { "Hookshot Cave - Middle North Chest", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" } },
        new object[] { "Hookshot Cave - Middle North Chest", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive", "Hookshot" } },
        new object[] { "Hookshot Cave - Middle North Chest", true, new string[] { "ProgressiveGlove", "Hammer", "MoonPearl", "OcarinaInactive", "Hookshot" } },

        new object[] { "Hookshot Cave - Top Chest", false, new string[] {  } },
        new object[] { "Hookshot Cave - Top Chest", false, new string[] { "ProgressiveGlove", "Lamp" } },
        new object[] { "Hookshot Cave - Top Chest", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" } },
        new object[] { "Hookshot Cave - Top Chest", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive", "Hookshot" } },
        new object[] { "Hookshot Cave - Top Chest", true, new string[] { "ProgressiveGlove", "Hammer", "MoonPearl", "OcarinaInactive", "Hookshot" } },
    };

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
