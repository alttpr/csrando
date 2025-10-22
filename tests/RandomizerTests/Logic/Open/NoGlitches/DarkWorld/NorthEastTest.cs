namespace RandomizerTests.Logic.Open.NoGlitches.DarkWorld;

[TestClass]
public class NorthEastTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Catfish", false, new string[] {  }],
        ["Catfish", false, new string[] { "AgahnimDefeated", "ProgressiveGlove" }],
        ["Catfish", false, new string[] { "MoonPearl", "AgahnimDefeated" }],
        ["Catfish", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove" }],
        ["Catfish", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove" }],
        ["Catfish", true, new string[] { "MoonPearl", "AgahnimDefeated", "TitansMitt" }],
        ["Catfish", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Catfish", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Catfish", true, new string[] { "MoonPearl", "TitansMitt", "Flippers" }],

        ["Pyramid Item", false, new string[] {  }],
        ["Pyramid Item", true, new string[] { "AgahnimDefeated" }],
        ["Pyramid Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Pyramid Item", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Pyramid Item", true, new string[] { "MoonPearl", "TitansMitt", "Flippers" }],

        ["Pyramid Fairy - Left", false, new string[] {  }],
        ["Pyramid Fairy - Left", false, new string[] { "Crystal5", "Crystal6", "AgahnimDefeated", "Hammer" }],
        ["Pyramid Fairy - Left", false, new string[] { "MoonPearl", "Crystal6", "AgahnimDefeated", "Hammer" }],
        ["Pyramid Fairy - Left", false, new string[] { "MoonPearl", "Crystal5", "AgahnimDefeated", "Hammer" }],
        // can"t jump with red bomb
        ["Pyramid Fairy - Left", false, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "Hookshot", "Flippers" }],
        ["Pyramid Fairy - Left", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "Hammer" }],
        ["Pyramid Fairy - Left", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "TitansMitt", "Hammer" }],
        ["Pyramid Fairy - Left", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "ProgressiveGlove", "Hammer" }],
        ["Pyramid Fairy - Left", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "PowerGlove", "Hammer" }],
        ["Pyramid Fairy - Left", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "TitansMitt", "MagicMirror" }],
        ["Pyramid Fairy - Left", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror" }],
        ["Pyramid Fairy - Left", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "MagicMirror" }],
        ["Pyramid Fairy - Left", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "PowerGlove", "Hookshot", "MagicMirror" }],
        ["Pyramid Fairy - Left", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "Flippers", "Hookshot", "MagicMirror" }],

        ["Pyramid Fairy - Right", false, new string[] {  }],
        ["Pyramid Fairy - Right", false, new string[] { "Crystal5", "Crystal6", "AgahnimDefeated", "Hammer" }],
        ["Pyramid Fairy - Right", false, new string[] { "MoonPearl", "Crystal6", "AgahnimDefeated", "Hammer" }],
        ["Pyramid Fairy - Right", false, new string[] { "MoonPearl", "Crystal5", "AgahnimDefeated", "Hammer" }],
        ["Pyramid Fairy - Right", false, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "Hookshot", "Flippers" }],
        ["Pyramid Fairy - Right", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "Hammer" }],
        ["Pyramid Fairy - Right", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "TitansMitt", "Hammer" }],
        ["Pyramid Fairy - Right", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "ProgressiveGlove", "Hammer" }],
        ["Pyramid Fairy - Right", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "PowerGlove", "Hammer" }],
        ["Pyramid Fairy - Right", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "TitansMitt", "MagicMirror" }],
        ["Pyramid Fairy - Right", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror" }],
        ["Pyramid Fairy - Right", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "MagicMirror" }],
        ["Pyramid Fairy - Right", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "PowerGlove", "Hookshot", "MagicMirror" }],
        ["Pyramid Fairy - Right", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "Flippers", "Hookshot", "MagicMirror" }],

        // TODO: update this
        ["Ganon", false, new string[] {  }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
