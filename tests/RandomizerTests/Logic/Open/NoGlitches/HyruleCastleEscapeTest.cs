using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Open.NoGlitches;

[TestClass]
public class HyruleCastleEscapeTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Sanctuary Chest", true, new string[] {  } },

        new object[] { "Sewers - Secret Room - Left", false, new string[] {  } },
        new object[] { "Sewers - Secret Room - Left", true, new string[] { "ProgressiveGlove" } },
        new object[] { "Sewers - Secret Room - Left", true, new string[] { "PowerGlove" } },
        new object[] { "Sewers - Secret Room - Left", true, new string[] { "TitansMitt" } },
        new object[] { "Sewers - Secret Room - Left", true, new string[] { "Lamp", "KeyH2" } },

        new object[] { "Sewers - Secret Room - Middle", false, new string[] {  } },
        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "ProgressiveGlove" } },
        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "PowerGlove" } },
        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "TitansMitt" } },
        new object[] { "Sewers - Secret Room - Middle", true, new string[] { "Lamp", "KeyH2" } },

        new object[] { "Sewers - Secret Room - Right", false, new string[] {  } },
        new object[] { "Sewers - Secret Room - Right", true, new string[] { "ProgressiveGlove" } },
        new object[] { "Sewers - Secret Room - Right", true, new string[] { "PowerGlove" } },
        new object[] { "Sewers - Secret Room - Right", true, new string[] { "TitansMitt" } },
        new object[] { "Sewers - Secret Room - Right", true, new string[] { "Lamp", "KeyH2" } },

        new object[] { "Sewers - Dark Cross Chest", true, new string[] { "Lamp" } },

        new object[] { "Hyrule Castle - Boomerang Chest", false, new string[] { "Lamp" } },
        new object[] { "Hyrule Castle - Boomerang Chest", false, new string[] { "PowerGlove" } },
        new object[] { "Hyrule Castle - Boomerang Chest", true, new string[] { "KeyH2" } },

        new object[] { "Hyrule Castle - Map Chest", true, new string[] {  } },

        new object[] { "Hyrule Castle - Zelda's Cell", false, new string[] { "Lamp" } },
        new object[] { "Hyrule Castle - Zelda's Cell", false, new string[] { "PowerGlove" } },
        new object[] { "Hyrule Castle - Zelda's Cell", true, new string[] { "KeyH2" } },

        new object[] { "Link's Uncle", true, new string[] {  } },

        new object[] { "Secret Passage", true, new string[] {  } },
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
