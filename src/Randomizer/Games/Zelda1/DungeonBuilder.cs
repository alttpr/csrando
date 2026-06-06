namespace Randomizer.Games.Zelda1;

using Randomizer.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

internal record DungeonConfig
{
    public int Width { get; init; } = 4;
    public int Height { get; init; } = 4;
    public int Rooms { get; init; } = 8;
    public int Segments { get; init; } = 1;
    public int ItemCellars { get; init; } = 1;

    /// <summary>
    /// Number of non-key item slots beyond map/compass. -1 means auto-scale based on room count.
    /// </summary>
    public int ExtraItemSlots { get; init; } = -1;

    /// <summary>Chance (0.0–1.0) that a room gets no enemies at all.</summary>
    public double EmptyRoomChance { get; init; } = 0.10;

    /// <summary>Probability modifier for non-open doors (locked/bombable/shutter). Higher = more complex navigation.</summary>
    public double DoorComplexity { get; init; } = 1.0;

    /// <summary>Controls how enemies are filtered for placement in generated dungeons.</summary>
    public EnemyPlacementOption EnemyPlacement { get; init; } = EnemyPlacementOption.Progressive;

    /// <summary>Controls whether generated dungeon items are hidden behind clearing the room.</summary>
    public HiddenItemsOption HiddenItems { get; init; } = HiddenItemsOption.Sometimes;

    // Baseline configs per level (Progressive style, no fuzz)

    private static readonly DungeonConfig[] BaseConfigs =
    [
        new() { Width = 5, Height = 5, Rooms = 12, Segments = 1, ItemCellars = 1 },   // Level 1
        new() { Width = 5, Height = 5, Rooms = 14, Segments = 1, ItemCellars = 1 },   // Level 2
        new() { Width = 6, Height = 6, Rooms = 18, Segments = 1, ItemCellars = 1 },   // Level 3
        new() { Width = 6, Height = 6, Rooms = 22, Segments = 1, ItemCellars = 1 },   // Level 4
        new() { Width = 7, Height = 7, Rooms = 26, Segments = 2, ItemCellars = 1 },   // Level 5
        new() { Width = 7, Height = 7, Rooms = 30, Segments = 2, ItemCellars = 1 },   // Level 6
        new() { Width = 8, Height = 8, Rooms = 36, Segments = 2, ItemCellars = 1 },   // Level 7
        new() { Width = 8, Height = 8, Rooms = 40, Segments = 2, ItemCellars = 2 },   // Level 8
        new() { Width = 8, Height = 8, Rooms = 56, Segments = 3, ItemCellars = 2 },   // Level 9
    ];

    /// <summary>
    /// Generates a DungeonConfig for the given level and style, with optional random fuzz.
    /// </summary>
    public static DungeonConfig GetConfigForLevel(int level, DungeonStyleOption style, EnemyPlacementOption enemyPlacement, Random rnd, HiddenItemsOption hiddenItems = HiddenItemsOption.Sometimes)
    {
        var config = style switch
        {
            DungeonStyleOption.Progressive => ApplyFuzz(BaseConfigs[level - 1], rnd, fuzzRooms: 2, fuzzSegments: false),
            DungeonStyleOption.Wild => GenerateWild(rnd),
            DungeonStyleOption.Megadungeon => GenerateMegadungeon(level, rnd),
            DungeonStyleOption.Nightmare => GenerateNightmare(rnd),
            DungeonStyleOption.Minimal => GenerateMinimal(),
            _ => BaseConfigs[level - 1],
        };
        // Nightmare defaults to Random enemies, but the explicit setting takes priority for other styles
        if (style != DungeonStyleOption.Nightmare)
            config = config with { EnemyPlacement = enemyPlacement };
        return config with { HiddenItems = hiddenItems };
    }

    private static DungeonConfig ApplyFuzz(DungeonConfig baseConfig, Random rnd,
        int fuzzRooms = 2, bool fuzzSegments = false, double emptyChance = 0.10, double doorComplexity = 1.0)
    {
        int rooms = baseConfig.Rooms + rnd.Next(-fuzzRooms, fuzzRooms + 1);
        int segments = baseConfig.Segments;
        if (fuzzSegments && rnd.NextDouble() < 0.3)
            segments = Math.Clamp(segments + rnd.Next(-1, 2), 1, 3);

        int cellars = baseConfig.ItemCellars;
        if (rnd.NextDouble() < 0.2)
            cellars = Math.Clamp(cellars + rnd.Next(-1, 2), 0, 3);

        // Reserve enough free grid slots for every cellar that will be created after layout:
        // (segments - 1) connector cellars + item cellars + 1 spare.
        int maxRooms = baseConfig.Width * baseConfig.Height;
        int reserved = (segments - 1) + cellars + 1;
        rooms = Math.Clamp(rooms, 6, maxRooms - reserved);

        return baseConfig with
        {
            Rooms = rooms,
            Segments = segments,
            ItemCellars = cellars,
            EmptyRoomChance = emptyChance,
            DoorComplexity = doorComplexity,
        };
    }

    private static DungeonConfig GenerateWild(Random rnd)
    {
        int size = rnd.Next(5, 9);
        int segments = rnd.Next(1, 4);
        int cellars = rnd.Next(0, 3);
        int reserved = (segments - 1) + cellars + 1;
        int rooms = rnd.Next(10, size * size - reserved);
        double emptyChance = 0.05 + rnd.NextDouble() * 0.15;
        double doorComplexity = 0.7 + rnd.NextDouble() * 0.8;

        return new DungeonConfig
        {
            Width = size,
            Height = size,
            Rooms = rooms,
            Segments = segments,
            ItemCellars = cellars,
            EmptyRoomChance = emptyChance,
            DoorComplexity = doorComplexity,
        };
    }

    private static DungeonConfig GenerateMegadungeon(int level, Random rnd)
    {
        var baseConfig = BaseConfigs[Math.Max(level - 1, 5)]; // floor at level 6 baseline
        return ApplyFuzz(baseConfig with
        {
            Width = 8,
            Height = 8,
            Rooms = Math.Max(baseConfig.Rooms + 10, 36) + rnd.Next(0, 8),
            Segments = Math.Max(baseConfig.Segments, 2),
            ItemCellars = Math.Max(baseConfig.ItemCellars, 2),
        }, rnd, fuzzRooms: 4, fuzzSegments: true, emptyChance: 0.05, doorComplexity: 1.3);
    }

    private static DungeonConfig GenerateNightmare(Random rnd)
    {
        int itemCellars = 2 + rnd.Next(0, 2);
        int segments = 3;
        int reserved = 62 - itemCellars - segments;
        int rooms = Math.Max(Math.Min(48 + rnd.Next(0, 16), 56), reserved);

        return new DungeonConfig
        {
            Width = 8,
            Height = 8,
            Rooms = rooms,
            Segments = 3,
            ItemCellars = itemCellars,
            EmptyRoomChance = 0.02,
            DoorComplexity = 1.6 + rnd.NextDouble() * 0.4,
            EnemyPlacement = EnemyPlacementOption.Random,
        };
    }

    private static DungeonConfig GenerateMinimal()
    {
        return new DungeonConfig
        {
            Width = 4,
            Height = 4,
            // Needs enough item-eligible rooms (non start/boss/end/connector) to host every
            // dungeon's Map + Compass + key(s); 8 leaves headroom even for L9, whose boss room
            // doesn't double as an item room. See AssignItems' requiredItems check.
            Rooms = 8,
            Segments = 1,
            ItemCellars = 0,
            ExtraItemSlots = -1,
            EmptyRoomChance = 0.50,
            DoorComplexity = 0.0,
        };
    }
}

internal class DungeonBuilder
{
    internal sealed record GeneratedMapData(
        int StartY,
        int SubmenuMapRotation,
        int StatusBarMapXOffset,
        int[] SubmenuMapMask,
        byte[] StatusBarMapTransferBuf
    );

    // Well-known screen IDs

    private static class ScreenId
    {
        public const int PushStairs = 0x1A;
        public const int PushCross = 0x20;
        public const int Lobby = 0x21;
        public const int LevelNineCheck = 0x26;
        public const int ZeldaRoom = 0x27;
        public const int GanonRoom = 0x28;
        public const int TriforceRoom = 0x29;
        public const int PassageCellar = 0x3E;
        public const int ItemCellar = 0x3F;
    }

    // Well-known enemy IDs

    private static class EnemyId
    {
        public const int Ganon = 0x3E;
        public const int ZeldaNpc = 0x37;
    }

    // Room role flags

    [Flags]
    private enum RoomRole
    {
        None           = 0,
        Start          = 1 << 0,
        End            = 1 << 1,
        Boss           = 1 << 2,
        Connector      = 1 << 3,
        Cellar         = 1 << 4,
        Item           = 1 << 5,
        Stairs         = 1 << 6,
        LevelNineCheck = 1 << 7,
        SegmentStart   = 1 << 8,
    }

    // Common role combinations

    /// <summary>Rooms that cannot have shutters assigned (all special-purpose rooms).</summary>
    private const RoomRole ShutterExcludedRoles =
        RoomRole.Start | RoomRole.End | RoomRole.Boss | RoomRole.Cellar |
        RoomRole.Stairs | RoomRole.Connector | RoomRole.LevelNineCheck;

    /// <summary>Rooms excluded when searching for end/boss room candidates.</summary>
    private const RoomRole EndCandidateExcludedRoles =
        RoomRole.Cellar | RoomRole.Connector | RoomRole.Start | RoomRole.LevelNineCheck;

    /// <summary>Rooms that should not receive regular enemy assignments.</summary>
    private const RoomRole EnemyExcludedRoles =
        RoomRole.Start | RoomRole.Cellar | RoomRole.End | RoomRole.Boss | RoomRole.LevelNineCheck;

    /// <summary>Mapping from RoomRole flags to display names for spoiler data.</summary>
    private static readonly (RoomRole role, string name)[] RoleNames =
    [
        (RoomRole.Start, "Start"), (RoomRole.End, "End"), (RoomRole.Boss, "Boss"),
        (RoomRole.Item, "Item"), (RoomRole.Connector, "Connector"), (RoomRole.Cellar, "Cellar"),
        (RoomRole.Stairs, "Stairs"), (RoomRole.LevelNineCheck, "LevelNineCheck"),
        (RoomRole.SegmentStart, "SegmentStart"),
    ];

    // Room

    private class Room
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool Used { get; set; }
        public RoomRole Role { get; set; } = RoomRole.None;
        public int Segment { get; set; } = -1;
        public int TargetSegment { get; set; } = -1;
        public int Screen { get; set; } = -1;
        public int EnemyId { get; set; } = -1;
        public int EnemyMode { get; set; }
        public int EnemyCount { get; set; }
        public YamlReader.RoomBehaviour Behaviour { get; set; } = YamlReader.RoomBehaviour.None;
        public bool PushBlock { get; set; }
        public List<Room> Neighbors { get; set; } = [];
        public Dictionary<string, YamlReader.DoorType> Doors { get; set; } = [];

        /// <summary>Returns true if the room has ANY of the specified roles.</summary>
        public bool HasAnyRole(RoomRole roles) => (Role & roles) != 0;
        /// <summary>Returns true if the room has ALL of the specified roles.</summary>
        public bool HasAllRoles(RoomRole roles) => (Role & roles) == roles;
        public void AddRole(RoomRole role) => Role |= role;
        public void RemoveRole(RoomRole role) => Role &= ~role;
    }

    // Map

    private class Map
    {
        public Room[,] Rooms;

        public Map(int width, int height)
        {
            Rooms = new Room[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    Rooms[x, y] = new Room { X = x, Y = y };
        }

        public IEnumerable<Room> AllRooms => Rooms.Cast<Room>();
        public IEnumerable<Room> UsedRooms => AllRooms.Where(r => r.Used);
        public IEnumerable<Room> UsedNonCellarRooms => UsedRooms.Where(r => !r.HasAnyRole(RoomRole.Cellar));

        public Room FindRoom(RoomRole role) => UsedRooms.First(r => r.HasAnyRole(role));
        public Room? FindRoomOrDefault(RoomRole role) => UsedRooms.FirstOrDefault(r => r.HasAnyRole(role));
        public IEnumerable<Room> FindRooms(RoomRole role) => UsedRooms.Where(r => r.HasAnyRole(role));
    }

    // Direction helpers

    private static readonly (int dx, int dy, string key)[] CardinalDirections =
        [(0, -1, "N"), (0, 1, "S"), (-1, 0, "W"), (1, 0, "E")];

    /// <summary>
    /// Returns the cardinal direction key ("N","S","W","E") from one room to an adjacent room.
    /// Returns "" for non-adjacent or same-position rooms.
    /// </summary>
    private static string GetDirection(Room from, Room to)
    {
        int dx = to.X - from.X;
        int dy = to.Y - from.Y;
        if (dy < 0) return "N";
        if (dy > 0) return "S";
        if (dx < 0) return "W";
        if (dx > 0) return "E";
        return "";
    }

    private static string OppositeDirection(string dir) => dir switch
    {
        "N" => "S", "S" => "N", "W" => "E", "E" => "W", _ => ""
    };

    private static YamlReader.Direction StringToYamlDirection(string dir) => dir switch
    {
        "N" => YamlReader.Direction.Up, "S" => YamlReader.Direction.Down,
        "W" => YamlReader.Direction.Left, "E" => YamlReader.Direction.Right,
        _ => throw new ArgumentException($"Invalid direction: {dir}")
    };

    private static string YamlDirectionToString(YamlReader.Direction dir) => dir switch
    {
        YamlReader.Direction.Up => "N", YamlReader.Direction.Down => "S",
        YamlReader.Direction.Left => "W", YamlReader.Direction.Right => "E",
        _ => ""
    };

    private static bool IsAdjacent(Room a, Room b) =>
        Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) == 1;

    private static int GeoDistance(Room a, Room b) =>
        Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

    // Instance state

    private readonly DungeonConfig _config;
    private readonly Random _rnd;
    private readonly Map _map;
    private readonly YamlReader.YamlData _data;
    private readonly int _level;
    private readonly YamlReader.Level _levelData;
    private readonly Dictionary<int, YamlReader.Screen> _underworldScreensById;
    private readonly HashSet<int> _killShutterScreens;
    private readonly HashSet<int> _pushBlockShutterScreens;
    private readonly HashSet<int> _pushBlockStairsScreens;
    private int[] _itemPositionSlots = [0x97, 0x97, 0x97, 0x97];
    private List<YamlReader.UnderworldMap>? _vanillaLevelMaps;
    private int[]? _defaultRoomPalettes;

    // Fixed center position used for triforce rooms and item cellars.
    // [6, 3] = center of the standard 12×7 room grid. Always in slot 0.
    private static readonly int FixedCenterPosition = EncodeItemPosition(6, 3);
    private const int FixedCenterPosSlot = 0;
    private const int GeneratedDungeonStartY = 0xDD;
    private const byte StatusBarMapTopTile = 0x67;
    private const byte StatusBarMapBottomTile = 0xFB;
    private const byte StatusBarMapFullTile = 0xFF;
    private const byte StatusBarMapGapTile = 0x24;
    private const byte StatusBarMapTransferTerminator = 0xFF;

    public DungeonBuilder(DungeonConfig config, YamlReader.YamlData yamlData, int level, PRNG prng)
    {
        _config = config;
        _rnd = new Random(prng.GetRandomInt(int.MaxValue));
        _map = new Map(_config.Width, _config.Height);
        _data = yamlData;
        _level = level;
        _levelData = _data.levels.First(l => l.level == _level && l.area == YamlReader.Area.Underworld);
        _underworldScreensById = _data.underworld_screens
            .Where(s => s.area == YamlReader.Area.Underworld)
            .GroupBy(s => s.screen)
            .ToDictionary(g => g.Key, g => g.First());
        _killShutterScreens = GetVanillaScreensForBehaviours(YamlReader.RoomBehaviour.KillForShutter, YamlReader.RoomBehaviour.KillForItem);
        _pushBlockShutterScreens = GetVanillaScreensForBehaviours(YamlReader.RoomBehaviour.PushBlockShutter);
        _pushBlockStairsScreens = GetVanillaScreensForBehaviours(YamlReader.RoomBehaviour.PushBlockStairs);
    }

    // ID helpers

    /// <summary>
    /// Returns a globally unique map ID for use in YamlData (graph building).
    /// Uses the range 0x100+ so generated maps don't collide with vanilla maps (0x00-0xFF).
    /// </summary>
    private int GetMapId(Room room)
    {
        int baseMapId = 0x100 + ((_level - 1) * 0x80);
        return baseMapId + GetLocalRoomId(room);
    }

    /// <summary>
    /// Returns the level-local room ID (0x00-0x7F) used by the NES game engine.
    /// Uses 16-column layout because NES engine hardcodes ±0x10 for N/S, ±1 for E/W.
    /// </summary>
    private static int GetLocalRoomId(Room room) => (room.Y * 16) + room.X;

    internal static GeneratedMapData BuildMapData(IEnumerable<int> visibleLocalRoomIds, int startY = GeneratedDungeonStartY)
    {
        var roomIds = visibleLocalRoomIds
            .Distinct()
            .OrderBy(id => id)
            .ToArray();

        if (roomIds.Length == 0)
        {
            return new GeneratedMapData(
                StartY: startY,
                SubmenuMapRotation: 0,
                StatusBarMapXOffset: 0,
                SubmenuMapMask: new int[16],
                StatusBarMapTransferBuf: CreateEmptyStatusBarMapTransferBuffer());
        }

        var coords = roomIds
            .Select(id => (X: id & 0x0F, Y: (id >> 4) & 0x0F))
            .ToArray();

        if (coords.Any(c => c.Y > 7))
            throw new InvalidOperationException("Generated dungeon map data expects room rows in the 0x00-0x7F range.");

        int minX = coords.Min(c => c.X);
        int maxX = coords.Max(c => c.X);
        int width = maxX - minX + 1;

        int submenuTargetMinX = (16 - width) / 2;
        int submenuRotation = Mod(submenuTargetMinX - minX, 16);

        var submenuMapMask = new int[16];
        foreach (var (x, y) in coords)
            submenuMapMask[x] |= 1 << (7 - y);
        submenuMapMask = RotateRight(submenuMapMask, submenuRotation);

        int statusBarTargetMinX = (8 - width) / 2;
        int statusBarMapXOffset = ((statusBarTargetMinX - minX) * 8) & 0xFF;
        byte[] statusBarMapTransferBuf = BuildStatusBarMapTransferBuffer(coords, minX, statusBarTargetMinX);

        return new GeneratedMapData(
            StartY: startY,
            SubmenuMapRotation: submenuRotation,
            StatusBarMapXOffset: statusBarMapXOffset,
            SubmenuMapMask: submenuMapMask,
            StatusBarMapTransferBuf: statusBarMapTransferBuf);
    }

    private static int Mod(int value, int modulus)
    {
        int result = value % modulus;
        return result < 0 ? result + modulus : result;
    }

    private static int[] RotateRight(IReadOnlyList<int> values, int amount)
    {
        if (values.Count == 0)
            return [];

        int rotation = Mod(amount, values.Count);
        if (rotation == 0)
            return values.ToArray();

        var rotated = new int[values.Count];
        for (int i = 0; i < values.Count; i++)
            rotated[(i + rotation) % values.Count] = values[i];
        return rotated;
    }

    private static byte[] CreateEmptyStatusBarMapTransferBuffer()
    {
        var buffer = new byte[45];
        Array.Fill(buffer, StatusBarMapTransferTerminator);
        return buffer;
    }

    private static byte[] BuildStatusBarMapTransferBuffer(IEnumerable<(int X, int Y)> coords, int minX, int targetMinX)
    {
        var occupancy = new bool[8, 8];
        foreach (var (x, y) in coords)
        {
            int projectedX = x - minX + targetMinX;
            if (projectedX is < 0 or >= 8)
                throw new InvalidOperationException($"Projected HUD map column {projectedX} was outside the supported 0-7 range.");

            occupancy[y, projectedX] = true;
        }

        var buffer = CreateEmptyStatusBarMapTransferBuffer();
        int writeIndex = 0;

        for (int rowPair = 0; rowPair < 4; rowPair++)
        {
            int topRow = rowPair * 2;
            int bottomRow = topRow + 1;
            int firstColumn = -1;
            int lastColumn = -1;
            var rowTiles = new byte[8];

            for (int x = 0; x < 8; x++)
            {
                bool topOccupied = occupancy[topRow, x];
                bool bottomOccupied = occupancy[bottomRow, x];

                rowTiles[x] = EncodeStatusBarMapTile(topOccupied, bottomOccupied);
                if (!topOccupied && !bottomOccupied)
                    continue;

                if (firstColumn == -1)
                    firstColumn = x;
                lastColumn = x;
            }

            if (firstColumn == -1)
                continue;

            int count = lastColumn - firstColumn + 1;
            if (writeIndex + 3 + count > buffer.Length - 1)
                throw new InvalidOperationException("Generated status bar map transfer buffer exceeded 45 bytes.");

            buffer[writeIndex++] = 0x20;
            buffer[writeIndex++] = (byte)(0x62 + rowPair * 0x20 + firstColumn);
            buffer[writeIndex++] = (byte)count;

            for (int x = firstColumn; x <= lastColumn; x++)
                buffer[writeIndex++] = rowTiles[x];
        }

        buffer[writeIndex] = StatusBarMapTransferTerminator;
        return buffer;
    }

    private static byte EncodeStatusBarMapTile(bool topOccupied, bool bottomOccupied) => (topOccupied, bottomOccupied) switch
    {
        (true, true) => StatusBarMapFullTile,
        (true, false) => StatusBarMapTopTile,
        (false, true) => StatusBarMapBottomTile,
        _ => StatusBarMapGapTile,
    };

    // Vanilla data helpers

    private List<YamlReader.UnderworldMap> GetVanillaLevelMaps()
    {
        if (_vanillaLevelMaps != null) return _vanillaLevelMaps;
        var levelRooms = _levelData.rooms.ToHashSet();
        _vanillaLevelMaps = _data.underworld_maps
            .Where(m => m.map < 0x100 && m.area == YamlReader.Area.Underworld && levelRooms.Contains(m.map))
            .ToList();
        return _vanillaLevelMaps;
    }

    private HashSet<int> GetVanillaScreensForBehaviours(params YamlReader.RoomBehaviour[] behaviours)
    {
        var behaviourSet = behaviours.Select(b => (int)b).ToHashSet();
        return _data.underworld_maps
            .Where(m => m.map < 0x100 && m.area == YamlReader.Area.Underworld && !m.passage && behaviourSet.Contains(m.behaviour))
            .Select(m => m.screen)
            .ToHashSet();
    }

    private int[] GetDefaultRoomPalettes()
    {
        if (_defaultRoomPalettes != null)
            return _defaultRoomPalettes;

        _defaultRoomPalettes = GetVanillaLevelMaps()
            .Where(m => !m.passage)
            .GroupBy(m => (Inner: m.palettes[0], Outer: m.palettes[1]))
            .OrderByDescending(g => g.Count())
            .Select(g => new[] { g.Key.Inner, g.Key.Outer })
            .FirstOrDefault()
            ?? [2, 2];

        return _defaultRoomPalettes;
    }

    private int[] GetRoomPalettes(int screenId, List<YamlReader.UnderworldMap> vanillaLevelMaps)
    {
        var exactMatch = vanillaLevelMaps.FirstOrDefault(m => !m.passage && m.screen == screenId);
        return exactMatch != null
            ? [.. exactMatch.palettes]
            : [.. GetDefaultRoomPalettes()];
    }

    // Item position logic

    private static int EncodeItemPosition(int x, int y) => ((x + 2) << 4) | (y + 6);

    private static bool IsValidItemPosition(YamlReader.Screen? screen, int encoded)
    {
        if (screen?.nodes?.regions == null) return false;
        int x = (encoded >> 4) - 2, y = (encoded & 0x0F) - 6;

        bool inWalkable = screen.nodes.regions.Any(r =>
            r.type == YamlReader.RegionType.Region &&
            r.from[0] <= x && r.from[1] <= y && r.to[0] >= x && r.to[1] >= y);
        if (!inWalkable) return false;

        bool blocked = screen.nodes.regions.Any(r =>
            r.type == YamlReader.RegionType.NoPlace &&
            r.from[0] <= x && r.from[1] <= y && r.to[0] >= x && r.to[1] >= y);
        if (blocked) return false;

        return screen.nodes.blocks?.Any(b => b[0] == x && b[1] == y) != true;
    }

    private static int GetItemPositionSlot(IReadOnlyList<int> slots, YamlReader.Screen? screen)
    {
        for (int i = 0; i < slots.Count; i++)
            if (IsValidItemPosition(screen, slots[i]))
                return i;
        throw new Exception($"No valid item position slot for screen {screen?.screen:X2}");
    }

    private YamlReader.Screen? GetUnderworldScreen(int screenId) => _underworldScreensById.GetValueOrDefault(screenId);

    private static bool ScreenHasMeta(YamlReader.Screen? screen, YamlReader.MetaType metaType)
        => screen?.nodes.meta?.Any(m => m.type == metaType) ?? false;

    private bool RoomNeedsGeneratedItemPosition(Room room)
        => room.HasAnyRole(RoomRole.Item) || room.Behaviour == YamlReader.RoomBehaviour.PushBlockStairs;

    private bool RoomHasVisibleStairs(Room room) => ScreenHasMeta(GetUnderworldScreen(room.Screen), YamlReader.MetaType.Stairs);

    private bool RoomSupportsHiddenStairs(Room room)
        => room.HasAnyRole(RoomRole.Stairs | RoomRole.Connector)
        && _pushBlockStairsScreens.Contains(room.Screen)
        && !RoomHasVisibleStairs(room);

    private bool RoomSupportsPushBlockShutters(Room room)
        => !room.HasAnyRole(ShutterExcludedRoles)
        && _pushBlockShutterScreens.Contains(room.Screen);

    private bool RoomSupportsKillShutters(Room room)
        => !room.HasAnyRole(ShutterExcludedRoles)
        && _killShutterScreens.Contains(room.Screen);

    private bool RoomCanUseAnyShutterTrigger(Room room)
        => RoomSupportsKillShutters(room) || RoomSupportsPushBlockShutters(room);

    private void SelectItemPositionSlots()
    {
        // Slot 0 is reserved for the fixed center position (triforce rooms, item cellars).
        // The greedy algorithm fills slots 1-3 for item-bearing rooms and hidden-stairs rooms.
        var selected = new List<int> { FixedCenterPosition };

        var screens = _map.UsedRooms
            .Where(RoomNeedsGeneratedItemPosition)
            .Where(r => !r.HasAnyRole(RoomRole.Cellar | RoomRole.Start | RoomRole.End))
            .Select(r => GetUnderworldScreen(r.Screen))
            .Where(s => s != null)
            .Cast<YamlReader.Screen>()
            .DistinctBy(s => s.screen)
            .ToArray();

        if (screens.Length == 0)
        {
            while (selected.Count < 4)
                selected.Add(FixedCenterPosition);
            _itemPositionSlots = [.. selected];
            return;
        }

        // Candidate positions: region centers, vanilla fallbacks, and every interior grid cell.
        // Including the full grid (not just region centers) gives the greedy set-cover below the
        // best chance to find a single position that is walkable across many screens, which is
        // what keeps us within the engine's 4 item-position slots.
        var gridCells = from x in Enumerable.Range(0, 12)
                        from y in Enumerable.Range(0, 7)
                        select EncodeItemPosition(x, y);

        var candidates = screens
            .SelectMany(s => s.nodes.regions
                .Where(r => r.type == YamlReader.RegionType.Region)
                .Select(r => EncodeItemPosition((r.from[0] + r.to[0]) / 2, (r.from[1] + r.to[1]) / 2)))
            .Concat(_levelData.shortcut_or_item_pos_array.Take(4))
            .Concat(gridCells)
            .Where(p => p != FixedCenterPosition)
            .Distinct()
            .ToList();

        // Mark screens already covered by the fixed center position
        var uncovered = screens.Select(s => s.screen).ToHashSet();
        foreach (var s in screens.Where(s => IsValidItemPosition(s, FixedCenterPosition)))
            uncovered.Remove(s.screen);

        // Greedy set cover: pick positions that cover the most uncovered screens
        while (selected.Count < 4 && uncovered.Count > 0)
        {
            var (pos, count) = candidates
                .Select(p => (p, screens.Count(s => uncovered.Contains(s.screen) && IsValidItemPosition(s, p))))
                .OrderByDescending(x => x.Item2)
                .First();

            if (count == 0) break;

            selected.Add(pos);
            candidates.Remove(pos);
            foreach (var s in screens.Where(s => IsValidItemPosition(s, pos)))
                uncovered.Remove(s.screen);
        }

        if (uncovered.Count > 0)
            throw new InvalidOperationException($"Could not find item positions covering all screens for level {_level}");

        // Pad to 4 slots with vanilla fallbacks
        foreach (var fb in _levelData.shortcut_or_item_pos_array.Take(4))
        {
            if (selected.Count >= 4) break;
            if (!selected.Contains(fb)) selected.Add(fb);
        }
        while (selected.Count < 4)
            selected.Add(FixedCenterPosition);

        _itemPositionSlots = [.. selected.Take(4)];
    }

    // Level 9 helpers

    private Room? GetLevelNineCheckRoom(Room startRoom)
    {
        var northNeighbor = startRoom.Neighbors.FirstOrDefault(n =>
            !n.HasAnyRole(RoomRole.Cellar) && n.X == startRoom.X && n.Y == startRoom.Y - 1);

        if (northNeighbor == null) return null;
        if (northNeighbor.Neighbors.Any(n => !n.HasAnyRole(RoomRole.Cellar) && n.X == northNeighbor.X + 1 && n.Y == northNeighbor.Y))
            return null;
        if (!northNeighbor.Neighbors.Any(n => !n.HasAnyRole(RoomRole.Cellar) && n != startRoom))
            return null;
        // The check room gates the dungeon via triforce shutters on its (non-south) doors. A cellar
        // stair passage is a separate connection that bypasses doors entirely, so a check room with
        // one would let the player descend straight into the rest of level 9 without the triforces
        // (multi-segment connectors are assigned before the check room is chosen). Reject it.
        if (northNeighbor.Neighbors.Any(n => n.HasAnyRole(RoomRole.Cellar)))
            return null;

        return northNeighbor;
    }

    /// <summary>
    /// Disconnects all non-cellar neighbors from the start room except the check room,
    /// ensuring the only way into the dungeon is through the triforce check.
    /// Orphaned rooms are reconnected through the check room when possible.
    /// </summary>
    private void IsolateStartRoomForLevelNine(Room startRoom, Room checkRoom)
    {
        var toDisconnect = startRoom.Neighbors
            .Where(n => n != checkRoom && !n.HasAnyRole(RoomRole.Cellar))
            .ToList();

        foreach (var neighbor in toDisconnect)
        {
            startRoom.Neighbors.Remove(neighbor);
            neighbor.Neighbors.Remove(startRoom);

            // If the neighbor lost its only non-cellar connection, reconnect it to the check room
            if (neighbor.Neighbors.Count(n => !n.HasAnyRole(RoomRole.Cellar)) == 0 && IsAdjacent(checkRoom, neighbor))
            {
                checkRoom.Neighbors.Add(neighbor);
                neighbor.Neighbors.Add(checkRoom);
            }
        }
    }

    // Write (serialization to YamlData)

    public void Write()
    {
        var vanillaLevelMaps = GetVanillaLevelMaps();
        var itemPositionSlots = new List<int>(_itemPositionSlots);
        WriteNonCellarRooms(vanillaLevelMaps, itemPositionSlots);
        WriteCellarRooms(vanillaLevelMaps);
        UpdateLevelData(itemPositionSlots);
    }

    private void WriteNonCellarRooms(List<YamlReader.UnderworldMap> vanillaLevelMaps, List<int> itemPositionSlots)
    {
        foreach (var room in _map.UsedNonCellarRooms)
        {
            int mapId = GetMapId(room);

            var neighborMapIds = new Dictionary<YamlReader.Direction, int>();
            foreach (var neighbor in room.Neighbors)
            {
                if (neighbor.HasAnyRole(RoomRole.Cellar) || !IsAdjacent(room, neighbor))
                    continue;

                var dir = StringToYamlDirection(GetDirection(room, neighbor));
                neighborMapIds[dir] = GetMapId(neighbor);
            }

            var doors = new int[4];
            doors[0] = room.Doors.TryGetValue("N", out var nDoor) ? (int)nDoor : (int)YamlReader.DoorType.Wall;
            doors[1] = room.Doors.TryGetValue("S", out var sDoor) ? (int)sDoor : (int)YamlReader.DoorType.Wall;
            doors[2] = room.Doors.TryGetValue("W", out var wDoor) ? (int)wDoor : (int)YamlReader.DoorType.Wall;
            doors[3] = room.Doors.TryGetValue("E", out var eDoor) ? (int)eDoor : (int)YamlReader.DoorType.Wall;

            int itemPos = 0;
            if (RoomNeedsGeneratedItemPosition(room))
            {
                var screenData = GetUnderworldScreen(room.Screen);
                itemPos = GetItemPositionSlot(itemPositionSlots, screenData);
            }
            else if (room.HasAnyRole(RoomRole.End) && _level != 9)
            {
                itemPos = FixedCenterPosSlot;
            }

            var vanillaPalettes = GetRoomPalettes(room.Screen, vanillaLevelMaps);

            _data.underworld_maps.Add(new YamlReader.UnderworldMap
            {
                map = mapId,
                screen = room.Screen,
                name = $"Dungeon L{_level} R({room.X},{room.Y})",
                area = YamlReader.Area.Underworld,
                palettes = vanillaPalettes,
                doors = doors,
                passage = false,
                passage_left = 0,
                passage_right = 0,
                enemies = room.EnemyCount,
                enemy_id = room.EnemyId,
                enemy_mode = room.EnemyMode,
                push_block = room.PushBlock,
                dark_room = room.HasAnyRole(RoomRole.Boss) && _level == 9,
                boss_sfx = (room.HasAnyRole(RoomRole.Boss) || IsBossAdjacent(room)) && _level != 9 ? 0x01 : 0x00,
                room_item = room.HasAnyRole(RoomRole.Item) ? 0x01 :
                            room.HasAnyRole(RoomRole.End) && _level != 9 ? 0x1B :
                            room.HasAnyRole(RoomRole.Boss) && _level == 9 ? 0x0E :
                            0x03,
                item_pos = itemPos,
                behaviour = (int)room.Behaviour,
                level_nine_check = room.HasAnyRole(RoomRole.LevelNineCheck) ? true : null,
                generated = true,
                generated_level = _level,
                local_room_id = GetLocalRoomId(room),
                neighbor_map_ids = neighborMapIds,
            });
        }
    }

    private void WriteCellarRooms(List<YamlReader.UnderworldMap> vanillaLevelMaps)
    {
        foreach (var room in _map.FindRooms(RoomRole.Cellar))
        {
            int mapId = GetMapId(room);
            var connectedRooms = room.Neighbors.Where(n => !n.HasAnyRole(RoomRole.Cellar)).ToList();
            bool isConnectorCellar = connectedRooms.Count >= 2;

            int screen;
            int passageLeft = 0, passageRight = 0, itemPos = 0;

            if (isConnectorCellar)
            {
                screen = ScreenId.PassageCellar;
                passageLeft = GetMapId(connectedRooms[0]);
                passageRight = GetMapId(connectedRooms[1]);
            }
            else
            {
                screen = ScreenId.ItemCellar;
                passageLeft = GetMapId(connectedRooms[0]);
            }

            var cellarTemplate = (vanillaLevelMaps.FirstOrDefault(m => m.passage && m.screen == screen)
                ?? _data.underworld_maps.FirstOrDefault(m => m.map < 0x100 && m.area == YamlReader.Area.Underworld && m.passage && m.screen == screen))
                ?? throw new Exception($"No vanilla cellar template found for screen {screen:X2}");

            if (!isConnectorCellar)
            {
                itemPos = FixedCenterPosSlot;
                passageRight = passageLeft;
            }

            _data.underworld_maps.Add(new YamlReader.UnderworldMap
            {
                map = mapId,
                screen = screen,
                name = $"Dungeon L{_level} Cellar({room.X},{room.Y})",
                area = YamlReader.Area.Underworld,
                palettes = [.. cellarTemplate.palettes],
                doors = [.. cellarTemplate.doors],
                passage = true,
                passage_left = passageLeft,
                passage_right = passageRight,
                enemies = cellarTemplate.enemies,
                enemy_id = cellarTemplate.enemy_id,
                enemy_mode = cellarTemplate.enemy_mode,
                push_block = cellarTemplate.push_block,
                dark_room = cellarTemplate.dark_room,
                boss_sfx = cellarTemplate.boss_sfx,
                room_item = room.HasAnyRole(RoomRole.Item) ? cellarTemplate.room_item : 0x03,
                item_pos = itemPos,
                behaviour = cellarTemplate.behaviour,
                level_nine_check = null,
                generated = true,
                generated_level = _level,
                local_room_id = GetLocalRoomId(room),
                neighbor_map_ids = null,
            });
        }
    }

    private void UpdateLevelData(List<int> itemPositionSlots)
    {
        _levelData.rooms = _map.UsedRooms.Select(r => GetMapId(r)).ToArray();
        _levelData.cellar_room_id_array = _map.FindRooms(RoomRole.Cellar).Select(r => GetMapId(r)).ToArray();

        var startRoom = _map.FindRoom(RoomRole.Start);
        var bossRoom = _map.FindRoom(RoomRole.Boss);
        var endRoom = _map.FindRoom(RoomRole.End);

        _levelData.start_room_id = GetMapId(startRoom);
        _levelData.boss_room_id = GetMapId(bossRoom);
        _levelData.triforce_room_id = GetMapId(endRoom);
        _levelData.name = $"Dungeon L{_level} Randomized";
        _levelData.shortcut_or_item_pos_array = [.. itemPositionSlots];

        // Custom world flags in WRAM region 0x1B80-0x2000, 0x80 bytes per level
        int worldFlagsAddr = (_level - 1) * 0x80 + 0x1B80;
        _levelData.world_flags_addr = [worldFlagsAddr & 0xFF, (worldFlagsAddr >> 8) & 0xFF];

        var visibleMapRoomIds = _map.UsedNonCellarRooms
            .Where(r => !r.HasAnyRole(RoomRole.LevelNineCheck))
            .Select(GetLocalRoomId);
        var mapData = BuildMapData(visibleMapRoomIds);

        _levelData.start_y = mapData.StartY;
        _levelData.submenu_map_rotation = mapData.SubmenuMapRotation;
        _levelData.status_bar_map_x_offset = mapData.StatusBarMapXOffset;
        _levelData.submenu_map_mask = mapData.SubmenuMapMask;
        _levelData.status_bar_map_transfer_buf = mapData.StatusBarMapTransferBuf;
    }

    // Generate (main entry point)

    public void Generate()
    {
        var segments = CreateSegments();

        const int maxAttempts = 100;
        int attempts = 0;
        while (attempts < maxAttempts)
        {
            try
            {
                PlaceSegments(segments);
                ConnectSegments();
                AssignStartAndEndRooms();
                break;
            }
            catch (InvalidOperationException)
            {
                ResetMap();
                attempts++;
            }
        }

        if (attempts >= maxAttempts)
            throw new InvalidOperationException($"Dungeon generation failed for level {_level} after {maxAttempts} attempts");

        var startRoom = _map.FindRoom(RoomRole.Start);
        var endRoom = _map.FindRoom(RoomRole.End);

        var criticalPath = GetShortestPath(startRoom, endRoom);
        var criticalPathDistances = CalculateDistancesFromPath(criticalPath.ToHashSet());

        CreateItemCellars(criticalPathDistances);

        if (_config.Segments == 1 && _config.Rooms >= 20)
            CreateIntraConnectorCellar(criticalPathDistances);

        AlignStartRoomToBottomEdge();
        AssignScreens();
        AssignDoors(criticalPath, criticalPathDistances);

        var bossRoom = _map.FindRoom(RoomRole.Boss);
        endRoom = _map.FindRoom(RoomRole.End);
        var bossToEndDirection = GetDirection(bossRoom, endRoom);
        if (string.IsNullOrEmpty(bossToEndDirection))
            throw new InvalidOperationException("Boss room must be directly adjacent to the end room.");

        // The boss-side shutter stays closed until the boss is killed, gating entry to the
        // triforce room (vanilla behaviour). The triforce-room side, however, must stay open:
        // a shutter there has no trigger to reopen (the End room's behaviour is None for levels
        // 1-8), so the engine would close it on entry and trap the exit shut. Level 9's end room
        // (the Zelda room) uses KillForShutter and reopens on clear, so it keeps the shutter.
        bossRoom.Doors[bossToEndDirection] = YamlReader.DoorType.Shutter;
        endRoom.Doors[OppositeDirection(bossToEndDirection)] =
            _level == 9 ? YamlReader.DoorType.Shutter : YamlReader.DoorType.Open;

        // Level 9: force special door configurations
        if (_level == 9)
        {
            // Check room: shutters on all exits except south (back to start).
            // The level_nine_check flag makes these require triforces in the graph.
            var checkRoom = _map.FindRoom(RoomRole.LevelNineCheck);
            foreach (var dir in checkRoom.Doors.Keys.ToList())
            {
                if (dir == "S")
                    checkRoom.Doors[dir] = YamlReader.DoorType.Open;
                else
                {
                    checkRoom.Doors[dir] = YamlReader.DoorType.Shutter;
                    // Neighbor side is also shutter (graph reads from.level_nine_check for the requirement)
                    var neighbor = checkRoom.Neighbors.FirstOrDefault(n => GetDirection(checkRoom, n) == dir);
                    if (neighbor != null)
                        neighbor.Doors[OppositeDirection(dir)] = YamlReader.DoorType.Shutter;
                }
            }

            _map.FindRoom(RoomRole.Start).Doors["N"] = YamlReader.DoorType.Open;
        }

        EnsureStartExitDoor();
        AssignEnemies();
        AssignItems(criticalPath, criticalPathDistances);
        ResolveRoomBehaviours();
        SelectItemPositionSlots();
        FixOrphanedItemLocations();

    }

    // Spoiler data extraction

    public DungeonSpoilerData GetSpoilerData()
    {
        var rooms = new List<RoomSpoilerData>();
        foreach (var room in _map.UsedRooms)
        {
            var roles = RoleNames
                .Where(r => room.HasAnyRole(r.role))
                .Select(r => r.name)
                .ToList();

            var doors = new Dictionary<string, string>();
            foreach (var (dir, type) in room.Doors)
                doors[dir] = type.ToString();

            string? enemyName = null;
            if (room.EnemyId >= 0)
                enemyName = GetEnemyName(_data, room.EnemyId, room.EnemyMode);

            int[][]? connectedTo = null;
            if (room.HasAnyRole(RoomRole.Cellar))
            {
                connectedTo = room.Neighbors
                    .Where(n => !n.HasAnyRole(RoomRole.Cellar))
                    .Select(n => new[] { n.X, n.Y })
                    .ToArray();
            }

            rooms.Add(new RoomSpoilerData(
                X: room.X,
                Y: room.Y,
                Roles: [.. roles],
                Doors: doors,
                Enemy: enemyName,
                EnemyCount: room.EnemyCount,
                Segment: room.Segment,
                Screen: room.Screen.ToString("X2"),
                ConnectedTo: connectedTo
            ));
        }

        int maxX = rooms.Count > 0 ? rooms.Max(r => r.X) + 1 : _config.Width;
        int maxY = rooms.Count > 0 ? rooms.Max(r => r.Y) + 1 : _config.Height;

        return new DungeonSpoilerData(
            Level: _level,
            Width: Math.Max(_config.Width, maxX),
            Height: Math.Max(_config.Height, maxY),
            Rooms: rooms
        );
    }

    private static string GetEnemyName(YamlReader.YamlData data, int id, int mode)
    {
        int effectiveId = GetEffectiveEnemyId(id, mode);
        if (effectiveId < 0x49)
        {
            var enemy = data.enemies.enemies.FirstOrDefault(e => e.id == effectiveId);
            return enemy?.name ?? $"Unknown ({effectiveId:X2})";
        }
        else if (effectiveId >= 0x62)
        {
            int listId = effectiveId - 0x62;
            var list = data.enemies.enemy_lists.FirstOrDefault(l => l.id == listId);
            if (list?.data.Count > 0)
            {
                var firstEnemy = data.enemies.enemies.FirstOrDefault(e => e.id == list.data[0]);
                return $"List: {firstEnemy?.name ?? list.data[0].ToString("X2")}...";
            }
            return list != null ? $"List {listId}" : $"Unknown List ({listId:X2})";
        }
        return $"Unknown ({effectiveId:X2})";
    }

    // Item assignment

    private bool ItemFitsRoom(Room room)
    {
        if (room.HasAnyRole(RoomRole.Start | RoomRole.Boss | RoomRole.End | RoomRole.Connector | RoomRole.Stairs | RoomRole.Item | RoomRole.LevelNineCheck))
            return false;

        // Item + shutters only works when the room can use clear logic.
        if (room.Doors.Values.Any(d => d == YamlReader.DoorType.Shutter) && !CanUseClearTrigger(room))
            return false;

        return true;
    }

    private void AssignItems(List<Room> criticalPath, Dictionary<Room, int> criticalPathDistances)
    {
        int lockedDoorsCount = _map.UsedRooms
            .Sum(r => r.Doors.Values.Count(d => d == YamlReader.DoorType.Locked || d == YamlReader.DoorType.Locked2));
        lockedDoorsCount /= 2;

        // Extra item slots beyond keys: auto-scale with room count, or use config override
        int roomCount = _map.UsedNonCellarRooms.Count();
        int extraItems = _config.ExtraItemSlots >= 0
            ? _config.ExtraItemSlots
            : Math.Max(5, roomCount / 4);

        // The item pool (ItemPooler) always assigns Map + Compass + keys to this dungeon's
        // z1d{level} set, where keys = Max(1, lockedDoorsCount + 1). Every dungeon must therefore
        // generate at least that many item rooms, or item placement later fails globally with
        // "Not enough set locations available: z1d{level}". Keep this in sync with
        // ItemPooler.GetKeyCountForLevel.
        int requiredItems = 2 + Math.Max(1, lockedDoorsCount + 1);
        int totalItems = Math.Max(lockedDoorsCount + extraItems, requiredItems);
        int itemsPlaced = 0;

        // Reserve one dungeon item slot for the boss room so defeating the boss always
        // produces an item without increasing the overall number of generated item rooms.
        if (_level != 9 && totalItems > 0)
        {
            _map.FindRoom(RoomRole.Boss).AddRole(RoomRole.Item);
            itemsPlaced++;
        }

        // Ensure at least one item is placed before any locked door
        var accessibleWithoutKeys = GetAccessibleRoomsWithoutKeys();
        var earlyCandidates = accessibleWithoutKeys
            .Where(r => ItemFitsRoom(r))
            .OrderByDescending(r => criticalPathDistances.GetValueOrDefault(r, 0))
            .ToList();

        if (earlyCandidates.Count > 0)
        {
            earlyCandidates.First().AddRole(RoomRole.Item);
            itemsPlaced++;
        }

        var candidates = _map.UsedRooms
            .Where(r => ItemFitsRoom(r))
            .OrderBy(r => r.Neighbors.Count)
            .ThenByDescending(r => criticalPathDistances.GetValueOrDefault(r, 0))
            .ToList();

        while (itemsPlaced < totalItems && candidates.Count > 0)
        {
            int takeTop = Math.Min(candidates.Count, 5);
            var room = candidates[_rnd.Next(takeTop)];
            room.AddRole(RoomRole.Item);
            itemsPlaced++;
            candidates.Remove(room);
        }

        // If this layout couldn't host enough item rooms for the dungeon's item pool, reject it
        // so DataLoader retries with a fresh layout instead of failing globally during item
        // placement. (Small configs like Minimal can run out of eligible rooms on unlucky seeds.)
        if (itemsPlaced < requiredItems)
            throw new InvalidOperationException(
                $"Level {_level}: only {itemsPlaced} item rooms could be placed, but the item pool needs {requiredItems}.");
    }

    private void ResolveRoomBehaviours()
    {
        foreach (var room in _map.UsedNonCellarRooms)
        {
            room.Behaviour = ResolveRoomBehaviour(room);
            room.PushBlock = (room.Behaviour is YamlReader.RoomBehaviour.PushBlockShutter or YamlReader.RoomBehaviour.PushBlockStairs || room.Screen == ScreenId.PushCross || room.Screen == ScreenId.PushStairs);
        }
    }

    private YamlReader.RoomBehaviour ResolveRoomBehaviour(Room room)
    {
        if (room.HasAnyRole(RoomRole.Boss) && _level == 9)
            return YamlReader.RoomBehaviour.GetTriforceShutter;
        if (room.HasAnyRole(RoomRole.Boss))
            return YamlReader.RoomBehaviour.KillForItem;
        if (room.HasAnyRole(RoomRole.End))
            return _level == 9 ? YamlReader.RoomBehaviour.KillForShutter : YamlReader.RoomBehaviour.None;
        if (room.HasAnyRole(RoomRole.LevelNineCheck))
            return YamlReader.RoomBehaviour.None;

        bool hasShutters = room.Doors.Values.Any(d => d == YamlReader.DoorType.Shutter);

        if (RoomSupportsHiddenStairs(room))
        {
            if (hasShutters)
                ReplaceShutters(room, YamlReader.DoorType.Open);
            return YamlReader.RoomBehaviour.PushBlockStairs;
        }

        if (hasShutters)
        {
            if (room.HasAnyRole(RoomRole.Item))
            {
                // KillForItem (trigger 7) both opens the shutters and keeps the item hidden
                // until the room is cleared, so the item is genuinely earned by killing.
                // When hidden items are enabled and the room can use a clear trigger, hide it;
                // otherwise just open the shutters so the visible item is reachable.
                if (CanUseClearTrigger(room) && ShouldHideItemBehindClear(forced: true))
                    return YamlReader.RoomBehaviour.KillForItem;

                ReplaceShutters(room, YamlReader.DoorType.Open);
                return YamlReader.RoomBehaviour.None;
            }

            if (RoomSupportsPushBlockShutters(room) && (!CanUseClearTrigger(room) || _rnd.NextDouble() < 0.50))
                return YamlReader.RoomBehaviour.PushBlockShutter;

            // No item to hide here; the kill trigger only needs to open the shutters.
            if (CanUseClearTrigger(room))
                return YamlReader.RoomBehaviour.KillForShutter;

            ReplaceShutters(room, YamlReader.DoorType.Open);
            return YamlReader.RoomBehaviour.None;
        }

        if (room.HasAnyRole(RoomRole.Item) && CanUseClearTrigger(room) && ShouldHideItemBehindClear(forced: false))
            return YamlReader.RoomBehaviour.KillForItem;

        return YamlReader.RoomBehaviour.None;
    }

    /// <summary>
    /// Decides whether an item room (that can use a kill trigger) should hide its item until cleared.
    /// <paramref name="forced"/> is true when the room already has shutters that a kill trigger would
    /// open anyway, so the only choice is "hidden item" vs "opened shutters with a visible item" —
    /// in that case Sometimes also hides, since the room has to be cleared to progress regardless.
    /// </summary>
    private bool ShouldHideItemBehindClear(bool forced) => _config.HiddenItems switch
    {
        HiddenItemsOption.Off => false,
        HiddenItemsOption.Always => true,
        HiddenItemsOption.Sometimes => forced || _rnd.NextDouble() < 0.35,
        _ => false,
    };

    private void ReplaceShutters(Room room, YamlReader.DoorType replacement)
    {
        foreach (var direction in room.Doors
            .Where(kvp => kvp.Value == YamlReader.DoorType.Shutter)
            .Select(kvp => kvp.Key)
            .ToList())
        {
            room.Doors[direction] = replacement;

            var neighbor = room.Neighbors.FirstOrDefault(n => GetDirection(room, n) == direction);
            if (neighbor == null)
                continue;

            // Never reopen the level-9 check room's gate shutters. They are a deliberate triforce
            // gate forced on after door assignment; this room (a neighbor of the check room) only
            // has a shutter here because that gating put one on its side too. Opening our own side
            // is fine, but propagating it would drag the gate open and let the player bypass it.
            if (neighbor.HasAnyRole(RoomRole.LevelNineCheck))
                continue;

            var opposite = OppositeDirection(direction);
            if (!string.IsNullOrEmpty(opposite))
                neighbor.Doors[opposite] = replacement;
        }
    }

    private bool CanUseClearTrigger(Room room)
        => RoomSupportsKillShutters(room) && RoomHasKillableEnemies(room);

    private bool RoomHasKillableEnemies(Room room)
        => GetEnemyNames(room.EnemyCount, room.EnemyId, room.EnemyMode)
            .Any(enemyName => _data.enemies.enemies.Any(e => e.name == enemyName && e.kill_items.Count > 0));

    private bool TemplateHasKillableEnemies((int enemies, int enemy_id, int enemy_mode) template)
        => GetEnemyNames(template.enemies, template.enemy_id, template.enemy_mode)
            .Any(enemyName => _data.enemies.enemies.Any(e => e.name == enemyName && e.kill_items.Count > 0));

    private IEnumerable<string> GetEnemyNames(int enemyCountIndex, int enemyId, int enemyMode)
    {
        if (enemyId < 0 || enemyCountIndex < 0 || enemyCountIndex >= _levelData.enemy_counts.Length)
            yield break;

        int enemyCount = _levelData.enemy_counts[enemyCountIndex];
        if (enemyCount <= 0)
            yield break;

        int effectiveId = GetEffectiveEnemyId(enemyId, enemyMode);
        if (effectiveId >= 0x62)
        {
            int listId = effectiveId - 0x62;
            var list = _data.enemies.enemy_lists.FirstOrDefault(l => l.id == listId);
            if (list == null)
                yield break;

            for (int i = 0; i < enemyCount && i < list.data.Count; i++)
            {
                var enemy = _data.enemies.enemies.FirstOrDefault(e => e.id == list.data[i]);
                if (enemy != null && enemy.name != "Nothing")
                    yield return enemy.name;
            }
            yield break;
        }

        var singleEnemy = _data.enemies.enemies.FirstOrDefault(e => e.id == effectiveId);
        if (singleEnemy == null || singleEnemy.name == "Nothing")
            yield break;

        for (int i = 0; i < enemyCount; i++)
            yield return singleEnemy.name;
    }

    private List<Room> GetAccessibleRoomsWithoutKeys()
    {
        var startRoom = _map.FindRoom(RoomRole.Start);
        var visited = new HashSet<Room> { startRoom };
        var queue = new Queue<Room>();
        queue.Enqueue(startRoom);
        var accessible = new List<Room>();

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            accessible.Add(current);

            foreach (var neighbor in current.Neighbors)
            {
                if (visited.Contains(neighbor)) continue;

                string dir = GetDirection(current, neighbor);
                if (current.Doors.TryGetValue(dir, out var doorType) &&
                    (doorType == YamlReader.DoorType.Locked || doorType == YamlReader.DoorType.Locked2))
                    continue;

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }
        return accessible;
    }

    // Segment placement

    private void PlaceSegments(int[] segments, double loopChance = 0.15, double dfsChance = 0.35)
    {
        int attempts = 0;
        while (attempts < 10)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                int segAttempts = 0;
                while (!PlaceSegment(i, segments[i], loopChance, dfsChance) && segAttempts < 100)
                {
                    ResetMap(i);
                    segAttempts++;
                }

                if (segAttempts == 100)
                    throw new InvalidOperationException($"Failed to place segment {i} with {segments[i]} rooms after 100 attempts");
            }

            if (_map.UsedRooms.Count() >= _config.Rooms)
                return;

            attempts++;
            ResetMap();
        }

        throw new InvalidOperationException("Failed to place all segments after 10 attempts");
    }

    private void ResetMap(int segmentIndex = -1)
    {
        foreach (var room in _map.Rooms)
        {
            if (room.Segment == segmentIndex || segmentIndex == -1)
            {
                room.Used = false;
                room.Segment = -1;
                room.Role = RoomRole.None;
                room.Neighbors.Clear();
            }
        }
    }

    private int[] CreateSegments(int moves = 20)
    {
        int[] segments = new int[_config.Segments];
        int baseRooms = _config.Rooms / _config.Segments;
        int remainingRooms = _config.Rooms % _config.Segments;

        for (int i = 0; i < _config.Segments; i++)
            segments[i] = baseRooms + (i < remainingRooms ? 1 : 0);

        for (int i = 0; i < moves; i++)
        {
            int from = _rnd.Next(0, _config.Segments);
            int to = _rnd.Next(0, _config.Segments);
            if (from != to && segments[from] > 1)
            {
                segments[from]--;
                segments[to]++;
            }
        }

        return segments;
    }

    private bool PlaceSegment(int segmentIndex, int segmentRooms, double loopChance, double dfsChance)
    {
        var startRoom = _rnd.GetItems([.. _map.AllRooms.Where(r => !r.Used)], 1).FirstOrDefault();
        if (startRoom == null) return false;

        startRoom.Used = true;
        startRoom.Segment = segmentIndex;
        startRoom.AddRole(RoomRole.SegmentStart);
        int roomsPlaced = 1;

        var frontier = new List<Room>(GetGridNeighbors(startRoom, r => !r.Used));

        while (frontier.Count > 0 && roomsPlaced < segmentRooms)
        {
            var room = _rnd.NextDouble() < dfsChance ? frontier.Last() : frontier[_rnd.Next(frontier.Count)];
            frontier.Remove(room);

            if (room.Used) continue;

            room.Used = true;
            room.Segment = segmentIndex;
            roomsPlaced++;

            var usedNeighbors = GetGridNeighbors(room, r => r.Used && r.Segment == room.Segment).ToArray();
            if (usedNeighbors.Length == 0)
                return false;

            var neighbor = usedNeighbors[_rnd.Next(usedNeighbors.Length)];
            room.Neighbors.Add(neighbor);
            neighbor.Neighbors.Add(room);

            if (usedNeighbors.Length > 1 && _rnd.NextDouble() < loopChance)
            {
                var loopNeighbor = usedNeighbors.Where(n => n != neighbor).ToArray();
                var picked = loopNeighbor[_rnd.Next(loopNeighbor.Length)];
                room.Neighbors.Add(picked);
                picked.Neighbors.Add(room);
            }

            frontier.AddRange(GetGridNeighbors(room, r => !r.Used));
        }

        return true;
    }

    // Grid neighbor queries

    private IEnumerable<Room> GetGridNeighbors(Room room, Func<Room, bool>? predicate = null)
    {
        foreach (var (dx, dy, _) in CardinalDirections)
        {
            int nx = room.X + dx, ny = room.Y + dy;
            if (nx >= 0 && nx < _config.Width && ny >= 0 && ny < _config.Height)
            {
                var neighbor = _map.Rooms[nx, ny];
                if (predicate == null || predicate(neighbor))
                    yield return neighbor;
            }
        }
    }

    // Segment connection

    private void ConnectSegments(double connectorDistanceFactor = 0.8)
    {
        double inverseFactor = 1.0 - connectorDistanceFactor;

        for (int i = 0; i < _config.Segments - 1; i++)
        {
            var sourceStart = _map.UsedRooms.First(r => r.Segment == i && r.HasAnyRole(RoomRole.SegmentStart));
            var targetStart = _map.UsedRooms.First(r => r.Segment == i + 1 && r.HasAnyRole(RoomRole.SegmentStart));

            var sourceDistances = Distances(sourceStart, true);
            var targetDistances = Distances(targetStart, true);

            int sourceMax = sourceDistances.Max(d => d.dist);
            int targetMax = targetDistances.Max(d => d.dist);

            var sourceRooms = sourceDistances
                .Where(d => d.dist >= sourceMax * connectorDistanceFactor && !d.room.HasAnyRole(RoomRole.Connector))
                .Select(d => d.room).ToArray();

            var targetRooms = targetDistances
                .Where(d => d.dist <= targetMax * inverseFactor && !d.room.HasAnyRole(RoomRole.Connector))
                .Select(d => d.room).ToArray();

            var sourceRoom = sourceRooms[_rnd.Next(sourceRooms.Length)];
            var targetRoom = targetRooms[_rnd.Next(targetRooms.Length)];

            ConnectRoomsThroughCellar(sourceRoom, targetRoom);
        }
    }

    private void ConnectRoomsThroughCellar(Room roomA, Room roomB)
    {
        var cellar = CreateCellar();
        cellar.Segment = roomA.Segment;

        roomA.Neighbors.Add(cellar);
        roomA.AddRole(RoomRole.Connector | RoomRole.Stairs);
        roomA.TargetSegment = roomB.Segment;
        cellar.Neighbors.Add(roomA);

        roomB.Neighbors.Add(cellar);
        roomB.AddRole(RoomRole.Connector | RoomRole.Stairs);
        roomB.TargetSegment = roomA.Segment;
        cellar.Neighbors.Add(roomB);
    }

    // Cellar creation

    private void CreateItemCellars(Dictionary<Room, int> criticalPathDistances, double distanceFactor = 0.5)
    {
        for (int i = 0; i < _config.ItemCellars; i++)
        {
            var cellar = CreateCellar();
            cellar.AddRole(RoomRole.Item);

            int minDistance = (int)(criticalPathDistances.Values.Max() * distanceFactor);
            var possibleRooms = _map.UsedRooms
                .Where(r => !r.HasAnyRole(RoomRole.Start | RoomRole.End | RoomRole.Boss | RoomRole.Cellar | RoomRole.Connector | RoomRole.LevelNineCheck)
                         && criticalPathDistances[r] >= minDistance)
                .ToArray();

            var room = possibleRooms[_rnd.Next(possibleRooms.Length)];
            room.Neighbors.Add(cellar);
            room.AddRole(RoomRole.Stairs);
            cellar.Neighbors.Add(room);
            cellar.Segment = room.Segment;
        }
    }

    private void CreateIntraConnectorCellar(Dictionary<Room, int> criticalPathDistances, double distanceFactor = 0.5)
    {
        int minDistance = (int)(criticalPathDistances.Values.Max() * distanceFactor);

        var candidates = _map.UsedRooms
            .Where(r => !r.HasAnyRole(RoomRole.Start | RoomRole.End | RoomRole.Cellar | RoomRole.Connector | RoomRole.LevelNineCheck) &&
                        criticalPathDistances[r] >= minDistance)
            .ToArray();

        var roomA = candidates[_rnd.Next(candidates.Length)];

        var possibleRoomsB = candidates
            .Where(r => r.Segment == roomA.Segment && r != roomA)
            .OrderBy(r => GeoDistance(roomA, r))
            .ToArray();

        if (possibleRoomsB.Length == 0) return;

        ConnectRoomsThroughCellar(roomA, possibleRoomsB.Last());
    }

    private Room CreateCellar()
    {
        var room = _map.AllRooms.FirstOrDefault(r => !r.Used)
            ?? throw new InvalidOperationException("No unused room available to create cellar");
        room.Used = true;
        room.AddRole(RoomRole.Cellar);
        return room;
    }

    // Start/end room assignment

    private void AssignStartAndEndRooms(double forceEndOtherSegment = 0.75, double depthFactor = 0.8)
    {
        var usedNonCellar = _map.UsedNonCellarRooms
            .Where(r => !r.HasAnyRole(RoomRole.Connector))
            .ToArray();

        if (usedNonCellar.Length == 0)
            throw new InvalidOperationException("No valid rooms available for start room");

        int maxY = usedNonCellar.Max(r => r.Y);
        var startCandidates = usedNonCellar.Where(r => r.Y == maxY).ToArray();

        if (_level == 9)
        {
            // Prefer dead-end start rooms (only one non-cellar neighbor, going north)
            // so we don't have to disconnect extra neighbors later
            var deadEndL9 = startCandidates
                .Where(r => r.Neighbors.Count(n => !n.HasAnyRole(RoomRole.Cellar)) == 1
                          && GetLevelNineCheckRoom(r) != null)
                .ToArray();

            startCandidates = deadEndL9.Length > 0
                ? deadEndL9
                : startCandidates.Where(r => GetLevelNineCheckRoom(r) != null).ToArray();
        }

        if (startCandidates.Length == 0)
        {
            if (_level == 9) throw new InvalidOperationException("No valid level 9 entrance candidate found");
            startCandidates = usedNonCellar;
        }

        var startRoom = startCandidates[_rnd.Next(startCandidates.Length)];
        startRoom.AddRole(RoomRole.Start);

        // The start must be the bottommost used room: AlignStartRoomToBottomEdge shifts it to
        // row 7, and anything below it would be pushed off the engine's 8-row map. Connectors are
        // excluded from start candidates, so one can sit below the chosen start — reject if so.
        if (_map.UsedNonCellarRooms.Any(r => r.Y > startRoom.Y))
            throw new InvalidOperationException("A connector room sits below the start room");

        if (_level == 9)
        {
            var checkRoom = GetLevelNineCheckRoom(startRoom)
                ?? throw new InvalidOperationException("Level 9 entrance must connect to a dedicated E6-style check room");
            checkRoom.AddRole(RoomRole.LevelNineCheck);
            IsolateStartRoomForLevelNine(startRoom, checkRoom);

            // Isolation can orphan rooms that aren't adjacent to the check room. If any used
            // non-cellar room is no longer reachable from the start, the layout is unusable
            // (item rooms there can never be filled) — bail so the retry loop picks another.
            var reachable = Distances(startRoom).Select(d => d.room).ToHashSet();
            if (_map.UsedNonCellarRooms.Any(r => !reachable.Contains(r)))
                throw new InvalidOperationException("Level 9 isolation orphaned one or more rooms");
        }

        var distances = Distances(startRoom);
        int maxDist = distances.Max(d => d.dist);
        bool forceOtherSegment = _rnd.NextDouble() < forceEndOtherSegment;

        var endCandidates = FindEndCandidates(distances, maxDist * depthFactor, startRoom, forceOtherSegment);

        if (endCandidates.Length == 0)
            throw new InvalidOperationException("No valid end room candidates found");

        var endRoom = endCandidates[_rnd.Next(endCandidates.Length)];
        endRoom.AddRole(RoomRole.End);

        var bossRoom = endRoom.Neighbors
            .Where(n => !n.HasAnyRole(EndCandidateExcludedRoles | RoomRole.End))
            .FirstOrDefault()
            ?? throw new InvalidOperationException("No valid boss room neighbor for end room");
        bossRoom.AddRole(RoomRole.Boss);
    }

    private Room[] FindEndCandidates(List<(int dist, Room room)> distances, double minDist, Room startRoom, bool forceOtherSegment)
    {
        // Primary: deep dead-end rooms
        var candidates = distances
            .Where(d => d.dist >= minDist && d.room.Neighbors.Count == 1 &&
                        !d.room.HasAnyRole(EndCandidateExcludedRoles))
            .Select(d => d.room)
            .ToArray();

        if (forceOtherSegment && candidates.Any(r => r.Segment != startRoom.Segment))
            candidates = candidates.Where(r => r.Segment != startRoom.Segment).ToArray();

        // Fallback 1: any dead-end room, ordered by distance
        if (candidates.Length == 0)
        {
            candidates = distances
                .Where(d => d.room.Neighbors.Count == 1 &&
                            !d.room.HasAnyRole(EndCandidateExcludedRoles))
                .OrderByDescending(d => d.dist)
                .Select(d => d.room)
                .Take(3)
                .ToArray();
        }

        // Fallback 2: any non-start, non-cellar room farthest from start
        if (candidates.Length == 0)
        {
            candidates = distances
                .Where(d => !d.room.HasAnyRole(EndCandidateExcludedRoles))
                .OrderByDescending(d => d.dist)
                .Select(d => d.room)
                .Take(3)
                .ToArray();
        }

        // Level 9: end room must be directly above boss room
        if (_level == 9)
            candidates = candidates.Where(r => r.Neighbors.Count == 1 && r.Neighbors.First().Y > r.Y).ToArray();

        return candidates;
    }

    // Door assignment

    private void AssignDoors(List<Room> criticalPath, Dictionary<Room, int> criticalPathDistances)
    {
        var criticalSet = new HashSet<Room>(criticalPath);

        const double lockedBase = 0.10;
        const double bombableBase = 0.15;
        const double shutterBase = 0.15;
        double baseNonOpen = (lockedBase + bombableBase + shutterBase) * _config.DoorComplexity;

        // DoorComplexity == 0 means every door is Open. Without this guard the share divisions
        // below are x/0 = +Infinity, which makes lockedProb infinite and turns *every* door into
        // a locked door (the opposite of the intent, and far too many keys for the item pool).
        bool allDoorsOpen = baseNonOpen <= 0.0;

        double lockedShare = allDoorsOpen ? 0.0 : lockedBase / baseNonOpen;
        double bombableShare = allDoorsOpen ? 0.0 : bombableBase / baseNonOpen;

        foreach (var room in _map.UsedRooms)
        {
            foreach (var neighbor in room.Neighbors)
            {
                if (neighbor.HasAnyRole(RoomRole.Cellar) || room.HasAnyRole(RoomRole.Cellar))
                    continue;
                if (!IsAdjacent(room, neighbor))
                    continue;
                // Process each edge only once (canonical ordering)
                if (neighbor.X < room.X || (neighbor.X == room.X && neighbor.Y < room.Y))
                    continue;

                YamlReader.DoorType doorType;
                if (allDoorsOpen)
                {
                    doorType = YamlReader.DoorType.Open;
                }
                else
                {
                    bool onCritical = criticalSet.Contains(room) && criticalSet.Contains(neighbor);

                    double modifier = 0.0;
                    if (onCritical)
                        modifier = -0.2;
                    else if (criticalPathDistances.TryGetValue(room, out int distRoom) &&
                             criticalPathDistances.TryGetValue(neighbor, out int distNeighbor))
                        modifier = (distRoom + distNeighbor) * 0.05;

                    double nonOpen = Math.Clamp(baseNonOpen + modifier, 0.05, 0.90);
                    double lockedProb = nonOpen * lockedShare;
                    double bombableProb = nonOpen * bombableShare;
                    double shutterProb = nonOpen * (1.0 - lockedShare - bombableShare);

                    if (!RoomCanUseAnyShutterTrigger(room) || !RoomCanUseAnyShutterTrigger(neighbor))
                        shutterProb = 0.0;

                    double roll = _rnd.NextDouble();
                    if (roll < lockedProb)
                        doorType = YamlReader.DoorType.Locked;
                    else if (roll < lockedProb + bombableProb)
                        doorType = YamlReader.DoorType.Bombable;
                    else if (roll < lockedProb + bombableProb + shutterProb)
                        doorType = YamlReader.DoorType.Shutter;
                    else
                        doorType = YamlReader.DoorType.Open;
                }

                string dirToNeighbor = GetDirection(room, neighbor);
                string dirToRoom = OppositeDirection(dirToNeighbor);

                room.Doors[dirToNeighbor] = doorType;
                neighbor.Doors[dirToRoom] = doorType;
            }
        }
    }

    // Layout adjustments

    private void AlignStartRoomToBottomEdge()
    {
        var startRoom = _map.FindRoom(RoomRole.Start);
        int deltaY = 7 - startRoom.Y;
        if (deltaY <= 0) return;

        foreach (var room in _map.UsedNonCellarRooms)
            room.Y += deltaY;

        var occupiedRoomIds = _map.UsedNonCellarRooms.Select(GetLocalRoomId).ToHashSet();
        var freeRoomIds = new Queue<int>(Enumerable.Range(0, 0x80).Where(id => !occupiedRoomIds.Contains(id)));

        foreach (var cellar in _map.FindRooms(RoomRole.Cellar))
        {
            if (freeRoomIds.Count == 0)
                throw new InvalidOperationException("No free room slots remain for cellar placement");
            int roomId = freeRoomIds.Dequeue();
            cellar.X = roomId & 0x0F;
            cellar.Y = roomId >> 4;
        }
    }

    private void EnsureStartExitDoor()
    {
        _map.FindRoom(RoomRole.Start).Doors["S"] = YamlReader.DoorType.Open;
    }

    // Enemy assignment

    /// <summary>Progressive enemy tiers: levels in the same tier share their enemy pools.</summary>
    private static readonly int[][] EnemyTiers = [[1, 2, 3], [4, 5], [6, 7, 8], [9]];

    /// <summary>Returns the set of dungeon levels whose enemies are valid for the current dungeon.</summary>
    private HashSet<int> GetValidEnemyLevels() => _config.EnemyPlacement switch
    {
        EnemyPlacementOption.Vanilla => [_level],
        EnemyPlacementOption.Progressive => EnemyTiers.First(t => t.Contains(_level)).ToHashSet(),
        EnemyPlacementOption.Random => [1, 2, 3, 4, 5, 6, 7, 8, 9],
        _ => [_level],
    };

    // Screen compatibility lists (from Z1M1 DungeonEnemyRando)

    /// <summary>Screens where spike traps (corner traps, trap combos) work correctly.</summary>
    private static readonly HashSet<int> GoodSpikeScreens =
    [
        0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06,
        0x0A, 0x0C, 0x0D, 0x0E, 0x0F,
        0x11, 0x13, 0x16, 0x17,
        0x18, 0x19, 0x1A, 0x1B, 0x1D, 0x1E, 0x1F,
        0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
    ];

    /// <summary>Screens large/open enough for Gleeok.</summary>
    private static readonly HashSet<int> GoodGleeokScreens =
        [0x00, 0x02, 0x03, 0x04, 0x05, 0x1B, 0x1D, 0x1E, 0x1F, 0x21, 0x23, 0x24, 0x25, 0x26];

    /// <summary>Screens large/open enough for Lanmola.</summary>
    private static readonly HashSet<int> GoodLanmolaScreens =
        [0x00, 0x0A, 0x1A, 0x1E, 0x22, 0x23, 0x24, 0x25, 0x26];

    /// <summary>Screens compatible with the Rupee Boss (ten-rupee secret).</summary>
    private static readonly HashSet<int> GoodRupeeBossScreens =
        [0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x08, 0x0A, 0x11, 0x13, 0x1B, 0x1D, 0x22, 0x24, 0x25, 0x26];

    /// <summary>Screens where Dodongo can't move properly.</summary>
    private static readonly HashSet<int> BadDodongoScreens = [0x0B, 0x12, 0x14, 0x17];

    /// <summary>Screens that should never have enemies placed on them.</summary>
    private static readonly HashSet<int> ExcludedEnemyScreens = [0x20];

    /// <summary>Returns true if the room is directly adjacent to the boss room (for boss roar SFX).</summary>
    private bool IsBossAdjacent(Room room)
    {
        var bossRoom = _map.FindRoomOrDefault(RoomRole.Boss);
        return bossRoom != null && room.Neighbors.Contains(bossRoom);
    }

    /// <summary>Computes the effective enemy ID from mode and id: (mode &lt;&lt; 6) | id.</summary>
    private static int GetEffectiveEnemyId(int enemyId, int enemyMode) => (enemyMode << 6) | enemyId;

    /// <summary>
    /// Returns true if the given effective enemy ID represents a trap/spike combo.
    /// Covers standalone traps (0x49/0x4A) and enemy lists containing traps.
    /// </summary>
    private static bool IsTrapEnemy(int enemyId, int enemyMode)
    {
        int effectiveId = GetEffectiveEnemyId(enemyId, enemyMode);
        // Standalone traps: 0x49 (Three Pairs), 0x4A (Corner Traps)
        if (effectiveId == 0x49 || effectiveId == 0x4A) return true;
        // Enemy lists that contain traps (lists 11, 12, 20, 21)
        if (effectiveId == 0x6D || effectiveId == 0x6E || effectiveId == 0x76 || effectiveId == 0x77) return true;
        return false;
    }

    /// <summary>
    /// Each rule returns false to reject an enemy template for a room, true to allow it.
    /// Room already has its screen assigned when these are evaluated.
    /// </summary>
    private static readonly List<Func<(int enemies, int enemy_id, int enemy_mode), Room, int, bool>> EnemyRules =
    [
        // Skip unoccupied rooms
        (_, room, _) => room.Used,

        // No enemies in start, cellar, end, boss, or L9 check rooms (boss rooms are handled separately)
        (_, room, _) => !room.HasAnyRole(EnemyExcludedRoles),

        // Skip excluded screens
        (_, room, _) => !ExcludedEnemyScreens.Contains(room.Screen),

        // Gleeok (mode=1, codes 0x02-0x05) needs large open screens
        (t, room, _) => !(t.enemy_mode == 1 && t.enemy_id >= 0x02 && t.enemy_id <= 0x05)
            || GoodGleeokScreens.Contains(room.Screen),

        // Lanmola (mode=0, codes 0x3A/0x3B) needs open screens
        (t, room, _) => !(t.enemy_mode == 0 && (t.enemy_id == 0x3A || t.enemy_id == 0x3B))
            || GoodLanmolaScreens.Contains(room.Screen),

        // Dodongo (mode=0, codes 0x31/0x32) can't go in tight screens
        (t, room, _) => !(t.enemy_mode == 0 && (t.enemy_id == 0x31 || t.enemy_id == 0x32))
            || !BadDodongoScreens.Contains(room.Screen),

        // Rupee Boss (mode=0, code 0x35) needs specific screens
        (t, room, _) => !(t.enemy_mode == 0 && t.enemy_id == 0x35)
            || GoodRupeeBossScreens.Contains(room.Screen),

        // Traps and trap combos need screens with proper spike tile positions
        (t, room, _) => !IsTrapEnemy(t.enemy_id, t.enemy_mode)
            || GoodSpikeScreens.Contains(room.Screen),
    ];

    private static bool EnemyFitsRoom((int enemies, int enemy_id, int enemy_mode) template, Room room, int level)
        => EnemyRules.All(rule => rule(template, room, level));

    private void AssignEnemies()
    {
        var vanillaLevelMaps = GetVanillaLevelMaps();
        var vanillaBossRoom = vanillaLevelMaps.FirstOrDefault(m => m.map == _levelData.boss_room_id && !m.passage);
        var vanillaLevelNineCheckRoom = _level == 9
            ? _data.underworld_maps.FirstOrDefault(m => m.map < 0x100 && m.area == YamlReader.Area.Underworld && !m.passage && m.level_nine_check == true)
            : null;
        if (vanillaBossRoom == null && _level != 9)
            throw new Exception($"No vanilla boss room template found for level {_level} (boss_room_id={_levelData.boss_room_id})");
        if (_level == 9 && vanillaLevelNineCheckRoom == null)
            throw new Exception("No vanilla Level 9 check room template found.");

        var validLevels = GetValidEnemyLevels();

        var validEnemyIds = _data.enemies.enemies
            .Where(e => e.allowed_levels.Any(l => validLevels.Contains(l)))
            .Select(e => e.id)
            .ToHashSet();

        var validListIds = _data.enemies.enemy_lists
            .Where(el => el.allowed_levels.Any(l => validLevels.Contains(l)))
            .Select(el => el.id)
            .ToHashSet();

        // Gather enemy templates from ALL vanilla dungeon levels, filtered by allowed_levels.
        // effectiveId = GetEffectiveEnemyId(enemy_id, enemy_mode):
        //   < 0x62: individual enemy, check against validEnemyIds
        //   >= 0x62: enemy list (listId = effectiveId - 0x62), check against validListIds
        var enemyTemplates = _data.underworld_maps
            .Where(m => m.map < 0x100 && m.area == YamlReader.Area.Underworld && !m.passage &&
                        m.enemies > 0 &&
                        m.screen != ScreenId.Lobby && m.screen != ScreenId.ZeldaRoom &&
                        m.screen != ScreenId.GanonRoom && m.screen != ScreenId.TriforceRoom)
            .Where(m =>
            {
                int effectiveId = GetEffectiveEnemyId(m.enemy_id, m.enemy_mode);
                if (effectiveId == 0) return false;
                if (effectiveId >= 0x62) return validListIds.Contains(effectiveId - 0x62);
                return validEnemyIds.Contains(effectiveId);
            })
            .Select(m => (m.enemies, m.enemy_id, m.enemy_mode))
            .Distinct()
            .ToArray();

        foreach (var room in _map.UsedRooms)
        {
            // Boss rooms use dedicated boss template
            if (room.HasAnyRole(RoomRole.Boss))
            {
                if (_level == 9)
                {
                    room.EnemyMode = 0;
                    room.EnemyId = EnemyId.Ganon;
                    room.EnemyCount = 0;
                }
                else
                {
                    room.EnemyMode = vanillaBossRoom!.enemy_mode;
                    room.EnemyId = vanillaBossRoom.enemy_id;
                    room.EnemyCount = vanillaBossRoom.enemies;
                }
                continue;
            }

            // Level 9 end room: Zelda NPC
            if (room.HasAnyRole(RoomRole.End) && _level == 9)
            {
                room.EnemyMode = 0;
                room.EnemyId = EnemyId.ZeldaNpc;
                room.EnemyCount = 0;
                continue;
            }

            // Level 9 check room uses the dedicated vanilla NPC/object payload.
            if (room.HasAnyRole(RoomRole.LevelNineCheck))
            {
                room.EnemyMode = vanillaLevelNineCheckRoom!.enemy_mode;
                room.EnemyId = vanillaLevelNineCheckRoom.enemy_id;
                room.EnemyCount = vanillaLevelNineCheckRoom.enemies;
                continue;
            }

            var candidates = enemyTemplates
                .Where(t => EnemyFitsRoom(t, room, _level))
                .ToArray();

            bool needsClearableRoom = room.Doors.Values.Any(d => d == YamlReader.DoorType.Shutter) && !RoomSupportsPushBlockShutters(room);
            if (needsClearableRoom)
                candidates = candidates.Where(TemplateHasKillableEnemies).ToArray();

            if (candidates.Length == 0 || (!needsClearableRoom && _rnd.NextDouble() < _config.EmptyRoomChance))
                continue;

            var template = candidates[_rnd.Next(candidates.Length)];
            room.EnemyMode = template.enemy_mode;
            room.EnemyId = template.enemy_id;
            room.EnemyCount = template.enemies;
        }
    }

    // Screen assignment

    private void AssignScreens()
    {
        var underworldScreens = _data.underworld_screens;
        var screenConnectivity = PrecalculateScreenConnectivity();

        foreach (var room in _map.UsedNonCellarRooms
            .Where(r => !r.HasAnyRole(RoomRole.Start | RoomRole.End | RoomRole.LevelNineCheck)))
        {
            var candidateScreens = underworldScreens
                .Where(s => ScreenFitsRoom(s, room, screenConnectivity[s.screen]))
                .ToArray();

            if (candidateScreens.Length == 0)
                throw new InvalidOperationException($"No candidate screens found for room at ({room.X}, {room.Y})");

            room.Screen = candidateScreens[_rnd.Next(candidateScreens.Length)].screen;
        }

        _map.FindRoom(RoomRole.Start).Screen = ScreenId.Lobby;

        if (_level == 9)
        {
            _map.FindRoom(RoomRole.LevelNineCheck).Screen = ScreenId.LevelNineCheck;
            _map.FindRoom(RoomRole.End).Screen = ScreenId.ZeldaRoom;
            _map.FindRoom(RoomRole.Boss).Screen = ScreenId.GanonRoom;
        }
        else
        {
            _map.FindRoom(RoomRole.End).Screen = ScreenId.TriforceRoom;
        }
    }

    private Dictionary<int, HashSet<(string, string)>> PrecalculateScreenConnectivity()
    {
        var result = new Dictionary<int, HashSet<(string, string)>>();

        foreach (var screen in _data.underworld_screens)
        {
            if (!result.ContainsKey(screen.screen))
                result[screen.screen] = [];

            var entranceDirs = screen.nodes.exits.Where(e => e.type == YamlReader.ExitType.Entrance).ToDictionary(
                e => YamlDirectionToString(e.direction),
                e => e.name);

            var exitDirs = screen.nodes.exits.Where(e => e.type == YamlReader.ExitType.Exit).ToDictionary(
                e => YamlDirectionToString(e.direction),
                e => e.name);

            // Record (from, to) for every ordered pair of directions where you can enter via
            // `from` and leave via `to`, including self-pairs (enter and leave the same edge,
            // i.e. turn around). Self-connectivity matters for dead-end rooms: a room with a
            // single door can only work on a screen that lets you return through that door —
            // which push-style screens (e.g. 0x20) do not.
            foreach (var from in entranceDirs.Keys)
            {
                foreach (var to in exitDirs.Keys)
                {
                    if (HasPathBetween(screen, entranceDirs[from], exitDirs[to]))
                        result[screen.screen].Add((from, to));
                }
            }
        }

        return result;
    }

    private static bool HasPathBetween(YamlReader.Screen screen, string startNode, string endNode)
    {
        var visited = new HashSet<string>();
        var stack = new Stack<string>();
        stack.Push(startNode);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current == endNode) return true;
            if (!visited.Add(current)) continue;

            // Directed edges
            if (screen.edges.directed != null)
            {
                foreach (var edge in screen.edges.directed.SelectMany(d => d.Value).Cast<List<object>>())
                {
                    if ((string)edge[0] == current && !visited.Contains((string)edge[1]))
                        stack.Push((string)edge[1]);
                }
            }

            // Undirected edges
            foreach (var edge in screen.edges.undirected.SelectMany(d => d.Value).Cast<List<object>>())
            {
                string from = (string)edge[0], to = (string)edge[1];
                if (current == from && !visited.Contains(to)) stack.Push(to);
                else if (current == to && !visited.Contains(from)) stack.Push(from);
            }
        }

        return false;
    }

    /// <summary>
    /// Screens with a walled-off central passage that no item or NoPlace tile bridges to the
    /// side regions, so it's reachable only through the listed door directions. The engine still
    /// drops the room item / spawns enemies there, so a room without one of these doors strands
    /// the passage and looks broken. 0x0E is vertical (N/S), 0x0F horizontal (W/E).
    /// </summary>
    private static readonly Dictionary<int, string[]> BlockedPassageScreenDoors = new()
    {
        [0x0E] = ["N", "S"],
        [0x0F] = ["W", "E"],
    };

    /// <summary>
    /// Returns the cardinal directions ("N"/"S"/"W"/"E") in which the room has a neighbour. Every
    /// neighbour becomes a passable door (open/locked/bombable/shutter — never a wall), so this is
    /// the set of edges the player can actually enter and leave the room through.
    /// </summary>
    private static HashSet<string> GetRoomDoorDirections(Room room)
    {
        var directions = new HashSet<string>();
        foreach (var neighbor in room.Neighbors)
        {
            string dir = GetDirection(room, neighbor);
            if (!string.IsNullOrEmpty(dir))
                directions.Add(dir);
        }
        return directions;
    }

    // Orphaned item recovery

    /// <summary>
    /// Last line of defence against item locations the player could never reach. After screens,
    /// doors, items and item-position slots are all decided, a room's item can still land in a
    /// walkable region that no door connects to — even assuming every item in the game (the
    /// reachability search below walks all edges, including conditional ones like the stepladder).
    /// The graph filler already refuses to put progression there, but a stranded location still
    /// looks like a bug to players. For each such room, swap to a drop-in compatible screen where
    /// the item is reachable; if none exists, leave it (best effort — no worse than before).
    /// </summary>
    private void FixOrphanedItemLocations()
    {
        var connectivity = PrecalculateScreenConnectivity();
        var slots = _itemPositionSlots;

        // Plain item rooms only: start/end/boss/check rooms and cellars use fixed special screens
        // (and boss enemy placement is screen-coupled), so they're not candidates for a free swap.
        var itemRooms = _map.UsedNonCellarRooms
            .Where(r => r.HasAnyRole(RoomRole.Item)
                     && !r.HasAnyRole(RoomRole.Boss | RoomRole.End | RoomRole.Start
                                      | RoomRole.LevelNineCheck | RoomRole.Connector | RoomRole.Stairs))
            .ToList();

        foreach (var room in itemRooms)
        {
            var doorDirs = GetRoomDoorDirections(room);
            var currentScreen = GetUnderworldScreen(room.Screen);
            if (currentScreen == null || ItemReachableOnScreen(currentScreen, room, doorDirs, slots))
                continue;

            // Prefer the simplest compatible screen (fewest regions) — those have the least chance
            // of stranding the item or the room's enemies somewhere else.
            var replacement = _data.underworld_screens
                .Where(s => s.screen != room.Screen)
                .Where(s => ScreenFitsRoom(s, room, connectivity[s.screen]))
                .Where(s => SwapKeepsEnemyValid(room, s))
                .Where(s => SwapKeepsBehaviourValid(room, s))
                .Where(s => ItemReachableOnScreen(s, room, doorDirs, slots))
                .OrderBy(s => s.nodes.regions.Count(r => r.type == YamlReader.RegionType.Region))
                .ToList();

            if (replacement.Count == 0)
                continue;

            int fewestRegions = replacement[0].nodes.regions.Count(r => r.type == YamlReader.RegionType.Region);
            var best = replacement
                .Where(s => s.nodes.regions.Count(r => r.type == YamlReader.RegionType.Region) == fewestRegions)
                .ToList();

            room.Screen = best[_rnd.Next(best.Count)].screen;
            room.PushBlock = room.Behaviour is YamlReader.RoomBehaviour.PushBlockShutter or YamlReader.RoomBehaviour.PushBlockStairs
                || room.Screen == ScreenId.PushCross || room.Screen == ScreenId.PushStairs;
        }
    }

    /// <summary>True if the room's item position on the given screen sits in a region reachable
    /// from one of the room's doors (walking all edges, i.e. assuming every item is owned).</summary>
    private bool ItemReachableOnScreen(YamlReader.Screen screen, Room room, HashSet<string> doorDirs, IReadOnlyList<int> slots)
    {
        if (!TryGetItemPosition(slots, screen, out int pos))
            return false; // no slot is even walkable here — Write would throw, so this screen is unusable
        var region = GetItemRegion(screen, pos);
        return region != null && IsRegionReachableFromDoors(screen, doorDirs, region);
    }

    /// <summary>Resolves the encoded item position a room would use on a screen (first walkable
    /// slot, matching <see cref="GetItemPositionSlot"/>), without throwing when none fits.</summary>
    private static bool TryGetItemPosition(IReadOnlyList<int> slots, YamlReader.Screen? screen, out int encodedPos)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (IsValidItemPosition(screen, slots[i]))
            {
                encodedPos = slots[i];
                return true;
            }
        }
        encodedPos = 0;
        return false;
    }

    private static YamlReader.Region? GetItemRegion(YamlReader.Screen screen, int encodedPos)
    {
        int x = (encodedPos >> 4) - 2, y = (encodedPos & 0x0F) - 6;
        return screen.nodes.regions.FirstOrDefault(r =>
            r.type == YamlReader.RegionType.Region &&
            r.from[0] <= x && r.from[1] <= y && r.to[0] >= x && r.to[1] >= y);
    }

    private static bool IsRegionReachableFromDoors(YamlReader.Screen screen, HashSet<string> doorDirs, YamlReader.Region region)
    {
        var entranceByDir = screen.nodes.exits
            .Where(e => e.type == YamlReader.ExitType.Entrance)
            .GroupBy(e => YamlDirectionToString(e.direction))
            .ToDictionary(g => g.Key, g => g.First().name);

        return doorDirs.Any(d => entranceByDir.TryGetValue(d, out var entrance)
            && HasPathBetween(screen, entrance, region.name));
    }

    /// <summary>
    /// True if an item at <paramref name="encodedItemPos"/> on the screen can be reached from at
    /// least one of the room's open doors, walking every edge (i.e. assuming all items are owned).
    /// <paramref name="doorTypes"/> is indexed N, S, W, E as in <c>UnderworldMap.doors</c>. Exposed
    /// for tests so they can assert generated item locations are never orphaned.
    /// </summary>
    internal static bool IsItemPositionReachable(YamlReader.Screen screen, IReadOnlyList<int> doorTypes, int encodedItemPos)
    {
        var doorDirs = new HashSet<string>();
        if ((YamlReader.DoorType)doorTypes[0] != YamlReader.DoorType.Wall) doorDirs.Add("N");
        if ((YamlReader.DoorType)doorTypes[1] != YamlReader.DoorType.Wall) doorDirs.Add("S");
        if ((YamlReader.DoorType)doorTypes[2] != YamlReader.DoorType.Wall) doorDirs.Add("W");
        if ((YamlReader.DoorType)doorTypes[3] != YamlReader.DoorType.Wall) doorDirs.Add("E");

        var region = GetItemRegion(screen, encodedItemPos);
        return region != null && IsRegionReachableFromDoors(screen, doorDirs, region);
    }

    /// <summary>True if the room's already-assigned enemy still fits a candidate screen, so the
    /// swap doesn't need to re-roll enemies. Evaluated with the room temporarily on that screen.</summary>
    private bool SwapKeepsEnemyValid(Room room, YamlReader.Screen screen)
    {
        if (room.EnemyId < 0)
            return true;

        int original = room.Screen;
        room.Screen = screen.screen;
        try
        {
            if (!EnemyFitsRoom((room.EnemyCount, room.EnemyId, room.EnemyMode), room, _level))
                return false;
            // If shutters can only open by clearing the room, the enemy must stay killable there.
            bool needsClear = room.Doors.Values.Any(d => d == YamlReader.DoorType.Shutter)
                && !RoomSupportsPushBlockShutters(room);
            return !needsClear || RoomHasKillableEnemies(room);
        }
        finally
        {
            room.Screen = original;
        }
    }

    /// <summary>True if the candidate screen still supports whatever trigger the room's resolved
    /// behaviour relies on, so the swap doesn't have to re-resolve behaviours.</summary>
    private bool SwapKeepsBehaviourValid(Room room, YamlReader.Screen screen)
    {
        int original = room.Screen;
        room.Screen = screen.screen;
        try
        {
            return room.Behaviour switch
            {
                YamlReader.RoomBehaviour.KillForItem or YamlReader.RoomBehaviour.KillForShutter
                    => RoomSupportsKillShutters(room) && RoomHasKillableEnemies(room),
                YamlReader.RoomBehaviour.PushBlockShutter => RoomSupportsPushBlockShutters(room),
                YamlReader.RoomBehaviour.PushBlockStairs => RoomSupportsHiddenStairs(room),
                _ => true,
            };
        }
        finally
        {
            room.Screen = original;
        }
    }

    private bool ScreenFitsRoom(YamlReader.Screen screen, Room room, HashSet<(string, string)> connectivity)
    {
        if (screen.screen == ScreenId.ZeldaRoom || screen.screen == ScreenId.GanonRoom || screen.screen == ScreenId.TriforceRoom)
            return false;

        if (room.HasAnyRole(RoomRole.Cellar))
            return false;

        bool hasVisibleStairs = ScreenHasMeta(screen, YamlReader.MetaType.Stairs);
        bool supportsHiddenStairs = _pushBlockStairsScreens.Contains(screen.screen) && ScreenHasMeta(screen, YamlReader.MetaType.Push);

        if (room.HasAnyRole(RoomRole.Connector | RoomRole.Stairs))
        {
            if (!hasVisibleStairs && !supportsHiddenStairs)
                return false;
        }
        else if (hasVisibleStairs)
        {
            return false;
        }

        if (room.HasAnyRole(RoomRole.End) && !(screen.nodes.meta?.Any(m => m.type == YamlReader.MetaType.Meta && m.name == "Triforce") ?? false))
            return false;

        if (room.HasAnyRole(RoomRole.Start) && screen.screen != 0)
            return false;

        // Screen 0x1B has a wall structure on the right side that blocks the east doorway
        if (screen.screen == 0x1B && room.Neighbors.Any(n => n.X == room.X + 1 && n.Y == room.Y))
            return false;

        var directions = GetRoomDoorDirections(room);

        // Don't use a blocked-passage screen unless a door actually opens onto the passage,
        // otherwise the walled-off middle (and its item/enemies) is unreachable.
        if (BlockedPassageScreenDoors.TryGetValue(screen.screen, out var passageDoors)
            && !passageDoors.Any(directions.Contains))
            return false;

        // A dead-end room (single connection) is only usable on a screen that lets you enter
        // and leave through that same edge. Without this check, push-style screens (which only
        // route to perpendicular exits) get assigned to dead-ends and trap the room.
        if (directions.Count == 1)
        {
            var dir = directions.First();
            return connectivity.Contains((dir, dir));
        }

        foreach (var dir1 in directions)
        {
            foreach (var dir2 in directions)
            {
                if (dir1 == dir2)
                    continue;

                if (!connectivity.Contains((dir1, dir2)) && !connectivity.Contains((dir2, dir1)))
                    return false;
            }
        }

        return true;
    }

    // Graph utilities

    private List<(int dist, Room room)> Distances(Room startRoom, bool withinSegment = false)
    {
        var visited = new HashSet<Room> { startRoom };
        var queue = new Queue<(int dist, Room room)>();
        var result = new List<(int dist, Room room)>();
        queue.Enqueue((0, startRoom));

        while (queue.Count > 0)
        {
            var (dist, room) = queue.Dequeue();
            result.Add((dist, room));
            foreach (var neighbor in room.Neighbors)
            {
                if (withinSegment && neighbor.Segment != startRoom.Segment) continue;
                if (visited.Add(neighbor))
                    queue.Enqueue((dist + 1, neighbor));
            }
        }
        return result;
    }

    private List<Room> GetShortestPath(Room start, Room target)
    {
        var visited = new HashSet<Room> { start };
        var queue = new Queue<Room>();
        var parentMap = new Dictionary<Room, Room>();
        queue.Enqueue(start);
        Room? current = null;

        while (queue.Count > 0)
        {
            current = queue.Dequeue();
            if (current == target) break;
            foreach (var neighbor in current.Neighbors)
            {
                if (visited.Add(neighbor))
                {
                    parentMap[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        if (current != target) return [];

        var path = new List<Room>();
        for (var step = target; step != start; step = parentMap[step])
            path.Add(step);
        path.Add(start);
        path.Reverse();
        return path;
    }

    private Dictionary<Room, int> CalculateDistancesFromPath(HashSet<Room> criticalPathSet)
    {
        var distances = new Dictionary<Room, int>();
        var queue = new Queue<Room>();

        foreach (var room in criticalPathSet)
        {
            distances[room] = 0;
            queue.Enqueue(room);
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            int currentDist = distances[current];
            foreach (var neighbor in current.Neighbors)
            {
                if (!distances.ContainsKey(neighbor))
                {
                    distances[neighbor] = currentDist + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }

        foreach (var room in _map.UsedRooms)
        {
            if (!distances.ContainsKey(room))
                distances[room] = int.MaxValue;
        }

        return distances;
    }

}
