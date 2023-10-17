namespace Randomizer.Graph;

/**
 * Model of a world in which a player would be playing.
 */
public sealed class World
{
    public int Id { get; }
    public Graph Graph { get; }
    private readonly HashSet<Vertex> _vertices = new();
    public Inventory CollectedItems { get; }
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

        var start = Graph.AddVertex(new Vertex
        {
            Name = $"start:{Id}",
            Type = VertexType.Meta,
        });

        var meta = Graph.AddVertex(new Vertex
        {
            Name = $"Meta:{Id}",
            Type = VertexType.Meta,
        });
        Graph.AddDirected(start, meta, GetItem("fixed"));

        var items = new List<Item>
        {
            GetItem("MagicBar"),
            GetItem("LiftBush"),
            GetItem("LiftPot"),
            GetItem("UseBomb"),
            GetItem("OpenChest"),
            GetItem("BombUpgrade10"),
            GetItem("ArrowUpgrade10"),
            GetItem("ArrowUpgrade10"),
            GetItem("ArrowUpgrade10"),
            GetItem("fixed"),
            GetItem("hop"),
        };
        items.AddRange(randomizerConfig.StartingEquipment.Select(x => GetItem(x)));
        if (Config.State == StateOption.Standard)
        {
            items.Add(GetItem("EscapeLamp"));
        }
        if (Config.Accessibility != AccessibilityOption.Locations)
        {
            items.Add(GetItem("KeyForKey"));
        }
        CollectedItems = new Inventory(items.ToArray());

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
        // set special edges
        if (Graph.GetVertex($"TowerEntry:{Id}") is Vertex towerEntry)
        {
            if (Config.CrystalsTower == 0)
            {
                Graph.AddDirected(meta, towerEntry, GetItem("fixed"));
            }
            else
            {
                Graph.AddDirected(meta, towerEntry, GetItem("Crystal"), Config.CrystalsTower);
            }
        }
        if (Graph.GetVertex($"GanonVulnerable:{Id}") is Vertex ganonVulnerable)
        {
            switch (Config.Goal)
            {
                case GoalOption.Dungeons:
                    // this has no effect; likely a relic of GraphViz to show us it was AD.
                    //this.graph.newVertex(["AllDungeons"]);
                    break;
                case GoalOption.Ganon:
                case GoalOption.FastGanon:
                default:
                    if (Config.CrystalsGanon == 0)
                    {
                        Graph.AddDirected(meta, ganonVulnerable, GetItem("fixed"));
                    }
                    else
                    {
                        Graph.AddDirected(meta, ganonVulnerable, GetItem("Crystal"), Config.CrystalsGanon);
                    }
                    break;
            }
        }

        RemapVertices();
    }

    public void RemapVertices()
    {
        _vertices.Clear();
        _vertices.UnionWith(Graph.GetVertices());
    }

    /**
     * Get a vertex by name.
     *
     * @param string location_name name to search for
     */
    public Vertex? GetLocation(string locationName)
    {
        if (!locationName.Contains(':'))
        {
            locationName = locationName + ":" + Id;
        }
        return _vertices.FirstOrDefault(v => v.Name == locationName);
    }

    /**
     * Get a vertices by type.
     *
     * @param string type type to search for
     * 
     * @return Collection<Vertex>
     */
    public IEnumerable<Vertex> GetLocationsOfType(VertexType type)
    {
        return _vertices.Where(vertex => vertex.Type == type);
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

        return item;
    }

    public Item? GetItemOrNull(string? name)
    {
        if (name != null)
            return GetItem(name);
        return null;
    }
}
