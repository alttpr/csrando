namespace RandomizerTests.Games.SuperMetroid;

using System.Text.Json;
using Randomizer.Games;
using Randomizer.Graph;

[TestClass]
[TestCategory(TestCategories.Slow)]
public sealed class GenerationRegressionTest
{
    [DataTestMethod]
    [DoNotParallelize]
    [DataRow(9881)]
    [DataRow(9903)]
    [DataRow(9908)]
    [DataRow(9917)]
    [DataRow(9931)]
    [DataRow(9955)]
    [DataRow(9970)]
    public void SettledSearchAndFillRetries_ProduceWinnableSeed(int seed)
    {
        var config = new WorldConfig
        {
            Combo = new Randomizer.Games.Combo.Config { InitialGame = "sm" },
            SuperMetroid = new Randomizer.Games.SuperMetroid.Config(),
        };
        var randomizer = new Randomizer.Games.Combo.GameRandomizer(
            [config], new PRNG(seed));

        randomizer.Randomize();

        using var playthrough = JsonDocument.Parse(
            randomizer.SpoilerLog!.Spoiler["playthrough"]["data"]);
        var root = playthrough.RootElement;
        Assert.IsTrue(root.GetProperty("complete").GetBoolean(),
            $"Incomplete playthrough for seed {seed}.");
        Assert.IsTrue(randomizer.IsWinnable(),
            $"Independent final search could not win seed {seed}.");
    }
}
