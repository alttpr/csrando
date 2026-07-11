namespace RandomizerTests.Games.Combo;

using Randomizer.Games;
using Randomizer.Games.Combo;
using Randomizer.Graph;
using Randomizer.RomModifications;
using ComboWorld = Randomizer.Games.Combo.World;
using Graph = Randomizer.Graph.Graph;
using M1Area = Randomizer.Games.Metroid.YamlReader.Area;
using Z1ShopShuffleOption = Randomizer.Games.Zelda1.ShopShuffleOption;

// Combo world construction builds every present game's world and is by far the heaviest
// setup in the suite; keeping these tests out of the parallel schedule avoids adding
// pressure to the ALttP logic tests, which are already flaky under parallel load
// (see the caching FIXME in LogicTestBase).
[TestClass]
[DoNotParallelize]
public sealed class PortalTests
{
    /// <summary>Minimal in-memory rom capturing PortalWriter output.</summary>
    private sealed class MemoryRom : IRom
    {
        private readonly byte[] _data = new byte[8 * 1024 * 1024];

        public byte[] Read(Address address, int length) => _data[address.Value..(address.Value + length)];
        public void Write(Address address, in ReadOnlySpan<byte> data) => data.CopyTo(_data.AsSpan(address.Value));
        public void ApplyBasePatch(FileInfo baseBPS) => throw new NotSupportedException();
        public void Resize(int size) { }
        public void UpdateChecksum() { }
        public void Dispose() { }
    }

    private static ComboWorld CreateWorld(bool alttp = false, bool sm = false, bool z1 = false, bool m1 = false,
        string initialGame = "alttp", int seed = 42, Z1ShopShuffleOption z1Shop = Z1ShopShuffleOption.Off,
        bool m1MapShuffle = false, bool smMapRando = false)
    {
        var worldConfig = new WorldConfig
        {
            Game = RandomizerTarget.Combo,
            Combo = new Randomizer.Games.Combo.Config { InitialGame = initialGame },
            Alttp = alttp ? new Randomizer.Games.Alttp.Config() : null,
            SuperMetroid = sm ? new Randomizer.Games.SuperMetroid.Config
            {
                MapRandomizer = smMapRando
                    ? Randomizer.Games.SuperMetroid.MapRandomizerSetting.Standard
                    : Randomizer.Games.SuperMetroid.MapRandomizerSetting.None,
            } : null,
            Zelda1 = z1 ? new Randomizer.Games.Zelda1.Config { ShopShuffle = z1Shop, Triforces = "8" } : null,
            Metroid = m1 ? new Randomizer.Games.Metroid.Config { MapShuffle = m1MapShuffle } : null,
        };
        return new ComboWorld(0, worldConfig, new Graph(), new PRNG(seed));
    }

    private static byte[] Words(params int[] words) =>
        words.SelectMany(w => new[] { (byte)(w & 0xFF), (byte)(w >> 8 & 0xFF) }).ToArray();

    private static void AssertTable(MemoryRom rom, int address, byte[] expected, string table)
    {
        CollectionAssert.AreEqual(expected, rom.Read(address, expected.Length),
            $"{table} transition table mismatch");
    }

    private static IWorld WorldFor(ComboWorld world, PortalAnchor anchor) => anchor.GameId switch
    {
        "sm" => world.SMWorld!,
        "alttp" => world.AlttpWorld!,
        "z1" => world.Z1World!,
        "m1" => world.M1World!,
        _ => throw new ArgumentException($"Unknown portal game id '{anchor.GameId}'"),
    };

    private static readonly byte[] VanillaMapPreopenedDoorCode =
    [
        0xAF, 0xB2, 0xD8, 0x7E, 0x09, 0x01, 0x00, 0x8F, 0xB2, 0xD8, 0x7E,
        0xAF, 0xB8, 0xD8, 0x7E, 0x09, 0x00, 0x10, 0x8F, 0xB8, 0xD8, 0x7E,
        0xAF, 0xB6, 0xD8, 0x7E, 0x09, 0x04, 0x00, 0x8F, 0xB6, 0xD8, 0x7E,
        0xA9, 0x00, 0x00, 0x6B,
    ];

    private static readonly byte[] VanillaMapPortalEntryDoorCode =
    [
        0xAF, 0xB2, 0xD8, 0x7E, 0x09, 0x01, 0x00, 0x8F, 0xB2, 0xD8, 0x7E,
        0xAF, 0xB8, 0xD8, 0x7E, 0x09, 0x00, 0x10, 0x8F, 0xB8, 0xD8, 0x7E,
        0xAF, 0xB6, 0xD8, 0x7E, 0x09, 0x04, 0x00, 0x8F, 0xB6, 0xD8, 0x7E,
        0x22, 0x79, 0x9A, 0x80, 0x6B,
    ];

    [TestMethod]
    public void Sm_VanillaMap_PreopensSmz3Doors()
    {
        var world = CreateWorld(sm: true);
        var rom = new MemoryRom();
        rom.Write(0x304059, [0x22, 0x79, 0x9A, 0x80]); // JSL $809A79

        new Randomizer.Games.SuperMetroid.Rom(rom, 0).WriteMiscPatches(world.SMWorld!);

        CollectionAssert.AreEqual(VanillaMapPreopenedDoorCode,
            rom.Read((SNES)0x80D004, VanillaMapPreopenedDoorCode.Length));
        CollectionAssert.AreEqual(new byte[] { 0x22, 0x40, 0xD0, 0x80 }, rom.Read(0x304059, 4));
        CollectionAssert.AreEqual(VanillaMapPortalEntryDoorCode,
            rom.Read((SNES)0x80D040, VanillaMapPortalEntryDoorCode.Length));

        string[] preopenedDoors =
        [
            "Crateria - Red Brinstar Elevator Room - Top Door",
            "Norfair - Business Center - Middle Left Door",
            "Brinstar - Construction Zone - Right Door",
        ];
        foreach (var doorName in preopenedDoors)
            Assert.AreEqual("blue", ((Randomizer.Games.SuperMetroid.Vertex)world.SMWorld!.GetLocation(doorName)).Node!.NodeSubType,
                $"{doorName} should be blue in logic");
    }

    [TestMethod]
    public void Sm_MapRando_DoesNotPreopenSmz3Doors()
    {
        var world = CreateWorld(sm: true);
        world.SMWorld!.Map = new([], [], [], [], [], [], [], [], [], [], []);
        var rom = new MemoryRom();
        var untouched = Enumerable.Repeat((byte)0xCC, VanillaMapPreopenedDoorCode.Length).ToArray();
        var untouchedPortalCode = Enumerable.Repeat((byte)0xCC, VanillaMapPortalEntryDoorCode.Length).ToArray();
        rom.Write((SNES)0x80D004, untouched);
        rom.Write(0x304059, [0x22, 0x79, 0x9A, 0x80]); // JSL $809A79
        rom.Write((SNES)0x80D040, untouchedPortalCode);

        new Randomizer.Games.SuperMetroid.Rom(rom, 0).WriteMiscPatches(world.SMWorld);

        CollectionAssert.AreEqual(untouched,
            rom.Read((SNES)0x80D004, untouched.Length));
        CollectionAssert.AreEqual(new byte[] { 0x22, 0x79, 0x9A, 0x80 }, rom.Read(0x304059, 4));
        CollectionAssert.AreEqual(untouchedPortalCode,
            rom.Read((SNES)0x80D040, untouchedPortalCode.Length));
    }

    [TestMethod]
    public void Sm_MapRando_StartsAtCrateriaMapRoom()
    {
        string oldDataRoot = Randomizer.Games.SuperMetroid.Model.JsonReader.DataRoot;
        try
        {
            // The large map corpus is intentionally excluded from build output, so use
            // its source location for this production-pipeline regression test.
            Randomizer.Games.SuperMetroid.Model.JsonReader.DataRoot = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory, "../../../../../src/Randomizer/Games/SuperMetroid/data"));

            var world = CreateWorld(sm: true, initialGame: "sm", smMapRando: true);
            var sm = world.SMWorld!;
            var mapRoom = sm.JsonData.RoomGeometries.Single(r => r.name == "Crateria Map Room");
            var mapDoor = mapRoom.doors.Single(d => d.exit_ptr.HasValue && d.entrance_ptr.HasValue);
            Assert.AreEqual("Crateria - Crateria Map Room - Left Door", sm.Start.Name,
                "map-rando logic must start in the same room as the ROM");

            var rom = new MemoryRom();
            Randomizer.Games.SuperMetroid.RomWriter.Write(rom, sm, new PRNG(42));

            // The initial SRAM template selects the new Crateria station and the generated
            // map area containing the map room.
            const int initialSram = 0x799000; // SA-1-mapped $F9:9000
            CollectionAssert.AreEqual(Words(0x10), rom.Read(initialSram + 0x166, 2), "station slot");
            CollectionAssert.AreEqual(Words(mapRoom.area), rom.Read(initialSram + 0x168, 2), "original area");
            int mapRoomIndex = sm.Map!.room_id.FindIndex(id => id == mapRoom.room_id);
            CollectionAssert.AreEqual(Words(sm.Map.room_area[mapRoomIndex]), rom.Read(initialSram + 0x96E, 2), "map area");

            // Slot $10 loads the map room through the door paired with it by the shuffled map.
            CollectionAssert.AreEqual(Words(
                0x8000 | (mapRoom.rom_address & 0x7FFF),
                Randomizer.Games.SuperMetroid.Portals.RemapEntranceDoor(sm, mapDoor.entrance_ptr!.Value) & 0xFFFF,
                0, 0, 0, 0x78, 0),
                rom.Read(0x44C5 + 0x10 * 14, 14));
        }
        finally
        {
            Randomizer.Games.SuperMetroid.Model.JsonReader.DataRoot = oldDataRoot;
        }
    }

    [TestMethod]
    public void Vanilla4Games_TablesMatchFrozenLayout()
    {
        // The frozen vanilla layout: the ALttP hub connections plus the direct
        // SM<->M1 (Bubble Mountain Save <-> bubble shaft), SM<->Z1 (Caterpillar Save <->
        // cave 0x0C) and M1<->Z1 (Kraid shaft <-> map 0x70 cave) portals. SM door pointers
        // are the C#-generated conversions in the reserved slots ($83B100 + slot*0x18;
        // in at +0, out at +12).
        var world = CreateWorld(alttp: true, sm: true, z1: true, m1: true);
        var rom = new MemoryRom();
        PortalWriter.WritePortals(rom, world);

        AssertTable(rom, 0x300000, Words(
            0xB10C, 1, 0x0200, 0x0000,
            0xB124, 1, 0x0201, 0x0000,
            0xB13C, 1, 0x0202, 0x0040,
            0xB154, 1, 0x0203, 0x0040,
            0xB184, 2, 0x000C, 0x0003,
            0xB16C, 3, 0x1515, 0x0061,
            0x0000), "sm");

        AssertTable(rom, 0x550000, Words(
            0x0122, 0x0035, 0, 0xB100, 0x0000,
            0x00E5, 0x0003, 0, 0xB118, 0x0000,
            0x010E, 0x0077, 0, 0xB130, 0x0000,
            0x0115, 0x0070, 0, 0xB148, 0x0000,
            0x0122, 0x0011, 2, 0x0066, 0x0003,
            0x011F, 0x0002, 3, 0x0B0C, 0x0040,
            0x0000), "alttp");

        AssertTable(rom, 0x63A000, Words(
            0x0066, 1, 0x0220, 0x0000,
            0x000C, 0, 0xB178, 0x0000,
            0x0070, 3, 0x0810, 0x0042,
            0x0000), "z1");

        AssertTable(rom, Randomizer.Games.Metroid.RomWriter.TransitionTableAddress, Words(
            0x0B0C, 0x0001, 1, 0x0210, 0x0000,
            0x1515, 0x0001, 0, 0xB160, 0x0000,
            0x0810, 0x0001, 2, 0x0070, 0x0003,
            0x0000), "m1");

        // Caterpillar Save Room is the SM<->Z1 portal and uses Brinstar's reserved
        // station slot 6. Its save PLM must carry that slot so a manual save reloads
        // into this room instead of vanilla station 1.
        CollectionAssert.AreEqual(new byte[] { 0x6F, 0xB7, 0x07, 0x0B, 0x06, 0x00, 0x00, 0x00 },
            rom.Read(0x7F058, 8));

        // Bubble Mountain Save Room is the SM<->M1 portal. Norfair's map/refill
        // portals use autosave-only slots $10/$11, leaving manual-save-compatible
        // slot 6 for this room. SM masks save PLM arguments to three bits, so using
        // $10 here would reload at Norfair station 0 (Post Crocomire).
        CollectionAssert.AreEqual(new byte[] { 0x6F, 0xB7, 0x07, 0x0B, 0x06, 0x00, 0x00, 0x00 },
            rom.Read(0x7F048, 8));

        // Save-room portal arrivals spawn two blocks lower than map/refill portal
        // arrivals, aligning Samus vertically with the save station.
        CollectionAssert.AreEqual(new byte[]
        {
            0xDD, 0xB0, 0x9A, 0x95, // room, vanilla entrance door
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x98, 0x00, 0x00, 0x00,
        }, rom.Read(0x46D9 + 6 * 14, 14));
    }

    [TestMethod]
    public void NoAlttp_SmZ1M1_TablesMatchFixedPairing()
    {
        var world = CreateWorld(sm: true, z1: true, m1: true);
        var rom = new MemoryRom();
        PortalWriter.WritePortals(rom, world);

        // Without ALttP the direct portals form a triangle, then two leftover ALttP
        // endpoints are reassigned to SM so those endpoints are not wasted.
        AssertTable(rom, 0x300000, Words(
            0xB124, 2, 0x000C, 0x0003,
            0xB13C, 2, 0x0066, 0x0003,
            0xB10C, 3, 0x1515, 0x0061,
            0xB154, 3, 0x0B0C, 0x0040,
            0x0000), "sm");

        AssertTable(rom, 0x63A000, Words(
            0x000C, 0, 0xB118, 0x0000,
            0x0066, 0, 0xB130, 0x0000,
            0x0070, 3, 0x0810, 0x0042,
            0x0000), "z1");

        AssertTable(rom, Randomizer.Games.Metroid.RomWriter.TransitionTableAddress, Words(
            0x1515, 0x0001, 0, 0xB100, 0x0000,
            0x0B0C, 0x0001, 0, 0xB148, 0x0000,
            0x0810, 0x0001, 2, 0x0070, 0x0003,
            0x0000), "m1");
    }

    [TestMethod]
    public void NoAlttp_Z1M1_ReallocatesUnusedEndpoints()
    {
        var world = CreateWorld(z1: true, m1: true);
        Assert.AreEqual(6, world.DerivePortalEdges().Count, "three bidirectional portals = six edges");

        var rom = new MemoryRom();
        PortalWriter.WritePortals(rom, world);

        AssertTable(rom, 0x63A000, Words(
            0x0070, 3, 0x0810, 0x0042,
            0x0066, 3, 0x0B0C, 0x0040,
            0x000C, 3, 0x1515, 0x0061,
            0x0000), "z1");

        AssertTable(rom, Randomizer.Games.Metroid.RomWriter.TransitionTableAddress, Words(
            0x0810, 0x0001, 2, 0x0070, 0x0003,
            0x0B0C, 0x0001, 2, 0x0066, 0x0003,
            0x1515, 0x0001, 2, 0x000C, 0x0003,
            0x0000), "m1");
    }

    [TestMethod]
    public void VanillaM1_NorfairPortalUsesAlternatePalette()
    {
        var world = CreateWorld(sm: true, m1: true);
        var norfairRoom = world.M1World!.PortalRooms.Single(room => room.Area == M1Area.Norfair);
        var norfairAnchor = world.M1World.PortalAnchors.Single(anchor => anchor.Name == norfairRoom.Name);

        Assert.AreEqual(0x20, norfairAnchor.DestinationArgs & 0x20,
            "vanilla Norfair portal arrivals need the alternate palette");
    }

    [TestMethod]
    public void MapShuffleM1_NorfairPortalUsesNormalPalette()
    {
        var world = CreateWorld(sm: true, m1: true, m1MapShuffle: true);
        var norfairRoom = world.M1World!.PortalRooms.Single(room => room.Area == M1Area.Norfair);
        var norfairAnchor = world.M1World.PortalAnchors.Single(anchor => anchor.Name == norfairRoom.Name);

        Assert.AreEqual(0, norfairAnchor.DestinationArgs & 0x20,
            "map-shuffle Norfair portal arrivals use the generated room palette");
    }

    [TestMethod]
    public void M1Portal_UsesZ1MoneyHintCaveInsteadOfDefaultShop()
    {
        var world = CreateWorld(z1: true, m1: true);
        var z1Anchor = world.DerivePortalEdges()
            .SelectMany(edge => new[] { edge.From, edge.To })
            .Distinct()
            .Single(anchor => anchor.GameId == "z1" && anchor.RowPrefix[0] == 0x70);

        Assert.AreEqual(0x70, z1Anchor.DestinationId,
            "the M1 portal should use map 0x70's money-hint cave");
        Assert.IsFalse(world.Z1World!.GetLocationsOfType(VertexType.Item)
            .Any(location => location.Name.StartsWith("Cave 1C - ")),
            "the money-hint cave must not consume an item location when shop shuffle is off");
    }

    [TestMethod]
    public void AnyPartnerPresent_EverySmAnchorPairedExactlyOnce()
    {
        // An SM portal door without a transition row falls through to a broken vanilla
        // door transition, so every SM anchor must be consumed whenever a partner exists.
        foreach (var (alttp, z1, m1) in new[]
                 { (true, true, true), (true, false, false), (false, true, true), (false, true, false), (false, false, true) })
        {
            var world = CreateWorld(alttp: alttp, sm: true, z1: z1, m1: m1);
            var smAnchors = world.SMWorld!.PortalAnchors;

            // The default layout links every SM room it converts.
            Assert.AreEqual(world.SMWorld.PortalRooms.Count, smAnchors.Count);
            Assert.IsTrue(smAnchors.Count > 0, $"alttp={alttp} z1={z1} m1={m1}: no SM portals");

            var portalEdges = world.DerivePortalEdges();
            foreach (var anchor in smAnchors)
            {
                Assert.AreEqual(1, portalEdges.Count(e => ReferenceEquals(e.From, anchor)),
                    $"alttp={alttp} z1={z1} m1={m1}: SM anchor {anchor.Name} outgoing edges");
                Assert.AreEqual(1, portalEdges.Count(e => ReferenceEquals(e.To, anchor)),
                    $"alttp={alttp} z1={z1} m1={m1}: SM anchor {anchor.Name} incoming edges");
            }
        }
    }

    // The four SM rooms the base patch pre-converts into portal rooms, with the vanilla
    // room-header bytes that must reappear when the room is left unlinked. Header PCs and
    // values come from the vanilla SM ROM (resources/sm.sfc).
    private static readonly (string RoomName, int HeaderPc, byte[] DoorList, byte[] LevelData, byte[] PlmList)[]
        BasePreConvertedRooms =
    [
        ("Crateria Map Room",             0x79994, [0xBB, 0x99], [0xBD, 0x86, 0xCE], [0x44, 0x84]),
        ("Norfair Map Room",              0x7B0B4, [0xDB, 0xB0], [0xC3, 0x83, 0xCE], [0xD8, 0x8D]),
        ("Maridia Missile Refill Room",   0x7D845, [0x6C, 0xD8], [0x31, 0xAB, 0xCE], [0x65, 0xC7]),
        ("Golden Torizo Energy Recharge", 0x7B305, [0x2C, 0xB3], [0xDC, 0x98, 0xCE], [0x90, 0x8E]),
    ];

    [TestMethod]
    public void UnlinkedBasePreConvertedRooms_RevertToVanillaHeaders()
    {
        // No ALttP: the four ALttP-paired base rooms are never re-converted or linked, so
        // their base-patch portal door would loop back into the room. They must revert.
        var world = CreateWorld(sm: true, m1: true);
        var connectedAnchorNames = world.DerivePortalEdges()
            .SelectMany(edge => new[] { edge.From, edge.To })
            .Where(anchor => anchor.GameId == "sm")
            .Select(anchor => anchor.Name)
            .ToHashSet();
        var linkedRoomNames = world.SMWorld!.PortalRooms
            .Where(room => connectedAnchorNames.Contains(room.Name))
            .Select(room => room.RoomName)
            .ToHashSet();

        var rom = new MemoryRom();
        PortalWriter.WritePortals(rom, world);

        int reverted = 0;
        foreach (var (roomName, headerPc, doorList, levelData, plmList) in BasePreConvertedRooms)
        {
            if (linkedRoomNames.Contains(roomName))
                continue;

            reverted++;
            CollectionAssert.AreEqual(doorList, rom.Read(headerPc + 9, doorList.Length),
                $"{roomName} door list not reverted");
            CollectionAssert.AreEqual(levelData, rom.Read(headerPc + 13, levelData.Length),
                $"{roomName} level data not reverted");
            CollectionAssert.AreEqual(plmList, rom.Read(headerPc + 33, plmList.Length),
                $"{roomName} PLM list not reverted");
        }

        Assert.IsTrue(reverted > 0, "expected at least one unlinked base room to revert");
    }

    [TestMethod]
    public void LinkedBasePreConvertedRooms_KeepTheirConversion()
    {
        // With ALttP present, all four base rooms are re-converted and linked, so none is
        // reverted: the room header keeps the C# conversion's repointed door list.
        var world = CreateWorld(alttp: true, sm: true);
        var rom = new MemoryRom();
        PortalWriter.WritePortals(rom, world);

        foreach (var (roomName, headerPc, doorList, _, _) in BasePreConvertedRooms)
        {
            CollectionAssert.AreNotEqual(doorList, rom.Read(headerPc + 9, doorList.Length),
                $"{roomName} was reverted but is linked");
        }
    }

    [TestMethod]
    public void AnchorsMaterializeExactlyForTheConnectedEdges()
    {
        foreach (var (alttp, sm, z1, m1) in new[]
                 { (true, true, true, true), (false, true, true, true), (false, true, false, true), (false, true, true, false), (false, false, true, true) })
        {
            var world = CreateWorld(alttp: alttp, sm: sm, z1: z1, m1: m1);
            var portalEdges = world.DerivePortalEdges();

            // M1's portal rooms are physical map content, always all built so the
            // layout can reassign endpoints when a partner game is missing.
            if (m1)
                Assert.AreEqual(Randomizer.Games.Metroid.DataLoader.PortalRoomAreas.Length,
                    world.M1World!.PortalRooms.Count, $"alttp={alttp} sm={sm} z1={z1}: m1 portal rooms");

            // Anchors exist only because an edge needed them: every materialized anchor
            // participates in at least one portal edge.
            var connected = portalEdges.SelectMany(e => new[] { e.From, e.To }).ToHashSet();
            IEnumerable<PortalAnchor> all =
            [
                .. world.SMWorld?.PortalAnchors ?? [],
                .. world.AlttpWorld?.PortalAnchors ?? [],
                .. world.Z1World?.PortalAnchors ?? [],
                .. world.M1World?.PortalAnchors ?? [],
            ];
            foreach (var anchor in all)
                Assert.IsTrue(connected.Contains(anchor),
                    $"alttp={alttp} sm={sm} z1={z1} m1={m1}: anchor '{anchor.Name}' materialized without an edge");

            foreach (var edge in portalEdges)
                Assert.AreNotEqual(edge.From.GameId, edge.To.GameId, "portal edge does not cross games");
        }
    }

    [TestMethod]
    public void DefaultLayout_ConnectsPresentGamePairsWithoutReusingEndpoints()
    {
        foreach (var (alttp, sm, z1, m1) in new[]
                 {
                     (true, true, true, true),
                     (true, true, true, false),
                     (true, true, false, true),
                     (true, false, true, true),
                     (false, true, true, true),
                     (true, true, false, false),
                     (true, false, true, false),
                     (true, false, false, true),
                     (false, true, true, false),
                     (false, true, false, true),
                     (false, false, true, true),
                 })
        {
            var world = CreateWorld(alttp: alttp, sm: sm, z1: z1, m1: m1);
            var portalEdges = world.DerivePortalEdges();
            var presentGames = PresentGames(alttp, sm, z1, m1);

            foreach (var expected in GamePairs(presentGames))
                Assert.IsTrue(portalEdges.Any(edge => SameGamePair(edge, expected)),
                    $"alttp={alttp} sm={sm} z1={z1} m1={m1}: missing {expected.A}<->{expected.B} portal");

            var anchors = new List<PortalAnchor>();
            foreach (var anchor in portalEdges.SelectMany(edge => new[] { edge.From, edge.To }))
            {
                if (!anchors.Any(existing => ReferenceEquals(existing, anchor)))
                    anchors.Add(anchor);
            }

            foreach (var anchor in anchors)
            {
                Assert.AreEqual(1, portalEdges.Count(edge => ReferenceEquals(edge.From, anchor)),
                    $"alttp={alttp} sm={sm} z1={z1} m1={m1}: endpoint '{anchor.Name}' outgoing use count");
                Assert.AreEqual(1, portalEdges.Count(edge => ReferenceEquals(edge.To, anchor)),
                    $"alttp={alttp} sm={sm} z1={z1} m1={m1}: endpoint '{anchor.Name}' incoming use count");
            }
        }
    }

    [TestMethod]
    public void PortalEdges_AreWiredBothWays()
    {
        var world = CreateWorld(alttp: true, sm: true, z1: true, m1: true);

        foreach (var edge in world.DerivePortalEdges())
        {
            var exit = WorldFor(world, edge.From).GetLocation(edge.From.ExitVertexName);
            var entry = WorldFor(world, edge.To).GetLocation(edge.To.EntryVertexName);
            Assert.IsTrue(exit.Edges.Any(e => e.To == entry),
                $"missing portal edge {exit.Name} -> {entry.Name}");
        }
    }

    [DataTestMethod]
    [DataRow(Z1ShopShuffleOption.Full)]
    [DataRow(Z1ShopShuffleOption.Junk)]
    public void Z1PortalCaves_DoNotExposeShopLocations_WithShopShuffle(Z1ShopShuffleOption shop)
    {
        var world = CreateWorld(alttp: true, z1: true, z1Shop: shop);
        var z1 = world.Z1World!;

        foreach (var (map, caveNode) in new[]
                 { (0x66, "Open cave"), (0x0C, "Open cave"), (0x70, "Open cave") })
        {
            var portalCave = z1.GetLocation($"Overworld - Map {map:X2} - {caveNode}");
            Assert.IsTrue(z1.PortalAnchors.Any(a => a.EntryVertexName == portalCave.Name),
                $"map {map:X2} should be a materialized portal anchor");
            Assert.IsFalse(portalCave.Edges.Any(e => e.To.World == z1 && e.To.Name.StartsWith("Cave ")),
                $"map {map:X2} portal cave should not also enter a shuffled Z1 cave");
        }

        // Map 0x70's cave 0x1C is a money-hint cave the M1 portal reserves, so it must not
        // create a shop item location (unlike cave 0x1E, which the portal no longer uses:
        // maps 0x66/0x0C reserve their copies, but map 0x5E is free to be a normal shop).
        Assert.IsFalse(z1.GetLocationsOfType(VertexType.Item).Any(v => v.Name.StartsWith("Cave 1C - Shop - ")),
            "Cave 1C (map 0x70) is reserved for the default M1 portal and must not create shop item locations.");

        Assert.IsFalse(z1.PortalAnchors.Any(anchor => anchor.RowPrefix[0] == 0x5E || anchor.RowPrefix[0] == 0x10),
            "the former M1 portal caves (0x5E, 0x10) should be available for normal Z1 behavior");
    }

    private static string[] PresentGames(bool alttp, bool sm, bool z1, bool m1)
    {
        var games = new List<string>();
        if (sm)
            games.Add("sm");
        if (alttp)
            games.Add("alttp");
        if (z1)
            games.Add("z1");
        if (m1)
            games.Add("m1");
        return games.ToArray();
    }

    private static IEnumerable<(string A, string B)> GamePairs(IReadOnlyList<string> games)
    {
        for (int i = 0; i < games.Count; i++)
        {
            for (int j = i + 1; j < games.Count; j++)
                yield return (games[i], games[j]);
        }
    }

    private static bool SameGamePair(PortalEdge edge, (string A, string B) pair) =>
        edge.From.GameId == pair.A && edge.To.GameId == pair.B
        || edge.From.GameId == pair.B && edge.To.GameId == pair.A;


    [TestMethod]
    public void EffectiveInitialGame_UsesFirstPresentGameWhenConfiguredGameIsAbsent()
    {
        Assert.AreEqual("sm", CreateWorld(sm: true).EffectiveInitialGame);
        Assert.AreEqual("z1", CreateWorld(z1: true).EffectiveInitialGame);
    }

    [TestMethod]
    public void EffectiveInitialGame_RejectsUnknownGame()
    {
        var world = CreateWorld(sm: true, initialGame: "bogus");
        var exception = Assert.ThrowsException<ArgumentException>(() => _ = world.EffectiveInitialGame);
        StringAssert.Contains(exception.Message, "Invalid initial game");
    }

    [TestMethod]
    public void Alttp_CreateEntranceAnchor_DerivesEverythingFromTheGraph()
    {
        var world = CreateWorld(alttp: true);
        var alttp = world.AlttpWorld!;

        // Any entrance works; Lumberjacks Cave is not one of the vanilla six.
        var anchor = Randomizer.Games.Alttp.Portals.CreateEntranceAnchor(alttp, "Lumberjacks Cave");

        Assert.AreEqual(0x0002u, anchor.RowPrefix[1], "overworld area (map 0x02)");
        Assert.AreEqual(0x0230, anchor.DestinationId, "first dynamic RoomToOutlet row");
        Assert.AreEqual(0x0000, anchor.DestinationArgs, "light world entrance");
        Assert.AreEqual("Lumberjacks Cave - Out", anchor.EntryVertexName);
        Assert.AreEqual("Lumberjacks Cave - In", anchor.ExitVertexName);

        // Arrival spawn: one byte pointing the RoomToOutlet row at the entrance's outlet.
        var patch = anchor.RomPatches.Single();
        Assert.AreEqual(0x586B00 + 0x0230, patch.Address);
        CollectionAssert.AreEqual(new byte[] { 0x13 }, patch.Data, "Lumberjacks Cave outlet id");

        // A second anchor allocates the next row.
        var next = Randomizer.Games.Alttp.Portals.CreateEntranceAnchor(alttp, "High Stakes Chest Game");
        Assert.AreEqual(0x0231, next.DestinationId);
    }

    [TestMethod]
    public void Alttp_VanillaAnchors_MatchTheHistoricalRows()
    {
        // The vanilla pairing only names entrance vertices; the anchors materialize from
        // the cross-game edges and must reproduce the constants the tables froze on.
        var world = CreateWorld(alttp: true, sm: true, z1: true, m1: true);
        var anchors = world.AlttpWorld!.PortalAnchors;

        (string Name, uint Room, uint Area, int Dest, int Args)[] expected =
        [
            ("Lake Hylia Fortune Teller", 0x0122, 0x0035, 0x0200, 0x0000),
            ("Old Man Home Circle", 0x00E5, 0x0003, 0x0201, 0x0000),
            ("Hint Giver Cave", 0x010E, 0x0077, 0x0202, 0x0040),
            ("Mire Big Fairy", 0x0115, 0x0070, 0x0203, 0x0040),
            ("Lumberjacks House", 0x011F, 0x0002, 0x0210, 0x0000),
            ("Kakariko Fortune Teller", 0x0122, 0x0011, 0x0220, 0x0000),
        ];

        Assert.AreEqual(expected.Length, anchors.Count);
        foreach (var (name, room, area, dest, args) in expected)
        {
            var anchor = anchors.Single(a => a.Name == name);
            Assert.AreEqual(room, anchor.RowPrefix[0], $"{name} room");
            Assert.AreEqual(area, anchor.RowPrefix[1], $"{name} overworld area");
            Assert.AreEqual(dest, anchor.DestinationId, $"{name} destination");
            Assert.AreEqual(args, anchor.DestinationArgs, $"{name} args");
        }
    }

    [TestMethod]
    public void Sm_ConvertRoom_BuildsRoomConversionPatches()
    {
        var world = CreateWorld(sm: true);
        var sm = world.SMWorld!;

        // Creation is separate from linking: converting registers the portal room and
        // its patches, but writes nothing and creates no anchor by itself.
        var portalRoom = Randomizer.Games.SuperMetroid.Portals.ConvertRoom(sm, "Brinstar Map Room");
        Assert.AreEqual(0, sm.PortalAnchors.Count, "conversion must not create an anchor");

        // First conversion (an SM-only world links no default portals): door data slot
        // at $83B100 (in), out at +12. The room kind (map station) is inferred from the
        // room's utility nodes.
        var anchor = world.ResolveAnchor(sm.GetLocation(portalRoom.VertexName));
        Assert.AreEqual(0xB10Cu, anchor.RowPrefix[0], "portal out door pointer");
        Assert.AreEqual(0xB100, anchor.DestinationId, "portal in door pointer");

        // Door data: room header $9C35 (rom_address 0x79C35), original door on the right.
        var doorData = anchor.RomPatches.Single(p => p.Address == 0x1B100);
        CollectionAssert.AreEqual(new byte[]
        {
            0x35, 0x9C, 0x40, 0x04, 0x01, 0x06, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, // in
            0x35, 0x9C, 0x40, 0x05, 0x01, 0x06, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, // out
        }, doorData.Data);

        // Door list (right room: portal first) + map station PLM in the bank $8F slot.
        var lists = anchor.RomPatches.Single(p => p.Address == 0x7F000);
        CollectionAssert.AreEqual(new byte[]
        {
            0x0C, 0xB1, 0x72, 0x8D, 0x00, 0x00, 0x00, 0x00, // doors: portal out, original ($8D72), end
            0xD3, 0xB6, 0x08, 0x0A, 0x00, 0x80, 0x00, 0x00, // map station PLM
        }, lists.Data);

        // Room header repoints: door list, map room level data, PLM list.
        CollectionAssert.AreEqual(new byte[] { 0x00, 0xF0 }, anchor.RomPatches.Single(p => p.Address == 0x79C35 + 9).Data);
        CollectionAssert.AreEqual(new byte[] { 0x00, 0xE0, 0xDE }, anchor.RomPatches.Single(p => p.Address == 0x79C35 + 13).Data);
        CollectionAssert.AreEqual(new byte[] { 0x08, 0xF0 }, anchor.RomPatches.Single(p => p.Address == 0x79C35 + 33).Data);

        // A map room only needs its arrival autosave entry, so it preserves the
        // manual-save-compatible slots 6/7 by using autosave-only slot $10.
        var station = anchor.RomPatches.Single(p => p.Address == 0x45CF + 0x10 * 14);
        CollectionAssert.AreEqual(new byte[]
        {
            0x35, 0x9C, 0x36, 0x8D, // room, vanilla entrance door
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x78, 0x00, 0x00, 0x00,
        }, station.Data);

        // The portal attaches to the room's real door vertex; there is no synthetic
        // portal vertex â€” the portal door is engine plumbing.
        Assert.AreEqual("Brinstar - Brinstar Map Room - Right Door", anchor.EntryVertexName);
        Assert.AreEqual(anchor.EntryVertexName, anchor.ExitVertexName);
    }

    [TestMethod]
    public void NewPortal_ConnectedPair_WritesRowsAndPatches()
    {
        // The full dynamic flow: create the portal room first (a separate step), then
        // connect two VERTICES. The anchors materialize from the edge, and all rows and
        // patches derive from the graph when writing.
        var world = CreateWorld(alttp: true, sm: true);
        Randomizer.Games.SuperMetroid.Portals.ConvertRoom(world.SMWorld!, "Brinstar Map Room");

        world.ConnectPortal(
            world.SMWorld!.GetLocation("Brinstar - Brinstar Map Room - Right Door"),
            world.AlttpWorld!.GetLocation("Lumberjacks Cave - In"));

        var smAnchor = world.SMWorld!.PortalAnchors.Single(a => a.Name == "Brinstar Map Room Portal");
        var alttpAnchor = world.AlttpWorld!.PortalAnchors.Single(a => a.Name == "Lumberjacks Cave");
        Assert.AreEqual(0xB190, smAnchor.DestinationId, "linked through the portal room's portal door");
        Assert.AreEqual(0x0230, alttpAnchor.DestinationId, "materialized with the first dynamic row");

        var rom = new MemoryRom();
        PortalWriter.WritePortals(rom, world);

        // The new rows follow the six default ALttP<->SM rows in both tables.
        AssertTable(rom, 0x300000 + 6 * 8, Words(
            (int)smAnchor.RowPrefix[0], 1, 0x0230, 0x0000,
            0x0000), "sm (new row)");
        AssertTable(rom, 0x550000 + 6 * 10, Words(
            (int)alttpAnchor.RowPrefix[0], 0x0002, 0, smAnchor.DestinationId, 0x0000,
            0x0000), "alttp (new row)");

        // Both anchors' patches were written: the arrival spawn row and the room conversion.
        Assert.AreEqual(0x13, rom.Read(0x586B00 + 0x0230, 1)[0], "RoomToOutlet row");
        Assert.AreEqual(0x35, rom.Read(0x10000 + smAnchor.DestinationId, 1)[0], "SM door data");
    }

    [TestMethod]
    public void OnExitAnchor_RowGoesToTheOutTable()
    {
        var world = CreateWorld(alttp: true, sm: true);
        var portalRoom = Randomizer.Games.SuperMetroid.Portals.ConvertRoom(world.SMWorld!, "Brinstar Map Room");
        var smAnchor = world.ResolveAnchor(world.SMWorld!.GetLocation(portalRoom.VertexName));
        var alttpAnchor = Randomizer.Games.Alttp.Portals.CreateEntranceAnchor(
            world.AlttpWorld!, "Lumberjacks Cave", trigger: PortalTrigger.OnExit);

        world.ConnectPortal(smAnchor, alttpAnchor);

        var rom = new MemoryRom();
        PortalWriter.WritePortals(rom, world);

        // Keyed on the interior room alone; fires when leaving the cave.
        AssertTable(rom, 0x542000, Words(
            (int)alttpAnchor.RowPrefix[0], 0x0000, 0, smAnchor.DestinationId, 0x0000,
            0x0000), "alttp out-table");

        // And it does not appear in the in-table (vanilla rows + terminator only).
        AssertTable(rom, 0x550000 + 6 * 10, Words(0x0000), "alttp in-table terminator");
    }

    [TestMethod]
    public void Sm_PlainDoor_BecomesAPortalConsumingItsPassage()
    {
        // No portal room at all: any plain door can be linked. Its own door data becomes
        // the transition key, and the doorway's outgoing passage is consumed.
        var world = CreateWorld(alttp: true, sm: true);
        var sm = world.SMWorld!;
        var door = sm.GetLocation("Crateria - Crateria Save Room - Right Door");

        // The physical passage: the save room's right door leads into the Parlor.
        var passage = door.Edges.First(e => e.To.Name.StartsWith("Crateria - Parlor"));

        world.ConnectPortal(door, world.AlttpWorld!.GetLocation("Lumberjacks Cave - In"));

        // Row key and arrival come from the door's own geometry data.
        var geometry = sm.JsonData.RoomGeometries.Find(r => r.name == "Crateria Save Room")!;
        var geoDoor = geometry.doors.Single(d => d.exit_ptr.HasValue);
        var anchor = sm.PortalAnchors.Single(a => a.Name == door.Name);
        Assert.AreEqual((uint)(0x8000 | (geoDoor.exit_ptr!.Value & 0x7FFF)), anchor.RowPrefix[0]);
        Assert.AreEqual(0x8000 | (geoDoor.entrance_ptr!.Value & 0x7FFF), anchor.DestinationId);
        Assert.AreEqual(0, anchor.RomPatches.Count, "a plain door needs no ROM changes of its own");

        // The outgoing passage is severed (the way back in remains on the other side).
        Assert.IsFalse(door.Edges.Contains(passage), "consumed passage must leave the graph");
        Assert.IsTrue(passage.To.Edges.Any(e => e.To == door), "the reverse direction must survive");
    }

    [TestMethod]
    public void M1_PlainDoor_BecomesAPortalConsumingItsPassage()
    {
        // Any M1 left/right door can be linked without a portal room. This consumes
        // the selected doorway's passage and derives the row from the door itself.
        var world = CreateWorld(z1: true, m1: true);
        var m1 = world.M1World!;
        var door = m1.GetLocation("Norfair - Right Bubble Tunnel Shaft - Left Door Bomb Tunnel (0) - Left door");
        var passage = door.Edges.First(e =>
            e.To.World == door.World && !e.To.Name.StartsWith("Norfair - Right Bubble Tunnel Shaft - "));

        world.ConnectPortal(world.Z1World!.GetLocation("Overworld - Map 0C - Open cave"), door);

        var anchor = m1.PortalAnchors.Single(a => a.Name == door.Name);
        Assert.AreEqual(0x1D12u, anchor.RowPrefix[0], "cell of the door screen");
        Assert.AreEqual(0x0002u, anchor.RowPrefix[1], "left-hand door");
        Assert.AreEqual(0x1D12, anchor.DestinationId, "arrivals walk back into the same cell");
        Assert.AreEqual(0x00C1, anchor.DestinationArgs,
            "left-door arrival ($80) into a vertical ($40) Norfair (1) room");

        // The outgoing passage is severed (the way back in remains on the other side).
        Assert.IsFalse(door.Edges.Contains(passage), "consumed passage must leave the graph");
        Assert.IsTrue(passage.To.Edges.Any(e => e.To == door), "the reverse direction must survive");
    }

    [TestMethod]
    public void CrossGameEdge_WithoutPortalAnchors_Throws()
    {
        var world = CreateWorld(alttp: true, sm: true);

        // A cross-game edge between arbitrary vertices cannot become a portal.
        world.Graph.AddDirected(world.AlttpWorld!.GetLocation("start"),
            world.SMWorld!.Start, world.AlttpWorld.GetItem("fixed"));

        var exception = Assert.ThrowsException<Exception>(() => world.DerivePortalEdges());
        StringAssert.Contains(exception.Message, "cannot host a portal");
    }
}
