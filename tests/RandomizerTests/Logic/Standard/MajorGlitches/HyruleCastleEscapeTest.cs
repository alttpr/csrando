namespace RandomizerTests.Logic.Standard.MajorGlitches;

[TestClass]
public sealed class HyruleCastleEscapeTest : StandardMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Sanctuary", true, new string[] { "UncleSword", "KeyH2" }],

        ["Sewers - Secret Room - Left", true, new string[] { "UncleSword", "KeyH2" }],

        ["Sewers - Secret Room - Middle", true, new string[] { "UncleSword", "KeyH2" }],

        ["Sewers - Secret Room - Right", true, new string[] { "UncleSword", "KeyH2" }],

        ["Sewers - Dark Cross", true, new string[] { "UncleSword" }],

        ["Hyrule Castle - Boomerang Chest", true, new string[] { "UncleSword" }],

        ["Hyrule Castle - Map Chest", true, new string[] { "UncleSword" }],

        ["Hyrule Castle - Zelda's Cell", true, new string[] { "UncleSword" }],
    ];
}
