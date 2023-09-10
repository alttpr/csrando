using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Inverted.NoGlitches;

[TestClass]
public class ThievesTownTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Thieves' Town - Attic", false, new string[] {  } },
        new object[] { "Thieves' Town - Attic", false, new string[] { "BigKeyD4" } },
        new object[] { "Thieves' Town - Attic", false, new string[] { "KeyD4" } },
        new object[] { "Thieves' Town - Attic", true, new string[] { "BigKeyD4", "KeyD4" } },

        new object[] { "Thieves' Town - Big Key Chest", true, new string[] {  } },

        new object[] { "Thieves' Town - Map Chest", true, new string[] {  } },

        new object[] { "Thieves' Town - Compass Chest", true, new string[] {  } },

        new object[] { "Thieves' Town - Ambush Chest", true, new string[] {  } },

        new object[] { "Thieves' Town - Big Chest", false, new string[] {  } },
        new object[] { "Thieves' Town - Big Chest", false, new string[] { "KeyD4", "BigKeyD4" } },
        new object[] { "Thieves' Town - Big Chest", false, new string[] { "Hammer", "KeyD4" } },
        new object[] { "Thieves' Town - Big Chest", true, new string[] { "Hammer", "KeyD4", "BigKeyD4" } },

        new object[] { "Thieves' Town - Blind's Cell", false, new string[] {  } },
        new object[] { "Thieves' Town - Blind's Cell", true, new string[] { "BigKeyD4" } },

        new object[] { "Thieves' Town - Boss", false, new string[] {  } },
        new object[] { "Thieves' Town - Boss", false, new string[] { "KeyD4" } },
        new object[] { "Thieves' Town - Boss", false, new string[] { "BigKeyD4" } },
        new object[] { "Thieves' Town - Boss", true, new string[] { "L1Sword", "KeyD4", "BigKeyD4" } },
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
