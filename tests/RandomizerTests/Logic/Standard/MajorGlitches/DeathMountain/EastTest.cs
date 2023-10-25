namespace RandomizerTests.Logic.Standard.MajorGlitches.DeathMountain;

[TestClass]
public sealed class EastTest : StandardMajorGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Spiral Cave Item", true, new string[] {  } },

        new object[] { "Paradox Cave Lower - Far Left", true, new string[] {  } },

        new object[] { "Paradox Cave Lower - Left", true, new string[] {  } },

        new object[] { "Paradox Cave Lower - Middle", true, new string[] {  } },

        new object[] { "Paradox Cave Lower - Right", true, new string[] {  } },

        new object[] { "Paradox Cave Lower - Far Right", true, new string[] {  } },

        new object[] { "Paradox Cave Upper - Left", true, new string[] {  } },

        new object[] { "Paradox Cave Upper - Right", true, new string[] {  } },
    };
}
