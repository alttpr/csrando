using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Inverted.NoGlitches.DarkWorld;

[TestClass]
public class NorthEastTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Catfish", false, new string[] {  } },
        new object[] { "Catfish", false, new string[] { "AgahnimDefeated", "MagicMirror" } },
        new object[] { "Catfish", true, new string[] { "AgahnimDefeated", "MagicMirror", "ProgressiveGlove" } },
        new object[] { "Catfish", true, new string[] { "AgahnimDefeated", "MagicMirror",  "PowerGlove" } },
        new object[] { "Catfish", true, new string[] { "AgahnimDefeated", "MagicMirror",  "TitansMitt" } },
        new object[] { "Catfish", true, new string[] { "ProgressiveGlove", "Hammer" } },
        new object[] { "Catfish", true, new string[] { "ProgressiveGlove", "Flippers" } },
        new object[] { "Catfish", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "MoonPearl" } },

        new object[] { "Pyramid Item", false, new string[] {  } },
        new object[] { "Pyramid Item", true, new string[] { "AgahnimDefeated", "MagicMirror" } },
        new object[] { "Pyramid Item", true, new string[] { "Hammer" } },
        new object[] { "Pyramid Item", true, new string[] { "Flippers", "ProgressiveGlove" } },
        new object[] { "Pyramid Item", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "MoonPearl" } },

        new object[] { "Pyramid Fairy - Left", false, new string[] {  } },
        new object[] { "Pyramid Fairy - Left", false, new string[] { "AgahnimDefeated", "MagicMirror", "Hammer" } },
        new object[] { "Pyramid Fairy - Left", false, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "Hammer" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "MagicMirror", "Hammer" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "MagicMirror", "ProgressiveGlove", "Flippers" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "MagicMirror", "PowerGlove", "Flippers" } },
        new object[] { "Pyramid Fairy - Left", true, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "MagicMirror", "TitansMitt", "Flippers" } },

        new object[] { "Pyramid Fairy - Right", false, new string[] {  } },
        new object[] { "Pyramid Fairy - Right", false, new string[] { "AgahnimDefeated", "MagicMirror", "Hammer" } },
        new object[] { "Pyramid Fairy - Right", false, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "Hammer" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "MagicMirror", "Hammer" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "MagicMirror", "ProgressiveGlove", "Flippers" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "MagicMirror", "PowerGlove", "Flippers" } },
        new object[] { "Pyramid Fairy - Right", true, new string[] { "AgahnimDefeated", "Crystal5", "Crystal6", "MagicMirror", "TitansMitt", "Flippers" } },
    };
    [TestMethod]
    [DynamicData(nameof(TestData))]
    public void TestLogic(string location, bool expected, string[] inventory)
    {
        var randomizer = new Randomizer(new[]
        {
            new Dictionary<string, object>()
            {
                { "mode.state", "open" },
                { "logic", "NoGlitches" },
            }
        });
        randomizer.AssumeItems(inventory.Select(i => Item.Get(i, 0)));
        Assert.AreEqual(expected, randomizer.CanReachLocation($"{location}:0"));
    }
}
