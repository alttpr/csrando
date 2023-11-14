namespace RandomizerTests.Logic.Open.NoGlitches;

[TestClass]
public class DesertPalaceTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Desert Palace - Main Room - Center", false, new string[] {  }],
        ["Desert Palace - Main Room - Center", true, new string[] { "BookOfMudora" }],
        ["Desert Palace - Main Room - Center", true, new string[] { "OcarinaActive", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Desert Palace - Main Room - Center", true, new string[] { "OcarinaActive", "MagicMirror", "TitansMitt" }],

        ["Desert Palace - Map Chest", false, new string[] {  }],
        ["Desert Palace - Map Chest", true, new string[] { "BookOfMudora" }],
        ["Desert Palace - Map Chest", true, new string[] { "OcarinaActive", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Desert Palace - Map Chest", true, new string[] { "OcarinaActive", "MagicMirror", "TitansMitt" }],

        ["Desert Palace - Big Chest", false, new string[] {  }],
        ["Desert Palace - Big Chest", true, new string[] { "BookOfMudora", "BigKeyP2" }],
        ["Desert Palace - Big Chest", true, new string[] { "OcarinaActive", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove", "BigKeyP2" }],
        ["Desert Palace - Big Chest", true, new string[] { "OcarinaActive", "MagicMirror", "TitansMitt", "BigKeyP2" }],

        ["Desert Palace - Torch", false, new string[] {  }],
        ["Desert Palace - Torch", true, new string[] { "BookOfMudora", "PegasusBoots" }],
        ["Desert Palace - Torch", true, new string[] { "OcarinaActive", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove", "PegasusBoots" }],
        ["Desert Palace - Torch", true, new string[] { "OcarinaActive", "MagicMirror", "TitansMitt", "PegasusBoots" }],

        ["Desert Palace - Compass Chest", false, new string[] {  }],
        ["Desert Palace - Compass Chest", true, new string[] { "BookOfMudora", "KeyP2" }],
        ["Desert Palace - Compass Chest", true, new string[] { "OcarinaActive", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove", "KeyP2" }],
        ["Desert Palace - Compass Chest", true, new string[] { "OcarinaActive", "MagicMirror", "TitansMitt", "KeyP2" }],

        ["Desert Palace - Big Key Chest", false, new string[] {  }],
        ["Desert Palace - Big Key Chest", true, new string[] { "BookOfMudora", "KeyP2" }],
        ["Desert Palace - Big Key Chest", true, new string[] { "OcarinaActive", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove", "KeyP2" }],
        ["Desert Palace - Big Key Chest", true, new string[] { "OcarinaActive", "MagicMirror", "TitansMitt", "KeyP2" }],

        ["Desert Palace - Boss", false, new string[] {  }],
        ["Desert Palace - Boss", false, new string[] { "UncleSword", "BookOfMudora", "Lamp", "ProgressiveGlove", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "BookOfMudora", "Lamp", "ProgressiveGlove", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "BookOfMudora", "Lamp", "PowerGlove", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "BookOfMudora", "Lamp", "TitansMitt", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "BookOfMudora", "FireRod", "ProgressiveGlove", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "BookOfMudora", "FireRod", "PowerGlove", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "BookOfMudora", "FireRod", "TitansMitt", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "OcarinaActive", "MagicMirror", "Lamp", "ProgressiveGlove", "ProgressiveGlove", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "OcarinaActive", "MagicMirror", "Lamp", "TitansMitt", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "OcarinaActive", "MagicMirror", "FireRod", "ProgressiveGlove", "ProgressiveGlove", "BigKeyP2" }],
        ["Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "OcarinaActive", "MagicMirror", "FireRod", "TitansMitt", "BigKeyP2" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
