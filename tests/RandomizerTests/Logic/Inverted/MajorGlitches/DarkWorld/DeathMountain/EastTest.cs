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

    public static IEnumerable<object[]> TestData => [
        ["Superbunny Cave - Top", true, new string[] {  }],

        ["Superbunny Cave - Bottom", true, new string[] {  }],

        ["Hookshot Cave - Bottom Chest", false, new string[] {  }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "PegasusBoots" }],
        ["Hookshot Cave - Bottom Chest", true, new string[] { "Hookshot" }],

        ["Hookshot Cave - Middle South Chest", false, new string[] {  }],
        ["Hookshot Cave - Middle South Chest", true, new string[] { "Hookshot" }],

        ["Hookshot Cave - Middle North Chest", false, new string[] {  }],
        ["Hookshot Cave - Middle North Chest", true, new string[] { "Hookshot" }],

        ["Hookshot Cave - Top Chest", false, new string[] {  }],
        ["Hookshot Cave - Top Chest", true, new string[] { "Hookshot" }],
    ];
}
