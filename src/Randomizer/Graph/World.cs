namespace AlttpRandomizer.Graph;

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
    public World(int id, WorldConfig randomizerConfig)
    {
        Id = id;
        Config = randomizerConfig;

        Graph = new Graph();
        var start = Graph.NewVertex(new()
        {
            { "name", "start:" + Id },
            { "type", VertexType.Meta },
        });

        var meta = Graph.NewVertex(new()
        {
            { "name", "Meta:" + Id },
            { "type", VertexType.Meta },
        });
        Graph.AddDirected(start, meta, $"fixed:{Id}");

        var items = new List<object>
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
            $"fixed:{Id}",
            $"hop:{Id}",
        };
        items.AddRange(randomizerConfig.StartingEquipment.Select(x => GetItem(x)));
        if (Config.State == StateOption.Standard)
        {
            items.Add($"EscapeLamp:{Id}");
        }
        if (Config.Accessibility != AccessibilityOption.Locations)
        {
            items.Add($"KeyForKey:{Id}");
        }
        CollectedItems = new Inventory(items.ToArray());

        var vertices = new VertexCollector().LoadYmlData(this);
        vertices.ForEach(data =>
        {
            if (data.TryGetValue("item", out object? item) && item is string itemKey)
            {
                data["item"] = GetItem(itemKey);
            }
            if (data.TryGetValue("trophy", out object? trophy) && trophy is string trophyKey)
            {
                data["trophy"] = GetItem(trophyKey);
            }
            Graph.NewVertex(data);
        });

        var edges = new EdgeCollector().GetForWorld(this);
        foreach (var (group, data) in edges)
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
                Graph.AddDirected(from, to, group);
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
                Graph.AddDirected(from, to, group);
                Graph.AddDirected(to, from, group);
            }
        }
        // set special edges
        if (Graph.GetVertex($"TowerEntry:{Id}") is Vertex towerEntry)
        {
            string entry = Config.CrystalsTower == 1
                ? "Crystal:" + Id
                : "Crystal:" + Id + "|" + Config.CrystalsTower;

            Graph.AddDirected(meta, towerEntry, entry);
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
                    string vulnerable = Config.CrystalsGanon == 1
                        ? "Crystal:" + Id
                        : "Crystal:" + Id + "|" + Config.CrystalsGanon;

                    Graph.AddDirected(meta, ganonVulnerable, vulnerable);
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
        return _vertices.Where((Vertex vertex) => vertex.Type == type);
    }

    public Item GetItem(string name)
    {
        string world_name = name + ":" + Id;

        if (_allItems.TryGetValue(world_name, out var matchingItem))
        {
            return matchingItem;
        }

        // allow made up items
        var item = new Item(name, Id);
        _allItems.Add(item.Name, item);

        return item;
    }
}
