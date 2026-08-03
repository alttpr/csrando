namespace Randomizer.Games.SuperMetroid;

using Randomizer.Games;
using Randomizer.RomModifications;
using BaseVertex = Randomizer.Graph.Vertex;

/// <summary>
/// SM cross-game portal support, in two decoupled halves:
///
/// 1. Portal room creation (<see cref="ConvertRoom"/>): gives a single-door
///    map/save/refill room a second (portal) door so linking a portal there does not
///    consume the room's real doorway. All conversions — including the vanilla layout's
///    four rooms — are built from C# as ROM patches in the reserved slots
///    ($83B100-$83B2FF); the equivalent newrooms.asm conversions in the base patches
///    are simply left unused (the room header repoints replace theirs).
///
/// 2. Portal resolution (<see cref="ResolveVertexAnchor"/>): links a door vertex into a
///    portal. Converted rooms use their dedicated portal doors. Any other door works
///    too, using its own door data — leaving through it warps cross-game and arriving
///    walks in through its doorway — at the cost of consuming that doorway's outgoing
///    passage (the corresponding graph edge is severed so logic stays truthful).
/// </summary>
public static class Portals
{
    public enum RoomKind { Map, Save, Refill }

    // Reserved slots for dynamically converted rooms (kept free by the base patches).
    private const int DynamicDoorDataBase = 0x83B100;  // 24 bytes per room, until $83B2FF
    private const int DynamicDoorDataStride = 0x18;
    private const int DynamicListBase = 0x8FF000;      // door list + PLM list, until $8FF1FF
    private const int DynamicListStride = 0x10;
    private const int MaxDynamicRooms = 0x200 / DynamicDoorDataStride; // bank $83 slots run out first

    // Save station tables per area (PC, $80:C4C5+). Manual save PLMs mask their station
    // argument to three bits, so save rooms must use 6 or 7. Map/refill portal rooms only
    // need an arrival autosave entry and preferentially use 0x10-0x11, preserving 6/7.
    // Slots 8-0xF belong to the map rando's own stations.
    private static readonly int[] SaveStationAreaOffsets = [0x44C5, 0x45CF, 0x46D9, 0x481B, 0x4917, 0x4A2F];
    private static readonly int[] ManualSaveSlots = [6, 7];
    private static readonly int[] AutosaveOnlySlots = [0x10, 0x11];

    /// <summary>
    /// Rooms the base patch (newrooms.asm) already converts into double-door portal rooms,
    /// with the vanilla room-header fields those macros overwrite (door list at +9, level
    /// data at +13, PLM list at +33). These are the four rooms the default layout pairs
    /// with ALttP: when ALttP is absent the layout never re-converts or links them, but the
    /// base ROM still carries the extra portal door, whose door data warps back into the
    /// room itself — a dead-end self-loop. <see cref="RevertUnusedBaseConversions"/> writes
    /// these vanilla values back for any such room left unlinked, restoring a plain room.
    /// </summary>
    private sealed record BaseConversion(string RoomName, int HeaderPc,
        byte[] DoorList, byte[] LevelData, byte[] PlmList);

    private static readonly BaseConversion[] BaseConversions =
    [
        new("Crateria Map Room",           0x79994, [0xBB, 0x99], [0xBD, 0x86, 0xCE], [0x44, 0x84]),
        new("Norfair Map Room",            0x7B0B4, [0xDB, 0xB0], [0xC3, 0x83, 0xCE], [0xD8, 0x8D]),
        new("Maridia Missile Refill Room", 0x7D845, [0x6C, 0xD8], [0x31, 0xAB, 0xCE], [0x65, 0xC7]),
        new("Golden Torizo Energy Recharge", 0x7B305, [0x2C, 0xB3], [0xDC, 0x98, 0xCE], [0x90, 0x8E]),
    ];

    /// <summary>
    /// Resolves a door vertex into its portal anchor: a linked one, a converted portal
    /// room's dedicated portal door, or — for any other door — the door's own door data
    /// (which consumes the doorway's outgoing passage).
    /// </summary>
    public static PortalAnchor ResolveVertexAnchor(World world, BaseVertex vertex)
    {
        if (world.FindPortalAnchor(vertex) is { } existing)
            return existing;

        PortalAnchor anchor;
        var portalRoom = world.PortalRooms.Find(r => r.VertexName == vertex.Name);
        if (portalRoom != null)
        {
            anchor = PortalAnchor.Sm(portalRoom.Name, portalRoom.DoorOutPointer, portalRoom.DoorInPointer,
                0x0000, entryVertexName: vertex.Name, exitVertexName: vertex.Name)
                with
            { RomPatches = portalRoom.Patches };
        }
        else
        {
            anchor = ResolvePlainDoor(world, vertex);
        }

        world.PortalAnchors.Add(anchor);
        return anchor;
    }

    /// <summary>
    /// Restores the vanilla room header of every base-pre-converted room (see
    /// <see cref="BaseConversions"/>) the layout left unlinked, so its unused portal door
    /// reverts to a plain room instead of a self-looping dead end. A room counts as used
    /// when a portal room with that name was converted and linked — i.e. its name appears
    /// in <paramref name="linkedRoomNames"/>. Runs after the transition tables and anchor
    /// patches so a linked room's own conversion always wins.
    /// </summary>
    public static void RevertUnusedBaseConversions(IRom rom, ISet<string> linkedRoomNames)
    {
        foreach (var conversion in BaseConversions)
        {
            if (linkedRoomNames.Contains(conversion.RoomName))
                continue;

            rom.Write(conversion.HeaderPc + 9, conversion.DoorList);
            rom.Write(conversion.HeaderPc + 13, conversion.LevelData);
            rom.Write(conversion.HeaderPc + 33, conversion.PlmList);
        }
    }

    /// <summary>A portal on a plain door: leaving through it warps cross-game (its own
    /// door data is the transition key) and arrivals walk in through its doorway (the
    /// neighboring room's entrance door data). The doorway's outgoing passage is
    /// consumed, so the corresponding graph edge is severed.</summary>
    private static PortalAnchor ResolvePlainDoor(World world, BaseVertex vertex)
    {
        var (room, doorNode) = FindRoomByDoorVertex(world, vertex.Name)
            ?? throw new Exception($"SM vertex '{vertex.Name}' is not a door and cannot host a portal");

        var geometry = world.JsonData.RoomGeometries.Find(r => r.name == room.Name)
            ?? throw new Exception($"SM room '{room.Name}' has no geometry data");
        int nodeAddress = Convert.ToInt32(doorNode.NodeAddress, 16);
        var door = geometry.doors.FirstOrDefault(d => d.exit_ptr == nodeAddress)
            ?? throw new Exception($"SM door '{vertex.Name}' has no door data and cannot host a portal");
        if (door.subtype == "elevator")
            throw new Exception($"SM door '{vertex.Name}' is an elevator and cannot host a portal");
        if (door.entrance_ptr == null)
            throw new Exception($"SM door '{vertex.Name}' has no entrance door data and cannot host a portal");

        // The outgoing passage now leads cross-game; drop the edge to the neighboring
        // room's door (the way back in stays — the neighbor's own door is untouched).
        vertex.Edges.RemoveAll(e =>
            e.To.World == vertex.World && FindRoomByDoorVertex(world, e.To.Name)?.Room is { } other
            && !ReferenceEquals(other, room));

        return PortalAnchor.Sm(vertex.Name,
            (uint)(0x8000 | (door.exit_ptr!.Value & 0x7FFF)),
            0x8000 | (door.entrance_ptr.Value & 0x7FFF),
            0x0000,
            entryVertexName: vertex.Name, exitVertexName: vertex.Name);
    }

    /// <summary>
    /// Converts a single-door map/save/refill room into a portal room and registers it on
    /// the world. Creation is separate from linking: the conversion's ROM patches are only
    /// written when the room is later linked into a portal, so unused conversions cost
    /// nothing. The room kind (map/save/refill station) is inferred from its utility node.
    /// </summary>
    public static World.PortalRoom ConvertRoom(World world, string roomName)
    {
        if (world.PortalRooms.Any(r => r.RoomName == roomName))
            throw new Exception($"SM room '{roomName}' is already converted");

        var room = world.JsonData.Rooms.Find(r => r.Name == roomName)
            ?? throw new Exception($"SM room '{roomName}' not found");
        var geometry = world.JsonData.RoomGeometries.Find(r => r.name == roomName)
            ?? throw new Exception($"SM room '{roomName}' has no geometry data");
        var kind = InferKind(room);

        var doors = geometry.doors.Where(d => d.exit_ptr.HasValue && d.entrance_ptr.HasValue).ToList();
        if (doors.Count != 1)
            throw new Exception($"SM room '{roomName}' must have exactly one real door, found {doors.Count}");
        var door = doors[0];
        bool doorOnLeft = door.direction switch
        {
            "left" => true,
            "right" => false,
            _ => throw new Exception($"SM room '{roomName}' door faces '{door.direction}'; only left/right rooms can host a portal room"),
        };

        int index = world.PortalRooms.Count;
        if (index >= MaxDynamicRooms)
            throw new Exception($"No reserved door data slots left for SM portal rooms (max {MaxDynamicRooms})");

        int roomHeaderPc = geometry.rom_address;
        ushort roomHeader16 = (ushort)(0x8000 | (roomHeaderPc & 0x7FFF));
        ushort origDoorOut16 = (ushort)(0x8000 | (door.exit_ptr!.Value & 0x7FFF));
        ushort vanillaEntrance16 = (ushort)(0x8000 | (door.entrance_ptr!.Value & 0x7FFF));

        int doorDataLong = DynamicDoorDataBase + index * DynamicDoorDataStride;
        ushort portalIn16 = (ushort)(doorDataLong & 0xFFFF);
        ushort portalOut16 = (ushort)(portalIn16 + 12);
        int listLong = DynamicListBase + index * DynamicListStride;
        ushort doorList16 = (ushort)(listLong & 0xFFFF);
        ushort plmList16 = (ushort)(doorList16 + 8);

        var usedSaveStationSlots = world.PortalRooms
            .Where(r => world.JsonData.RoomGeometries.Find(g => g.name == r.RoomName)?.area == geometry.area)
            .Select(r => r.SaveStationSlot)
            .ToHashSet();
        var candidateSlots = kind == RoomKind.Save
            ? ManualSaveSlots
            : AutosaveOnlySlots.Concat(ManualSaveSlots);
        int saveStationSlot = candidateSlots.FirstOrDefault(slot => !usedSaveStationSlots.Contains(slot), -1);
        if (saveStationSlot < 0)
            throw new Exception($"No compatible portal save station slots left in area {geometry.area} for {kind} room '{roomName}'");

        // Door list order: the portal door comes first on a left-facing room, and save rooms
        // flip that (matching the newrooms.asm macros), so the two flips cancel for a
        // left-facing save room and for a right-facing non-save room.
        bool portalDoorFirst = (kind == RoomKind.Save) == doorOnLeft;

        var patches = new List<RomPatch>
        {
            // Portal in/out door data (12 bytes each): room header, door properties,
            // then dw $8000, $0000 (no door ASM).
            new(SnesToPc(doorDataLong),
            [
                .. DoorBlob(roomHeader16, doorOnLeft ? [0x40, 0x05, 0x0E, 0x06, 0x00, 0x00] : [0x40, 0x04, 0x01, 0x06, 0x00, 0x00]),
                .. DoorBlob(roomHeader16, doorOnLeft ? [0x40, 0x04, 0x0E, 0x06, 0x00, 0x00] : [0x40, 0x05, 0x01, 0x06, 0x00, 0x00]),
            ]),
            // Door list + PLM list.
            new(SnesToPc(listLong),
            [
                .. Words(portalDoorFirst
                    ? [portalOut16, origDoorOut16, 0x0000]
                    : [origDoorOut16, portalOut16, 0x0000]),
                0x00, 0x00, // slot padding
                .. PlmList(kind, (ushort)saveStationSlot),
            ]),
            // Room header: repoint the door list, level data and PLM list.
            new(roomHeaderPc + 9, Words([doorList16])),
            new(roomHeaderPc + 13, LevelData(kind)),
            new(roomHeaderPc + 33, Words([plmList16])),
        };

        // Save station entry so arriving autosaves (update_save_station scans slots 6-0x11
        // of the arrival room's area for the room). Under the map randomizer the room's
        // real door leads elsewhere, so the station's spawn door is the remapped one.
        int stationAddress = SaveStationAreaOffsets[geometry.area] + saveStationSlot * 14;
        ushort stationEntrance16 = world.Map == null
            ? vanillaEntrance16
            : (ushort)(RemapEntranceDoor(world, door.entrance_ptr!.Value) & 0xFFFF);
        ushort stationYOffset = kind == RoomKind.Save ? (ushort)0x0098 : (ushort)0x0078;
        patches.Add(new(stationAddress,
        [
            .. Words([roomHeader16, stationEntrance16]),
            .. Words([0x0000, 0x0000, 0x0000, stationYOffset, 0x0000]),
        ]));

        var doorNode = room.Nodes.FirstOrDefault(n => n.NodeType == "door")
            ?? throw new Exception($"SM room '{roomName}' has no door node");
        string doorVertexName = $"{room.Area} - {room.Name} - {doorNode.Name}";

        var portalRoom = new World.PortalRoom($"{roomName} Portal", roomName, doorVertexName,
            portalOut16, portalIn16, !doorOnLeft, saveStationSlot, patches);
        world.PortalRooms.Add(portalRoom);
        return portalRoom;
    }

    /// <summary>
    /// The door data entering a room in the map randomizer's shuffled layout: the room's
    /// own exit door (the vanilla counterpart of <paramref name="vanillaEntrancePtr"/>)
    /// is looked up in the shuffled connections, whose partner exit is the door data that
    /// now leads in. Mirrors the map rando's save station rewiring.
    /// </summary>
    internal static int RemapEntranceDoor(World world, int vanillaEntrancePtr)
    {
        var map = world.Map!;
        var vanillaCounterpart = new Dictionary<int, int>();
        var shuffledPartner = new Dictionary<int, int>();
        for (int i = 0; i < map.conn_from_door_id.Count; i++)
        {
            var fromGeo = world.JsonData.RoomGeometries.Find(r => r.room_id == map.conn_from_room_id[i]);
            var toGeo = world.JsonData.RoomGeometries.Find(r => r.room_id == map.conn_to_room_id[i]);
            if (fromGeo == null || toGeo == null)
                continue;

            var fromDoor = fromGeo.doors[map.conn_from_door_id[i]];
            var toDoor = toGeo.doors[map.conn_to_door_id[i]];

            if (fromDoor.exit_ptr.HasValue && fromDoor.entrance_ptr.HasValue)
                vanillaCounterpart[fromDoor.exit_ptr.Value] = fromDoor.entrance_ptr.Value;
            if (toDoor.exit_ptr.HasValue && toDoor.entrance_ptr.HasValue)
                vanillaCounterpart[toDoor.exit_ptr.Value] = toDoor.entrance_ptr.Value;
            if (fromDoor.exit_ptr.HasValue && toDoor.exit_ptr.HasValue)
            {
                shuffledPartner[fromDoor.exit_ptr.Value] = toDoor.exit_ptr.Value;
                shuffledPartner[toDoor.exit_ptr.Value] = fromDoor.exit_ptr.Value;
            }
        }

        return shuffledPartner[vanillaCounterpart[vanillaEntrancePtr]];
    }

    /// <summary>The room and door node a "{Area} - {Room} - {Door}" vertex belongs to.</summary>
    private static (Model.Room Room, Model.Node Door)? FindRoomByDoorVertex(World world, string vertexName)
    {
        foreach (var room in world.JsonData.Rooms)
        {
            string prefix = $"{room.Area} - {room.Name} - ";
            if (!vertexName.StartsWith(prefix))
                continue;
            var node = room.Nodes.FirstOrDefault(n =>
                n.NodeType == "door" && vertexName == prefix + n.Name);
            if (node != null)
                return (room, node);
        }

        return null;
    }

    private static RoomKind InferKind(Model.Room room)
    {
        var utilityKinds = room.Nodes
            .Where(n => n.NodeType == "utility")
            .Select(n => n.NodeSubType)
            .ToHashSet();

        if (utilityKinds.Contains("map"))
            return RoomKind.Map;
        if (utilityKinds.Contains("save"))
            return RoomKind.Save;
        if (utilityKinds.Contains("energy") || utilityKinds.Contains("missile"))
            return RoomKind.Refill;

        throw new Exception($"SM room '{room.Name}' is not a map/save/refill room and cannot become a portal room");
    }

    private static byte[] DoorBlob(ushort roomHeader16, byte[] props) =>
        [.. Words([roomHeader16]), .. props, .. Words([0x8000, 0x0000])];

    private static byte[] PlmList(RoomKind kind, ushort saveStationSlot) => kind switch
    {
        RoomKind.Map => Words([0xB6D3, 0x0A08, 0x8000, 0x0000]),
        RoomKind.Refill => Words([0xB6DF, 0x0A07, 0x0048, 0x0000]),
        // The save PLM's argument is the area's load-station index. Manual saves use
        // this value directly, so it must match the reserved station entry above.
        RoomKind.Save => Words([0xB76F, 0x0B07, saveStationSlot, 0x0000]),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    private static byte[] LevelData(RoomKind kind) => kind switch
    {
        RoomKind.Map => [0x00, 0xE0, 0xDE],
        RoomKind.Refill => [0xA6, 0x8F, 0xCE],
        RoomKind.Save => [0xF6, 0x9E, 0xCE],
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    private static byte[] Words(ushort[] words) =>
        words.SelectMany(w => new[] { (byte)(w & 0xFF), (byte)(w >> 8) }).ToArray();

    // SM sits at the start of the combo ROM with plain LoROM mapping.
    private static int SnesToPc(int address) => Randomizer.RomModifications.SNES.ToPC(address);
}
