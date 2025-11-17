namespace RandomizerTests.Logic.Alttp.Inverted.OverworldGlitches.DarkWorld;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Inverted.OverworldGlitches;

[TestClass]
public sealed class SouthTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Hype Cave - Top", true, new string[] {  }],

        ["Hype Cave - Middle Right", true, new string[] {  }],

        ["Hype Cave - Middle Left", true, new string[] {  }],

        ["Hype Cave - Bottom", true, new string[] {  }],

        ["Hype Cave - NPC Item", true, new string[] {  }],

        ["Stumpy", true, new string[] {  }],

        ["Digging Game", true, new string[] {  }],
    ];
}
