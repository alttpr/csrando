namespace RandomizerTests.Logic.Alttp.Open.NoGlitches;

using RandomizerTests.Logic.Alttp;

[TestClass]
public class TowerOfHeraTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Tower Of Hera - Big Key Chest", false, new string[] {  }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "OcarinaActive", "MagicMirror", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "ProgressiveGlove", "MagicMirror", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "PowerGlove", "MagicMirror", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "TitansMitt", "MagicMirror", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "OcarinaActive", "Hookshot", "Hammer", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "ProgressiveGlove", "Hookshot", "Hammer", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "PowerGlove", "Hookshot", "Hammer", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "TitansMitt", "Hookshot", "Hammer", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "FireRod", "OcarinaActive", "MagicMirror", "KeyP3" }],
        ["Tower Of Hera - Big Key Chest", true, new string[] { "FireRod", "OcarinaActive", "Hookshot", "Hammer", "KeyP3" }],

        ["Tower Of Hera - Basement Cage", false, new string[] {  }],
        ["Tower Of Hera - Basement Cage", true, new string[] { "OcarinaActive", "MagicMirror" }],
        ["Tower Of Hera - Basement Cage", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" }],
        ["Tower Of Hera - Basement Cage", true, new string[] { "PowerGlove", "Lamp", "MagicMirror" }],
        ["Tower Of Hera - Basement Cage", true, new string[] { "TitansMitt", "Lamp", "MagicMirror" }],
        ["Tower Of Hera - Basement Cage", true, new string[] { "OcarinaActive", "Hookshot", "Hammer" }],
        ["Tower Of Hera - Basement Cage", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot", "Hammer" }],
        ["Tower Of Hera - Basement Cage", true, new string[] { "PowerGlove", "Lamp", "Hookshot", "Hammer" }],
        ["Tower Of Hera - Basement Cage", true, new string[] { "TitansMitt", "Lamp", "Hookshot", "Hammer" }],

        ["Tower Of Hera - Map Chest", false, new string[] {  }],
        ["Tower Of Hera - Map Chest", true, new string[] { "OcarinaActive", "MagicMirror" }],
        ["Tower Of Hera - Map Chest", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" }],
        ["Tower Of Hera - Map Chest", true, new string[] { "PowerGlove", "Lamp", "MagicMirror" }],
        ["Tower Of Hera - Map Chest", true, new string[] { "TitansMitt", "Lamp", "MagicMirror" }],
        ["Tower Of Hera - Map Chest", true, new string[] { "OcarinaActive", "Hookshot", "Hammer" }],
        ["Tower Of Hera - Map Chest", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot", "Hammer" }],
        ["Tower Of Hera - Map Chest", true, new string[] { "PowerGlove", "Lamp", "Hookshot", "Hammer" }],
        ["Tower Of Hera - Map Chest", true, new string[] { "TitansMitt", "Lamp", "Hookshot", "Hammer" }],

        ["Tower Of Hera - Compass Chest", false, new string[] {  }],
        ["Tower Of Hera - Compass Chest", true, new string[] { "OcarinaActive", "MagicMirror", "BigKeyP3" }],
        ["Tower Of Hera - Compass Chest", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BigKeyP3" }],
        ["Tower Of Hera - Compass Chest", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BigKeyP3" }],
        ["Tower Of Hera - Compass Chest", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BigKeyP3" }],
        ["Tower Of Hera - Compass Chest", true, new string[] { "OcarinaActive", "Hookshot", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Compass Chest", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Compass Chest", true, new string[] { "PowerGlove", "Lamp", "Hookshot", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Compass Chest", true, new string[] { "TitansMitt", "Lamp", "Hookshot", "Hammer", "BigKeyP3" }],

        ["Tower Of Hera - Big Chest", false, new string[] {  }],
        ["Tower Of Hera - Big Chest", true, new string[] { "OcarinaActive", "MagicMirror", "BigKeyP3" }],
        ["Tower Of Hera - Big Chest", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BigKeyP3" }],
        ["Tower Of Hera - Big Chest", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BigKeyP3" }],
        ["Tower Of Hera - Big Chest", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BigKeyP3" }],
        ["Tower Of Hera - Big Chest", true, new string[] { "OcarinaActive", "Hookshot", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Big Chest", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Big Chest", true, new string[] { "PowerGlove", "Lamp", "Hookshot", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Big Chest", true, new string[] { "TitansMitt", "Lamp", "Hookshot", "Hammer", "BigKeyP3" }],

        ["Tower Of Hera - Boss", false, new string[] {  }],
        ["Tower Of Hera - Boss", true, new string[] { "OcarinaActive", "MagicMirror", "BigKeyP3", "ProgressiveSword" }],
        ["Tower Of Hera - Boss", true, new string[] { "OcarinaActive", "MagicMirror", "BigKeyP3", "UncleSword" }],
        ["Tower Of Hera - Boss", true, new string[] { "OcarinaActive", "MagicMirror", "BigKeyP3", "MasterSword" }],
        ["Tower Of Hera - Boss", true, new string[] { "OcarinaActive", "MagicMirror", "BigKeyP3", "L3Sword" }],
        ["Tower Of Hera - Boss", true, new string[] { "OcarinaActive", "MagicMirror", "BigKeyP3", "L4Sword" }],
        ["Tower Of Hera - Boss", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BigKeyP3", "UncleSword" }],
        ["Tower Of Hera - Boss", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BigKeyP3", "MasterSword" }],
        ["Tower Of Hera - Boss", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BigKeyP3", "ProgressiveSword" }],
        ["Tower Of Hera - Boss", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BigKeyP3", "L3Sword" }],
        ["Tower Of Hera - Boss", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BigKeyP3", "L4Sword" }],
        ["Tower Of Hera - Boss", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BigKeyP3", "UncleSword" }],
        ["Tower Of Hera - Boss", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BigKeyP3", "MasterSword" }],
        ["Tower Of Hera - Boss", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BigKeyP3", "ProgressiveSword" }],
        ["Tower Of Hera - Boss", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BigKeyP3", "L3Sword" }],
        ["Tower Of Hera - Boss", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BigKeyP3", "L4Sword" }],
        ["Tower Of Hera - Boss", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BigKeyP3", "UncleSword" }],
        ["Tower Of Hera - Boss", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BigKeyP3", "MasterSword" }],
        ["Tower Of Hera - Boss", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BigKeyP3", "ProgressiveSword" }],
        ["Tower Of Hera - Boss", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BigKeyP3", "L3Sword" }],
        ["Tower Of Hera - Boss", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BigKeyP3", "L4Sword" }],
        ["Tower Of Hera - Boss", true, new string[] { "OcarinaActive", "Hookshot", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Boss", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Boss", true, new string[] { "PowerGlove", "Lamp", "Hookshot", "Hammer", "BigKeyP3" }],
        ["Tower Of Hera - Boss", true, new string[] { "TitansMitt", "Lamp", "Hookshot", "Hammer", "BigKeyP3" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
