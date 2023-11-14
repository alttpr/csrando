namespace RandomizerTests.Logic.Standard.OverworldGlitches.DarkWorld;

[TestClass]
public sealed class SouthTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Hype Cave - Top", false, new string[] {  }],
        ["Hype Cave - Top", false, new string[] { "PegasusBoots" }],
        ["Hype Cave - Top", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Hype Cave - Middle Right", false, new string[] {  }],
        ["Hype Cave - Middle Right", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Hype Cave - Middle Left", false, new string[] {  }],
        ["Hype Cave - Middle Left", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Hype Cave - Bottom", false, new string[] {  }],
        ["Hype Cave - Bottom", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Hype Cave - NPC Item", false, new string[] {  }],
        ["Hype Cave - NPC Item", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Stumpy", false, new string[] {  }],
        ["Stumpy", true, new string[] { "MoonPearl", "PegasusBoots" }],

        ["Digging Game", false, new string[] {  }],
        ["Digging Game", true, new string[] { "MoonPearl", "PegasusBoots" }],
    ];
}
