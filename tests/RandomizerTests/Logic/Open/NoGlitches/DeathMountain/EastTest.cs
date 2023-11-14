namespace RandomizerTests.Logic.Open.NoGlitches.DeathMountain;

[TestClass]
public class EastTest : OpenNoGlitchesLogicTests
{
    public static IEnumerable<object[]> TestDataWithMedallion => [
        ["Mimic Cave", false, "Quake", new string[] {  }],
        ["Mimic Cave", false, "Quake", new string[] { "Quake" }],
        ["Mimic Cave", false, "Quake", new string[] { "Gloves", "OcarinaActive" }],
        ["Mimic Cave", false, "Quake", new string[] { "Hammer" }],
        ["Mimic Cave", false, "Quake", new string[] { "MagicMirror" }],
        ["Mimic Cave", false, "Quake", new string[] { "MoonPearl" }],
        ["Mimic Cave", false, "Quake", new string[] { "CaneOfSomaria" }],
        ["Mimic Cave", false, "Ether", new string[] {  }],
        ["Mimic Cave", false, "Ether", new string[] { "Ether" }],
        ["Mimic Cave", false, "Ether", new string[] { "Gloves", "OcarinaActive" }],
        ["Mimic Cave", false, "Ether", new string[] { "Hammer" }],
        ["Mimic Cave", false, "Ether", new string[] { "MagicMirror" }],
        ["Mimic Cave", false, "Ether", new string[] { "MoonPearl" }],
        ["Mimic Cave", false, "Ether", new string[] { "CaneOfSomaria" }],
        ["Mimic Cave", false, "Bombos", new string[] {  }],
        ["Mimic Cave", false, "Bombos", new string[] { "Bombos" }],
        ["Mimic Cave", false, "Bombos", new string[] { "Gloves", "OcarinaActive" }],
        ["Mimic Cave", false, "Bombos", new string[] { "Hammer" }],
        ["Mimic Cave", false, "Bombos", new string[] { "MagicMirror" }],
        ["Mimic Cave", false, "Bombos", new string[] { "MoonPearl" }],
        ["Mimic Cave", false, "Bombos", new string[] { "CaneOfSomaria" }],
    ];

    public static IEnumerable<object[]> TestData => [
        ["Spiral Cave Item", false, new string[] {  }],
        ["Spiral Cave Item", false, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" }],
        ["Spiral Cave Item", false, new string[] { "ProgressiveGlove", "Hookshot" }],
        ["Spiral Cave Item", false, new string[] { "OcarinaActive", "MagicMirror" }],
        ["Spiral Cave Item", false, new string[] { "OcarinaActive", "Hammer" }],
        ["Spiral Cave Item", true, new string[] { "OcarinaActive", "Hookshot" }],
        ["Spiral Cave Item", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" }],
        ["Spiral Cave Item", true, new string[] { "PowerGlove", "Lamp", "Hookshot" }],
        ["Spiral Cave Item", true, new string[] { "TitansMitt", "Lamp", "Hookshot" }],
        ["Spiral Cave Item", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "Hammer" }],
        ["Spiral Cave Item", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "Hammer" }],
        ["Spiral Cave Item", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "Hammer" }],
        ["Spiral Cave Item", true, new string[] { "OcarinaActive", "MagicMirror", "Hammer" }],

        ["Paradox Cave Lower - Far Left", false, new string[] {  }],
        ["Paradox Cave Lower - Far Left", false, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" }],
        ["Paradox Cave Lower - Far Left", false, new string[] { "ProgressiveGlove", "Hookshot" }],
        ["Paradox Cave Lower - Far Left", false, new string[] { "OcarinaActive", "MagicMirror" }],
        ["Paradox Cave Lower - Far Left", false, new string[] { "OcarinaActive", "Hammer" }],
        ["Paradox Cave Lower - Far Left", true, new string[] { "OcarinaActive", "Hookshot" }],
        ["Paradox Cave Lower - Far Left", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" }],
        ["Paradox Cave Lower - Far Left", true, new string[] { "PowerGlove", "Lamp", "Hookshot" }],
        ["Paradox Cave Lower - Far Left", true, new string[] { "TitansMitt", "Lamp", "Hookshot" }],
        ["Paradox Cave Lower - Far Left", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Lower - Far Left", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Lower - Far Left", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Lower - Far Left", true, new string[] { "OcarinaActive", "MagicMirror", "Hammer" }],

        ["Paradox Cave Lower - Left", false, new string[] {  }],
        ["Paradox Cave Lower - Left", false, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" }],
        ["Paradox Cave Lower - Left", false, new string[] { "ProgressiveGlove", "Hookshot" }],
        ["Paradox Cave Lower - Left", false, new string[] { "OcarinaActive", "MagicMirror" }],
        ["Paradox Cave Lower - Left", false, new string[] { "OcarinaActive", "Hammer" }],
        ["Paradox Cave Lower - Left", true, new string[] { "OcarinaActive", "Hookshot" }],
        ["Paradox Cave Lower - Left", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" }],
        ["Paradox Cave Lower - Left", true, new string[] { "PowerGlove", "Lamp", "Hookshot" }],
        ["Paradox Cave Lower - Left", true, new string[] { "TitansMitt", "Lamp", "Hookshot" }],
        ["Paradox Cave Lower - Left", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Lower - Left", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Lower - Left", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Lower - Left", true, new string[] { "OcarinaActive", "MagicMirror", "Hammer" }],

        ["Paradox Cave Lower - Middle", false, new string[] {  }],
        ["Paradox Cave Lower - Middle", false, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" }],
        ["Paradox Cave Lower - Middle", false, new string[] { "ProgressiveGlove", "Hookshot" }],
        ["Paradox Cave Lower - Middle", false, new string[] { "OcarinaActive", "MagicMirror" }],
        ["Paradox Cave Lower - Middle", false, new string[] { "OcarinaActive", "Hammer" }],
        ["Paradox Cave Lower - Middle", true, new string[] { "OcarinaActive", "Hookshot" }],
        ["Paradox Cave Lower - Middle", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" }],
        ["Paradox Cave Lower - Middle", true, new string[] { "PowerGlove", "Lamp", "Hookshot" }],
        ["Paradox Cave Lower - Middle", true, new string[] { "TitansMitt", "Lamp", "Hookshot" }],
        ["Paradox Cave Lower - Middle", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Lower - Middle", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Lower - Middle", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Lower - Middle", true, new string[] { "OcarinaActive", "MagicMirror", "Hammer" }],

        ["Paradox Cave Lower - Right", false, new string[] {  }],
        ["Paradox Cave Lower - Right", false, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" }],
        ["Paradox Cave Lower - Right", false, new string[] { "ProgressiveGlove", "Hookshot" }],
        ["Paradox Cave Lower - Right", false, new string[] { "OcarinaActive", "MagicMirror" }],
        ["Paradox Cave Lower - Right", false, new string[] { "OcarinaActive", "Hammer" }],
        ["Paradox Cave Lower - Right", true, new string[] { "OcarinaActive", "Hookshot" }],
        ["Paradox Cave Lower - Right", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" }],
        ["Paradox Cave Lower - Right", true, new string[] { "PowerGlove", "Lamp", "Hookshot" }],
        ["Paradox Cave Lower - Right", true, new string[] { "TitansMitt", "Lamp", "Hookshot" }],
        ["Paradox Cave Lower - Right", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Lower - Right", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Lower - Right", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Lower - Right", true, new string[] { "OcarinaActive", "MagicMirror", "Hammer" }],

        ["Paradox Cave Lower - Far Right", false, new string[] {  }],
        ["Paradox Cave Lower - Far Right", false, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" }],
        ["Paradox Cave Lower - Far Right", false, new string[] { "ProgressiveGlove", "Hookshot" }],
        ["Paradox Cave Lower - Far Right", false, new string[] { "OcarinaActive", "MagicMirror" }],
        ["Paradox Cave Lower - Far Right", false, new string[] { "OcarinaActive", "Hammer" }],
        ["Paradox Cave Lower - Far Right", true, new string[] { "OcarinaActive", "Hookshot" }],
        ["Paradox Cave Lower - Far Right", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" }],
        ["Paradox Cave Lower - Far Right", true, new string[] { "PowerGlove", "Lamp", "Hookshot" }],
        ["Paradox Cave Lower - Far Right", true, new string[] { "TitansMitt", "Lamp", "Hookshot" }],
        ["Paradox Cave Lower - Far Right", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Lower - Far Right", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Lower - Far Right", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Lower - Far Right", true, new string[] { "OcarinaActive", "MagicMirror", "Hammer" }],

        ["Paradox Cave Upper - Left", false, new string[] {  }],
        ["Paradox Cave Upper - Left", false, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" }],
        ["Paradox Cave Upper - Left", false, new string[] { "ProgressiveGlove", "Hookshot" }],
        ["Paradox Cave Upper - Left", false, new string[] { "OcarinaActive", "MagicMirror" }],
        ["Paradox Cave Upper - Left", false, new string[] { "OcarinaActive", "Hammer" }],
        ["Paradox Cave Upper - Left", true, new string[] { "OcarinaActive", "Hookshot" }],
        ["Paradox Cave Upper - Left", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" }],
        ["Paradox Cave Upper - Left", true, new string[] { "PowerGlove", "Lamp", "Hookshot" }],
        ["Paradox Cave Upper - Left", true, new string[] { "TitansMitt", "Lamp", "Hookshot" }],
        ["Paradox Cave Upper - Left", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Upper - Left", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Upper - Left", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Upper - Left", true, new string[] { "OcarinaActive", "MagicMirror", "Hammer" }],

        ["Paradox Cave Upper - Right", false, new string[] {  }],
        ["Paradox Cave Upper - Right", false, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" }],
        ["Paradox Cave Upper - Right", false, new string[] { "ProgressiveGlove", "Hookshot" }],
        ["Paradox Cave Upper - Right", false, new string[] { "OcarinaActive", "MagicMirror" }],
        ["Paradox Cave Upper - Right", false, new string[] { "OcarinaActive", "Hammer" }],
        ["Paradox Cave Upper - Right", true, new string[] { "OcarinaActive", "Hookshot" }],
        ["Paradox Cave Upper - Right", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" }],
        ["Paradox Cave Upper - Right", true, new string[] { "PowerGlove", "Lamp", "Hookshot" }],
        ["Paradox Cave Upper - Right", true, new string[] { "TitansMitt", "Lamp", "Hookshot" }],
        ["Paradox Cave Upper - Right", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Upper - Right", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Upper - Right", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "Hammer" }],
        ["Paradox Cave Upper - Right", true, new string[] { "OcarinaActive", "MagicMirror", "Hammer" }],

        ["Floating Island Item", false, new string[] {  }],
        ["Floating Island Item", false, new string[] { "TitansMitt", "Lamp", "MagicMirror", "Hammer" }],
        ["Floating Island Item", true, new string[] { "MoonPearl", "TitansMitt", "Lamp", "MagicMirror", "Hammer" }],
    ];

    [TestMethod]
    [DynamicData(nameof(TestData), DynamicDataDisplayName = nameof(GetLogicTestDisplayNames), DynamicDataDisplayNameDeclaringType = typeof(LogicTestBase))]
    public override void TestLogic(string location, bool expected, string[] inventory)
    {
        base.TestLogic(location, expected, inventory);
    }

    [TestMethod]
    [DynamicData(nameof(TestDataWithMedallion))]
    public void TestLogicWithMedallion(string location, bool expected, string medallion, string[] inventory)
    {
        base.TestLogic(location, expected, inventory.Concat(new[] { "TurtleRockEntry" + medallion }).ToArray());
    }
}
