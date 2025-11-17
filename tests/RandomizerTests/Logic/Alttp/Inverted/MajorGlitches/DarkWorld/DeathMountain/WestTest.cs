namespace RandomizerTests.Logic.Alttp.Inverted.MajorGlitches.DarkWorld.DeathMountain;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Inverted.MajorGlitches;

[TestClass]
public sealed class WestTest : InvertedMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Spike Cave", false, new string[] {  }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "PowerGlove", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "TitansMitt", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "PowerGlove", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "TitansMitt", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "ProgressiveGlove", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "PowerGlove", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "TitansMitt", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "ProgressiveGlove", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "PowerGlove", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "TitansMitt", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "ProgressiveGlove", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "PowerGlove", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "TitansMitt", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "ProgressiveGlove", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "PowerGlove", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "TitansMitt", "CaneOfByrna" }],
    ];
}
