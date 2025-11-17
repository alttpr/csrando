namespace RandomizerTests.Logic.Alttp.Inverted.NoGlitches.DarkWorld.DeathMountain;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Inverted.NoGlitches;

[TestClass]
public class EastTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Superbunny Cave - Top", false, new string[] {  }],
        ["Superbunny Cave - Top", true, new string[] { "ProgressiveGlove", "Lamp" }],
        ["Superbunny Cave - Top", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive" }],
        ["Superbunny Cave - Top", true, new string[] { "Hammer", "ProgressiveGlove", "MoonPearl", "OcarinaInactive" }],

        ["Superbunny Cave - Bottom", false, new string[] {  }],
        ["Superbunny Cave - Bottom", true, new string[] { "ProgressiveGlove", "Lamp" }],
        ["Superbunny Cave - Bottom", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive" }],
        ["Superbunny Cave - Bottom", true, new string[] { "Hammer", "ProgressiveGlove", "MoonPearl", "OcarinaInactive" }],

        ["Hookshot Cave - Bottom Chest", false, new string[] {  }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "ProgressiveGlove", "Lamp", "PegasusBoots" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive", "PegasusBoots" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "ProgressiveGlove", "Hammer", "MoonPearl", "OcarinaInactive", "PegasusBoots" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive", "Hookshot" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "ProgressiveGlove", "Hammer", "MoonPearl", "OcarinaInactive", "Hookshot" }],

        ["Hookshot Cave - Middle South Chest", false, new string[] {  }],
        ["Hookshot Cave - Middle South Chest", false, new string[] { "ProgressiveGlove", "Lamp" }],
        ["Hookshot Cave - Middle South Chest", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" }],
        ["Hookshot Cave - Middle South Chest", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive", "Hookshot" }],
        ["Hookshot Cave - Middle South Chest", true, new string[] { "ProgressiveGlove", "Hammer", "MoonPearl", "OcarinaInactive", "Hookshot" }],

        ["Hookshot Cave - Middle North Chest", false, new string[] {  }],
        ["Hookshot Cave - Middle North Chest", false, new string[] { "ProgressiveGlove", "Lamp" }],
        ["Hookshot Cave - Middle North Chest", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" }],
        ["Hookshot Cave - Middle North Chest", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive", "Hookshot" }],
        ["Hookshot Cave - Middle North Chest", true, new string[] { "ProgressiveGlove", "Hammer", "MoonPearl", "OcarinaInactive", "Hookshot" }],

        ["Hookshot Cave - Top Chest", false, new string[] {  }],
        ["Hookshot Cave - Top Chest", false, new string[] { "ProgressiveGlove", "Lamp" }],
        ["Hookshot Cave - Top Chest", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" }],
        ["Hookshot Cave - Top Chest", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive", "Hookshot" }],
        ["Hookshot Cave - Top Chest", true, new string[] { "ProgressiveGlove", "Hammer", "MoonPearl", "OcarinaInactive", "Hookshot" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
