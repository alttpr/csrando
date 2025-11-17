namespace RandomizerTests.Logic.Alttp.Standard.NoGlitches;

using RandomizerTests.Logic.Alttp;

[TestClass]
public class HyruleCastleEscapeTest : StandardNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Sanctuary Chest", true, new string[] { "UncleSword", "KeyH2" }],

        ["Sewers - Secret Room - Left", true, new string[] { "UncleSword", "KeyH2" }],

        ["Sewers - Secret Room - Middle", true, new string[] { "UncleSword", "KeyH2" }],

        ["Sewers - Secret Room - Right", true, new string[] { "UncleSword", "KeyH2" }],

        ["Sewers - Dark Cross Chest", true, new string[] { "UncleSword" }],

        ["Hyrule Castle - Boomerang Chest", true, new string[] { "UncleSword" }],

        ["Hyrule Castle - Map Chest", true, new string[] { "UncleSword" }],

        ["Hyrule Castle - Zelda's Cell", true, new string[] { "UncleSword" }],

        // Pyramid ledge needs to be gated behind rescued Zelda;
        // otherwise logic assumes you can walk through Sanctuary to get items to actually rescue her.
        ["Pyramid Ledge", false, new string[] { "MasterSword", "Lamp", "KeyA1", "KeyA1" }],
        // this should never happen in practise, but verifies our data is correct.
        ["Pyramid Ledge", true, new string[] { "MasterSword", "Lamp", "KeyA1", "KeyA1", "RescueZelda" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
