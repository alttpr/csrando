namespace RandomizerTests.Games.SuperMetroid;

using System.Buffers.Binary;
using Randomizer.Games;
using Randomizer.Games.SuperMetroid;
using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;
using Randomizer.RomModifications;
using World = Randomizer.Games.SuperMetroid.World;

[TestClass]
public sealed class RomTest
{
    private static readonly HashSet<ushort> EyeDoorPlmIds =
    [
        0xDB48, 0xDB4C, 0xDB52,
        0xDB56, 0xDB5A, 0xDB60,
    ];

    private static readonly byte[] NothingPlm = [0x2F, 0xB6, 0x00, 0x00, 0x00, 0x00];

    private static readonly Dictionary<ushort, byte[]> BossKeycardDoorHeaders = new()
    {
        [0xA56B] = [0x14, 0xD4, 0x1E, 0x16], // Kraid: left-facing keycard door
        [0xB37A] = [0x1A, 0xD4, 0x01, 0x06], // Ridley: right-facing keycard door
        [0xCC6F] = [0x14, 0xD4, 0x4E, 0x06], // Phantoon: left-facing keycard door
        [0xD78F] = [0x1A, 0xD4, 0x01, 0x26], // Draygon: right-facing keycard door
    };

    private sealed class MemoryRom : IRom
    {
        private readonly byte[] _data = Enumerable.Repeat((byte)0xCC, 8 * 1024 * 1024).ToArray();

        public byte[] Read(Address address, int length) => _data[address.Value..(address.Value + length)];
        public void Write(Address address, in ReadOnlySpan<byte> data) => data.CopyTo(_data.AsSpan(address.Value));
        public void ApplyBasePatch(FileInfo baseBPS) => throw new NotSupportedException();
        public void Resize(int size) { }
        public void UpdateChecksum() { }
        public void Dispose() { }
    }

    [TestMethod]
    public void MapRando_ReplacesEveryCompleteGadoraPlmRecord()
    {
        var world = CreateMapRandoWorld(Keycards.None);
        var rom = new MemoryRom();

        new Rom(rom, 0).WritePlms(world);

        var eyeDoorPlms = world.JsonData.RoomPLMs
            .Where(plm => EyeDoorPlmIds.Contains(plm.PlmId))
            .ToList();
        Assert.AreEqual(15, eyeDoorPlms.Count, "Expected three PLMs for each of the five vanilla Gadoras");
        Assert.AreEqual(5, eyeDoorPlms.GroupBy(plm => (plm.Room, plm.State)).Count(),
            "Expected five vanilla Gadora room states");

        foreach (var plm in eyeDoorPlms)
        {
            CollectionAssert.AreEqual(NothingPlm,
                rom.Read((SNES)(plm.Address + plm.PlmIndex * 6), NothingPlm.Length),
                $"Gadora PLM {plm.PlmId:X4} in room {plm.Room:X4}, state {plm.State:X4}");
        }
    }

    [TestMethod]
    public void MapRando_KeycardsReplaceBossGadorasAfterCleanup()
    {
        var world = CreateMapRandoWorld(Keycards.All);
        var rom = new MemoryRom();

        new Rom(rom, 0).WritePlms(world);

        var groups = world.JsonData.RoomPLMs
            .Where(plm => EyeDoorPlmIds.Contains(plm.PlmId))
            .GroupBy(plm => (plm.Room, plm.State))
            .ToList();
        Assert.AreEqual(5, groups.Count);

        foreach (var group in groups)
        {
            var plms = group.OrderBy(plm => plm.PlmIndex).ToList();
            Assert.AreEqual(3, plms.Count);

            // The Tourian Gadora has no keycard replacement. The four boss Gadoras do:
            // their eye PLM becomes the keycard door and both shutter PLMs stay deleted.
            if (group.Key.Room == 0xDDC4)
            {
                foreach (var plm in plms)
                {
                    CollectionAssert.AreEqual(NothingPlm,
                        rom.Read((SNES)(plm.Address + plm.PlmIndex * 6), NothingPlm.Length));
                }
            }
            else
            {
                CollectionAssert.AreEqual(BossKeycardDoorHeaders[group.Key.Room],
                    rom.Read((SNES)(plms[0].Address + plms[0].PlmIndex * 6), 4),
                    $"Boss Gadora in room {group.Key.Room:X4} should become a keycard door");
                foreach (var plm in plms.Skip(1))
                {
                    CollectionAssert.AreEqual(NothingPlm,
                        rom.Read((SNES)(plm.Address + plm.PlmIndex * 6), NothingPlm.Length));
                }
            }
        }
    }

    private static World CreateMapRandoWorld(Keycards keycards)
    {
        var world = new World(0, new WorldConfig
        {
            SuperMetroid = new Config { Keycards = keycards },
        }, new Randomizer.Graph.Graph(), new PRNG(1234));
        world.Map = new Map([], [], [], [], [], [], [], [], [], [], []);
        return world;
    }

    // Chosen so the map's Toilet overlaps a room that comes later in the write
    // order (here: Three Musketeers' Room), which the toilet-priority test needs.
    private const int MapWorldSeed = 1;
    private const int ToiletRoomId = 321;
    private static readonly int[] CreClobberingRoomIds = [84, 122]; // Kraid Room, Crocomire's Room
    private static readonly int[] BossRoomIds = [84, 142, 158, 193, 238];

    private static readonly object MapWorldLock = new();
    private static World? _mapWorld;
    private static MemoryRom? _mapRom;

    /// <summary>
    /// A fully map-randomized world with WriteMap applied to a blank ROM, built once
    /// and shared by the map cosmetics tests below.
    /// </summary>
    private static (World World, MemoryRom Rom) GetWrittenMapRandoWorld()
    {
        lock (MapWorldLock)
        {
            if (_mapWorld != null)
                return (_mapWorld, _mapRom!);

            string sourceDataRoot = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "../../../../../src/Randomizer/Games/SuperMetroid/data"));
            string mapCorpus = Path.Combine(sourceDataRoot, "maps");
            if (!Directory.Exists(mapCorpus)
                || !Directory.EnumerateFiles(mapCorpus, "*.avro", SearchOption.AllDirectories).Any())
            {
                Assert.Inconclusive("Requires the local SM map-rando Avro corpus.");
            }

            string oldDataRoot = JsonReader.DataRoot;
            try
            {
                JsonReader.DataRoot = sourceDataRoot;
                var world = new World(0, new WorldConfig
                {
                    SuperMetroid = new Config { MapRandomizer = MapRandomizerSetting.Standard },
                }, new Randomizer.Graph.Graph(), new PRNG(MapWorldSeed));
                Assert.IsNotNull(world.Map, "Map randomization produced no map");

                var rom = new MemoryRom();
                new Rom(rom, 0).WriteMap(world);

                _mapWorld = world;
                _mapRom = rom;
            }
            finally
            {
                JsonReader.DataRoot = oldDataRoot;
            }
            return (_mapWorld!, _mapRom!);
        }
    }

    [TestMethod]
    [DoNotParallelize]
    public void MapRando_WriteMap_SetsCreReloadFlagOnlyOnRoomsEnteredFromKraidAndCrocomire()
    {
        var (world, rom) = GetWrittenMapRandoWorld();
        var map = world.Map!;

        // Every room a player can transition into directly from Kraid's Room or
        // Crocomire's Room must have the "reload CRE" bit ($8F room header offset 8,
        // bit 1) set, because those bosses' tilesets overwrite CRE in VRAM.
        var expectedFlagged = new HashSet<int>();
        int exits = 0;
        for (int i = 0; i < map.conn_from_room_id.Count; i++)
        {
            var transitions = new List<(int Source, int Destination)>
            {
                (map.conn_from_room_id[i], map.conn_to_room_id[i]),
            };
            if (map.conn_bidirectional[i])
            {
                transitions.Add((map.conn_to_room_id[i], map.conn_from_room_id[i]));
            }

            foreach (var (source, destination) in transitions)
            {
                if (!CreClobberingRoomIds.Contains(source))
                    continue;
                var destGeo = world.JsonData.RoomGeometries.First(r => r.room_id == destination);
                expectedFlagged.Add(destGeo.rom_address);
                exits++;
            }
        }
        Assert.AreEqual(4, exits, "Kraid and Crocomire each have two exit doors");

        foreach (var geo in world.JsonData.RoomGeometries)
        {
            byte bitset = rom.Read(geo.rom_address + 8, 1)[0];
            bool shouldFlag = expectedFlagged.Contains(geo.rom_address);
            Assert.AreEqual(shouldFlag, (bitset & 0x02) != 0,
                $"CRE reload bit on room '{geo.name}'");

            if (!shouldFlag)
                continue;

            // The whole byte must be the vanilla special graphics bitflag plus the CRE bit.
            // A few rooms ship non-zero bitflags (0x01, 0x05), and generation cannot read the
            // current value back out of the ROM, so the vanilla bits come from
            // room_headers.json and have to survive untouched.
            var header = world.JsonData.RoomHeaders
                .First(r => (r.Address & 0xFFFF) == (geo.rom_address & 0xFFFF));
            Assert.AreEqual((byte)(header.SpecialGraphicsBitflag | 0x02), bitset,
                $"CRE reload byte on room '{geo.name}'");
        }
    }

    [TestMethod]
    [DoNotParallelize]
    public void MapRando_WriteMap_NeverReadsBackDataItDidNotWrite()
    {
        var (world, _) = GetWrittenMapRandoWorld();

        string sourceDataRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/Randomizer/Games/SuperMetroid/data"));
        string oldDataRoot = JsonReader.DataRoot;
        try
        {
            JsonReader.DataRoot = sourceDataRoot;

            // Generation emits a patch and never holds a base ROM, so the real IRom is a
            // LoggedRom, which serves reads only for bytes the same session already wrote.
            // Any read-modify-write of vanilla data throws KeyNotFoundException and fails the
            // whole seed, so WriteMap must source vanilla values from the json data instead.
            new Rom(new LoggedRom(), 0).WriteMap(world);
        }
        finally
        {
            JsonReader.DataRoot = oldDataRoot;
        }
    }

    [TestMethod]
    [DoNotParallelize]
    public void MapRando_WriteMap_ToiletTilesOnlyFillOtherwiseEmptyCells()
    {
        var (world, rom) = GetWrittenMapRandoWorld();
        var map = world.Map!;

        int[] areaAddresses = [0xB59000, 0xB58000, 0xB5A000, 0xB5B000, 0xB5C000, 0xB5D000];
        var (areaXOffsets, areaYOffsets) = ComputeAreaOffsets(map);

        int toiletIndex = map.room_id.IndexOf(ToiletRoomId);
        Assert.IsTrue(toiletIndex >= 0, "Map must contain the Toilet");
        var toiletCells = world.JsonData.MapRoomData.Rooms
            .First(r => r.RoomId == ToiletRoomId).MapTiles
            .Select(tile => (map.room_area[toiletIndex],
                map.room_x[toiletIndex] + tile.Coords[0],
                map.room_y[toiletIndex] + tile.Coords[1]))
            .ToHashSet();

        // Every non-Toilet room must keep all of its own map tiles: the Toilet's
        // shaft may only occupy cells no real room claims.
        bool toiletOverlapsARoom = false;
        for (int i = 0; i < map.room_id.Count; i++)
        {
            if (map.room_id[i] == ToiletRoomId)
                continue;

            var geometry = world.JsonData.RoomGeometries.First(r => r.room_id == map.room_id[i]);
            var mapTiles = world.JsonData.MapRoomData.Rooms.FirstOrDefault(r => r.RoomId == geometry.room_id);
            if (mapTiles == null)
                continue;

            int area = map.room_area[i];
            bool isBossRoom = BossRoomIds.Contains(map.room_id[i]);
            foreach (var tile in mapTiles.MapTiles)
            {
                ushort expected = isBossRoom ? (ushort)0x01DF : tile.GetTileValue();
                ushort palette = 0x0C00; // red
                if (mapTiles.Heated == true)
                    palette = 0x1800; // orange
                else if ((mapTiles.LiquidType ?? "") == "water" && (mapTiles.LiquidLevel ?? 0m) <= 1m)
                    palette = 0x1000; // green
                expected |= (ushort)(palette & 0x1C00);

                int cellX = map.room_x[i] + tile.Coords[0];
                int cellY = map.room_y[i] + tile.Coords[1];
                if (toiletCells.Contains((area, cellX, cellY)))
                    toiletOverlapsARoom = true;

                int fx = cellX - areaXOffsets[area] + 4;
                int fy = cellY - areaYOffsets[area] + 4;
                int address = areaAddresses[area] + ((fx % 32) * 2 + fy * 64) + (fx / 32) * 0x800;
                var bytes = rom.Read((SNES)address, 2);
                ushort actual = (ushort)(bytes[0] | (bytes[1] << 8));
                Assert.AreEqual(expected, actual,
                    $"Tile of '{geometry.name}' at ({tile.Coords[0]},{tile.Coords[1]}) was overwritten");
            }
        }

        Assert.IsTrue(toiletOverlapsARoom,
            "Expected the Toilet to pass through at least one room in this map");
    }

    [TestMethod]
    [DoNotParallelize]
    public void MapRando_WriteMap_ElevatorAreaMarkersSitAdjacentToTheShaft()
    {
        var (world, rom) = GetWrittenMapRandoWorld();
        var map = world.Map!;
        var (areaXOffsets, areaYOffsets) = ComputeAreaOffsets(map);

        var decoEntriesByArea = new List<(int X, int Y)>[6];
        for (int area = 0; area < 6; area++)
        {
            var entries = new List<(int X, int Y)>();
            int listPtr = BitConverter.ToUInt16(rom.Read((SNES)(0x89E000 + area * 2), 2));
            int position = 0x890000 + listPtr;
            while (BitConverter.ToUInt16(rom.Read((SNES)position, 2)) != 0)
            {
                var xy = rom.Read((SNES)(position + 2), 2);
                entries.Add((xy[0], xy[1]));
                position += 4;
            }
            decoEntriesByArea[area] = entries;
        }

        int elevatorMarkers = 0;
        for (int i = 0; i < map.conn_from_room_id.Count; i++)
        {
            int fromIndex = map.room_id.IndexOf(map.conn_from_room_id[i]);
            int toIndex = map.room_id.IndexOf(map.conn_to_room_id[i]);
            if (map.room_area[fromIndex] == map.room_area[toIndex])
                continue;

            foreach (var (roomIndex, doorId) in new[]
            {
                (fromIndex, map.conn_from_door_id[i]),
                (toIndex, map.conn_to_door_id[i]),
            })
            {
                var geometry = world.JsonData.RoomGeometries.First(r => r.room_id == map.room_id[roomIndex]);
                var door = geometry.doors[doorId];
                if (door.subtype != "elevator")
                    continue;

                int dy = door.direction == "up" ? -1 : 1;
                int area = map.room_area[roomIndex];
                int expectedX = map.room_x[roomIndex] + door.x + 4 - areaXOffsets[area];
                int expectedY = map.room_y[roomIndex] + door.y + dy + 4 - areaYOffsets[area];

                Assert.IsTrue(decoEntriesByArea[area].Contains((expectedX, expectedY)),
                    $"No area marker directly {(dy < 0 ? "above" : "below")} the " +
                    $"'{geometry.name}' elevator (expected at ({expectedX},{expectedY}))");
                elevatorMarkers++;
            }
        }

        Assert.IsTrue(elevatorMarkers > 0, "Expected at least one cross-area elevator");
    }

    private static (int[] XOffsets, int[] YOffsets) ComputeAreaOffsets(Map map)
    {
        int[] xOffsets = new int[6];
        int[] yOffsets = new int[6];
        for (int area = 0; area < 6; area++)
        {
            int a = area;
            xOffsets[area] = map.room_x.Where((_, idx) => map.room_area[idx] == a).Min();
            yOffsets[area] = map.room_y.Where((_, idx) => map.room_area[idx] == a).Min();
        }
        return (xOffsets, yOffsets);
    }
}
