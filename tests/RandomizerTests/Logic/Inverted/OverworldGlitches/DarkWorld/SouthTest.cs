namespace RandomizerTests.Logic.Inverted.OverworldGlitches.DarkWorld;

[TestClass]
public sealed class SouthTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Hype Cave - Top", true, new string[] {  } },

        new object[] { "Hype Cave - Middle Right", true, new string[] {  } },

        new object[] { "Hype Cave - Middle Left", true, new string[] {  } },

        new object[] { "Hype Cave - Bottom", true, new string[] {  } },

        new object[] { "Hype Cave - NPC Item", true, new string[] {  } },

        new object[] { "Stumpy", true, new string[] {  } },

        new object[] { "Digging Game", true, new string[] {  } },
    };
}
