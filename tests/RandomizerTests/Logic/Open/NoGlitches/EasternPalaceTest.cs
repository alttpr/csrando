using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Open.NoGlitches;

[TestClass]
public class EasternPalaceTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Eastern Palace - Compass Chest", true, new string[] {  } },

        new object[] { "Eastern Palace - Cannonball Chest", true, new string[] {  } },

        new object[] { "Eastern Palace - Big Chest", false, new string[] {  } },
        new object[] { "Eastern Palace - Big Chest", true, new string[] { "BigKeyP1" } },

        new object[] { "Eastern Palace - Map Chest", true, new string[] {  } },

        new object[] { "Eastern Palace - Big Key Chest", false, new string[] {  } },
        new object[] { "Eastern Palace - Big Key Chest", true, new string[] { "Lamp" } },

        new object[] { "Eastern Palace - Boss", false, new string[] {  } },
        new object[] { "Eastern Palace - Boss", true, new string[] { "Lamp", "BowAndArrows", "BigKeyP1" } },
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
