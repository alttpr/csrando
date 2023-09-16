namespace RandomizerTests.Logic.Standard.OverworldGlitches.DeathMountain;

[TestClass]
public sealed class WestTest : StandardOverworldGlitchesLogicTests
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
        new object[] { "Ether Tablet", false, new string[] { "PegasusBoots", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", false, new string[] { "PegasusBoots", "BookOfMudora", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "PegasusBoots", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "PegasusBoots", "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "PegasusBoots", "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "PegasusBoots", "BookOfMudora", "L4Sword" } },

        new object[] { "Old Man", false, new string[] {  } },
        new object[] { "Old Man", false, new string[] { "PegasusBoots" } },
        new object[] { "Old Man", true, new string[] { "PegasusBoots", "Lamp" } },

        new object[] { "Spectacle Rock Cave", false, new string[] {  } },
        new object[] { "Spectacle Rock Cave", true, new string[] { "PegasusBoots" } },

        new object[] { "Spectacle Rock", false, new string[] {  } },
        new object[] { "Spectacle Rock", true, new string[] { "PegasusBoots" } },
    };
}
