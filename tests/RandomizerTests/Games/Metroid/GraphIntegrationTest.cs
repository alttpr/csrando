namespace RandomizerTests.Games.Metroid;

using System.Text.RegularExpressions;
using Randomizer.Games.Metroid;
using Randomizer.Games.Metroid.MapGen;
using static Randomizer.Games.Metroid.YamlReader;

/// <summary>
/// Phase 2 acceptance: the generated, fitted world must drive the real YamlReader graph
/// builder, and graph reachability must agree with the physical solver. A generated graph
/// that builds but is severed was the core failure mode of earlier attempts, so the
/// comparison here is a hard requirement, not debugging aid.
/// </summary>
[TestClass]
public sealed class GraphIntegrationTest
{
    private static readonly Lazy<(ScreenCatalog Catalog, YamlData Data)> Loaded = new(() =>
    {
        var reader = new YamlReader(new Config());
        reader.LoadData();
        return (ScreenCatalog.Build(reader.Data!.screens, reader.Data!.rooms), reader.Data!);
    });

    private static (GeneratedWorld World, GeneratedRooms Rooms, YamlReader GraphReader) BuildWorld(int seed)
    {
        var world = new TopologyGenerator(Loaded.Value.Catalog).Generate(seed);
        ScreenFitter.Fit(world.Grid, Loaded.Value.Catalog, seed);
        var rooms = RoomBuilder.Build(world);

        var graphReader = new YamlReader(new Config());
        graphReader.LoadData();
        RoomBuilder.ApplyTo(graphReader.Data!, rooms);
        graphReader.BuildGraph();

        return (world, rooms, graphReader);
    }

    [TestMethod]
    public void GeneratedWorld_BuildsGraphAndStartResolves()
    {
        for (int seed = 1; seed <= 10; seed++)
        {
            var (_, rooms, reader) = BuildWorld(seed);
            var names = reader.GetVertices(null!).Select(v => (string)v["name"]!).ToHashSet();
            Assert.IsTrue(names.Contains(rooms.StartLocationName),
                $"seed {seed}: start vertex '{rooms.StartLocationName}' not in graph");
        }
    }

    [TestMethod]
    public void GeneratedWorld_GraphMatchesPhysicalReachability()
    {
        for (int seed = 1; seed <= 10; seed++)
        {
            var (world, rooms, reader) = BuildWorld(seed);

            // Requirement-blind BFS over the built graph = full-inventory reachability.
            var adjacency = new Dictionary<string, List<string>>();
            void Add(string from, string to)
            {
                if (!adjacency.TryGetValue(from, out var list))
                    adjacency[from] = list = [];
                list.Add(to);
            }

            foreach (var pair in reader.GetEdges(null!).Values)
            {
                foreach (var edge in pair.Directed)
                    Add(edge[0], edge[1]);
                foreach (var edge in pair.Undirected)
                {
                    Add(edge[0], edge[1]);
                    Add(edge[1], edge[0]);
                }
            }

            var reachedNames = new HashSet<string> { rooms.StartLocationName };
            var queue = new Queue<string>([rooms.StartLocationName]);
            while (queue.Count > 0)
            {
                foreach (var next in adjacency.GetValueOrDefault(queue.Dequeue()) ?? [])
                    if (reachedNames.Add(next))
                        queue.Enqueue(next);
            }

            // Map vertex names back to grid coordinates: "{area} - {room} - {screen} ({i}) - {node}".
            var roomsByName = rooms.Rooms.ToDictionary(r => r.name);
            Point? CoordOf(string vertexName)
            {
                var parts = vertexName.Split(" - ");
                if (parts.Length < 4 || !roomsByName.TryGetValue(parts[1], out var room))
                    return null;
                int index = int.Parse(Regex.Match(parts[2], @"\((\d+)\)").Groups[1].Value);
                return room.scroll == Scrolling.Horizontal
                    ? new Point(room.position[0] + index, room.position[1])
                    : new Point(room.position[0], room.position[1] + index);
            }

            var allVertices = reader.GetVertices(null!).Select(v => (string)v["name"]!).ToList();
            var coordsWithVertices = new HashSet<Point>();
            foreach (var name in allVertices)
                if (CoordOf(name) is { } p)
                    coordsWithVertices.Add(p);

            var graphReached = new HashSet<Point>();
            foreach (var name in reachedNames)
                if (CoordOf(name) is { } p)
                    graphReached.Add(p);

            // Physical solver reach (excluding caps, which produce no vertices anyway).
            var physical = AxisSolver.Solve(world.Grid, world.Start);

            // Every coordinate that owns graph vertices and is physically reachable must be
            // graph-reachable: a gap means the graph builder severed a chain the engine allows.
            var gaps = coordsWithVertices
                .Where(p => physical.Contains(p) && !graphReached.Contains(p))
                .OrderBy(p => p.Y).ThenBy(p => p.X)
                .ToList();

            Assert.AreEqual(0, gaps.Count,
                $"seed {seed}: {gaps.Count} physically reachable cells missing from graph: " +
                string.Join(", ", gaps.Take(8).Select(p => $"{p} {world.Grid.Cell(p)}")));

            // And the graph must not invent reachability the engine forbids.
            var phantom = graphReached.Where(p => !physical.Contains(p)
                && world.Grid.Cell(p)?.Role != CellRole.Cap).ToList();
            Assert.AreEqual(0, phantom.Count,
                $"seed {seed}: graph reaches cells the physical model does not: " +
                string.Join(", ", phantom.Take(8)));
        }
    }

    [TestMethod]
    public void GeneratedWorld_ItemAndBossVerticesReachable()
    {
        for (int seed = 1; seed <= 10; seed++)
        {
            var (world, rooms, reader) = BuildWorld(seed);

            int itemSprites = rooms.Rooms.Sum(r => r.sprites!.Count(s => s.type == SpriteType.Item));
            int itemCells = world.Grid.Cells.Count(c => c.Role == CellRole.Item)
                + world.Grid.Cells.Count(c => c.Role == CellRole.Boss
                    && (c.AssignedScreen?.ItemLocationNames.Count ?? 0) > 0);
            Assert.AreEqual(itemCells, itemSprites, $"seed {seed}: item sprite count mismatch");

            var bossItems = reader.GetVertices(null!)
                .Where(v => v.TryGetValue("item", out var item) && item is string s
                    && (s == "KraidDefeated" || s == "RidleyDefeated"))
                .Select(v => (string)v["name"]!)
                .ToList();
            Assert.AreEqual(2, bossItems.Count, $"seed {seed}: expected Kraid and Ridley defeat vertices");
        }
    }
}
