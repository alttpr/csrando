namespace RandomizerTests.Logic.Open.NoGlitches.DarkWorld;

[TestClass]
public class MireTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Mire Shed - Left", false, new string[] {  } },
        new object[] { "Mire Shed - Left", false, new string[] { "OcarinaActive", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Mire Shed - Left", false, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Mire Shed - Left", false, new string[] { "MoonPearl", "OcarinaActive", "ProgressiveGlove" } },
        new object[] { "Mire Shed - Left", true, new string[] { "MoonPearl", "OcarinaActive", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Mire Shed - Left", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt" } },

        new object[] { "Mire Shed - Right", false, new string[] {  } },
        new object[] { "Mire Shed - Right", false, new string[] { "OcarinaActive", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Mire Shed - Right", false, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Mire Shed - Right", false, new string[] { "MoonPearl", "OcarinaActive", "ProgressiveGlove" } },
        new object[] { "Mire Shed - Right", true, new string[] { "MoonPearl", "OcarinaActive", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Mire Shed - Right", true, new string[] { "MoonPearl", "OcarinaActive", "TitansMitt" } },
    };

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
