namespace RandomizerTests.Logic.Inverted.NoGlitches.DarkWorld;

[TestClass]
public class SouthTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Hype Cave - Top", true, new string[] {  } },

        new object[] { "Hype Cave - Middle Right", true, new string[] {  } },

        new object[] { "Hype Cave - Middle Left", true, new string[] {  } },

        new object[] { "Hype Cave - Bottom", true, new string[] {  } },

        new object[] { "Hype Cave - NPC", true, new string[] {  } },

        new object[] { "Stumpy", true, new string[] {  } },

        new object[] { "Digging Game - Item", true, new string[] {  } },

        new object[] { "Link's House - Chest", true, new string[] {  } },
    };

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
