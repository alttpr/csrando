namespace RandomizerTests.Logic.Alttp.Inverted.NoGlitches.DarkWorld;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Inverted.NoGlitches;

[TestClass]
public class MireTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Mire Shed - Left", false, new string[] {  }],
        //new object[] { "Mire Shed - Left", false, [], new string[] { "OcarinaInactive", "MagicMirror" } },
        ["Mire Shed - Left", true, new string[] { "MoonPearl", "OcarinaInactive", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Mire Shed - Left", true, new string[] { "MoonPearl", "OcarinaInactive", "ProgressiveGlove", "Hammer" }],
        ["Mire Shed - Left", true, new string[] { "MoonPearl", "OcarinaInactive", "AgahnimDefeated" }],
        ["Mire Shed - Left", true, new string[] { "MagicMirror", "AgahnimDefeated" }],

        ["Mire Shed - Right", false, new string[] {  }],
        //new object[] { "Mire Shed - Right", false, [], new string[] { "OcarinaInactive", "MagicMirror" } },
        ["Mire Shed - Right", true, new string[] { "MoonPearl", "OcarinaInactive", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Mire Shed - Right", true, new string[] { "MoonPearl", "OcarinaInactive", "ProgressiveGlove", "Hammer" }],
        ["Mire Shed - Right", true, new string[] { "MoonPearl", "OcarinaInactive", "AgahnimDefeated" }],
        ["Mire Shed - Right", true, new string[] { "MagicMirror", "AgahnimDefeated" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
