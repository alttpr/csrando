namespace RandomizerTests.Logic.Inverted.NoGlitches.DarkWorld;

[TestClass]
public class NorthEastTest : InvertedNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Catfish", false, new string[] {  }],
        ["Catfish", false, new string[] { "AgahnimDefeated", "MagicMirror" }],
        ["Catfish", true, new string[] { "AgahnimDefeated", "MagicMirror", "ProgressiveGlove" }],
        ["Catfish", true, new string[] { "AgahnimDefeated", "MagicMirror",  "PowerGlove" }],
        ["Catfish", true, new string[] { "AgahnimDefeated", "MagicMirror",  "TitansMitt" }],
        ["Catfish", true, new string[] { "ProgressiveGlove", "Hammer" }],
        ["Catfish", true, new string[] { "ProgressiveGlove", "Flippers" }],
        ["Catfish", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "MoonPearl" }],

        ["Pyramid Item", false, new string[] {  }],
        ["Pyramid Item", true, new string[] { "AgahnimDefeated", "MagicMirror" }],
        ["Pyramid Item", true, new string[] { "Hammer" }],
        ["Pyramid Item", true, new string[] { "Flippers", "ProgressiveGlove" }],
        ["Pyramid Item", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "MoonPearl" }],

        ["Pyramid Fairy - Left", false, new string[] {  }],
        ["Pyramid Fairy - Left", false, new string[] { "AgahnimDefeated", "MagicMirror", "Hammer" }],
        ["Pyramid Fairy - Left", false, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "Hammer" }],
        ["Pyramid Fairy - Left", true, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "MagicMirror", "Hammer" }],
        ["Pyramid Fairy - Left", true, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "MagicMirror", "ProgressiveGlove", "Flippers" }],
        ["Pyramid Fairy - Left", true, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "MagicMirror", "PowerGlove", "Flippers" }],
        ["Pyramid Fairy - Left", true, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "MagicMirror", "TitansMitt", "Flippers" }],

        ["Pyramid Fairy - Right", false, new string[] {  }],
        ["Pyramid Fairy - Right", false, new string[] { "AgahnimDefeated", "MagicMirror", "Hammer" }],
        ["Pyramid Fairy - Right", false, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "Hammer" }],
        ["Pyramid Fairy - Right", true, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "MagicMirror", "Hammer" }],
        ["Pyramid Fairy - Right", true, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "MagicMirror", "ProgressiveGlove", "Flippers" }],
        ["Pyramid Fairy - Right", true, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "MagicMirror", "PowerGlove", "Flippers" }],
        ["Pyramid Fairy - Right", true, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "MagicMirror", "TitansMitt", "Flippers" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
