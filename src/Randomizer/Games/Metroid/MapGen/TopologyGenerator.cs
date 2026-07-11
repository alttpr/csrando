namespace Randomizer.Games.Metroid.MapGen;

using System;
using System.Collections.Generic;
using System.Linq;
using static Randomizer.Games.Metroid.YamlReader;

/// <summary>Thrown by one generation attempt; the retry loop catches it and tries a new layout.</summary>
public class GenerationException(string message) : Exception(message);

/// <summary>The output of topology generation: one consistent layout plus diagnostics.</summary>
public class GeneratedWorld
{
    public required WorldGrid Grid { get; init; }
    public required Point Start { get; init; }
    public required int Seed { get; init; }
    public required int Attempts { get; init; }
    public Dictionary<string, Point> Landmarks { get; init; } = [];
    public List<string> Diagnostics { get; init; } = [];

    /// <summary>
    /// Grid cell to the PC address of its power-up table entry; set by ROM emission so the
    /// item filler's choices can be written into the generated special-items tables.
    /// </summary>
    public Dictionary<Point, int>? ItemAddresses { get; set; }
}

/// <summary>
/// Generates the abstract 32x32 world topology.
///
/// Order: Brinstar spine first (it owns the outbound elevators), then the Tourian complex
/// (statues gate, gated elevator and the templated Tourian below it), then the
/// Brinstar-to-Kraid/Norfair elevators with their entrance shafts, then Norfair-to-Ridley,
/// then boss complexes, then branch/item filler.
///
/// Every feature is placed all-or-nothing; whole-world retries handle dead ends. Every door
/// pairing replicates a vanilla door context (see <see cref="ScreenCatalog.DoorAccepts"/>),
/// which is what makes the layout traversable under the engine's scroll axis rules.
/// Validation = per-cell screen fittability plus physical reachability from the start.
/// </summary>
public class TopologyGenerator(ScreenCatalog catalog)
{
    public int MaxAttempts { get; init; } = 300;

    /// <summary>
    /// Scales the per-area cell-count goals. 1.0 targets the vanilla area sizes
    /// (Brinstar 130, Norfair 160, Kraid 100, Ridley 87). Targets are aspirational
    /// (growth stops when space runs out — Ridley in particular rarely reaches vanilla
    /// size in the band it gets); the scaled minimums are hard and trigger a retry.
    /// </summary>
    public double SizeScale { get; init; } = 1.0;

    /// <summary>
    /// Number of cross-game portal anchors to place (combo mode transitions). Standalone
    /// seeds keep the rooms as inert dead ends, so anchors are always generated.
    /// </summary>
    public int PortalCount { get; init; } = 1;

    /// <summary>
    /// Nightmare mode: ignore the size targets and keep growing every area until nothing
    /// fits anymore, saturating the grid. Minimums stay at the SizeScale level (a hard
    /// minimum near saturation would make generation impossible).
    /// </summary>
    public bool Saturate { get; init; }

    private Random rng = new(0);
    private WorldGrid grid = new();
    private readonly Dictionary<Area, List<Run>> shafts = [];
    private readonly Dictionary<Area, (int MinX, int MaxX, int MinY, int MaxY)> bounds = [];
    private Point start;
    private readonly Dictionary<string, Point> landmarks = [];
    /// <summary>Full two-door connections placed per shaft pair, keyed by run ids.</summary>
    private readonly Dictionary<(int, int), int> fullConnections = [];
    /// <summary>Cycle-making connector corridors placed per area during growth.</summary>
    private readonly Dictionary<Area, int> areaCycles = [];

    /// <summary>Chance a shaft cell that already has a door takes one on the other side too.</summary>
    private const int DoubleDoorPercent = 15;

    /// <summary>
    /// True while filler growth runs. The door-density rules only apply then: growth is
    /// what turns shafts into door ladders, while the fixed setpieces (Construction Zone,
    /// elevators, lairs) have so few candidate rows that rationing them just fails attempts.
    /// </summary>
    private bool densityRulesActive;

    // Base size goals at SizeScale 1.0 = the vanilla cell counts (B130/N160/K100/R87).
    // Minimums sit well below target: the grid bands cannot always fit vanilla bulk
    // (Ridley especially), and a hard minimum near target would explode retry counts.
    private static readonly Dictionary<Area, (int Min, int Target)> BaseSizeGoals = new()
    {
        [Area.Brinstar] = (55, 130),
        [Area.Norfair] = (65, 160),
        [Area.Kraid] = (40, 100),
        [Area.Ridley] = (32, 87),
    };

    private (int Min, int Target) GoalFor(Area area)
    {
        // Minimums never scale above 1.0: targets past vanilla size are best-effort, and
        // raising the hard floor with them converts every growth stall into a whole-world
        // retry (at 1.2 that used to fail half of all seeds outright).
        var (min, target) = BaseSizeGoals[area];
        return (Math.Max(10, (int)(min * Math.Min(1.0, SizeScale))), Math.Max(12, (int)(target * SizeScale)));
    }

    /// <summary>Column half-width of a lower area's band, widened for above-vanilla scales.</summary>
    private int AreaHalfWidth => (int)Math.Round(16 * Math.Max(1.0, SizeScale));

    public GeneratedWorld Generate(int seed)
    {
        var failures = new List<string>();

        for (int attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            rng = new Random(unchecked(seed * 1000003 + attempt));
            grid = new WorldGrid();
            shafts.Clear();
            bounds.Clear();
            landmarks.Clear();
            fullConnections.Clear();
            areaCycles.Clear();
            densityRulesActive = false;

            try
            {
                BuildBrinstarSpine();
                BuildTourianComplex();
                PlaceConstructionZone();
                BuildLowerArea(Area.Kraid, elevatorScreen: 0x1C, preferRight: false);
                PlaceKraidLair();
                BuildLowerArea(Area.Norfair, elevatorScreen: 0x0B, preferRight: true);

                // Let Norfair claim some space before Ridley is hung off one of its shafts;
                // otherwise Ridley always sits next to the entrance and boxes Norfair in.
                // The budget is fixed so high SizeScale values don't starve Ridley. No shaft
                // extensions here: Ridley hangs below Norfair's deepest shaft, and pre-grow
                // extensions dig Norfair to the grid bottom, leaving Ridley no vertical band.
                GrowArea(Area.Norfair, grid.CellsOf(Area.Norfair).Count() + 30, allowExtend: false);

                BuildRidley();
                PlaceRidleyLair();
                PlaceChozoItemRooms();
                PlacePortalAnchors();
                GrowAreas();
                EnsureItemCells();
                PlaceHiddenBombWalls();
                ValidateWorld();

                return new GeneratedWorld
                {
                    Grid = grid,
                    Start = start,
                    Seed = seed,
                    Attempts = attempt,
                    Landmarks = new(landmarks),
                    Diagnostics = BuildDiagnostics(attempt)
                };
            }
            catch (GenerationException e)
            {
                failures.Add($"attempt {attempt}: {e.Message}");
            }
        }

        throw new InvalidOperationException(
            $"M1 topology generation failed after {MaxAttempts} attempts for seed {seed}. Last failures:\n" +
            string.Join("\n", failures.TakeLast(10)));
    }

    private int Rand(int minInclusive, int maxInclusive) => rng.Next(minInclusive, maxInclusive + 1);
    private T Pick<T>(IReadOnlyList<T> list) => list.Count > 0 ? list[rng.Next(list.Count)]
        : throw new GenerationException("empty pick list");
    private List<T> Shuffled<T>(IEnumerable<T> source) => source.OrderBy(_ => rng.Next()).ToList();

    private List<Run> ShaftsOf(Area area) => shafts.TryGetValue(area, out var list) ? list : shafts[area] = [];

    /// <summary>
    /// Shaft cells that can take side doors: interiors (scroll both ways) and run ends
    /// (one scroll edge) — vanilla constantly puts doors on bottom/top pieces like Ridley
    /// 0x0C, and end pieces are the only right-door option in areas whose interior
    /// right-door bodies are internally one-way. Excludes caps and forced cells.
    /// </summary>
    private static List<AbstractCell> InteriorCells(Run shaft) =>
        shaft.Cells.Where(c => (c.Up == EdgeRequirement.Scroll || c.Down == EdgeRequirement.Scroll)
            && c.Role != CellRole.Cap && !c.ForcedScreenId.HasValue).ToList();

    /// <summary>
    /// True if adding a door of <paramref name="newColor"/> on <paramref name="side"/>
    /// facing a run of <paramref name="neighborKind"/> still leaves the cell fillable by a
    /// real screen (signature, door context, door colors, and any door already on the
    /// other side included). E.g. Kraid's only double-door shaft bodies have a red right
    /// door, so a second blue door must be rejected here, not at final validation.
    /// </summary>
    private bool CanAddDoor(AbstractCell cell, Direction side, RunKind neighborKind, DoorType newColor = DoorType.Blue)
    {
        if (cell.Edge(side) != EdgeRequirement.Wall || cell.ForcedScreenId.HasValue)
            return false;

        // Door-density rules, applied to vertical runs (every feature anchors to a shaft, so
        // without them shafts turn into solid door ladders). Vanilla spreads side rooms out:
        // no door directly above or below an existing same-side door of the same shaft, and a
        // cell that already has a door on the other side only occasionally takes a second one.
        if (densityRulesActive && cell.Run.Axis == Scrolling.Vertical)
        {
            bool SameSideDoor(Point p) =>
                grid.Cell(p) is { } n && n.Run == cell.Run && n.Edge(side) == EdgeRequirement.Door;
            if (SameSideDoor(cell.Position.Step(Direction.Up)) || SameSideDoor(cell.Position.Step(Direction.Down)))
                return false;

            var opposite = side == Direction.Left ? Direction.Right : Direction.Left;
            if (cell.Edge(opposite) == EdgeRequirement.Door && rng.Next(100) >= DoubleDoorPercent)
                return false;
        }

        EdgeRequirement Probe(Direction dir) => dir == side ? EdgeRequirement.Door : cell.Edge(dir);

        RunKind? KindBeside(Direction dir)
        {
            if (dir == side)
                return neighborKind;
            if (cell.Edge(dir) != EdgeRequirement.Door)
                return null;
            return grid.Cell(cell.Position.Step(dir))?.Run.Kind;
        }

        DoorType? RequiredColor(Direction dir) => dir == side ? newColor
            : dir == Direction.Left && cell.Left == EdgeRequirement.Door ? cell.LeftDoorColor
            : dir == Direction.Right && cell.Right == EdgeRequirement.Door ? cell.RightDoorColor
            : null;

        return catalog.Query(cell.Area, cell.Axis,
                Probe(Direction.Up), Probe(Direction.Down), Probe(Direction.Left), Probe(Direction.Right),
                leftNeighbor: KindBeside(Direction.Left), rightNeighbor: KindBeside(Direction.Right))
            .Any(p => (RequiredColor(Direction.Left) is not { } lc || p.Left.Color == lc)
                   && (RequiredColor(Direction.Right) is not { } rc || p.Right.Color == rc));
    }

    /// <summary>
    /// True if a corridor end cell with a door of <paramref name="color"/> on
    /// <paramref name="doorSide"/> facing a run of <paramref name="neighborKind"/> can be
    /// filled by a real screen of the area.
    /// </summary>
    private bool CorridorEndExists(Area area, Direction doorSide, RunKind neighborKind, DoorType color = DoorType.Blue)
    {
        var candidates = doorSide == Direction.Left
            ? catalog.Query(area, Scrolling.Horizontal,
                EdgeRequirement.Wall, EdgeRequirement.Wall, EdgeRequirement.Door, EdgeRequirement.Scroll,
                leftNeighbor: neighborKind)
            : catalog.Query(area, Scrolling.Horizontal,
                EdgeRequirement.Wall, EdgeRequirement.Wall, EdgeRequirement.Scroll, EdgeRequirement.Door,
                rightNeighbor: neighborKind);

        return candidates.Any(p => p.Connector(doorSide).Color == color);
    }

    // ---------------------------------------------------------------- Brinstar spine

    private void BuildBrinstarSpine()
    {
        bounds[Area.Brinstar] = (2, 29, 1, 14);

        // Shaft columns: leftmost far enough right that the Tourian complex fits to its west.
        int shaftCount = Rand(3, 4);
        var columns = new List<int> { Rand(10, 12) };
        for (int i = 1; i < shaftCount; i++)
        {
            int next = columns[^1] + Rand(4, 7);
            if (next > 29)
                break;
            columns.Add(next);
        }
        if (columns.Count < 3)
            throw new GenerationException("Brinstar spine too narrow");

        foreach (int x in columns)
        {
            int yTop = Rand(2, 5);
            int total = Rand(6, 10) + 2; // body + caps at both ends
            if (yTop + total - 1 > 14)
                total = 14 - yTop + 1;
            if (total < 6)
                throw new GenerationException("Brinstar shaft too short");

            var cells = Enumerable.Range(0, total).Select(i => new Point(x, yTop + i));
            if (!cells.All(grid.CanPlace))
                throw new GenerationException($"Brinstar shaft at x={x} blocked");

            ShaftsOf(Area.Brinstar).Add(
                grid.PlaceRun(Area.Brinstar, Scrolling.Vertical, new Point(x, yTop), total, CellRole.Shaft, capStart: true, capEnd: true));
        }

        // Connect each adjacent shaft pair with a doored corridor; one corridor hosts the start.
        var corridors = new List<Run>();
        for (int i = 0; i + 1 < columns.Count; i++)
            corridors.Add(ConnectShaftsWithCorridor(ShaftsOf(Area.Brinstar)[i], ShaftsOf(Area.Brinstar)[i + 1]));

        // The start needs two adjacent free-scrolling interior cells: the spawn platform
        // (0x09) and, like vanilla's Morph Room, an item pedestal (0x17) right next to it.
        // The pedestal guarantees at least one item location reachable with no equipment,
        // which the item filler's Morph front-fill depends on.
        var startPairs = new List<(AbstractCell Spawn, AbstractCell Pedestal)>();
        foreach (var corridor in corridors)
        {
            for (int i = 0; i + 1 < corridor.Cells.Count; i++)
            {
                var a = corridor.Cells[i];
                var b = corridor.Cells[i + 1];
                if (a.Role == CellRole.Corridor && b.Role == CellRole.Corridor
                    && a.Left == EdgeRequirement.Scroll && a.Right == EdgeRequirement.Scroll
                    && b.Left == EdgeRequirement.Scroll && b.Right == EdgeRequirement.Scroll)
                {
                    startPairs.Add((a, b));
                    startPairs.Add((b, a));
                }
            }
        }
        if (startPairs.Count == 0)
            throw new GenerationException("no corridor with adjacent interior cells for start + pedestal");

        var (spawn, pedestal) = Pick(startPairs);
        spawn.Role = CellRole.Start;
        pedestal.Role = CellRole.Item;
        pedestal.ForcedScreenId = 0x17; // Morph Pedestal: item reachable from both sides with nothing
        start = spawn.Position;
        landmarks["Start"] = start;
        landmarks["StartPedestal"] = pedestal.Position;
    }

    /// <summary>Places a horizontal corridor between two vertical shafts, doors at both ends.</summary>
    private Run ConnectShaftsWithCorridor(Run leftShaft, Run rightShaft)
    {
        int lx = leftShaft.Cells[0].Position.X;
        int rx = rightShaft.Cells[0].Position.X;

        var rows = Shuffled(
            InteriorCells(leftShaft).Select(c => c.Position.Y)
                .Intersect(InteriorCells(rightShaft).Select(c => c.Position.Y)));

        foreach (int y in rows)
        {
            var leftCell = grid.Cell(lx, y)!;
            var rightCell = grid.Cell(rx, y)!;
            if (!CanAddDoor(leftCell, Direction.Right, RunKind.MultiHorizontal)
                || !CanAddDoor(rightCell, Direction.Left, RunKind.MultiHorizontal))
                continue;

            int len = rx - lx - 1;
            var cells = Enumerable.Range(lx + 1, len).Select(x => new Point(x, y));
            if (!cells.All(grid.CanPlace))
                continue;

            var corridor = grid.PlaceRun(leftShaft.Area, Scrolling.Horizontal, new Point(lx + 1, y), len, CellRole.Corridor);
            grid.LinkDoor(leftCell, corridor.Cells[0]);
            grid.LinkDoor(corridor.Cells[^1], rightCell);
            MaybePlaceItem(corridor);
            return corridor;
        }

        throw new GenerationException($"no corridor row between shafts x={lx} and x={rx}");
    }

    // ---------------------------------------------------------------- Tourian

    /// <summary>
    /// Tourian's vocabulary is thin and every door piece faces a fixed side, so the area is
    /// a template copied from the vanilla layout, anchored west of the leftmost Brinstar shaft:
    ///
    ///   shaft - blue door - corridor (0x27..) - red door - statues gate 0x2B - elevator 0x2C
    ///   below the elevator: entrance shaft [0x01 spacer, 0x0E platform, .., 0x0F]
    ///   - orange door east - metroid corridor A [0x11 .. 0x12] - red door - shaft B down
    ///   [0x0D .. 0x0A] - red door west - metroid corridor B [0x05 .. 0x09]
    ///   - blue door - Mother Brain corridor [0x04, 0x03, 0x02].
    ///
    /// Lengths vary per seed; door colors and orientations are fixed by the screens.
    /// </summary>
    private void BuildTourianComplex()
    {
        var gateShaft = ShaftsOf(Area.Brinstar)[0];
        int sx = gateShaft.Cells[0].Position.X;

        // Anchor as high as possible: the complex extends downward from the gate row, so a
        // top anchor packs Tourian into the top-left corner and leaves the west band below
        // it free for Kraid to sprawl into.
        foreach (var shaftCell in InteriorCells(gateShaft).OrderBy(c => c.Position.Y))
        {
            if (!CanAddDoor(shaftCell, Direction.Left, RunKind.MultiHorizontal))
                continue;

            int gy = shaftCell.Position.Y;
            int gLen = Rand(2, 4);            // gate corridor between gate and shaft
            int gateX = sx - gLen - 1;
            int ex = gateX - 1;               // elevator and Tourian entrance column

            int eLen = Rand(3, 6);            // entrance: 0x01 spacer, 0x0E platform, bodies, 0x0F
            int aLen = Rand(2, 5);            // metroid corridor A (east of the entrance)
            int bLen = Rand(3, 5);            // shaft B (0x0D top .. 0x0A bottom)
            int cLen = Rand(2, 5);            // metroid corridor B (west of shaft B's bottom)
            int escLen = Rand(3, 6);          // escape shaft (0x0E top, 0x10 climb, 0x0F bottom)

            int ybA = gy + eLen;              // entrance bottom row = corridor A row
            int bx = ex + aLen + 1;           // shaft B column
            int ybB = ybA + bLen - 1;         // shaft B bottom row = corridor B and MB row
            int mbx = bx - cLen - 3;          // Mother Brain corridor left column
            int escX = mbx - 1;               // escape shaft column, directly west of Mother Brain
            int escTop = ybB - escLen + 1;    // escape shaft top row

            if (ex < 1 || escX < 0 || escTop < 2 || ybB > 30)
                continue;

            var needed = new List<Point>();
            needed.AddRange(Enumerable.Range(gateX + 1, gLen).Select(x => new Point(x, gy)));
            needed.Add(new Point(gateX, gy));
            needed.Add(new Point(ex, gy));
            needed.AddRange(Enumerable.Range(gy + 1, eLen).Select(y => new Point(ex, y)));
            needed.AddRange(Enumerable.Range(ex + 1, aLen).Select(x => new Point(x, ybA)));
            needed.AddRange(Enumerable.Range(ybA, bLen).Select(y => new Point(bx, y)));
            needed.AddRange(Enumerable.Range(bx - cLen, cLen).Select(x => new Point(x, ybB)));
            needed.AddRange(Enumerable.Range(mbx, 3).Select(x => new Point(x, ybB)));
            needed.AddRange(Enumerable.Range(escTop, escLen).Select(y => new Point(escX, y)));

            if (needed.Distinct().Count() != needed.Count
                || !needed.All(grid.CanPlace) || !grid.CanPlace(new Point(escX, escTop - 1)))
                continue;

            // Gate corridor: 0x27 (left red door) west end, a right-door piece east end.
            var corrG = grid.PlaceRun(Area.Brinstar, Scrolling.Horizontal, new Point(gateX + 1, gy), gLen, CellRole.Corridor);
            corrG.Cells[0].ForcedScreenId = 0x27;
            grid.LinkDoor(corrG.Cells[^1], shaftCell);

            // Statues gate and the Tourian elevator behind its red door.
            var gateRun = grid.PlaceRun(Area.Brinstar, Scrolling.Horizontal, new Point(gateX, gy), 1, CellRole.Gate);
            gateRun.Cells[0].ForcedScreenId = 0x2B;
            var elevRun = grid.PlaceRun(Area.Brinstar, Scrolling.Horizontal, new Point(ex, gy), 1, CellRole.ElevatorTop);
            elevRun.Cells[0].ForcedScreenId = 0x2C;
            grid.LinkDoor(gateRun.Cells[0], corrG.Cells[0], DoorType.Red, DoorType.Red);
            grid.LinkDoor(elevRun.Cells[0], gateRun.Cells[0], DoorType.Blue, DoorType.Blue);

            // Entrance shaft: elevator spacer, platform, bodies, orange-door bottom.
            var entrance = grid.PlaceRun(Area.Tourian, Scrolling.Vertical, new Point(ex, gy + 1), eLen, CellRole.Shaft);
            entrance.Cells[0].ForcedScreenId = 0x01;
            entrance.Cells[1].ForcedScreenId = 0x0E;
            entrance.Cells[1].Role = CellRole.ElevatorBottom;
            entrance.Cells[^1].ForcedScreenId = 0x0F;
            entrance.Cells[0].SetEdge(Direction.Down, EdgeRequirement.Elevator);
            entrance.Cells[1].SetEdge(Direction.Up, EdgeRequirement.Elevator);
            grid.LinkElevator(elevRun.Cells[0], entrance.Cells[0]);

            // Metroid corridor A east of the entrance bottom.
            var corrA = grid.PlaceRun(Area.Tourian, Scrolling.Horizontal, new Point(ex + 1, ybA), aLen, CellRole.Corridor);
            corrA.Cells[0].ForcedScreenId = 0x11;
            corrA.Cells[^1].ForcedScreenId = 0x12;
            grid.LinkDoor(entrance.Cells[^1], corrA.Cells[0], DoorType.Orange, DoorType.Orange);

            // Shaft B descending from corridor A's east end.
            var shaftB = grid.PlaceRun(Area.Tourian, Scrolling.Vertical, new Point(bx, ybA), bLen, CellRole.Shaft);
            shaftB.Cells[0].ForcedScreenId = 0x0D;
            shaftB.Cells[^1].ForcedScreenId = 0x0A;
            grid.LinkDoor(corrA.Cells[^1], shaftB.Cells[0], DoorType.Red, DoorType.Red);

            // Metroid corridor B west of shaft B's bottom.
            var corrB = grid.PlaceRun(Area.Tourian, Scrolling.Horizontal, new Point(bx - cLen, ybB), cLen, CellRole.Corridor);
            corrB.Cells[0].ForcedScreenId = 0x05;
            corrB.Cells[^1].ForcedScreenId = 0x09;
            grid.LinkDoor(corrB.Cells[^1], shaftB.Cells[^1], DoorType.Red, DoorType.Red);

            // Mother Brain corridor.
            var mb = grid.PlaceRun(Area.Tourian, Scrolling.Horizontal, new Point(mbx, ybB), 3, CellRole.Corridor);
            mb.Cells[0].ForcedScreenId = 0x04;
            mb.Cells[0].Role = CellRole.Boss;
            mb.Cells[1].ForcedScreenId = 0x03;
            mb.Cells[2].ForcedScreenId = 0x02;
            grid.LinkDoor(mb.Cells[2], corrB.Cells[0], DoorType.Blue, DoorType.Blue);

            // Escape shaft west of Mother Brain (win-critical: the door that spawns on MB's
            // west wall after the fight is the only way to finish when M1 ends the run).
            // Vanilla shape: 0x0E elevator entrance on top, 0x10 climb bodies, 0x0F bottom
            // whose orange door faces Mother Brain. The escape elevator exit faces a cell
            // that must stay empty, exactly like vanilla.
            var escape = grid.PlaceRun(Area.Tourian, Scrolling.Vertical, new Point(escX, escTop), escLen, CellRole.Escape);
            foreach (var c in escape.Cells)
                c.Role = CellRole.Escape;
            escape.Cells[0].ForcedScreenId = 0x0E;
            foreach (var c in escape.Cells.Skip(1).Take(escLen - 2))
                c.ForcedScreenId = 0x10;
            escape.Cells[^1].ForcedScreenId = 0x0F;
            grid.LinkDoor(escape.Cells[^1], mb.Cells[0], DoorType.Orange, DoorType.Blue);
            grid.ReservedEmpty.Add(new Point(escX, escTop - 1));

            landmarks["StatuesGate"] = gateRun.Cells[0].Position;
            landmarks["TourianElevator"] = elevRun.Cells[0].Position;
            landmarks["MotherBrain"] = mb.Cells[0].Position;
            landmarks["EscapeShaft"] = escape.Cells[0].Position;
            return;
        }

        throw new GenerationException("could not anchor Tourian complex");
    }

    // ---------------------------------------------------------------- vanilla setpieces

    /// <summary>
    /// The vanilla Construction Zone arrangement: a Brinstar shaft whose top piece (0x18)
    /// has doors on both sides and a bombable floor — in vanilla it sits right of the start
    /// room and bombing through its floor is the route down to the Kraid elevator. Placed
    /// between two spine shafts at a shared row, with a short corridor on each side:
    ///
    ///   shaft - door - corridor A - door - [0x18, bodies.., cap] - door - corridor B - door - shaft
    ///
    /// The new shaft joins the shaft pool, so elevators, item complexes and growth can hang
    /// off the cells below the bomb floor, recreating the vanilla gating organically.
    /// </summary>
    private void PlaceConstructionZone()
    {
        var spine = ShaftsOf(Area.Brinstar);
        var (_, _, _, maxY) = bounds[Area.Brinstar];

        foreach (int i in Shuffled(Enumerable.Range(0, spine.Count - 1)))
        {
            var (left, right) = (spine[i], spine[i + 1]);
            int lx = left.Cells[0].Position.X;
            int rx = right.Cells[0].Position.X;
            if (rx - lx - 1 < 3)
                continue;

            var rows = Shuffled(
                InteriorCells(left).Select(c => c.Position.Y)
                    .Intersect(InteriorCells(right).Select(c => c.Position.Y)));

            foreach (int y in rows)
            {
                int mx = Rand(lx + 2, rx - 2);
                int lenA = mx - lx - 1;
                int lenB = rx - mx - 1;

                // Side corridor pieces must really exist: a length-1 side is a single-cell
                // room with doors both ways (0x00), longer sides are normal corridors whose
                // end pieces face vertical runs.
                bool SideExists(int len) => len == 1
                    ? catalog.HasPiece(Area.Brinstar, Scrolling.Horizontal,
                        EdgeRequirement.Wall, EdgeRequirement.Wall, EdgeRequirement.Door, EdgeRequirement.Door,
                        leftNeighbor: RunKind.MultiVertical, rightNeighbor: RunKind.MultiVertical)
                    : CorridorEndExists(Area.Brinstar, Direction.Left, RunKind.MultiVertical)
                        && CorridorEndExists(Area.Brinstar, Direction.Right, RunKind.MultiVertical);

                var leftCell = grid.Cell(lx, y)!;
                var rightCell = grid.Cell(rx, y)!;
                if (!CanAddDoor(leftCell, Direction.Right, lenA == 1 ? RunKind.SingleCell : RunKind.MultiHorizontal)
                    || !CanAddDoor(rightCell, Direction.Left, lenB == 1 ? RunKind.SingleCell : RunKind.MultiHorizontal)
                    || !SideExists(lenA) || !SideExists(lenB))
                    continue;

                int depth = Math.Min(Rand(4, 8), maxY - y + 1); // 0x18 + bodies + bottom cap
                if (depth < 4)
                    continue;

                var needed = new List<Point>();
                needed.AddRange(Enumerable.Range(lx + 1, lenA).Select(x => new Point(x, y)));
                needed.AddRange(Enumerable.Range(mx + 1, lenB).Select(x => new Point(x, y)));
                needed.AddRange(Enumerable.Range(y, depth).Select(yy => new Point(mx, yy)));
                if (!needed.All(grid.CanPlace))
                    continue;

                var corrA = grid.PlaceRun(Area.Brinstar, Scrolling.Horizontal, new Point(lx + 1, y), lenA, CellRole.Corridor);
                var shaft = grid.PlaceRun(Area.Brinstar, Scrolling.Vertical, new Point(mx, y), depth, CellRole.Shaft, capEnd: true);
                var corrB = grid.PlaceRun(Area.Brinstar, Scrolling.Horizontal, new Point(mx + 1, y), lenB, CellRole.Corridor);

                shaft.Cells[0].ForcedScreenId = 0x18;
                grid.LinkDoor(leftCell, corrA.Cells[0]);
                grid.LinkDoor(corrA.Cells[^1], shaft.Cells[0]);
                grid.LinkDoor(shaft.Cells[0], corrB.Cells[0]);
                grid.LinkDoor(corrB.Cells[^1], rightCell);

                MaybePlaceItem(corrA);
                MaybePlaceItem(corrB);
                ShaftsOf(Area.Brinstar).Add(shaft);
                landmarks["ConstructionZone"] = shaft.Cells[0].Position;
                return;
            }
        }

        throw new GenerationException("could not place the Construction Zone shaft");
    }

    // ---------------------------------------------------------------- elevators + lower areas

    /// <summary>
    /// Hangs an elevator room off a Brinstar shaft and drops the area's entrance shaft
    /// below it. The elevator screens have a left-facing door, so the room sits on the east
    /// side; whether it touches the shaft directly or via a corridor is dictated by the
    /// screen's vanilla door context (Kraid 0x1C faced a shaft, Norfair 0x0B a corridor).
    /// </summary>
    private void BuildLowerArea(Area area, int elevatorScreen, bool preferRight)
    {
        bool needsCorridor = !catalog.GetDoorContexts(Area.Brinstar, elevatorScreen, Direction.Left)
            .Contains(RunKind.MultiVertical);

        var ordered = preferRight
            ? ShaftsOf(Area.Brinstar).OrderByDescending(s => s.Cells[0].Position.X).ToList()
            : ShaftsOf(Area.Brinstar).OrderBy(s => s.Cells[0].Position.X).ToList();

        foreach (var shaft in ordered)
        {
            foreach (var shaftCell in InteriorCells(shaft).OrderByDescending(c => c.Position.Y))
            {
                int corrLen = needsCorridor ? Rand(2, 4) : 0;
                var neighborOfShaft = needsCorridor ? RunKind.MultiHorizontal : RunKind.SingleCell;
                if (!CanAddDoor(shaftCell, Direction.Right, neighborOfShaft))
                    continue;

                var ePos = new Point(shaftCell.Position.X + 1 + corrLen, shaftCell.Position.Y);
                var corridorCells = Enumerable.Range(shaftCell.Position.X + 1, corrLen)
                    .Select(x => new Point(x, shaftCell.Position.Y)).ToList();

                int entranceLen = Rand(5, 10) + 3; // 0x01 spacer + platform + bodies + bottom cap
                var entranceCells = Enumerable.Range(ePos.Y + 1, entranceLen).Select(y => new Point(ePos.X, y)).ToList();

                if (entranceCells[^1].Y > 29 || !corridorCells.All(grid.CanPlace)
                    || !grid.CanPlace(ePos) || !entranceCells.All(grid.CanPlace))
                    continue;

                AbstractCell doorNeighbor = shaftCell;
                if (needsCorridor)
                {
                    var corridor = grid.PlaceRun(Area.Brinstar, Scrolling.Horizontal, corridorCells[0], corrLen, CellRole.Corridor);
                    grid.LinkDoor(shaftCell, corridor.Cells[0]);
                    doorNeighbor = corridor.Cells[^1];
                }

                var elevRun = grid.PlaceRun(Area.Brinstar, Scrolling.Horizontal, ePos, 1, CellRole.ElevatorTop);
                elevRun.Cells[0].ForcedScreenId = elevatorScreen;
                grid.LinkDoor(doorNeighbor, elevRun.Cells[0]);

                var entrance = PlaceEntranceShaft(area, entranceCells[0], entranceLen);
                grid.LinkElevator(elevRun.Cells[0], entrance.Cells[0]);

                ShaftsOf(area).Add(entrance);
                bounds[area] = (Math.Max(1, ePos.X - AreaHalfWidth), Math.Min(30, ePos.X + AreaHalfWidth), ePos.Y + 1, 30);
                landmarks[$"{area}Elevator"] = ePos;
                return;
            }
        }

        throw new GenerationException($"could not place {area} elevator");
    }

    /// <summary>Entrance run below an elevator: 0x01 spacer, 0x02 platform, bodies, bottom cap.</summary>
    private Run PlaceEntranceShaft(Area area, Point top, int length)
    {
        var entrance = grid.PlaceRun(area, Scrolling.Vertical, top, length, CellRole.Shaft, capEnd: true);
        entrance.Cells[0].ForcedScreenId = 0x01;
        entrance.Cells[1].ForcedScreenId = 0x02;
        entrance.Cells[1].Role = CellRole.ElevatorBottom;
        entrance.Cells[0].SetEdge(Direction.Down, EdgeRequirement.Elevator);
        entrance.Cells[1].SetEdge(Direction.Up, EdgeRequirement.Elevator);
        return entrance;
    }

    /// <summary>
    /// Ridley hangs below Norfair: elevator room 0x05 sits directly east of a Norfair shaft.
    /// Norfair has already been pre-grown, so attach to the deepest shaft cell available,
    /// like vanilla where the Ridley elevator sits at the bottom edge of Norfair.
    /// </summary>
    private void BuildRidley()
    {
        var candidates = ShaftsOf(Area.Norfair)
            .SelectMany(InteriorCells)
            .OrderByDescending(c => c.Position.Y);

        foreach (var shaftCell in candidates)
        {
            if (!CanAddDoor(shaftCell, Direction.Right, RunKind.SingleCell))
                continue;

            var ePos = shaftCell.Position.Step(Direction.Right);
            int entranceLen = Rand(2, 7) + 3;
            var entranceCells = Enumerable.Range(ePos.Y + 1, entranceLen).Select(y => new Point(ePos.X, y)).ToList();
            if (!grid.CanPlace(ePos) || entranceCells[^1].Y > 30 || !entranceCells.All(grid.CanPlace))
                continue;

            // Norfair 0x05 'Ridley Elevator' is a vertical single-cell room with a left door.
            var elevRun = grid.PlaceRun(Area.Norfair, Scrolling.Vertical, ePos, 1, CellRole.ElevatorTop);
            elevRun.Cells[0].ForcedScreenId = 0x05;
            grid.LinkDoor(shaftCell, elevRun.Cells[0]);

            var entrance = PlaceEntranceShaft(Area.Ridley, entranceCells[0], entranceLen);
            grid.LinkElevator(elevRun.Cells[0], entrance.Cells[0]);

            ShaftsOf(Area.Ridley).Add(entrance);
            bounds[Area.Ridley] = (Math.Max(1, ePos.X - AreaHalfWidth), Math.Min(30, ePos.X + AreaHalfWidth), entranceCells[0].Y, 30);
            landmarks["RidleyElevator"] = ePos;
            return;
        }

        throw new GenerationException("could not place Ridley elevator");
    }

    // ---------------------------------------------------------------- bosses

    /// <summary>
    /// Kraid's lair complex, copied from the vanilla pattern: the lair's red door faces a
    /// short shaft capped above ([cap, 0x1B]); 0x1B's blue east door opens to a corridor
    /// that doors into an existing Kraid shaft.
    /// </summary>
    private void PlaceKraidLair()
    {
        foreach (var shaft in Shuffled(ShaftsOf(Area.Kraid)))
        {
            foreach (var shaftCell in Shuffled(InteriorCells(shaft)))
            {
                if (!CanAddDoor(shaftCell, Direction.Left, RunKind.MultiHorizontal))
                    continue;

                int y = shaftCell.Position.Y;
                int corrLen = Rand(2, 3);
                int miniX = shaftCell.Position.X - corrLen - 1;
                int lairX = miniX - 1;

                var needed = new List<Point> { new(lairX, y), new(miniX, y), new(miniX, y - 1) };
                needed.AddRange(Enumerable.Range(miniX + 1, corrLen).Select(x => new Point(x, y)));

                if (lairX < 0 || !needed.All(grid.CanPlace))
                    continue;

                var lair = grid.PlaceRun(Area.Kraid, Scrolling.Horizontal, new Point(lairX, y), 1, CellRole.Boss);
                lair.Cells[0].ForcedScreenId = 0x1D;

                // Two-cell shaft: cap on top, 0x1B 'Kraid Hallway' with doors on both sides below.
                var mini = grid.PlaceRun(Area.Kraid, Scrolling.Vertical, new Point(miniX, y - 1), 2, CellRole.Shaft, capStart: true);
                mini.Cells[1].ForcedScreenId = 0x1B;
                grid.LinkDoor(lair.Cells[0], mini.Cells[1], DoorType.Red, DoorType.Red);

                var corridor = grid.PlaceRun(Area.Kraid, Scrolling.Horizontal, new Point(miniX + 1, y), corrLen, CellRole.Corridor);
                grid.LinkDoor(mini.Cells[1], corridor.Cells[0]);
                grid.LinkDoor(corridor.Cells[^1], shaftCell);

                landmarks["Kraid"] = lair.Cells[0].Position;
                return;
            }
        }

        throw new GenerationException("could not place Kraid's lair");
    }

    /// <summary>
    /// Ridley's lair complex, copied from the vanilla pattern: energy tank corridor
    /// [0x11, .., 0x10] - purple door - lair 0x12 - blue door - corridor [0x13, .., 0x15]
    /// - blue door - existing Ridley shaft.
    /// </summary>
    private void PlaceRidleyLair()
    {
        foreach (var shaft in Shuffled(ShaftsOf(Area.Ridley)))
        {
            foreach (var shaftCell in Shuffled(InteriorCells(shaft)))
            {
                if (!CanAddDoor(shaftCell, Direction.Left, RunKind.MultiHorizontal))
                    continue;

                int y = shaftCell.Position.Y;
                int eastLen = Rand(2, 4);
                int lairX = shaftCell.Position.X - eastLen - 1;
                int tankLen = Rand(2, 3);

                var needed = new List<Point> { new(lairX, y) };
                needed.AddRange(Enumerable.Range(lairX + 1, eastLen).Select(x => new Point(x, y)));
                needed.AddRange(Enumerable.Range(lairX - tankLen, tankLen).Select(x => new Point(x, y)));

                if (lairX - tankLen < 0 || !needed.All(grid.CanPlace))
                    continue;

                var lair = grid.PlaceRun(Area.Ridley, Scrolling.Horizontal, new Point(lairX, y), 1, CellRole.Boss);
                lair.Cells[0].ForcedScreenId = 0x12;

                var east = grid.PlaceRun(Area.Ridley, Scrolling.Horizontal, new Point(lairX + 1, y), eastLen, CellRole.Corridor);
                east.Cells[0].ForcedScreenId = 0x13;
                east.Cells[^1].ForcedScreenId = 0x15;
                grid.LinkDoor(lair.Cells[0], east.Cells[0], DoorType.Blue, DoorType.Blue);
                grid.LinkDoor(east.Cells[^1], shaftCell);

                var tank = grid.PlaceRun(Area.Ridley, Scrolling.Horizontal, new Point(lairX - tankLen, y), tankLen, CellRole.Corridor);
                tank.Cells[0].ForcedScreenId = 0x11; // Ridley Energy Tank, solid west wall
                tank.Cells[0].Role = CellRole.Item;
                tank.Cells[^1].ForcedScreenId = 0x10; // Right Door Post Ridley (purple)
                grid.LinkDoor(tank.Cells[^1], lair.Cells[0], DoorType.Purple, DoorType.Purple);

                landmarks["Ridley"] = lair.Cells[0].Position;
                return;
            }
        }

        throw new GenerationException("could not place Ridley's lair");
    }

    // ---------------------------------------------------------------- chozo item rooms

    // Vanilla chozo statue complexes (Long Beam/Varia/Bomb/Ice, Screw Attack/Wave Beam):
    // [cap, chozo] item room - red doors - pre-item corridor [redLeft .. blueRight] - shaft.
    // Only Brinstar and Norfair have chozo screens.
    private static readonly Dictionary<Area, (int Chozo, int PreWest, int PreEast)> ChozoComplexes = new()
    {
        [Area.Brinstar] = (0x0A, 0x1A, 0x28),
        [Area.Norfair] = (0x04, 0x0F, 0x10),
    };

    /// <summary>
    /// Places one or two vanilla-style chozo item room complexes in each area that has them.
    /// Brinstar's first complex is always the vanilla Varia arrangement: the chozo corridor
    /// hangs off the shoot-up tower instead of doored directly into a shaft.
    /// </summary>
    private void PlaceChozoItemRooms()
    {
        foreach (var (area, spec) in ChozoComplexes)
        {
            int placed = 0;
            if (area == Area.Brinstar)
            {
                foreach (var shaftCell in Shuffled(ShaftsOf(area).SelectMany(InteriorCells)))
                {
                    if (TryPlaceVariaComplex(spec, shaftCell))
                    {
                        placed++;
                        break;
                    }
                }
                if (placed == 0)
                    throw new GenerationException("no varia tower complex placed in Brinstar");
            }

            int wanted = Rand(1, 2);
            foreach (var shaftCell in Shuffled(ShaftsOf(area).SelectMany(InteriorCells)))
            {
                if (placed >= wanted)
                    break;
                if (TryPlaceChozoComplex(area, spec, shaftCell))
                    placed++;
            }

            if (placed == 0)
                throw new GenerationException($"no chozo item room placed in {area}");
        }
    }

    /// <summary>
    /// The vanilla Varia arrangement: the chozo complex hangs off a two-piece tower instead
    /// of a main shaft. The tower bottom (0x1E) has doors on both sides under a breakable
    /// ceiling — shoot and jump up, then the middle piece (0x2E) needs HiJump to reach the
    /// chozo corridor's door, exactly the vanilla Varia climb:
    ///
    ///   [cap, chozo] - red doors - [0x1A .. 0x28] - blue door - 0x2E   &lt;- cap above 0x2E
    ///                                  entry corridor - blue door - 0x1E   &lt;- below 0x2E
    ///
    /// 0x1E's other door is sealed against a reserved-empty cell, like vanilla's unused
    /// openings. Both 0x1E doors faced multi-screen horizontal rooms in vanilla, so the
    /// entry corridor is always at least two cells.
    /// </summary>
    private bool TryPlaceVariaComplex((int Chozo, int PreWest, int PreEast) spec, AbstractCell shaftCell)
    {
        if (!CanAddDoor(shaftCell, Direction.Left, RunKind.MultiHorizontal))
            return false;

        int y = shaftCell.Position.Y;
        var (_, _, minY, _) = bounds[Area.Brinstar];
        if (y - 2 < minY)
            return false;

        int entryLen = Rand(2, 4);
        int tx = shaftCell.Position.X - entryLen - 1; // tower column
        int preLen = Rand(2, 4);
        int capX = tx - preLen - 2;                   // chozo item room cap column

        var needed = new List<Point> { new(tx - 1, y) }; // sealed-door cell, must stay free
        needed.AddRange(Enumerable.Range(0, 3).Select(i => new Point(tx, y - 2 + i)));
        needed.AddRange(Enumerable.Range(tx + 1, entryLen).Select(x => new Point(x, y)));
        needed.AddRange(Enumerable.Range(capX, preLen + 2).Select(x => new Point(x, y - 1)));

        if (capX < 0 || !needed.All(grid.CanPlace)
            || !CorridorEndExists(Area.Brinstar, Direction.Left, RunKind.MultiVertical)
            || !CorridorEndExists(Area.Brinstar, Direction.Right, RunKind.MultiVertical))
            return false;

        var tower = grid.PlaceRun(Area.Brinstar, Scrolling.Vertical, new Point(tx, y - 2), 3, CellRole.Shaft, capStart: true);
        tower.Cells[1].ForcedScreenId = 0x2E; // Left Door Empty Shaft: HiJump to the door
        tower.Cells[2].ForcedScreenId = 0x1E; // Breakable Top Shaft: shoot up, bomb back down
        grid.ReservedEmpty.Add(new Point(tx - 1, y));

        var entry = grid.PlaceRun(Area.Brinstar, Scrolling.Horizontal, new Point(tx + 1, y), entryLen, CellRole.Corridor);
        grid.LinkDoor(tower.Cells[2], entry.Cells[0]);
        grid.LinkDoor(entry.Cells[^1], shaftCell);

        var pre = grid.PlaceRun(Area.Brinstar, Scrolling.Horizontal, new Point(tx - preLen, y - 1), preLen, CellRole.Corridor);
        pre.Cells[0].ForcedScreenId = spec.PreWest;
        pre.Cells[^1].ForcedScreenId = spec.PreEast;
        grid.LinkDoor(pre.Cells[^1], tower.Cells[1]);

        var itemRoom = grid.PlaceRun(Area.Brinstar, Scrolling.Horizontal, new Point(capX, y - 1), 2, CellRole.Item, capStart: true);
        itemRoom.Cells[1].ForcedScreenId = spec.Chozo;
        grid.LinkDoor(itemRoom.Cells[1], pre.Cells[0], DoorType.Red, DoorType.Red);

        MaybePlaceItem(entry);
        landmarks["VariaShaft"] = tower.Cells[2].Position;
        return true;
    }

    private bool TryPlaceChozoComplex(Area area, (int Chozo, int PreWest, int PreEast) spec, AbstractCell shaftCell)
    {
        if (!CanAddDoor(shaftCell, Direction.Left, RunKind.MultiHorizontal))
            return false;

        int y = shaftCell.Position.Y;
        int preLen = Rand(2, 4);
        int chozoX = shaftCell.Position.X - preLen - 1;
        int capX = chozoX - 1;

        var needed = Enumerable.Range(capX, preLen + 2).Select(x => new Point(x, y)).ToList();
        if (capX < 0 || !needed.All(grid.CanPlace))
            return false;

        var itemRoom = grid.PlaceRun(area, Scrolling.Horizontal, new Point(capX, y), 2, CellRole.Item, capStart: true);
        itemRoom.Cells[1].ForcedScreenId = spec.Chozo;

        var pre = grid.PlaceRun(area, Scrolling.Horizontal, new Point(chozoX + 1, y), preLen, CellRole.Corridor);
        pre.Cells[0].ForcedScreenId = spec.PreWest;
        pre.Cells[^1].ForcedScreenId = spec.PreEast;

        grid.LinkDoor(itemRoom.Cells[1], pre.Cells[0], DoorType.Red, DoorType.Red);
        grid.LinkDoor(pre.Cells[^1], shaftCell, DoorType.Blue, DoorType.Blue);
        return true;
    }

    /// <summary>
    /// Places the cross-game portal anchors: a single-cell portal room (Brinstar 0x1F,
    /// blue left door, sealed right scroll) hanging east off a Brinstar shaft cell —
    /// the exact arrangement vanilla combo creates for its portal, so the engine-side
    /// transition code works unchanged. The cell east of the portal room is reserved
    /// empty because 0x1F's right scroll opening relies on the engine's seal-against-
    /// empty behavior. The combo layer decides what each portal connects to.
    /// </summary>
    private void PlacePortalAnchors()
    {
        int placed = 0;
        foreach (var shaftCell in Shuffled(ShaftsOf(Area.Brinstar).SelectMany(InteriorCells)))
        {
            if (placed >= PortalCount)
                break;

            var portalPos = shaftCell.Position.Step(Direction.Right);
            if (!grid.CanPlace(portalPos) || !grid.CanPlace(portalPos.Step(Direction.Right))
                || !CanAddDoor(shaftCell, Direction.Right, RunKind.SingleCell))
                continue;

            var portalRun = grid.PlaceRun(Area.Brinstar, Scrolling.Horizontal, portalPos, 1, CellRole.Portal);
            portalRun.Cells[0].ForcedScreenId = 0x1F;
            grid.LinkDoor(shaftCell, portalRun.Cells[0]);
            grid.ReservedEmpty.Add(portalPos.Step(Direction.Right));

            landmarks[$"Portal{placed}"] = portalPos;
            placed++;
        }

        if (placed < PortalCount)
            throw new GenerationException($"only placed {placed}/{PortalCount} portal anchors");
    }

    // ---------------------------------------------------------------- filler growth

    /// <summary>
    /// Per-area growth character. Connector corridors make dense vanilla-style ladder
    /// blocks — Norfair's identity, but the reason Kraid and Ridley came out boxy, so they
    /// trade connectors for long reaching corridors. The west bias sends Kraid sprawling
    /// into the open quadrant below Tourian; Ridley leans the other way.
    /// </summary>
    /// <param name="ConnectorPercent">Share of growth iterations spent on shaft connectors.</param>
    /// <param name="WestPercent">Chance a new branch grows west instead of east.</param>
    /// <param name="LongCorridorPercent">Chance a branch corridor uses the long length range.</param>
    /// <param name="SpacedBranches">Reject branches directly above/below an existing parallel
    /// corridor, so growth makes spread-out trees instead of stacked ladder blocks.</param>
    private sealed record GrowthProfile(int ConnectorPercent, int WestPercent, int LongCorridorPercent, bool SpacedBranches);

    private static readonly Dictionary<Area, GrowthProfile> GrowthProfiles = new()
    {
        [Area.Brinstar] = new(ConnectorPercent: 35, WestPercent: 50, LongCorridorPercent: 10, SpacedBranches: false),
        // Norfair keeps its ladder blocks but rolls more long corridors, so at vanilla
        // size it forms several connected blocks instead of one compact one.
        [Area.Norfair] = new(ConnectorPercent: 30, WestPercent: 50, LongCorridorPercent: 35, SpacedBranches: false),
        [Area.Kraid] = new(ConnectorPercent: 15, WestPercent: 70, LongCorridorPercent: 35, SpacedBranches: true),
        // No spaced branches for Ridley: it grows in the cramped band above the bottom
        // edge, where the spacing rule starves it below its size minimum.
        [Area.Ridley] = new(ConnectorPercent: 25, WestPercent: 30, LongCorridorPercent: 25, SpacedBranches: true),
    };

    private void GrowAreas()
    {
        if (Saturate)
        {
            GrowAreasToSaturation();
            return;
        }

        // Ridley first: it lives in the cramped bottom band and Norfair's shaft extensions
        // would dig into it before it gets a turn. Norfair right after (biggest vanilla area,
        // competes with Brinstar for the rows below the spine); Brinstar last since its
        // spine already guarantees its bulk.
        foreach (var area in new[] { Area.Ridley, Area.Norfair, Area.Kraid, Area.Brinstar })
        {
            var (min, target) = GoalFor(area);
            // Goals are aspirational (growth stops when space runs out; only min is hard),
            // so the floor sits at two thirds of the target — a low roll used to produce
            // areas hugging their minimum, which read as starved.
            int floor = Math.Max(min + 5, target * 2 / 3);
            GrowArea(area, Rand(floor, Math.Max(floor + 1, target)));

            if (grid.CellsOf(area).Count() < min)
                throw new GenerationException($"{area} too small: {grid.CellsOf(area).Count()} < {min}");
        }
    }

    /// <summary>
    /// Nightmare growth: rounds of small growth chunks over all four areas until a full
    /// round adds nothing anywhere. The round-robin keeps the space split fairly — one
    /// area growing to exhaustion first would wall the later ones in.
    /// </summary>
    private void GrowAreasToSaturation()
    {
        var areas = new[] { Area.Ridley, Area.Norfair, Area.Kraid, Area.Brinstar };

        // The normal column bounds keep areas in vanilla-like neighborhoods; saturation
        // opens the lower areas to the whole width so no region of the grid stays off
        // limits. Brinstar keeps its top band (the lower bands grow up against it).
        foreach (var area in new[] { Area.Norfair, Area.Kraid, Area.Ridley })
        {
            var (_, _, minY, _) = bounds[area];
            bounds[area] = (1, 30, minY, 30);
        }

        bool progress = true;
        while (progress)
        {
            progress = false;
            foreach (var area in areas)
            {
                int before = grid.CellsOf(area).Count();
                GrowArea(area, before + 12, iterationBudget: 260);
                if (grid.CellsOf(area).Count() > before)
                    progress = true;
            }
        }

        foreach (var area in areas)
        {
            var (min, _) = GoalFor(area);
            if (grid.CellsOf(area).Count() < min)
                throw new GenerationException($"{area} too small: {grid.CellsOf(area).Count()} < {min}");
        }

        // How far saturation gets depends on the early layout — an unlucky arrangement
        // partitions the grid and walls growth out of whole regions. Nightmare promises
        // "as big as possible", so an under-saturated result retries a new layout.
        int total = grid.Cells.Count();
        if (total < SaturationMinimum)
            throw new GenerationException($"saturation stalled at {total} cells");
    }

    /// <summary>Hard floor for Nightmare totals; under-saturated layouts retry.</summary>
    public const int SaturationMinimum = 400;

    /// <summary>
    /// Grows one area toward a cell-count goal by mixing two feature kinds: new corridor
    /// branches (optionally spawning new shafts) and connector corridors between existing
    /// shafts. The connectors are what produce vanilla's dense ladder blocks (parallel
    /// shafts joined by stacked corridor rows, like Norfair's missile rooms).
    /// </summary>
    private void GrowArea(Area area, int goal, int? iterationBudget = null, bool allowExtend = true)
    {
        var profile = GrowthProfiles[area];
        // Generous iteration budget: at vanilla-size goals most late iterations fail on
        // placement (occupied cells, no fitting screen), so attempts are cheap retries.
        int maxIterations = iterationBudget ?? Math.Max(120, goal * 20);
        // Nightmare trades looks for bulk: the density rules would starve saturation
        // (cramped areas like Ridley stall well below their minimums with rationed doors).
        densityRulesActive = !Saturate;
        for (int i = 0; i < maxIterations && grid.CellsOf(area).Count() < goal; i++)
        {
            int roll = rng.Next(100);
            if (roll < profile.ConnectorPercent)
                TryConnectShafts(area);
            else if (allowExtend && roll < profile.ConnectorPercent + ExtendPercent)
                TryExtendShaft(area);
            else
                TryAddFeature(area);
        }
        densityRulesActive = false;
    }

    /// <summary>
    /// Share of growth iterations spent deepening an existing shaft. Above-vanilla scales
    /// lean harder on extensions: they are the move that keeps minting fresh door slots
    /// once the bands fill up, which is what lets Large actually reach its bigger targets.
    /// </summary>
    private int ExtendPercent => SizeScale > 1.0 ? 20 : 12;

    /// <summary>
    /// Extension feature: pops a shaft's bottom cap and deepens the run by a few cells,
    /// re-capping the end. Extensions replenish the growth frontier — the door-density
    /// rules ration side-door slots per shaft, so without fresh shaft cells growth would
    /// stall once every shaft's slots are spent — and they stretch areas downward, which
    /// reads as sprawl instead of a block.
    /// </summary>
    private void TryExtendShaft(Area area)
    {
        var areaShafts = ShaftsOf(area);
        if (areaShafts.Count == 0)
            return;

        var shaft = Pick(areaShafts);
        var cap = shaft.Cells[^1];
        if (cap.Role != CellRole.Cap || cap.ForcedScreenId.HasValue)
            return;

        var (_, _, _, maxY) = bounds[area];
        int extra = Rand(2, 5);
        var newCells = Enumerable.Range(1, extra)
            .Select(i => new Point(cap.Position.X, cap.Position.Y + i)).ToList();
        if (newCells[^1].Y > maxY || !newCells.All(grid.CanPlace))
            return;

        grid.ExtendRunDown(shaft, extra, CellRole.Shaft);
    }

    /// <summary>
    /// Connector feature: a corridor between two existing shafts of the area at a shared
    /// interior row. The first connection between a pair is a real two-door passage; once
    /// a pair is connected, further corridors between them become dead-end rooms attached
    /// to only one of the shafts — stacked fully-connected rows read as one big chamber,
    /// while dead ends between the same shafts give the map maze-like pockets.
    /// </summary>
    private void TryConnectShafts(Area area)
    {
        var areaShafts = ShaftsOf(area);
        if (areaShafts.Count < 2)
            return;

        var first = Pick(areaShafts);
        var second = Pick(areaShafts);
        if (first == second)
            return;

        var (left, right) = first.Cells[0].Position.X < second.Cells[0].Position.X ? (first, second) : (second, first);
        int lx = left.Cells[0].Position.X;
        int rx = right.Cells[0].Position.X;
        int len = rx - lx - 1;
        if (len < 2 || len > 10)
            return;

        if (!CorridorEndExists(area, Direction.Left, RunKind.MultiVertical)
            || !CorridorEndExists(area, Direction.Right, RunKind.MultiVertical))
            return;

        var pairKey = (Math.Min(left.Id, right.Id), Math.Max(left.Id, right.Id));
        int connected = fullConnections.GetValueOrDefault(pairKey);

        // Every shaft is born attached to the existing structure, so the area graph is
        // always connected and every full connector here adds one more loop. Mazes are
        // trees plus a few loops: past the per-area cycle budget, connectors demote to
        // dead-end rooms (which keep the item-pocket texture without shortcuts).
        int cycleBudget = Math.Max(2, grid.CellsOf(area).Count() / 25);
        bool wantDeadEnd = areaCycles.GetValueOrDefault(area) >= cycleBudget
            || connected >= 2 || (connected >= 1 && rng.Next(100) < 70);
        if (wantDeadEnd && len < 3)
            return;
        bool deadEnd = wantDeadEnd;

        var rows = Shuffled(
            InteriorCells(left).Select(c => c.Position.Y)
                .Intersect(InteriorCells(right).Select(c => c.Position.Y)));

        foreach (int y in rows)
        {
            var leftCell = grid.Cell(lx, y)!;
            var rightCell = grid.Cell(rx, y)!;

            if (deadEnd)
            {
                // Attach to one side only; the far end caps short of the other shaft.
                bool fromLeft = rng.Next(2) == 0;
                var (anchor, dir) = fromLeft ? (leftCell, Direction.Right) : (rightCell, Direction.Left);
                if (!CanAddDoor(anchor, dir, RunKind.MultiHorizontal))
                    continue;

                int bodyLen = Rand(2, len - 1);
                int startX = fromLeft ? lx + 1 : rx - bodyLen - 1;
                var deadEndCells = Enumerable.Range(startX, bodyLen + 1).Select(x => new Point(x, y));
                if (!deadEndCells.All(grid.CanPlace))
                    continue;

                var room = grid.PlaceRun(area, Scrolling.Horizontal, new Point(startX, y), bodyLen + 1,
                    CellRole.Corridor, capStart: !fromLeft, capEnd: fromLeft);
                var doorCell = fromLeft ? room.Cells[0] : room.Cells[^1];
                if (fromLeft)
                    grid.LinkDoor(anchor, doorCell);
                else
                    grid.LinkDoor(doorCell, anchor);
                MaybePlaceItem(room);
                return;
            }

            if (!CanAddDoor(leftCell, Direction.Right, RunKind.MultiHorizontal)
                || !CanAddDoor(rightCell, Direction.Left, RunKind.MultiHorizontal))
                continue;

            var cells = Enumerable.Range(lx + 1, len).Select(x => new Point(x, y));
            if (!cells.All(grid.CanPlace))
                continue;

            var corridor = grid.PlaceRun(area, Scrolling.Horizontal, new Point(lx + 1, y), len, CellRole.Corridor);
            grid.LinkDoor(leftCell, corridor.Cells[0]);
            grid.LinkDoor(corridor.Cells[^1], rightCell);
            fullConnections[pairKey] = connected + 1;
            areaCycles[area] = areaCycles.GetValueOrDefault(area) + 1;
            MaybePlaceItem(corridor);
            return;
        }
    }

    /// <summary>
    /// One filler feature: a doored corridor off an existing shaft, ending either in a cap
    /// cell or in a door to a brand-new shaft (which becomes a future attachment point).
    /// Corridor interiors may carry item locations.
    /// </summary>
    private void TryAddFeature(Area area)
    {
        var areaShafts = ShaftsOf(area);
        if (areaShafts.Count == 0)
            return;

        // Bias toward the newest shafts (the frontier). Uniform picks grow a blob that
        // thickens around the entrance; growing mostly from freshly placed shafts makes
        // long branching arms, which is what reads as sprawl on the map.
        var shaft = rng.Next(100) < 60
            ? areaShafts[^(1 + rng.Next(Math.Max(1, areaShafts.Count / 3)))]
            : Pick(areaShafts);
        var interior = InteriorCells(shaft);
        if (interior.Count == 0)
            return;

        // Bias toward the deeper cells of the shaft so areas expand downward into the
        // unused bottom of the grid instead of clustering at their entrances.
        var ordered = interior.OrderByDescending(c => c.Position.Y).ToList();
        var cell = ordered[Math.Min(rng.Next(rng.Next(ordered.Count) + 1), ordered.Count - 1)];

        var profile = GrowthProfiles[area];
        var dir = rng.Next(100) < profile.WestPercent ? Direction.Left : Direction.Right;
        if (!CanAddDoor(cell, dir, RunKind.MultiHorizontal))
            return;

        // Spread-out areas refuse to stack a branch right on top of an existing parallel
        // corridor; the stacked rows are what reads as a solid box on the map. (This also
        // helps saturation: spread shafts are the anchors later growth hangs off.)
        if (profile.SpacedBranches)
        {
            var firstCorridorCell = cell.Position.Step(dir);
            bool ParallelCorridor(Point p) =>
                grid.Cell(p) is { } n && n.Run.Axis == Scrolling.Horizontal && n.Area == area;
            if (ParallelCorridor(firstCorridorCell.Step(Direction.Up))
                || ParallelCorridor(firstCorridorCell.Step(Direction.Down)))
                return;
        }

        // The corridor's near end doors back into the existing shaft; the far end (when a
        // new shaft is added) doors into that shaft. Both ends need real screens, and the
        // new shaft needs a body piece with a door facing the corridor.
        var nearSide = dir == Direction.Right ? Direction.Left : Direction.Right;
        if (!CorridorEndExists(area, nearSide, RunKind.MultiVertical))
            return;

        bool extendWithShaft = rng.Next(100) < 60
            && CorridorEndExists(area, dir, RunKind.MultiVertical)
            && catalog.HasPiece(area, Scrolling.Vertical,
                EdgeRequirement.Scroll, EdgeRequirement.Scroll,
                dir == Direction.Right ? EdgeRequirement.Door : EdgeRequirement.Wall,
                dir == Direction.Right ? EdgeRequirement.Wall : EdgeRequirement.Door,
                leftNeighbor: dir == Direction.Right ? RunKind.MultiHorizontal : null,
                rightNeighbor: dir == Direction.Left ? RunKind.MultiHorizontal : null);

        // Long corridors push new shafts well away from the existing cluster, which is
        // what keeps sprawly areas from packing into a solid block. Saturation and
        // above-vanilla scales retry shorter lengths so the feature can still squeeze
        // into a dense grid's gaps instead of wasting the iteration.
        int rolledLen = rng.Next(100) < profile.LongCorridorPercent ? Rand(5, 9) : Rand(2, 6);
        var lengths = Saturate || SizeScale > 1.0
            ? Enumerable.Range(2, rolledLen - 1).Reverse().ToArray()
            : [rolledLen];

        var (minX, maxX, minY, maxY) = bounds[area];
        int sign = dir == Direction.Right ? 1 : -1;

        List<Point>? corridorCells = null;
        List<Point>? shaftCells = null;
        int doorRowIndex = 0;

        foreach (var tryLen in lengths)
        {
            var cells = Enumerable.Range(1, tryLen)
                .Select(i => new Point(cell.Position.X + sign * i, cell.Position.Y)).ToList();

            // Without a new shaft the corridor ends in a cap cell.
            var farEnd = cells[^1].Step(dir);
            if (!extendWithShaft)
                cells.Add(farEnd);

            if (cells.Any(p => p.X < minX || p.X > maxX || p.Y < minY || p.Y > maxY)
                || !cells.All(grid.CanPlace))
                continue;

            if (extendWithShaft)
            {
                int up = Rand(0, 4);
                int down = Rand(1, 8);
                if (up + down < 2)
                    down = 2 - up;
                int yTop = cell.Position.Y - up - 1;   // -1 for the top cap
                int total = up + down + 3;             // body + two caps

                var candidate = Enumerable.Range(0, total).Select(i => new Point(farEnd.X, yTop + i)).ToList();
                if (candidate.Any(p => p.X < minX || p.X > maxX || p.Y < Math.Max(1, minY - 4) || p.Y > Math.Min(30, maxY))
                    || !candidate.All(grid.CanPlace))
                    continue;

                shaftCells = candidate;
                doorRowIndex = up + 1;
            }

            corridorCells = cells;
            break;
        }

        if (corridorCells == null)
            return;

        // Some branch corridors are gated by a red door on the corridor side and guarantee
        // an item, like vanilla's red-door item corridors (Kraid/Ridley have no chozo
        // screens; this is their item-room pattern). Only when the area really has a
        // corridor end piece with a red door that may face a shaft.
        bool redGate = rng.Next(100) < 35
            && CorridorEndExists(area, nearSide, RunKind.MultiVertical, DoorType.Red);

        // Commit.
        var corridorStart = dir == Direction.Right ? corridorCells[0] : corridorCells[^1];
        var corridor = grid.PlaceRun(area, Scrolling.Horizontal, corridorStart, corridorCells.Count, CellRole.Corridor,
            capStart: dir == Direction.Left && !extendWithShaft,
            capEnd: dir == Direction.Right && !extendWithShaft);

        var nearEnd = dir == Direction.Right ? corridor.Cells[0] : corridor.Cells[^1];
        if (dir == Direction.Right)
            grid.LinkDoor(cell, nearEnd, DoorType.Blue, redGate ? DoorType.Red : DoorType.Blue);
        else
            grid.LinkDoor(nearEnd, cell, redGate ? DoorType.Red : DoorType.Blue, DoorType.Blue);

        if (extendWithShaft)
        {
            var newShaft = grid.PlaceRun(area, Scrolling.Vertical, shaftCells![0], shaftCells.Count, CellRole.Shaft,
                capStart: true, capEnd: true);
            var shaftDoorCell = newShaft.Cells[doorRowIndex];
            var farEnd = dir == Direction.Right ? corridor.Cells[^1] : corridor.Cells[0];

            if (dir == Direction.Right)
                grid.LinkDoor(farEnd, shaftDoorCell);
            else
                grid.LinkDoor(shaftDoorCell, farEnd);

            ShaftsOf(area).Add(newShaft);
        }

        MaybePlaceItem(corridor, always: redGate);
    }

    /// <summary>Marks one corridor cell as an item location when a real item screen can fill it.</summary>
    private void MaybePlaceItem(Run corridor, bool always = false)
    {
        var itemCandidates = corridor.Cells.Where(c =>
            c.Role == CellRole.Corridor
            && catalog.ForArea(corridor.Area).Any(p => p.HasItemLocation && grid.FitsStrict(catalog, p, c))).ToList();

        if (itemCandidates.Count == 0 || (!always && rng.Next(100) >= 75))
            return;

        // In dead-end rooms the item goes to the back, behind the walk past the cap —
        // the classic reward-at-the-end-of-the-detour feel.
        var cap = corridor.Cells.FirstOrDefault(c => c.Role == CellRole.Cap);
        var pick = cap == null
            ? Pick(itemCandidates)
            : itemCandidates.OrderBy(c =>
                Math.Abs(c.Position.X - cap.Position.X) + Math.Abs(c.Position.Y - cap.Position.Y)).First();
        pick.Role = CellRole.Item;
    }

    /// <summary>
    /// Tops up item locations until the world can hold the item pool: the pool carries
    /// 10 progression items plus 20 missiles and 6 energy tanks, and beating Ridley needs
    /// 15 missiles, so with fewer than 31 locations an unlucky filler cannot make the seed
    /// winnable. Targets 33-36 (vanilla has ~36) and fails the attempt below the minimum.
    /// </summary>
    private void EnsureItemCells()
    {
        const int HardMinimum = 31;
        const int AreaMinimum = 2;
        // The item pool is fixed (ItemPooler: 10 progression + 20 missiles + 6 energy
        // tanks). More locations than pool items would leave unfilled pedestals, which
        // the emitted ROM tables render as free Bombs pickups (the placeholder item id).
        const int PoolSize = 36;
        int target = Rand(33, PoolSize);

        bool HoldsItem(AbstractCell c) => c.Role == CellRole.Item
            || (c.Role == CellRole.Boss && c.ForcedScreenId == 0x1D); // Kraid's lair holds the energy tank
        int Count() => grid.Cells.Count(HoldsItem);
        int AreaCount(Area area) => grid.CellsOf(area).Count(HoldsItem);

        // Interest steers placement gently toward spots that feel like rewards: cells in
        // dead-end rooms and cells out toward the map edges. The random jitter keeps it
        // a bias, not a rule.
        int Interest(AbstractCell c) =>
            Math.Max(Math.Abs(c.Position.X - 15), Math.Abs(c.Position.Y - 15))
            + (c.Run.Cells.Any(rc => rc.Role == CellRole.Cap) ? 6 : 0);

        var candidates = grid.Cells.Where(c => c.Role == CellRole.Corridor
                && catalog.ForArea(c.Area).Any(p => p.HasItemLocation && grid.FitsStrict(catalog, p, c)))
            .OrderByDescending(c => Interest(c) + rng.Next(10))
            .ToList();

        // Every playable area must hold a couple of items on its own; the growth rolls
        // make that likely but not certain (a west-sprawling Kraid can miss every roll).
        foreach (var area in new[] { Area.Brinstar, Area.Norfair, Area.Kraid, Area.Ridley })
        {
            foreach (var cell in candidates.Where(c => c.Area == area))
            {
                if (AreaCount(area) >= AreaMinimum)
                    break;
                cell.Role = CellRole.Item;
            }
            if (AreaCount(area) < AreaMinimum)
                throw new GenerationException($"{area} cannot hold {AreaMinimum} item locations");
        }

        foreach (var cell in candidates)
        {
            if (Count() >= target)
                break;
            if (cell.Role == CellRole.Corridor)
                cell.Role = CellRole.Item;
        }

        // Big maps can organically overshoot through the growth rolls: demote surplus
        // back to plain corridors, least interesting spots first. Forced item cells are
        // structural (the morph pedestal, chozo rooms) and per-area minimums must
        // survive the trim.
        foreach (var cell in grid.Cells.Where(c => c.Role == CellRole.Item && !c.ForcedScreenId.HasValue)
            .OrderBy(c => Interest(c) + rng.Next(10)))
        {
            if (Count() <= target)
                break;
            if (AreaCount(cell.Area) > AreaMinimum)
                cell.Role = CellRole.Corridor;
        }

        if (Count() < HardMinimum)
            throw new GenerationException($"only {Count()} item locations, need {HardMinimum}");
        if (Count() > PoolSize)
            throw new GenerationException($"{Count()} item locations exceed the {PoolSize}-item pool");
    }

    /// <summary>
    /// Forces vanilla hidden bomb-block walls (Brinstar 0x1D and friends) into a few
    /// corridor interiors, like the secret passages in vanilla's top-right corridor. Only
    /// screens gated in BOTH directions qualify: a wall with a free direction could drop a
    /// bombless player into a pocket it cannot leave, while a symmetric wall is only ever
    /// crossed with bombs in inventory. The logic graph picks the requirement up from the
    /// screen YAML. The start corridor is excluded so the no-equipment start guarantee
    /// (spawn beside the morph pedestal) survives; Tourian keeps its fixed template.
    /// </summary>
    private void PlaceHiddenBombWalls()
    {
        var startRun = grid.Cell(start)!.Run;
        int placed = 0;

        foreach (var area in new[] { Area.Brinstar, Area.Norfair, Area.Kraid, Area.Ridley })
        {
            var walls = catalog.ForArea(area).Where(p =>
                p.Axis == Scrolling.Horizontal
                && p.Left.Type == ConnectorType.Scroll && p.Right.Type == ConnectorType.Scroll
                && p.Up.Type == ConnectorType.None && p.Down.Type == ConnectorType.None
                && p.EdgesConnected(Direction.Left, Direction.Right)
                && !p.FreeEdgePairs.Contains((Direction.Left, Direction.Right))
                && !p.FreeEdgePairs.Contains((Direction.Right, Direction.Left))
                && !p.HasItemLocation && !p.HasBossLocation && !p.HasStartLocation
                && !p.HasElevatorPlatform && !p.IsOneWay).ToList();
            if (walls.Count == 0)
                continue;

            int wanted = Rand(1, 2);
            int areaPlaced = 0;
            foreach (var cell in Shuffled(grid.CellsOf(area).Where(c =>
                c.Role == CellRole.Corridor && !c.ForcedScreenId.HasValue && c.Run != startRun
                && c.Left == EdgeRequirement.Scroll && c.Right == EdgeRequirement.Scroll)))
            {
                if (areaPlaced >= wanted)
                    break;
                cell.ForcedScreenId = Pick(walls).ScreenId;
                landmarks[$"HiddenWall{placed}"] = cell.Position;
                areaPlaced++;
                placed++;
            }
        }

        if (placed == 0)
            throw new GenerationException("no hidden bomb wall placed");
    }

    // ---------------------------------------------------------------- validation

    private void ValidateWorld()
    {
        var fitErrors = grid.ValidateFittability(catalog);
        if (fitErrors.Count > 0)
            throw new GenerationException("fittability: " + fitErrors[0] + $" (+{fitErrors.Count - 1} more)");

        var reachProblems = AxisSolver.Validate(grid, start);
        if (reachProblems.Count > 0)
            throw new GenerationException("reachability: " + reachProblems[0] + $" (+{reachProblems.Count - 1} more)");

        int elevators = grid.Links.Count(l => l.Type == LinkType.Elevator);
        if (elevators != 4)
            throw new GenerationException($"expected 4 elevator links, found {elevators}");

        foreach (var required in new[] { "Start", "StatuesGate", "TourianElevator", "MotherBrain", "EscapeShaft",
                                         "Kraid", "Ridley", "ConstructionZone", "VariaShaft", "HiddenWall0" })
            if (!landmarks.ContainsKey(required))
                throw new GenerationException($"missing landmark {required}");
    }

    private List<string> BuildDiagnostics(int attempts)
    {
        var lines = new List<string> { grid.Summary() };
        lines.AddRange(landmarks.Select(kv => $"{kv.Key}: {kv.Value}"));
        lines.Add($"items: {grid.Cells.Count(c => c.Role == CellRole.Item)}");
        lines.Add($"attempts: {attempts}");
        return lines;
    }
}
