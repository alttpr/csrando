namespace RandomizerTests.Logic.Standard.OverworldGlitches.DarkWorld;

[TestClass]
public sealed class MireTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Mire Shed - Left", false, new string[] {  }],
        ["Mire Shed - Left", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Mire Shed - Left", true, new string[] { "MagicMirror", "Flute", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Mire Shed - Left", true, new string[] { "MagicMirror", "Flute", "TitansMitt" }],

        ["Mire Shed - Right", false, new string[] {  }],
        ["Mire Shed - Right", true, new string[] { "MoonPearl", "PegasusBoots" }],
        ["Mire Shed - Right", true, new string[] { "MagicMirror", "Flute", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Mire Shed - Right", true, new string[] { "MagicMirror", "Flute", "TitansMitt" }],
    ];
}
