namespace Randomizer.Graph;

/// <summary>Model of a world in which a player would be playing.</summary>
public sealed class World
{
    public int Id { get; }
    public Graph Graph { get; }
    public Inventory StartingItems { get; }
    public WorldConfig Config { get; }
    private readonly Dictionary<string, Item> _allItems = new();

    /// <summary>
    /// Creates an internal-use world that acts as host for <see cref="Graph"/> nodes that do not belong to a player world.
    /// </summary>
    internal World(Graph graph)
    {
        Id = -1;
        Graph = graph;
        StartingItems = new();
        Config = new();
    }

    /// <summary>Add all the vertices to the graph for this region.</summary>
    /// <param name="id">id of this world</param>
    /// <param name="randomizerConfig">options for this world</param>
    public World(int id, WorldConfig randomizerConfig, Graph graph)
    {
        Id = id;
        Config = randomizerConfig;
        Graph = graph;

        List<Item> items = [GetItem("fixed")];
        items.Add(GetItem($"ConfigWorldState{Config.State}"));
        items.Add(GetItem($"ConfigWorldGlitches{Config.Glitches}"));
        items.Add(GetItem($"ConfigWorldEnemyShuffle{Config.EnemyShuffle}"));
        items.Add(GetItem($"ConfigWorldTowerEntryRequired{Config.CrystalsTower}"));
        items.Add(GetItem($"ConfigWorldGanonVulnerableRequired{Config.CrystalsGanon}"));
        foreach (var tech in Config.Techs)
        {
            items.Add(GetItem($"ConfigWorldTech{tech}"));
        }

        items.AddRange(randomizerConfig.StartingEquipment.Select(x => GetItem(x)));
        StartingItems = new Inventory(items.ToArray());

        foreach (var vertex in VertexCollector.LoadYmlData(this))
        {
            Graph.AddVertex(vertex);
        }

        var edges = new EdgeCollector().GetForWorld(this);
        foreach (var (condition, data) in edges)
        {
            foreach (var edgeData in data.Directed)
            {
                var from = GetLocation(edgeData[0]);
                var to = GetLocation(edgeData[1]);
                if (from is null || to is null)
                {
                    throw new Exception(
                        "Name Connection Mismatch: " +
                        $"({edgeData[0]}, {edgeData[1]}) => " +
                        $"({from}, {to})");
                }
                Graph.AddDirected(from, to, condition);
            }
            foreach (var edgeData in data.Undirected)
            {
                var from = GetLocation(edgeData[0]);
                var to = GetLocation(edgeData[1]);
                if (from is null || to is null)
                {
                    throw new Exception(
                        "Undirected Name Connection Mismatch: " +
                        $"({edgeData[0]}, {edgeData[1]}) => " +
                        $"({from}, {to})");
                }
                Graph.AddDirected(from, to, condition);
                Graph.AddDirected(to, from, condition);
            }
        }

        PruneConfigEdges();
    }

    private void PruneConfigEdges()
    {
        foreach (var v in Graph.GetVertices().Where(v => v.World == this))
        {
            int before = v.Edges.Count;
            v.Edges = v.Edges.Where(e => !e.Condition.Item.Name.StartsWith("ConfigWorld") || StartingItems.Has(e.Condition.Item)).ToList();
            if (v.Edges.Count != before)
                System.Console.WriteLine($"Removed edges in `{v.Name}` from {before} to {v.Edges.Count}");
        }
    }

    public Inventory ComputeStartingItems()
    {
        var inventory = new Inventory([GetItem("fixed"), .. Config.StartingEquipment.Select(GetItem)]);
        var searcher = new Searcher(Graph, GetLocation("DefaultItems"), inventory);
        return inventory;
    }

    /// <summary>
    /// Get a vertex by name in this world.
    /// </summary>
    /// <param name="locationName">name to search for</param>
    public Vertex GetLocation(string locationName)
    {
        return Graph.GetVertex($"{locationName}:{Id}");
    }

    public bool HasLocation(string locationName)
    {
        return Graph.HasVertex($"{locationName}:{Id}");
    }

    /// <summary>
    /// Get all vertices of a given type.
    /// </summary>
    /// <param name="type">type to search for</param>
    public IEnumerable<Vertex> GetLocationsOfType(VertexType type)
    {
        return Graph.GetVertices().Where(vertex => vertex.Type == type);
    }

    public Item GetItem(string name)
    {
        if (_allItems.TryGetValue(name, out var matchingItem))
        {
            return matchingItem;
        }

        // allow made up items
        var item = new Item(name, this);
        _allItems.Add(item.Name, item);
        item.Id = Graph.AllItems.Count;
        Graph.AllItems.Add(item);

        return item;
    }

    public Item? GetItemOrNull(string? name)
    {
        if (name != null)
            return GetItem(name);
        return null;
    }

    public IEnumerable<Item> GetAllItems()
    {
        return _allItems.Values;
    }
}
