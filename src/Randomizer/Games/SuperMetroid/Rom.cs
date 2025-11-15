namespace Randomizer.Games.SuperMetroid;

using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;
using Randomizer.RomModifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Rom : GameRom
{
    private int _plmTableOffset;
    private FreeSpaceManager _fsm;

    private const int MAP_X_OFFSET = 4;
    private const int MAP_Y_OFFSET = 4;


    public Rom(IRom rom, int offset) : base(rom, offset)
    {
        _plmTableOffset = 0xf800;
        _fsm = new FreeSpaceManager();
    }

    public void WriteItems(World world)
    {
        foreach (var location in world.GetLocationsOfType(VertexType.Item).Where(l => l.Item != null))
        {
            if (location.Item!.Bytes == null)
                continue;

            if (location.Addresses == null)
                continue;

            if (location.Item!.Bytes.Length < 2)
            {
                Console.WriteLine($"Item {location.Item.Name} does not have enough bytes");
                continue;
            }

            int plmBytes = (int)location.Item!.Bytes[0] + ((int)location.Item!.Bytes[1] << 8);
            int offset = location.Node!.NodeSubType switch
            {
                "chozo" => plmBytes >= 0xEFE0 ? 0x04 : 0x54,
                "hidden" => plmBytes >= 0xEFE0 ? 0x08 : 0xA8,
                _ => 0
            };

            plmBytes += offset;
            Write((Address)location.Addresses[0], [(byte)(plmBytes & 0xFF), (byte)((plmBytes >> 8) & 0xFF)]);

            if (plmBytes >= 0xEFE0)
            {
                Write((Address)(location.Addresses[0] + 5), [location.Item!.Bytes[2]]);
            }
        }
    }

    public void WriteEventFlags(World world)
    {
        ushort flags = 0x0001; // Zebes awake
        if (world.Map != null)
        {
            flags |= 0x07C0; // Pre-open G4 fully
        }

        Write(0x3F0202, BitConverter.GetBytes(flags));
    }

    public void WriteBossesNeeded(World world)
    {
        Write(0x3F0200, [byte.Parse(world.Config.Bosses)]);
    }

    public void WriteKeycardFlag(World world)
    {
        if (world.Config.Keycards == Keycards.All)
        {
            Write(0x3F0206, [0x01]);
        }
    }

    public void WriteSpawnAllItems(World world)
    {
        Write((SNES)0x8F9EC5, [0xE6, 0x86]);
        Write((SNES)0x8F9AB6, [0x86, 0x84]);
        Write((SNES)0x8FCDCE, [0x57, 0xC3]);
        Write((SNES)0x8FCE17, [0x5F, 0xC3]);
        Write((SNES)0x8FCC4D, [0x37, 0xC3]);
        Write((SNES)0x8FCAD4, [0x19, 0xC3]);
        Write((SNES)0x8FC9B4, [0xD1, 0xC2]);
        Write((SNES)0x8FCE66, [0x6D, 0xC3]);
    }

    public void WritePlms(World world)
    {
        ushort plaquePlm = 0xd410;
        int plmTablePos = _plmTableOffset;

        if (world.Config.Keycards == Keycards.All)
        {
            var doorList = new List<ushort[]> {
                                // RoomId  Door Facing                yyxx  Keycard Event Type                   Plaque type               yyxx, Address (if 0 a dynamic PLM is created)
                    // Crateria
                    new ushort[] { 0x91F8, KeycardDoors.Right,      0x2601, KeycardEvents.CrateriaLevel1,        KeycardPlaque.Level1,   0x2400, 0x0000 },  // Crateria - Landing Site - Door to gauntlet
                    new ushort[] { 0x91F8, KeycardDoors.Left,       0x168E, KeycardEvents.CrateriaLevel1,        KeycardPlaque.Level1,   0x148F, 0x801E },  // Crateria - Landing Site - Door to landing site PB
                    new ushort[] { 0x948C, KeycardDoors.Left,       0x062E, KeycardEvents.CrateriaLevel2,        KeycardPlaque.Level2,   0x042F, 0x8222 },  // Crateria - Before Moat - Door to moat (overwrite PB door)
                    new ushort[] { 0x99BD, KeycardDoors.Left,       0x660E, KeycardEvents.CrateriaBoss,          KeycardPlaque.Boss,     0x640F, 0x8470 },  // Crateria - Before G4 - Door to G4
                    new ushort[] { 0x9879, KeycardDoors.Left,       0x062E, KeycardEvents.CrateriaBoss,          KeycardPlaque.Boss,     0x042F, 0x8420 },  // Crateria - Before BT - Door to Bomb Torizo

                    // Brinstar
                    new ushort[] { 0x9F11, KeycardDoors.Left,       0x060E, KeycardEvents.BrinstarLevel1,        KeycardPlaque.Level1,   0x040F, 0x8784 },  // Brinstar - Blue Brinstar - Door to ceiling e-tank room
                    new ushort[] { 0x9AD9, KeycardDoors.Right,      0xA601, KeycardEvents.BrinstarLevel2,        KeycardPlaque.Level2,   0xA400, 0x0000 },  // Brinstar - Green Brinstar - Door to etecoon area
                    new ushort[] { 0x9D9C, KeycardDoors.Down,       0x0336, KeycardEvents.BrinstarBoss,          KeycardPlaque.Boss,     0x0234, 0x863A },  // Brinstar - Pink Brinstar - Door to spore spawn
                    new ushort[] { 0xA130, KeycardDoors.Left,       0x161E, KeycardEvents.BrinstarLevel2,        KeycardPlaque.Level2,   0x141F, 0x881C },  // Brinstar - Pink Brinstar - Door to wave gate e-tank
                    new ushort[] { 0xA0A4, KeycardDoors.Left,       0x062E, KeycardEvents.BrinstarLevel2,        KeycardPlaque.Level2,   0x042F, 0x0000 },  // Brinstar - Pink Brinstar - Door to spore spawn super
                    new ushort[] { 0xA56B, KeycardDoors.Left,       0x161E, KeycardEvents.BrinstarBoss,          KeycardPlaque.Boss,     0x141F, 0x8A1A },  // Brinstar - Before Kraid - Door to Kraid
                    // Upper Norfair
                    new ushort[] { 0xA7DE, KeycardDoors.Right,      0x3601, KeycardEvents.NorfairLevel1,         KeycardPlaque.Level1,   0x3400, 0x8B00 },  // Norfair - Business Centre - Door towards Ice
                    new ushort[] { 0xA923, KeycardDoors.Right,      0x0601, KeycardEvents.NorfairLevel1,         KeycardPlaque.Level1,   0x0400, 0x0000 },  // Norfair - Pre-Crocomire - Door towards Ice
                    new ushort[] { 0xA788, KeycardDoors.Left,       0x162E, KeycardEvents.NorfairLevel2,         KeycardPlaque.Level2,   0x142F, 0x8AEA },  // Norfair - Lava Missile Room - Door towards Bubble Mountain
                    new ushort[] { 0xAF72, KeycardDoors.Left,       0x061E, KeycardEvents.NorfairLevel2,         KeycardPlaque.Level2,   0x041F, 0x0000 },  // Norfair - After frog speedway - Door to Bubble Mountain
                    new ushort[] { 0xAEDF, KeycardDoors.Down,       0x0206, KeycardEvents.NorfairLevel2,         KeycardPlaque.Level2,   0x0204, 0x0000 },  // Norfair - Below bubble mountain - Door to Bubble Mountain
                    new ushort[] { 0xAD5E, KeycardDoors.Right,      0x0601, KeycardEvents.NorfairLevel2,         KeycardPlaque.Level2,   0x0400, 0x0000 },  // Norfair - LN Escape - Door to Bubble Mountain

                    new ushort[] { 0xA923, KeycardDoors.Up,         0x2DC6, KeycardEvents.NorfairBoss,           KeycardPlaque.Boss,     0x2EC4, 0x8B96 },  // Norfair - Pre-Crocomire - Door to Crocomire
                    // Lower Norfair
                    new ushort[] { 0xB4AD, KeycardDoors.Left,       0x160E, KeycardEvents.LowerNorfairLevel1,    KeycardPlaque.Level1,   0x140F, 0x0000 },  // Lower Norfair - WRITG - Door to Amphitheatre
                    new ushort[] { 0xAD5E, KeycardDoors.Left,       0x065E, KeycardEvents.LowerNorfairLevel1,    KeycardPlaque.Level1,   0x045F, 0x0000 },  // Lower Norfair - Exit - Door to "Reverse LN Entry"
                    new ushort[] { 0xB37A, KeycardDoors.Right,      0x0601, KeycardEvents.LowerNorfairBoss,      KeycardPlaque.Boss,     0x0400, 0x8EA6 },  // Lower Norfair - Pre-Ridley - Door to Ridley
                    // Maridia
                    new ushort[] { 0xD0B9, KeycardDoors.Left,       0x065E, KeycardEvents.MaridiaLevel1,         KeycardPlaque.Level1,   0x045F, 0x0000 },  // Maridia - Mt. Everest - Door to Pink Maridia
                    new ushort[] { 0xD5A7, KeycardDoors.Right,      0x1601, KeycardEvents.MaridiaLevel1,         KeycardPlaque.Level1,   0x1400, 0x0000 },  // Maridia - Aqueduct - Door towards Beach
                    new ushort[] { 0xD617, KeycardDoors.Left,       0x063E, KeycardEvents.MaridiaLevel2,         KeycardPlaque.Level2,   0x043F, 0x0000 },  // Maridia - Pre-Botwoon - Door to Botwoon
                    new ushort[] { 0xD913, KeycardDoors.Right,      0x2601, KeycardEvents.MaridiaLevel2,         KeycardPlaque.Level2,   0x2400, 0x0000 },  // Maridia - Pre-Colloseum - Door to post-botwoon
                    new ushort[] { 0xD78F, KeycardDoors.Right,      0x2601, KeycardEvents.MaridiaBoss,           KeycardPlaque.Boss,     0x2400, 0xC73B },  // Maridia - Precious Room - Door to Draygon
                    new ushort[] { 0xDA2B, KeycardDoors.BossLeft,   0x164E, 0x00f0, /* Door id 0xf0 */           KeycardPlaque.None,     0x144F, 0x0000 },  // Maridia - Change Cac Alley Door to Boss Door (prevents key breaking)
                    // Wrecked Ship
                    new ushort[] { 0x93FE, KeycardDoors.Left,       0x167E, KeycardEvents.WreckedShipLevel1,     KeycardPlaque.Level1,   0x147F, 0x0000 },  // Wrecked Ship - Outside Wrecked Ship West - Door to Reserve Tank Check
                    new ushort[] { 0x968F, KeycardDoors.Left,       0x060E, KeycardEvents.WreckedShipLevel1,     KeycardPlaque.Level1,   0x040F, 0x0000 },  // Wrecked Ship - Outside Wrecked Ship West - Door to Bowling Alley
                    new ushort[] { 0xCE40, KeycardDoors.Left,       0x060E, KeycardEvents.WreckedShipLevel1,     KeycardPlaque.Level1,   0x040F, 0x0000 },  // Wrecked Ship - Gravity Suit - Door to Bowling Alley
                    new ushort[] { 0xCC6F, KeycardDoors.Left,       0x064E, KeycardEvents.WreckedShipBoss,       KeycardPlaque.Boss,     0x044F, 0xC29D },  // Wrecked Ship - Pre-Phantoon - Door to Phantoon

                };
            ushort doorId = 0x0000;
            foreach (var door in doorList)
            {
                /* When "Fast Ganon" is set, don't place the G4 Boss key door to enable faster games */
                // TODO: Add a SM specific option for this
                if (door[0] == 0x99BD && world.Config.FastG4)
                {
                    continue;
                }
                var doorArgs = door[4] != KeycardPlaque.None ? doorId | door[3] : door[3];
                if (door[6] == 0)
                {
                    // Write dynamic door
                    var doorData = door[0..3].SelectMany(x => UshortBytes(x)).Concat(UshortBytes(doorArgs)).ToArray();
                    //patches.Add((Snes(0x8f0000 + plmTablePos), doorData));
                    Write((SNES)(0x8f0000 + plmTablePos), doorData);
                    plmTablePos += 0x08;
                }
                else
                {
                    // Overwrite existing door
                    var doorData = door[1..3].SelectMany(x => UshortBytes(x)).Concat(UshortBytes(doorArgs)).ToArray();
                    Write((SNES)(0x8f0000 + door[6]), doorData);
                    if ((door[3] == KeycardEvents.BrinstarBoss && door[0] != 0x9D9C) || door[3] == KeycardEvents.LowerNorfairBoss || door[3] == KeycardEvents.MaridiaBoss || door[3] == KeycardEvents.WreckedShipBoss)
                    {
                        // Overwrite the extra parts of the Gadora with a PLM that just deletes itself
                        Write((SNES)(0x8f0000 + door[6] + 0x06), new byte[] { 0x2F, 0xB6, 0x00, 0x00, 0x00, 0x00, 0x2F, 0xB6, 0x00, 0x00, 0x00, 0x00 });
                    }
                }
                // Plaque data
                if (door[4] != KeycardPlaque.None)
                {
                    var plaqueData = UshortBytes(door[0]).Concat(UshortBytes(plaquePlm)).Concat(UshortBytes(door[5])).Concat(UshortBytes(door[4])).ToArray();
                    Write((SNES)(0x8f0000 + plmTablePos), plaqueData);
                    plmTablePos += 0x08;
                }
                doorId += 1;
            }
        }

        /* Write plaque showing SM bosses that needs to be killed */
        int bosses = int.Parse(world.Config.Bosses);
        if (bosses != 4)
        {
            var plaqueData = UshortBytes(0xA5ED).Concat(UshortBytes(plaquePlm)).Concat(UshortBytes(0x044F))
                .Concat(UshortBytes(KeycardPlaque.Zero + bosses)).ToArray();
            Write((SNES)(0x8f0000 + plmTablePos), plaqueData);
            plmTablePos += 0x08;
        }


        Write((SNES)(0x8f0000 + plmTablePos), new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
        _plmTableOffset = plmTablePos;
    }

    public void WriteMap(World world)
    {
        var doorData = DoorReader.ReadDoorData();

        // Writes the door connections of a map randomizer map
        if (world.Map != null)
        {
            for (int i = 0; i < world.Map.conn_from_door_id.Count; i++)
            {
                var from_room_id = world.Map.conn_from_room_id[i];
                var to_room_id = world.Map.conn_to_room_id[i];

                var from_room_idx = world.Map.room_id.FindIndex(r => r == from_room_id);
                var to_room_idx = world.Map.room_id.FindIndex(r => r == to_room_id);

                var from_area = world.Map.room_area[from_room_idx];
                var to_area = world.Map.room_area[to_room_idx];

                var from_door_id = world.Map.conn_from_door_id[i];
                var to_door_id = world.Map.conn_to_door_id[i];

                var fromRoom = world.JsonData.Rooms.Find(r => r.Id == from_room_id);
                var toRoom = world.JsonData.Rooms.Find(r => r.Id == to_room_id);

                var fromGeo = world.JsonData.RoomGeometries.Find(r => r.room_id == from_room_id)!;
                var toGeo = world.JsonData.RoomGeometries.Find(r => r.room_id == to_room_id)!;

                var fromGeoDoor = fromGeo.doors[from_door_id];
                var toGeoDoor = toGeo.doors[to_door_id];

                var bidirectional = world.Map.conn_bidirectional[i];

                ushort fromRoomId = (ushort)(int.Parse(fromRoom?.RoomAddress?.Substring(2) ?? "0", System.Globalization.NumberStyles.HexNumber) & 0xFFFF);
                ushort toRoomId = (ushort)(int.Parse(toRoom?.RoomAddress?.Substring(2) ?? "0", System.Globalization.NumberStyles.HexNumber) & 0xFFFF);

                // Debug write
                Console.WriteLine($"Door {fromRoom?.Name} ({fromRoomId:X4}) -> {toRoom?.Name} ({toRoomId:X4}), bidir: {bidirectional}, ptrs: {fromGeoDoor.exit_ptr:X4}->{fromGeoDoor.entrance_ptr:X4} to {toGeoDoor.exit_ptr:X4}->{toGeoDoor.entrance_ptr:X4}");

                var originalDoorData = doorData.Where(d => d.ptr == toGeoDoor.entrance_ptr).FirstOrDefault();
                if (originalDoorData == null)
                {
                    Console.WriteLine($"Failed to find door data for door ptr {toGeoDoor.entrance_ptr:X4} ({fromRoom?.Name} -> {toRoom?.Name})");
                    continue;
                }

                if (from_area != to_area)
                {
                    originalDoorData.elevator |= 0x40;
                }
                else
                {
                    originalDoorData.elevator = (byte)(originalDoorData.elevator & ~0x40);
                }

                Console.WriteLine($" Writing door ptr {fromGeoDoor.exit_ptr:X4} to point to room {toRoomId:X4} at door ptr {toGeoDoor.entrance_ptr:X4}, original data: {originalDoorData}");
                Write((Address)fromGeoDoor.exit_ptr!, DoorReader.GetDoorBytes(originalDoorData));

                if (fromGeoDoor.exit_ptr! == 0x1A798)
                {
                    // Pants Room right door
                    // Also write the same data to the East Pants Room right door
                    Write((Address)0x1A7BC, DoorReader.GetDoorBytes(originalDoorData));
                }

                if (bidirectional == true)
                {
                    if (fromGeoDoor.entrance_ptr == 0)
                        continue;

                    originalDoorData = doorData.Where(d => d.ptr == fromGeoDoor.entrance_ptr).First();
                    if (from_area != to_area)
                    {
                        originalDoorData.elevator |= 0x40;
                    }
                    else
                    {
                        originalDoorData.elevator = (byte)(originalDoorData.elevator & ~0x40);
                    }
                    Console.WriteLine($" Writing reverse door ptr {toGeoDoor.exit_ptr:X4} to point to room {fromRoomId:X4} at door ptr {fromGeoDoor.entrance_ptr:X4}, original data: {originalDoorData}");
                    Write((Address)toGeoDoor.exit_ptr!, DoorReader.GetDoorBytes(originalDoorData));
                }
            }

            (int, int)[] saveStationPtrs = new (int, int)[]
            {
                (0x44C5, 0x896A), (0x44D3, 0x899A), (0x44E1, 0x0000), (0x45CF, 0x8DF6), (0x45DD, 0x8D12), (0x45EB, 0x8F52), (0x45F9, 0x9186), (0x4607, 0x90D2),
                (0x46D9, 0x9456), (0x46E7, 0x959A), (0x46F5, 0x97DA), (0x4703, 0x93BA), (0x4711, 0x9702), (0x471F, 0x9A0E), (0x481B, 0xA240), (0x4917, 0xA354),
                (0x4925, 0xA588), (0x4933, 0xA744), (0x4941, 0xA7EC), (0x4A2F, 0xAABC), (0x4A3D, 0xA99C)
            };

            // Create dictionaries to map original and new door pointers.
            var origDoorMap = new Dictionary<int, int>();
            var newDoorMap = new Dictionary<int, int>();

            // Loop over each door in the world's map.
            //foreach (var door in world.Map.doors)
            for (int i = 0; i < world.Map.conn_from_door_id.Count; i++)
            {
                var from_room_id = world.Map.conn_from_room_id[i];
                var to_room_id = world.Map.conn_to_room_id[i];
                var from_door_id = world.Map.conn_from_door_id[i];
                var to_door_id = world.Map.conn_to_door_id[i];

                var fromGeoRoom = world.JsonData.RoomGeometries.Find(r => r.room_id == from_room_id)!;
                var toGeoRoom = world.JsonData.RoomGeometries.Find(r => r.room_id == to_room_id)!;

                var fromGeoDoor = fromGeoRoom.doors[from_door_id];
                var toGeoDoor = toGeoRoom.doors[to_door_id];

                int? srcExitPtr = fromGeoDoor.exit_ptr;
                int? srcEntrancePtr = fromGeoDoor.entrance_ptr;
                int? dstExitPtr = toGeoDoor.exit_ptr;
                int? dstEntrancePtr = toGeoDoor.entrance_ptr;

                // Populate origDoorMap for source door if both pointers exist.
                if (srcExitPtr.HasValue && srcEntrancePtr.HasValue)
                {
                    origDoorMap[srcExitPtr.Value] = srcEntrancePtr.Value;
                }
                // Populate origDoorMap for destination door if both pointers exist.
                if (dstExitPtr.HasValue && dstEntrancePtr.HasValue)
                {
                    origDoorMap[dstExitPtr.Value] = dstEntrancePtr.Value;
                }
                // Populate newDoorMap for bidirectional mapping if both exit pointers exist.
                if (srcExitPtr.HasValue && dstExitPtr.HasValue)
                {
                    newDoorMap[srcExitPtr.Value] = dstExitPtr.Value;
                    newDoorMap[dstExitPtr.Value] = srcExitPtr.Value;
                }
            }

            // For each save station pointer, update the door connection.
            foreach ((int ptr, int orig) in saveStationPtrs)
            {
                if (orig == 0)
                {
                    continue;
                }

                ushort readValue = (ushort)orig;

                // Calculate the full original entrance door pointer.
                int origEntranceDoorPtr = readValue + 0x10000;

                // Lookup the corresponding exit pointer and then the new entrance pointer.
                int exitDoorPtr = origDoorMap[origEntranceDoorPtr];
                int entranceDoorPtr = newDoorMap[exitDoorPtr];

                // Write the lower 16 bits of the new entrance pointer back to ROM.
                Write((Address)((ptr + 2)), BitConverter.GetBytes((ushort)(entranceDoorPtr & 0xFFFF)));
            }

            // Write new save stations for the cross-game transition rooms (stations 6/7/10/11 per area)
            // TODO: Hardcoded, fix later
            (int, int, int)[] transitionRooms = [
                (0, 0x9994, 0x8BDA), // Crateria Map Room
                //(1, 0x8C35, 0x8D36), // Brinstar Map Room (no portal yet)
                (2, 0xB0B4, 0x9306), // Norfair Map Room
                (2, 0xB305, 0x9A7A), // Lower norfair energy station
                (4, 0xD845, 0xA8F4), // Maridia Missile Station
            ];

            // Group by area (first value) and write new stations
            var roomsByArea = transitionRooms.GroupBy(t => t.Item1);
            int[] areaOffsets = [0x44C5, 0x45CF, 0x46D9, 0x481B, 0x4917, 0xCA2F];
            foreach (var areaRooms in roomsByArea)
            {
                int saveIndex = 6;
                foreach (var room in areaRooms)
                {
                    int area = room.Item1;
                    int roomPtr = room.Item2;
                    int entrancePtr = room.Item3;
                    int ptr = areaOffsets[area] + (saveIndex * 14);
                    saveIndex++;
                    if (saveIndex == 8) { saveIndex = 0x10; }

                    // Calculate the full original entrance door pointer.
                    int origEntranceDoorPtr = entrancePtr + 0x10000;
                    // Lookup the corresponding exit pointer and then the new entrance pointer.
                    int exitDoorPtr = origDoorMap[origEntranceDoorPtr];
                    int entranceDoorPtr = newDoorMap[exitDoorPtr];

                    Write((Address)ptr, BitConverter.GetBytes((ushort)(roomPtr & 0xFFFF)));
                    Write((Address)(ptr + 2), BitConverter.GetBytes((ushort)(entranceDoorPtr & 0xFFFF)));
                    Write((Address)(ptr + 4), [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x78, 0x00, 0x00, 0x00]);
                    Console.WriteLine($"Writing new save station at area {area} ptr {ptr:X4} to point to room {roomPtr:X4} at door ptr {entranceDoorPtr:X4}");
                }
            }

            // Write extra door asm for toilet and boss rooms
            int asmPtr = WriteMapDoorAsm(world);
            asmPtr = ClampSamusPosition(world, asmPtr);
            WriteMiniMapData(world);
            WriteDoorCaps(world);

            WriteEscapeRoomModifications(world);
            WriteMiscMapPatches(world);
            WriteMiniMapPalettes(world);
            WriteMusicOverrides(world);
            WriteBuffedDrops(world);
            WriteQuickBigBoy(world);

            // Converts the tourian save station into a map station
            WriteTourianMapStation();
        }
    }

    private void WriteMiniMapPalettes(World world)
    {
        // Write new palettes for the minimap for use with map randomization
        Write((SNES)0xB6F000 + 0x42, [0xCC, 0x5D]); // Light blue
        Write((SNES)0xB6F000 + 0x82, [0xE5, 0x44]); // Dark blue
        Write((SNES)0xB6F000 + 0xA2, [0xDE, 0x3A]); // Light orange
        Write((SNES)0xB6F000 + 0xE2, [0x55, 0x52]); // Light pink
    }

    private void WriteBuffedDrops(World world)
    {
        /*
         * ; Adjust drop rates of respawning enemies and Kagos:
        ; - Double PB drop rates of respawning enemies (Gamet, Zeb, Geega, Zebbo, Zoa, Covern)
        ; - Double Super drop rate of Geega, Zeb, and Kagos.
        ; - At the expense of nothing, small energy, and missiles drop rates
        ; - Shift some drop rate into large energy to compensate for loss of some small
        ;                  __________________________ ; 0: Small health
        ;                 |     _____________________ ; 1: Big health
        ;                 |    |     ________________ ; 2: Missiles
        ;                 |    |    |     ___________ ; 3: Nothing
        ;                 |    |    |    |     ______ ; 4: Super missiles
        ;                 |    |    |    |    |     _ ; 5: Power bombs
        ;                 |    |    |    |    |    |
        org $B4F25A : db $3C, $3C, $32, $05, $3C, $14  ; Gamet (enemy $F213)
        org $B4F248 : db $14, $41, $1E, $00, $78, $14  ; Zeb (enemy $F193)
        org $B4F24E : db $14, $41, $1E, $00, $78, $14  ; Geega (enemy $F253)
        org $B4F254 : db $00, $8C, $05, $00, $64, $0A  ; Zebbo (enemy $F1D3)
        org $B4F260 : db $00, $64, $3C, $05, $46, $14  ; Zoa (enemy $DA7F)
        org $B4F266 : db $32, $5F, $32, $00, $14, $28  ; Covern (enemy $E77F)
        org $B4F26C : db $23, $5F, $3C, $05, $28, $14  ; Kago (enemy $E7FF)

        ; Make Power Bomb drop give 2 Power Bombs:
        org $86F0D9
            LDA #$0002
        */

        Write((SNES)0xB4F25A, [0x3C, 0x3C, 0x32, 0x05, 0x3C, 0x14]);
        Write((SNES)0xB4F248, [0x14, 0x41, 0x1E, 0x00, 0x78, 0x14]);
        Write((SNES)0xB4F24E, [0x14, 0x41, 0x1E, 0x00, 0x78, 0x14]);
        Write((SNES)0xB4F254, [0x00, 0x8C, 0x05, 0x00, 0x64, 0x0A]);
        Write((SNES)0xB4F260, [0x00, 0x64, 0x3C, 0x05, 0x46, 0x14]);
        Write((SNES)0xB4F266, [0x32, 0x5F, 0x32, 0x00, 0x14, 0x28]);
        Write((SNES)0xB4F26C, [0x23, 0x5F, 0x3C, 0x05, 0x28, 0x14]);

        Write((SNES)0x86F0D9, [0xA9, 0x02]);
    }

    private void WriteQuickBigBoy(World world)
    {
        /*
        ;;;
        ; Shorten Big Boy cutscene:
        ;;;

        ; Delay before Big Boy attacks Dead Sidehopper
        org $A9F031
            LDA #$002C          ; replaces: LDA #$01D0

        ; Hop initial velocities (make the first hop bigger, to effective skip a hop)
        org $A9D951
            dw $FE00, $FE00, $FC00, $FE00
            dw $0120, $0250, $0300, $01C0

        ; Delay between Dead Sidehopper hops
        org $A9D916
            LDA #$0001          ; replaces: LDA #$0040

        ; X position that Big Boy rushes toward
        org $A9F049
            LDA #$01B0          ; replaces: LDA #$0248

        ; Big Boy realizing what he did
        org $A9F2A8
            LDA #$0020           ; replaces: LDA #$0078

        ; Big Boy rising from Samus
        org $A9F2BA
            LDA #$0030           ; replaces: LDA #$00C0

        ; Big Boy backing off
        org $A9F2E6
            LDA #$0016          ; replaces: LDA #$0058

        ; Big Boy going left guiltily
        org $A9F31E
            LDA #$0016          ; replaces: LDA #$0058

        ; Big Boy going right guiltily
        org $A9F34A
            LDA #$0016          ; replaces: LDA #$0100
        */

        Write((SNES)0xA9F031, [0x2C]);
        Write((SNES)0xA9D951, [0x00, 0xFE, 0x00, 0xFE, 0x00, 0xFC, 0x00, 0xFE, 0x20, 0x01, 0x50, 0x02, 0x00, 0x03, 0xC0, 0x01]);
        Write((SNES)0xA9D916, [0xA9, 0x01, 0x00]);
        Write((SNES)0xA9F049, [0xA9, 0xB0, 0x01]);
        Write((SNES)0xA9F2A8, [0xA9, 0x20, 0x00]);
        Write((SNES)0xA9F2BA, [0xA9, 0x30, 0x00]);
        Write((SNES)0xA9F2E6, [0xA9, 0x16, 0x00]);
        Write((SNES)0xA9F31E, [0xA9, 0x16, 0x00]);
        Write((SNES)0xA9F34A, [0xA9, 0x16, 0x00]);

    }

    private void WriteMusicOverrides(World world)
    {
        // Write Tourian 1 song into MB room
        Write((SNES)0x8FDD72, [0x1E, 0x05]);
        Write((SNES)0x8FDD8C, [0x1E, 0x05]);

        // Write Super Metrroid song 2 into big boy room
        Write((SNES)0x8FDCC7, [0x45, 0x06]);
        Write((SNES)0x8FDCE1, [0x45, 0x06]);
    }

    public void WriteMiscPatches(World world)
    {
        // In Shaktool room, skip setting screens to red scroll (so that it won't glitch out when entering from the right):
        Write((SNES)0x84B8DC, [0x60]); // RTS
    }

    private void WriteMiscMapPatches(World world)
    {
        // In Kraid's room, no longer restrict Samus X position to left screen:
        Write((SNES)0xA7C9EE, [0x60]); // RTS

        // Remove fake gray door that gets drawn in Phantoon's Room:
        Write((SNES)0xA7D4E5, [0xEA, 0xEA, 0xEA, 0xEA, 0xEA, 0xEA, 0xEA, 0xEA]);

        // In Crocomire's initialization, skip setting the leftmost screens to red scroll. Even in the vanilla game there
        // is no purpose to this, as they are already red. But it important to skip here in the rando, because when entering
        // from the left door with Crocomire still alive, these scrolls are set to blue by the door ASM, and if they
        // were overridden with red it would break the graphics.
        Write((SNES)0xA48A92, [0xEA, 0xEA, 0xEA, 0xEA]); // NOP:NOP:NOP:NOP

        // Release Spore Spawn camera so it won't be glitched when entering from the right:
        Write((SNES)0xA5EADA, [0xEA, 0xEA, 0xEA]); // NOP:NOP:NOP

        // Likewise release Kraid camera so it won't be as glitched when entering from the right:
        Write((SNES)0xA7A9F4, [0xEA, 0xEA, 0xEA, 0xEA]); // NOP:NOP:NOP:NOP

        // Adjust the door cap location for the Green Brinstar Main Shaft door to itself left-to-right:
        // In vanilla it spawns a screen to the left of where it "should". We keep it wrong, to retain
        // the behavior of the door appearing immediately closed, but move the spawn location to be in an
        // out-of-the-way off-camera location, in the top-right of the room.
        Write((SNES)0x838CF2, [0x21, 0x06]);

        // Remove the room ASM pointer from tourian elevator room so the map isn't auto-revealed
        Write((SNES)0x8FDAD3, [0x00, 0x00]);

        // Disable the code that writes the standard elevator destination area markers
        Write((SNES)0x82BB30, [0x6B]); // RTL
    }


    // Modifies tourian escape rooms to be more map-rando friendly
    private void WriteEscapeRoomModifications(World world)
    {
        Write((SNES)0x8FC91F, [0x60]); // Disable Escape Room 1 Setup ASM
        Write((SNES)0x8FC933, [0x60]); // Disable Escape Room 2 Setup ASM
        Write((SNES)0x8FC946, [0x60]); // Disable Escape Room 3 Setup ASM
        Write((SNES)0x8FC953, [0x60]); // Disable Escape Room 4 Setup ASM

        Write((SNES)0x8FE5A0, [0x60]); // Disable Escape Room 1 & 3 Main ASM
        Write((SNES)0x8FE57C, [0x60]); // Disable Escape Room 2 Main ASM
        Write((SNES)0x8FE5A4, [0x60]); // Disable Escape Room 4 Main ASM

        // Remove the wall from the right side of escape room 1
        Write((SNES)0x8FC881, [0x00, 0x00]);
    }

    private int ClampSamusPosition(World world, int asmPtr)
    {
        var sandEntrances = new (int? FromDoorPtr, int? ToDoorPtr, int MinX, int MaxX)[]
        {
            // East Sand Hole
            (0x1A6C0, 0x1A6FC, 0x0065, 0x009B),
            // West Sand Hole
            (0x1A6A8, 0x1A6E4, 0x0165, 0x019B),
            // West Sand Hall
            (0x1A654, 0x1A6B4, 0x0265, 0x029B),
            // East Sand Hall
            (0x1A69C, 0x1A6CC, 0x0165, 0x019B),
            // Plasma Beach Quicksand Room
            (null,   0x1A624, 0x0045, 0x00BB),
            // Butterfly Room
            (null,   0x1A8A0, 0x0065, 0x009B),
            // Botwoon Quicksand Room (left)
            (null,   0x1A864, 0x0085, 0x00DB),
            // Botwoon Quicksand Room (right)
            (null,   0x1A858, 0x0125, 0x019B),
            // Below Botwoon Energy Tank (left)
            (null,   0x1A8AC, 0x0265, 0x02BB),
            // Below Botwoon Energy Tank (right)
            (null,   0x1A8B8, 0x0345, 0x03BB),
        };

        foreach (var (fromPtr, toPtr, minPos, maxPos) in sandEntrances)
        {
            var asm = new List<byte>
            {
                // LDA #minPos
                0xA9, (byte)(minPos & 0xFF), (byte)((minPos >> 8) & 0xFF),
                // CMP $0AF6
                0xCD, 0xF6, 0x0A,
                // BCC .no_clamp_min
                0x90, 0x06,
                // STA $0AF6
                0x8D, 0xF6, 0x0A,
                // STA $0B10
                0x8D, 0x10, 0x0B,

                // LDA #maxPos
                0xA9, (byte)(maxPos & 0xFF), (byte)((maxPos >> 8) & 0xFF),
                // CMP $0AF6
                0xCD, 0xF6, 0x0A,
                // BCS .no_clamp_max
                0xB0, 0x06,
                // STA $0AF6
                0x8D, 0xF6, 0x0A,
                // STA $0B10
                0x8D, 0x10, 0x0B,
            };

            var exitRoom = world.JsonData.RoomGeometries.Where(r => r.doors.Any(d => d.entrance_ptr == toPtr)).FirstOrDefault();
            var exitDoorIdx = exitRoom?.doors.Select((d, idx) => (d, idx)).Where(d => d.d.entrance_ptr == toPtr).FirstOrDefault().idx;

            var mapConnIdx = world.Map!.conn_to_room_id
                .Select((roomId, index) => new { roomId, index })
                .FirstOrDefault(x => x.roomId == exitRoom?.room_id && world.Map.conn_to_door_id[x.index] == exitDoorIdx)
                ?.index ?? -1;

            if (mapConnIdx == -1)
            {
                Console.WriteLine($"Warning: Failed to find map connection for sand entrance door ptr {toPtr:X4}, skipping clamp ASM for this room.");
                continue;
            }

            var entranceRoomId = world.Map!.conn_from_room_id[mapConnIdx];
            var entranceDoorId = world.Map!.conn_from_door_id[mapConnIdx];

            var entranceRoom = world.JsonData.RoomGeometries.Where(r => r.room_id == entranceRoomId).FirstOrDefault();
            var entranceDoorExitPtr = world.JsonData.RoomGeometries.Where(r => r.room_id == entranceRoomId).First().doors[entranceDoorId].exit_ptr;

            Console.WriteLine($"Writing sand entrance clamp ASM to outgoing ptr {entranceDoorExitPtr:X4} for door ptr {toPtr:X4} in room {exitRoom?.name} (room ID {exitRoom?.room_id}) coming from room {entranceRoom?.name} (room ID {entranceRoom?.room_id})");

            asmPtr = WriteExtraDoorAsm(world, entranceDoorExitPtr!.Value & 0xFFFF, asmPtr, asm.ToArray());

        }

        return asmPtr;
    }



    private int WriteMapDoorAsm(World world)
    {
        byte[] toiletAsm = [0x20, 0x01, 0xE3]; // JSR $E301
        byte[] bossAsm = [0x9C, 0x1E, 0x0E]; // STZ $0E1E

        int asmPtr = 0xF500;
        asmPtr = WriteExtraDoorAsm(world, 0xA600, asmPtr, toiletAsm);
        asmPtr = WriteExtraDoorAsm(world, 0xA60C, asmPtr, toiletAsm);

        asmPtr = WriteExtraDoorAsm(world, 0x91CE, asmPtr, bossAsm);
        asmPtr = WriteExtraDoorAsm(world, 0x91DA, asmPtr, bossAsm);
        asmPtr = WriteExtraDoorAsm(world, 0xA96C, asmPtr, bossAsm);
        asmPtr = WriteExtraDoorAsm(world, 0xA978, asmPtr, bossAsm);
        asmPtr = WriteExtraDoorAsm(world, 0x93DE, asmPtr, bossAsm);
        asmPtr = WriteExtraDoorAsm(world, 0x93EA, asmPtr, bossAsm);
        asmPtr = WriteExtraDoorAsm(world, 0xA2C4, asmPtr, bossAsm);
        return asmPtr;
    }

    private int WriteExtraDoorAsm(World world, int doorPtr, int asmPtr, byte[] asm)
    {
        ushort originalDoorAsmPtr = 0;
        try
        {
            byte[] originalDoorData = Read((SNES)(0x830000 + doorPtr + 10), 2);
            originalDoorAsmPtr = BitConverter.ToUInt16(originalDoorData);
        }
        catch { }

        byte[] writeAsm = [.. asm, .. (byte[])(originalDoorAsmPtr >= 0x8000 ? [0x4C, .. BitConverter.GetBytes(originalDoorAsmPtr)] : [0x60])];

        Write((SNES)(0x8f0000 + asmPtr), writeAsm);
        Write((SNES)(0x830000 + doorPtr + 10), BitConverter.GetBytes((ushort)asmPtr));

        return asmPtr + writeAsm.Length;
    }

    private void WriteMiniMapData(World world)
    {
        var mapStations = new List<(int, int, int)>(); // area, x, y
        var bossIcons = new List<(int, int, int)>(); // area, x, y

        for (int i = 0; i < 0x8000; i += 2)
        {
            Write((SNES)(0xB58000 + i), new byte[] { 0x1F, 0x00 });
        }

        int[] area_x_offsets = new int[6];
        int[] area_y_offsets = new int[6];
        for (int i = 0; i < 6; i++)
        {
            area_x_offsets[i] = world.Map!.room_x.Select((r, idx) => (r, idx)).Where(r => world.Map.room_area[r.idx] == i).Min(r => r.r);
            area_y_offsets[i] = world.Map!.room_y.Select((r, idx) => (r, idx)).Where(r => world.Map.room_area[r.idx] == i).Min(r => r.r);
        }

        Dictionary<(int, int, int), bool> mapTileWrites = new Dictionary<(int, int, int), bool>();

        for (int i = 0; i < world.Map!.room_id.Count(); i++)
        {
            var mapRoom = world.Map.room_id[i];
            var mapRoomX = world.Map.room_x[i];
            var mapRoomY = world.Map.room_y[i];
            var mapArea = world.Map.room_area[i];
            var roomGeometry = world.JsonData.RoomGeometries.Where(r => r.room_id == mapRoom).FirstOrDefault();

            if (roomGeometry == null)
            {
                Console.WriteLine($"Warning: No room geometry found for room ID {mapRoom}, skipping minimap data for this room.");
                continue;
            }

            var mapTiles = world.JsonData.MapRoomData.Rooms.FirstOrDefault(r => r.RoomId == roomGeometry.room_id);

            if (mapTiles == null)
            {
                Console.WriteLine($"Warning: No map tile data found for room '{roomGeometry.name}', skipping minimap data for this room.");
                continue;
            }

            var (offsetX, offsetY) = (mapRoomX - area_x_offsets[mapArea], mapRoomY - area_y_offsets[mapArea]);

            foreach (var tile in mapTiles.MapTiles)
            {
                var tileBytes = tile.GetBytes();

                ushort palette = MapTile.Red;
                if (mapTiles.Heated == true)
                {
                    palette = MapTile.Orange;
                }
                else if ((mapTiles.LiquidType ?? "") == "water" && (mapTiles.LiquidLevel ?? 0m) <= 1m)
                {
                    palette = MapTile.Green;
                }

                tileBytes[1] = (byte)((tileBytes[1] | ((palette >> 8) & 0x1F)));

                if(mapTileWrites.ContainsKey((mapArea, offsetX + tile.Coords[0], offsetY + tile.Coords[1])))
                {
                    // Already wrote a tile here, skip so we don't overwrite already written map tiles
                    // This can be a problem with for example the toilet
                    continue;
                }

                WriteMapTile(mapArea, (offsetX + tile.Coords[0]), (offsetY + tile.Coords[1]), tileBytes);
                mapTileWrites[(mapArea, offsetX + tile.Coords[0], offsetY + tile.Coords[1])] = true;

                if (tile.Interior == TileInterior.MapStation)
                {
                    mapStations.Add((mapArea, offsetX + tile.Coords[0], offsetY + tile.Coords[1]));
                }
            }

            Write((Address)(roomGeometry.rom_address + 2), [(byte)(offsetX + MAP_X_OFFSET), (byte)(offsetY + MAP_Y_OFFSET - 1)]);

            // Write to the new "map area" index what area this room belongs to on the map
            var roomHeader = world.JsonData.RoomHeaders.First(r => (r.Address & 0xFFFF) == (roomGeometry.rom_address & 0xFFFF));
            Write((SNES)(0x8FFD00 + (roomHeader.RoomArea * 128) + roomHeader.RoomIndex), [(byte)mapArea]);

            if (roomGeometry.name == "Pants Room")
            {
                // Also update east pants room
                Write((SNES)(0x8FFD00 + (roomHeader.RoomArea * 128) + 0x25), [(byte)mapArea]);
            }

            if (roomGeometry.name == "West Ocean")
            {
                // Also update homing geemer room
                Write((SNES)(0x8FFD00 + (roomHeader.RoomArea * 128) + 0x11), [(byte)mapArea]);
            }
        }

        WriteMapMarkers(world);
        WriteDecoTiles(world, mapStations, area_x_offsets, area_y_offsets);
    }

    private void WriteMapTile(int mapArea, int x, int y, byte[] mapTileData)
    {
        int areaAddress = mapArea switch
        {
            0 => 0xB59000,
            1 => 0xB58000,
            2 => 0xB5A000,
            3 => 0xB5B000,
            4 => 0xB5C000,
            5 => 0xB5D000,
            _ => throw new Exception("Invalid map area")
        };

        x += MAP_X_OFFSET;
        y += MAP_Y_OFFSET;

        Write((SNES)(areaAddress + (((x % 32) * 2 + y * 64) + (x / 32) * 0x800)), mapTileData);
    }

    private void WriteMapMarkers(World world)
    {
        // When playing map rando, the default map markers are useless so let's disable all of them for now
        // TODO: Actually write updated markers in the correct spots
        Write((SNES)0x82C7CB, [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
        Write((SNES)0x82C7DB, [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
        Write((SNES)0x82C7EB, [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
        Write((SNES)0x82C7FB, [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
        Write((SNES)0x82C80B, [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
        Write((SNES)0x82C81B, [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
        Write((SNES)0x82C82B, [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
    }

    /*
     Map Decoration has 3 layers which work as followed:
        Area Pointer:
	        A list of all areas to point to a list of decoration groups for their respective area.
	        This is labeled on top in the file as "MapDecoration_AreaPointer".
	        You can change the label to a unique one, for instance "MapDecoration_List_Crateria" to point to a new decoration list
	        or "MapDecoration_NoDecoration" to not draw any deco tiles for that area.
        Deco Group List:
	        A list of various deco group instructions to point to and position to in the map.
	        An instruction for it looks like this:
		        DW $aaaa : DB $xx, $yy
	        aaaa : pointer to deco group instruction. Labels also can be used for this.
	               $0000 is the terminator for deco group lists.
	        xx : X position for deco tilegroup (range: $00 - $1F ($3F, if vertical map is not set for this area))
	        yy : Y position for deco tilegroup (range: $00 - $1F ($3F, if vertical map is set for this area))
        Deco Group Instruction:
	        The actual instruction to draw these decorations.
	        An instruction for it looks like this:
		        DB bb : DW tttt, ...
	        bb : amount of tiles to draw in one row. If 00, it will jump to the next row.
	             Setting it to $40 and above is the terminator for deco group instructions.
	        tttt : tiledata. You should write as many tiledata as listed in "bb"
	               the bitmask for tiledata looks like this:
				        vhpc cctt tttt tttt
		           v = vertical mirror of tile
		           h = horizontal mirror of tile
		           p = priority bit (must be 0)
		           c = palette
		           t = tile ID (must be $100 and above to be considered as a deco tile)
    */
    private void WriteDecoTiles(World world, List<(int, int, int)> mapStations, int[] areaXOffsets, int[] areaYOffsets)
    {
        int decoTilePtr = 0x89E000;
        int decoInstructionPtr = 0x89E020;

        (var mapInstruction, decoInstructionPtr) = CreateDecoTileInstruction(0x081C, decoInstructionPtr);

        (var createriaInstruction, decoInstructionPtr) = CreateDecoTileInstruction(0x0832, decoInstructionPtr); // "C"
        (var brinstarInstruction, decoInstructionPtr) = CreateDecoTileInstruction(0x0831, decoInstructionPtr); // "B"
        (var wreckedShipInstruction, decoInstructionPtr) = CreateDecoTileInstruction(0x0846, decoInstructionPtr); // "W"
        (var maridiaInstruction, decoInstructionPtr) = CreateDecoTileInstruction(0x083C, decoInstructionPtr); // "M"
        (var tourianInstruction, decoInstructionPtr) = CreateDecoTileInstruction(0x0843, decoInstructionPtr); // "T"
        (var norfairInstruction, decoInstructionPtr) = CreateDecoTileInstruction(0x083D, decoInstructionPtr); // "N"

        var areaInstructions = new int[]
        {
            createriaInstruction,
            brinstarInstruction,
            norfairInstruction,
            wreckedShipInstruction,
            maridiaInstruction,
            tourianInstruction
        };


        List<(int, int, int)>[] areaDecoInstructions = new List<(int, int, int)>[6];
        for (int i = 0; i < 6; i++)
        {
            areaDecoInstructions[i] = new List<(int, int, int)>();
        }


        // Find all cross-area connections
        foreach (var connection in world.JsonData.Connections.SelectMany(c => c.Connections))
        {
            // Get source node
            var leftNode = world.JsonData.Rooms.Find(r => connection.Nodes[0].RoomId == r.Id)?.Nodes.First(n => n.Id == connection.Nodes[0].NodeId);
            var rightNode = world.JsonData.Rooms.Find(r => connection.Nodes[1].RoomId == r.Id)?.Nodes.First(n => n.Id == connection.Nodes[1].NodeId);

            if (leftNode == null || rightNode == null)
            {
                Console.WriteLine($"Warning: Could not find nodes for cross-area connection between room {connection.Nodes[0].RoomId} and {connection.Nodes[1].RoomId}");
            }

            var leftExitPtr = Convert.ToInt32(leftNode?.NodeAddress?.Substring(2) ?? "0", 16);
            var leftRoomGeometry = world.JsonData.RoomGeometries.FirstOrDefault(r => r.doors.Any(d => d.exit_ptr == leftExitPtr));
            var leftRoomMapIdx = world.Map!.room_id.IndexOf(leftRoomGeometry?.room_id ?? -1);

            var rightExitPtr = Convert.ToInt32(rightNode?.NodeAddress?.Substring(2) ?? "0", 16);
            var rightRoomGeometry = world.JsonData.RoomGeometries.FirstOrDefault(r => r.doors.Any(d => d.exit_ptr == rightExitPtr));
            var rightRoomMapIdx = world.Map!.room_id.IndexOf(rightRoomGeometry?.room_id ?? -1);

            if (leftRoomMapIdx == -1 || rightRoomMapIdx == -1)
            {
                Console.WriteLine($"Warning: Could not find map data for cross-area connection between room {connection.Nodes[0].RoomId} and {connection.Nodes[1].RoomId}");
                continue;
            }

            if (world.Map.room_area[leftRoomMapIdx] == world.Map.room_area[rightRoomMapIdx])
            {
                continue;
            }

            Console.WriteLine($"Adding map decoration for cross-area connection between {leftRoomGeometry?.name} and {rightRoomGeometry?.name}");

            var (leftArea, rightArea) = (world.Map.room_area[leftRoomMapIdx], world.Map.room_area[rightRoomMapIdx]);

            foreach (var node in new[] { (leftNode, rightArea), (rightNode, leftArea) })
            {
                // Get exitptr of source node
                var exitPtr = Convert.ToInt32(node.Item1?.NodeAddress?.Substring(2) ?? "0", 16);
                var roomGeometry = world.JsonData.RoomGeometries.FirstOrDefault(r => r.doors.Any(d => d.exit_ptr == exitPtr));
                var doorGeometry = roomGeometry?.doors.FirstOrDefault(d => d.exit_ptr == exitPtr)!;
                var roomMapIdx = world.Map!.room_id.IndexOf(roomGeometry?.room_id ?? -1);
                var roomMapX = world.Map!.room_x[roomMapIdx];
                var roomMapY = world.Map!.room_y[roomMapIdx];
                var roomMapArea = world.Map!.room_area[roomMapIdx];

                // Offset by door coordinates
                var x = roomMapX + (doorGeometry?.x ?? 0) + MAP_X_OFFSET;
                var y = roomMapY + (doorGeometry?.y ?? 0) + MAP_Y_OFFSET;

                // Offset by door orientation
                var (dx, dy) = doorGeometry!.direction switch
                {
                    "left" => (-1, 0),
                    "right" => (1, 0),
                    "down" => (0, 1),
                    "up" => (0, -1),
                    _ => (0, 0)
                };

                if (doorGeometry!.subtype == "elevator")
                {
                    var elevatorHeight = roomGeometry!.map.Count();
                    if (doorGeometry!.direction == "up")
                    {
                        y += (elevatorHeight - 1);
                    }
                    else if (doorGeometry!.direction == "down")
                    {
                        y -= (elevatorHeight - 1);
                    }
                }

                var markerCoordinates = ((x + dx) - areaXOffsets[roomMapArea], (y + dy) - areaYOffsets[roomMapArea]);
                areaDecoInstructions[roomMapArea].Add((areaInstructions[node.Item2], markerCoordinates.Item1, markerCoordinates.Item2));
            }
        }

        // Add map stations
        var stationsByArea = mapStations.GroupBy(s => s.Item1);
        foreach (var areaGroup in stationsByArea)
        {
            foreach (var (a, x, y) in areaGroup)
            {
                areaDecoInstructions[a].Add((mapInstruction, x + MAP_X_OFFSET, y + MAP_Y_OFFSET));
            }
        }

        // Write out all the decoration group lists
        int decoTileDataPtr = decoInstructionPtr;
        for (int area = 0; area < 6; area++)
        {
            int decoTileAreaPtr = decoTilePtr + (area * 2);
            Write((SNES)decoTileAreaPtr, BitConverter.GetBytes((ushort)(decoTileDataPtr & 0xFFFF)));
            foreach (var (instructionPtr, x, y) in areaDecoInstructions[area])
            {
                // Write the decoration group list entry
                Write((SNES)decoTileDataPtr, BitConverter.GetBytes((ushort)(instructionPtr & 0xFFFF)));
                decoTileDataPtr += 2;
                Write((SNES)decoTileDataPtr, [(byte)x, (byte)y]);
                decoTileDataPtr += 2;
            }
            // Write the terminator for the decoration group list
            Write((SNES)decoTileDataPtr, [0x00, 0x00]);
            decoTileDataPtr += 2;
        }

    }

    private (int, int) CreateDecoTileInstruction(ushort tileMask, int instructionPtr)
    {
        int next = instructionPtr;
        Write((SNES)next, [0x01]); // 1 tile in a row
        next += 1;
        Write((SNES)next, BitConverter.GetBytes(tileMask));
        next += 2;
        Write((SNES)next, [0x40]); // Terminator
        next += 1;
        return (instructionPtr, next);
    }

    private void WriteDoorCaps(World world)
    {
        var doorData = DoorReader.ReadDoorData();
        var allDoors = world.JsonData.Rooms
            .SelectMany(r => r.Nodes
                .Where(n => n.NodeType == "door" &&
                           (n.NodeSubType == "blue" || n.NodeSubType == "red" ||
                            n.NodeSubType == "green" || n.NodeSubType == "yellow" ||
                            n.NodeSubType == "eye" || n.NodeSubType == "gray"))
                .Select(n => new { RoomId = r.Id, Node = n })
            )
            .ToList();

        ushort doorIndex = 1;

        foreach (var door in allDoors)
        {
            var room = world.JsonData.Rooms.First(r => r.Id == door.RoomId);
            var doorPlms = world.JsonData.DoorPLMMaps.FirstOrDefault(p => p.RoomId == door.RoomId && p.NodeId == door.Node.Id);

            var plmToWrite = door.Node.NodeSubType switch
            {
                "blue" => DoorTypePlm.Nothing,
                "red" => door.Node.DoorOrientation switch
                {
                    "left" => DoorTypePlm.RedDoorRight,
                    "right" => DoorTypePlm.RedDoorLeft,
                    "up" => DoorTypePlm.RedDoorDown,
                    "down" => DoorTypePlm.RedDoorUp,
                    _ => throw new Exception("Invalid door orientation")
                },
                "green" => door.Node.DoorOrientation switch
                {
                    "left" => DoorTypePlm.GreenDoorRight,
                    "right" => DoorTypePlm.GreenDoorLeft,
                    "up" => DoorTypePlm.GreenDoorDown,
                    "down" => DoorTypePlm.GreenDoorUp,
                    _ => throw new Exception("Invalid door orientation")
                },
                "yellow" => door.Node.DoorOrientation switch
                {
                    "left" => DoorTypePlm.YellowDoorRight,
                    "right" => DoorTypePlm.YellowDoorLeft,
                    "up" => DoorTypePlm.YellowDoorDown,
                    "down" => DoorTypePlm.YellowDoorUp,
                    _ => throw new Exception("Invalid door orientation")
                },
                "gray" => door.Node.DoorOrientation switch
                {
                    "left" => DoorTypePlm.GreyDoorRight,
                    "right" => DoorTypePlm.GreyDoorLeft,
                    "up" => DoorTypePlm.GreyDoorDown,
                    "down" => DoorTypePlm.GreyDoorUp,
                    _ => throw new Exception("Invalid door orientation")
                },
                "eye" => DoorTypePlm.Nothing,
                _ => throw new Exception("Invalid door type")
            };

            if (doorPlms != null && doorPlms.PLMs != null && doorPlms.PLMs.Count() > 0)
            {
                int doorsWritten = 0;
                foreach (var plm in doorPlms.PLMs)
                {
                    // Don't overwrite existing grey doors with new ones
                    if (plm.DoorType.IsGrey() && plmToWrite.IsGrey())
                    {
                        continue;
                    }

                    if (plm.DoorType != DoorTypePlm.Unknown)
                    {
                        if (plmToWrite == DoorTypePlm.Nothing)
                        {
                            Write((SNES)(plm.Address + (plm.PlmIndex * 6)), [.. BitConverter.GetBytes((ushort)plmToWrite), 0xFF, 0xFF, 0xFF, 0xFF]);
                        }
                        else
                        {
                            Write((SNES)(plm.Address + (plm.PlmIndex * 6)), [.. BitConverter.GetBytes((ushort)plmToWrite), (byte)doorPlms.XPosition, (byte)doorPlms.YPosition, .. BitConverter.GetBytes((ushort)doorIndex)]);
                            doorsWritten++;
                        }
                    }
                }

                if (doorsWritten > 0)
                {
                    doorIndex++;
                }
            }
            else if (doorPlms != null && plmToWrite != DoorTypePlm.Unknown && plmToWrite != DoorTypePlm.Nothing)
            {
                Write((SNES)(0x8F0000 + _plmTableOffset), [.. BitConverter.GetBytes((ushort)(doorPlms.RoomAddress & 0xFFFF)), .. BitConverter.GetBytes((ushort)plmToWrite), (byte)doorPlms.XPosition, (byte)doorPlms.YPosition, .. BitConverter.GetBytes((ushort)doorIndex)]);
                _plmTableOffset += 0x08;
                doorIndex++;
            }
        }

        // Write back specific door caps to prevent issues
        // We want to write a new fancy grey door to MB's room that checks the boss kill bits
        var motherBrainDoor = world.GetLocation("Tourian - Mother Brain Room - Right Door");
        var otherDoor = (Vertex)motherBrainDoor.Edges.First(e => ((Vertex)e.From).RoomId != ((Vertex)e.To).RoomId).To;
        var doorPlmData = world.JsonData.DoorPLMMaps.First(d => d.RoomId == otherDoor.RoomId && d.NodeId == otherDoor.NodeId);

        if (doorPlmData.PLMs != null)
        {
            if (otherDoor.Node is null)
                throw new InvalidOperationException("Expected door node information for Mother Brain room.");

            // Write a new PLM to this room with our mother brain door, facing the correct way
            var plmToWrite = otherDoor.Node.DoorOrientation switch
            {
                "left" => KeycardDoors.Right,
                "right" => KeycardDoors.Left,
                _ => throw new Exception("Invalid door orientation")
            };

            var plmArgument = KeycardEvents.MotherBrainDoor;

            Write((SNES)(0x8F0000 + _plmTableOffset), [.. BitConverter.GetBytes((ushort)(doorPlmData.RoomAddress & 0xFFFF)), .. BitConverter.GetBytes((ushort)plmToWrite), (byte)doorPlmData.XPosition, (byte)doorPlmData.YPosition, .. BitConverter.GetBytes((ushort)plmArgument)]);
            _plmTableOffset += 0x08;
        }

        // Permanently block off the wrong side of MB's room
        var motherBrainLeftDoor = world.GetLocation("Tourian - Mother Brain Room - Left Blast Door");
        var otherLeftdoor = (Vertex)motherBrainLeftDoor.Edges.First(e => ((Vertex)e.From).RoomId != ((Vertex)e.To).RoomId).To;
        var doorPlmDataLeft = world.JsonData.DoorPLMMaps.First(d => d.RoomId == otherLeftdoor.RoomId && d.NodeId == otherLeftdoor.NodeId);

        if (doorPlmDataLeft.PLMs != null)
        {
            if (otherLeftdoor.Node is null)
                throw new InvalidOperationException("Expected door node information for Mother Brain left door.");

            // Write a new PLM to this room with our mother brain door, facing the correct way
            var plmToWrite = otherLeftdoor.Node.DoorOrientation switch
            {
                "left" => KeycardDoors.Right,
                "right" => KeycardDoors.Left,
                _ => throw new Exception("Invalid door orientation")
            };
            var plmArgument = KeycardEvents.NeverDoor;
            Write((SNES)(0x8F0000 + _plmTableOffset), [.. BitConverter.GetBytes((ushort)(doorPlmDataLeft.RoomAddress & 0xFFFF)), .. BitConverter.GetBytes((ushort)plmToWrite), (byte)doorPlmDataLeft.XPosition, (byte)doorPlmDataLeft.YPosition, .. BitConverter.GetBytes((ushort)plmArgument)]);
            _plmTableOffset += 0x08;
        }


        Write((SNES)(0x8F0000 + _plmTableOffset), [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
    }

    // Builds a door header entry
    private byte[] BuildDoorData(ushort roomHeader, byte[] doorProps, ushort distance = 0x8000, ushort asmPtr = 0x0000)
    {
        if (doorProps == null || doorProps.Length != 6)
        {
            throw new ArgumentException("doorProps must be 6 bytes");
        }

        var buf = new List<byte>(14);
        buf.AddRange(UshortBytes(roomHeader));
        buf.AddRange(doorProps);
        buf.AddRange(UshortBytes(distance));
        buf.AddRange(UshortBytes(asmPtr));
        return buf.ToArray();
    }

    // Builds a door out list entry table
    private byte[] BuildDoorList(params ushort[] doorPtrs)
    {
        var buf = new List<byte>(2 * doorPtrs.Length);
        foreach (var p in doorPtrs)
        {
            buf.AddRange(UshortBytes(p));
        }
        return [.. buf];
    }


    // Builds a PLM list entry table
    private byte[] BuildPlmList(params PlmEntry[] entries)
    {
        var buf = new List<byte>(entries.Length * 6 + 2);
        foreach (var e in entries)
        {
            buf.AddRange(UshortBytes(e.Id));
            buf.Add(e.X); buf.Add(e.Y);
            buf.AddRange(UshortBytes(e.Var));
        }
        buf.AddRange(UshortBytes(0x0000));
        return [.. buf];
    }


    // Default door properties for refill/map/save room door replacement
    private static byte[] PropsIn(OriginalDoorPosition dir) =>
        dir == OriginalDoorPosition.Left ? [0x40, 0x05, 0x0E, 0x06, 0x00, 0x00]
                                         : [0x40, 0x04, 0x01, 0x06, 0x00, 0x00];
    private static byte[] PropsOut(OriginalDoorPosition dir) =>
        dir == OriginalDoorPosition.Left ? [0x40, 0x04, 0x0E, 0x06, 0x00, 0x00]
                                         : [0x40, 0x05, 0x01, 0x06, 0x00, 0x00];

    private void PatchRoomHeader(int roomHeaderLong, ushort doorList, int levelDataPtr, ushort plmList)
    {
        Write((SNES)(roomHeaderLong + 0x09), UshortBytes(doorList));
        Write((SNES)(roomHeaderLong + 0x0D), UlongBytes(levelDataPtr));
        Write((SNES)(roomHeaderLong + 0x21), UshortBytes(plmList));
    }

    /// Replace a MAP room
    public void ReplaceMapRoom(
        int origRoomHeaderLong, ushort origDoorPtr,
        OriginalDoorPosition direction, int levelDataPtr = 0xDEE000,
        byte bank83 = 0x83, byte bank8F = 0x8F)
    {
        ushort roomHeader = (ushort)(origRoomHeaderLong);

        // Door data
        var inBlob = BuildDoorData(roomHeader, PropsIn(direction));
        var outBlob = BuildDoorData(roomHeader, PropsOut(direction));
        int doorStart = _fsm.Alloc(bank83, inBlob.Length + outBlob.Length, 2);
        Write((SNES)(doorStart), inBlob);
        Write((SNES)(doorStart + inBlob.Length), outBlob);
        ushort doorPtr = (ushort)(doorStart + inBlob.Length);

        // Door list (order depends on direction)
        var doorList = direction == OriginalDoorPosition.Left
            ? BuildDoorList(origDoorPtr, doorPtr)
            : BuildDoorList(doorPtr, origDoorPtr);
        int doorListStart = _fsm.Alloc(bank8F, doorList.Length, 2);
        Write((SNES)(doorListStart), doorList);
        ushort doorListPtr = (ushort)(doorListStart);

        // PLMs: map station
        var plms = BuildPlmList(new PlmEntry(0xB6D3, 0x08, 0x0A, 0x8000));
        int plmStart = _fsm.Alloc(bank8F, plms.Length, 2);
        Write((SNES)(plmStart), plms);
        ushort plmList = (ushort)(plmStart);

        PatchRoomHeader(origRoomHeaderLong, doorListPtr, levelDataPtr, plmList);
    }

    /// Replace a REFILL room (double-door)
    public void ReplaceRefillRoom(
        int origRoomHeaderLong, ushort origDoorPtr,
        OriginalDoorPosition direction, int levelDataPtr = 0xCE8FA6,
        byte bank83 = 0x83, byte bank8F = 0x8F)
    {
        ushort roomHeader = (ushort)(origRoomHeaderLong);

        var inBlob = BuildDoorData(roomHeader, PropsIn(direction));
        var outBlob = BuildDoorData(roomHeader, PropsOut(direction));
        int doorStart = _fsm.Alloc(bank83, inBlob.Length + outBlob.Length, 2);
        Write((SNES)(doorStart), inBlob);
        Write((SNES)(doorStart + inBlob.Length), outBlob);
        ushort doorPtr = (ushort)(doorStart + inBlob.Length);

        var doorList = direction == OriginalDoorPosition.Left
            ? BuildDoorList(origDoorPtr, doorPtr)
            : BuildDoorList(doorPtr, origDoorPtr);
        int doorListStart = _fsm.Alloc(bank8F, doorList.Length, 2);
        Write((SNES)(doorListStart), doorList);
        ushort doorListPtr = (ushort)(doorListStart);

        // PLMs: refill station
        var plms = BuildPlmList(new PlmEntry(0xB6DF, 0x07, 0x0A, 0x0048));
        int plmStart = _fsm.Alloc(bank8F, plms.Length, 2);
        Write((SNES)(plmStart), plms);
        ushort plmList = (ushort)(plmStart);

        PatchRoomHeader(origRoomHeaderLong, doorListPtr, levelDataPtr, plmList);
    }

    /// Replace a SAVE room
    public void ReplaceSaveRoom(
        int origRoomHeaderLong, ushort origDoorPtr,
        OriginalDoorPosition direction, int levelDataPtr = 0xCE9EF6,
        byte bank83 = 0x83, byte bank8F = 0x8F)
    {
        ushort roomHeader = (ushort)(origRoomHeaderLong);

        var inBlob = BuildDoorData(roomHeader, PropsIn(direction));
        var outBlob = BuildDoorData(roomHeader, PropsOut(direction));
        int doorStart = _fsm.Alloc(bank83, inBlob.Length + outBlob.Length, 2);
        Write((SNES)(doorStart), inBlob);
        Write((SNES)(doorStart + inBlob.Length), outBlob);
        ushort doorPtr = (ushort)(doorStart + inBlob.Length);

        var doorList = direction == OriginalDoorPosition.Left
            ? BuildDoorList(origDoorPtr, doorPtr)
            : BuildDoorList(doorPtr, origDoorPtr);
        int doorListStart = _fsm.Alloc(bank8F, doorList.Length, 2);
        Write((SNES)(doorListStart), doorList);
        ushort doorListPtr = (ushort)(doorListStart);

        // PLMs: save station
        var plms = BuildPlmList(new PlmEntry(0xB76F, 0x07, 0x0B, 0x0001));
        int plmStart = _fsm.Alloc(bank8F, plms.Length, 2);
        Write((SNES)(plmStart), plms);
        ushort plmList = (ushort)(plmStart);

        PatchRoomHeader(origRoomHeaderLong, doorListPtr, levelDataPtr, plmList);
    }

    /// Patch the tourian save station into a map station
    public void WriteTourianMapStation(bool singleDoor = true)
    {
        int roomHeaderPtr = 0x8FDF1B;
        ushort origDoorPtr = 0xAB40;

        if (singleDoor == false)
        {
            // Replace this room with a double-door map station
            ReplaceMapRoom(roomHeaderPtr, origDoorPtr, OriginalDoorPosition.Left);
        }
        else
        {
            // Switch the level data and PLM to the crateria map station
            int mapRoomLevelDataPtr = 0xCE86BD;
            int mapRoomPlmPtr = 0x8444;
            PatchRoomHeader(roomHeaderPtr, 0xDF42, mapRoomLevelDataPtr, (ushort)mapRoomPlmPtr);
        }
    }

    static class KeycardPlaque
    {
        public const ushort Level1 = 0xf7;
        public const ushort Level2 = 0xf8;
        public const ushort Boss = 0xf9;
        public const ushort None = 0xfa;
        public const ushort Zero = 0xfa;
        public const ushort One = 0xfb;
        public const ushort Two = 0xfc;
        public const ushort Three = 0xfd;
        public const ushort Four = 0xfe;
    }

    static class KeycardDoors
    {
        public const ushort Left = 0xd414;
        public const ushort Right = 0xd41a;
        public const ushort Up = 0xd420;
        public const ushort Down = 0xd426;
        public const ushort BossLeft = 0xc842;
        public const ushort BossRight = 0xc848;
    }

    static class KeycardEvents
    {
        public const ushort CrateriaLevel1 = 0x0000;
        public const ushort CrateriaLevel2 = 0x0100;
        public const ushort CrateriaBoss = 0x0200;
        public const ushort BrinstarLevel1 = 0x0300;
        public const ushort BrinstarLevel2 = 0x0400;
        public const ushort BrinstarBoss = 0x0500;
        public const ushort NorfairLevel1 = 0x0600;
        public const ushort NorfairLevel2 = 0x0700;
        public const ushort NorfairBoss = 0x0800;
        public const ushort MaridiaLevel1 = 0x0900;
        public const ushort MaridiaLevel2 = 0x0a00;
        public const ushort MaridiaBoss = 0x0b00;
        public const ushort WreckedShipLevel1 = 0x0c00;
        public const ushort WreckedShipBoss = 0x0d00;
        public const ushort LowerNorfairLevel1 = 0x0e00;
        public const ushort LowerNorfairBoss = 0x0f00;
        public const ushort MotherBrainDoor = 0x1000;
        public const ushort NeverDoor = 0x8000;
    }

    public readonly struct PlmEntry
    {
        public readonly ushort Id;
        public readonly byte X;
        public readonly byte Y;
        public readonly ushort Var;
        public PlmEntry(ushort id, byte x, byte y, ushort var) { Id = id; X = x; Y = y; Var = var; }
    }

    public enum OriginalDoorPosition : int
    {
        Left = 0,
        Right = 1,
    }

    private static byte[] UintBytes(int value) => BitConverter.GetBytes((uint)value);

    // Gets a 24-bit SNES address in little-endian format
    private static byte[] UlongBytes(int value) => [(byte)(value & 0xFF), (byte)((value >> 8) & 0xFF), (byte)((value >> 16) & 0xFF)];

    private static byte[] UshortBytes(int value) => BitConverter.GetBytes((ushort)value);

    private static byte[] AsAscii(string text) => Encoding.ASCII.GetBytes(text);

}

public sealed class FreeSpaceManager
{
    private sealed class Segment
    {
        public int Start;
        public int End;
        public Segment(int s, int e) { Start = s; End = e; }
    }

    private readonly Dictionary<byte, List<Segment>> _free = new();

    public FreeSpaceManager()
    {
        // Default free space configuration
        AddRange(0x83B100, 0x83B2FF);
        AddRange(0x8FF000, 0x8FF1FF);
    }

    public void AddRange(int startLong, int endLong)
    {
        if (startLong >= endLong) return;
        byte b0 = (byte)((startLong >> 16) & 0xFF);
        byte b1 = (byte)(((endLong - 1) >> 16) & 0xFF);
        if (b0 != b1) throw new ArgumentException("Free range must be in a single bank.");

        if (!_free.TryGetValue(b0, out var list))
            _free[b0] = list = new List<Segment>();

        list.Add(new Segment(startLong, endLong));
        Coalesce(list);
    }

    public int Alloc(byte bank, int size, int alignment = 2)
    {
        if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
        if (alignment <= 0) alignment = 1;

        if (!_free.TryGetValue(bank, out var list) || list.Count == 0)
            throw new InvalidOperationException($"No free space configured in bank ${bank:X2}");

        for (int i = 0; i < list.Count; i++)
        {
            var seg = list[i];
            int alignedStart = Align(seg.Start, alignment);
            if (alignedStart + size <= seg.End)
            {
                int allocStart = alignedStart;
                int allocEnd = allocStart + size;

                var pieces = new List<Segment>();
                if (seg.Start < allocStart) pieces.Add(new Segment(seg.Start, allocStart));
                if (allocEnd < seg.End) pieces.Add(new Segment(allocEnd, seg.End));

                list.RemoveAt(i);
                if (pieces.Count > 0) list.InsertRange(i, pieces);
                Console.WriteLine($"Allocated {size} bytes (align {alignment}) in bank ${bank:X2} at ${allocStart:X6}");
                return allocStart;
            }
        }

        throw new InvalidOperationException($"Insufficient free space in bank ${bank:X2} for {size} bytes (align {alignment}).");
    }

    private static int Align(int v, int a) => ((v + (a - 1)) / a) * a;

    private static void Coalesce(List<Segment> list)
    {
        list.Sort((a, b) => a.Start.CompareTo(b.Start));
        for (int i = 1; i < list.Count;)
        {
            var prev = list[i - 1];
            var cur = list[i];
            if (prev.End >= cur.Start)
            {
                prev.End = Math.Max(prev.End, cur.End);
                list.RemoveAt(i);
            }
            else i++;
        }
    }
}
