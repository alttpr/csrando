namespace RandomizerTests.Logic.Alttp.Inverted.OverworldGlitches.DarkWorld;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Inverted.OverworldGlitches;

[TestClass]
public sealed class MireTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Mire Shed - Left", false, new string[] {  }],
        ["Mire Shed - Left", true, new string[] { "PegasusBoots" }],

        ["Mire Shed - Right", false, new string[] {  }],
        ["Mire Shed - Right", true, new string[] { "PegasusBoots" }],
    ];
}
