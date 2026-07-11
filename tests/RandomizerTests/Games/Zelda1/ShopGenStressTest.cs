namespace RandomizerTests.Games.Zelda1;

using System;
using Randomizer.Games;
using Randomizer.Games.Zelda1;
using Randomizer.Graph;
using Graph = Randomizer.Graph.Graph;

[TestClass]
[TestCategory(TestCategories.Slow)]
public sealed class ShopGenStressTest
{
    // Reassigning a shop screen can orphan a vanilla shop ID whose only screen got moved. BuildGraph
    // must skip caves without an entrance node under shop shuffle rather than throw. Only happens on
    // some seeds (when that lone screen rolls buy-once), so sweep a range.
    [TestMethod]
    public void ManySeeds_NoEntranceNotFound()
    {
        int fails = 0;
        for (int seed = 1; seed <= 60; seed++)
        {
            try
            {
                var c = new Config { ShopShuffle = ShopShuffleOption.Full, Triforces = "8" };
                c.SelectRandomValues(new PRNG(seed));
                _ = new World(1, new WorldConfig { Zelda1 = c }, new Graph(), new PRNG(seed));
            }
            catch (Exception e) when (e.Message.Contains("entrance not found"))
            {
                fails++;
                Console.WriteLine($"seed {seed}: {e.Message}");
            }
        }
        Assert.AreEqual(0, fails, $"{fails} seeds failed with 'entrance not found'");
    }
}
