namespace RandomizerTests.Games.Combo;

using System.Text.Json;
using Randomizer.Games;
using Randomizer.Graph;

[TestClass]
public sealed class PlaythroughTest
{
    [TestMethod]
    public void AlttpZelda1_ProducesProgressionSpheres()
    {
        var config = new WorldConfig
        {
            Combo = new Randomizer.Games.Combo.Config(),
            Alttp = new Randomizer.Games.Alttp.Config(),
            Zelda1 = new Randomizer.Games.Zelda1.Config(),
        };
        config.Alttp.SelectRandomValues(new PRNG(42));

        var randomizer = new Randomizer.Games.Combo.GameRandomizer([config], new PRNG(4321));
        randomizer.Randomize();
        Assert.IsTrue(randomizer.IsWinnable());

        var json = randomizer.SpoilerLog!.Spoiler["playthrough"]["data"];
        using var playthrough = JsonDocument.Parse(json);
        var root = playthrough.RootElement;
        Assert.IsTrue(root.GetProperty("complete").GetBoolean());
        Assert.IsTrue(root.GetProperty("spheres").GetArrayLength() > 0);
    }

    [TestMethod]
    public void AlttpSuperMetroid_IncludesStatefulCrossGameRoutes()
    {
        var config = new WorldConfig
        {
            Combo = new Randomizer.Games.Combo.Config(),
            Alttp = new Randomizer.Games.Alttp.Config(),
            SuperMetroid = new Randomizer.Games.SuperMetroid.Config(),
        };
        config.Alttp.SelectRandomValues(new PRNG(42));

        var randomizer = new Randomizer.Games.Combo.GameRandomizer([config], new PRNG(9876));
        randomizer.Randomize();

        using var playthrough = JsonDocument.Parse(
            randomizer.SpoilerLog!.Spoiler["playthrough"]["data"]);
        var root = playthrough.RootElement;
        var steps = root.GetProperty("spheres").EnumerateArray()
            .SelectMany(sphere => sphere.GetProperty("pickups").EnumerateArray())
            .SelectMany(pickup => pickup.GetProperty("path").EnumerateArray())
            .ToList();

        Assert.IsTrue(root.GetProperty("complete").GetBoolean());
        Assert.IsTrue(steps.Any(step => step.GetProperty("crossGame").GetBoolean()
            && step.GetProperty("from").GetProperty("game").GetString()
                != step.GetProperty("to").GetProperty("game").GetString()));
        Assert.IsTrue(steps.Any(step => step.GetProperty("game").GetString() == "sm"
            && step.TryGetProperty("strategy", out var strategy)
            && !string.IsNullOrWhiteSpace(strategy.GetString())));
    }

    [TestMethod]
    public void SuperMetroidMetroid_ProducesCompletePlaythrough()
    {
        for (int seed = 1; seed <= 5; seed++)
        {
            var config = new WorldConfig
            {
                Combo = new Randomizer.Games.Combo.Config(),
                SuperMetroid = new Randomizer.Games.SuperMetroid.Config(),
                Metroid = new Randomizer.Games.Metroid.Config(),
            };
            var randomizer = new Randomizer.Games.Combo.GameRandomizer(
                [config], new PRNG(seed));
            randomizer.Randomize();
            Assert.IsTrue(randomizer.IsWinnable(), $"seed {seed}: seed is not winnable");

            using var playthrough = JsonDocument.Parse(
                randomizer.SpoilerLog!.Spoiler["playthrough"]["data"]);
            var root = playthrough.RootElement;
            Assert.IsTrue(root.GetProperty("complete").GetBoolean(),
                $"seed {seed}: winnable seed has an incomplete playthrough; found " +
                string.Join(", ", root.GetProperty("spheres").EnumerateArray()
                    .SelectMany(sphere => sphere.GetProperty("pickups").EnumerateArray())
                    .Select(pickup => pickup.GetProperty("item").GetProperty("name").GetString())));
        }
    }

    [TestMethod]
    [DoNotParallelize]
    [TestCategory(TestCategories.Slow)]
    public void SuperMetroidMapRando_MetroidMapShuffle_ProducesCompletePlaythrough()
    {
        string sourceDataRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "../../../../../src/Randomizer/Games/SuperMetroid/data"));
        string mapCorpus = Path.Combine(sourceDataRoot, "maps");
        if (!Directory.Exists(mapCorpus)
            || !Directory.EnumerateFiles(mapCorpus, "*.avro", SearchOption.AllDirectories).Any())
        {
            Assert.Inconclusive("Requires the local SM map-rando Avro corpus.");
        }

        string oldDataRoot = Randomizer.Games.SuperMetroid.Model.JsonReader.DataRoot;
        try
        {
            Randomizer.Games.SuperMetroid.Model.JsonReader.DataRoot = sourceDataRoot;
            var config = new WorldConfig
            {
                Combo = new Randomizer.Games.Combo.Config { InitialGame = "sm" },
                SuperMetroid = new Randomizer.Games.SuperMetroid.Config
                {
                    MapRandomizer = Randomizer.Games.SuperMetroid.MapRandomizerSetting.Standard,
                },
                Metroid = new Randomizer.Games.Metroid.Config { MapShuffle = true },
            };
            // Keep this regression on a topology with an accessible opening item.
            // Seed 104729 produces an impossible front-fill start on both main and
            // this branch, before portal-state behavior can be exercised.
            var randomizer = new Randomizer.Games.Combo.GameRandomizer(
                [config], new PRNG(9876));
            randomizer.Randomize();
            Assert.IsTrue(randomizer.IsWinnable());

            using var playthrough = JsonDocument.Parse(
                randomizer.SpoilerLog!.Spoiler["playthrough"]["data"]);
            Assert.IsTrue(playthrough.RootElement.GetProperty("complete").GetBoolean(),
                "The playthrough must retain M1 portal access after SM state changes.");
            Assert.IsTrue(playthrough.RootElement.GetProperty("spheres").GetArrayLength() > 0);
        }
        finally
        {
            Randomizer.Games.SuperMetroid.Model.JsonReader.DataRoot = oldDataRoot;
        }
    }
}
