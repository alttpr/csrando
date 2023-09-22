namespace RandomizerTests.Logic.Inverted.MajorGlitches.DarkWorld.DeathMountain;

[TestClass]
public sealed class EastTest : InvertedMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Superbunny Cave - Top", true, new string[] {  } },

        new object[] { "Superbunny Cave - Bottom", true, new string[] {  } },

        new object[] { "Hookshot Cave - Bottom Right", false, new string[] {  } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "PegasusBoots" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "Hookshot" } },

        new object[] { "Hookshot Cave - Bottom Left", false, new string[] {  } },
        new object[] { "Hookshot Cave - Bottom Left", true, new string[] { "Hookshot" } },

        new object[] { "Hookshot Cave - Top Left", false, new string[] {  } },
        new object[] { "Hookshot Cave - Top Left", true, new string[] { "Hookshot" } },

        new object[] { "Hookshot Cave - Top Right", false, new string[] {  } },
        new object[] { "Hookshot Cave - Top Right", true, new string[] { "Hookshot" } },
    };
}
