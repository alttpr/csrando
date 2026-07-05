namespace Randomizer.Games.Metroid;

using Randomizer.Games.Metroid.MapGen;
using Randomizer.Graph;

internal static class DataLoader
{
    // Vanilla start location vertex name; map shuffle overrides this with its generated name.
    private const string VanillaStartLocation = "Brinstar - Morph Room - Spawn Platform (2) - Spawn Platform";

    public static Vertex Fill(World world)
    {
        var graph = world.Graph;
        var yamlReader = new YamlReader(world.Config);

        yamlReader.LoadData();

        string startLocationName = VanillaStartLocation;

        if (world.Config.MapShuffle)
        {
            // Sets world.PatchData to the generated map's ROM data (grid, item tables,
            // respawn positions). The vanilla portal room patches are NOT included; generated
            // portal anchors for combo mode are still to be done.
            startLocationName = GenerateMap(world, yamlReader.Data!);
        }
        else
        {
            world.PatchData = yamlReader.BuildPortalRooms(world);

            // The vanilla fixed anchor created by BuildPortalRooms: portal room (0x0C,0x0C)
            // behind an east-side door on the Left Vertical Shaft cell (0x0B,0x0C).
            world.PortalAnchors.Add(new PortalAnchor(
                "Brinstar Portal",
                "Brinstar - Brinstar Portal - Left Door Wavers (0) - Left door",
                RoomWord: 0x0B0C, Direction: 0x0004,
                DestinationId: 0x0C0C, DestinationArgs: 0x0000));
        }

        yamlReader.BuildGraph();

        world.YamlData = yamlReader.Data!;

        LoadVertices(world, yamlReader.GetVertices(world));
        LoadEdges(world, yamlReader.GetEdges(world));


        var startingVertex = new Vertex()
        {
            World = world,
            Name = "start",
            Type = VertexType.Meta,
        };

        graph.AddVertex(startingVertex);
        world.Graph.AddDirected(startingVertex, world.GetLocation(startLocationName), world.GetItem("fixed"));
        world.Graph.AddDirected(startingVertex, world.GetLocation("Meta - Metroid Meta Locations - Meta (0) - Meta"), world.GetItem("fixed"));


        return startingVertex;
    }

    /// <summary>
    /// Generates a randomized map, fits screens, and replaces the vanilla rooms in
    /// <paramref name="data"/>. Returns the generated start location vertex name.
    /// </summary>
    private static string GenerateMap(World world, YamlReader.YamlData data)
    {
        var catalog = ScreenCatalog.Build(data.screens, data.rooms);
        int mapSeed = world.Prng.GetRandomInt(int.MaxValue);

        var generator = new TopologyGenerator(catalog)
        {
            SizeScale = world.Config.MapSize switch
            {
                MapSizeOption.Small => 0.7,
                MapSizeOption.Large => 1.4,
                _ => 1.0,
            },
            Saturate = world.Config.MapSize == MapSizeOption.Nightmare,
        };

        var generated = generator.Generate(mapSeed);
        ScreenFitter.Fit(generated.Grid, catalog, mapSeed);

        // ROM emission joins vanilla special-item payloads to screens through the vanilla
        // room coordinates, so it must run before ApplyTo replaces the room list. Combo
        // seeds (the only ones with a Combo config) are entered through the portal, so
        // Brinstar deaths respawn at the portal room instead of the generated start cell.
        var emission = RomEmitter.Emit(generated, data.rooms,
            respawnAtPortal: world.WorldConfig.Combo != null);
        generated.ItemAddresses = emission.ItemAddresses;
        world.PatchData = emission.Patches;

        var rooms = RoomBuilder.Build(generated);
        RoomBuilder.ApplyTo(data, rooms);

        // Each generated portal room becomes an anchor; the door cell is always directly
        // west of it (the vanilla combo arrangement the transition code expects).
        foreach (var portal in rooms.Portals)
        {
            world.PortalAnchors.Add(new PortalAnchor(
                $"{portal.Area} Portal {portal.Cell.X:X2}{portal.Cell.Y:X2}",
                portal.DoorVertexName,
                RoomWord: (portal.Cell.X - 1) << 8 | portal.Cell.Y,
                Direction: 0x0004,
                DestinationId: portal.Cell.X << 8 | portal.Cell.Y,
                DestinationArgs: (int)portal.Area));
        }

        world.GeneratedMap = generated;
        return rooms.StartLocationName;
    }

    private static void LoadVertices(World world, List<Dictionary<string, object?>> vertices)
    {
        foreach (var vtx in vertices)
        {
            if (!vtx.TryGetValue("name", out var nameValue) || nameValue is not string name)
                throw new InvalidDataException("Metroid vertex without a name");

            var type = vtx.TryGetValue("type", out var typeValue) && typeValue is VertexType typeCast ? typeCast : VertexType.Meta;
            VertexType? subtype = vtx.TryGetValue("subtype", out var subtypeValue) && subtypeValue is VertexType subtypeCast ? subtypeCast : (type == VertexType.Item ? VertexType.Standing : null);
            var item = vtx.TryGetValue("item", out var itemValue) && itemValue is string itemName ? itemName : null;
            var itemset = vtx.TryGetValue("itemset", out var itemsetValue) && itemsetValue is string[] itemsetArray ? itemsetArray : null;
            int? address = vtx.TryGetValue("address", out var addressValue) && addressValue is int addressInt ? addressInt : null;

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
                Addresses = address.HasValue ? [(long)address.Value] : null,
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

    // Keys are (Y, X) — world-map row first — matching the vanilla special-items tables.
    private static readonly Dictionary<(int Y, int X), int> CoordToAddressMap = new()
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

    // TODO: Vanilla path should write its own complete sprite table (as map shuffle does)
    // instead of relying on these hardcoded addresses.
    private static int? GetItemLocationAddress(World world, string vertexName)
    {
        var data = world.YamlData
            ?? throw new Exception("No Metroid Data for world " + world.Id);

        var roomName = vertexName.Split(" - ")[1].Trim();
        var room = data.rooms.Where(r => r.name == roomName).First();

        // Screen index is embedded in the vertex name inside parentheses.
        var screenIndex = int.Parse(System.Text.RegularExpressions.Regex.Match(vertexName, @"\(([^)]*)\)").Groups[1].Value);

        var x = room.position[0] + (room.scroll == YamlReader.Scrolling.Horizontal ? screenIndex : 0);
        var y = room.position[1] + (room.scroll == YamlReader.Scrolling.Vertical ? screenIndex : 0);

        if (world.Config.MapShuffle)
        {
            // Map shuffle emits its own special-items tables; every item vertex must have an
            // entry there or the filled item could never appear in the ROM.
            var addresses = world.GeneratedMap?.ItemAddresses
                ?? throw new Exception($"No generated item addresses for world {world.Id}");
            if (!addresses.TryGetValue(new MapGen.Point(x, y), out var generatedAddress))
                throw new Exception($"Item vertex '{vertexName}' at ({x},{y}) has no emitted table entry");
            return generatedAddress;
        }

        return CoordToAddressMap.TryGetValue((y, x), out var addr) ? addr : null;
    }

}
