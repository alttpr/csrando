using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Open.NoGlitches.DarkWorld.DeathMountain;

[TestClass]
public class EastTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Superbunny Cave - Top", false, new string[] {  } },
        new object[] { "Superbunny Cave - Top", false, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Top", false, new string[] { "MoonPearl", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "Hammer", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "MoonPearl", "TitansMitt", "MagicMirror", "Hammer", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "Lamp" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "Lamp" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "Hammer", "Lamp" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "MoonPearl", "TitansMitt", "MagicMirror", "Hammer", "Lamp" } },

        new object[] { "Superbunny Cave - Bottom", false, new string[] {  } },
        new object[] { "Superbunny Cave - Bottom", false, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Bottom", false, new string[] { "MoonPearl", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "Hammer", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "TitansMitt", "MagicMirror", "Hammer", "OcarinaActive" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "Lamp" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "Lamp" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "Hammer", "Lamp" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "MoonPearl", "TitansMitt", "MagicMirror", "Hammer", "Lamp" } },

        new object[] { "Hookshot Cave - Bottom Right", false, new string[] {  } },
        new object[] { "Hookshot Cave - Bottom Right", false, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Bottom Right", false, new string[] { "MoonPearl", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "Hammer", "OcarinaActive", "PegasusBoots" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "MoonPearl", "TitansMitt", "MagicMirror", "Hammer", "OcarinaActive", "PegasusBoots" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "Lamp" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "Lamp" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "MagicMirror", "Hammer", "Lamp", "PegasusBoots" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "MoonPearl", "TitansMitt", "MagicMirror", "Hammer", "Lamp", "PegasusBoots" } },

        new object[] { "Hookshot Cave - Bottom Left", false, new string[] {  } },
        new object[] { "Hookshot Cave - Bottom Left", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Bottom Left", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Bottom Left", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "Lamp" } },
        new object[] { "Hookshot Cave - Bottom Left", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "Lamp" } },

        new object[] { "Hookshot Cave - Top Left", false, new string[] {  } },
        new object[] { "Hookshot Cave - Top Left", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Top Left", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Top Left", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "Lamp" } },
        new object[] { "Hookshot Cave - Top Left", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "Lamp" } },

        new object[] { "Hookshot Cave - Top Right", false, new string[] {  } },
        new object[] { "Hookshot Cave - Top Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Top Right", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "OcarinaActive" } },
        new object[] { "Hookshot Cave - Top Right", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hookshot", "Lamp" } },
        new object[] { "Hookshot Cave - Top Right", true, new string[] { "MoonPearl", "TitansMitt", "Hookshot", "Lamp" } },
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
