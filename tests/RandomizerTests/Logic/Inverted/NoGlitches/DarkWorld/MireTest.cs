namespace RandomizerTests.Logic.Inverted.NoGlitches.DarkWorld;

[TestClass]
public class MireTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Mire Shed - Left", false, new string[] {  } },
        //new object[] { "Mire Shed - Left", false, [], new string[] { "OcarinaInactive", "MagicMirror" } },
        new object[] { "Mire Shed - Left", true, new string[] { "MoonPearl", "OcarinaInactive", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Mire Shed - Left", true, new string[] { "MoonPearl", "OcarinaInactive", "ProgressiveGlove", "Hammer" } },
        new object[] { "Mire Shed - Left", true, new string[] { "MoonPearl", "OcarinaInactive", "AgahnimDefeated" } },
        new object[] { "Mire Shed - Left", true, new string[] { "MagicMirror", "AgahnimDefeated" } },

        new object[] { "Mire Shed - Right", false, new string[] {  } },
        //new object[] { "Mire Shed - Right", false, [], new string[] { "OcarinaInactive", "MagicMirror" } },
        new object[] { "Mire Shed - Right", true, new string[] { "MoonPearl", "OcarinaInactive", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Mire Shed - Right", true, new string[] { "MoonPearl", "OcarinaInactive", "ProgressiveGlove", "Hammer" } },
        new object[] { "Mire Shed - Right", true, new string[] { "MoonPearl", "OcarinaInactive", "AgahnimDefeated" } },
        new object[] { "Mire Shed - Right", true, new string[] { "MagicMirror", "AgahnimDefeated" } },
    };

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
