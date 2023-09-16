namespace RandomizerTests.Logic.Standard.OverworldGlitches.DarkWorld.DeathMountain;

[TestClass]
public sealed class WestTest : StandardOverworldGlitchesLogicTests
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
        new object[] { "Spike Cave", false, new string[] { "Bottle", "MoonPearl", "Hammer", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", false, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", false, new string[] { "Bottle", "MoonPearl", "ProgressiveGlove", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", false, new string[] { "Bottle", "MoonPearl", "Hammer", "ProgressiveGlove", "PegasusBoots" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "ProgressiveGlove", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "PowerGlove", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "TitansMitt", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "ProgressiveGlove", "PegasusBoots", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "PowerGlove", "PegasusBoots", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "TitansMitt", "PegasusBoots", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "PowerGlove", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "TitansMitt", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "PegasusBoots", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "PowerGlove", "PegasusBoots", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "TitansMitt", "PegasusBoots", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "PowerGlove", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "TitansMitt", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "PegasusBoots", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "PowerGlove", "PegasusBoots", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "TitansMitt", "PegasusBoots", "CaneOfByrna" } },
    };
}
