namespace RandomizerTests.Logic.Inverted.MajorGlitches;

[TestClass]
public sealed class EasternPalaceTest : InvertedMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Eastern Palace - Compass Chest", true, new string[] {  } },

        new object[] { "Eastern Palace - Cannonball Chest", true, new string[] {  } },

        new object[] { "Eastern Palace - Big Chest", false, new string[] {  } },
        new object[] { "Eastern Palace - Big Chest", true, new string[] { "BigKeyP1" } },

        new object[] { "Eastern Palace - Map Chest", true, new string[] {  } },

        new object[] { "Eastern Palace - Big Key Chest", false, new string[] {  } },
        new object[] { "Eastern Palace - Big Key Chest", true, new string[] { "Lamp" } },

        new object[] { "Eastern Palace - Boss", false, new string[] {  } },
        new object[] { "Eastern Palace - Boss", true, new string[] { "Lamp", "BowAndArrows", "BigKeyP1" } },
    };
}
