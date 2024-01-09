namespace Randomizer.Graph;

/**
 * Model of a world in which a player would be playing.
 */
public sealed class World
{
    public int Id { get; }
    public Graph Graph { get; }
    public Inventory StartingItems { get; }
    public WorldConfig Config { get; }
    private readonly Dictionary<string, Item> _allItems = new();

    /**
     * Add all the vertices to the graph for this region.
     *
     * @param int id id of this world
     * @param array config options for this world
     *
     * @return void
     */
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
        if (Config.Accessibility != AccessibilityOption.Locations)
        {
            items.Add(GetItem("KeyForKey"));
        }
        StartingItems = new Inventory(items.ToArray());

        foreach (var vertex in VertexCollector.LoadYmlData(this))
        {
            Graph.AddVertex(vertex);
        }

        var edges = new EdgeCollector().GetForWorld(this);
        foreach (var (condition, data) in edges)
        {
            foreach (var edge_data in data.Directed)
            {
                var from = Graph.GetVertex(edge_data[0]);
                var to = Graph.GetVertex(edge_data[1]);
                if (from is null || to is null)
                {
                    throw new Exception(
                        "Name Connection Mismatch: " +
                        $"({edge_data[0]}, {edge_data[1]}) => " +
                        $"({from}, {to})");
                }
                Graph.AddDirected(from, to, condition);
            }
            foreach (var edge_data in data.Undirected)
            {
                var from = Graph.GetVertex(edge_data[0]);
                var to = Graph.GetVertex(edge_data[1]);
                if (from is null || to is null)
                {
                    throw new Exception(
                        "Undirected Name Connection Mismatch: " +
                        $"({edge_data[0]}, {edge_data[1]}) => " +
                        $"({from}, {to})");
                }
                Graph.AddDirected(from, to, condition);
                Graph.AddDirected(to, from, condition);
            }
        }
    }

    /// <summary>
    /// Get a vertex by name.
    /// </summary>
    /// <param name="locationName">name to search for</param>
    public Vertex GetLocation(string locationName)
    {
        if (!locationName.Contains(':'))
        {
            locationName = locationName + ":" + Id;
        }
        return Graph.GetVertex(locationName);
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
