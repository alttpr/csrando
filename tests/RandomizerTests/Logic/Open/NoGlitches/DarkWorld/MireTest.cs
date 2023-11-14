namespace RandomizerTests.Logic.Open.NoGlitches.DarkWorld;

[TestClass]
public class MireTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Mire Shed - Left", false, new string[] {  }],
        ["Mire Shed - Left", false, new string[] { "OcarinaActive", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Mire Shed - Left", false, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Mire Shed - Left", false, new string[] { "MoonPearl", "OcarinaActive", "ProgressiveGlove" }],
        ["Mire Shed - Left", true, new string[] { "MoonPearl", "OcarinaActive", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Mire Shed - Left", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt" }],

        ["Mire Shed - Right", false, new string[] {  }],
        ["Mire Shed - Right", false, new string[] { "OcarinaActive", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Mire Shed - Right", false, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Mire Shed - Right", false, new string[] { "MoonPearl", "OcarinaActive", "ProgressiveGlove" }],
        ["Mire Shed - Right", true, new string[] { "MoonPearl", "OcarinaActive", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Mire Shed - Right", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
