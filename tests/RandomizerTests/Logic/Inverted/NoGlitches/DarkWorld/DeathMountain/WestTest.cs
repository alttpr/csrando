namespace RandomizerTests.Logic.Inverted.NoGlitches.DarkWorld.DeathMountain;

[TestClass]
public class WestTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Spike Cave", false, new string[] {  }],
        ["Spike Cave", false, new string[] { "Bottle", "Hammer", "Lamp", "Cape" }],
        ["Spike Cave", false, new string[] { "Bottle", "ProgressiveGlove", "Lamp", "Cape" }],
        ["Spike Cave", false, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "Lamp" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "Lamp", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "PowerGlove", "Lamp", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "TitansMitt", "Lamp", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "OcarinaInactive", "MoonPearl", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "PowerGlove", "OcarinaInactive", "MoonPearl", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "TitansMitt", "OcarinaInactive", "MoonPearl", "Cape" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "Lamp", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "PowerGlove", "Lamp", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "TitansMitt", "Lamp", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "OcarinaInactive", "MoonPearl", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "PowerGlove", "OcarinaInactive", "MoonPearl", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "Bottle", "Hammer", "TitansMitt", "OcarinaInactive", "MoonPearl", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "ProgressiveGlove", "Lamp", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "PowerGlove", "Lamp", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "TitansMitt", "Lamp", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "ProgressiveGlove", "OcarinaInactive", "MoonPearl", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "PowerGlove", "OcarinaInactive", "MoonPearl", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "TitansMitt", "OcarinaInactive", "MoonPearl", "Cape" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "ProgressiveGlove", "Lamp", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "PowerGlove", "Lamp", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "TitansMitt", "Lamp", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "ProgressiveGlove", "OcarinaInactive", "MoonPearl", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "PowerGlove", "OcarinaInactive", "MoonPearl", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "HalfMagic", "Hammer", "TitansMitt", "OcarinaInactive", "MoonPearl", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "ProgressiveGlove", "Lamp", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "PowerGlove", "Lamp", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "TitansMitt", "Lamp", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "ProgressiveGlove", "OcarinaInactive", "MoonPearl", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "PowerGlove", "OcarinaInactive", "MoonPearl", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "TitansMitt", "OcarinaInactive", "MoonPearl", "Cape" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "ProgressiveGlove", "Lamp", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "PowerGlove", "Lamp", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "TitansMitt", "Lamp", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "ProgressiveGlove", "OcarinaInactive", "MoonPearl", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "PowerGlove", "OcarinaInactive", "MoonPearl", "CaneOfByrna" }],
        ["Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "TitansMitt", "OcarinaInactive", "MoonPearl", "CaneOfByrna" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
