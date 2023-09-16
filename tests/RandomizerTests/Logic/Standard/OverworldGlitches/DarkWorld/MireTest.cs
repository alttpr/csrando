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

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Mire Shed - Left", false, new string[] {  } },
        new object[] { "Mire Shed - Left", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Mire Shed - Left", true, new string[] { "MagicMirror", "Flute", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Mire Shed - Left", true, new string[] { "MagicMirror", "Flute", "TitansMitt" } },

        new object[] { "Mire Shed - Right", false, new string[] {  } },
        new object[] { "Mire Shed - Right", true, new string[] { "MoonPearl", "PegasusBoots" } },
        new object[] { "Mire Shed - Right", true, new string[] { "MagicMirror", "Flute", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Mire Shed - Right", true, new string[] { "MagicMirror", "Flute", "TitansMitt" } },
    };
}
