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

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Hype Cave - Top", false, new string[] {  } },
        new object[] { "Hype Cave - Top", false, new string[] { "PegasusBoots" } },
        new object[] { "Hype Cave - Top", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Hype Cave - Middle Right", false, new string[] {  } },
        new object[] { "Hype Cave - Middle Right", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Hype Cave - Middle Left", false, new string[] {  } },
        new object[] { "Hype Cave - Middle Left", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Hype Cave - Bottom", false, new string[] {  } },
        new object[] { "Hype Cave - Bottom", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Hype Cave - NPC Item", false, new string[] {  } },
        new object[] { "Hype Cave - NPC Item", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Stumpy", false, new string[] {  } },
        new object[] { "Stumpy", true, new string[] { "MoonPearl", "PegasusBoots" } },

        new object[] { "Digging Game", false, new string[] {  } },
        new object[] { "Digging Game", true, new string[] { "MoonPearl", "PegasusBoots" } },
    };
}
