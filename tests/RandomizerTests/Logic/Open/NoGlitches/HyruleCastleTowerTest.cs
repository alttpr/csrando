namespace RandomizerTests.Logic.Open.NoGlitches;

[TestClass]
public class HyruleCastleTowerTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Agahnims Tower - First Chest", false, new string[] {  } },
        new object[] { "Agahnims Tower - First Chest", true, new string[] { "L2Sword" } },
        new object[] { "Agahnims Tower - First Chest", true, new string[] { "Cape" } },

        new object[] { "Agahnims Tower - Second Chest", false, new string[] {  } },
        new object[] { "Agahnims Tower - Second Chest", false, new string[] { "L2Sword" } },
        new object[] { "Agahnims Tower - Second Chest", true, new string[] { "KeyA1", "L2Sword", "Lamp" } },
        new object[] { "Agahnims Tower - Second Chest", true, new string[] { "KeyA1", "Cape", "Lamp" } },

        new object[] { "Agahnims Tower - Boss", false, new string[] {  } },
        new object[] { "Agahnims Tower - Boss", false, new string[] { "KeyA1", "KeyA1", "Cape", "Lamp" } },
        new object[] { "Agahnims Tower - Boss", true, new string[] { "KeyA1", "KeyA1", "L2Sword", "Lamp" } },
        new object[] { "Agahnims Tower - Boss", true, new string[] { "KeyA1", "KeyA1", "L1Sword", "Cape", "Lamp" } },
    };

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
