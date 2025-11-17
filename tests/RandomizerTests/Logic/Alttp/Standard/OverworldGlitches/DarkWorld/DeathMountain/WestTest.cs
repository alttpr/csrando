namespace RandomizerTests.Logic.Alttp.Standard.OverworldGlitches.DarkWorld.DeathMountain;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Standard.OverworldGlitches;

[TestClass]
public sealed class WestTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Spike Cave", false, new string[] {  }],
        ["Spike Cave", false, new string[] { "Bottle", "MoonPearl", "Hammer", "PegasusBoots", "Cape" }],
        ["Spike Cave", false, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "PegasusBoots", "Cape" }],
        ["Spike Cave", false, new string[] { "Bottle", "MoonPearl", "ProgressiveGlove", "PegasusBoots", "Cape" }],
        ["Spike Cave", false, new string[] { "Bottle", "MoonPearl", "Hammer", "ProgressiveGlove", "PegasusBoots" }],
        ["Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "ProgressiveGlove", "PegasusBoots", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "PowerGlove", "PegasusBoots", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "TitansMitt", "PegasusBoots", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "ProgressiveGlove", "PegasusBoots", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "PowerGlove", "PegasusBoots", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "TitansMitt", "PegasusBoots", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "PegasusBoots", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "PowerGlove", "PegasusBoots", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "TitansMitt", "PegasusBoots", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "PegasusBoots", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "PowerGlove", "PegasusBoots", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "TitansMitt", "PegasusBoots", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "PegasusBoots", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "PowerGlove", "PegasusBoots", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "TitansMitt", "PegasusBoots", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "PegasusBoots", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "PowerGlove", "PegasusBoots", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "TitansMitt", "PegasusBoots", "CaneOfByrna" }],
    ];
}
