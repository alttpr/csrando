namespace RandomizerTests.Games.Alttp;

using System.Text.Json;
using Randomizer.Games;
using Randomizer.Graph;

[TestClass]
public sealed class PlaythroughTest
{
    [TestMethod]
    public void Standalone_ProducesProgressionSpheres()
    {
        var randomizer = new Randomizer.Games.Alttp.GameRandomizer(
            [new WorldConfig { Alttp = new Randomizer.Games.Alttp.Config() }], new PRNG(4321));
        randomizer.Randomize();
        Assert.IsTrue(randomizer.IsWinnable());

        using var playthrough = JsonDocument.Parse(
            randomizer.SpoilerLog!.Spoiler["playthrough"]["data"]);
        var root = playthrough.RootElement;
        Assert.IsTrue(root.GetProperty("complete").GetBoolean());
        Assert.IsTrue(root.GetProperty("spheres").GetArrayLength() > 0);
        var pickups = root.GetProperty("spheres").EnumerateArray()
            .SelectMany(sphere => sphere.GetProperty("pickups").EnumerateArray())
            .ToList();
        var itemNames = pickups.Select(pickup =>
            pickup.GetProperty("item").GetProperty("name").GetString()).ToHashSet();
        Assert.IsTrue(itemNames.Contains("Shovel"));
        Assert.IsTrue(itemNames.Contains("BugCatchingNet"));
        Assert.IsTrue(pickups.Where(pickup =>
                pickup.GetProperty("item").GetProperty("name").GetString()
                    is "Shovel" or "BugCatchingNet")
            .Any(pickup => !pickup.GetProperty("required").GetBoolean()));
    }

    [TestMethod]
    public void EntranceShuffle_GanonRequiresCrystalsAndCombatItems()
    {
        var config = new Randomizer.Games.Alttp.Config
        {
            EntranceShuffle = Randomizer.Games.Alttp.EntranceShuffleOption.Simple,
            CrystalsGanon = "7",
        };
        var randomizer = new Randomizer.Games.Alttp.GameRandomizer(
            [new WorldConfig { Alttp = config }], new PRNG(2468));
        randomizer.Randomize();

        using var playthrough = JsonDocument.Parse(
            randomizer.SpoilerLog!.Spoiler["playthrough"]["data"]);
        var pickups = playthrough.RootElement.GetProperty("spheres").EnumerateArray()
            .SelectMany(sphere => sphere.GetProperty("pickups").EnumerateArray())
            .ToList();
        var triforce = pickups.Single(pickup =>
            pickup.GetProperty("item").GetProperty("name").GetString() == "Triforce");
        var requirements = triforce.GetProperty("requiredItems").EnumerateArray()
            .Select(item => (Name: item.GetProperty("name").GetString(), Count: item.GetProperty("count").GetInt32()))
            .ToList();

        Assert.IsTrue(requirements.Contains(("Crystal", 7)));
        Assert.IsFalse(triforce.GetProperty("requiredItems").EnumerateArray()
            .Single(item => item.GetProperty("name").GetString() == "Crystal")
            .GetProperty("meta").GetBoolean());
        Assert.IsTrue(requirements.Any(requirement => requirement.Name is "L2Sword" or "Hammer"));
        Assert.IsTrue(requirements.Any(requirement => requirement.Name is "Lamp" or "FireRod"));
        Assert.IsTrue(requirements.Any(requirement => requirement.Name == "BowAndSilverArrows"));
        Assert.IsTrue(playthrough.RootElement.GetProperty("startingItems").EnumerateArray()
            .Where(item => item.GetProperty("name").GetString()!.StartsWith("ConfigWorld"))
            .All(item => item.GetProperty("initial").GetBoolean()
                && !item.GetProperty("simple").GetBoolean()));
        Assert.IsTrue(triforce.GetProperty("path").EnumerateArray()
            .SelectMany(step => step.GetProperty("requirements").EnumerateArray())
            .Any(item => item.GetProperty("name").GetString()!.Contains("DefeatGanon")
                && item.GetProperty("meta").GetBoolean()
                && !item.GetProperty("initial").GetBoolean()
                && item.GetProperty("simple").GetBoolean()));
        var defeatGanon = pickups.Single(pickup =>
            pickup.GetProperty("item").GetProperty("name").GetString() == "DefeatGanon");
        var fightRoute = defeatGanon.GetProperty("path").EnumerateArray()
            .Select(step => step.GetProperty("to").GetProperty("name").GetString())
            .ToList();
        CollectionAssert.IsSubsetOf(new[]
        {
            "FightGanon",
            "GanonPhaseOneComplete",
            "GanonPhaseTwoComplete",
            "GanonPhaseThreeComplete",
            "GanonTorchesLit",
            "DefeatGanon",
        }, fightRoute);
    }

    [TestMethod]
    public void AllDungeonsGoal_GatesGanonOnDungeonCompletion()
    {
        var config = new Randomizer.Games.Alttp.Config
        {
            Goal = Randomizer.Games.Alttp.GoalOption.Dungeons,
        };
        var randomizer = new Randomizer.Games.Alttp.GameRandomizer(
            [new WorldConfig { Alttp = config }], new PRNG(1357));
        randomizer.Randomize();

        using var playthrough = JsonDocument.Parse(
            randomizer.SpoilerLog!.Spoiler["playthrough"]["data"]);
        var triforce = playthrough.RootElement.GetProperty("spheres").EnumerateArray()
            .SelectMany(sphere => sphere.GetProperty("pickups").EnumerateArray())
            .Single(pickup => pickup.GetProperty("item").GetProperty("name").GetString() == "Triforce");

        Assert.IsTrue(playthrough.RootElement.GetProperty("complete").GetBoolean());
        Assert.IsTrue(triforce.GetProperty("requiredItems").EnumerateArray().Any(item =>
            item.GetProperty("name").GetString() == "AllDungeonsComplete"));
    }
}
