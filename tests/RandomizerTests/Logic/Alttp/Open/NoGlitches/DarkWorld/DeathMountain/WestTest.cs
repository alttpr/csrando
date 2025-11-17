namespace RandomizerTests.Logic.Alttp.Open.NoGlitches.DarkWorld.DeathMountain;

using RandomizerTests.Logic.Alttp;
using RandomizerTests.Logic.Alttp.Open.NoGlitches;

[TestClass]
public class WestTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        // TODO: update this to handle bottle/magic
        ["Spike Cave", false, new string[] {  }],
        ["Spike Cave", false, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "Lamp", "Cape" }],
        ["Spike Cave", false, new string[] { "Bottle", "MoonPearl", "ProgressiveGlove", "Lamp", "Cape" }],
        ["Spike Cave", false, new string[] { "Bottle", "MoonPearl", "Hammer", "Lamp", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "ProgressiveGlove", "Lamp", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "PowerGlove", "Lamp", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "TitansMitt", "Lamp", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "ProgressiveGlove", "OcarinaActive", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "PowerGlove", "OcarinaActive", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "TitansMitt", "OcarinaActive", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "ProgressiveGlove", "Lamp", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "PowerGlove", "Lamp", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "TitansMitt", "Lamp", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "ProgressiveGlove", "OcarinaActive", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "PowerGlove", "OcarinaActive", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "TitansMitt", "OcarinaActive", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "Lamp", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "PowerGlove", "Lamp", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "TitansMitt", "Lamp", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "OcarinaActive", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "PowerGlove", "OcarinaActive", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "TitansMitt", "OcarinaActive", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "Lamp", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "PowerGlove", "Lamp", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "TitansMitt", "Lamp", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "OcarinaActive", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "PowerGlove", "OcarinaActive", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "TitansMitt", "OcarinaActive", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "Lamp", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "PowerGlove", "Lamp", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "TitansMitt", "Lamp", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "OcarinaActive", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "PowerGlove", "OcarinaActive", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "TitansMitt", "OcarinaActive", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "Lamp", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "PowerGlove", "Lamp", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "TitansMitt", "Lamp", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "OcarinaActive", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "PowerGlove", "OcarinaActive", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "TitansMitt", "OcarinaActive", "CaneOfByrna" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
