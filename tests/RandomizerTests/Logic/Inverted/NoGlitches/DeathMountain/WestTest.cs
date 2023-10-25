namespace RandomizerTests.Logic.Inverted.NoGlitches.DeathMountain;

[TestClass]
public class WestTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Old Man", false, new string[] {  } },
        new object[] { "Old Man", false, new string[] { "ProgressiveGlove" } },
        new object[] { "Old Man", true, new string[] { "ProgressiveGlove", "Lamp" } },
        new object[] { "Old Man", true, new string[] { "PowerGlove", "Lamp" } },
        new object[] { "Old Man", true, new string[] { "TitansMitt", "Lamp" } },
        new object[] { "Old Man", true, new string[] { "OcarinaActive", "Lamp" } },

        new object[] { "Spectacle Rock Cave Item", false, new string[] {  } },
        new object[] { "Spectacle Rock Cave Item", false, new string[] { "OcarinaInactive", "ProgressiveGlove", "Hammer" } },
        new object[] { "Spectacle Rock Cave Item", false, new string[] { "OcarinaInactive", "PowerGlove", "Hammer" } },
        new object[] { "Spectacle Rock Cave Item", false, new string[] { "OcarinaInactive", "ProgressiveGlove", "ProgressiveGlove", "Hammer" } },
        new object[] { "Spectacle Rock Cave Item", false, new string[] { "OcarinaInactive", "TitansMitt" } },
        new object[] { "Spectacle Rock Cave Item", true, new string[] { "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Spectacle Rock Cave Item", true, new string[] { "OcarinaInactive", "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Spectacle Rock Cave Item", true, new string[] { "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hammer" } },
        new object[] { "Spectacle Rock Cave Item", true, new string[] { "OcarinaInactive", "MoonPearl", "TitansMitt" } },
        new object[] { "Spectacle Rock Cave Item", true, new string[] { "ProgressiveGlove", "Lamp" } },
        new object[] { "Spectacle Rock Cave Item", true, new string[] { "PowerGlove", "Lamp" } },
        new object[] { "Spectacle Rock Cave Item", true, new string[] { "TitansMitt", "Lamp" } },
    };

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
