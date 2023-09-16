namespace RandomizerTests.Logic.Standard.MajorGlitches.DeathMountain;

[TestClass]
public sealed class WestTest : StandardMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Ether Tablet", false, new string[] {  } },
        new object[] { "Ether Tablet", true, new string[] { "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "BookOfMudora", "L4Sword" } },

        new object[] { "Old Man", false, new string[] {  } },
        new object[] { "Old Man", true, new string[] { "Lamp" } },

        new object[] { "Spectacle Rock Cave", true, new string[] {  } },

        new object[] { "Spectacle Rock", true, new string[] {  } },
    };
}
