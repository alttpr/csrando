namespace RandomizerTests.Logic.Alttp.Inverted.OverworldGlitches.DarkWorld.DeathMountain;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Inverted.OverworldGlitches;

[TestClass]
public sealed class WestTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Spike Cave", false, new string[] {  }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "PegasusBoots", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "PowerGlove", "PegasusBoots", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "TitansMitt", "PegasusBoots", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "PegasusBoots", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "PowerGlove", "PegasusBoots", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "TitansMitt", "PegasusBoots", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "ProgressiveGlove", "PegasusBoots", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "PowerGlove", "PegasusBoots", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "TitansMitt", "PegasusBoots", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "ProgressiveGlove", "PegasusBoots", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "PowerGlove", "PegasusBoots", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "TitansMitt", "PegasusBoots", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "ProgressiveGlove", "PegasusBoots", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "PowerGlove", "PegasusBoots", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "TitansMitt", "PegasusBoots", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "ProgressiveGlove", "PegasusBoots", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "PowerGlove", "PegasusBoots", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "TitansMitt", "PegasusBoots", "CaneOfByrna" }],
    ];
}
