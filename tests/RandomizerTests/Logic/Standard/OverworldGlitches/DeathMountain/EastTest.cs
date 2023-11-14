namespace RandomizerTests.Logic.Standard.OverworldGlitches.DeathMountain;

[TestClass]
public sealed class EastTest : StandardOverworldGlitchesLogicTests
{
    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    public static IEnumerable<object[]> TestData => [
        ["Spiral Cave Item", false, new string[] {  }],
        ["Spiral Cave Item", true, new string[] { "PegasusBoots" }],

        ["Paradox Cave Lower - Far Left", false, new string[] {  }],
        ["Paradox Cave Lower - Far Left", true, new string[] { "PegasusBoots" }],

        ["Paradox Cave Lower - Left", false, new string[] {  }],
        ["Paradox Cave Lower - Left", true, new string[] { "PegasusBoots" }],

        ["Paradox Cave Lower - Middle", false, new string[] {  }],
        ["Paradox Cave Lower - Middle", true, new string[] { "PegasusBoots" }],

        ["Paradox Cave Lower - Right", false, new string[] {  }],
        ["Paradox Cave Lower - Right", true, new string[] { "PegasusBoots" }],

        ["Paradox Cave Lower - Far Right", false, new string[] {  }],
        ["Paradox Cave Lower - Far Right", true, new string[] { "PegasusBoots" }],

        ["Paradox Cave Upper - Left", false, new string[] {  }],
        ["Paradox Cave Lower - Left", true, new string[] { "PegasusBoots" }],

        ["Paradox Cave Upper - Right", false, new string[] {  }],
        ["Paradox Cave Lower - Right", true, new string[] { "PegasusBoots" }],

        ["Mimic Cave", false, new string[] {  }],
        ["Mimic Cave", false, new string[] { "Hammer", "PegasusBoots" }],
        ["Mimic Cave", false, new string[] { "MagicMirror", "PegasusBoots" }],
        ["Mimic Cave", true, new string[] { "MagicMirror", "Hammer", "PegasusBoots" }],
        ["Mimic Cave", true, new string[] { "MagicMirror", "Hammer", "ProgressiveGlove", "Lamp" }],
        ["Mimic Cave", true, new string[] { "MagicMirror", "Hammer", "PowerGlove", "Lamp" }],
        ["Mimic Cave", true, new string[] { "MagicMirror", "Hammer", "TitansMitt", "Lamp" }],
        ["Mimic Cave", true, new string[] { "MagicMirror", "Hammer", "Flute" }],
    ];
}
