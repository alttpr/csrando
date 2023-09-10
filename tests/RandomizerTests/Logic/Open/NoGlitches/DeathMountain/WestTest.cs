using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Open.NoGlitches.DeathMountain;

[TestClass]
public class WestTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Ether Tablet", false, new string[] {  } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaActive", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaActive", "MagicMirror", "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaActive", "MagicMirror", "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaActive", "MagicMirror", "BookOfMudora", "L4Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror", "BookOfMudora", "L4Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "MagicMirror", "BookOfMudora", "L4Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "MagicMirror", "BookOfMudora", "L4Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaActive", "Hammer", "Hookshot", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaActive", "Hammer", "Hookshot", "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaActive", "Hammer", "Hookshot", "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaActive", "Hammer", "Hookshot", "BookOfMudora", "L4Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "L4Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "L4Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "Hammer", "Hookshot", "BookOfMudora", "L4Sword" } },

        new object[] { "Old Man", false, new string[] {  } },
        new object[] { "Old Man", true, new string[] { "OcarinaActive", "Lamp" } },
        new object[] { "Old Man", true, new string[] { "ProgressiveGlove", "Lamp" } },
        new object[] { "Old Man", true, new string[] { "PowerGlove", "Lamp" } },
        new object[] { "Old Man", true, new string[] { "TitansMitt", "Lamp" } },

        new object[] { "Spectacle Rock Cave", false, new string[] {  } },
        new object[] { "Spectacle Rock Cave", true, new string[] { "OcarinaActive" } },
        new object[] { "Spectacle Rock Cave", true, new string[] { "ProgressiveGlove", "Lamp" } },
        new object[] { "Spectacle Rock Cave", true, new string[] { "PowerGlove", "Lamp" } },
        new object[] { "Spectacle Rock Cave", true, new string[] { "TitansMitt", "Lamp" } },

        new object[] { "Spectacle Rock", false, new string[] {  } },
        new object[] { "Spectacle Rock", true, new string[] { "OcarinaActive", "MagicMirror" } },
        new object[] { "Spectacle Rock", true, new string[] { "ProgressiveGlove", "Lamp", "MagicMirror" } },
        new object[] { "Spectacle Rock", true, new string[] { "PowerGlove", "Lamp", "MagicMirror" } },
        new object[] { "Spectacle Rock", true, new string[] { "TitansMitt", "Lamp", "MagicMirror" } },
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
