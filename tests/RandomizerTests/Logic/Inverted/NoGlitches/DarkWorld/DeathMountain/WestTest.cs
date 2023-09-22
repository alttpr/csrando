namespace RandomizerTests.Logic.Inverted.NoGlitches.DarkWorld.DeathMountain;

[TestClass]
public class WestTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Spike Cave", false, new string[] {  } },
        new object[] { "Spike Cave", false, new string[] { "Bottle", "Hammer", "Lamp", "Cape" } },
        new object[] { "Spike Cave", false, new string[] { "Bottle", "ProgressiveGlove", "Lamp", "Cape" } },
        new object[] { "Spike Cave", false, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "Lamp" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "Lamp", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "PowerGlove", "Lamp", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "TitansMitt", "Lamp", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "OcarinaInactive", "MoonPearl", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "PowerGlove", "OcarinaInactive", "MoonPearl", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "TitansMitt", "OcarinaInactive", "MoonPearl", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "Lamp", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "PowerGlove", "Lamp", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "TitansMitt", "Lamp", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "OcarinaInactive", "MoonPearl", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "PowerGlove", "OcarinaInactive", "MoonPearl", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "TitansMitt", "OcarinaInactive", "MoonPearl", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "ProgressiveGlove", "Lamp", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "PowerGlove", "Lamp", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "TitansMitt", "Lamp", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "ProgressiveGlove", "OcarinaInactive", "MoonPearl", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "PowerGlove", "OcarinaInactive", "MoonPearl", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "TitansMitt", "OcarinaInactive", "MoonPearl", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "ProgressiveGlove", "Lamp", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "PowerGlove", "Lamp", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "TitansMitt", "Lamp", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "ProgressiveGlove", "OcarinaInactive", "MoonPearl", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "PowerGlove", "OcarinaInactive", "MoonPearl", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "TitansMitt", "OcarinaInactive", "MoonPearl", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "ProgressiveGlove", "Lamp", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "PowerGlove", "Lamp", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "TitansMitt", "Lamp", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "ProgressiveGlove", "OcarinaInactive", "MoonPearl", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "PowerGlove", "OcarinaInactive", "MoonPearl", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "TitansMitt", "OcarinaInactive", "MoonPearl", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "ProgressiveGlove", "Lamp", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "PowerGlove", "Lamp", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "TitansMitt", "Lamp", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "ProgressiveGlove", "OcarinaInactive", "MoonPearl", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "PowerGlove", "OcarinaInactive", "MoonPearl", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "TitansMitt", "OcarinaInactive", "MoonPearl", "CaneOfByrna" } },
    };

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
