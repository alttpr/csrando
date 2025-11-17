namespace RandomizerTests.Logic.Alttp.Inverted.NoGlitches.DeathMountain;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Inverted.NoGlitches;

[TestClass]
public class WestTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Old Man", false, new string[] {  }],
        ["Old Man", false, new string[] { "ProgressiveGlove" }],
        ["Old Man", true, new string[] { "ProgressiveGlove", "Lamp" }],
        ["Old Man", true, new string[] { "PowerGlove", "Lamp" }],
        ["Old Man", true, new string[] { "TitansMitt", "Lamp" }],
        ["Old Man", true, new string[] { "OcarinaActive", "Lamp" }],

        ["Spectacle Rock Cave Item", false, new string[] {  }],
        ["Spectacle Rock Cave Item", false, new string[] { "OcarinaInactive", "ProgressiveGlove", "Hammer" }],
        ["Spectacle Rock Cave Item", false, new string[] { "OcarinaInactive", "PowerGlove", "Hammer" }],
        ["Spectacle Rock Cave Item", false, new string[] { "OcarinaInactive", "ProgressiveGlove", "ProgressiveGlove", "Hammer" }],
        ["Spectacle Rock Cave Item", false, new string[] { "OcarinaInactive", "TitansMitt" }],
        ["Spectacle Rock Cave Item", true, new string[] { "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Spectacle Rock Cave Item", true, new string[] { "OcarinaInactive", "MoonPearl", "PowerGlove", "Hammer" }],
        ["Spectacle Rock Cave Item", true, new string[] { "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hammer" }],
        ["Spectacle Rock Cave Item", true, new string[] { "OcarinaInactive", "MoonPearl", "TitansMitt" }],
        ["Spectacle Rock Cave Item", true, new string[] { "ProgressiveGlove", "Lamp" }],
        ["Spectacle Rock Cave Item", true, new string[] { "PowerGlove", "Lamp" }],
        ["Spectacle Rock Cave Item", true, new string[] { "TitansMitt", "Lamp" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
