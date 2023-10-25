namespace RandomizerTests.Logic.Inverted.OverworldGlitches.DarkWorld.DeathMountain;

[TestClass]
public sealed class EastTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Superbunny Cave - Top", false, new string[] {  } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "PegasusBoots" } },

        new object[] { "Superbunny Cave - Bottom", false, new string[] {  } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "PegasusBoots" } },

        new object[] { "Hookshot Cave - Bottom Chest", false, new string[] {  } },
        new object[] { "Hookshot Cave - Bottom Chest", true, new string[] { "PegasusBoots" } },

        new object[] { "Hookshot Cave - Middle South Chest", false, new string[] {  } },
        new object[] { "Hookshot Cave - Middle South Chest", true, new string[] { "PegasusBoots", "Hookshot" } },

        new object[] { "Hookshot Cave - Middle North Chest", false, new string[] {  } },
        new object[] { "Hookshot Cave - Middle North Chest", true, new string[] { "PegasusBoots", "Hookshot" } },

        new object[] { "Hookshot Cave - Top Chest", false, new string[] {  } },
        new object[] { "Hookshot Cave - Top Chest", true, new string[] { "PegasusBoots", "Hookshot" } },
    };
}
