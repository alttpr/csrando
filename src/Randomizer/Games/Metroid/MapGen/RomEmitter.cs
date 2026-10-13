namespace Randomizer.Games.Metroid.MapGen;

using System;
using System.Collections.Generic;
using System.Linq;
using static Randomizer.Games.Metroid.YamlReader;

/// <summary>The ROM data computed for a generated map.</summary>
public class RomEmission
{
    /// <summary>PC address to bytes, written by <see cref="Rom.WritePatchData"/>.</summary>
    public required Dictionary<int, byte[]> Patches { get; init; }

    /// <summary>
    /// Grid cell to the PC address of that cell's power-up entry (its type byte).
    /// <see cref="Rom.WriteItems"/> overwrites [type, itemId] there with the filled item.
    /// </summary>
    public required Dictionary<Point, int> ItemAddresses { get; init; }

    /// <summary>Total size of the emitted special-items blob, for diagnostics.</summary>
    public required int TableSize { get; init; }
}

/// <summary>
/// Emits the ROM data for a generated map. All facts below were verified against the
/// disassembly (Bank00/Bank07) and asm/multirando-asm/src/m1:
///
/// The M1 NES PRG is embedded in the combo ROM with NES bank n at
/// PC 0x680000 + n*0x8000 (each SNES bank holds one 16KB area bank at $8000-$BFFF plus the
/// fixed bank at $C000). Banks: 0 title/map, 1 Brinstar, 2 Norfair, 3 Tourian, 4 Kraid,
/// 5 Ridley.
///
/// Emitted data:
/// - The shared 32x32 world map grid (bank 0 $A53E): one screen id per cell, $FF empty.
/// - Per-area Samus respawn map position ($95D7 X, $95D8 Y in each area bank): Brinstar
///   gets the spawn cell, the lower areas their elevator platform (vanilla behavior:
///   continuing in an area respawns you at its elevator). Combo seeds point Brinstar at
///   the portal room instead and also patch $95D9 (the in-screen spawn height).
/// - The five special-items tables, rebuilt from scratch in the bank-88 window
///   (SNES $98:8000 = PC 0x6C0000, reserved through $98:9000) and repointed via the
///   word at $9598 in each area bank. Table format (ScanForItems, $ED98): rows sorted
///   ascending by Y -- [Y][word next row, $FFFF ends][entries]; entries sorted ascending
///   by X -- [X][offset to next entry's X byte, $FF ends row][payloads...][$00]; a payload
///   is a type byte plus data and one entry can chain several payloads.
/// - Power-up payloads are [$02, itemId, position]; the position byte is screen-relative
///   so it is copied per screen from the vanilla tables (<see cref="VanillaSpecialItems"/>),
///   as are door/enemy/Mother-Brain payloads. The item id written here is a placeholder;
///   the filler's choice is written over [type, itemId] via <see cref="RomEmission.ItemAddresses"/>.
/// - Elevator payloads are [$04, data]: data bit7 set = up elevator (arrival handler $D8BF:
///   destination Brinstar, except $84 which goes to Norfair, and $8F which triggers the
///   ending), bit7 clear = destination area index. Per generated pair this emits the
///   vanilla three-entry pattern: the down byte at the upper room, an $81 mirror at the
///   same coordinate in the lower area's table (the boarding room reloads in the lower
///   area's bank mid-ride), and $80|area at the platform two cells below. The escape
///   elevator gets $8F at the escape shaft's top cell.
/// </summary>
public static class RomEmitter
{
    /// <summary>PC address of the 32x32 world map grid (NES bank 0 $A53E).</summary>
    public const int GridAddress = 0x68253E;
    /// <summary>PC address of the special-items blob (SNES $98:8000).</summary>
    public const int TableAddress = 0x6C0000;
    /// <summary>The bank-88 window reserved for the item tables ($98:8000-$98:9000).</summary>
    public const int TableLimit = 0x1000;

    private const int TablePointerNes = 0x9598;
    private const int StartPositionNes = 0x95D7; // X at $95D7, Y at $95D8

    /// <summary>Extended custom-item payload type (hooks.asm $EDDF), power-up layout.</summary>
    private const byte CustomItemType = 0x0B;
    /// <summary>Local map item (newitems.asm !M1_MAP_ITEM_ID): reveals the area, no inventory.</summary>
    private const byte MapItemId = 0xCE;

    private static readonly Area[] TableAreas =
        [Area.Brinstar, Area.Norfair, Area.Tourian, Area.Kraid, Area.Ridley];

    private static int NesBank(Area area) => area switch
    {
        Area.Brinstar => 1,
        Area.Norfair => 2,
        Area.Tourian => 3,
        Area.Kraid => 4,
        Area.Ridley => 5,
        _ => throw new ArgumentOutOfRangeException(nameof(area))
    };

    private static int NesToPc(Area area, int nesAddress) =>
        0x680000 + NesBank(area) * 0x8000 + (nesAddress - 0x8000);

    /// <summary>One special-items entry under construction for a generated cell.</summary>
    private sealed class TableEntry
    {
        public required Point Cell { get; init; }
        public List<byte> Payload { get; } = [];
        /// <summary>Offset of the power-up type byte within the payload, if any.</summary>
        public int? ItemOffset { get; set; }
    }

    /// <summary>
    /// Computes all ROM patches for <paramref name="world"/>. Must run after screen fitting
    /// and before the vanilla rooms in the yaml data are replaced (<paramref name="vanillaRooms"/>
    /// maps the vanilla coordinates of every screen, which is what ties
    /// <see cref="VanillaSpecialItems"/> payloads to screens).
    /// </summary>
    public static RomEmission Emit(GeneratedWorld world, IEnumerable<Room> vanillaRooms,
        bool respawnAtPortal = false)
    {
        var grid = world.Grid;
        var specials = JoinVanillaPayloadsToScreens(vanillaRooms);

        // One entry per cell that needs one: items first (their address is exposed), then
        // any per-screen extra payloads (doors, flavor enemies, Mother Brain hardware).
        var entries = TableAreas.ToDictionary(a => a, _ => new Dictionary<Point, TableEntry>());

        foreach (var cell in grid.Cells)
        {
            var screen = cell.AssignedScreen
                ?? throw new InvalidOperationException($"cell {cell} has no assigned screen");
            var entry = new TableEntry { Cell = cell.Position };

            // Fillable item slots. MapStation cells fit item screens too but must stay
            // out of this set, or the filler would write over their fixed pickup below.
            if (cell.Role is CellRole.Item or CellRole.Boss && screen.ItemLocationNames.Count > 0)
            {
                var position = screen.ItemPosition
                    ?? throw new InvalidOperationException(
                        $"{cell.Area} screen 0x{screen.ScreenId:X2} has an item location without a position");
                entry.ItemOffset = 0;
                // WriteItems overwrites [type, id]; id $FF is the engine's "empty slot"
                // marker, so a location that never gets an item written stays blank.
                entry.Payload.AddRange([0x02, 0xFF, position]);
            }

            // Map stations hold their area's fixed map pickup. The bytes are final here
            // (no ItemOffset), so the location never enters the item filler's pool.
            if (cell.Role == CellRole.MapStation)
            {
                var position = screen.ItemPosition
                    ?? throw new InvalidOperationException(
                        $"{cell.Area} map station screen 0x{screen.ScreenId:X2} has no item position");
                entry.Payload.AddRange([CustomItemType, MapItemId, position]);
            }

            if (specials.Extras.TryGetValue((cell.Area, screen.ScreenId), out var extras))
            {
                foreach (var extra in extras)
                    entry.Payload.AddRange(extra);
            }

            if (entry.Payload.Count > 0)
                entries[cell.Area][cell.Position] = entry;
        }

        AddElevatorEntries(world, entries);

        var patches = new Dictionary<int, byte[]>
        {
            [GridAddress] = BuildGrid(grid)
        };

        var itemAddresses = new Dictionary<Point, int>();
        var blob = SerializeTables(entries, patches, itemAddresses);
        if (blob.Length > TableLimit)
        {
            throw new InvalidOperationException(
                $"special-items blob is 0x{blob.Length:X} bytes, over the reserved 0x{TableLimit:X} window");
        }

        patches[TableAddress] = blob;

        AddStartPositions(world, patches, respawnAtPortal);
        AddAutomap(world, patches);

        return new RomEmission
        {
            Patches = patches,
            ItemAddresses = itemAddresses,
            TableSize = blob.Length
        };
    }

    /// <summary>Per-screen non-power-up vanilla payloads (doors, flavor enemies, MB hardware).</summary>
    private sealed class ScreenSpecials
    {
        public Dictionary<(Area, int), List<byte[]>> Extras { get; } = [];
    }

    /// <summary>
    /// Maps every vanilla special-items entry from its vanilla coordinate to the screen the
    /// vanilla room list places there. Power-up payloads only contribute their trailing
    /// extras (their position byte comes from <see cref="ScreenProfile.ItemPosition"/>).
    /// A screen reused at several coordinates keeps one payload per type.
    /// </summary>
    private static ScreenSpecials JoinVanillaPayloadsToScreens(IEnumerable<Room> vanillaRooms)
    {
        var screenAt = new Dictionary<(Area, int X, int Y), int>();
        foreach (var room in vanillaRooms.Where(r => r.area != Area.Meta))
        {
            for (int i = 0; i < room.screens.Length; i++)
            {
                int x = room.position[0] + (room.scroll == Scrolling.Horizontal ? i : 0);
                int y = room.position[1] + (room.scroll == Scrolling.Vertical ? i : 0);
                screenAt[(room.area, x, y)] = room.screens[i];
            }
        }

        var specials = new ScreenSpecials();
        var seenExtraTypes = new HashSet<(Area, int Screen, int Type)>();

        foreach (var entry in VanillaSpecialItems.Entries)
        {
            if (!screenAt.TryGetValue((entry.Area, entry.X, entry.Y), out var screen))
            {
                throw new InvalidOperationException(
                    $"vanilla special item at {entry.Area} ({entry.X},{entry.Y}) is outside every vanilla room");
            }

            var extra = entry.Payload[0] == 0x02 ? entry.Payload[3..] : entry.Payload;
            if (extra.Length == 0)
                continue;

            if (seenExtraTypes.Add((entry.Area, screen, extra[0] & 0x0F)))
            {
                if (!specials.Extras.TryGetValue((entry.Area, screen), out var list))
                    specials.Extras[(entry.Area, screen)] = list = [];
                list.Add(extra);
            }
        }

        return specials;
    }

    private static void AddElevatorEntries(GeneratedWorld world, Dictionary<Area, Dictionary<Point, TableEntry>> entries)
    {
        var grid = world.Grid;

        foreach (var link in grid.Links.Where(l => l.Type == LinkType.Elevator))
        {
            var top = grid.Cell(link.A)!;     // the upper area's elevator room
            var spacer = grid.Cell(link.B)!;  // the 0x01 pass-through below it
            var platform = grid.Cell(link.B.Step(Direction.Down))
                ?? throw new InvalidOperationException($"elevator at {link.A} has no platform cell");
            byte lowerArea = (byte)(int)spacer.Area;

            AddElevator(entries, top.Area, top.Position, lowerArea);
            AddElevator(entries, spacer.Area, top.Position, 0x81);
            AddElevator(entries, spacer.Area, platform.Position, (byte)(0x80 | lowerArea));
        }

        if (!world.Landmarks.TryGetValue("EscapeShaft", out var escapeTop))
            throw new InvalidOperationException("generated world has no EscapeShaft landmark");
        AddElevator(entries, Area.Tourian, escapeTop, 0x8F);
    }

    private static void AddElevator(Dictionary<Area, Dictionary<Point, TableEntry>> entries,
        Area area, Point cell, byte data)
    {
        if (!entries[area].TryGetValue(cell, out var entry))
            entries[area][cell] = entry = new TableEntry { Cell = cell };
        entry.Payload.AddRange([0x04, data]);
    }

    private static byte[] BuildGrid(WorldGrid grid)
    {
        var bytes = new byte[WorldGrid.Size * WorldGrid.Size];
        Array.Fill(bytes, (byte)0xFF);
        foreach (var cell in grid.Cells)
            bytes[cell.Position.Y * WorldGrid.Size + cell.Position.X] = (byte)cell.AssignedScreen!.ScreenId;
        return bytes;
    }

    /// <summary>
    /// Serializes the five area tables into one blob at <see cref="TableAddress"/>, adds the
    /// repointing patches for each area's $9598 table pointer, and records the PC address of
    /// every power-up type byte.
    /// </summary>
    private static byte[] SerializeTables(Dictionary<Area, Dictionary<Point, TableEntry>> entries,
        Dictionary<int, byte[]> patches, Dictionary<Point, int> itemAddresses)
    {
        var blob = new List<byte>();

        foreach (var area in TableAreas)
        {
            int tableStart = blob.Count;
            patches[NesToPc(area, TablePointerNes)] = [(byte)(0x8000 + tableStart), (byte)((0x8000 + tableStart) >> 8)];

            var rows = entries[area].Values
                .GroupBy(e => e.Cell.Y)
                .OrderBy(g => g.Key)
                .Select(g => (Y: g.Key, Entries: g.OrderBy(e => e.Cell.X).ToList()))
                .ToList();
            if (rows.Count == 0)
                throw new InvalidOperationException($"{area} has no special items at all");

            // First pass: compute each row's blob offset so its header can point at the next row.
            // Row layout: [Y][nextRowLo][nextRowHi] + per-entry [X][nextEntryOffset][payload][$00].
            var rowStarts = new int[rows.Count];
            int rowOffset = tableStart;
            for (int i = 0; i < rows.Count; i++)
            {
                rowStarts[i] = rowOffset;
                rowOffset += 3 + rows[i].Entries.Sum(e => 2 + e.Payload.Count + 1);
            }

            for (int i = 0; i < rows.Count; i++)
            {
                var (y, rowEntries) = rows[i];
                int nextRow = i + 1 < rows.Count ? 0x8000 + rowStarts[i + 1] : 0xFFFF;
                blob.Add((byte)y);
                blob.Add((byte)nextRow);
                blob.Add((byte)(nextRow >> 8));

                for (int j = 0; j < rowEntries.Count; j++)
                {
                    var entry = rowEntries[j];
                    blob.Add((byte)entry.Cell.X);
                    blob.Add(j + 1 < rowEntries.Count ? (byte)(2 + entry.Payload.Count + 1) : (byte)0xFF);
                    if (entry.ItemOffset.HasValue)
                        itemAddresses[entry.Cell] = TableAddress + blob.Count + entry.ItemOffset.Value;
                    blob.AddRange(entry.Payload);
                    blob.Add(0x00);
                }
            }
        }

        return [.. blob];
    }

    /// <summary>
    /// Composes the automap payload for the generated map and patches the bounds, seed
    /// identity, and tilemap planes of the base ROM's M1MP block. The changed seed
    /// identity makes the runtime reset stale explored-map SRAM for the new layout.
    /// </summary>
    private static void AddAutomap(GeneratedWorld world, Dictionary<int, byte[]> patches)
    {
        var automap = AutomapComposer.Compose(world);
        patches[AutomapComposer.BoundsAddress] = automap.Bounds;
        patches[AutomapComposer.SeedIdAddress] =
        [
            (byte)automap.SeedId, (byte)(automap.SeedId >> 8),
            (byte)(automap.SeedId >> 16), (byte)(automap.SeedId >> 24),
        ];
        patches[AutomapComposer.TilemapsAddress] = automap.Planes;
    }

    private static void AddStartPositions(GeneratedWorld world, Dictionary<int, byte[]> patches,
        bool respawnAtPortal)
    {
        var grid = world.Grid;
        var startArea = grid.Cell(world.Start)!.Area;

        // Continuing a game respawns Samus at the current area's $95D7/$95D8 map
        // position: the spawn cell for the start area, the elevator platform for areas
        // entered from above. Brinstar is never an elevator destination, so when the
        // start is elsewhere it falls back to a Brinstar-side elevator host cell.
        var respawn = new Dictionary<Area, Point> { [startArea] = world.Start };
        foreach (var link in grid.Links.Where(l => l.Type == LinkType.Elevator))
        {
            var lower = grid.Cell(link.B)!.Area;
            respawn.TryAdd(lower, link.B.Step(Direction.Down));
        }
        bool brinstarFallback = !respawn.ContainsKey(Area.Brinstar);
        if (brinstarFallback)
        {
            respawn[Area.Brinstar] = grid.Links
                .First(l => l.Type == LinkType.Elevator && grid.Cell(l.A)!.Area == Area.Brinstar).A;
        }

        // Combo seeds that don't cold-boot into M1 are entered through the portal, so a
        // Brinstar death respawns in the portal room itself rather than at the start
        // cell — continuing keeps the player where they came into the map.
        bool portalRespawn = respawnAtPortal && world.Landmarks.ContainsKey("Portal0");
        if (portalRespawn)
            respawn[Area.Brinstar] = world.Landmarks["Portal0"];

        // The third byte ($95D9) is the in-screen spawn height: the engine drops Samus
        // at center X falling from this Y. Mid-screen $6E (the vanilla value in every
        // non-Brinstar bank, so those areas need only 2 bytes) clears the ceiling and
        // lands her on the floor; Brinstar's vanilla $B0 only suits the spawn-platform
        // screen, so any other Brinstar respawn cell overrides it.
        foreach (var area in TableAreas)
        {
            var point = respawn[area];
            patches[NesToPc(area, StartPositionNes)] = area == Area.Brinstar && (portalRespawn || brinstarFallback)
                ? [(byte)point.X, (byte)point.Y, 0x6E]
                : [(byte)point.X, (byte)point.Y];
        }

        // The boot config: the area the seed cold-starts into, and each area's respawn
        // room orientation (0 horizontal, 1 vertical — SamusInit picks scroll direction
        // and PPU mirroring from it on every spawn). Indexed by the InArea/enum order,
        // NOT by TableAreas, whose bank order swaps Kraid and Tourian.
        patches[RomWriter.StartAreaConfigAddress] = [(byte)startArea];
        var orientation = new byte[5];
        foreach (var (area, point) in respawn)
            orientation[(int)area] = grid.Cell(point)!.Run.Axis == Scrolling.Vertical ? (byte)1 : (byte)0;
        patches[RomWriter.StartAreaConfigAddress + 1] = orientation;
    }
}
