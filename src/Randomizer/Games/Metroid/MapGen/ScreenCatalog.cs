namespace Randomizer.Games.Metroid.MapGen;

using System;
using System.Collections.Generic;
using System.Linq;
using static Randomizer.Games.Metroid.YamlReader;

/// <summary>
/// The kind of connector a screen exposes on one of its four edges.
/// </summary>
public enum ConnectorType
{
    None,
    Scroll,
    Door,
    Elevator,
    Tunnel
}

/// <summary>
/// A single edge connector of a screen: its type and, for doors, the door color.
/// </summary>
public readonly record struct EdgeConnector(ConnectorType Type, DoorType? Color = null)
{
    public static readonly EdgeConnector None = new(ConnectorType.None);
    public static readonly EdgeConnector Scroll = new(ConnectorType.Scroll);
    public static readonly EdgeConnector Elevator = new(ConnectorType.Elevator);
    public static readonly EdgeConnector Tunnel = new(ConnectorType.Tunnel);
    public static EdgeConnector Door(DoorType color) => new(ConnectorType.Door, color);

    public override string ToString() => Type == ConnectorType.Door ? $"Door({Color})" : Type.ToString();
}

/// <summary>
/// Normalized classification of one vanilla screen, derived from the screen YAML.
/// This is the "puzzle piece" description used by topology generation and screen fitting.
/// </summary>
public class ScreenProfile
{
    public required Area Area { get; init; }
    public required int ScreenId { get; init; }
    public required string Name { get; init; }
    public required Scrolling Axis { get; init; }

    public EdgeConnector Up { get; init; } = EdgeConnector.None;
    public EdgeConnector Down { get; init; } = EdgeConnector.None;
    public EdgeConnector Left { get; init; } = EdgeConnector.None;
    public EdgeConnector Right { get; init; } = EdgeConnector.None;

    public bool HasItemLocation { get; init; }
    public bool HasBossLocation { get; init; }
    public bool HasStartLocation { get; init; }
    public bool HasElevatorPlatform { get; init; }
    public bool HasMetaLocation { get; init; }

    /// <summary>
    /// True when the item orb/pedestal is drawn into the screen's room structure (rather than
    /// only via the special-items table, e.g. Brinstar 0x2A's pillars). Such a screen always
    /// shows the orb, so it must only be placed on an Item cell — on a plain corridor it would
    /// render a phantom orb with no item behind it.
    /// </summary>
    public bool HasStructuralItem { get; init; }

    public List<string> ItemLocationNames { get; init; } = [];
    public List<string> ElevatorLocationNames { get; init; } = [];
    public string? StartLocationName { get; init; }

    /// <summary>
    /// The engine position byte for the screen's item location, derived from the location's
    /// YAML position [x, y] in 16px tiles: (y &lt;&lt; 4) | x, the PowerUpHandler format
    /// (verified against the vanilla tables: chozo orbs [7,3] = $37, morph pedestal
    /// [6,9] = $96, Brinstar lava pillar [7,6] = $67).
    /// </summary>
    public byte? ItemPosition { get; init; }

    /// <summary>Door node names from the screen YAML, for building full vertex names.</summary>
    public string? LeftDoorName { get; init; }
    public string? RightDoorName { get; init; }

    /// <summary>Edges that also have one or more morph tunnel openings (screen-internal, ability gated).</summary>
    public HashSet<Direction> TunnelEdges { get; init; } = [];

    /// <summary>No connectors at all; usable only as a solid wall cap inside a run.</summary>
    public bool IsCap { get; init; }

    /// <summary>
    /// True if the screen has scroll exits on both ends of its axis but internal traversal
    /// is only possible in one direction (e.g. Ridley 0x04 fall shaft).
    /// </summary>
    public bool IsOneWay { get; init; }

    /// <summary>For one-way screens, the only direction traversal is possible (axis-aligned).</summary>
    public Direction? OneWayDirection { get; init; }

    /// <summary>
    /// Ordered pairs of edges connected by the screen's internal edges under full inventory.
    /// Some screens are internally partial (e.g. Ridley 0x0A's right door only reaches its
    /// top-right opening), so a screen is only safe for a cell if every pair of the cell's
    /// committed edges is connected both ways.
    /// </summary>
    public HashSet<(Direction From, Direction To)> ConnectedEdgePairs { get; init; } = [];

    /// <summary>True if traversal between the two edges is possible in both directions.</summary>
    public bool EdgesConnected(Direction a, Direction b) =>
        ConnectedEdgePairs.Contains((a, b)) && ConnectedEdgePairs.Contains((b, a));

    /// <summary>
    /// Like <see cref="ConnectedEdgePairs"/> but with zero equipment: only requirement-free
    /// edges count. Screens missing a free pair are fall pieces or item-gated climbs —
    /// legal, but the fitter keeps them rare.
    /// </summary>
    public HashSet<(Direction From, Direction To)> FreeEdgePairs { get; init; } = [];

    /// <summary>True if traversal between the two edges needs no items, in both directions.</summary>
    public bool EdgesFreelyConnected(Direction a, Direction b) =>
        FreeEdgePairs.Contains((a, b)) && FreeEdgePairs.Contains((b, a));

    public EdgeConnector Connector(Direction dir) => dir switch
    {
        Direction.Up => Up,
        Direction.Down => Down,
        Direction.Left => Left,
        Direction.Right => Right,
        _ => throw new ArgumentOutOfRangeException(nameof(dir))
    };

    public override string ToString() =>
        $"{Area} 0x{ScreenId:X2} '{Name}' {Axis} U:{Up} D:{Down} L:{Left} R:{Right}" +
        $"{(HasItemLocation ? " [Item]" : "")}{(HasBossLocation ? " [Boss]" : "")}{(HasStartLocation ? " [Start]" : "")}" +
        $"{(HasElevatorPlatform ? " [ElevPlatform]" : "")}{(IsCap ? " [Cap]" : "")}{(IsOneWay ? $" [OneWay:{OneWayDirection}]" : "")}";
}

/// <summary>
/// What a topology cell requires from a screen edge. Wall accepts a missing connector
/// or any extra opening that will face an empty cell (the engine seals those).
/// </summary>
public enum EdgeRequirement
{
    Wall,
    Scroll,
    Door,
    Elevator
}

/// <summary>
/// The kind of run a door faces. The M1 engine has two door tile types: $A0 (scroll axis
/// flips on exit) and $A1 (scroll axis forced horizontal on exit, used by item rooms).
/// The type is baked into each screen's tile data, so a door is only traversable when its
/// neighbor matches the kind of neighbor it had in vanilla. Single-cell rooms never scroll,
/// so any door can face one.
/// </summary>
public enum RunKind
{
    SingleCell,
    MultiHorizontal,
    MultiVertical
}

/// <summary>
/// Loads all screen YAML data into normalized <see cref="ScreenProfile"/>s, validates the
/// engine invariants, and answers vocabulary queries ("does area X have a piece shaped like Y?").
/// </summary>
public class ScreenCatalog
{
    private readonly List<ScreenProfile> profiles = [];
    private readonly Dictionary<Area, List<ScreenProfile>> byArea = [];
    private readonly Dictionary<(Area, int, Direction), HashSet<RunKind>> doorContexts = [];

    public IReadOnlyList<ScreenProfile> Profiles => profiles;

    public static readonly Area[] PlayableAreas = [Area.Brinstar, Area.Norfair, Area.Kraid, Area.Ridley, Area.Tourian];

    public static ScreenCatalog Build(IEnumerable<Screen> screens, IEnumerable<Room>? vanillaRooms = null)
    {
        var catalog = new ScreenCatalog();
        var errors = new List<string>();

        foreach (var screen in screens)
        {
            if (screen.area == Area.Meta)
                continue;

            var profile = BuildProfile(screen, errors);
            if (profile != null)
                catalog.profiles.Add(profile);
        }

        foreach (var area in PlayableAreas)
            catalog.byArea[area] = catalog.profiles.Where(p => p.Area == area).ToList();

        if (vanillaRooms != null)
            catalog.BuildDoorContexts(vanillaRooms.ToList());

        catalog.Validate(errors);

        if (errors.Count > 0)
            throw new InvalidOperationException("Screen catalog validation failed:\n" + string.Join("\n", errors));

        return catalog;
    }

    /// <summary>
    /// Learns, for every door of every screen, what kind of run it faced in vanilla.
    /// This encodes the $A0/$A1 door tile behavior without needing the tile data itself:
    /// a door is safe to pair with a multi-cell run only if it did so in vanilla.
    /// </summary>
    private void BuildDoorContexts(List<Room> rooms)
    {
        // Map every vanilla coordinate to the run kind of the room covering it.
        var occupancy = new Dictionary<(int X, int Y), RunKind>();
        foreach (var room in rooms.Where(r => r.area != Area.Meta))
        {
            var kind = room.screens.Length == 1 ? RunKind.SingleCell
                : room.scroll == Scrolling.Horizontal ? RunKind.MultiHorizontal : RunKind.MultiVertical;
            for (int i = 0; i < room.screens.Length; i++)
            {
                var (x, y) = room.scroll == Scrolling.Horizontal
                    ? (room.position[0] + i, room.position[1])
                    : (room.position[0], room.position[1] + i);
                occupancy[(x, y)] = kind;
            }
        }

        foreach (var room in rooms.Where(r => r.area != Area.Meta))
        {
            for (int i = 0; i < room.screens.Length; i++)
            {
                var profile = Find(room.area, room.screens[i]);
                if (profile == null)
                    continue;

                var (x, y) = room.scroll == Scrolling.Horizontal
                    ? (room.position[0] + i, room.position[1])
                    : (room.position[0], room.position[1] + i);

                foreach (var dir in new[] { Direction.Left, Direction.Right })
                {
                    if (profile.Connector(dir).Type != ConnectorType.Door)
                        continue;

                    var neighbor = dir == Direction.Left ? (x - 1, y) : (x + 1, y);
                    if (!occupancy.TryGetValue(neighbor, out var kind))
                        continue; // door faced an empty cell in vanilla (sealed)

                    var key = (room.area, room.screens[i], dir);
                    if (!doorContexts.TryGetValue(key, out var set))
                        doorContexts[key] = set = [];
                    set.Add(kind);
                }
            }
        }
    }

    /// <summary>The run kinds this screen's door faced in vanilla; empty if never used.</summary>
    public IReadOnlySet<RunKind> GetDoorContexts(Area area, int screenId, Direction side) =>
        doorContexts.GetValueOrDefault((area, screenId, side)) ?? (IReadOnlySet<RunKind>)new HashSet<RunKind>();

    /// <summary>
    /// True if this screen's door on <paramref name="side"/> may face a run of
    /// <paramref name="neighborKind"/>: single-cell rooms never scroll so they are always
    /// safe; multi-cell runs require a matching vanilla precedent for the door tile type.
    /// </summary>
    public bool DoorAccepts(ScreenProfile profile, Direction side, RunKind neighborKind) =>
        neighborKind == RunKind.SingleCell
        || GetDoorContexts(profile.Area, profile.ScreenId, side).Contains(neighborKind);

    private static ScreenProfile? BuildProfile(Screen screen, List<string> errors)
    {
        string id = $"{screen.area} 0x{screen.screen:X2} '{screen.name}'";

        // An edge can legitimately have several openings (two scroll gaps, scroll plus a
        // morph tunnel at a different height, several tunnels). Collect everything first,
        // then resolve each edge to one structural connector.
        var openings = new Dictionary<Direction, List<EdgeConnector>>();

        void AddOpening(Direction dir, EdgeConnector connector)
        {
            if (!openings.TryGetValue(dir, out var list))
                openings[dir] = list = [];
            list.Add(connector);
        }

        foreach (var door in screen.nodes?.doors ?? [])
        {
            if (door.direction is Direction.Up or Direction.Down)
            {
                errors.Add($"{id}: illegal {door.direction} door '{door.name}' (doors must be left/right)");
                continue;
            }
            AddOpening(door.direction, EdgeConnector.Door(door.type));
        }

        foreach (var exit in screen.nodes?.exits ?? [])
        {
            switch (exit.type)
            {
                case ExitType.Scroll:
                    bool axisOk = screen.scroll == Scrolling.Vertical
                        ? exit.direction is Direction.Up or Direction.Down
                        : exit.direction is Direction.Left or Direction.Right;
                    if (!axisOk)
                        errors.Add($"{id}: scroll exit '{exit.name}' direction {exit.direction} does not match {screen.scroll} axis");
                    AddOpening(exit.direction, EdgeConnector.Scroll);
                    break;

                case ExitType.Elevator:
                    if (exit.direction is not (Direction.Up or Direction.Down))
                        errors.Add($"{id}: elevator exit '{exit.name}' must be vertical, was {exit.direction}");
                    AddOpening(exit.direction, EdgeConnector.Elevator);
                    break;

                case ExitType.Tunnel:
                    AddOpening(exit.direction, EdgeConnector.Tunnel);
                    break;
            }
        }

        var connectors = new Dictionary<Direction, EdgeConnector>();
        var tunnelEdges = new HashSet<Direction>();

        foreach (var (dir, list) in openings)
        {
            if (list.Any(c => c.Type == ConnectorType.Tunnel))
                tunnelEdges.Add(dir);

            var doors = list.Where(c => c.Type == ConnectorType.Door).ToList();
            var elevators = list.Where(c => c.Type == ConnectorType.Elevator).ToList();
            var scrolls = list.Where(c => c.Type == ConnectorType.Scroll).ToList();

            // Doors, elevators and scroll openings are mutually exclusive transition types on
            // one edge; tunnels can coexist with anything since they are screen internals.
            int structuralKinds = (doors.Count > 0 ? 1 : 0) + (elevators.Count > 0 ? 1 : 0) + (scrolls.Count > 0 ? 1 : 0);
            if (structuralKinds > 1 || doors.Count > 1 || elevators.Count > 1)
            {
                errors.Add($"{id}: conflicting connectors on edge {dir}: {string.Join(", ", list)}");
                continue;
            }

            if (doors.Count == 1)
                connectors[dir] = doors[0];
            else if (elevators.Count == 1)
                connectors[dir] = elevators[0];
            else if (scrolls.Count > 0)
                connectors[dir] = EdgeConnector.Scroll;
            else
                connectors[dir] = EdgeConnector.Tunnel;
        }

        var locations = screen.nodes?.locations ?? [];
        var itemLocations = locations.Where(l => l.type == LocationType.Item).ToList();

        var (oneWay, oneWayDir) = DetectOneWay(screen);
        var connectedPairs = ComputeConnectedEdgePairs(screen);

        return new ScreenProfile
        {
            Area = screen.area,
            ScreenId = screen.screen,
            Name = screen.name,
            Axis = screen.scroll,
            Up = connectors.GetValueOrDefault(Direction.Up, EdgeConnector.None),
            Down = connectors.GetValueOrDefault(Direction.Down, EdgeConnector.None),
            Left = connectors.GetValueOrDefault(Direction.Left, EdgeConnector.None),
            Right = connectors.GetValueOrDefault(Direction.Right, EdgeConnector.None),
            HasItemLocation = itemLocations.Count > 0,
            HasStructuralItem = screen.structuralItem,
            HasBossLocation = locations.Any(l => l.type == LocationType.Boss),
            HasStartLocation = locations.Any(l => l.type == LocationType.Start),
            HasElevatorPlatform = locations.Any(l => l.type == LocationType.Elevator),
            HasMetaLocation = locations.Any(l => l.type == LocationType.Meta),
            ItemLocationNames = itemLocations.Select(l => l.name).ToList(),
            ElevatorLocationNames = locations.Where(l => l.type == LocationType.Elevator).Select(l => l.name).ToList(),
            StartLocationName = locations.FirstOrDefault(l => l.type == LocationType.Start)?.name,
            ItemPosition = itemLocations.FirstOrDefault()?.position is [var ix, var iy]
                ? (byte)(iy << 4 | ix) : null,
            LeftDoorName = screen.nodes?.doors?.FirstOrDefault(d => d.direction == Direction.Left)?.name,
            RightDoorName = screen.nodes?.doors?.FirstOrDefault(d => d.direction == Direction.Right)?.name,
            TunnelEdges = tunnelEdges,
            IsCap = connectors.Count == 0,
            IsOneWay = oneWay,
            OneWayDirection = oneWayDir,
            ConnectedEdgePairs = connectedPairs,
            FreeEdgePairs = ComputeConnectedEdgePairs(screen, freeOnly: true)
        };
    }

    /// <summary>
    /// Computes which edge pairs the screen connects internally, treating every requirement
    /// as satisfied (full inventory) and respecting directed edges. Edge node sets include
    /// doors and all exits on that side (tunnels included: with Morph they are passages).
    /// With <paramref name="freeOnly"/> only requirement-free edges count, i.e. traversal
    /// with zero equipment.
    /// </summary>
    private static HashSet<(Direction, Direction)> ComputeConnectedEdgePairs(Screen screen, bool freeOnly = false)
    {
        var nodesByDir = new Dictionary<Direction, List<string>>();
        void Add(Direction dir, string name)
        {
            if (!nodesByDir.TryGetValue(dir, out var list))
                nodesByDir[dir] = list = [];
            list.Add(name);
        }

        foreach (var door in screen.nodes?.doors ?? [])
            Add(door.direction, door.name);
        foreach (var exit in screen.nodes?.exits ?? [])
            Add(exit.direction, exit.name);

        var reach = BuildInternalReachability(screen, freeOnly);
        var pairs = new HashSet<(Direction, Direction)>();

        foreach (var (fromDir, fromNodes) in nodesByDir)
        {
            foreach (var (toDir, toNodes) in nodesByDir)
            {
                if (fromDir == toDir)
                    continue;
                if (fromNodes.Any(f => reach.TryGetValue(f, out var set) && toNodes.Any(set.Contains)))
                    pairs.Add((fromDir, toDir));
            }
        }

        return pairs;
    }

    /// <summary>
    /// A screen is one-way if it has scroll exits on both ends of its axis but its internal
    /// edges (under any requirement, i.e. full inventory) only allow traversal one way.
    /// </summary>
    private static (bool, Direction?) DetectOneWay(Screen screen)
    {
        var scrollExits = (screen.nodes?.exits ?? []).Where(e => e.type == ExitType.Scroll).ToList();
        var (negDir, posDir) = screen.scroll == Scrolling.Vertical
            ? (Direction.Up, Direction.Down)
            : (Direction.Left, Direction.Right);

        var negExits = scrollExits.Where(e => e.direction == negDir).ToList();
        var posExits = scrollExits.Where(e => e.direction == posDir).ToList();
        if (negExits.Count == 0 || posExits.Count == 0)
            return (false, null);

        var reach = BuildInternalReachability(screen);
        bool Reaches(List<Exit> from, List<Exit> to) => from.Any(f =>
            reach.TryGetValue(f.name, out var set) && to.Any(t => set.Contains(t.name)));

        bool negToPos = Reaches(negExits, posExits);
        bool posToNeg = Reaches(posExits, negExits);

        if (negToPos && !posToNeg)
            return (true, posDir);
        if (posToNeg && !negToPos)
            return (true, negDir);
        return (false, null);
    }

    /// <summary>
    /// Computes node-to-node reachability inside one screen using its YAML edges,
    /// treating every requirement as satisfied (full inventory), or with
    /// <paramref name="freeOnly"/> only following requirement-free ("fixed") edges.
    /// </summary>
    private static Dictionary<string, HashSet<string>> BuildInternalReachability(Screen screen, bool freeOnly = false)
    {
        var adjacency = new Dictionary<string, HashSet<string>>();

        void Add(string from, string to)
        {
            if (!adjacency.TryGetValue(from, out var set))
                adjacency[from] = set = [];
            set.Add(to);
        }

        foreach (var (key, edgeList) in screen.edges?.undirected ?? [])
        {
            if (freeOnly && key != "fixed")
                continue;
            foreach (var edge in edgeList)
            {
                var pair = ((List<object>)edge).Cast<string>().ToList();
                Add(pair[0], pair[1]);
                Add(pair[1], pair[0]);
            }
        }

        foreach (var (key, edgeList) in screen.edges?.directed ?? [])
        {
            if (freeOnly && key != "fixed")
                continue;
            foreach (var edge in edgeList)
            {
                var pair = ((List<object>)edge).Cast<string>().ToList();
                Add(pair[0], pair[1]);
            }
        }

        var result = new Dictionary<string, HashSet<string>>();
        foreach (var start in adjacency.Keys)
        {
            var visited = new HashSet<string> { start };
            var queue = new Queue<string>([start]);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                foreach (var next in adjacency.GetValueOrDefault(node) ?? [])
                {
                    if (visited.Add(next))
                        queue.Enqueue(next);
                }
            }
            result[start] = visited;
        }

        return result;
    }

    private void Validate(List<string> errors)
    {
        foreach (var area in PlayableAreas)
        {
            if (!byArea.TryGetValue(area, out var areaProfiles) || areaProfiles.Count == 0)
            {
                errors.Add($"Catalog has no screens for area {area}");
                continue;
            }

            // Each playable area needs at least one usable two-way vertical shaft body.
            bool hasShaftBody = areaProfiles.Any(p =>
                p.Axis == Scrolling.Vertical &&
                p.Up.Type == ConnectorType.Scroll &&
                p.Down.Type == ConnectorType.Scroll &&
                !p.IsOneWay);
            if (!hasShaftBody)
                errors.Add($"Area {area} has no usable two-way vertical shaft body screen");
        }
    }

    public IReadOnlyList<ScreenProfile> ForArea(Area area) => byArea.GetValueOrDefault(area) ?? [];

    public ScreenProfile? Find(Area area, int screenId) =>
        byArea.GetValueOrDefault(area)?.FirstOrDefault(p => p.ScreenId == screenId);

    /// <summary>
    /// True if <paramref name="profile"/> satisfies the given per-edge requirements.
    /// Scroll/Door/Elevator requirements must be matched exactly on that edge; Wall edges
    /// accept any connector because an opening facing an empty cell is sealed by the engine
    /// (whether the facing cell really is empty is checked by the fitter, not here).
    /// </summary>
    public static bool Satisfies(ScreenProfile profile, Scrolling axis,
        EdgeRequirement up, EdgeRequirement down, EdgeRequirement left, EdgeRequirement right,
        bool requireTwoWay = true)
    {
        if (profile.Axis != axis)
            return false;
        if (requireTwoWay && profile.IsOneWay)
            return false;

        if (!(EdgeOk(profile.Up, up) && EdgeOk(profile.Down, down)
            && EdgeOk(profile.Left, left) && EdgeOk(profile.Right, right)))
            return false;

        // Every pair of committed edges must be internally connected both ways, or the
        // screen would sever the run/door chain it is placed into.
        var committed = new List<Direction>(4);
        if (up != EdgeRequirement.Wall) committed.Add(Direction.Up);
        if (down != EdgeRequirement.Wall) committed.Add(Direction.Down);
        if (left != EdgeRequirement.Wall) committed.Add(Direction.Left);
        if (right != EdgeRequirement.Wall) committed.Add(Direction.Right);

        for (int i = 0; i < committed.Count; i++)
            for (int j = i + 1; j < committed.Count; j++)
                if (!profile.EdgesConnected(committed[i], committed[j]))
                    return false;

        return true;

        static bool EdgeOk(EdgeConnector actual, EdgeRequirement required) => required switch
        {
            // Only scroll openings on Wall edges are sealed by the engine; a door still
            // scrolls Samus out of bounds and an elevator hole has no platform (mirrors
            // WorldGrid.FitsStrict for unforced cells).
            EdgeRequirement.Wall => actual.Type is not (ConnectorType.Door or ConnectorType.Elevator),
            EdgeRequirement.Scroll => actual.Type == ConnectorType.Scroll,
            EdgeRequirement.Door => actual.Type == ConnectorType.Door,
            EdgeRequirement.Elevator => actual.Type == ConnectorType.Elevator,
            _ => false
        };
    }

    /// <summary>
    /// All screens in <paramref name="area"/> that satisfy the given shape, optionally
    /// requiring that door edges accept the given neighbor run kinds. Used by the topology
    /// generator to ensure it never plans a cell no real screen can fill.
    /// </summary>
    public List<ScreenProfile> Query(Area area, Scrolling axis,
        EdgeRequirement up, EdgeRequirement down, EdgeRequirement left, EdgeRequirement right,
        bool requireTwoWay = true, RunKind? leftNeighbor = null, RunKind? rightNeighbor = null)
    {
        return ForArea(area)
            .Where(p => Satisfies(p, axis, up, down, left, right, requireTwoWay)
                && (left != EdgeRequirement.Door || leftNeighbor == null || DoorAccepts(p, Direction.Left, leftNeighbor.Value))
                && (right != EdgeRequirement.Door || rightNeighbor == null || DoorAccepts(p, Direction.Right, rightNeighbor.Value)))
            .ToList();
    }

    public bool HasPiece(Area area, Scrolling axis,
        EdgeRequirement up, EdgeRequirement down, EdgeRequirement left, EdgeRequirement right,
        bool requireTwoWay = true, RunKind? leftNeighbor = null, RunKind? rightNeighbor = null)
    {
        return ForArea(area).Any(p =>
            Satisfies(p, axis, up, down, left, right, requireTwoWay)
            && (left != EdgeRequirement.Door || leftNeighbor == null || DoorAccepts(p, Direction.Left, leftNeighbor.Value))
            && (right != EdgeRequirement.Door || rightNeighbor == null || DoorAccepts(p, Direction.Right, rightNeighbor.Value)));
    }

    /// <summary>Multi-line human-readable dump of the entire catalog, for diagnostics.</summary>
    public string Dump()
    {
        var lines = new List<string>();
        foreach (var area in PlayableAreas)
        {
            lines.Add($"=== {area} ({ForArea(area).Count} screens) ===");
            foreach (var p in ForArea(area).OrderBy(p => p.ScreenId))
                lines.Add("  " + p);
        }
        return string.Join("\n", lines);
    }
}
