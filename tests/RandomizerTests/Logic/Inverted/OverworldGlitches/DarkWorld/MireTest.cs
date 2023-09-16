namespace RandomizerTests.Logic.Inverted.OverworldGlitches.DarkWorld;

[TestClass]
public sealed class MireTest : InvertedOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Mire Shed - Left", false, new string[] {  } },
        new object[] { "Mire Shed - Left", true, new string[] { "PegasusBoots" } },

        new object[] { "Mire Shed - Right", false, new string[] {  } },
        new object[] { "Mire Shed - Right", true, new string[] { "PegasusBoots" } },
    };
}
