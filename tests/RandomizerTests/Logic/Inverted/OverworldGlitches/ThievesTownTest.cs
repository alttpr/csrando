namespace RandomizerTests.Logic.Inverted.OverworldGlitches;

[TestClass]
public sealed class ThievesTownTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Thieves' Town - Attic", false, new string[] {  } },
        new object[] { "Thieves' Town - Attic", true, new string[] { "BigKeyD4", "KeyD4" } },

        new object[] { "Thieves' Town - Big Key Chest", true, new string[] {  } },

        new object[] { "Thieves' Town - Map Chest", true, new string[] {  } },

        new object[] { "Thieves' Town - Compass Chest", true, new string[] {  } },

        new object[] { "Thieves' Town - Ambush Chest", true, new string[] {  } },

        new object[] { "Thieves' Town - Big Chest", false, new string[] {  } },
        new object[] { "Thieves' Town - Big Chest", true, new string[] { "Hammer", "KeyD4", "BigKeyD4" } },

        new object[] { "Thieves' Town - Blind's Cell", false, new string[] {  } },
        new object[] { "Thieves' Town - Blind's Cell", true, new string[] { "BigKeyD4" } },

        new object[] { "Thieves' Town - Boss", false, new string[] {  } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "KeyD4", "BigKeyD4" } },
    };
}
