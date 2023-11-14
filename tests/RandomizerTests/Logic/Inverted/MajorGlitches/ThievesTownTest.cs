namespace RandomizerTests.Logic.Inverted.MajorGlitches;

[TestClass]
public sealed class ThievesTownTest : InvertedMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Thieves' Town - Attic", false, new string[] {  }],
        ["Thieves' Town - Attic", true, new string[] { "BigKeyD4", "KeyD4" }],

        ["Thieves' Town - Big Key Chest", true, new string[] {  }],

        ["Thieves' Town - Map Chest", true, new string[] {  }],

        ["Thieves' Town - Compass Chest", true, new string[] {  }],

        ["Thieves' Town - Ambush Chest", true, new string[] {  }],

        ["Thieves' Town - Big Chest", false, new string[] {  }],
        ["Thieves' Town - Big Chest", true, new string[] { "Hammer", "KeyD4", "BigKeyD4" }],

        ["Thieves' Town - Blind's Cell", false, new string[] {  }],
        ["Thieves' Town - Blind's Cell", true, new string[] { "BigKeyD4" }],

        ["Thieves' Town - Boss", false, new string[] {  }],
        ["Thieves' Town - Boss", true, new string[] { "KeyD4", "BigKeyD4" }],
    ];
}
