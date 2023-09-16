namespace RandomizerTests.Logic.Standard.MajorGlitches.DarkWorld.DeathMountain;

[TestClass]
public sealed class WestTest : StandardMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Spike Cave", false, new string[] {  } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "ProgressiveGlove", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "PowerGlove", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "TitansMitt", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "ProgressiveGlove", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "PowerGlove", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "TitansMitt", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "PowerGlove", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "TitansMitt", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "PowerGlove", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "TitansMitt", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "PowerGlove", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "TitansMitt", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "PowerGlove", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "TitansMitt", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Bottle", "Hammer", "ProgressiveGlove", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Bottle", "Hammer", "PowerGlove", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Bottle", "Hammer", "TitansMitt", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Bottle", "Hammer", "ProgressiveGlove", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Bottle", "Hammer", "PowerGlove", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Bottle", "Hammer", "TitansMitt", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Bottle", "Hammer", "ProgressiveGlove", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Bottle", "Hammer", "PowerGlove", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Bottle", "Hammer", "TitansMitt", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Bottle", "Hammer", "ProgressiveGlove", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Bottle", "Hammer", "PowerGlove", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Bottle", "Hammer", "TitansMitt", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Bottle", "Hammer", "ProgressiveGlove", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Bottle", "Hammer", "PowerGlove", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Bottle", "Hammer", "TitansMitt", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Bottle", "Hammer", "ProgressiveGlove", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Bottle", "Hammer", "PowerGlove", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Bottle", "Hammer", "TitansMitt", "CaneOfByrna" } },
    };
}
