using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Open.NoGlitches;

[TestClass]
public class HyruleCastleTowerTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Agahnims Tower - First Chest", false, new string[] {  } },
        new object[] { "Agahnims Tower - First Chest", true, new string[] { "L2Sword" } },
        new object[] { "Agahnims Tower - First Chest", true, new string[] { "Cape" } },

        new object[] { "Agahnims Tower - Second Chest", false, new string[] {  } },
        new object[] { "Agahnims Tower - Second Chest", false, new string[] { "L2Sword" } },
        new object[] { "Agahnims Tower - Second Chest", true, new string[] { "KeyA1", "L2Sword", "Lamp" } },
        new object[] { "Agahnims Tower - Second Chest", true, new string[] { "KeyA1", "Cape", "Lamp" } },

        new object[] { "Agahnims Tower - Boss", false, new string[] {  } },
        new object[] { "Agahnims Tower - Boss", false, new string[] { "KeyA1", "KeyA1", "Cape", "Lamp" } },
        new object[] { "Agahnims Tower - Boss", true, new string[] { "KeyA1", "KeyA1", "L2Sword", "Lamp" } },
        new object[] { "Agahnims Tower - Boss", true, new string[] { "KeyA1", "KeyA1", "L1Sword", "Cape", "Lamp" } },
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
