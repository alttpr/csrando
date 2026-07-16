namespace RandomizerTests.Games.SuperMetroid;

using Randomizer.Games;
using Randomizer.Games.SuperMetroid;
using Randomizer.Graph;
using SmVertex = Randomizer.Games.SuperMetroid.Vertex;

[TestClass]
public sealed class BacktrackRegionTest
{
    [TestMethod]
    public void VisitedState_DoesNotUseMissilesAsRequiredSupers()
    {
        var missiles = new VisitedState
        {
            Energy = 99,
            Missiles = 15,
        };
        var supers = new VisitedState
        {
            Energy = 99,
            SuperMissiles = 5,
        };

        Assert.IsFalse(missiles.Dominates(supers));
        Assert.IsTrue(supers.Dominates(missiles));
    }

    [TestMethod]
    public void RequirementResult_MergeFailCopiesOnlyWhenExtended()
    {
        var first = RequirementResult.Fail("Morph");
        var combined = RequirementResult.Fail();

        combined.MergeFail(first);
        CollectionAssert.AreEquivalent(
            new[] { "Morph" }, combined.Missing!.ToArray());

        combined.MergeFail(RequirementResult.Fail("Bombs"));
        CollectionAssert.AreEquivalent(
            new[] { "Morph", "Bombs" }, combined.Missing!.ToArray());
        CollectionAssert.AreEquivalent(
            new[] { "Morph" }, first.Missing!.ToArray(),
            "Extending a combined failure must not mutate its child result.");
    }

    [TestMethod]
    public void Build_WalksIncomingDirectedEdges()
    {
        var graph = new Graph();
        var world = new TestWorld("sm", graph);
        var fixedItem = world.GetItem("fixed");
        var a = AddVertex(graph, world, "A");
        var b = AddVertex(graph, world, "B");
        var c = AddVertex(graph, world, "C");
        var d = AddVertex(graph, world, "D");
        var e = AddVertex(graph, world, "E");

        // The acquisition direction does not imply that C can return to A.
        graph.AddDirected(a, c, fixedItem);

        // The independently modelled return route is E -> D -> B -> A.
        graph.AddDirected(b, a, fixedItem);
        graph.AddDirected(d, b, fixedItem);
        graph.AddDirected(e, d, fixedItem);

        var region = BacktrackRegion.Build(graph, a);

        CollectionAssert.AreEquivalent(
            new[] { a, b, d, e }, region.Vertices.ToArray());
        Assert.IsFalse(region.Vertices.Contains(c));
        CollectionAssert.AreEqual(
            new[] { b },
            region.IncomingEdges[a].Select(edge => edge.From).ToArray());
    }

    [TestMethod]
    public void Build_IncludesPathsToCrossWorldExits()
    {
        var graph = new Graph();
        var sm = new TestWorld("sm", graph);
        var other = new TestWorld("other", graph);
        var fixedItem = sm.GetItem("fixed");
        var target = AddVertex(graph, sm, "Target");
        var beforeExit = AddVertex(graph, sm, "Before exit");
        var exit = AddVertex(graph, sm, "Exit");
        var outside = AddVertex(graph, other, "Outside");

        graph.AddDirected(beforeExit, exit, fixedItem);
        graph.AddDirected(exit, outside, fixedItem);

        var region = BacktrackRegion.Build(graph, target);

        Assert.IsTrue(region.Vertices.Contains(target));
        Assert.IsTrue(region.Vertices.Contains(exit));
        Assert.IsTrue(region.Vertices.Contains(beforeExit));
        Assert.IsFalse(region.Vertices.Contains(outside));
    }

    [TestMethod]
    public void ReverseSearch_OnlyProvesRoutesAcceptedByStatefulSearch()
    {
        var graph = new Graph();
        var config = new WorldConfig
        {
            SuperMetroid = new Config(),
        };
        var world = new World(0, config, graph, new PRNG(1234));
        graph.SetVertexIds();

        var inventory = new Inventory(world.GetAllItems().Cast<IItem>().ToArray());
        var state = new VisitedState
        {
            Energy = 99 + inventory.GetCount(world.GetItem("ETank")) * 100,
            Missiles = inventory.GetCount(world.GetItem("Missile")) * 5,
            SuperMissiles = inventory.GetCount(world.GetItem("Super")) * 5,
            PowerBombs = inventory.GetCount(world.GetItem("PowerBomb")) * 5,
        };
        var target = (SmVertex)world.Start;
        var region = BacktrackRegion.Build(graph, target);
        var reverse = ReverseBacktrackSearch.Build(region, target, inventory);
        var proven = region.Vertices
            .Where(vertex => vertex != target
                && reverse.CanReturn(vertex, state))
            .Take(20)
            .ToArray();

        Assert.IsTrue(proven.Length > 0,
            "Expected the reverse search to find some SM return routes.");
        foreach (var vertex in proven)
        {
            var forwardSearch = new StatefulSearcher(
                graph, vertex, inventory.Clone(), target: target, visitedState: state);
            Assert.IsTrue(forwardSearch.HasVisited(target),
                $"Reverse search incorrectly proved a return route from {vertex.Name}.");
        }
    }

    [TestMethod]
    public void ReverseSearch_AccumulatesResourceCost()
    {
        var graph = new Graph();
        var world = new World(0, new WorldConfig
        {
            SuperMetroid = new Config(),
        }, graph, new PRNG(1234));
        var target = AddVertex(graph, world, "Resource target");
        var source = AddVertex(graph, world, "Resource source");
        source.Edges.Add(new Randomizer.Games.SuperMetroid.Edge(
            source, target,
            [new Randomizer.Games.SuperMetroid.Model.Strat
            {
                Name = "Spend power bombs",
                Requires = new Randomizer.Games.SuperMetroid.Model.Requirement.Ammo(
                    "PowerBomb", 2),
            }]));
        graph.SetVertexIds();

        var inventory = new Inventory(world.GetItem("PowerBomb"));
        var reverse = ReverseBacktrackSearch.Build(
            BacktrackRegion.Build(graph, target), target, inventory);

        Assert.IsFalse(reverse.CanReturn(source,
            new VisitedState { Energy = 99, PowerBombs = 1 }));
        Assert.IsTrue(reverse.CanReturn(source,
            new VisitedState { Energy = 99, PowerBombs = 2 }));
    }

    [TestMethod]
    public void ReverseSearch_InvertsObstacleRequirementsAndMutations()
    {
        var graph = new Graph();
        var world = new World(0, new WorldConfig
        {
            SuperMetroid = new Config(),
        }, graph, new PRNG(1234));
        var target = AddVertex(graph, world, "Obstacle target");
        var requires = AddVertex(graph, world, "Requires obstacle");
        var clears = AddVertex(graph, world, "Clears obstacle");
        requires.Edges.Add(new Randomizer.Games.SuperMetroid.Edge(
            requires, target,
            [new Randomizer.Games.SuperMetroid.Model.Strat
            {
                Name = "Use obstacle",
                Requires = new Randomizer.Games.SuperMetroid.Model.Requirement
                    .ObstaclesCleared(["A"]),
            }]));
        clears.Edges.Add(new Randomizer.Games.SuperMetroid.Edge(
            clears, requires,
            [new Randomizer.Games.SuperMetroid.Model.Strat
            {
                Name = "Clear obstacle",
                Requires = new Randomizer.Games.SuperMetroid.Model.Requirement.Always(),
                ClearsObstacles = ["A"],
            }]));
        graph.SetVertexIds();

        var inventory = new Inventory(world.GetAllItems().Cast<IItem>().ToArray());
        var reverse = ReverseBacktrackSearch.Build(
            BacktrackRegion.Build(graph, target), target, inventory);
        var state = new VisitedState { Energy = 99 };

        Assert.IsFalse(reverse.CanReturn(requires, state));
        Assert.IsTrue(reverse.CanReturn(requires,
            state with { ObstacleBitFlags = 1 }));
        Assert.IsTrue(reverse.CanReturn(clears, state));
    }

    [TestMethod]
    public void ReverseSearch_SelectsStrategiesForConcreteInventory()
    {
        var graph = new Graph();
        var world = new World(0, new WorldConfig
        {
            SuperMetroid = new Config(),
        }, graph, new PRNG(1234));
        var target = AddVertex(graph, world, "Alternative target");
        var source = AddVertex(graph, world, "Alternative source");
        source.Edges.Add(new Randomizer.Games.SuperMetroid.Edge(
            source, target,
            [
                new Randomizer.Games.SuperMetroid.Model.Strat
                {
                    Name = "Use Morph",
                    Requires = new Randomizer.Games.SuperMetroid.Model.Requirement.Single(
                        "Morph"),
                },
                new Randomizer.Games.SuperMetroid.Model.Strat
                {
                    Name = "Use Bombs",
                    Requires = new Randomizer.Games.SuperMetroid.Model.Requirement.Single(
                        "Bombs"),
                },
            ]));
        graph.SetVertexIds();

        var inventory = new Inventory(world.GetItem("Morph"));
        var reverse = ReverseBacktrackSearch.Build(
            BacktrackRegion.Build(graph, target), target, inventory);
        var state = new VisitedState { Energy = 99 };

        Assert.IsTrue(reverse.CanReturn(source, state));
        Assert.IsTrue(new StatefulSearcher(
            graph, source, inventory, target: target, visitedState: state)
            .HasVisited(target));
    }

    [TestMethod]
    public void ReverseSearch_UsesPromotedFlagInventory()
    {
        var graph = new Graph();
        var world = new World(0, new WorldConfig
        {
            SuperMetroid = new Config(),
        }, graph, new PRNG(1234));
        var target = AddVertex(graph, world, "Flag target");
        var fight = AddVertex(graph, world, "Flag producer");
        var source = AddVertex(graph, world, "Flag source");
        var flag = world.GetItem("f_DefeatedGoldenTorizo");

        source.Edges.Add(new Randomizer.Games.SuperMetroid.Edge(
            source, fight,
            [new Randomizer.Games.SuperMetroid.Model.Strat
            {
                Name = "Enter fight",
                Requires = new Randomizer.Games.SuperMetroid.Model.Requirement.Always(),
            }]));
        fight.Edges.Add(new Randomizer.Games.SuperMetroid.Edge(
            fight, fight,
            [new Randomizer.Games.SuperMetroid.Model.Strat
            {
                Name = "Win fight",
                Requires = new Randomizer.Games.SuperMetroid.Model.Requirement.Ammo(
                    "Super", 2),
                SetsFlags = [flag.Name],
            }]));
        fight.Edges.Add(new Randomizer.Games.SuperMetroid.Edge(
            fight, target,
            [new Randomizer.Games.SuperMetroid.Model.Strat
            {
                Name = "Use flag",
                Requires = new Randomizer.Games.SuperMetroid.Model.Requirement.And(
                    [
                        new Randomizer.Games.SuperMetroid.Model.Requirement.Single(
                            flag.Name),
                        new Randomizer.Games.SuperMetroid.Model.Requirement.Ammo(
                            "Super", 2),
                    ]),
            }]));
        graph.SetVertexIds();

        var inventory = new Inventory(world.GetItem("Super"), flag);
        var reverse = ReverseBacktrackSearch.Build(
            BacktrackRegion.Build(graph, target), target, inventory);

        Assert.IsFalse(reverse.CanReturn(source,
            new VisitedState { Energy = 99, SuperMissiles = 1 }));
        Assert.IsTrue(reverse.CanReturn(source,
            new VisitedState { Energy = 99, SuperMissiles = 2 }));
    }

    [TestMethod]
    public void ReverseSearch_UsesPromotedEquipmentInventory()
    {
        var graph = new Graph();
        var world = new World(0, new WorldConfig
        {
            SuperMetroid = new Config(),
        }, graph, new PRNG(1234));
        var target = AddVertex(graph, world, "Equipment target");
        var pickup = AddVertex(
            graph, world, "Ice pickup", type: VertexType.Item);
        var source = AddVertex(graph, world, "Equipment source");
        var ice = world.GetItem("Ice");
        pickup.Item = ice;

        source.Edges.Add(new Randomizer.Games.SuperMetroid.Edge(
            source, pickup,
            [new Randomizer.Games.SuperMetroid.Model.Strat
            {
                Name = "Reach Ice",
                Requires = new Randomizer.Games.SuperMetroid.Model.Requirement.Always(),
            }]));
        pickup.Edges.Add(new Randomizer.Games.SuperMetroid.Edge(
            pickup, target,
            [new Randomizer.Games.SuperMetroid.Model.Strat
            {
                Name = "Use Ice",
                Requires = new Randomizer.Games.SuperMetroid.Model.Requirement.Single(
                    ice.Name),
            }]));
        graph.SetVertexIds();

        var inventory = new Inventory(ice);
        var region = BacktrackRegion.Build(graph, target);
        var reverse = ReverseBacktrackSearch.Build(region, target, inventory);
        var state = new VisitedState { Energy = 99 };

        Assert.IsTrue(reverse.CanReturn(source, state));
        Assert.IsTrue(new StatefulSearcher(
            graph, source, inventory, target: target, visitedState: state)
            .HasVisited(target));
    }

    [TestMethod]
    public void ReverseSearch_RejectsRoomEntryWithRequiredLocalObstacle()
    {
        var graph = new Graph();
        var world = new World(0, new WorldConfig
        {
            SuperMetroid = new Config(),
        }, graph, new PRNG(1234));
        var source = AddVertex(graph, world, "Previous room", 1);
        var entry = AddVertex(graph, world, "New room entry", 2);
        var target = AddVertex(graph, world, "New room target", 2);

        source.Edges.Add(new Randomizer.Games.SuperMetroid.Edge(
            source, entry,
            [new Randomizer.Games.SuperMetroid.Model.Strat
            {
                Name = "Enter room",
                Requires = new Randomizer.Games.SuperMetroid.Model.Requirement.Always(),
            }]));
        entry.Edges.Add(new Randomizer.Games.SuperMetroid.Edge(
            entry, target,
            [new Randomizer.Games.SuperMetroid.Model.Strat
            {
                Name = "Needs local obstacle",
                Requires = new Randomizer.Games.SuperMetroid.Model.Requirement.ObstaclesCleared(
                    ["A"]),
            }]));
        graph.SetVertexIds();

        var inventory = new Inventory();
        var state = new VisitedState
        {
            Energy = 99,
            ObstacleBitFlags = RequirementHandler.ObstacleMaskFromArray(["A"]),
        };
        var reverse = ReverseBacktrackSearch.Build(
            BacktrackRegion.Build(graph, target), target, inventory);

        Assert.IsFalse(reverse.CanReturn(source, state));
        Assert.IsFalse(new StatefulSearcher(
            graph, source, inventory, target: target, visitedState: state)
            .HasVisited(target));
    }

    [TestMethod]
    public void Cache_SharesStructuralRegionForGraph()
    {
        var graph = new Graph();
        var world = new World(0, new WorldConfig
        {
            SuperMetroid = new Config(),
        }, graph, new PRNG(1234));
        var otherWorld = new TestWorld("other", graph);
        var unrelatedItem = otherWorld.GetItem("Unrelated");
        graph.SetVertexIds();
        var target = (SmVertex)world.Start;
        var metrics = BacktrackMetrics.ForGraph(graph);
        var cache = BacktrackCache.ForGraph(graph);
        var region = cache.GetRegion(graph, target, metrics);

        Assert.AreSame(region, cache.GetRegion(graph, target, metrics));
        var inventory = world.StartingItems.Clone();
        var reverse = cache.GetReverseSearch(
            target, region, inventory, metrics);
        Assert.AreSame(reverse, cache.GetReverseSearch(
            target, region, inventory.Clone(), metrics));
        var inventoryWithUnrelatedItem = inventory.Clone();
        inventoryWithUnrelatedItem.AddItem(unrelatedItem);
        Assert.AreSame(reverse, cache.GetReverseSearch(
            target, region, inventoryWithUnrelatedItem, metrics));
        Assert.AreEqual(1, BacktrackMetrics.SnapshotFor(graph).RegionBuilds);
        Assert.AreEqual(1,
            BacktrackMetrics.SnapshotFor(graph).ReverseSearchBuilds);
    }

    [TestMethod]
    public void MetricsCsv_AppendsOneHeaderAndEscapesLabels()
    {
        string path = Path.Combine(
            Path.GetTempPath(), $"sm-backtracking-{Guid.NewGuid():N}.csv");
        var file = new FileInfo(path);
        var metrics = new BacktrackMetricsSnapshot(
            Checks: 10,
            StructuralRejects: 2,
            ReverseProofs: 3,
            ReverseRejects: 0,
            FallbackSearches: 5,
            FallbackSuccesses: 4,
            FallbackFailures: 1,
            FallbackElapsedTicks: System.Diagnostics.Stopwatch.Frequency,
            FallbackStates: 250,
            RegionBuilds: 1,
            RegionBuildElapsedTicks: 0,
            RegionVertices: 100,
            ReverseSearchBuilds: 1,
            ReverseSearchBuildElapsedTicks: 0,
            ReverseFrontierVertices: 20,
            ReverseFrontierEntries: 25,
            StructurallyPrunedEnqueues: 7,
            ForwardPasses: 8,
            ForwardDequeuedStates: 500,
            ForwardRequirementEvaluations: 1_000,
            ForwardEnqueueAttempts: 750,
            ForwardEnqueues: 600);

        try
        {
            BacktrackMetricsCsv.Append(
                file, "baseline,legacy", BacktrackBenchmarkMode.Legacy,
                100, "Test", TimeSpan.FromSeconds(2), metrics);
            BacktrackMetricsCsv.Append(
                file, "reverse", BacktrackBenchmarkMode.Reverse,
                101, "Test", TimeSpan.FromSeconds(1), metrics);

            string[] lines = File.ReadAllLines(path);
            Assert.AreEqual(3, lines.Length);
            Assert.IsTrue(lines[0].StartsWith("schema_version,timestamp_utc,label,mode,"));
            StringAssert.Contains(lines[1], "\"baseline,legacy\",Legacy,100");
            StringAssert.Contains(lines[1], ",50.000,");
            StringAssert.Contains(lines[2], ",reverse,Reverse,101,");
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static SmVertex AddVertex(
        Graph graph, IWorld world, string name, int roomId = 0,
        VertexType type = VertexType.Meta)
    {
        var vertex = new SmVertex
        {
            Name = name,
            Type = type,
            World = world,
            RoomId = roomId,
            Node = new Randomizer.Games.SuperMetroid.Model.Node
            {
                Name = name,
                NodeType = "junction",
                NodeSubType = "junction",
            },
        };
        graph.AddVertex(vertex);
        return vertex;
    }

    private sealed class TestItem(string name, IWorld world)
        : Randomizer.Graph.Item(name, world);

    private sealed class TestWorld(string gameId, Graph graph) : IWorld
    {
        private readonly Dictionary<string, IItem> _items = [];

        public IItem GetItem(string name) => _items.TryGetValue(name, out var item)
            ? item
            : _items[name] = graph.RegisterItem(new TestItem(name, this));
        public IItem? GetExistingItem(string name) => _items.GetValueOrDefault(name);
        public Randomizer.Graph.Vertex GetLocation(string locationName) =>
            throw new NotSupportedException();
        public IEnumerable<Randomizer.Graph.Vertex> GetLocations() =>
            graph.GetVertices().Where(vertex => vertex.World == this);
        public bool HasLocation(string locationName) => false;
        public IEnumerable<Randomizer.Graph.Vertex> GetEmptyLocationsInSet(
            ISearcher searcher, IItem itemToPlace, ItemSetName itemSet,
            Dictionary<ItemSetName, int> setCounts) => [];
        public void TrackPlacedItem(Randomizer.Graph.Vertex location) { }
        public bool IsWinnable(Randomizer.Graph.Vertex start, Inventory startingInventory) => false;
        public ISearcher GetSearcherForWorld(
            Graph searchGraph, Randomizer.Graph.Vertex? start, Inventory inventory,
            SetLocations? setLocations = null) => throw new NotSupportedException();

        public WorldConfig WorldConfig { get; } = new();
        public Graph Graph => graph;
        public int Id => 0;
        public string GameId => gameId;
        public ushort PlacedItemCount { get; set; }
        public Inventory StartingItems { get; } = new();
        public Randomizer.Graph.Vertex Start => throw new NotSupportedException();
    }
}
