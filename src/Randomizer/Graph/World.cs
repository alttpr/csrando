namespace AlttpRandomizer.Graph;

using System.Collections.Concurrent;

/**
 * Model of a world in which a player would be playing.
 */
public sealed class World
{
    public int Id { get; }
    public Graph Graph { get; }
    private readonly HashSet<Vertex> _vertices = new();
    public Inventory CollectedItems { get; }
    private readonly ConcurrentDictionary<string, object> _config;

    /**
     * Add all the vertices to the graph for this region.
     *
     * @param int id id of this world
     * @param array config options for this world
     *
     * @return void
     */
    public World(int id = 0, Dictionary<string, object>? config = null)
    {
        Id = id;
        config ??= new();
        config.TryAdd("difficulty", "normal");
        config.TryAdd("logic", "NoGlitches");
        config.TryAdd("goal", "ganon");
        config.TryAdd("enemizer.enemyShuffle", "none");
        _config = new(config);

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
        items.AddRange((Item[]?)config.GetValueOrDefault("equipment") ?? new[] {
            Item.Get("BossHeartContainer", Id),
            Item.Get("BossHeartContainer", Id),
            Item.Get("BossHeartContainer", Id),
        });
        if (Config<string>("mode.state") == "standard")
        {
            items.Add($"EscapeLamp:{Id}");
        }
        if (Config<string>("accessibility") != "locations")
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
            foreach (var edge_data in data.directed)
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
            foreach (var edge_data in data.undirected)
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
            string entry = Config("crystals.tower", 7) == 1
                ? "Crystal:" + Id
                : "Crystal:" + Id + "|" + Config("crystals.tower", 7);

            Graph.AddDirected(meta, towerEntry, entry);
        }
        if (Graph.GetVertex($"GanonVulnerable:{Id}") is Vertex ganonVulnerable)
        {
            switch (Config<string>("goal"))
            {
                case "dungeons":
                    // this has no effect; likely a relic of GraphViz to show us it was AD.
                    //this.graph.newVertex(["AllDungeons"]);
                    break;
                case "ganon":
                case "fastganon":
                default:
                    string vulnerable = Config("crystals.ganon", 7) == 1
                        ? "Crystal:" + Id
                        : "Crystal:" + Id + "|" + Config("crystals.ganon", 7);

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

    /**
     * Get config value based on the currently set rules.
     *
     * @param string key dot notation key of config
     * @param mixed|null default value to return if key is not found
     *
     * @return mixed
     */
    public T Config<T>(string key, T @default = default!)
    {
        return (T)_config.GetOrAdd(key, @default!);
    }
}
