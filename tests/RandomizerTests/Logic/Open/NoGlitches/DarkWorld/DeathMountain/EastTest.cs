namespace RandomizerTests.Logic.Open.NoGlitches.DarkWorld.DeathMountain;

[TestClass]
public class EastTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Superbunny Cave - Top", false, new string[] {  }],
        ["Superbunny Cave - Top", false, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" }],
        ["Superbunny Cave - Top", false, new string[] { "MoonPearl", "ProgressiveGlove", "Hookshot", "OcarinaActive" }],
        ["Superbunny Cave - Top", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" }],
        ["Superbunny Cave - Top", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "OcarinaActive" }],
        ["Superbunny Cave - Top", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "Hammer", "OcarinaActive" }],
        ["Superbunny Cave - Top", true, new string[] { "MoonPearl", "TitansMitt", "MagicMirror", "Hammer", "OcarinaActive" }],
        ["Superbunny Cave - Top", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "Lamp" }],
        ["Superbunny Cave - Top", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "Lamp" }],
        ["Superbunny Cave - Top", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "Hammer", "Lamp" }],
        ["Superbunny Cave - Top", true, new string[] { "MoonPearl", "TitansMitt", "MagicMirror", "Hammer", "Lamp" }],

        ["Superbunny Cave - Bottom", false, new string[] {  }],
        ["Superbunny Cave - Bottom", false, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" }],
        ["Superbunny Cave - Bottom", false, new string[] { "MoonPearl", "ProgressiveGlove", "Hookshot", "OcarinaActive" }],
        ["Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" }],
        ["Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "OcarinaActive" }],
        ["Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "Hammer", "OcarinaActive" }],
        ["Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "TitansMitt", "MagicMirror", "Hammer", "OcarinaActive" }],
        ["Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "Lamp" }],
        ["Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "Lamp" }],
        ["Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "Hammer", "Lamp" }],
        ["Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "TitansMitt", "MagicMirror", "Hammer", "Lamp" }],

        ["Hookshot Cave - Bottom Chest", false, new string[] {  }],
        ["Hookshot Cave - Bottom Chest", false, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" }],
        ["Hookshot Cave - Bottom Chest", false, new string[] { "MoonPearl", "ProgressiveGlove", "Hookshot", "OcarinaActive" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "OcarinaActive" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "Hammer", "OcarinaActive", "PegasusBoots" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "MoonPearl", "TitansMitt", "MagicMirror", "Hammer", "OcarinaActive", "PegasusBoots" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "Lamp" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "Lamp" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "Hammer", "Lamp", "PegasusBoots" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "MoonPearl", "TitansMitt", "MagicMirror", "Hammer", "Lamp", "PegasusBoots" }],

        ["Hookshot Cave - Middle South Chest", false, new string[] {  }],
        ["Hookshot Cave - Middle South Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" }],
        ["Hookshot Cave - Middle South Chest", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "OcarinaActive" }],
        ["Hookshot Cave - Middle South Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "Lamp" }],
        ["Hookshot Cave - Middle South Chest", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "Lamp" }],

        ["Hookshot Cave - Middle North Chest", false, new string[] {  }],
        ["Hookshot Cave - Middle North Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" }],
        ["Hookshot Cave - Middle North Chest", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "OcarinaActive" }],
        ["Hookshot Cave - Middle North Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "Lamp" }],
        ["Hookshot Cave - Middle North Chest", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "Lamp" }],

        ["Hookshot Cave - Top Chest", false, new string[] {  }],
        ["Hookshot Cave - Top Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" }],
        ["Hookshot Cave - Top Chest", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "OcarinaActive" }],
        ["Hookshot Cave - Top Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "Lamp" }],
        ["Hookshot Cave - Top Chest", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "Lamp" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
