using Randomizer.Graph;

namespace RandomizerTests.Logic.Inverted.NoGlitches;

[TestClass]
public class TowerOfHeraTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Tower Of Hera - Big Key Chest", false, new string[] {  }],
        ["Tower Of Hera - Big Key Chest", false, new string[] { "Lamp", "MoonPearl", "OcarinaActive", "Hookshot", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", false, new string[] { "Lamp", "Hammer", "OcarinaActive", "Hookshot" }],
        ["Tower Of Hera - Big Key Chest", false, new string[] { "Lamp", "Hammer", "MoonPearl", "OcarinaActive", "Hookshot" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "Hammer", "MoonPearl", "OcarinaActive", "Hookshot", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "Hammer", "MoonPearl", "ProgressiveGlove", "Hookshot", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "Hammer", "MoonPearl", "PowerGlove", "Hookshot", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "Hammer", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "Hammer", "MoonPearl", "TitansMitt", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "FireRod", "Hammer", "MoonPearl", "OcarinaInactive", "AgahnimDefeated", "Hookshot", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "FireRod", "Hammer", "MoonPearl", "OcarinaInactive", "ProgressiveGlove", "Hookshot", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "FireRod", "Hammer", "MoonPearl", "OcarinaInactive", "PowerGlove", "Hookshot", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "FireRod", "Hammer", "MoonPearl", "OcarinaInactive", "ProgressiveGlove", "ProgressiveGlove", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "FireRod", "Hammer", "MoonPearl", "OcarinaInactive", "TitansMitt", "KeyP3" }],

        ["Tower Of Hera - Basement Cage", false, new string[] {  }],
        ["Tower Of Hera - Basement Cage", false, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "Hammer" }],
        ["Tower Of Hera - Basement Cage", true, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer" }],
        ["Tower Of Hera - Basement Cage", true, new string[] { "PowerGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer" }],
        ["Tower Of Hera - Basement Cage", true, new string[] { "TitansMitt", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer" }],
        ["Tower Of Hera - Basement Cage", true, new string[] { "ProgressiveGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer" }],
        ["Tower Of Hera - Basement Cage", true, new string[] { "PowerGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer" }],
        ["Tower Of Hera - Basement Cage", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "Lamp", "Hammer" }],
        ["Tower Of Hera - Basement Cage", true, new string[] { "TitansMitt", "MoonPearl", "Lamp", "Hammer" }],

        ["Tower Of Hera - Map Chest", false, new string[] {  }],
        ["Tower Of Hera - Map Chest", false, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "Hammer" }],
        ["Tower Of Hera - Map Chest", true, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer" }],
        ["Tower Of Hera - Map Chest", true, new string[] { "PowerGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer" }],
        ["Tower Of Hera - Map Chest", true, new string[] { "TitansMitt", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer" }],
        ["Tower Of Hera - Map Chest", true, new string[] { "ProgressiveGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer" }],
        ["Tower Of Hera - Map Chest", true, new string[] { "PowerGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer" }],
        ["Tower Of Hera - Map Chest", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "Lamp", "Hammer" }],
        ["Tower Of Hera - Map Chest", true, new string[] { "TitansMitt", "MoonPearl", "Lamp", "Hammer" }],

        ["Tower Of Hera - Compass Chest", false, new string[] {  }],
        ["Tower Of Hera - Compass Chest", false, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Compass Chest", false, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer" }],
        ["Tower Of Hera - Compass Chest", true, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Compass Chest", true, new string[] { "PowerGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Compass Chest", true, new string[] { "TitansMitt", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Compass Chest", true, new string[] { "ProgressiveGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Compass Chest", true, new string[] { "PowerGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Compass Chest", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "Lamp", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Compass Chest", true, new string[] { "TitansMitt", "MoonPearl", "Lamp", "Hammer", "BigKeyP3" }],

        ["Tower Of Hera - Big Chest", false, new string[] {  }],
        ["Tower Of Hera - Big Chest", false, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Big Chest", false, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer" }],
        ["Tower Of Hera - Big Chest", true, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Big Chest", true, new string[] { "PowerGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Big Chest", true, new string[] { "TitansMitt", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Big Chest", true, new string[] { "ProgressiveGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Big Chest", true, new string[] { "PowerGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Big Chest", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "Lamp", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Big Chest", true, new string[] { "TitansMitt", "MoonPearl", "Lamp", "Hammer", "BigKeyP3" }],

        ["Tower Of Hera - Boss", false, new string[] {  }],
        ["Tower Of Hera - Boss", false, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Boss", false, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "BigKeyP3" }],
        ["Tower Of Hera - Boss", false, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer" }],
        ["Tower Of Hera - Boss", true, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Boss", true, new string[] { "PowerGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Boss", true, new string[] { "TitansMitt", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Boss", true, new string[] { "ProgressiveGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Boss", true, new string[] { "PowerGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Boss", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "Lamp", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Boss", true, new string[] { "TitansMitt", "MoonPearl", "Lamp", "Hammer", "BigKeyP3" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    [TestMethod]
    public void TestKeyForKey()
    {
        RunLogicTest(
        [
            new WorldConfig
            {
                Accessibility = AccessibilityOption.Items,
                Glitches = GlitchesOption.None,
                State = StateOption.Inverted,
            }
        ], "Tower Of Hera - Big Key Chest:0", true, new[] { "Lamp", "Hammer", "MoonPearl", "OcarinaActive", "Hookshot" });
    }

}
