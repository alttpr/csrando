namespace Randomizer.Games.Zelda1;

using Randomizer.Graph;

internal static class DataLoader
{
    public static Vertex Fill(World world)
    {
        var graph = world.Graph;
        var yamlReader = new YamlReader(world.Config);

        // Load the yaml data from files into memory
        yamlReader.LoadData();

        // Shuffle entrances in the data if needed before building the graph
        if(world.Config.EntranceShuffle == EntranceShuffleOption.Overworld)
        {
            var entranceShuffler = new EntranceShuffler(world.Prng, yamlReader);
            entranceShuffler.Shuffle();
        }

        // Build the graph from the yaml data, this will parse the data and create vertices and edges according to the
        // current world data configuration
        yamlReader.BuildGraph();

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
        var newEdge = new Edge(entranceEdge.From, entranceEdge.To, new ItemCondition(entranceEdge.Condition.Item, world.Config.Triforces));
        levelEntrance.Edges.Remove(entranceEdge);
        levelEntrance.Edges.Add(newEdge);

        world.YamlData = yamlReader.Data!;

        return startingVertex;
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
