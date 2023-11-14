namespace RandomizerTests.Logic.Open.NoGlitches.DarkWorld;

[TestClass]
public class SouthTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestData => [
        ["Hype Cave - Top", false, new string[] {  }],
        ["Hype Cave - Top", false, new string[] { "AgahnimDefeated", "Hammer" }],
        ["Hype Cave - Top", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer" }],
        ["Hype Cave - Top", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Hype Cave - Top", true, new string[] { "MoonPearl", "TitansMitt" }],
        ["Hype Cave - Top", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Hype Cave - Top", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Hype Cave - Top", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" }],
        ["Hype Cave - Top", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" }],
        ["Hype Cave - Top", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" }],

        ["Hype Cave - Middle Right", false, new string[] {  }],
        ["Hype Cave - Middle Right", false, new string[] { "AgahnimDefeated", "Hammer" }],
        ["Hype Cave - Middle Right", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer" }],
        ["Hype Cave - Middle Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Hype Cave - Middle Right", true, new string[] { "MoonPearl", "TitansMitt" }],
        ["Hype Cave - Middle Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Hype Cave - Middle Right", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Hype Cave - Middle Right", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" }],
        ["Hype Cave - Middle Right", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" }],
        ["Hype Cave - Middle Right", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" }],

        ["Hype Cave - Middle Left", false, new string[] {  }],
        ["Hype Cave - Middle Left", false, new string[] { "AgahnimDefeated", "Hammer" }],
        ["Hype Cave - Middle Left", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer" }],
        ["Hype Cave - Middle Left", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Hype Cave - Middle Left", true, new string[] { "MoonPearl", "TitansMitt" }],
        ["Hype Cave - Middle Left", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Hype Cave - Middle Left", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Hype Cave - Middle Left", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" }],
        ["Hype Cave - Middle Left", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" }],
        ["Hype Cave - Middle Left", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" }],

        ["Hype Cave - Bottom", false, new string[] {  }],
        ["Hype Cave - Bottom", false, new string[] { "AgahnimDefeated", "Hammer" }],
        ["Hype Cave - Bottom", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer" }],
        ["Hype Cave - Bottom", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Hype Cave - Bottom", true, new string[] { "MoonPearl", "TitansMitt" }],
        ["Hype Cave - Bottom", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Hype Cave - Bottom", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Hype Cave - Bottom", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" }],
        ["Hype Cave - Bottom", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" }],
        ["Hype Cave - Bottom", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" }],

        ["Hype Cave - NPC Item", false, new string[] {  }],
        ["Hype Cave - NPC Item", false, new string[] { "AgahnimDefeated", "Hammer" }],
        ["Hype Cave - NPC Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer" }],
        ["Hype Cave - NPC Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Hype Cave - NPC Item", true, new string[] { "MoonPearl", "TitansMitt" }],
        ["Hype Cave - NPC Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Hype Cave - NPC Item", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Hype Cave - NPC Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" }],
        ["Hype Cave - NPC Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" }],
        ["Hype Cave - NPC Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" }],

        ["Stumpy", false, new string[] {  }],
        ["Stumpy", false, new string[] { "AgahnimDefeated", "Hammer" }],
        ["Stumpy", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer" }],
        ["Stumpy", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Stumpy", true, new string[] { "MoonPearl", "TitansMitt" }],
        ["Stumpy", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Stumpy", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Stumpy", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" }],
        ["Stumpy", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" }],
        ["Stumpy", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" }],

        ["Digging Game - Item", false, new string[] {  }],
        ["Digging Game - Item", false, new string[] { "AgahnimDefeated", "Hammer" }],
        ["Digging Game - Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "Hammer" }],
        ["Digging Game - Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" }],
        ["Digging Game - Item", true, new string[] { "MoonPearl", "TitansMitt" }],
        ["Digging Game - Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" }],
        ["Digging Game - Item", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" }],
        ["Digging Game - Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" }],
        ["Digging Game - Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" }],
        ["Digging Game - Item", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }
}
