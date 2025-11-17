namespace RandomizerTests.Logic.Alttp.Standard.MajorGlitches.DeathMountain;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Standard.MajorGlitches;

[TestClass]
public sealed class WestTest : StandardMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Ether Tablet", false, new string[] {  }],
        ["Ether Tablet", true, new string[] { "BookOfMudora", "ProgressiveSword", "ProgressiveSword" }],
        ["Ether Tablet", true, new string[] { "BookOfMudora", "L2Sword" }],
        ["Ether Tablet", true, new string[] { "BookOfMudora", "L3Sword" }],
        ["Ether Tablet", true, new string[] { "BookOfMudora", "L4Sword" }],

        ["Old Man", false, new string[] {  }],
        ["Old Man", true, new string[] { "Lamp" }],

        ["Spectacle Rock Cave Item", true, new string[] {  }],

        ["Spectacle Rock", true, new string[] {  }],
    ];
}
