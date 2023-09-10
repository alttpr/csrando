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
    public RandomizerConfig RandomizerConfig { get; }

    /**
     * Add all the vertices to the graph for this region.
     *
     * @param int id id of this world
     * @param array config options for this world
     *
     * @return void
     */
    public World(int id = 0, RandomizerConfig randomizerConfig = null)
    {
        this.RandomizerConfig = randomizerConfig;
        Id = id;

        Graph = new Graph();
        var start = Graph.NewVertex(new()
        {
            { "name", "start:" + Id },
            { "type", "meta" },
        });

        var meta = Graph.NewVertex(new()
        {
            { "name", "Meta:" + Id },
            { "type", "meta" },
        });
        Graph.AddDirected(start, meta, $"fixed:{Id}");

        var items = new List<object>
        {
            Item.Get("MagicBar", Id),
            Item.Get("LiftBush", Id),
            Item.Get("LiftPot", Id),
            Item.Get("UseBomb", Id),
            Item.Get("OpenChest", Id),
            Item.Get("BombUpgrade10", Id),
            Item.Get("ArrowUpgrade10", Id),
            Item.Get("ArrowUpgrade10", Id),
            Item.Get("ArrowUpgrade10", Id),
            $"fixed:{Id}",
            $"hop:{Id}",
        };
        items.AddRange(randomizerConfig.StartingEquipment.Select(x => Item.Get(x, Id)));
        if (RandomizerConfig.State == StateOption.Standard)
        {
            items.Add($"EscapeLamp:{Id}");
        }
        if (RandomizerConfig.Accessibility != AccessibilityOption.Locations)
        {
            items.Add($"KeyForKey:{Id}");
        }
        CollectedItems = new Inventory(items.ToArray());

        var vertices = new VertexCollector().LoadYmlData(this);
        vertices.ForEach(data =>
        {
            if (data.TryGetValue("item", out object? item) && item is string itemKey)
            {
                data["item"] = Item.Get(itemKey, Id);
            }
            if (data.TryGetValue("trophy", out object? trophy) && trophy is string trophyKey)
            {
                data["trophy"] = Item.Get(trophyKey, Id);
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
            string entry = RandomizerConfig.CrystalsTower == 1
                ? "Crystal:" + Id
                : "Crystal:" + Id + "|" + RandomizerConfig.CrystalsTower;

            Graph.AddDirected(meta, towerEntry, entry);
        }
        if (Graph.GetVertex($"GanonVulnerable:{Id}") is Vertex ganonVulnerable)
        {
            switch (RandomizerConfig.Goal)
            {
                case GoalOption.Dungeons:
                    // this has no effect; likely a relic of GraphViz to show us it was AD.
                    //this.graph.newVertex(["AllDungeons"]);
                    break;
                case GoalOption.Ganon:
                case GoalOption.FastGanon:
                default:
                    string vulnerable = RandomizerConfig.CrystalsGanon == 1
                        ? "Crystal:" + Id
                        : "Crystal:" + Id + "|" + RandomizerConfig.CrystalsGanon;

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
    public IEnumerable<Vertex> GetLocationsOfType(string type)
    {
        return _vertices.Where((Vertex vertex) => vertex.Type == type);
    }
}
