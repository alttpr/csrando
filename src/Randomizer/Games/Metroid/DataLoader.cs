namespace Randomizer.Games.Metroid;

using Randomizer.Graph;

internal static class DataLoader
{
    public static Vertex Fill(World world)
    {
        var graph = world.Graph;
        var yamlReader = new YamlReader(world.Config);

        // Load the yaml data from files into memory
        yamlReader.LoadData();

        // Build the graph from the yaml data, this will parse the data and create vertices and edges according to the
        // current world data configuration
        yamlReader.BuildGraph();

        world.YamlData = yamlReader.Data!;

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
        world.Graph.AddDirected(startingVertex, world.GetLocation("Brinstar - Morph Room - Spawn Platform (2) - Spawn Platform"), world.GetItem("fixed"));
        world.Graph.AddDirected(startingVertex, world.GetLocation("Meta - Metroid Meta Locations - Meta (0) - Meta"), world.GetItem("fixed"));


        return startingVertex;
    }

    private static void LoadVertices(World world, List<Dictionary<string, object>> vertices)
    {
        foreach (var vtx in vertices)
        {
            var name = vtx.TryGetValue("name", out object? nameValue) ? (string)nameValue : throw new InvalidDataException("Metroid vertex without a name");
            var type = vtx.TryGetValue("type", out object? typeValue) ? (VertexType)typeValue : VertexType.Meta;
            var subtype = vtx.TryGetValue("subtype", out object? subtypeValue) ? (VertexType?)subtypeValue : (type == VertexType.Item ? VertexType.Standing : null);
            var item = vtx.TryGetValue("item", out object? itemValue) ? (string)itemValue : null;
            var itemset = vtx.TryGetValue("itemset", out object? itemsetValue) ? (string[])itemsetValue : null;
            var address = vtx.TryGetValue("address", out object? addressValue) ? (int?)addressValue : null;

            if(type == VertexType.Item)
            {
                address = GetItemLocationAddress(world, name);
            }

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

    private static readonly Dictionary<(int, int), int> CoordToAddressMap = new()
    {
        
        // Brinstar
        { (0x02, 0x0F), 0x6C000A },
        { (0x03, 0x18), 0x6C0013 },
        { (0x03, 0x1B), 0x6C0019 },
        { (0x05, 0x07), 0x6C0022 },
        { (0x05, 0x19), 0x6C0028 },
        { (0x07, 0x19), 0x6C0035 },
        { (0x09, 0x13), 0x6C003E },
        { (0x0B, 0x12), 0x6C004B },
        { (0x0E, 0x02), 0x6C0059 },
        { (0x0E, 0x09), 0x6C005F },
        
        // Norfair
        { (0x0A, 0x1B), 0x6C0205 },
        { (0x0A, 0x1C), 0x6C020B },
        { (0x0B, 0x1A), 0x6C0219 },
        { (0x0B, 0x1B), 0x6C021F },
        { (0x0B, 0x1C), 0x6C0225 },
        { (0x0C, 0x1A), 0x6C022E },
        { (0x0E, 0x12), 0x6C023F },
        { (0x0F, 0x11), 0x6C0248 },
        { (0x0F, 0x13), 0x6C024F },
        { (0x0F, 0x14), 0x6C0255 },
        { (0x10, 0x0F), 0x6C0267 },
        { (0x11, 0x1B), 0x6C0286 },
        { (0x13, 0x1A), 0x6C0299 },
        { (0x14, 0x1C), 0x6C02AC },
        { (0x15, 0x12), 0x6C02B5 },
        { (0x16, 0x13), 0x6C02C3 },
        { (0x16, 0x14), 0x6C02C9 },
        
        // Kraid
        { (0x15, 0x04), 0x6C0615 },
        { (0x15, 0x09), 0x6C061B },
        { (0x16, 0x0A), 0x6C0624 },
        { (0x19, 0x0A), 0x6C062D },
        { (0x1B, 0x05), 0x6C0636 },
        { (0x1D, 0x08), 0x6C0646 },
        
        // Ridley
        { (0x18, 0x12), 0x6C0805 },
        { (0x19, 0x11), 0x6C0813 },
        { (0x1B, 0x18), 0x6C081C },
        { (0x1D, 0x0F), 0x6C0825 },
        { (0x1E, 0x14), 0x6C082E },

    };

    // TODO: Ideally the randomizer should write its own complete sprite table to the rom instead of looking up hardcoded data
    // But we'll do this for now to get things working
    private static int? GetItemLocationAddress(World world, string vertexName)
    {
        var data = world.YamlData;

        if (data is null)
        {
            throw new Exception("No Metroid Data for world " + world.Id);
        }

        var roomName = vertexName.Split(" - ")[1].Trim();
        var room = data.rooms.Where(r => r.name == roomName).First();

        // The screen is embedded in the vertex name within parentheses, extract it using regex
        var screenIndex = Int32.Parse(System.Text.RegularExpressions.Regex.Match(vertexName, @"\(([^)]*)\)").Groups[1].Value);

        //var sprite = room.sprites.Where(s => vertexName.Contains(s.name)).First();
        //var screen = data.screens.Where(s => s.screen == room.screens[sprite.screen]).First();

        var x = room.position[0] + (room.scroll == YamlReader.Scrolling.Horizontal ? screenIndex : 0);
        var y = room.position[1] + (room.scroll == YamlReader.Scrolling.Vertical ? screenIndex : 0);

        int? address = CoordToAddressMap.TryGetValue((y, x), out var addr) ? addr : null;
        return address;
    }

}
