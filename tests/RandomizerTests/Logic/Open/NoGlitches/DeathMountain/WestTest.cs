namespace RandomizerTests.Logic.Open.NoGlitches.DeathMountain;

[TestClass]
public sealed class WestTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Ether Tablet", false, new string[] {  }],
        ["Ether Tablet", true, new string[] { "OcarinaActive", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" }],
        ["Ether Tablet", true, new string[] { "OcarinaActive", "MagicMirror", "BookOfMudora", "L2Sword" }],
        ["Ether Tablet", true, new string[] { "OcarinaActive", "MagicMirror", "BookOfMudora", "L3Sword" }],
        ["Ether Tablet", true, new string[] { "OcarinaActive", "MagicMirror", "BookOfMudora", "L4Sword" }],
        ["Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" }],
        ["Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BookOfMudora", "L2Sword" }],
        ["Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BookOfMudora", "L3Sword" }],
        ["Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BookOfMudora", "L4Sword" }],
        ["Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" }],
        ["Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BookOfMudora", "L2Sword" }],
        ["Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BookOfMudora", "L3Sword" }],
        ["Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BookOfMudora", "L4Sword" }],
        ["Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" }],
        ["Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BookOfMudora", "L2Sword" }],
        ["Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BookOfMudora", "L3Sword" }],
        ["Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BookOfMudora", "L4Sword" }],
        ["Ether Tablet", true, new string[] { "OcarinaActive", "Hammer", "Hookshot", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" }],
        ["Ether Tablet", true, new string[] { "OcarinaActive", "Hammer", "Hookshot", "BookOfMudora", "L2Sword" }],
        ["Ether Tablet", true, new string[] { "OcarinaActive", "Hammer", "Hookshot", "BookOfMudora", "L3Sword" }],
        ["Ether Tablet", true, new string[] { "OcarinaActive", "Hammer", "Hookshot", "BookOfMudora", "L4Sword" }],
        ["Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" }],
        ["Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "L2Sword" }],
        ["Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "L3Sword" }],
        ["Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "L4Sword" }],
        ["Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" }],
        ["Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "L2Sword" }],
        ["Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "L3Sword" }],
        ["Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "L4Sword" }],
        ["Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" }],
        ["Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "L2Sword" }],
        ["Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "L3Sword" }],
        ["Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "L4Sword" }],

        ["Old Man", false, new string[] {  }],
        ["Old Man", true, new string[] { "OcarinaActive", "Lamp" }],
        ["Old Man", true, new string[] { "ProgressiveGlove", "Lamp" }],
        ["Old Man", true, new string[] { "PowerGlove", "Lamp" }],
        ["Old Man", true, new string[] { "TitansMitt", "Lamp" }],

        ["Spectacle Rock Cave Item", false, new string[] {  }],
        ["Spectacle Rock Cave Item", true, new string[] { "OcarinaActive" }],
        ["Spectacle Rock Cave Item", true, new string[] { "ProgressiveGlove", "Lamp" }],
        ["Spectacle Rock Cave Item", true, new string[] { "PowerGlove", "Lamp" }],
        ["Spectacle Rock Cave Item", true, new string[] { "TitansMitt", "Lamp" }],

        ["Spectacle Rock", false, new string[] {  }],
        ["Spectacle Rock", true, new string[] { "OcarinaActive", "MagicMirror" }],
        ["Spectacle Rock", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" }],
        ["Spectacle Rock", true, new string[] { "PowerGlove", "Lamp", "MagicMirror" }],
        ["Spectacle Rock", true, new string[] { "TitansMitt", "Lamp", "MagicMirror" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
