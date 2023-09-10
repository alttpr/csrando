using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Open.NoGlitches.DarkWorld;

[TestClass]
public class NorthWestTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Brewery", false, new string[] {  } },
        new object[] { "Brewery", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Brewery", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Brewery", true, new string[] { "MoonPearl", "TitansMitt" } },
        new object[] { "Brewery", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Brewery", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Brewery", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Brewery", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" } },
        new object[] { "Brewery", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" } },

        new object[] { "C-Shaped House", false, new string[] {  } },
        new object[] { "C-Shaped House", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "C-Shaped House", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "C-Shaped House", true, new string[] { "MoonPearl", "TitansMitt" } },
        new object[] { "C-Shaped House", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "C-Shaped House", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "C-Shaped House", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "C-Shaped House", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" } },
        new object[] { "C-Shaped House", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" } },

        new object[] { "Chest Game", false, new string[] {  } },
        new object[] { "Chest Game", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Chest Game", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Chest Game", true, new string[] { "MoonPearl", "TitansMitt" } },
        new object[] { "Chest Game", true, new string[] { "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Chest Game", true, new string[] { "MoonPearl", "PowerGlove", "Hammer" } },
        new object[] { "Chest Game", true, new string[] { "MoonPearl", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Chest Game", true, new string[] { "MoonPearl", "AgahnimDefeated", "PowerGlove", "Hookshot" } },
        new object[] { "Chest Game", true, new string[] { "MoonPearl", "AgahnimDefeated", "Flippers", "Hookshot" } },

        new object[] { "Hammer Pegs", false, new string[] {  } },
        new object[] { "Hammer Pegs", false, new string[] { "Hammer", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Hammer Pegs", false, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Hammer Pegs", false, new string[] { "MoonPearl", "Hammer", "ProgressiveGlove" } },
        new object[] { "Hammer Pegs", true, new string[] { "MoonPearl", "Hammer", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Hammer Pegs", true, new string[] { "MoonPearl", "Hammer", "TitansMitt" } },

        new object[] { "Bumper Cave", false, new string[] {  } },
        new object[] { "Bumper Cave", false, new string[] { "Cape", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Bumper Cave", false, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Bumper Cave", false, new string[] { "MoonPearl", "Cape", "ProgressiveGlove" } },
        new object[] { "Bumper Cave", true, new string[] { "MoonPearl", "Cape", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Bumper Cave", true, new string[] { "MoonPearl", "Cape", "TitansMitt" } },
        new object[] { "Bumper Cave", true, new string[] { "MoonPearl", "Cape", "ProgressiveGlove", "Hammer" } },
        new object[] { "Bumper Cave", true, new string[] { "MoonPearl", "Cape", "PowerGlove", "Hammer" } },
        new object[] { "Bumper Cave", true, new string[] { "MoonPearl", "Cape", "AgahnimDefeated", "ProgressiveGlove", "Hookshot" } },
        new object[] { "Bumper Cave", true, new string[] { "MoonPearl", "Cape", "AgahnimDefeated", "PowerGlove", "Hookshot" } },

        new object[] { "Blacksmith", false, new string[] {  } },
        new object[] { "Blacksmith", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Blacksmith", false, new string[] { "MoonPearl", "ProgressiveGlove" } },
        new object[] { "Blacksmith", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Blacksmith", true, new string[] { "MoonPearl", "TitansMitt" } },

        new object[] { "Purple Chest", false, new string[] {  } },
        new object[] { "Purple Chest", false, new string[] { "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Purple Chest", false, new string[] { "MoonPearl", "ProgressiveGlove" } },
        new object[] { "Purple Chest", true, new string[] { "MoonPearl", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Purple Chest", true, new string[] { "MoonPearl", "TitansMitt" } },
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
