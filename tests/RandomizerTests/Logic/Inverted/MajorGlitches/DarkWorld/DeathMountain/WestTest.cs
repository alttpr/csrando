namespace RandomizerTests.Logic.Inverted.MajorGlitches.DarkWorld.DeathMountain;

[TestClass]
public sealed class WestTest : InvertedMajorGlitchesLogicTests
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
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "PowerGlove", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "TitansMitt", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "ProgressiveGlove", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "PowerGlove", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "Bottle", "Hammer", "TitansMitt", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "ProgressiveGlove", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "PowerGlove", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "TitansMitt", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "ProgressiveGlove", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "PowerGlove", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "HalfMagic", "Hammer", "TitansMitt", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "ProgressiveGlove", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "PowerGlove", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "TitansMitt", "Cape" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "ProgressiveGlove", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "PowerGlove", "CaneOfByrna" } },
        new object[] { "Spike Cave", true, new string[] { "QuarterMagic", "Hammer", "TitansMitt", "CaneOfByrna" } },
    };
}
