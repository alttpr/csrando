namespace Randomizer.Games.Metroid.MapGen;

using System;
using System.Collections.Generic;
using System.Linq;
using static Randomizer.Games.Metroid.YamlReader;

/// <summary>Role of an abstract cell, used for screen capability matching and diagnostics.</summary>
public enum CellRole
{
    /// <summary>Body cell of a vertical shaft.</summary>
    Shaft,
    /// <summary>Body cell of a horizontal corridor.</summary>
    Corridor,
    /// <summary>Solid wall terminator cell inside a run (vanilla places cap screens as real cells).</summary>
    Cap,
    /// <summary>Single vertical cell with left+right doors joining two horizontal features.</summary>
    DoorTube,
    /// <summary>Cell that must host an item location.</summary>
    Item,
    /// <summary>Cell that must host a boss location.</summary>
    Boss,
    /// <summary>The start cell (Brinstar 0x09).</summary>
    Start,
    /// <summary>Upper half of an elevator pair (elevator exit on the bottom edge).</summary>
    ElevatorTop,
    /// <summary>Lower half of an elevator pair (elevator exit on the top edge).</summary>
    ElevatorBottom,
    /// <summary>The statues/Silver Two gate cell guarding Tourian.</summary>
    Gate,
    /// <summary>Post-Mother-Brain escape cell (non-progression in phase 1).</summary>
    Escape,
    /// <summary>A cross-game portal room (combo mode anchors transitions here).</summary>
    Portal
}

/// <summary>A grid coordinate. X is the column (0-31), Y is the row (0-31, increasing downward).</summary>
public readonly record struct Point(int X, int Y)
{
    public Point Step(Direction dir) => dir switch
    {
        Direction.Up => new(X, Y - 1),
        Direction.Down => new(X, Y + 1),
        Direction.Left => new(X - 1, Y),
        Direction.Right => new(X + 1, Y),
        _ => throw new ArgumentOutOfRangeException(nameof(dir))
    };

    public override string ToString() => $"({X},{Y})";
}

/// <summary>
/// One cell of the abstract 32x32 world layout. Stores everything screen fitting will need:
/// area, axis, role, the required connector on each edge, and door colors where they matter.
/// </summary>
public class AbstractCell
{
    public required Point Position { get; init; }
    public required Area Area { get; init; }
    public required Scrolling Axis { get; init; }
    public CellRole Role { get; set; }

    public EdgeRequirement Up { get; set; } = EdgeRequirement.Wall;
    public EdgeRequirement Down { get; set; } = EdgeRequirement.Wall;
    public EdgeRequirement Left { get; set; } = EdgeRequirement.Wall;
    public EdgeRequirement Right { get; set; } = EdgeRequirement.Wall;

    /// <summary>Door color requirements; only meaningful when the edge requirement is Door.</summary>
    public DoorType? LeftDoorColor { get; set; }
    public DoorType? RightDoorColor { get; set; }

    /// <summary>Forces a specific screen for fixed landmarks (statues, elevators, bosses, Tourian template).</summary>
    public int? ForcedScreenId { get; set; }

    /// <summary>The concrete screen chosen by the fitter (phase 2); null until fitting runs.</summary>
    public ScreenProfile? AssignedScreen { get; set; }

    /// <summary>The scroll run (future room) this cell belongs to.</summary>
    public required Run Run { get; init; }

    public EdgeRequirement Edge(Direction dir) => dir switch
    {
        Direction.Up => Up,
        Direction.Down => Down,
        Direction.Left => Left,
        Direction.Right => Right,
        _ => throw new ArgumentOutOfRangeException(nameof(dir))
    };

    public void SetEdge(Direction dir, EdgeRequirement req, DoorType? color = null)
    {
        switch (dir)
        {
            case Direction.Up: Up = req; break;
            case Direction.Down: Down = req; break;
            case Direction.Left: Left = req; if (req == EdgeRequirement.Door) LeftDoorColor = color ?? DoorType.Blue; break;
            case Direction.Right: Right = req; if (req == EdgeRequirement.Door) RightDoorColor = color ?? DoorType.Blue; break;
        }
    }

    public override string ToString() =>
        $"{Position} {Area} {Axis} {Role} U:{Up} D:{Down} L:{Left} R:{Right}" +
        (ForcedScreenId.HasValue ? $" forced=0x{ForcedScreenId:X2}" : "");
}

/// <summary>
/// A maximal scroll-run of same-area cells along one axis; becomes one room at decomposition.
/// Cells are ordered left-to-right (horizontal) or top-to-bottom (vertical).
/// A run of length 1 is a single-screen room and is axis-neutral for traversal.
/// </summary>
public class Run
{
    // Never reset between attempts; per-attempt state (fullConnections) is cleared
    // on each retry, so stale IDs from a previous attempt cannot collide with new ones.
    private static int nextId;
    public int Id { get; } = nextId++;
    public required Area Area { get; init; }
    public required Scrolling Axis { get; init; }
    public List<AbstractCell> Cells { get; } = [];

    public bool IsSingleCell => Cells.Count == 1;

    public RunKind Kind => IsSingleCell ? RunKind.SingleCell
        : Axis == Scrolling.Horizontal ? RunKind.MultiHorizontal : RunKind.MultiVertical;
}

public enum LinkType
{
    Door,
    Elevator
}

/// <summary>A committed non-scroll connection between two cells (doors and elevator pairs).</summary>
public record Link(Point A, Point B, LinkType Type)
{
    public override string ToString() => $"{Type} {A}<->{B}";
}

/// <summary>
/// The abstract 32x32 world layout under construction: occupied cells, their runs,
/// and all door/elevator links. This single structure must later drive screen fitting,
/// room decomposition, the logic graph and ROM emission.
/// </summary>
public class WorldGrid
{
    public const int Size = 32;

    private readonly AbstractCell?[,] cells = new AbstractCell?[Size, Size];
    public List<Run> Runs { get; } = [];
    public List<Link> Links { get; } = [];

    /// <summary>
    /// Cells that must stay engine-empty (0xFF) because a committed screen has an opening
    /// facing them that relies on the engine's seal-against-empty behavior.
    /// </summary>
    public HashSet<Point> ReservedEmpty { get; } = [];

    public AbstractCell? Cell(Point p) => InBounds(p) ? cells[p.X, p.Y] : null;
    public AbstractCell? Cell(int x, int y) => Cell(new Point(x, y));
    public bool IsFree(Point p) => InBounds(p) && cells[p.X, p.Y] == null;

    /// <summary>True if generation may put a new cell here (in bounds, unoccupied, not reserved empty).</summary>
    public bool CanPlace(Point p) => IsFree(p) && !ReservedEmpty.Contains(p);

    public static bool InBounds(Point p) => p.X >= 0 && p.X < Size && p.Y >= 0 && p.Y < Size;

    public IEnumerable<AbstractCell> Cells
    {
        get
        {
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                    if (cells[x, y] != null)
                        yield return cells[x, y]!;
        }
    }

    public IEnumerable<AbstractCell> CellsOf(Area area) => Cells.Where(c => c.Area == area);

    public void Place(AbstractCell cell)
    {
        if (!InBounds(cell.Position))
            throw new InvalidOperationException($"Cell {cell} out of bounds");
        if (cells[cell.Position.X, cell.Position.Y] != null)
            throw new InvalidOperationException($"Cell {cell} overlaps {cells[cell.Position.X, cell.Position.Y]}");
        if (ReservedEmpty.Contains(cell.Position))
            throw new InvalidOperationException($"Cell {cell} placed on a reserved-empty coordinate");
        cells[cell.Position.X, cell.Position.Y] = cell;
    }

    /// <summary>
    /// Places a new scroll run of <paramref name="length"/> cells starting at
    /// <paramref name="start"/> extending right (horizontal) or down (vertical).
    /// Interior edges along the axis are set to Scroll; everything else stays Wall.
    /// With <paramref name="capStart"/>/<paramref name="capEnd"/> the first/last cell becomes
    /// a solid-wall cap terminator (vanilla style); caps get no scroll edges.
    /// </summary>
    public Run PlaceRun(Area area, Scrolling axis, Point start, int length, CellRole bodyRole,
        bool capStart = false, bool capEnd = false)
    {
        var run = new Run { Area = area, Axis = axis };
        var step = axis == Scrolling.Horizontal ? Direction.Right : Direction.Down;
        var p = start;

        for (int i = 0; i < length; i++)
        {
            bool isCap = (i == 0 && capStart) || (i == length - 1 && capEnd);
            var cell = new AbstractCell { Position = p, Area = area, Axis = axis, Run = run, Role = isCap ? CellRole.Cap : bodyRole };
            bool prevIsCap = i == 1 && capStart;
            bool nextIsCap = i == length - 2 && capEnd;
            if (i > 0 && !isCap && !prevIsCap)
                cell.SetEdge(axis == Scrolling.Horizontal ? Direction.Left : Direction.Up, EdgeRequirement.Scroll);
            if (i < length - 1 && !isCap && !nextIsCap)
                cell.SetEdge(step, EdgeRequirement.Scroll);
            Place(cell);
            run.Cells.Add(cell);
            p = p.Step(step);
        }

        Runs.Add(run);
        return run;
    }

    /// <summary>
    /// Extends a vertical run past its bottom cap: the old cap becomes a body cell and the
    /// run gains <paramref name="extra"/> cells below it, ending in a fresh cap. The caller
    /// must have verified the new coordinates with <see cref="CanPlace"/>. Follows the
    /// PlaceRun cap convention: no scroll edges on or toward the cap cell.
    /// </summary>
    public void ExtendRunDown(Run run, int extra, CellRole bodyRole)
    {
        if (run.Axis != Scrolling.Vertical || run.Cells.Count < 2 || run.Cells[^1].Role != CellRole.Cap)
            throw new InvalidOperationException($"run cannot extend down: {run.Cells[^1]}");

        var oldCap = run.Cells[^1];
        oldCap.Role = bodyRole;
        oldCap.SetEdge(Direction.Up, EdgeRequirement.Scroll);
        run.Cells[^2].SetEdge(Direction.Down, EdgeRequirement.Scroll);

        var prev = oldCap;
        var p = oldCap.Position.Step(Direction.Down);
        for (int i = 0; i < extra; i++)
        {
            bool isCap = i == extra - 1;
            var cell = new AbstractCell
            {
                Position = p, Area = run.Area, Axis = run.Axis, Run = run,
                Role = isCap ? CellRole.Cap : bodyRole
            };
            if (!isCap)
            {
                prev.SetEdge(Direction.Down, EdgeRequirement.Scroll);
                cell.SetEdge(Direction.Up, EdgeRequirement.Scroll);
            }
            Place(cell);
            run.Cells.Add(cell);
            prev = cell;
            p = p.Step(Direction.Down);
        }
    }

    /// <summary>
    /// Connects two horizontally adjacent cells with a door (sets both facing edges and records the link).
    /// Each cell's own door color gates traversal leaving through it.
    /// </summary>
    public void LinkDoor(AbstractCell left, AbstractCell right, DoorType leftCellColor = DoorType.Blue, DoorType rightCellColor = DoorType.Blue)
    {
        if (left.Position.Y != right.Position.Y || left.Position.X + 1 != right.Position.X)
            throw new InvalidOperationException($"Door link cells not horizontally adjacent: {left.Position} / {right.Position}");

        left.SetEdge(Direction.Right, EdgeRequirement.Door, leftCellColor);
        right.SetEdge(Direction.Left, EdgeRequirement.Door, rightCellColor);
        Links.Add(new Link(left.Position, right.Position, LinkType.Door));
    }

    /// <summary>Connects two vertically adjacent cells as an elevator pair (top above bottom).</summary>
    public void LinkElevator(AbstractCell top, AbstractCell bottom)
    {
        if (top.Position.X != bottom.Position.X || top.Position.Y + 1 != bottom.Position.Y)
            throw new InvalidOperationException($"Elevator link cells not vertically adjacent: {top.Position} / {bottom.Position}");

        top.SetEdge(Direction.Down, EdgeRequirement.Elevator);
        bottom.SetEdge(Direction.Up, EdgeRequirement.Elevator);
        Links.Add(new Link(top.Position, bottom.Position, LinkType.Elevator));
    }

    public bool HasLink(Point a, Point b) =>
        Links.Any(l => (l.A == a && l.B == b) || (l.A == b && l.B == a));

    /// <summary>
    /// Validates that every cell can be filled by at least one real screen, using the final
    /// occupancy: edges that face an occupied cell must be genuinely matched (a Wall edge
    /// facing an occupied cell requires a screen with no opening there, because the engine
    /// only seals openings that face empty cells).
    /// </summary>
    public List<string> ValidateFittability(ScreenCatalog catalog)
    {
        var errors = new List<string>();

        foreach (var cell in Cells)
        {
            var candidates = catalog.ForArea(cell.Area).Where(p => FitsStrict(catalog, p, cell)).ToList();
            if (candidates.Count == 0)
                errors.Add($"No screen fits cell {cell}");
        }

        return errors;
    }

    /// <summary>
    /// True if <paramref name="profile"/> can fill <paramref name="cell"/> given the final grid:
    /// required connectors must match exactly; door edges must accept the neighbor's run kind
    /// (the $A0/$A1 door tile behavior learned from vanilla); Wall edges facing occupied cells
    /// must be truly closed (tunnels excepted: ability-gated screen internals, never engine
    /// transitions); Wall edges facing empty cells accept any opening (the engine seals them).
    /// </summary>
    public bool FitsStrict(ScreenCatalog catalog, ScreenProfile profile, AbstractCell cell)
    {
        if (cell.Role == CellRole.Cap)
            return profile.IsCap;
        if (profile.Axis != cell.Axis)
            return false;
        if (profile.IsOneWay)
            return false;
        if (cell.ForcedScreenId.HasValue && profile.ScreenId != cell.ForcedScreenId.Value)
            return false;

        foreach (var dir in new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right })
        {
            var required = cell.Edge(dir);
            var actual = profile.Connector(dir);

            switch (required)
            {
                case EdgeRequirement.Scroll when actual.Type != ConnectorType.Scroll:
                case EdgeRequirement.Door when actual.Type != ConnectorType.Door:
                case EdgeRequirement.Elevator when actual.Type != ConnectorType.Elevator:
                    return false;

                case EdgeRequirement.Door:
                    var requiredColor = dir == Direction.Left ? cell.LeftDoorColor : cell.RightDoorColor;
                    if (requiredColor.HasValue && actual.Color != requiredColor.Value)
                        return false;
                    // Forced screens skip the vanilla door-context check, like the
                    // pair-connectivity check below: templates replicate arrangements
                    // validated by vanilla (or vanilla combo, e.g. the portal room 0x1F,
                    // which never appears in a vanilla room and so has no context).
                    var doorNeighbor = Cell(cell.Position.Step(dir));
                    if (doorNeighbor != null && !cell.ForcedScreenId.HasValue
                        && !catalog.DoorAccepts(profile, dir, doorNeighbor.Run.Kind))
                        return false;
                    break;

                case EdgeRequirement.Wall:
                    // An elevator opening is never acceptable on a Wall edge: the engine
                    // seals scroll and door openings against empty cells, but an elevator
                    // shaft hole is a real gap in the screen with no platform spawned.
                    // Forced template cells are exempt (the escape shaft top 0x0E rides
                    // into a reserved-empty cell, exactly like vanilla).
                    if (actual.Type == ConnectorType.Elevator && !cell.ForcedScreenId.HasValue)
                        return false;
                    // A scroll/door opening on a Wall edge is fine when it faces an empty
                    // cell (the engine seals it) or a cap cell (solid wall, cannot create a
                    // transition). Facing any other occupied cell it would create an
                    // unintended transition.
                    var facing = Cell(cell.Position.Step(dir));
                    if (facing != null && facing.Role != CellRole.Cap
                        && actual.Type is ConnectorType.Scroll or ConnectorType.Door)
                        return false;
                    break;
            }
        }

        // Every pair of committed edges must be internally connected both ways inside the
        // screen, or it would sever the chain it is placed into (e.g. Ridley 0x0A's right
        // door cannot reach its bottom opening). Forced screens are exempt: templates copy
        // hand-validated vanilla arrangements with intentional one-ways (Mother Brain's
        // escape door, Kraid's 0x1B fall hallway).
        if (!cell.ForcedScreenId.HasValue)
        {
            var committed = new List<Direction>(4);
            foreach (var dir in new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right })
                if (cell.Edge(dir) != EdgeRequirement.Wall)
                    committed.Add(dir);

            for (int i = 0; i < committed.Count; i++)
                for (int j = i + 1; j < committed.Count; j++)
                    if (!profile.EdgesConnected(committed[i], committed[j]))
                        return false;
        }

        // Screens that draw the item orb into their structure (e.g. Brinstar 0x2A) only belong
        // on an Item cell; on any other cell the orb renders with no item behind it. Forced
        // cells pick their own screen, so they are exempt.
        if (profile.HasStructuralItem && cell.Role != CellRole.Item && !cell.ForcedScreenId.HasValue)
            return false;

        return cell.Role switch
        {
            CellRole.Item => profile.HasItemLocation,
            CellRole.Boss => profile.HasBossLocation,
            CellRole.Start => profile.HasStartLocation,
            CellRole.ElevatorTop or CellRole.ElevatorBottom => profile.HasElevatorPlatform,
            _ => true
        };
    }

    /// <summary>Per-area cell counts plus link summary, for diagnostics.</summary>
    public string Summary()
    {
        var lines = new List<string>();
        foreach (var area in ScreenCatalog.PlayableAreas)
            lines.Add($"{area}: {CellsOf(area).Count()} cells, {Runs.Count(r => r.Area == area)} runs");
        lines.Add($"Links: {Links.Count(l => l.Type == LinkType.Door)} doors, {Links.Count(l => l.Type == LinkType.Elevator)} elevators");
        return string.Join("\n", lines);
    }
}
