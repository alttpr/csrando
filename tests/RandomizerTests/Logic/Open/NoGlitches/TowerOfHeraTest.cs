using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Open.NoGlitches;

[TestClass]
public class TowerOfHeraTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Tower Of Hera - Big Key Chest", false, new string[] {  } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "OcarinaActive", "MagicMirror", "KeyP3" } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "ProgressiveGlove", "MagicMirror", "KeyP3" } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "PowerGlove", "MagicMirror", "KeyP3" } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "TitansMitt", "MagicMirror", "KeyP3" } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "OcarinaActive", "Hookshot", "Hammer", "KeyP3" } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "ProgressiveGlove", "Hookshot", "Hammer", "KeyP3" } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "PowerGlove", "Hookshot", "Hammer", "KeyP3" } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "Lamp", "TitansMitt", "Hookshot", "Hammer", "KeyP3" } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "FireRod", "OcarinaActive", "MagicMirror", "KeyP3" } },
        new object[] { "Tower Of Hera - Big Key Chest", true, new string[] { "FireRod", "OcarinaActive", "Hookshot", "Hammer", "KeyP3" } },

        new object[] { "Tower Of Hera - Basement Cage", false, new string[] {  } },
        new object[] { "Tower Of Hera - Basement Cage", true, new string[] { "OcarinaActive", "MagicMirror" } },
        new object[] { "Tower Of Hera - Basement Cage", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" } },
        new object[] { "Tower Of Hera - Basement Cage", true, new string[] { "PowerGlove", "Lamp", "MagicMirror" } },
        new object[] { "Tower Of Hera - Basement Cage", true, new string[] { "TitansMitt", "Lamp", "MagicMirror" } },
        new object[] { "Tower Of Hera - Basement Cage", true, new string[] { "OcarinaActive", "Hookshot", "Hammer" } },
        new object[] { "Tower Of Hera - Basement Cage", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot", "Hammer" } },
        new object[] { "Tower Of Hera - Basement Cage", true, new string[] { "PowerGlove", "Lamp", "Hookshot", "Hammer" } },
        new object[] { "Tower Of Hera - Basement Cage", true, new string[] { "TitansMitt", "Lamp", "Hookshot", "Hammer" } },

        new object[] { "Tower Of Hera - Map Chest", false, new string[] {  } },
        new object[] { "Tower Of Hera - Map Chest", true, new string[] { "OcarinaActive", "MagicMirror" } },
        new object[] { "Tower Of Hera - Map Chest", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" } },
        new object[] { "Tower Of Hera - Map Chest", true, new string[] { "PowerGlove", "Lamp", "MagicMirror" } },
        new object[] { "Tower Of Hera - Map Chest", true, new string[] { "TitansMitt", "Lamp", "MagicMirror" } },
        new object[] { "Tower Of Hera - Map Chest", true, new string[] { "OcarinaActive", "Hookshot", "Hammer" } },
        new object[] { "Tower Of Hera - Map Chest", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot", "Hammer" } },
        new object[] { "Tower Of Hera - Map Chest", true, new string[] { "PowerGlove", "Lamp", "Hookshot", "Hammer" } },
        new object[] { "Tower Of Hera - Map Chest", true, new string[] { "TitansMitt", "Lamp", "Hookshot", "Hammer" } },

        new object[] { "Tower Of Hera - Compass Chest", false, new string[] {  } },
        new object[] { "Tower Of Hera - Compass Chest", true, new string[] { "OcarinaActive", "MagicMirror", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Compass Chest", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Compass Chest", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Compass Chest", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Compass Chest", true, new string[] { "OcarinaActive", "Hookshot", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Compass Chest", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Compass Chest", true, new string[] { "PowerGlove", "Lamp", "Hookshot", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Compass Chest", true, new string[] { "TitansMitt", "Lamp", "Hookshot", "Hammer", "BigKeyP3" } },

        new object[] { "Tower Of Hera - Big Chest", false, new string[] {  } },
        new object[] { "Tower Of Hera - Big Chest", true, new string[] { "OcarinaActive", "MagicMirror", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Big Chest", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Big Chest", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Big Chest", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Big Chest", true, new string[] { "OcarinaActive", "Hookshot", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Big Chest", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Big Chest", true, new string[] { "PowerGlove", "Lamp", "Hookshot", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Big Chest", true, new string[] { "TitansMitt", "Lamp", "Hookshot", "Hammer", "BigKeyP3" } },

        new object[] { "Tower Of Hera - Boss", false, new string[] {  } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "OcarinaActive", "MagicMirror", "BigKeyP3", "ProgressiveSword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "OcarinaActive", "MagicMirror", "BigKeyP3", "UncleSword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "OcarinaActive", "MagicMirror", "BigKeyP3", "MasterSword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "OcarinaActive", "MagicMirror", "BigKeyP3", "L3Sword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "OcarinaActive", "MagicMirror", "BigKeyP3", "L4Sword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BigKeyP3", "UncleSword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BigKeyP3", "MasterSword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BigKeyP3", "ProgressiveSword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BigKeyP3", "L3Sword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BigKeyP3", "L4Sword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BigKeyP3", "UncleSword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BigKeyP3", "MasterSword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BigKeyP3", "ProgressiveSword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BigKeyP3", "L3Sword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BigKeyP3", "L4Sword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BigKeyP3", "UncleSword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BigKeyP3", "MasterSword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BigKeyP3", "ProgressiveSword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BigKeyP3", "L3Sword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BigKeyP3", "L4Sword" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "OcarinaActive", "Hookshot", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "PowerGlove", "Lamp", "Hookshot", "Hammer", "BigKeyP3" } },
        new object[] { "Tower Of Hera - Boss", true, new string[] { "TitansMitt", "Lamp", "Hookshot", "Hammer", "BigKeyP3" } },
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
