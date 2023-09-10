using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Inverted.NoGlitches;

[TestClass]
public class TowerOfHeraTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Tower Of Hera - Big Key Chest", false, new string[] {  } },
        new object[] { "Tower Of Hera - Big Key Chest", false, new string[] { "Lamp", "MoonPearl", "OcarinaActive", "Hookshot", "KeyP3" } },
        new object[] { "Tower Of Hera - Big Key Chest", false, new string[] { "Lamp", "Hammer", "OcarinaActive", "Hookshot" } },
        new object[] { "Tower Of Hera - Big Key Chest", false, new string[] { "Lamp", "Hammer", "MoonPearl", "OcarinaActive", "Hookshot" } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "Hammer", "MoonPearl", "OcarinaActive", "Hookshot", "KeyP3" } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "Hammer", "MoonPearl", "ProgressiveGlove", "Hookshot", "KeyP3" } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "Hammer", "MoonPearl", "PowerGlove", "Hookshot", "KeyP3" } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "Hammer", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "KeyP3" } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "Hammer", "MoonPearl", "TitansMitt", "KeyP3" } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "FireRod", "Hammer", "MoonPearl", "OcarinaInactive", "AgahnimDefeated", "Hookshot", "KeyP3" } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "FireRod", "Hammer", "MoonPearl", "OcarinaInactive", "ProgressiveGlove", "Hookshot", "KeyP3" } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "FireRod", "Hammer", "MoonPearl", "OcarinaInactive", "PowerGlove", "Hookshot", "KeyP3" } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "FireRod", "Hammer", "MoonPearl", "OcarinaInactive", "ProgressiveGlove", "ProgressiveGlove", "KeyP3" } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "FireRod", "Hammer", "MoonPearl", "OcarinaInactive", "TitansMitt", "KeyP3" } },

        new object[] { "Tower Of Hera - Basement Cage", false, new string[] {  } },
        new object[] { "Tower Of Hera - Basement Cage", false, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "Hammer" } },
        new object[] { "Tower Of Hera - Basement Cage", true, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer" } },
        new object[] { "Tower Of Hera - Basement Cage", true, new string[] { "PowerGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer" } },
        new object[] { "Tower Of Hera - Basement Cage", true, new string[] { "TitansMitt", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer" } },
        new object[] { "Tower Of Hera - Basement Cage", true, new string[] { "ProgressiveGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer" } },
        new object[] { "Tower Of Hera - Basement Cage", true, new string[] { "PowerGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer" } },
        new object[] { "Tower Of Hera - Basement Cage", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "Lamp", "Hammer" } },
        new object[] { "Tower Of Hera - Basement Cage", true, new string[] { "TitansMitt", "MoonPearl", "Lamp", "Hammer" } },

        new object[] { "Tower Of Hera - Map Chest", false, new string[] {  } },
        new object[] { "Tower Of Hera - Map Chest", false, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "Hammer" } },
        new object[] { "Tower Of Hera - Map Chest", true, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer" } },
        new object[] { "Tower Of Hera - Map Chest", true, new string[] { "PowerGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer" } },
        new object[] { "Tower Of Hera - Map Chest", true, new string[] { "TitansMitt", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer" } },
        new object[] { "Tower Of Hera - Map Chest", true, new string[] { "ProgressiveGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer" } },
        new object[] { "Tower Of Hera - Map Chest", true, new string[] { "PowerGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer" } },
        new object[] { "Tower Of Hera - Map Chest", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "Lamp", "Hammer" } },
        new object[] { "Tower Of Hera - Map Chest", true, new string[] { "TitansMitt", "MoonPearl", "Lamp", "Hammer" } },

        new object[] { "Tower Of Hera - Compass Chest", false, new string[] {  } },
        new object[] { "Tower Of Hera - Compass Chest", false, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Compass Chest", false, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer" } },
        new object[] { "Tower Of Hera - Compass Chest", true, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Compass Chest", true, new string[] { "PowerGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Compass Chest", true, new string[] { "TitansMitt", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Compass Chest", true, new string[] { "ProgressiveGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Compass Chest", true, new string[] { "PowerGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Compass Chest", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "Lamp", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Compass Chest", true, new string[] { "TitansMitt", "MoonPearl", "Lamp", "Hammer", "BigKeyP3" } },

        new object[] { "Tower Of Hera - Big Chest", false, new string[] {  } },
        new object[] { "Tower Of Hera - Big Chest", false, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Big Chest", false, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer" } },
        new object[] { "Tower Of Hera - Big Chest", true, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Big Chest", true, new string[] { "PowerGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Big Chest", true, new string[] { "TitansMitt", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Big Chest", true, new string[] { "ProgressiveGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Big Chest", true, new string[] { "PowerGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Big Chest", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "Lamp", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Big Chest", true, new string[] { "TitansMitt", "MoonPearl", "Lamp", "Hammer", "BigKeyP3" } },

        new object[] { "Tower Of Hera - Boss", false, new string[] {  } },
        new object[] { "Tower Of Hera - Boss", false, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Boss", false, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Boss", false, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "ProgressiveGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "PowerGlove", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "TitansMitt", "OcarinaInactive", "Hookshot", "MoonPearl", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "ProgressiveGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "PowerGlove", "MoonPearl", "Lamp", "Hookshot", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "MoonPearl", "Lamp", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "TitansMitt", "MoonPearl", "Lamp", "Hammer", "BigKeyP3" } },
    };
    [TestMethod]
    [DynamicData(nameof(TestData))]
    public void TestLogic(string location, bool expected, string[] inventory)
    {
        var randomizer = new Randomizer(new[]
        {
            new Dictionary<string, object>()
            {
                { "mode.state", "inverted" },
                { "logic", "NoGlitches" },
            }
        });
        randomizer.AssumeItems(inventory.Select(i => Item.Get(i, 0)));
        Assert.AreEqual(expected, randomizer.CanReachLocation($"{location}:0"));
    }
    [TestMethod]
    public void TestKeyForKey()
    {
        var randomizer = new Randomizer(new[]
        {
            new Dictionary<string, object>()
            {
                { "mode.state", "inverted" },
                { "accessibility", "item" },
                { "logic", "NoGlitches" },
            }
        });
        randomizer.AssumeItems(new[] { "Lamp", "Hammer", "MoonPearl", "OcarinaActive", "Hookshot" }.Select(i => Item.Get(i, 0)));
        Assert.IsTrue(randomizer.CanReachLocation("Tower Of Hera - Big Key Chest:0"));
    }

}
