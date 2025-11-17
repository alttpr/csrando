namespace RandomizerTests.Logic.Alttp.Standard.OverworldGlitches.DarkWorld.DeathMountain;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Standard.OverworldGlitches;

[TestClass]
public sealed class EastTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Superbunny Cave - Top", false, new string[] {  }],
        ["Superbunny Cave - Top", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "PegasusBoots" }],
        ["Superbunny Cave - Top", true, new string[] { "TitansMitt", "PegasusBoots" }],
        ["Superbunny Cave - Top", true, new string[] { "Hammer", "PegasusBoots" }],
        ["Superbunny Cave - Top", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Superbunny Cave - Bottom", false, new string[] {  }],
        ["Superbunny Cave - Bottom", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "PegasusBoots" }],
        ["Superbunny Cave - Bottom", true, new string[] { "TitansMitt", "PegasusBoots" }],
        ["Superbunny Cave - Bottom", true, new string[] { "Hammer", "PegasusBoots" }],
        ["Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Hookshot Cave - Bottom Chest", false, new string[] {  }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Hookshot Cave - Middle South Chest", false, new string[] {  }],
        ["Hookshot Cave - Middle South Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Hookshot" }],

        ["Hookshot Cave - Middle North Chest", false, new string[] {  }],
        ["Hookshot Cave - Middle North Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Hookshot" }],

        ["Hookshot Cave - Top Chest", false, new string[] {  }],
        ["Hookshot Cave - Top Chest", true, new string[] { "MoonPearl", "PegasusBoots", "Hookshot" }],
    ];
}
