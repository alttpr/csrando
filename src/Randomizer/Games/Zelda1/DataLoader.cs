namespace Randomizer.Games.Zelda1;

using Randomizer.Graph;

internal static class DataLoader
{
    /// <summary>
    /// Overworld maps the default portal layout uses for cross-game portal caves. Any
    /// overworld map with a cave works (the cave-entry hook matches the map id against
    /// the transition table); these carry duplicated shops or a bombable cave, so
    /// repurposing them costs no item locations. They are always kept out of entrance
    /// shuffle so a portal placed on them never hides a shuffled cave; when combo shop
    /// shuffle is active, they are also excluded from shop location generation.
    /// </summary>
    internal static readonly (int Map, string CaveNode)[] PortalCaveCandidates =
    [
        (0x66, "Open cave"),
        (0x0C, "Open cave"),
        (0x70, "Open cave"),
    ];

    public static Vertex Fill(World world)
    {
        var graph = world.Graph;
        var yamlReader = new YamlReader(world.Config);
        var reservedPortalCaveMaps = ReservedPortalCaveMaps(world);

        // Load the yaml data from files into memory
        yamlReader.LoadData();

        // Shuffle entrances in the data if needed before building the graph
        if (world.Config.EntranceShuffle == EntranceShuffleOption.Overworld)
        {
            var entranceShuffler = new EntranceShuffler(world.Prng, yamlReader,
                PortalCaveCandidates.Select(c => c.Map).ToList());
            entranceShuffler.Shuffle();
        }

        // Assign unique per-screen cave IDs for shops (and synthesize single-purchase shops) so they
        // don't all share the same backing data. Runs after entrance shuffle so it sees final
        // screen->cave assignments, and before BuildGraph so the new caves become graph locations.
        if (world.Config.ShopShuffle != ShopShuffleOption.Off)
        {
            new ShopShuffler(world.Prng, yamlReader.Data!, reservedPortalCaveMaps).Shuffle();
        }

        // Generate randomized dungeons if enabled
        if (world.Config.DungeonShuffle)
        {
            for (int level = 1; level <= 9; level++)
            {
                var levelRnd = new Random(world.Prng.GetRandomInt(int.MaxValue));
                var cfg = DungeonConfig.GetConfigForLevel(level, world.Config.DungeonStyle, world.Config.EnemyPlacement, levelRnd, world.Config.HiddenItems, world.Config.MapPlacement);

                // Generate() throws InvalidOperationException for layouts it can't finish (e.g. a
                // connector below the start, L9 isolation orphaning rooms, or item positions that
                // don't fit the engine's slots). Retry with a fresh builder so a rejected layout
                // leaves no partial state; only Write() once we have a valid one.
                const int maxDungeonAttempts = 50;
                DungeonBuilder? builder = null;
                for (int attempt = 0; attempt < maxDungeonAttempts; attempt++)
                {
                    var candidate = new DungeonBuilder(cfg, yamlReader.Data!, level, world.Prng);
                    try
                    {
                        candidate.Generate();
                        builder = candidate;
                        break;
                    }
                    catch (InvalidOperationException) when (attempt < maxDungeonAttempts - 1)
                    {
                        // Layout was unusable; try again with the next PRNG draw.
                    }
                }

                if (builder == null)
                    throw new Exception($"Failed to generate a valid layout for level {level}");

                builder.Write();
                world.DungeonSpoilers.Add(builder.GetSpoilerData());
            }
        }

        // Build the graph from the yaml data, this will parse the data and create vertices and edges according to the
        // current world data configuration
        yamlReader.BuildGraph(reservedPortalCaveMaps);

        // Fill the world with vertices and edges after building the world from yaml data
        LoadVertices(world, yamlReader.GetVertices(world));
        LoadEdges(world, yamlReader.GetEdges(world));


        // Create a starting vertex for the player to start at
        var startingVertex = new Vertex()
        {
            World = world,
            Name = "start",
            Type = VertexType.Meta,
        };

        graph.AddVertex(startingVertex);

        // Connect the starting vertex to the starting location and meta location
        var startMap = yamlReader.GetStartMap();
        var formattedStartMap = startMap.ToString("X2");
        world.Graph.AddDirected(startingVertex, world.GetLocation($"Overworld - Map {formattedStartMap} - Left exit"), world.GetItem("fixed"));
        world.Graph.AddDirected(startingVertex, world.GetLocation($"Overworld - Meta - Meta"), world.GetItem("fixed"));


        // Patch the Level 9 entrance edge to account for different triforce requirements
        var levelEntrance = world.GetLocation("Level 9 - Entrance");
        var entranceEdge = levelEntrance.Edges.Find(e => e.Condition.Item.Name == "Triforce")!;
        var newEdge = new Edge(entranceEdge.From, entranceEdge.To, new ItemCondition(entranceEdge.Condition.Item, Convert.ToInt32(world.Config.Triforces)));
        levelEntrance.Edges.Remove(entranceEdge);
        levelEntrance.Edges.Add(newEdge);

        world.YamlData = yamlReader.Data!;

        return startingVertex;
    }

    private static HashSet<int> ReservedPortalCaveMaps(World world)
    {
        if (world.WorldConfig.Game != RandomizerTarget.Combo || world.WorldConfig.Combo == null)
            return [];

        bool hasPortalPartner = world.WorldConfig.Alttp != null
            || world.WorldConfig.SuperMetroid != null
            || world.WorldConfig.Metroid != null;

        return hasPortalPartner
            ? PortalCaveCandidates.Select(c => c.Map).ToHashSet()
            : [];
    }

    private static void LoadVertices(World world, List<Dictionary<string, object>> vertices)
    {
        foreach (var vtx in vertices)
        {
            var name = vtx.TryGetValue("name", out object? nameValue) ? (string)nameValue : throw new InvalidDataException("Zelda vertex without a name");
            var type = vtx.TryGetValue("type", out object? typeValue) ? (VertexType)typeValue : VertexType.Meta;
            var subtype = vtx.TryGetValue("subtype", out object? subtypeValue) ? (VertexType?)subtypeValue : (type == VertexType.Item ? VertexType.Standing : null);
            var item = vtx.TryGetValue("item", out object? itemValue) ? (string)itemValue : null;
            var itemset = vtx.TryGetValue("itemset", out object? itemsetValue) ? (string[])itemsetValue : null;
            var address = vtx.TryGetValue("address", out object? addressValue) ? (int?)addressValue : null;

            var vertex = new Vertex()
            {
                World = world,
                Name = name,
                Type = type,
                Item = item != null ? world.GetItem(item) : null,
                ItemSet = itemset?.Select(i => new ItemSetName(i, world)).ToArray() ?? [],
                Addresses = address != null ? [(long)address.Value] : null,
            };

            world.Graph.AddVertex(vertex);
        }
    }

    private static void LoadEdges(World world, Dictionary<string, DirectedUndirectedPair> edgeCollections)
    {
        foreach (var edgeCollection in edgeCollections)
        {
            var edgeCollectionData = edgeCollection.Key.Split(":").First().Split('|');
            var requirementName = edgeCollectionData.First();
            var requirement = world.GetItem(requirementName);
            var requirementCount = int.Parse(edgeCollectionData.Skip(1).FirstOrDefault() ?? "1");

            foreach (var edges in edgeCollection.Value.Directed)
            {
                var from = world.GetLocation(edges[0]);
                var to = world.GetLocation(edges[1]);
                if (from is null || to is null)
                {
                    throw new Exception("Name Connection Mismatch: " + $"({edges[0]}, {edges[1]}) => " + $"({from}, {to})");
                }

                world.Graph.AddDirected(from, to, requirement, requirementCount);
            }

            foreach (var edges in edgeCollection.Value.Undirected)
            {
                var from = world.GetLocation(edges[0]);
                var to = world.GetLocation(edges[1]);
                if (from is null || to is null)
                {
                    throw new Exception("Name Connection Mismatch: " + $"({edges[0]}, {edges[1]}) => " + $"({from}, {to})");
                }

                world.Graph.AddDirected(from, to, requirement, requirementCount);
                world.Graph.AddDirected(to, from, requirement, requirementCount);
            }
        }
    }

}
