namespace RandomizerTests.Logic.Open.NoGlitches.DarkWorld.DeathMountain;

[TestClass]
public class EastTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Superbunny Cave - Top", false, new string[] {  } },
        new object[] { "Superbunny Cave - Top", false, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Top", false, new string[] { "MoonPearl", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "Hammer", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "MoonPearl", "TitansMitt", "MagicMirror", "Hammer", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "Lamp" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "Lamp" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "Hammer", "Lamp" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "MoonPearl", "TitansMitt", "MagicMirror", "Hammer", "Lamp" } },

        new object[] { "Superbunny Cave - Bottom", false, new string[] {  } },
        new object[] { "Superbunny Cave - Bottom", false, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Bottom", false, new string[] { "MoonPearl", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "Hammer", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "TitansMitt", "MagicMirror", "Hammer", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "Lamp" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "Lamp" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "Hammer", "Lamp" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "TitansMitt", "MagicMirror", "Hammer", "Lamp" } },

        new object[] { "Hookshot Cave - Bottom Chest", false, new string[] {  } },
        new object[] { "Hookshot Cave - Bottom Chest", false, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Bottom Chest", false, new string[] { "MoonPearl", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Bottom Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Bottom Chest", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Bottom Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "Hammer", "OcarinaActive", "PegasusBoots" } },
        new object[] { "Hookshot Cave - Bottom Chest", true, new string[] { "MoonPearl", "TitansMitt", "MagicMirror", "Hammer", "OcarinaActive", "PegasusBoots" } },
        new object[] { "Hookshot Cave - Bottom Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "Lamp" } },
        new object[] { "Hookshot Cave - Bottom Chest", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "Lamp" } },
        new object[] { "Hookshot Cave - Bottom Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "Hammer", "Lamp", "PegasusBoots" } },
        new object[] { "Hookshot Cave - Bottom Chest", true, new string[] { "MoonPearl", "TitansMitt", "MagicMirror", "Hammer", "Lamp", "PegasusBoots" } },

        new object[] { "Hookshot Cave - Middle South Chest", false, new string[] {  } },
        new object[] { "Hookshot Cave - Middle South Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Middle South Chest", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Middle South Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "Lamp" } },
        new object[] { "Hookshot Cave - Middle South Chest", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "Lamp" } },

        new object[] { "Hookshot Cave - Middle North Chest", false, new string[] {  } },
        new object[] { "Hookshot Cave - Middle North Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Middle North Chest", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Middle North Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "Lamp" } },
        new object[] { "Hookshot Cave - Middle North Chest", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "Lamp" } },

        new object[] { "Hookshot Cave - Top Chest", false, new string[] {  } },
        new object[] { "Hookshot Cave - Top Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Top Chest", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Top Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "Lamp" } },
        new object[] { "Hookshot Cave - Top Chest", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "Lamp" } },
    };

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
