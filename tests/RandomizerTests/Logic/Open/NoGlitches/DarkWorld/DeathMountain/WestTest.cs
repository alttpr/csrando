namespace RandomizerTests.Logic.Open.NoGlitches.DarkWorld.DeathMountain;

[TestClass]
public class WestTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => new[]
    {
        // @todo update this to handle bottle/magic
        new object[] { "Spike Cave", false, new string[] {  } },
        new object[] { "Spike Cave", false, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "Lamp", "Cape" } },
        new object[] { "Spike Cave", false, new string[] { "Bottle", "MoonPearl", "ProgressiveGlove", "Lamp", "Cape" } },
        new object[] { "Spike Cave", false, new string[] { "Bottle", "MoonPearl", "Hammer", "Lamp", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "ProgressiveGlove", "Lamp", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "PowerGlove", "Lamp", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "TitansMitt", "Lamp", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "ProgressiveGlove", "OcarinaActive", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "PowerGlove", "OcarinaActive", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "TitansMitt", "OcarinaActive", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "ProgressiveGlove", "Lamp", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "PowerGlove", "Lamp", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "TitansMitt", "Lamp", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "ProgressiveGlove", "OcarinaActive", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "PowerGlove", "OcarinaActive", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "MoonPearl", "Hammer", "TitansMitt", "OcarinaActive", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "Lamp", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "PowerGlove", "Lamp", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "TitansMitt", "Lamp", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "OcarinaActive", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "PowerGlove", "OcarinaActive", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "TitansMitt", "OcarinaActive", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "Lamp", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "PowerGlove", "Lamp", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "TitansMitt", "Lamp", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "OcarinaActive", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "PowerGlove", "OcarinaActive", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "MoonPearl", "Hammer", "TitansMitt", "OcarinaActive", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "Lamp", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "PowerGlove", "Lamp", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "TitansMitt", "Lamp", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "OcarinaActive", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "PowerGlove", "OcarinaActive", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "TitansMitt", "OcarinaActive", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "Lamp", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "PowerGlove", "Lamp", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "TitansMitt", "Lamp", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "ProgressiveGlove", "OcarinaActive", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "PowerGlove", "OcarinaActive", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "MoonPearl", "Hammer", "TitansMitt", "OcarinaActive", "CaneOfByrna" } },
    };

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
