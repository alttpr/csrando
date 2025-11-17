namespace RandomizerTests.Logic.Alttp.Inverted.MajorGlitches.DeathMountain;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Inverted.MajorGlitches;

[TestClass]
public sealed class WestTest : InvertedMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Old Man", false, new string[] {  }],
        ["Old Man", true, new string[] { "Lamp" }],

        ["Spectacle Rock Cave Item", true, new string[] {  }],
    ];
}
