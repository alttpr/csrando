using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Open.NoGlitches.DeathMountain;

[TestClass]
public class EastTest
{
    public static IEnumerable<object[]> TestDataWithMedallion => new[]
    {
        new object[] { "Mimic Cave", false, "Quake", new string[] {  } },
        new object[] { "Mimic Cave", false, "Quake", new string[] { "Quake" } },
        new object[] { "Mimic Cave", false, "Quake", new string[] { "Gloves", "OcarinaActive" } },
        new object[] { "Mimic Cave", false, "Quake", new string[] { "Hammer" } },
        new object[] { "Mimic Cave", false, "Quake", new string[] { "MagicMirror" } },
        new object[] { "Mimic Cave", false, "Quake", new string[] { "MoonPearl" } },
        new object[] { "Mimic Cave", false, "Quake", new string[] { "CaneOfSomaria" } },
        new object[] { "Mimic Cave", false, "Ether", new string[] {  } },
        new object[] { "Mimic Cave", false, "Ether", new string[] { "Ether" } },
        new object[] { "Mimic Cave", false, "Ether", new string[] { "Gloves", "OcarinaActive" } },
        new object[] { "Mimic Cave", false, "Ether", new string[] { "Hammer" } },
        new object[] { "Mimic Cave", false, "Ether", new string[] { "MagicMirror" } },
        new object[] { "Mimic Cave", false, "Ether", new string[] { "MoonPearl" } },
        new object[] { "Mimic Cave", false, "Ether", new string[] { "CaneOfSomaria" } },
        new object[] { "Mimic Cave", false, "Bombos", new string[] {  } },
        new object[] { "Mimic Cave", false, "Bombos", new string[] { "Bombos" } },
        new object[] { "Mimic Cave", false, "Bombos", new string[] { "Gloves", "OcarinaActive" } },
        new object[] { "Mimic Cave", false, "Bombos", new string[] { "Hammer" } },
        new object[] { "Mimic Cave", false, "Bombos", new string[] { "MagicMirror" } },
        new object[] { "Mimic Cave", false, "Bombos", new string[] { "MoonPearl" } },
        new object[] { "Mimic Cave", false, "Bombos", new string[] { "CaneOfSomaria" } },
    };

    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Spiral Cave", false, new string[] {  } },
        new object[] { "Spiral Cave", false, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" } },
        new object[] { "Spiral Cave", false, new string[] { "ProgressiveGlove", "Hookshot" } },
        new object[] { "Spiral Cave", false, new string[] { "OcarinaActive", "MagicMirror" } },
        new object[] { "Spiral Cave", false, new string[] { "OcarinaActive", "Hammer" } },
        new object[] { "Spiral Cave", true, new string[] { "OcarinaActive", "Hookshot" } },
        new object[] { "Spiral Cave", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" } },
        new object[] { "Spiral Cave", true, new string[] { "PowerGlove", "Lamp", "Hookshot" } },
        new object[] { "Spiral Cave", true, new string[] { "TitansMitt", "Lamp", "Hookshot" } },
        new object[] { "Spiral Cave", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Spiral Cave", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Spiral Cave", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Spiral Cave", true, new string[] { "OcarinaActive", "MagicMirror", "Hammer" } },

        new object[] { "Paradox Cave Lower - Far Left", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Far Left", false, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" } },
        new object[] { "Paradox Cave Lower - Far Left", false, new string[] { "ProgressiveGlove", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Far Left", false, new string[] { "OcarinaActive", "MagicMirror" } },
        new object[] { "Paradox Cave Lower - Far Left", false, new string[] { "OcarinaActive", "Hammer" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "OcarinaActive", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "PowerGlove", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "TitansMitt", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "OcarinaActive", "MagicMirror", "Hammer" } },

        new object[] { "Paradox Cave Lower - Left", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Left", false, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" } },
        new object[] { "Paradox Cave Lower - Left", false, new string[] { "ProgressiveGlove", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Left", false, new string[] { "OcarinaActive", "MagicMirror" } },
        new object[] { "Paradox Cave Lower - Left", false, new string[] { "OcarinaActive", "Hammer" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "OcarinaActive", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "PowerGlove", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "TitansMitt", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "OcarinaActive", "MagicMirror", "Hammer" } },

        new object[] { "Paradox Cave Lower - Middle", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Middle", false, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" } },
        new object[] { "Paradox Cave Lower - Middle", false, new string[] { "ProgressiveGlove", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Middle", false, new string[] { "OcarinaActive", "MagicMirror" } },
        new object[] { "Paradox Cave Lower - Middle", false, new string[] { "OcarinaActive", "Hammer" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "OcarinaActive", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "PowerGlove", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "TitansMitt", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "OcarinaActive", "MagicMirror", "Hammer" } },

        new object[] { "Paradox Cave Lower - Right", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Right", false, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" } },
        new object[] { "Paradox Cave Lower - Right", false, new string[] { "ProgressiveGlove", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Right", false, new string[] { "OcarinaActive", "MagicMirror" } },
        new object[] { "Paradox Cave Lower - Right", false, new string[] { "OcarinaActive", "Hammer" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "OcarinaActive", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "PowerGlove", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "TitansMitt", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "OcarinaActive", "MagicMirror", "Hammer" } },

        new object[] { "Paradox Cave Lower - Far Right", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Far Right", false, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" } },
        new object[] { "Paradox Cave Lower - Far Right", false, new string[] { "ProgressiveGlove", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Far Right", false, new string[] { "OcarinaActive", "MagicMirror" } },
        new object[] { "Paradox Cave Lower - Far Right", false, new string[] { "OcarinaActive", "Hammer" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "OcarinaActive", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "PowerGlove", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "TitansMitt", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "OcarinaActive", "MagicMirror", "Hammer" } },

        new object[] { "Paradox Cave Upper - Left", false, new string[] {  } },
        new object[] { "Paradox Cave Upper - Left", false, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" } },
        new object[] { "Paradox Cave Upper - Left", false, new string[] { "ProgressiveGlove", "Hookshot" } },
        new object[] { "Paradox Cave Upper - Left", false, new string[] { "OcarinaActive", "MagicMirror" } },
        new object[] { "Paradox Cave Upper - Left", false, new string[] { "OcarinaActive", "Hammer" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "OcarinaActive", "Hookshot" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "PowerGlove", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "TitansMitt", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "OcarinaActive", "MagicMirror", "Hammer" } },

        new object[] { "Paradox Cave Upper - Right", false, new string[] {  } },
        new object[] { "Paradox Cave Upper - Right", false, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" } },
        new object[] { "Paradox Cave Upper - Right", false, new string[] { "ProgressiveGlove", "Hookshot" } },
        new object[] { "Paradox Cave Upper - Right", false, new string[] { "OcarinaActive", "MagicMirror" } },
        new object[] { "Paradox Cave Upper - Right", false, new string[] { "OcarinaActive", "Hammer" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "OcarinaActive", "Hookshot" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "PowerGlove", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "TitansMitt", "Lamp", "Hookshot" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "OcarinaActive", "MagicMirror", "Hammer" } },

        new object[] { "Floating Island", false, new string[] {  } },
        new object[] { "Floating Island", false, new string[] { "TitansMitt", "Lamp", "MagicMirror", "Hammer" } },
        new object[] { "Floating Island", true, new string[] { "MoonPearl", "TitansMitt", "Lamp", "MagicMirror", "Hammer" } },
    };
    [TestMethod]
    [DynamicData(nameof(TestData))]
    public void TestLogic(string location, bool expected, string[] inventory)
    {
        var randomizer = new Randomizer(new[]
        {
            new RandomizerConfig
            {
                Glitches = GlitchesOption.None,
                State = StateOption.Open,
            }
        });
        randomizer.AssumeItems(inventory.Select(i => Item.Get(i, 0)));
        Assert.AreEqual(expected, randomizer.CanReachLocation($"{location}:0"));
    }
    [TestMethod]
    [DynamicData(nameof(TestDataWithMedallion))]
    public void TestLogicWithMedallion(string location, bool expected, string medallion, string[] inventory)
    {
        var randomizer = new Randomizer(new[]
        {
            new RandomizerConfig
            {
                Glitches = GlitchesOption.None,
                State = StateOption.Open,
            }
        });
        randomizer.AssumeItems(inventory.Concat(new[] { "TurtleRockEntry" + medallion }).Select(i => Item.Get(i, 0)));
        Assert.AreEqual(expected, randomizer.CanReachLocation($"{location}:0"));
    }
}
