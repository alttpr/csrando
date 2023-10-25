namespace RandomizerTests.Logic.Inverted.OverworldGlitches.DeathMountain;

[TestClass]
public sealed class WestTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Old Man", false, new string[] {  } },
        new object[] { "Old Man", true, new string[] { "PegasusBoots", "Lamp" } },

        new object[] { "Spectacle Rock Cave Item", false, new string[] {  } },
        new object[] { "Spectacle Rock Cave Item", true, new string[] { "PegasusBoots" } },
    };
}
