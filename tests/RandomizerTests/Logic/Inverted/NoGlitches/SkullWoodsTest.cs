using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Inverted.NoGlitches;

[TestClass]
public class SkullWoodsTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Skull Woods - Big Chest", false, new string[] {  } },
        new object[] { "Skull Woods - Big Chest", true, new string[] { "BigKeyD3" } },

        new object[] { "Skull Woods - Big Key Chest", true, new string[] {  } },

        new object[] { "Skull Woods - Compass Chest", true, new string[] {  } },

        new object[] { "Skull Woods - Map Chest", true, new string[] {  } },

        new object[] { "Skull Woods - Bridge Room Chest", false, new string[] {  } },
        new object[] { "Skull Woods - Bridge Room Chest", true, new string[] { "FireRod" } },

        new object[] { "Skull Woods - Pot Prison", true, new string[] {  } },

        new object[] { "Skull Woods - Pinball Chest", true, new string[] {  } },

        new object[] { "Skull Woods - Boss", false, new string[] {  } },
        new object[] { "Skull Woods - Boss", false, new string[] { "KeyD3", "KeyD3", "KeyD3", "UncleSword" } },
        new object[] { "Skull Woods - Boss", false, new string[] { "KeyD3", "KeyD3", "KeyD3", "FireRod" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "FireRod", "UncleSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "FireRod", "MasterSword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "FireRod", "L3Sword" } },
        new object[] { "Skull Woods - Boss", true, new string[] { "KeyD3", "KeyD3", "KeyD3", "FireRod", "L4Sword" } },
    };

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    [TestMethod]
    public void TestKeyForKey()
    {
        RunLogicTest(new[]
        {
            new WorldConfig
            {
                Accessibility = AccessibilityOption.Items,
                Glitches = GlitchesOption.None,
                State = StateOption.Inverted,
            }
        }, "Skull Woods - Big Chest:0", true, new string[0]);
    }
}
