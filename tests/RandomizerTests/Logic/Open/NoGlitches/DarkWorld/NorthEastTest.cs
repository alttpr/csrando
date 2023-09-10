using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Open.NoGlitches.DarkWorld;

[TestClass]
public class NorthEastTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Catfish", false, new string[] {  } },
        new object[] { "Catfish", false, new string[] { "AgahnimDefeated", "ProgressiveGlove" } },
        new object[] { "Catfish", false, new string[] { "MoonPearl", "AgahnimDefeated" } },
        new object[] { "Catfish", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove" } },
        new object[] { "Catfish", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove" } },
        new object[] { "Catfish", true, new string[] { "MoonPearl", "AgahnimDefeated", "TitansMitt" } },
        new object[] { "Catfish", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Catfish", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Catfish", true, new string[] { "MoonPearl", "TitansMitt", "Flippers" } },

        new object[] { "Pyramid Item", false, new string[] {  } },
        new object[] { "Pyramid Item", true, new string[] { "AgahnimDefeated" } },
        new object[] { "Pyramid Item", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Pyramid Item", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Pyramid Item", true, new string[] { "MoonPearl", "TitansMitt", "Flippers" } },

        new object[] { "Pyramid Fairy - Left", false, new string[] {  } },
        new object[] { "Pyramid Fairy - Left", false, new string[] { "Crystal5", "Crystal6", "AgahnimDefeated", "Hammer" } },
        new object[] { "Pyramid Fairy - Left", false, new string[] { "MoonPearl", "Crystal6", "AgahnimDefeated", "Hammer" } },
        new object[] { "Pyramid Fairy - Left", false, new string[] { "MoonPearl", "Crystal5", "AgahnimDefeated", "Hammer" } },
        // can"t jump with red bomb
        new object[] { "Pyramid Fairy - Left", false, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "Hookshot", "Flippers" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "Hammer" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "TitansMitt", "Hammer" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "ProgressiveGlove", "Hammer" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "PowerGlove", "Hammer" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "TitansMitt", "MagicMirror" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "MagicMirror" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "PowerGlove", "Hookshot", "MagicMirror" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "Flippers", "Hookshot", "MagicMirror" } },

        new object[] { "Pyramid Fairy - Right", false, new string[] {  } },
        new object[] { "Pyramid Fairy - Right", false, new string[] { "Crystal5", "Crystal6", "AgahnimDefeated", "Hammer" } },
        new object[] { "Pyramid Fairy - Right", false, new string[] { "MoonPearl", "Crystal6", "AgahnimDefeated", "Hammer" } },
        new object[] { "Pyramid Fairy - Right", false, new string[] { "MoonPearl", "Crystal5", "AgahnimDefeated", "Hammer" } },
        new object[] { "Pyramid Fairy - Right", false, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "Hookshot", "Flippers" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "Hammer" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "TitansMitt", "Hammer" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "ProgressiveGlove", "Hammer" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "PowerGlove", "Hammer" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "TitansMitt", "MagicMirror" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "ProgressiveGlove", "Hookshot", "MagicMirror" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "PowerGlove", "Hookshot", "MagicMirror" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "MoonPearl", "Crystal5", "Crystal6", "AgahnimDefeated", "Flippers", "Hookshot", "MagicMirror" } },

        // @todo update this
        new object[] { "Ganon", false, new string[] {  } },
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
}
