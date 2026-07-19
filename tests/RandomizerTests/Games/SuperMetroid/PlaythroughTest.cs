namespace RandomizerTests.Games.SuperMetroid;

using System.Text.Json;
using Randomizer.Games;
using Randomizer.Games.SuperMetroid;
using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;
using World = Randomizer.Games.SuperMetroid.World;

[TestClass]
public sealed class PlaythroughTest
{
    private static readonly string[] MajorBossFlags =
    [
        "f_DefeatedKraid",
        "f_DefeatedPhantoon",
        "f_DefeatedDraygon",
        "f_DefeatedRidley",
    ];

    [TestMethod]
    public void Standalone_ProducesStatefulStrategyRoutes()
    {
        var config = new WorldConfig
        {
            SuperMetroid = new Randomizer.Games.SuperMetroid.Config(),
        };
        var randomizer = new Randomizer.Games.SuperMetroid.GameRandomizer(
            [config], new PRNG(4321));
        randomizer.Randomize();

        using var playthrough = JsonDocument.Parse(
            randomizer.SpoilerLog!.Spoiler["playthrough"]["data"]);
        var root = playthrough.RootElement;
        var pickups = root.GetProperty("spheres").EnumerateArray()
            .SelectMany(sphere => sphere.GetProperty("pickups").EnumerateArray())
            .ToList();
        var steps = pickups.SelectMany(pickup => pickup.GetProperty("path").EnumerateArray())
            .ToList();

        Assert.IsTrue(root.GetProperty("complete").GetBoolean(),
            $"Spheres: {root.GetProperty("spheres").GetArrayLength()}; pickups: " +
            string.Join(", ", pickups.Select(pickup =>
                pickup.GetProperty("item").GetProperty("name").GetString())));
        Assert.IsTrue(root.GetProperty("spheres").GetArrayLength() > 1);
        Assert.IsTrue(pickups.Count > 1);
        Assert.IsTrue(pickups.Any(pickup =>
            pickup.GetProperty("item").GetProperty("name").GetString() ==
            "f_DefeatedMotherBrain"));
        Assert.IsFalse(root.GetProperty("startingItems").EnumerateArray().Any(item =>
            item.GetProperty("name").GetString() == "f_TourianOpen"));
        var motherBrain = pickups.Single(pickup =>
            pickup.GetProperty("item").GetProperty("name").GetString() ==
            "f_DefeatedMotherBrain");
        var motherBrainRequirements = motherBrain.GetProperty("requiredItems").EnumerateArray()
            .Select(item => item.GetProperty("name").GetString())
            .ToList();
        CollectionAssert.IsSubsetOf(MajorBossFlags, motherBrainRequirements);
        Assert.IsTrue(steps.Any(step => step.GetProperty("game").GetString() == "sm"
                && step.TryGetProperty("strategy", out var strategy)
                && !string.IsNullOrWhiteSpace(strategy.GetString())));
        Assert.IsTrue(steps.Any(step => step.GetProperty("resourcesSpent").EnumerateArray()
            .Any(resource => resource.GetProperty("amount").GetInt32() > 0)));
        string[] ammoItems = ["Missile", "Super", "PowerBomb"];
        Assert.IsFalse(steps.SelectMany(step =>
                step.GetProperty("requirements").EnumerateArray())
            .Any(item => ammoItems.Contains(
                    item.GetProperty("name").GetString())
                && item.GetProperty("count").GetInt32() > 100),
            "Scripted ammo drains must not become expansion requirements.");
        Assert.IsTrue(pickups.Any(pickup => pickup.GetProperty("meta").GetBoolean()));
        Assert.IsTrue(pickups.Any(pickup => !pickup.GetProperty("meta").GetBoolean()));
        var selfDependentPickups = pickups.Where(pickup =>
        {
            string? item = pickup.GetProperty("item").GetProperty("name").GetString();
            return item!.StartsWith("f_", StringComparison.Ordinal)
                && pickup.GetProperty("path").EnumerateArray()
                .SelectMany(step => step.GetProperty("requirements").EnumerateArray())
                .Any(requirement =>
                    requirement.GetProperty("name").GetString() == item);
        })
            .Select(pickup => pickup.GetProperty("item").GetProperty("name").GetString())
            .ToList();
        Assert.AreEqual(0, selfDependentPickups.Count,
            "A pickup route cannot require the pickup being acquired: " +
            string.Join(", ", selfDependentPickups));
        Assert.AreEqual(0, root.GetProperty("warnings").GetArrayLength());
    }

    [TestMethod]
    public void RunwayEntranceConditions_AreOnlyReachableThroughMatchedArrivalStates()
    {
        var graph = new Randomizer.Graph.Graph();
        var world = new World(0, new WorldConfig
        {
            SuperMetroid = new Config(),
        }, graph, new PRNG(1234));
        graph.SetVertexIds();

        var aqueductDoor = (Randomizer.Games.SuperMetroid.Vertex)world.GetLocation(
            "Maridia - Aqueduct - Bottom Left Door");
        var aqueductItem = (Randomizer.Games.SuperMetroid.Vertex)world.GetLocation(
            "Maridia - Aqueduct - Top Right Left Item");

        Assert.IsFalse(aqueductDoor.Edges
            .OfType<Randomizer.Games.SuperMetroid.Edge>()
            .Where(edge => ReferenceEquals(edge.To, aqueductItem))
            .SelectMany(edge => edge.Strats ?? [])
            .Any(strat => strat.Name == "Suitless Shinespark"),
            "A normal or internally reached door state must not expose comeInRunning.");

        var suitlessArrival = graph.GetVertices()
            .OfType<Randomizer.Games.SuperMetroid.Vertex>()
            .Single(vertex => vertex.LogicalName == aqueductDoor.Name
                && vertex.Edges.OfType<Randomizer.Games.SuperMetroid.Edge>()
                    .Where(edge => ReferenceEquals(edge.To, aqueductItem))
                    .SelectMany(edge => edge.Strats ?? [])
                    .Any(strat => strat.Name == "Suitless Shinespark"));

        var incomingSuitlessEdges = graph.GetVertices().SelectMany(vertex => vertex.Edges)
            .OfType<Randomizer.Games.SuperMetroid.Edge>()
            .Where(edge => ReferenceEquals(edge.To, suitlessArrival))
            .ToList();
        Assert.IsTrue(incomingSuitlessEdges.Count > 0,
            "The Aqueduct conditioned arrival must have an incoming runway match.");
        Assert.IsTrue(incomingSuitlessEdges.All(edge => edge.Strats!.All(strat =>
                strat.ExitCondition is ExitCondition.LeaveWithRunway)),
            "A conditioned arrival state must only be entered from a matching runway exit.");
        Assert.IsTrue(incomingSuitlessEdges.SelectMany(edge => edge.Strats!)
            .Any(strat => strat.Requires is not Requirement.Never),
            "At least one Aqueduct runway must actually satisfy comeInRunning.");

        Type[] supportedConditions =
        [
            typeof(EntranceCondition.ComeInRunning),
            typeof(EntranceCondition.ComeInJumping),
            typeof(EntranceCondition.ComeInSpinning),
        ];
        var conditionedEdges = graph.GetVertices()
            .OfType<Randomizer.Games.SuperMetroid.Vertex>()
            .Where(vertex => vertex.LogicalName != null)
            .SelectMany(vertex => vertex.Edges
                .OfType<Randomizer.Games.SuperMetroid.Edge>()
                .SelectMany(edge => edge.Strats ?? []))
            .Where(strat => strat.EntranceCondition != null)
            .ToList();

        foreach (Type conditionType in supportedConditions)
        {
            Assert.IsTrue(conditionedEdges.Any(strat =>
                    strat.EntranceCondition!.GetType() == conditionType),
                $"Expected an arrival-state edge for {conditionType.Name}.");
        }

        Assert.IsFalse(graph.GetVertices()
            .OfType<Randomizer.Games.SuperMetroid.Vertex>()
            .Where(vertex => vertex.LogicalName == null)
            .SelectMany(vertex => vertex.Edges
                .OfType<Randomizer.Games.SuperMetroid.Edge>()
                .SelectMany(edge => edge.Strats ?? []))
            .Any(strat => strat.EntranceCondition != null
                && supportedConditions.Contains(strat.EntranceCondition.GetType())),
            "Supported entrance-conditioned strats leaked onto an ordinary node.");
    }

    [TestMethod]
    [DoNotParallelize]
    [DataRow(MapRandomizerSetting.None)]
    [DataRow(MapRandomizerSetting.Standard)]
    public void BossCount_GatesTourianOrMotherBrain(MapRandomizerSetting mapRandomizer)
    {
        var graph = new Randomizer.Graph.Graph();
        var worldConfig = new WorldConfig
        {
            SuperMetroid = new Config
            {
                Bosses = "2",
                MapRandomizer = mapRandomizer,
            },
        };
        string oldDataRoot = Randomizer.Games.SuperMetroid.Model.JsonReader.DataRoot;
        if (mapRandomizer == MapRandomizerSetting.Standard)
        {
            string sourceDataRoot = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "../../../../../src/Randomizer/Games/SuperMetroid/data"));
            string mapCorpus = Path.Combine(sourceDataRoot, "maps");
            if (!Directory.Exists(mapCorpus)
                || !Directory.EnumerateFiles(
                    mapCorpus, "*.avro", SearchOption.AllDirectories).Any())
            {
                Assert.Inconclusive("Requires the local SM map-rando Avro corpus.");
            }
            Randomizer.Games.SuperMetroid.Model.JsonReader.DataRoot = sourceDataRoot;
        }

        World world;
        try
        {
            world = new World(0, worldConfig, graph, new PRNG(1234));
            graph.SetVertexIds();
        }
        finally
        {
            Randomizer.Games.SuperMetroid.Model.JsonReader.DataRoot = oldDataRoot;
        }

        Assert.AreEqual(mapRandomizer == MapRandomizerSetting.Standard,
            world.StartingItems.Has(world.GetItem("f_TourianOpen")));

        IReadOnlyList<Requirement> gates;
        if (mapRandomizer == MapRandomizerSetting.None)
        {
            gates = world.JsonData.Rooms.First(room => room.Name == "Statues Room").Strats
                .Where(strat => strat.Name == "Statues Cutscene")
                .Select(strat => strat.Requires)
                .ToList();
            Assert.AreEqual(2, gates.Count);
        }
        else
        {
            var motherBrainDoor = (Randomizer.Games.SuperMetroid.Vertex)world.GetLocation(
                "Tourian - Mother Brain Room - Right Door");
            gates = graph.GetVertices().SelectMany(vertex => vertex.Edges)
                .OfType<Randomizer.Games.SuperMetroid.Edge>()
                .Where(edge => ReferenceEquals(edge.To, motherBrainDoor)
                    && ((Randomizer.Games.SuperMetroid.Vertex)edge.From).RoomId
                        != motherBrainDoor.RoomId)
                .SelectMany(edge => edge.Strats ?? [])
                .Select(strat => strat.Requires)
                .Take(1)
                .ToList();

            var motherBrainLeftDoor = (Randomizer.Games.SuperMetroid.Vertex)world.GetLocation(
                "Tourian - Mother Brain Room - Left Blast Door");
            var blockedEdges = graph.GetVertices().SelectMany(vertex => vertex.Edges)
                .OfType<Randomizer.Games.SuperMetroid.Edge>()
                .Where(edge => (((Randomizer.Games.SuperMetroid.Vertex)edge.From).RoomId
                            == motherBrainLeftDoor.RoomId
                        && ((Randomizer.Games.SuperMetroid.Vertex)edge.From).NodeId
                            == motherBrainLeftDoor.NodeId
                        || ((Randomizer.Games.SuperMetroid.Vertex)edge.To).RoomId
                            == motherBrainLeftDoor.RoomId
                        && ((Randomizer.Games.SuperMetroid.Vertex)edge.To).NodeId
                            == motherBrainLeftDoor.NodeId)
                    && ((Randomizer.Games.SuperMetroid.Vertex)edge.From).RoomId
                        != ((Randomizer.Games.SuperMetroid.Vertex)edge.To).RoomId)
                .ToList();
            Assert.IsTrue(blockedEdges.Count > 0);
            Assert.IsTrue(blockedEdges.SelectMany(edge => edge.Strats ?? [])
                .All(strat => strat.Requires is Requirement.Never));
        }
        Assert.AreEqual(mapRandomizer == MapRandomizerSetting.None ? 2 : 1, gates.Count);
        var inventory = world.StartingItems.Clone();
        inventory.AddItem(world.GetItem(MajorBossFlags[0]));
        var state = new VisitedState { Energy = 99 };

        foreach (var gate in gates)
            Assert.IsFalse(world.RequirementHandler.HandleRequirement(
                gate, state, inventory, world, []).Met);

        inventory.AddItem(world.GetItem(MajorBossFlags[1]));
        foreach (var gate in gates)
        {
            var result = world.RequirementHandler.HandleRequirement(
                gate, state, inventory, world, []);
            Assert.IsTrue(result.Met);
            Assert.AreEqual(2, result.UsedItems!.Keys.Count(MajorBossFlags.Contains));
        }
    }

    [TestMethod]
    public void ResourceCapacity_ConvertsResourceUnitsToExpansionCounts()
    {
        var graph = new Randomizer.Graph.Graph();
        var world = new World(0, new WorldConfig
        {
            SuperMetroid = new Config(),
        }, graph, new PRNG(1234));
        graph.SetVertexIds();
        var inventory = new Inventory();
        inventory.AddItem(world.GetItem("Missile"), 3);
        inventory.AddItem(world.GetItem("ETank"));
        inventory.AddItem(world.GetItem("ReserveTank"));
        var requirement = new Requirement.ResourceCapacity(
        [
            new ResourceTypeCount("Missile", 15),
            new ResourceTypeCount("RegularEnergy", 199),
            new ResourceTypeCount("ReserveEnergy", 100),
        ]);

        var result = world.RequirementHandler.HandleRequirement(
            requirement, new VisitedState { Energy = 99 }, inventory, world, []);

        Assert.IsTrue(result.Met);
        Assert.AreEqual(3, result.UsedItems!["Missile"]);
        Assert.AreEqual(1, result.UsedItems["ETank"]);
        Assert.AreEqual(1, result.UsedItems["ReserveTank"]);
    }

    [TestMethod]
    public void RequirementCostOr_UsesEachOperandsPowerBombCount()
    {
        var powerBombCost = new RequirementCost { PowerBombs = 2 };
        var missileCost = new RequirementCost { Missiles = 1 };

        Assert.AreEqual(missileCost, powerBombCost | missileCost);
    }
}
