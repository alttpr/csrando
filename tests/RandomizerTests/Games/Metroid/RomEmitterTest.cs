namespace RandomizerTests.Games.Metroid;

using Randomizer.Games;
using Randomizer.Games.Metroid;
using Randomizer.Games.Metroid.MapGen;
using Randomizer.Graph;
using Randomizer.RomModifications;
using static Randomizer.Games.Metroid.YamlReader;
using GameRandomizer = Randomizer.Games.Metroid.GameRandomizer;

/// <summary>
/// ROM emission acceptance: the emitted patches must follow the engine's exact data
/// formats. The table tests re-parse the emitted blob with a reimplementation of the
/// engine's ScanForItems walk (rows ascending by Y linked by absolute words, entries
/// ascending by X chained by size offsets, $00-terminated payload chains), so a payload
/// that parses here is one the engine can read.
/// </summary>
[TestClass]
public sealed class RomEmitterTest
{
    private static readonly Lazy<(ScreenCatalog Catalog, YamlData Data)> Loaded = new(() =>
    {
        var reader = new YamlReader(new Config());
        reader.LoadData();
        return (ScreenCatalog.Build(reader.Data!.screens, reader.Data!.rooms, reader.Data!.transitions), reader.Data!);
    });

    private static (GeneratedWorld World, RomEmission Emission) Emit(int seed)
    {
        var world = new TopologyGenerator(Loaded.Value.Catalog).Generate(seed);
        ScreenFitter.Fit(world.Grid, Loaded.Value.Catalog, seed);
        return (world, RomEmitter.Emit(world, Loaded.Value.Data.rooms));
    }

    private static int BankBase(Area area) => 0x680000 + area switch
    {
        Area.Brinstar => 1,
        Area.Norfair => 2,
        Area.Tourian => 3,
        Area.Kraid => 4,
        Area.Ridley => 5,
        _ => throw new ArgumentOutOfRangeException(nameof(area))
    } * 0x8000;

    private static readonly Area[] TableAreas =
        [Area.Brinstar, Area.Norfair, Area.Tourian, Area.Kraid, Area.Ridley];

    private record ParsedEntry(int Y, int X, List<byte[]> Payloads, int FirstPayloadAddress);

    /// <summary>Walks one area table exactly like the engine's ScanForItems does.</summary>
    private static List<ParsedEntry> ParseTable(RomEmission emission, Area area)
    {
        var blob = emission.Patches[RomEmitter.TableAddress];
        var pointer = emission.Patches[BankBase(area) + 0x1598];
        int rowOffset = (pointer[0] | pointer[1] << 8) - 0x8000;
        Assert.IsTrue(rowOffset >= 0 && rowOffset < blob.Length, $"{area} table pointer out of blob");

        var entries = new List<ParsedEntry>();
        int lastY = -1;

        while (true)
        {
            int y = blob[rowOffset];
            Assert.IsTrue(y > lastY, $"{area} rows must ascend ({lastY} then {y})");
            lastY = y;
            int nextRowWord = blob[rowOffset + 1] | blob[rowOffset + 2] << 8;

            int pos = rowOffset + 3;
            int lastX = -1;
            while (true)
            {
                int x = blob[pos];
                Assert.IsTrue(x > lastX, $"{area} row {y} entries must ascend ({lastX} then {x})");
                lastX = x;
                int offsetByte = blob[pos + 1];

                // Payload chain: type bytes route through handler indices 1-9 (plus the
                // extended custom-item type $0B from hooks.asm); $00 ends.
                var payloads = new List<byte[]>();
                int p = pos + 2;
                int firstPayloadAddress = RomEmitter.TableAddress + p;
                while (blob[p] != 0x00)
                {
                    int type = blob[p] & 0x0F;
                    Assert.IsTrue(type is >= 1 and <= 9 or 0x0B, $"{area} ({x},{y}): bad item type 0x{blob[p]:X2}");
                    int length = type switch
                    {
                        0x02 => 3,                          // power-up: type, id, position
                        0x0B => 3,                          // custom item: type, id, position
                        0x04 => 2,                          // elevator: type, data
                        0x09 => 2,                          // door: type, info
                        0x03 or 0x08 => 1,                  // special enemies / rinkas: type only
                        0x06 => 3,                          // mother brain: type + 2 data
                        0x05 => ScanCannons(blob, p),       // cannons: type + pairs ending in $3E
                        0x07 => 2,                          // zeebetites: type + data
                        0x01 => 6,                          // squeept: type + 5 enemy data bytes
                        _ => throw new AssertFailedException($"unhandled type {type}")
                    };
                    payloads.Add(blob[p..(p + length)]);
                    p += length;
                }

                entries.Add(new ParsedEntry(y, x, payloads, firstPayloadAddress));

                if (offsetByte == 0xFF)
                {
                    pos = p + 1;
                    break;
                }
                Assert.AreEqual(pos + offsetByte, p + 1, $"{area} ({x},{y}): entry offset byte mismatch");
                pos += offsetByte;
            }

            if (nextRowWord == 0xFFFF)
                break;
            rowOffset = nextRowWord - 0x8000;
            Assert.IsTrue(rowOffset > 0 && rowOffset < blob.Length, $"{area} next-row word out of blob");
        }

        return entries;
    }

    /// <summary>Cannon payloads are [type][position pairs...] ending with a $3E pair-terminator.</summary>
    private static int ScanCannons(byte[] blob, int start)
    {
        int p = start + 1;
        while (blob[p] != 0x3E)
            p++;
        return p - start + 1;
    }

    [TestMethod]
    public void Emit_GridMatchesAssignedScreens()
    {
        var (world, emission) = Emit(1);
        var bytes = emission.Patches[RomEmitter.GridAddress];
        Assert.AreEqual(WorldGrid.Size * WorldGrid.Size, bytes.Length);

        var occupied = world.Grid.Cells.ToDictionary(c => c.Position, c => c.AssignedScreen!.ScreenId);
        for (int y = 0; y < WorldGrid.Size; y++)
        {
            for (int x = 0; x < WorldGrid.Size; x++)
            {
                byte value = bytes[y * WorldGrid.Size + x];
                if (occupied.TryGetValue(new Point(x, y), out var screenId))
                    Assert.AreEqual((byte)screenId, value, $"({x},{y}) should hold its screen id");
                else
                    Assert.AreEqual(0xFF, value, $"({x},{y}) should be empty");
            }
        }
    }

    [TestMethod]
    public void Emit_TablesParseAndStayInsideOwnArea()
    {
        for (int seed = 1; seed <= 10; seed++)
        {
            var (world, emission) = Emit(seed);
            foreach (var area in TableAreas)
            {
                foreach (var entry in ParseTable(emission, area))
                {
                    var cell = world.Grid.Cell(entry.X, entry.Y);
                    Assert.IsNotNull(cell, $"seed {seed}: {area} entry at ({entry.X},{entry.Y}) on empty cell");

                    // Elevator mirrors legitimately sit on the upper area's cell; everything
                    // else must be on a cell of the table's own area.
                    if (cell.Area != area)
                    {
                        Assert.IsTrue(entry.Payloads.All(p => p[0] == 0x04),
                            $"seed {seed}: {area} non-elevator entry on {cell.Area} cell at ({entry.X},{entry.Y})");
                    }
                }
            }
        }
    }

    [TestMethod]
    public void Emit_EveryItemCellHasAPowerUpEntryAndAddress()
    {
        for (int seed = 1; seed <= 10; seed++)
        {
            var (world, emission) = Emit(seed);
            var blob = emission.Patches[RomEmitter.TableAddress];

            var itemCells = world.Grid.Cells
                .Where(c => c.Role is CellRole.Item or CellRole.Boss
                    && c.AssignedScreen!.ItemLocationNames.Count > 0)
                .Select(c => c.Position)
                .ToHashSet();
            Assert.IsTrue(itemCells.Count > 0, $"seed {seed}: no item cells");

            CollectionAssert.AreEquivalent(itemCells.ToList(), emission.ItemAddresses.Keys.ToList(),
                $"seed {seed}: item addresses must cover exactly the item cells");

            var parsedPowerUps = TableAreas
                .SelectMany(a => ParseTable(emission, a))
                .Where(e => e.Payloads[0][0] == 0x02)
                .ToDictionary(e => new Point(e.X, e.Y));

            foreach (var cell in itemCells)
            {
                Assert.IsTrue(parsedPowerUps.ContainsKey(cell), $"seed {seed}: no power-up entry at {cell}");
                Assert.AreEqual(parsedPowerUps[cell].FirstPayloadAddress, emission.ItemAddresses[cell],
                    $"seed {seed}: address at {cell} must point at the power-up type byte");
                Assert.AreEqual(0x02, blob[emission.ItemAddresses[cell] - RomEmitter.TableAddress],
                    $"seed {seed}: placeholder type byte at {cell}");
            }
        }
    }

    [TestMethod]
    public void Emit_MapStationsHoldTheFixedMapItem()
    {
        for (int seed = 1; seed <= 10; seed++)
        {
            var (world, emission) = Emit(seed);

            var stationCells = world.Grid.Cells
                .Where(c => c.Role == CellRole.MapStation)
                .Select(c => (c.Area, c.Position))
                .ToList();
            Assert.AreEqual(4, stationCells.Count, $"seed {seed}: one map station per area except Tourian");
            CollectionAssert.AreEquivalent(
                new[] { Area.Brinstar, Area.Norfair, Area.Kraid, Area.Ridley },
                stationCells.Select(s => s.Area).ToList(), $"seed {seed}");

            // Each station carries the final [custom item $0B, map id $CE, position] entry,
            // outside the fillable pool (no exposed item address).
            var mapEntries = TableAreas
                .SelectMany(a => ParseTable(emission, a))
                .Where(e => e.Payloads.Any(p => p[0] == 0x0B))
                .ToList();
            CollectionAssert.AreEquivalent(
                stationCells.Select(s => s.Position).ToList(),
                mapEntries.Select(e => new Point(e.X, e.Y)).ToList(),
                $"seed {seed}: map item entries must sit exactly on the map station cells");

            foreach (var entry in mapEntries)
            {
                var payload = entry.Payloads.Single(p => p[0] == 0x0B);
                Assert.AreEqual(0xCE, payload[1], $"seed {seed}: map item id at ({entry.X},{entry.Y})");
                Assert.IsFalse(emission.ItemAddresses.ContainsKey(new Point(entry.X, entry.Y)),
                    $"seed {seed}: map station at ({entry.X},{entry.Y}) must not be a fillable location");
            }
        }
    }

    [TestMethod]
    public void Vanilla_PatchesTheInitialMapReveal()
    {
        // Vanilla layouts place no map-station pickups; the seed pre-reveals every
        // area's automap through the M1MapInitialReveal byte instead.
        var vanilla = new GameRandomizer(
            [new WorldConfig { Metroid = new Config() }], new PRNG(21));
        vanilla.Randomize();
        var patch = ((World)vanilla.Worlds[0]).PatchData![AutomapComposer.InitialRevealAddress];
        CollectionAssert.AreEqual(new byte[] { 0x1F }, patch, "vanilla seeds reveal all five areas");

        var shuffled = new GameRandomizer(
            [new WorldConfig { Metroid = new Config { MapShuffle = true } }], new PRNG(21));
        shuffled.Randomize();
        Assert.IsFalse(((World)shuffled.Worlds[0]).PatchData!.ContainsKey(AutomapComposer.InitialRevealAddress),
            "map shuffle keeps the assembled $00 (areas revealed by map stations)");
    }

    [TestMethod]
    public void Emit_ElevatorEntriesFollowVanillaPattern()
    {
        var (world, emission) = Emit(1);
        var parsed = TableAreas.ToDictionary(a => a, a => ParseTable(emission, a));

        List<byte[]> ElevatorsAt(Area area, Point p) =>
            parsed[area].Where(e => e.X == p.X && e.Y == p.Y)
                .SelectMany(e => e.Payloads.Where(pl => pl[0] == 0x04)).ToList();

        var links = world.Grid.Links.Where(l => l.Type == LinkType.Elevator).ToList();
        Assert.AreEqual(4, links.Count, "expected the four area elevators");

        foreach (var link in links)
        {
            var upperArea = world.Grid.Cell(link.A)!.Area;
            var lowerArea = world.Grid.Cell(link.B)!.Area;
            var platform = link.B.Step(Direction.Down);

            var down = ElevatorsAt(upperArea, link.A);
            Assert.AreEqual(1, down.Count, $"down elevator at {link.A}");
            Assert.AreEqual((byte)(int)lowerArea, down[0][1], $"down byte for {upperArea}->{lowerArea}");

            var mirror = ElevatorsAt(lowerArea, link.A);
            Assert.AreEqual(1, mirror.Count, $"mirror at {link.A} in {lowerArea} table");
            Assert.AreEqual(0x81, mirror[0][1], $"mirror byte for {upperArea}->{lowerArea}");

            var up = ElevatorsAt(lowerArea, platform);
            Assert.AreEqual(1, up.Count, $"up elevator at {platform}");
            Assert.AreEqual((byte)(0x80 | (int)lowerArea), up[0][1], $"up byte for {lowerArea}");
        }

        var escape = ElevatorsAt(Area.Tourian, world.Landmarks["EscapeShaft"]);
        Assert.AreEqual(1, escape.Count, "escape elevator entry");
        Assert.AreEqual(0x8F, escape[0][1], "escape elevator must lead to the ending");
    }

    [TestMethod]
    public void Emit_RespawnPositionsPatchedForAllAreas()
    {
        var (world, emission) = Emit(1);

        var brinstar = emission.Patches[BankBase(Area.Brinstar) + 0x15D7];
        Assert.AreEqual((byte)world.Start.X, brinstar[0], "Brinstar respawn X must be the spawn cell");
        Assert.AreEqual((byte)world.Start.Y, brinstar[1], "Brinstar respawn Y must be the spawn cell");

        foreach (var area in TableAreas.Skip(1))
        {
            var patch = emission.Patches[BankBase(area) + 0x15D7];
            var cell = world.Grid.Cell(patch[0], patch[1]);
            Assert.IsNotNull(cell, $"{area} respawn on an occupied cell");
            Assert.AreEqual(area, cell.Area, $"{area} respawn must be inside the area");
            Assert.AreEqual(CellRole.ElevatorBottom, cell.Role, $"{area} respawn must be its elevator platform");
        }
    }

    [TestMethod]
    public void Emit_ComboRespawnsBrinstarAtThePortalRoom()
    {
        for (int seed = 1; seed <= 5; seed++)
        {
            var world = new TopologyGenerator(Loaded.Value.Catalog).Generate(seed);
            ScreenFitter.Fit(world.Grid, Loaded.Value.Catalog, seed);
            var emission = RomEmitter.Emit(world, Loaded.Value.Data.rooms, respawnAtPortal: true);

            // Dying in Brinstar must respawn at the portal room, with the third byte
            // ($95D9 in-screen spawn height) patched so Samus drops onto the room floor.
            var portal = world.Landmarks["Portal0"];
            var brinstar = emission.Patches[BankBase(Area.Brinstar) + 0x15D7];
            Assert.AreEqual(3, brinstar.Length, $"seed {seed}: combo Brinstar respawn must patch $95D7-$95D9");
            Assert.AreEqual((byte)portal.X, brinstar[0], $"seed {seed}");
            Assert.AreEqual((byte)portal.Y, brinstar[1], $"seed {seed}");
            Assert.AreEqual((byte)0x6E, brinstar[2], $"seed {seed}");

            // The lower areas keep their elevator respawns, vanilla two-byte patches.
            foreach (var area in TableAreas.Skip(1))
                Assert.AreEqual(2, emission.Patches[BankBase(area) + 0x15D7].Length, $"seed {seed}: {area}");
        }
    }

    [TestMethod]
    public void Emit_BlobStaysInsideReservedWindow()
    {
        for (int seed = 1; seed <= 20; seed++)
        {
            var (_, emission) = Emit(seed);
            Assert.IsTrue(emission.TableSize <= RomEmitter.TableLimit,
                $"seed {seed}: blob 0x{emission.TableSize:X} exceeds the reserved window");
        }
    }

    [TestMethod]
    public void Emit_IsDeterministic()
    {
        var (_, a) = Emit(7);
        var (_, b) = Emit(7);
        CollectionAssert.AreEqual(a.Patches.Keys.OrderBy(k => k).ToList(), b.Patches.Keys.OrderBy(k => k).ToList());
        foreach (var (address, bytes) in a.Patches)
            CollectionAssert.AreEqual(bytes, b.Patches[address], $"patch at 0x{address:X}");
    }

    [TestMethod]
    public void Combo_MapShuffle_WrittenRomContainsEveryFilledItem()
    {
        // Combo regression: the combo writer runs its own item pass (WriteItemsToRom) and
        // then clears the location addresses, so M1's patch data (which contains the item
        // tables with placeholder ids) must already be on the ROM at that point. Getting
        // this wrong shows up in-game as every item being Bombs (placeholder id 0).
        var config = new WorldConfig
        {
            Combo = new Randomizer.Games.Combo.Config(),
            Alttp = new Randomizer.Games.Alttp.Config(),
            Metroid = new Config { MapShuffle = true, EarlyMorph = true },
        };
        config.Alttp.SelectRandomValues(new PRNG(42));

        var randomizer = new Randomizer.Games.Combo.GameRandomizer([config], new PRNG(4321));
        randomizer.Randomize();
        Assert.IsTrue(randomizer.IsWinnable(), "combo seed should be winnable");

        var comboWorld = (Randomizer.Games.Combo.World)randomizer.Worlds[0];
        var m1Locations = comboWorld.GetLocationsOfType(VertexType.Item)
            .OfType<Randomizer.Graph.Vertex>()
            .Where(l => l.World is World && l.Item != null && l.Addresses != null)
            .ToList();
        Assert.IsTrue(m1Locations.Count >= 31, $"expected a full M1 item table, got {m1Locations.Count}");

        // Capture the expected bytes up front: the combo item pass nulls the addresses.
        var expected = m1Locations.Select(l => (
            Address: (int)l.Addresses![0],
            Bytes: Randomizer.Games.Combo.ItemMapper.GetItemBytes(l, l.Item!) ?? l.Item!.Bytes!,
            l.Name)).ToList();

        var broker = new LoggedRomBroker();
        randomizer.Write(broker);
        var written = ParseIps(broker.Worlds.Single().Value.IpsPatch);

        foreach (var (address, bytes, name) in expected)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                Assert.IsTrue(written.TryGetValue(address + i, out var value),
                    $"'{name}': nothing written at 0x{address + i:X}");
                Assert.AreEqual(bytes[i], value, $"'{name}': wrong byte {i} at 0x{address:X}");
            }
        }

        // The generated portal anchors must be wired into the graph and into both games'
        // transition tables. M1 now builds the full default endpoint pool for combo, so
        // reduced game sets can reassign endpoints whose original partner is absent.
        var portalEdges = comboWorld.DerivePortalEdges();
        var m1Edges = portalEdges.Where(e => e.From.GameId == "m1").ToList();
        var alttpEdges = portalEdges.Where(e => e.From.GameId == "alttp" && e.To.GameId == "m1").ToList();
        Assert.AreEqual(3, m1Edges.Count, "M1 should use all three default portal rooms");
        foreach (var anchor in comboWorld.M1World!.PortalAnchors)
            Assert.IsNotNull(comboWorld.M1World.GetLocation(anchor.EntryVertexName), "portal vertex missing");

        AssertWords(written, Randomizer.Games.Metroid.RomWriter.TransitionTableAddress,
            [.. m1Edges.SelectMany(e => e.From.RowTo(e.To).Select(word => (ushort)word)), 0x0000],
            "M1 transition table");
        AssertWords(written, 0x550000,
            [.. alttpEdges.SelectMany(e => e.From.RowTo(e.To).Select(word => (ushort)word)), 0x0000],
            "ALttP transition table");

        Assert.IsTrue(randomizer.SpoilerLog!.Spoiler.ContainsKey("m1Map"),
            "combo spoiler must contain the generated M1 map");

        // Combo seeds respawn Brinstar deaths at the portal room ($95D7-$95D9 in bank 1)
        // so dying doesn't strand the player at the unused generated start cell.
        var portalCell = comboWorld.M1World.GeneratedMap!.Landmarks["Portal0"];
        int respawnAddress = BankBase(Area.Brinstar) + 0x15D7;
        Assert.AreEqual((byte)portalCell.X, written[respawnAddress], "portal respawn X");
        Assert.AreEqual((byte)portalCell.Y, written[respawnAddress + 1], "portal respawn Y");
        Assert.AreEqual((byte)0x6E, written[respawnAddress + 2], "portal respawn spawn height");

        // The explicit Early Morph option makes M1's Morph reachable with starting
        // equipment rather than leaving it to normal assumed-fill ordering.
        var morphItem = comboWorld.M1World.GetItem("Morph");
        var morphLocation = comboWorld.GetLocationsOfType(VertexType.Item)
            .OfType<Randomizer.Graph.Vertex>()
            .Single(l => ReferenceEquals(l.Item, morphItem));
        morphLocation.Item = null;
        try
        {
            // Same searcher construction as FrontFillCrossWorld: starting items only.
            var sphereZero = randomizer
                .GetSearcherForInventory(comboWorld, comboWorld.StartingItems.All().Select(x => x.Key), comboWorld.Start)
                .GetEmptyLocationsInSet(Randomizer.Graph.ItemSetName.DefaultSet, null, true);
            Assert.IsTrue(sphereZero.Contains(morphLocation),
                $"M1 Morph at '{morphLocation.Name}' is not reachable with starting items");
        }
        finally
        {
            morphLocation.Item = morphItem;
        }
    }

    [TestMethod]
    public void Combo_Vanilla_PortalTablesMatchLegacyLayout()
    {
        // The graph-derived portal model must reproduce the frozen vanilla-layout
        // transition tables byte for byte on a vanilla quad seed.
        var config = new WorldConfig
        {
            Combo = new Randomizer.Games.Combo.Config(),
            Alttp = new Randomizer.Games.Alttp.Config(),
            SuperMetroid = new Randomizer.Games.SuperMetroid.Config(),
            Zelda1 = new Randomizer.Games.Zelda1.Config(),
            Metroid = new Config(),
        };
        config.Alttp.SelectRandomValues(new PRNG(42));

        var randomizer = new Randomizer.Games.Combo.GameRandomizer([config], new PRNG(777));
        randomizer.Randomize();

        var broker = new LoggedRomBroker();
        randomizer.Write(broker);
        var written = ParseIps(broker.Worlds.Single().Value.IpsPatch);

        // SM door pointers are the C#-generated portal room conversions in the
        // reserved slots ($83B100 + slot*0x18; in at +0, out at +12).
        AssertWords(written, 0x300000,
        [
            0xB10C, 0x0001, 0x0200, 0x0000,
            0xB124, 0x0001, 0x0201, 0x0000,
            0xB13C, 0x0001, 0x0202, 0x0040,
            0xB154, 0x0001, 0x0203, 0x0040,
            0xB184, 0x0002, 0x000C, 0x0003,
            0xB16C, 0x0003, 0x1515, 0x0061,
            0x0000
        ], "SM transition table");

        AssertWords(written, 0x550000,
        [
            0x0122, 0x0035, 0x0000, 0xB100, 0x0000,
            0x00E5, 0x0003, 0x0000, 0xB118, 0x0000,
            0x010E, 0x0077, 0x0000, 0xB130, 0x0000,
            0x0115, 0x0070, 0x0000, 0xB148, 0x0000,
            0x0122, 0x0011, 0x0002, 0x0066, 0x0003,
            0x011F, 0x0002, 0x0003, 0x0B0C, 0x0040,
            0x0000
        ], "ALttP transition table");

        AssertWords(written, 0x63A000,
        [
            0x0066, 0x0001, 0x0220, 0x0000,
            0x000C, 0x0000, 0xB178, 0x0000,
            0x0070, 0x0003, 0x0810, 0x0042,
            0x0000
        ], "Z1 transition table");

        AssertWords(written, Randomizer.Games.Metroid.RomWriter.TransitionTableAddress,
        [
            0x0B0C, 0x0001, 0x0001, 0x0210, 0x0000,
            0x1515, 0x0001, 0x0000, 0xB160, 0x0000,
            0x0810, 0x0001, 0x0002, 0x0070, 0x0003,
            0x0000
        ], "M1 transition table");
    }

    [TestMethod]
    public void MapShuffle_SpoilerContainsTheGeneratedMap()
    {
        var randomizer = new GameRandomizer(
            [new WorldConfig { Metroid = new Config { MapShuffle = true } }], new PRNG(11));
        randomizer.Randomize();

        Assert.IsTrue(randomizer.SpoilerLog!.Spoiler.TryGetValue("m1Map", out var section),
            "spoiler must contain the m1Map section");
        var data = System.Text.Json.JsonSerializer.Deserialize<MapSpoilerJson>(section["data"],
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var world = (World)randomizer.Worlds[0];
        var grid = world.GeneratedMap!.Grid;

        Assert.AreEqual(grid.Cells.Count(), data.Cells.Count, "every grid cell must be in the spoiler");
        Assert.IsTrue(data.Landmarks.ContainsKey("Start"), "start landmark");
        Assert.IsTrue(data.Landmarks.ContainsKey("MotherBrain"), "Mother Brain landmark");
        Assert.IsTrue(data.Landmarks.ContainsKey("Portal0"), "portal landmark");

        // Every filled item location must appear on the map at its cell.
        var filledLocations = world.GetLocationsOfType(VertexType.Item)
            .Count(l => l.Item != null);
        Assert.AreEqual(filledLocations, data.ItemPlacements.Count, "item placements");

        foreach (var cell in data.Cells)
        {
            var gridCell = grid.Cell(cell.X, cell.Y);
            Assert.IsNotNull(gridCell, $"spoiler cell ({cell.X},{cell.Y}) not on the grid");
            Assert.AreEqual(gridCell.Area.ToString(), cell.Area, $"area at ({cell.X},{cell.Y})");
            Assert.AreEqual($"{gridCell.AssignedScreen!.ScreenId:X2}", cell.Screen, $"screen at ({cell.X},{cell.Y})");
        }
    }

    private sealed record MapSpoilerJson(
        List<MapSpoilerJsonCell> Cells,
        Dictionary<string, string> ItemPlacements,
        Dictionary<string, string> Landmarks);

    private sealed record MapSpoilerJsonCell(
        int X, int Y, string Area, string? Role, string Screen, Dictionary<string, string> Edges);

    [TestMethod]
    public void Standalone_TransitionTableIsTerminated()
    {
        // Standalone seeds must neuter the base patch's vanilla transition entry, or a
        // generated door at the vanilla portal coordinate would transition into nothing.
        var randomizer = new GameRandomizer(
            [new WorldConfig { Metroid = new Config { MapShuffle = true } }], new PRNG(5));
        randomizer.Randomize();

        var broker = new LoggedRomBroker();
        randomizer.Write(broker);
        var written = ParseIps(broker.Worlds.Single().Value.IpsPatch);

        AssertWords(written, Randomizer.Games.Metroid.RomWriter.TransitionTableAddress,
            [0x0000], "standalone M1 transition table");
    }

    private static void AssertWords(Dictionary<int, byte> written, int address, ushort[] expected, string what)
    {
        for (int i = 0; i < expected.Length; i++)
        {
            int a = address + i * 2;
            Assert.IsTrue(written.ContainsKey(a) && written.ContainsKey(a + 1),
                $"{what}: nothing written at 0x{a:X} (word {i})");
            int actual = written[a] | written[a + 1] << 8;
            Assert.AreEqual(expected[i], actual, $"{what}: word {i} at 0x{a:X}");
        }
    }

    /// <summary>Reads a plain IPS patch (as LoggedRom emits: sorted records, no RLE).</summary>
    private static Dictionary<int, byte> ParseIps(byte[] ips)
    {
        var bytes = new Dictionary<int, byte>();
        int p = 5; // after "PATCH"
        while (p + 3 < ips.Length)
        {
            int offset = ips[p] << 16 | ips[p + 1] << 8 | ips[p + 2];
            int length = ips[p + 3] << 8 | ips[p + 4];
            p += 5;
            for (int i = 0; i < length; i++)
                bytes[offset + i] = ips[p + i];
            p += length;
        }
        Assert.AreEqual('E', (char)ips[^3]);
        return bytes;
    }

    [TestMethod]
    public void MapShuffle_WrittenRomContainsEveryFilledItem()
    {
        // The full pipeline: generate, fill, then write to a logged ROM and verify that
        // every filled item's bytes ended up at its emitted table address.
        var randomizer = new GameRandomizer(
            [new WorldConfig { Metroid = new Config { MapShuffle = true } }], new PRNG(1234));
        randomizer.Randomize();
        Assert.IsTrue(randomizer.IsWinnable(), "filled world should be winnable");

        var world = (World)randomizer.Worlds[0];
        var rom = new LoggedRom();
        RomWriter.Write(rom, world, new PRNG(1234));

        var itemLocations = world.GetLocationsOfType(VertexType.Item)
            .OfType<Randomizer.Graph.Vertex>()
            .Where(l => l.Item?.Bytes != null)
            .ToList();
        Assert.IsTrue(itemLocations.Count >= 31, $"expected a full item table, got {itemLocations.Count}");

        foreach (var location in itemLocations)
        {
            Assert.IsNotNull(location.Addresses, $"'{location.Name}' has no emitted address");
            var written = rom.Read((int)location.Addresses[0], location.Item!.Bytes!.Length);
            CollectionAssert.AreEqual(location.Item.Bytes, written,
                $"'{location.Name}' item bytes not present at 0x{location.Addresses[0]:X}");
        }
    }
}
