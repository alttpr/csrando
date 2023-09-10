using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Inverted.NoGlitches.DarkWorld.DeathMountain;

[TestClass]
public class EastTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Superbunny Cave - Top", false, new string[] {  } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "ProgressiveGlove", "Lamp" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive" } },
        new object[] { "Superbunny Cave - Top", true, new string[] { "Hammer", "ProgressiveGlove", "MoonPearl", "OcarinaInactive" } },

        new object[] { "Superbunny Cave - Bottom", false, new string[] {  } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "ProgressiveGlove", "Lamp" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive" } },
        new object[] { "Superbunny Cave - Bottom", true, new string[] { "Hammer", "ProgressiveGlove", "MoonPearl", "OcarinaInactive" } },

        new object[] { "Hookshot Cave - Bottom Right", false, new string[] {  } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "ProgressiveGlove", "Lamp", "PegasusBoots" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive", "PegasusBoots" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "ProgressiveGlove", "Hammer", "MoonPearl", "OcarinaInactive", "PegasusBoots" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive", "Hookshot" } },
        new object[] { "Hookshot Cave - Bottom Right", true, new string[] { "ProgressiveGlove", "Hammer", "MoonPearl", "OcarinaInactive", "Hookshot" } },

        new object[] { "Hookshot Cave - Bottom Left", false, new string[] {  } },
        new object[] { "Hookshot Cave - Bottom Left", false, new string[] { "ProgressiveGlove", "Lamp" } },
        new object[] { "Hookshot Cave - Bottom Left", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" } },
        new object[] { "Hookshot Cave - Bottom Left", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive", "Hookshot" } },
        new object[] { "Hookshot Cave - Bottom Left", true, new string[] { "ProgressiveGlove", "Hammer", "MoonPearl", "OcarinaInactive", "Hookshot" } },

        new object[] { "Hookshot Cave - Top Left", false, new string[] {  } },
        new object[] { "Hookshot Cave - Top Left", false, new string[] { "ProgressiveGlove", "Lamp" } },
        new object[] { "Hookshot Cave - Top Left", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" } },
        new object[] { "Hookshot Cave - Top Left", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive", "Hookshot" } },
        new object[] { "Hookshot Cave - Top Left", true, new string[] { "ProgressiveGlove", "Hammer", "MoonPearl", "OcarinaInactive", "Hookshot" } },

        new object[] { "Hookshot Cave - Top Right", false, new string[] {  } },
        new object[] { "Hookshot Cave - Top Right", false, new string[] { "ProgressiveGlove", "Lamp" } },
        new object[] { "Hookshot Cave - Top Right", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot" } },
        new object[] { "Hookshot Cave - Top Right", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "OcarinaInactive", "Hookshot" } },
        new object[] { "Hookshot Cave - Top Right", true, new string[] { "ProgressiveGlove", "Hammer", "MoonPearl", "OcarinaInactive", "Hookshot" } },
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
                State = StateOption.Inverted,
            }
        });
        randomizer.AssumeItems(inventory.Select(i => Item.Get(i, 0)));
        Assert.AreEqual(expected, randomizer.CanReachLocation($"{location}:0"));
    }
}
