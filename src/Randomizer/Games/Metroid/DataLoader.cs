namespace Randomizer.Games.Metroid;

using Randomizer.Games.Metroid.MapGen;
using Randomizer.Graph;

internal static class DataLoader
{
    /// <summary>
    /// Areas that get a portal room. Portal rooms are physical map content and must
    /// exist before the graph does — the one thing about portals that cannot resolve
    /// lazily on demand — so all of them are always built regardless of which partner
    /// games are present: the default portal layout reassigns endpoints when a partner
    /// is missing, and rooms never connected stay inert dead ends.
    /// </summary>
    internal static readonly YamlReader.Area[] PortalRoomAreas =
    [
        YamlReader.Area.Brinstar,
        YamlReader.Area.Norfair,
        YamlReader.Area.Kraid,
    ];

    // Vanilla-map start vertex per area. Non-Brinstar entries are the vanilla
    // elevator arrival cells
    private static readonly Dictionary<YamlReader.Area, string> VanillaStartLocations = new()
    {
        [YamlReader.Area.Brinstar] = "Brinstar - Morph Room - Spawn Platform (2) - Spawn Platform",
        [YamlReader.Area.Norfair] = "Norfair - Elevator Shaft - Elevator Entrance (1) - Elevator Platform",
        [YamlReader.Area.Kraid] = "Kraid - Elevator Shaft - Elevator Entrance (1) - Elevator Platform",
        [YamlReader.Area.Tourian] = "Tourian - Elevator Shaft - Elevator Entrance (1) - Elevator Platform",
        [YamlReader.Area.Ridley] = "Ridley - Elevator Shaft - Elevator Entrance (1) - Elevator Platform",
    };

    /// <summary>The vanilla-map start vertex for an area (see <see cref="VanillaStartLocations"/>).</summary>
    internal static string StartLocationFor(YamlReader.Area area) => VanillaStartLocations[area];

    public static Vertex Fill(World world)
    {
        var graph = world.Graph;
        var yamlReader = new YamlReader(world.Config);

        yamlReader.LoadData();

        string? generatedStartLocation = null;

        if (world.Config.MapShuffle)
        {
            // Resolved before generation: the generator builds the start (and its
            // pedestal) into the chosen area. Draws nothing for the default Brinstar,
            // so existing map-shuffle seeds are unchanged.
            world.ResolveStartingArea();

            // Sets world.PatchData to the generated map's ROM data (grid, item tables,
            // respawn positions, boot config). The vanilla portal room patches are NOT
            // included; generated portal anchors for combo mode are still to be done.
            generatedStartLocation = GenerateMap(world, yamlReader.Data!);
        }
        else
        {
            // The vanilla fixed anchors created by BuildPortalRooms: portal rooms behind
            // east-side doors in the areas requested by the combo portal layout.
            world.PatchData = yamlReader.BuildPortalRooms(world);

            // Vanilla layouts place no map-station pickups, so pre-reveal every area's
            // automap instead: the base ROM ships the vanilla map payload, and this byte
            // is copied into the persistent reveal state when the automap state resets.
            world.PatchData[MapGen.AutomapComposer.InitialRevealAddress] =
                [MapGen.AutomapComposer.RevealAllAreas];
        }

        yamlReader.BuildGraph();

        world.YamlData = yamlReader.Data!;

        LoadVertices(world, yamlReader.GetVertices(world));
        LoadEdges(world, yamlReader.GetEdges(world));

        string startLocationName;
        if (generatedStartLocation != null)
        {
            startLocationName = generatedStartLocation;
        }
        else
        {
            // Resolved here rather than in the World constructor: whether an area is a
            // viable start is a property of the graph, which only exists now.
            world.ResolveStartingArea();
            startLocationName = VanillaStartLocations[world.StartingArea];

            // The area the boot code cold-starts into; YamlReader.Area matches the
            // engine's InArea order. The vanilla orientation table stays correct
            // because every vanilla start cell keeps its room orientation.
            world.PatchData![RomWriter.StartAreaConfigAddress] = [(byte)world.StartingArea];
        }


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
        var catalog = ScreenCatalog.Build(data.screens, data.rooms, data.transitions);
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
            PortalAreas = PortalRoomAreas,
            StartArea = world.StartingArea,
        };

        var generated = generator.Generate(mapSeed);
        // Topology reachability is validated pre-fit, before screens (and their directional
        // one-way transitions) are known. Try alternate deterministic fits when a concrete
        // assignment strands a region; the topology itself is still valid and does not need
        // to be regenerated.
        const int MaxFitAttempts = 16;
        List<string> stranded = [];
        for (int attempt = 0; attempt < MaxFitAttempts; attempt++)
        {
            ScreenFitter.Fit(generated.Grid, catalog,
                unchecked(mapSeed + attempt * 7919));
            stranded = AxisSolver.Validate(generated.Grid, generated.Start, catalog);
            if (stranded.Count == 0)
                break;
        }

        if (stranded.Count > 0)
            throw new GenerationException("post-fit reachability: " + stranded[0]
                + $" (+{stranded.Count - 1} more)");

        // ROM emission joins vanilla special-item payloads to screens through the vanilla
        // room coordinates, so it must run before ApplyTo replaces the room list. Combo
        // seeds are entered through the portal — Brinstar deaths respawn at the portal
        // room instead of the start cell — unless a moved start makes M1 the seed's
        // starting game, in which case the boot cell must survive as the respawn.
        bool bootsIntoMetroid = world.Config.ApplyStartArea && world.Config.StartAreaRequested;
        var emission = RomEmitter.Emit(generated, data.rooms,
            respawnAtPortal: world.WorldConfig.Combo != null && !bootsIntoMetroid);
        generated.ItemAddresses = emission.ItemAddresses;
        world.PatchData = emission.Patches;

        var rooms = RoomBuilder.Build(generated);
        RoomBuilder.ApplyTo(data, rooms);

        // Record each generated portal room; the host door cell is always directly west
        // of it (the vanilla combo arrangement the transition code expects). Leaving
        // fires on entering the host cell's right-hand door; arrivals scroll into the
        // host room through that door — the portal room is only a safety net.
        foreach (var portal in rooms.Portals)
        {
            var doorCell = new Point(portal.Cell.X - 1, portal.Cell.Y);
            var hostRoom = rooms.CellToRoom[doorCell].Room;
            world.PortalRooms.Add(new World.PortalRoom(
                $"{portal.Area} Portal {portal.Cell.X:X2}{portal.Cell.Y:X2}",
                portal.DoorVertexName,
                portal.Area,
                RoomWord: doorCell.X << 8 | doorCell.Y,
                Direction: 0x0001,
                DestinationId: doorCell.X << 8 | doorCell.Y,
                DestinationArgs: (hostRoom.scroll == YamlReader.Scrolling.Vertical ? 0x0040 : 0x0000)
                    | (int)portal.Area));
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

            if (type == VertexType.Item)
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

    // Bank order of the vanilla special-items tables (0x200 stride from the bank-88
    // window); differs from the Area enum order — Kraid and Tourian swap.
    private static readonly YamlReader.Area[] VanillaTableBankOrder =
    [
        YamlReader.Area.Brinstar,
        YamlReader.Area.Norfair,
        YamlReader.Area.Tourian,
        YamlReader.Area.Kraid,
        YamlReader.Area.Ridley,
    ];

    private static readonly Lazy<Dictionary<long, (int Y, int X)>> AddressToCoordMap = new(() =>
        CoordToAddressMap.ToDictionary(kv => (long)kv.Value, kv => kv.Key));

    /// <summary>
    /// Inverse of <see cref="CoordToAddressMap"/> for vanilla layouts: resolves an item
    /// vertex's table address back to the area and world-map cell it renders at. The
    /// vanilla automap planes use the world-map coordinates directly.
    /// </summary>
    internal static bool TryGetVanillaItemCell(long address, out YamlReader.Area area, out MapGen.Point cell)
    {
        if (!AddressToCoordMap.Value.TryGetValue(address, out var coord))
        {
            area = default;
            cell = default;
            return false;
        }

        area = VanillaTableBankOrder[((int)address - MapGen.RomEmitter.TableAddress) / 0x200];
        cell = new MapGen.Point(coord.X, coord.Y);
        return true;
    }

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
