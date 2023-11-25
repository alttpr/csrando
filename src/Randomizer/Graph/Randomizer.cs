namespace Randomizer.Graph;

/// <summary>
/// This is the primary entry point for randomization. A new object is created
/// with a config array dictating how the worlds should be created and prepping
/// all graph infomation for those worlds.
///
/// Walk thru walls: 7E037F01
/// </summary>
public sealed class Randomizer
{
    public Graph Graph { get; private set; }
    private readonly Inventory _startingItems = new();
    private readonly Vertex _start;
    private readonly World[] _worlds;
    private readonly PRNG _prng;

    /// <summary>
    /// Set up the Randomizer. This involves:
    /// 1. Creating a new Graph
    /// 2. Creating a starting vertex
    /// 3. and keeping track of all the vertices in the graph
    /// </summary>
    ///
    /// <param name="randomizerConfigs">All the configuration for the each world generation</param>
    /// <param name="seed">Seeded again, eh?</param>
    public Randomizer(WorldConfig[] randomizerConfigs, int? seed = null)
    {
        _prng = new PRNG(seed);
        System.Console.WriteLine($"Using seed: {_prng.Seed}");

        Graph = new Graph();
        _start = Graph.AddVertex(new Vertex
        {
            Name = "start",
            Type = VertexType.Meta,
        });

        _worlds = new World[randomizerConfigs.Length];
        for (var i = 0; i < randomizerConfigs.Length; ++i)
        {
            if (randomizerConfigs[i].CrystalsGanon == WorldConfig.RandomCrystals)
                randomizerConfigs[i].CrystalsGanon = _prng.GetRandomInt(7 + 1);

            if (randomizerConfigs[i].CrystalsTower == WorldConfig.RandomCrystals)
                randomizerConfigs[i].CrystalsTower = _prng.GetRandomInt(7 + 1);

            _worlds[i] = new World(i, randomizerConfigs[i], Graph);
            _startingItems = _startingItems.Merge(_worlds[i].CollectedItems);

            ShopFiller.AdjustEdges(_worlds[i], _prng);
            EntranceShuffler.AdjustEdges(_worlds[i], _prng);
            DarknessGraphifier.AdjustEdges(_worlds[i], _prng);
            BossShuffler.AdjustEdges(_worlds[i], _prng);
            EnemyShuffler.AdjustEdges(_worlds[i], _prng);
            BunnyGraphifier.AdjustEdges(_worlds[i], _prng);
            PrizePackShuffler.AdjustEdges(_worlds[i], _prng);

            Graph.AddDirected(_start, _worlds[i].Graph.GetVertex($"start:{i}"), _worlds[i].GetItem("fixed"));
        }

        SanityCheck();
    }

    public void SanityCheck()
    {
        HashSet<Vertex> visited = new();
        Queue<Vertex> vertexQueue = new();

        vertexQueue.Enqueue(_start);

        while (vertexQueue.Any())
        {
            Vertex v = vertexQueue.Dequeue();
            visited.Add(v);

            foreach (var edge in v.Edges)
            {
                if (!visited.Contains(edge.To))
                {
                    vertexQueue.Enqueue(edge.To);
                }
            }
        }

        var unvisited = Graph.GetVertices().ToHashSet().Except(visited).ToHashSet()
            .GroupBy(v => v.Type).ToDictionary(key => key, value => new HashSet<Vertex>(value));
        var visitedByType = visited.ToLookup(v => v.Type);

        var connectedGroups = new Dictionary<Vertex, HashSet<Vertex>>();
        foreach (var vertex in Graph.GetVertices())
        {
            if (connectedGroups.ContainsKey(vertex))
                continue;

            HashSet<Vertex> currentGroup = new();
            vertexQueue.Enqueue(vertex);

            while (vertexQueue.Any())
            {
                Vertex v = vertexQueue.Dequeue();

                if (connectedGroups.ContainsKey(v))
                    continue;

                connectedGroups[v] = currentGroup;
                currentGroup.Add(v);

                foreach (var edge in v.Edges)
                {
                    if (connectedGroups.ContainsKey(edge.To))
                    {
                        var otherGroup = connectedGroups[edge.To];
                        foreach (var v2 in currentGroup)
                        {
                            connectedGroups[v2] = otherGroup;
                        }
                        otherGroup.UnionWith(currentGroup);
                        currentGroup = otherGroup;
                    }
                    else
                    {
                        vertexQueue.Enqueue(edge.To);
                    }
                }
            }
        }

        var allGroups = connectedGroups.Values.ToHashSet().ToList().OrderByDescending(l => l.Count).ToList();
        var firstGroupByType = allGroups.First().GroupBy(v => v.Type);

        var badEntrances = Graph.GetVertices().Where(v => v.Type == VertexType.Entrance && v.Edges.Count == 0).ToList();
        var allOutlets = Graph.GetVertices().Where(v => v.Type == VertexType.Outlet).ToHashSet();
        var usedOutlets = Graph.GetVertices().SelectMany(v => v.Edges.Select(v2 => v2.To).Where(v2 => v2.Type == VertexType.Outlet)).ToHashSet();
        var unusedOutlets = allOutlets.Except(usedOutlets).ToList();

        // Those are regions from the overworld directly connected to an underworld room
        // that are not holes (they are directly connected for now)
        var badEdges = Graph.GetVertices().SelectMany(v => v.Edges).Where(e => e.From.Map.HasValue && e.From.Type != VertexType.Entrance && e.From.Type != VertexType.Hole && e.To.RoomId.HasValue).ToList();

        if (unvisited.Any())
        {
           //throw new Exception("Unreachable vertices found");
        }

        FindBridges();
    }

    private void FindBridges()
    {
        int vertexCount = Graph.GetVertices().Count();
        var low = new int[vertexCount];
        var pre = new int[vertexCount];
        int cnt = 0;

        Array.Fill(low, -1);
        Array.Fill(pre, -1);

        foreach (var vertex in Graph.GetVertices())
        {
            vertex.Id = cnt++;
        }
        cnt = 0;

        foreach (var vertex in Graph.GetVertices())
        {
            if (pre[vertex.Id] == -1)
            {
                FindBridgesDfs(vertex, vertex, ref cnt, pre, low);
            }
        }
        vertexCount++;
    }

    private void FindBridgesDfs(Vertex u, Vertex v, ref int cnt, int[] pre, int[] low)
    {
        pre[v.Id] = cnt++;
        low[v.Id] = pre[v.Id];

        foreach (Vertex w in v.Edges.Select(e => e.To))
        {
            if (pre[w.Id] == -1)
            {
                FindBridgesDfs(v, w, ref cnt, pre, low);
                low[v.Id] = Math.Min(low[v.Id], low[w.Id]);
                if (low[w.Id] == pre[w.Id])
                {
                    var keydoorBridge = v.Edges.Where(e => e.To == w && e.Condition.Item.Type == ItemType.SmallKey);
                    if (keydoorBridge.Any())
                        System.Console.WriteLine($"Found bridge between {v.Name} <-> {w.Name}");
                    else
                    {
                        var keydoorBridgeRev = w.Edges.Where(e => e.To == v && e.Condition.Item.Type == ItemType.SmallKey);
                        if (keydoorBridgeRev.Any())
                            System.Console.WriteLine($"Found REV bridge between {v.Name} <-> {w.Name}");
                    }
                }
            }
            else if (w.Id != u.Id)
            {
                low[v.Id] = Math.Min(low[v.Id], pre[w.Id]);
            }
        }
    }

    /// <summary>
    /// Randomize the worlds. This handles creating a filler and placing those
    /// items into the worlds.
    /// </summary>
    public void Randomize()
    {
        var filler = new RandomAssumedFiller(this, _prng);
        var sets = new ItemPooler(_worlds, _prng).GetPool();

        filler.FillGraph(sets);
    }

    /// <summary>
    /// Get a graph searched based on the items in the inventory.
    /// </summary>
    public Searcher GetSearcherForInventory(IEnumerable<Item> items)
    {
        return new(Graph, _start, _startingItems.Merge(new Inventory(items.ToArray())));
    }

    public Item GetItemForWorld(string name, int worldId)
    {
        return _worlds[worldId].GetItem(name);
    }

    /// <summary>
    /// Check if the worlds are winnable. This is done by creating a searcher.
    /// </summary>
    public bool IsWinnable()
    {
        Searcher searcher = new(Graph, _start, _startingItems);

        foreach (var world in _worlds)
        {
            if (!searcher.HasFound(world.GetItem("Triforce")))
            {
                return false;
            }
        }

        return true;
    }
}
