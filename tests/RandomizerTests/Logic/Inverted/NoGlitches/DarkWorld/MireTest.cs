using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Inverted.NoGlitches.DarkWorld;

[TestClass]
public class MireTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Mire Shed - Left", false, new string[] {  } },
        //new object[] { "Mire Shed - Left", false, [], new string[] { "OcarinaInactive", "MagicMirror" } },
        new object[] { "Mire Shed - Left", true, new string[] { "MoonPearl", "OcarinaInactive", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Mire Shed - Left", true, new string[] { "MoonPearl", "OcarinaInactive", "ProgressiveGlove", "Hammer" } },
        new object[] { "Mire Shed - Left", true, new string[] { "MoonPearl", "OcarinaInactive", "AgahnimDefeated" } },
        new object[] { "Mire Shed - Left", true, new string[] { "MagicMirror", "AgahnimDefeated" } },

        new object[] { "Mire Shed - Right", false, new string[] {  } },
        //new object[] { "Mire Shed - Right", false, [], new string[] { "OcarinaInactive", "MagicMirror" } },
        new object[] { "Mire Shed - Right", true, new string[] { "MoonPearl", "OcarinaInactive", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Mire Shed - Right", true, new string[] { "MoonPearl", "OcarinaInactive", "ProgressiveGlove", "Hammer" } },
        new object[] { "Mire Shed - Right", true, new string[] { "MoonPearl", "OcarinaInactive", "AgahnimDefeated" } },
        new object[] { "Mire Shed - Right", true, new string[] { "MagicMirror", "AgahnimDefeated" } },
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
