using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Inverted.NoGlitches.DeathMountain;

[TestClass]
public class EastTest
{
    public static IEnumerable<object[]> TestData => new[]
    {
        new object[] { "Spiral Cave", false, new string[] {  } },
        new object[] { "Spiral Cave", false, new string[] { "ProgressiveGlove", "Lamp", "MoonPearl" } },
        new object[] { "Spiral Cave", false, new string[] { "ProgressiveGlove", "Hookshot", "MoonPearl" } },
        new object[] { "Spiral Cave", false, new string[] { "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Spiral Cave", false, new string[] { "OcarinaInactive", "Hookshot", "MoonPearl" } },
        new object[] { "Spiral Cave", true, new string[] { "OcarinaInactive", "Hookshot", "MoonPearl", "ProgressiveGlove", "Hammer" } },
        new object[] { "Spiral Cave", true, new string[] { "ProgressiveGlove", "Lamp", "MoonPearl", "Hookshot" } },
        new object[] { "Spiral Cave", true, new string[] { "PowerGlove", "Lamp", "MoonPearl", "Hookshot" } },
        new object[] { "Spiral Cave", true, new string[] { "TitansMitt", "Lamp", "MoonPearl", "Hookshot" } },
        new object[] { "Spiral Cave", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Lamp", "MoonPearl" } },
        new object[] { "Spiral Cave", true, new string[] { "TitansMitt", "Lamp", "MoonPearl" } },
        new object[] { "Spiral Cave", true, new string[] { "OcarinaInactive", "ProgressiveGlove", "ProgressiveGlove", "MoonPearl" } },
        new object[] { "Spiral Cave", true, new string[] { "OcarinaInactive", "TitansMitt", "MoonPearl" } },

        new object[] { "Paradox Cave Lower - Far Left", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Far Left", false, new string[] { "ProgressiveGlove", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Far Left", false, new string[] { "OcarinaInactive", "ProgressiveGlove", "Hammer", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "OcarinaInactive", "ProgressiveGlove", "Hammer", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "PowerGlove", "Lamp", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Lamp", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "TitansMitt", "Lamp", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "OcarinaInactive", "ProgressiveGlove", "ProgressiveGlove", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Far Left", true, new string[] { "OcarinaInactive", "TitansMitt", "MoonPearl" } },

        new object[] { "Paradox Cave Lower - Left", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Left", false, new string[] { "ProgressiveGlove", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Left", false, new string[] { "OcarinaInactive", "ProgressiveGlove", "Hammer", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "OcarinaInactive", "ProgressiveGlove", "Hammer", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "PowerGlove", "Lamp", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Lamp", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "TitansMitt", "Lamp", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "OcarinaInactive", "ProgressiveGlove", "ProgressiveGlove", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Left", true, new string[] { "OcarinaInactive", "TitansMitt", "MoonPearl" } },

        new object[] { "Paradox Cave Lower - Middle", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Middle", false, new string[] { "ProgressiveGlove", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Middle", false, new string[] { "OcarinaInactive", "ProgressiveGlove", "Hammer", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "OcarinaInactive", "ProgressiveGlove", "Hammer", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "PowerGlove", "Lamp", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Lamp", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "TitansMitt", "Lamp", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "OcarinaInactive", "ProgressiveGlove", "ProgressiveGlove", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Middle", true, new string[] { "OcarinaInactive", "TitansMitt", "MoonPearl" } },

        new object[] { "Paradox Cave Lower - Right", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Right", false, new string[] { "ProgressiveGlove", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Right", false, new string[] { "OcarinaInactive", "ProgressiveGlove", "Hammer", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "OcarinaInactive", "ProgressiveGlove", "Hammer", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "PowerGlove", "Lamp", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Lamp", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "TitansMitt", "Lamp", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "OcarinaInactive", "ProgressiveGlove", "ProgressiveGlove", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Right", true, new string[] { "OcarinaInactive", "TitansMitt", "MoonPearl" } },

        new object[] { "Paradox Cave Lower - Far Right", false, new string[] {  } },
        new object[] { "Paradox Cave Lower - Far Right", false, new string[] { "ProgressiveGlove", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Far Right", false, new string[] { "OcarinaInactive", "ProgressiveGlove", "Hammer", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "OcarinaInactive", "ProgressiveGlove", "Hammer", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "PowerGlove", "Lamp", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Lamp", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "TitansMitt", "Lamp", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "OcarinaInactive", "ProgressiveGlove", "ProgressiveGlove", "MoonPearl" } },
        new object[] { "Paradox Cave Lower - Far Right", true, new string[] { "OcarinaInactive", "TitansMitt", "MoonPearl" } },

        new object[] { "Paradox Cave Upper - Left", false, new string[] {  } },
        new object[] { "Paradox Cave Upper - Left", false, new string[] { "ProgressiveGlove", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Upper - Left", false, new string[] { "OcarinaInactive", "ProgressiveGlove", "Hammer", "MoonPearl" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "OcarinaInactive", "ProgressiveGlove", "Hammer", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "PowerGlove", "Lamp", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Lamp", "MoonPearl" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "TitansMitt", "Lamp", "MoonPearl" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "OcarinaInactive", "ProgressiveGlove", "ProgressiveGlove", "MoonPearl" } },
        new object[] { "Paradox Cave Upper - Left", true, new string[] { "OcarinaInactive", "TitansMitt", "MoonPearl" } },

        new object[] { "Paradox Cave Upper - Right", false, new string[] {  } },
        new object[] { "Paradox Cave Upper - Right", false, new string[] { "ProgressiveGlove", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Upper - Right", false, new string[] { "OcarinaInactive", "ProgressiveGlove", "Hammer", "MoonPearl" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "OcarinaInactive", "ProgressiveGlove", "Hammer", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "ProgressiveGlove", "Lamp", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "PowerGlove", "Lamp", "Hookshot", "MoonPearl" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Lamp", "MoonPearl" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "TitansMitt", "Lamp", "MoonPearl" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "OcarinaInactive", "ProgressiveGlove", "ProgressiveGlove", "MoonPearl" } },
        new object[] { "Paradox Cave Upper - Right", true, new string[] { "OcarinaInactive", "TitansMitt", "MoonPearl" } },

        new object[] { "Mimic Cave", false, new string[] {  } },
        new object[] { "Mimic Cave", true, new string[] { "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "Hammer", "Hookshot" } },
        new object[] { "Mimic Cave", true, new string[] { "OcarinaInactive", "MoonPearl", "PowerGlove", "Hammer", "Hookshot" } },
        new object[] { "Mimic Cave", true, new string[] { "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hammer" } },
        new object[] { "Mimic Cave", true, new string[] { "OcarinaInactive", "MoonPearl", "TitansMitt", "Hammer" } },
        new object[] { "Mimic Cave", true, new string[] { "ProgressiveGlove", "Lamp", "MoonPearl", "Hammer", "Hookshot" } },
        new object[] { "Mimic Cave", true, new string[] { "PowerGlove", "Lamp", "MoonPearl", "Hammer", "Hookshot" } },
        new object[] { "Mimic Cave", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Lamp", "MoonPearl", "Hammer" } },
        new object[] { "Mimic Cave", true, new string[] { "TitansMitt", "Lamp", "MoonPearl", "Hammer" } },

        new object[] { "Ether Tablet", false, new string[] {  } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "Hammer", "Hookshot", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "Hammer", "Hookshot", "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "Hammer", "Hookshot", "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "Hammer", "Hookshot", "BookOfMudora", "L4Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaInactive", "MoonPearl", "PowerGlove", "Hammer", "Hookshot", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaInactive", "MoonPearl", "PowerGlove", "Hammer", "Hookshot", "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaInactive", "MoonPearl", "PowerGlove", "Hammer", "Hookshot", "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaInactive", "MoonPearl", "PowerGlove", "Hammer", "Hookshot", "BookOfMudora", "L4Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hammer", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hammer", "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hammer", "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hammer", "BookOfMudora", "L4Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaInactive", "MoonPearl", "TitansMitt", "Hammer", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaInactive", "MoonPearl", "TitansMitt", "Hammer", "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaInactive", "MoonPearl", "TitansMitt", "Hammer", "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "OcarinaInactive", "MoonPearl", "TitansMitt", "Hammer", "BookOfMudora", "L4Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "MoonPearl", "Hammer", "Hookshot", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "MoonPearl", "Hammer", "Hookshot", "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "MoonPearl", "Hammer", "Hookshot", "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "ProgressiveGlove", "Lamp", "MoonPearl", "Hammer", "Hookshot", "BookOfMudora", "L4Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "MoonPearl", "Hammer", "Hookshot", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "MoonPearl", "Hammer", "Hookshot", "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "MoonPearl", "Hammer", "Hookshot", "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "PowerGlove", "Lamp", "MoonPearl", "Hammer", "Hookshot", "BookOfMudora", "L4Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Lamp", "MoonPearl", "Hammer", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Lamp", "MoonPearl", "Hammer", "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Lamp", "MoonPearl", "Hammer", "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Lamp", "MoonPearl", "Hammer", "BookOfMudora", "L4Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "MoonPearl", "Hammer", "BookOfMudora", "ProgressiveSword", "ProgressiveSword" } },
        new object[] { "Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "MoonPearl", "Hammer", "BookOfMudora", "L2Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "MoonPearl", "Hammer", "BookOfMudora", "L3Sword" } },
        new object[] { "Ether Tablet", true, new string[] { "TitansMitt", "Lamp", "MoonPearl", "Hammer", "BookOfMudora", "L4Sword" } },

        new object[] { "Spectacle Rock", false, new string[] {  } },
        new object[] { "Spectacle Rock", true, new string[] { "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "Hammer", "Hookshot" } },
        new object[] { "Spectacle Rock", true, new string[] { "OcarinaInactive", "MoonPearl", "PowerGlove", "Hammer", "Hookshot" } },
        new object[] { "Spectacle Rock", true, new string[] { "OcarinaInactive", "MoonPearl", "ProgressiveGlove", "ProgressiveGlove", "Hammer" } },
        new object[] { "Spectacle Rock", true, new string[] { "OcarinaInactive", "MoonPearl", "TitansMitt", "Hammer" } },
        new object[] { "Spectacle Rock", true, new string[] { "ProgressiveGlove", "Lamp", "MoonPearl", "Hammer", "Hookshot" } },
        new object[] { "Spectacle Rock", true, new string[] { "PowerGlove", "Lamp", "MoonPearl", "Hammer", "Hookshot" } },
        new object[] { "Spectacle Rock", true, new string[] { "ProgressiveGlove", "ProgressiveGlove", "Lamp", "MoonPearl", "Hammer" } },
        new object[] { "Spectacle Rock", true, new string[] { "TitansMitt", "Lamp", "MoonPearl", "Hammer" } },
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
