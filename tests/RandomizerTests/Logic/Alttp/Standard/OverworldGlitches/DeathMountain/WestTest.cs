namespace RandomizerTests.Logic.Alttp.Standard.OverworldGlitches.DeathMountain;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Standard.OverworldGlitches;

[TestClass]
public sealed class WestTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Ether Tablet", false, new string[] {  }],
        ["Ether Tablet", false, new string[] { "PegasusBoots", "ProgressiveSword", "ProgressiveSword" }],
        ["Ether Tablet", false, new string[] { "PegasusBoots", "BookOfMudora", "ProgressiveSword" }],
        ["Ether Tablet", true, new string[] { "PegasusBoots", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" }],
        ["Ether Tablet", true, new string[] { "PegasusBoots", "BookOfMudora", "L2Sword" }],
        ["Ether Tablet", true, new string[] { "PegasusBoots", "BookOfMudora", "L3Sword" }],
        ["Ether Tablet", true, new string[] { "PegasusBoots", "BookOfMudora", "L4Sword" }],

        ["Old Man", false, new string[] {  }],
        ["Old Man", false, new string[] { "PegasusBoots" }],
        ["Old Man", true, new string[] { "PegasusBoots", "Lamp" }],

        ["Spectacle Rock Cave Item", false, new string[] {  }],
        ["Spectacle Rock Cave Item", true, new string[] { "PegasusBoots" }],

        ["Spectacle Rock", false, new string[] {  }],
        ["Spectacle Rock", true, new string[] { "PegasusBoots" }],
    ];
}
