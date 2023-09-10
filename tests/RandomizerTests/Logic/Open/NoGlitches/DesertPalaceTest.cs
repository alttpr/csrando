using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Open.NoGlitches;

[TestClass]
public class DesertPalaceTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Desert Palace - Main Room - Center", false, new string[] {  } },
        new object[] { "Desert Palace - Main Room - Center", true, new string[] { "BookOfMudora" } },
        new object[] { "Desert Palace - Main Room - Center", true, new string[] { "OcarinaActive", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Desert Palace - Main Room - Center", true, new string[] { "OcarinaActive", "MagicMirror", "TitansMitt" } },

        new object[] { "Desert Palace - Map Chest", false, new string[] {  } },
        new object[] { "Desert Palace - Map Chest", true, new string[] { "BookOfMudora" } },
        new object[] { "Desert Palace - Map Chest", true, new string[] { "OcarinaActive", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove" } },
        new object[] { "Desert Palace - Map Chest", true, new string[] { "OcarinaActive", "MagicMirror", "TitansMitt" } },

        new object[] { "Desert Palace - Big Chest", false, new string[] {  } },
        new object[] { "Desert Palace - Big Chest", true, new string[] { "BookOfMudora", "BigKeyP2" } },
        new object[] { "Desert Palace - Big Chest", true, new string[] { "OcarinaActive", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove", "BigKeyP2" } },
        new object[] { "Desert Palace - Big Chest", true, new string[] { "OcarinaActive", "MagicMirror", "TitansMitt", "BigKeyP2" } },

        new object[] { "Desert Palace - Torch", false, new string[] {  } },
        new object[] { "Desert Palace - Torch", true, new string[] { "BookOfMudora", "PegasusBoots" } },
        new object[] { "Desert Palace - Torch", true, new string[] { "OcarinaActive", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove", "PegasusBoots" } },
        new object[] { "Desert Palace - Torch", true, new string[] { "OcarinaActive", "MagicMirror", "TitansMitt", "PegasusBoots" } },

        new object[] { "Desert Palace - Compass Chest", false, new string[] {  } },
        new object[] { "Desert Palace - Compass Chest", true, new string[] { "BookOfMudora", "KeyP2" } },
        new object[] { "Desert Palace - Compass Chest", true, new string[] { "OcarinaActive", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove", "KeyP2" } },
        new object[] { "Desert Palace - Compass Chest", true, new string[] { "OcarinaActive", "MagicMirror", "TitansMitt", "KeyP2" } },

        new object[] { "Desert Palace - Big Key Chest", false, new string[] {  } },
        new object[] { "Desert Palace - Big Key Chest", true, new string[] { "BookOfMudora", "KeyP2" } },
        new object[] { "Desert Palace - Big Key Chest", true, new string[] { "OcarinaActive", "MagicMirror", "ProgressiveGlove", "ProgressiveGlove", "KeyP2" } },
        new object[] { "Desert Palace - Big Key Chest", true, new string[] { "OcarinaActive", "MagicMirror", "TitansMitt", "KeyP2" } },

        new object[] { "Desert Palace - Boss", false, new string[] {  } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "BookOfMudora", "Lamp", "ProgressiveGlove", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "BookOfMudora", "Lamp", "PowerGlove", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "BookOfMudora", "Lamp", "TitansMitt", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "BookOfMudora", "FireRod", "ProgressiveGlove", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "BookOfMudora", "FireRod", "PowerGlove", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "BookOfMudora", "FireRod", "TitansMitt", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "OcarinaActive", "MagicMirror", "Lamp", "ProgressiveGlove", "ProgressiveGlove", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "OcarinaActive", "MagicMirror", "Lamp", "TitansMitt", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "OcarinaActive", "MagicMirror", "FireRod", "ProgressiveGlove", "ProgressiveGlove", "BigKeyP2" } },
        new object[] { "Desert Palace - Boss", true, new string[] { "UncleSword", "KeyP2", "OcarinaActive", "MagicMirror", "FireRod", "TitansMitt", "BigKeyP2" } },
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
