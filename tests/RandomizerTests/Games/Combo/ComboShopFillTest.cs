namespace RandomizerTests.Games.Combo;

using System;
using Randomizer.Games;
using Randomizer.Graph;

/// <summary>
/// Combo front-fill must not crash when StatefulSearcher.BacktrackLocation is handed a location its
/// searcher never visited (the cross-world fill can validate with a fresh searcher). Z1 shop shuffle
/// perturbs the fill order enough to surface it on some seeds, so sweep a range.
/// </summary>
[TestClass]
[TestCategory(TestCategories.Slow)]
public sealed class ComboShopFillTest
{
    [TestMethod]
    public void ComboWithShopShuffle_ManySeeds_NoBacktrackKeyError()
    {
        int failures = 0;
        for (int seed = 1; seed <= 30; seed++)
        {
            var config = new WorldConfig
            {
                Combo = new Randomizer.Games.Combo.Config(),
                Alttp = new Randomizer.Games.Alttp.Config(),
                SuperMetroid = new Randomizer.Games.SuperMetroid.Config(),
                Zelda1 = new Randomizer.Games.Zelda1.Config { ShopShuffle = Randomizer.Games.Zelda1.ShopShuffleOption.Full },
                Metroid = new Randomizer.Games.Metroid.Config(),
            };
            config.Alttp.SelectRandomValues(new PRNG(seed));
            config.Zelda1.SelectRandomValues(new PRNG(seed));

            try
            {
                var randomizer = new Randomizer.Games.Combo.GameRandomizer([config], new PRNG(seed));
                randomizer.Randomize();
            }
            catch (KeyNotFoundException e)
            {
                failures++;
                Console.WriteLine($"seed {seed}: {e.Message}");
            }
        }

        Assert.AreEqual(0, failures, $"{failures} combo seeds crashed with a backtrack KeyNotFound.");
    }
}
