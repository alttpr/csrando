namespace RandomizerTests.Logic.Inverted.OverworldGlitches.DarkWorld.DeathMountain;

[TestClass]
public sealed class WestTest : InvertedOverworldGlitchesLogicTests
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
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "PowerGlove", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "TitansMitt", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "PegasusBoots", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "PowerGlove", "PegasusBoots", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "TitansMitt", "PegasusBoots", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "ProgressiveGlove", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "PowerGlove", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "TitansMitt", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "ProgressiveGlove", "PegasusBoots", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "PowerGlove", "PegasusBoots", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "TitansMitt", "PegasusBoots", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "ProgressiveGlove", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "PowerGlove", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "TitansMitt", "PegasusBoots", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "ProgressiveGlove", "PegasusBoots", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "PowerGlove", "PegasusBoots", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "TitansMitt", "PegasusBoots", "CaneOfByrna" } },
    };
}
