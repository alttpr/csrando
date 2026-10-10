namespace Randomizer.Games.Metroid.MapGen;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using static Randomizer.Games.Metroid.YamlReader;

/// <summary>
/// The composed automap payload for one generated M1 world: five 32x32 planes of
/// SNES BG3 tilemap words, the per-area view bounds, and the seed identity hash
/// the runtime uses to reset stale explored-map SRAM. Pure data; a thin patch step
/// in <see cref="RomEmitter"/> applies it through the normal ROM abstraction.
/// </summary>
public sealed class AutomapPayload
{
    /// <summary>Five planes in area order, each 1024 little-endian words (0x2800 bytes total).</summary>
    public required byte[] Planes { get; init; }

    /// <summary>Four bytes per area: minX, maxX, minY, maxY ($FF,$00,$FF,$00 when empty).</summary>
    public required byte[] Bounds { get; init; }

    /// <summary>FNV-1a hash over <see cref="Planes"/> followed by <see cref="Bounds"/>.</summary>
    public required uint SeedId { get; init; }
}

/// <summary>
/// The automap glyph lookup deserialized from m1_map_tiles.json — a snapshot of
/// asm/multirando-asm/src/data/m1_map_tiles.json, the source of truth for the map
/// graphics assembled into the base ROM. Tile numbers, flip flags and connection
/// rules all come from this file; nothing is duplicated in code.
/// </summary>
public sealed class AutomapTiles
{
    /// <summary>Connection bit values (from connectionBits: north=1, east=2, south=4, west=8).</summary>
    public int NorthBit { get; }
    public int EastBit { get; }
    public int SouthBit { get; }
    public int WestBit { get; }

    /// <summary>BG word attributes ORed into every glyph: priority + the palette-3 base ($2C00).</summary>
    public ushort Attributes { get; }
    public ushort CharacterMask { get; }
    public ushort VFlipMask { get; }

    public int HeaderSnesAddress { get; }
    public int AreaBoundsOffset { get; }
    public int TilemapsSnesAddress { get; }
    public int BytesPerArea { get; }
    public int AreaCount { get; }

    /// <summary>First tile number of the plain item glyph group and the number of tiles in it.
    /// The energy/major groups mirror the item group tile-for-tile, so
    /// <c>base + (char - ItemTileBase)</c> converts an item glyph to its tiered variant.</summary>
    public int ItemTileBase { get; }
    public int ItemTileCount { get; }
    public int EnergyTileBase { get; }
    public int MajorTileBase { get; }

    private readonly Dictionary<int, ushort> topology;
    private readonly Dictionary<int, ushort> items;
    private readonly Dictionary<int, ushort> energyItems;
    private readonly Dictionary<int, ushort> majorItems;
    private readonly Dictionary<int, ushort> portals;
    private readonly Dictionary<int, ushort> mapStations;
    private readonly Dictionary<(int Connections, int Doors), ushort> doors;
    private readonly Dictionary<int, ushort> bosses;
    private readonly ushort elevatorEntranceWest;
    private readonly ushort elevatorEntranceEast;
    private readonly ushort elevatorShaftTop;
    private readonly ushort elevatorShaft;

    public static AutomapTiles Load() => cached.Value;

    private static Lazy<AutomapTiles> cached = new(() =>
        new AutomapTiles(File.ReadAllText(Path.Combine(DataRoot, "m1_map_tiles.json"))));

    // The JSON file's shape, deserialized directly. Only the fields the composer
    // consumes are declared; `required` makes a missing field a load-time error
    // instead of a silent zero.
    private sealed record GlyphDto(int Connections, int Word);
    private sealed record DoorDto(int Connections, int Doors, int Word);
    private sealed record BossDto(int Doors, int Word);
    private sealed record ElevatorDto(string Kind, string? SideDoor, int Word);
    private sealed record FeaturesDto(
        List<GlyphDto> Item, List<GlyphDto> Portal, List<GlyphDto> Mapstation,
        List<GlyphDto> Energy, List<GlyphDto> Major);
    private sealed record GroupTilesDto(int Item, int Energy, int Major);
    private sealed record RomMapDto(
        int HeaderSnesAddress, int AreaBoundsOffset, int TilemapsSnesAddress,
        int Width, int Height, int EntryBytes, int AreaBytes, List<string> AreaOrder);
    private sealed record ConnectionBitsDto(int North, int East, int South, int West);
    private sealed record TilemapWordDto(int CharacterMask, int PaletteShift, int PriorityMask, int VFlipMask);
    private sealed class FileDto
    {
        public required RomMapDto RomMap { get; init; }
        public required ConnectionBitsDto ConnectionBits { get; init; }
        public required TilemapWordDto TilemapWord { get; init; }
        public required GroupTilesDto GroupTileBases { get; init; }
        public required GroupTilesDto GroupTileCounts { get; init; }
        public required List<GlyphDto> Topology { get; init; }
        public required FeaturesDto Features { get; init; }
        public required List<ElevatorDto> Elevators { get; init; }
        public required List<BossDto> Bosses { get; init; }
        public required List<DoorDto> Doors { get; init; }
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AutomapTiles(string json)
    {
        var file = JsonSerializer.Deserialize<FileDto>(json, JsonOptions)
            ?? throw new InvalidDataException("m1_map_tiles.json is empty");

        HeaderSnesAddress = file.RomMap.HeaderSnesAddress;
        AreaBoundsOffset = file.RomMap.AreaBoundsOffset;
        TilemapsSnesAddress = file.RomMap.TilemapsSnesAddress;
        BytesPerArea = file.RomMap.AreaBytes;

        // The composer assumes the runtime grid dimensions; anything else means the
        // snapshot and the composer disagree and the payload would be garbage.
        if (file.RomMap.Width != WorldGrid.Size || file.RomMap.Height != WorldGrid.Size
            || file.RomMap.EntryBytes != 2 || BytesPerArea != WorldGrid.Size * WorldGrid.Size * 2)
        {
            throw new InvalidDataException("m1_map_tiles.json map dimensions do not match the 32x32 word grid");
        }

        AreaCount = file.RomMap.AreaOrder.Count;
        for (int i = 0; i < AreaCount; i++)
        {
            if (!Enum.TryParse<Area>(file.RomMap.AreaOrder[i], out var area) || (int)area != i)
            {
                throw new InvalidDataException(
                    $"m1_map_tiles.json area order '{file.RomMap.AreaOrder[i]}' does not match Area enum index {i}");
            }
        }

        NorthBit = file.ConnectionBits.North;
        EastBit = file.ConnectionBits.East;
        SouthBit = file.ConnectionBits.South;
        WestBit = file.ConnectionBits.West;

        CharacterMask = (ushort)file.TilemapWord.CharacterMask;
        VFlipMask = (ushort)file.TilemapWord.VFlipMask;
        // Palette 3 is the standard overlay text palette; the runtime swaps it per
        // exploration state. Priority keeps the map above the dimmed game layer.
        Attributes = (ushort)(file.TilemapWord.PriorityMask | 3 << file.TilemapWord.PaletteShift);

        ItemTileBase = file.GroupTileBases.Item;
        ItemTileCount = file.GroupTileCounts.Item;
        EnergyTileBase = file.GroupTileBases.Energy;
        MajorTileBase = file.GroupTileBases.Major;
        if (file.GroupTileCounts.Energy != ItemTileCount || file.GroupTileCounts.Major != ItemTileCount)
            throw new InvalidDataException("m1_map_tiles.json energy/major glyph groups do not mirror the item group");

        topology = file.Topology.ToDictionary(g => g.Connections, g => (ushort)g.Word);
        items = file.Features.Item.ToDictionary(g => g.Connections, g => (ushort)g.Word);
        energyItems = file.Features.Energy.ToDictionary(g => g.Connections, g => (ushort)g.Word);
        majorItems = file.Features.Major.ToDictionary(g => g.Connections, g => (ushort)g.Word);
        portals = file.Features.Portal.ToDictionary(g => g.Connections, g => (ushort)g.Word);
        mapStations = file.Features.Mapstation.ToDictionary(g => g.Connections, g => (ushort)g.Word);
        doors = file.Doors.ToDictionary(d => (d.Connections, d.Doors), d => (ushort)d.Word);
        bosses = file.Bosses.ToDictionary(b => b.Doors, b => (ushort)b.Word);

        ushort Elevator(string kind, string? sideDoor = null) =>
            (ushort?)file.Elevators.FirstOrDefault(e => e.Kind == kind && e.SideDoor == sideDoor)?.Word
                ?? throw new InvalidDataException($"missing elevator glyph '{kind}' ({sideDoor ?? "no side door"})");
        elevatorEntranceWest = Elevator("entrance", "west");
        elevatorEntranceEast = Elevator("entrance", "east");
        elevatorShaftTop = Elevator("shaftTop");
        elevatorShaft = Elevator("shaft");
    }

    private static ushort Require(Dictionary<int, ushort> group, int connections, string what) =>
        group.TryGetValue(connections, out var word) ? word
            : throw new InvalidOperationException($"no {what} map glyph for connection mask {connections}");

    public ushort Topology(int connections) => Require(topology, connections, "topology");
    public ushort Item(int connections) => Require(items, connections, "item");
    public ushort Energy(int connections) => Require(energyItems, connections, "energy item");
    public ushort Major(int connections) => Require(majorItems, connections, "major item");
    public ushort Portal(int connections) => Require(portals, connections, "portal");
    public ushort MapStation(int connections) => Require(mapStations, connections, "map station");

    public ushort Door(int connections, int doorMask) =>
        doors.TryGetValue((connections, doorMask), out var word) ? word
            : throw new InvalidOperationException($"no door map glyph for connections {connections} doors {doorMask}");

    public ushort Boss(int doorMask) =>
        bosses.TryGetValue(doorMask, out var word) ? word
            : throw new InvalidOperationException($"no boss map glyph for door mask {doorMask}");

    public ushort ElevatorEntrance(bool doorEast) => doorEast ? elevatorEntranceEast : elevatorEntranceWest;
    public ushort ElevatorShaftTop => elevatorShaftTop;
    public ushort ElevatorShaft => elevatorShaft;

    /// <summary>
    /// Overlay-font text marker for an area's initial. The font (small_overlay.tbl)
    /// maps 'A'-'Z' to characters $10-$29; characters below $100 always render with
    /// their own palette, which is what makes elevator destinations readable before
    /// the area is explored.
    /// </summary>
    public ushort AreaMarker(Area area) =>
        (ushort)(0x10 + (area.ToString()[0] - 'A') | Attributes);
}

/// <summary>
/// Composes the automap payload from the final generated world. Cell glyphs follow
/// the vanilla map conventions verified from the base ROM's vanilla payload:
///
/// - Normal cells use the topology/door lookups keyed by their open edges; scroll
///   and door edges both count as connections, doors only on east/west (the engine
///   and the graphics support no others).
/// - Item cells use the item glyph; their door edges still render on the adjacent
///   cell's own glyph (the JSON sharedDoorRule). Boss cells use the boss glyph
///   keyed by door mask; portal rooms use the portal glyph. Map-station cells use
///   the map-station glyph, which the runtime renders as an always-visible landmark
///   so players can find the map pickup on an unexplored layout.
/// - An elevator draws six cells across the two planes involved, matching the
///   vanilla Norfair-Ridley column: the upper plane shows the entrance room (side
///   door east or west), a shaft segment below it, and a text marker naming the
///   destination area; the lower plane shows a closed-top shaft cell at the upper
///   room's coordinate (the runtime switches areas mid-ride, so Samus can briefly
///   occupy it), and shaft segments at the spacer and platform. The platform uses
///   the shaft glyph rather than the flipped entrance glyph: generated platforms
///   always scroll onward into the entrance run below, and the entrance glyph
///   would draw a side door that does not exist.
/// - The Tourian escape shaft top renders as the closed-top shaft cell, like the
///   vanilla payload draws it.
/// - Cap cells are solid wall screens Samus can never occupy: blank.
/// </summary>
public static class AutomapComposer
{
    /// <summary>
    /// PC address of the SNES bank-$98 data window (RomEmitter's item-table window,
    /// $98:8000). All automap addresses are offsets from it, taken from the JSON.
    /// </summary>
    private const int WindowSnesBase = 0x988000;

    public static int SnesToPc(int snesAddress) => RomEmitter.TableAddress + (snesAddress - WindowSnesBase);

    public static int HeaderAddress => SnesToPc(AutomapTiles.Load().HeaderSnesAddress);
    public static int SeedIdAddress => HeaderAddress + 0x0C;
    public static int BoundsAddress => HeaderAddress + AutomapTiles.Load().AreaBoundsOffset;
    public static int TilemapsAddress => SnesToPc(AutomapTiles.Load().TilemapsSnesAddress);

    /// <summary>
    /// The M1MapInitialReveal byte directly after the bounds records (map_data.asm): the
    /// revealed-areas mask applied when the automap state resets. Vanilla-layout seeds
    /// write <see cref="RevealAllAreas"/> here; generated maps keep the assembled $00 and
    /// reveal areas through their map-station pickups.
    /// </summary>
    public static int InitialRevealAddress => BoundsAddress + AutomapTiles.Load().AreaCount * 4;

    /// <summary>All five area bits (M1MapAreaBits $01|$02|$04|$08|$10).</summary>
    public const byte RevealAllAreas = 0x1F;

    public static AutomapPayload Compose(GeneratedWorld world) => Compose(world, AutomapTiles.Load());

    public static AutomapPayload Compose(GeneratedWorld world, AutomapTiles tiles)
    {
        var grid = world.Grid;
        var planes = new ushort[tiles.AreaCount, WorldGrid.Size * WorldGrid.Size];
        var handled = new HashSet<Point>();

        void Set(Area area, Point p, ushort word)
        {
            if (!WorldGrid.InBounds(p))
                throw new InvalidOperationException($"automap cell {p} out of bounds");
            int index = p.Y * WorldGrid.Size + p.X;
            if (planes[(int)area, index] != 0)
                throw new InvalidOperationException($"automap cell {p} written twice in {area}");
            planes[(int)area, index] = word;
        }

        ushort Glyph(ushort lookupWord) => (ushort)(lookupWord | tiles.Attributes);

        foreach (var link in grid.Links.Where(l => l.Type == LinkType.Elevator))
        {
            var top = grid.Cell(link.A)!;
            var spacer = grid.Cell(link.B)!;
            var platform = grid.Cell(link.B.Step(Direction.Down))
                ?? throw new InvalidOperationException($"elevator at {link.A} has no platform cell");

            bool doorEast = top.Right == EdgeRequirement.Door;
            if (doorEast == (top.Left == EdgeRequirement.Door))
                throw new InvalidOperationException($"elevator room {top} must have exactly one side door");

            Set(top.Area, top.Position, Glyph(tiles.ElevatorEntrance(doorEast)));
            Set(top.Area, spacer.Position, Glyph(tiles.ElevatorShaft));
            Set(top.Area, platform.Position, tiles.AreaMarker(spacer.Area));

            Set(spacer.Area, top.Position, Glyph(tiles.ElevatorShaftTop));
            Set(spacer.Area, spacer.Position, Glyph(tiles.ElevatorShaft));
            Set(spacer.Area, platform.Position, Glyph(tiles.ElevatorShaft));

            handled.Add(top.Position);
            handled.Add(spacer.Position);
            handled.Add(platform.Position);
        }

        // The escape elevator has no grid link (its exit faces a reserved-empty cell);
        // vanilla draws its boarding room as the closed-top shaft piece.
        if (!world.Landmarks.TryGetValue("EscapeShaft", out var escapeTop))
            throw new InvalidOperationException("generated world has no EscapeShaft landmark");
        Set(grid.Cell(escapeTop)!.Area, escapeTop, Glyph(tiles.ElevatorShaftTop));
        handled.Add(escapeTop);

        foreach (var cell in grid.Cells)
        {
            if (cell.Role == CellRole.Cap || handled.Contains(cell.Position))
                continue;

            int connections = 0, doorMask = 0;
            foreach (var dir in Directions.All)
            {
                var edge = cell.Edge(dir);
                if (edge == EdgeRequirement.Wall)
                    continue;
                if (edge == EdgeRequirement.Elevator)
                    throw new InvalidOperationException($"unhandled elevator edge at {cell}");
                // A scroll edge toward a cap is a wall in the fitted screen; the cap
                // itself is blank, so the connection must not be drawn either.
                if (grid.Cell(cell.Position.Step(dir))?.Role == CellRole.Cap)
                    continue;

                int bit = dir switch
                {
                    Direction.Up => tiles.NorthBit,
                    Direction.Down => tiles.SouthBit,
                    Direction.Left => tiles.WestBit,
                    _ => tiles.EastBit,
                };
                connections |= bit;
                if (edge == EdgeRequirement.Door)
                    doorMask |= bit;
            }

            ushort word = cell.Role switch
            {
                CellRole.Boss => tiles.Boss(doorMask),
                CellRole.Portal => tiles.Portal(connections),
                CellRole.Item => tiles.Item(connections),
                CellRole.MapStation => tiles.MapStation(connections),
                _ when doorMask != 0 => tiles.Door(connections, doorMask),
                _ => tiles.Topology(connections),
            };
            Set(cell.Area, cell.Position, Glyph(word));
        }

        var planeBytes = new byte[tiles.AreaCount * tiles.BytesPerArea];
        var bounds = new byte[tiles.AreaCount * 4];
        for (int area = 0; area < tiles.AreaCount; area++)
        {
            int minX = 0xFF, maxX = 0x00, minY = 0xFF, maxY = 0x00;
            for (int i = 0; i < WorldGrid.Size * WorldGrid.Size; i++)
            {
                ushort word = planes[area, i];
                planeBytes[area * tiles.BytesPerArea + i * 2] = (byte)word;
                planeBytes[area * tiles.BytesPerArea + i * 2 + 1] = (byte)(word >> 8);
                if ((word & tiles.CharacterMask) == 0)
                    continue;
                minX = Math.Min(minX, i % WorldGrid.Size);
                maxX = Math.Max(maxX, i % WorldGrid.Size);
                minY = Math.Min(minY, i / WorldGrid.Size);
                maxY = Math.Max(maxY, i / WorldGrid.Size);
            }
            bounds[area * 4] = (byte)minX;
            bounds[area * 4 + 1] = (byte)maxX;
            bounds[area * 4 + 2] = (byte)minY;
            bounds[area * 4 + 3] = (byte)maxY;
        }

        return new AutomapPayload
        {
            Planes = planeBytes,
            Bounds = bounds,
            SeedId = SeedHash(planeBytes, bounds),
        };
    }

    /// <summary>
    /// 32-bit FNV-1a over the payload bytes. The runtime only compares the id against
    /// the copy in SRAM to detect a different seed, so any deterministic, payload-
    /// sensitive hash serves; it never needs to match the zlib CRC-32 the ASM build
    /// stamps on the vanilla payload.
    /// </summary>
    public static uint SeedHash(params byte[][] buffers)
    {
        uint hash = 2166136261u;
        foreach (var buffer in buffers)
        {
            foreach (byte b in buffer)
                hash = (hash ^ b) * 16777619u;
        }

        return hash;
    }
}
