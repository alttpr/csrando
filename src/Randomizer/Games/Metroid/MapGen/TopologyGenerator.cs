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
    /// (growth stops when space runs out â€” Ridley in particular rarely reaches vanilla
    /// size in the band it gets); the scaled minimums are hard and trigger a retry.
    /// </summary>
    public double SizeScale { get; init; } = 1.0;

    /// <summary>
    /// Areas that get a cross-game portal room (see <see cref="DataLoader.PortalRoomAreas"/>
    /// â€” always all built; rooms never connected to a portal stay inert dead ends).
    /// </summary>
    public IReadOnlyList<Area> PortalAreas { get; init; } = [Area.Brinstar];

    /// <summary>
    /// The area that hosts the start/pedestal pair. Brinstar keeps its original early
    /// placement inside <see cref="BuildBrinstarSpine"/> (bit-identical seeds); any
    /// other area places the pair after growth, when it has corridors to spare.
    /// </summary>
    public Area StartArea { get; init; } = Area.Brinstar;

    /// <summary>
    /// Per-area pedestal screen forced next to the start: an item location reachable
    /// with zero equipment, which the filler's front fill depends on. Screen ids are
    /// area-local. Tourian has no item screens and cannot host a start.
    /// </summary>
    private static readonly Dictionary<Area, int> StartPedestalScreens = new()
    {
        [Area.Brinstar] = 0x17, // Morph Pedestal
        [Area.Norfair] = 0x19,  // Missile Pillar
        [Area.Kraid] = 0x10,    // Grey Pillars
        [Area.Ridley] = 0x0F,   // Grey Item Pillar
    };

    /// <summary>
    /// Nightmare mode: ignore the size targets and keep growing every area until nothing
    /// fits anymore, saturating the grid. Minimums stay at the SizeScale level (a hard
    /// minimum near saturation would make generation impossible).
    /// </summary>
    public bool Saturate { get; init; }

    private Random rng = new(0);
    private CancellationToken cancellationToken;
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
        [Area.Ridley] = (34, 87),
    };

    private (int Min, int Target) GoalFor(Area area)
    {
        // Minimums never scale above 1.0: targets past vanilla size are best-effort, and
        // raising the hard floor with them converts every growth stall into a whole-world
        // retry, which fails a large fraction of seeds outright at above-vanilla scales.
        var (min, target) = BaseSizeGoals[area];
        return (Math.Max(10, (int)(min * Math.Min(1.0, SizeScale))), Math.Max(12, (int)(target * SizeScale)));
    }

    /// <summary>Column half-width of a lower area's band, widened for above-vanilla scales.</summary>
    private int AreaHalfWidth => (int)Math.Round(16 * Math.Max(1.0, SizeScale));

    /// <summary>Diagnostics hook: called with the failure reason of every failed attempt.</summary>
    public Action<string>? AttemptFailed { get; init; }

    public GeneratedWorld Generate(int seed)
    {
        var failures = new List<string>();
        cancellationToken = GenerationContext.CancellationToken;

        for (int attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
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
                BuildMiniSpine(Area.Kraid, Rand(1, 2));
                BuildLowerArea(Area.Norfair, elevatorScreen: 0x0B, preferRight: true);
                BuildMiniSpine(Area.Norfair, Rand(1, 2));

                // Let Norfair claim some space before Ridley is hung off one of its shafts;
                // otherwise Ridley always sits next to the entrance and boxes Norfair in.
                // The budget is fixed so high SizeScale values don't starve Ridley. No shaft
                // extensions here: Ridley hangs below Norfair's deepest shaft, and pre-grow
                // extensions dig Norfair to the grid bottom, leaving Ridley no vertical band.
                GrowArea(Area.Norfair, grid.CellsOf(Area.Norfair).Count() + 20, allowExtend: false);

                BuildRidley();
                BuildMiniSpine(Area.Ridley, 1);
                PlaceChozoItemRooms();
                PlacePortalAnchors();

                // Lairs are placed AFTER targeted growth so the anchor pool holds every
                // grown shaft, not just the entrance shaft â€” otherwise both boss rooms
                // always dangle directly off the area's main shaft. Minimums are checked
                // after the lairs so their complexes count toward area size, like they
                // did when lairs were placed first. Under Saturate the saturation rounds
                // then grow around the placed lair complexes (and re-check minimums).
                GrowAreasTargeted();
                densityRulesActive = false;
                PlaceKraidLair();
                PlaceRidleyLair();
                if (Saturate)
                    GrowAreasToSaturation();
                else
                    EnforceAreaMinimums();

                // Non-Brinstar starts are placed only now: they claim no new space
                // (two existing corridor cells are re-roled), and the sparse lower
                // areas rarely have a qualifying corridor before growth. Must precede
                // PlaceMapStations, whose depth search is rooted at the start.
                if (StartArea != Area.Brinstar)
                    PlaceStart(StartArea, grid.Runs);

                PlaceMapStations();
                EnsureItemCells();
                PlaceBombPassages();
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
                AttemptFailed?.Invoke(e.Message);
            }
        }

        throw new InvalidOperationException(
            $"M1 topology generation failed after {MaxAttempts} attempts for seed {seed}. Last failures:\n" +
            string.Join("\n", failures.TakeLast(10)));
    }

    private static int Chebyshev(Point a, Point b) =>
        Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));

    /// <summary>
    /// Score handicap for anchoring a boss lair on the area's entrance shaft. Doubled
    /// distance plus 0-8 jitter means an entrance row must out-distance every other
    /// shaft's rows by ~6+ cells to win — possible, but rarer than any other shaft.
    /// </summary>
    private const int EntranceAnchorPenalty = 12;

    private int Rand(int minInclusive, int maxInclusive) => rng.Next(minInclusive, maxInclusive + 1);
    private T Pick<T>(IReadOnlyList<T> list) => list.Count > 0 ? list[rng.Next(list.Count)]
        : throw new GenerationException("empty pick list");
    private List<T> Shuffled<T>(IEnumerable<T> source) => source.OrderBy(_ => rng.Next()).ToList();

    private List<Run> ShaftsOf(Area area) => shafts.TryGetValue(area, out var list) ? list : shafts[area] = [];

    /// <summary>
    /// Shaft cells that can take side doors: interiors (scroll both ways) and run ends
    /// (one scroll edge) â€” vanilla constantly puts doors on bottom/top pieces like Ridley
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
    /// <summary>
    /// A plain one-screen room with a single blue door on <paramref name="doorSide"/> and no
    /// other connectors, whose vanilla door faced a shaft. Boss and structural-item screens
    /// are excluded (those belong to their dedicated placements).
    /// </summary>
    private bool SingleDoorRoomExists(Area area, Direction doorSide) =>
        catalog.Query(area, Scrolling.Horizontal,
            EdgeRequirement.Wall, EdgeRequirement.Wall,
            doorSide == Direction.Left ? EdgeRequirement.Door : EdgeRequirement.Wall,
            doorSide == Direction.Right ? EdgeRequirement.Door : EdgeRequirement.Wall,
            leftNeighbor: doorSide == Direction.Left ? RunKind.MultiVertical : null,
            rightNeighbor: doorSide == Direction.Right ? RunKind.MultiVertical : null)
        .Any(p => p.Connector(doorSide).Color == DoorType.Blue && !p.HasBossLocation && !p.HasStructuralItem
            // Truly closed on the other three edges (tunnels are sealed screen
            // internals): a piece with extra scroll openings only fits while its
            // neighbors stay empty, which a dense grown grid does not guarantee.
            && Directions.All.Where(d => d != doorSide).All(d =>
                p.Connector(d).Type is ConnectorType.None or ConnectorType.Tunnel));

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

        // Brinstar starts pick their pair from the fresh spine corridors, before any
        // other draw, so existing Brinstar seeds stay bit-identical. Other areas place
        // theirs after growth (see Generate), when the area has corridors to spare.
        if (StartArea == Area.Brinstar)
            PlaceStart(Area.Brinstar, corridors);
    }

    /// <summary>
    /// Re-roles two adjacent free-scrolling corridor cells into the start pair: the
    /// spawn screen and, like vanilla's Morph Room, an item pedestal right next to it.
    /// The pedestal guarantees at least one item location reachable with no equipment,
    /// which the item filler's front fill depends on.
    /// </summary>
    private void PlaceStart(Area area, IEnumerable<Run> candidateRuns)
    {
        if (!StartPedestalScreens.TryGetValue(area, out int pedestalScreen))
            throw new GenerationException($"{area} cannot host a start (no pedestal screen)");

        // Vanilla-style alternative for the lower areas: start on the elevator arrival
        // platform, the same cell death/continue respawns use (and the spawn the
        // engine was built for). Rolled at one-in-three so the corridor-with-pedestal
        // start stays the common case; also the fallback when no corridor qualifies.
        // Brinstar is never an elevator destination, so its early placement never
        // reaches this and stays bit-identical.
        var elevatorArrivals = grid.Links
            .Where(l => l.Type == LinkType.Elevator)
            .Select(l => l.B.Step(Direction.Down))
            .Where(p => grid.Cell(p)?.Area == area)
            .ToList();
        if (elevatorArrivals.Count > 0 && Rand(0, 2) == 0)
        {
            start = Pick(elevatorArrivals);
            landmarks["Start"] = start;
            return;
        }

        var startPairs = new List<(AbstractCell Spawn, AbstractCell Pedestal)>();
        foreach (var run in candidateRuns.Where(r => r.Area == area && r.Axis == Scrolling.Horizontal))
        {
            for (int i = 0; i + 1 < run.Cells.Count; i++)
            {
                var a = run.Cells[i];
                var b = run.Cells[i + 1];
                if (a.Role == CellRole.Corridor && b.Role == CellRole.Corridor
                    && !a.ForcedScreenId.HasValue && !b.ForcedScreenId.HasValue
                    && a.Left == EdgeRequirement.Scroll && a.Right == EdgeRequirement.Scroll
                    && b.Left == EdgeRequirement.Scroll && b.Right == EdgeRequirement.Scroll
                    && a.Up == EdgeRequirement.Wall && a.Down == EdgeRequirement.Wall
                    && b.Up == EdgeRequirement.Wall && b.Down == EdgeRequirement.Wall)
                {
                    startPairs.Add((a, b));
                    startPairs.Add((b, a));
                }
            }
        }
        if (startPairs.Count == 0)
        {
            if (elevatorArrivals.Count > 0)
            {
                start = Pick(elevatorArrivals);
                landmarks["Start"] = start;
                return;
            }
            throw new GenerationException($"no {area} corridor with adjacent interior cells for start + pedestal");
        }

        var (spawn, pedestal) = Pick(startPairs);
        spawn.Role = CellRole.Start;
        pedestal.Role = CellRole.Item;
        pedestal.ForcedScreenId = pedestalScreen;
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
    /// has doors on both sides and a bombable floor â€” in vanilla it sits right of the start
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

    /// <summary>
    /// Gives a lower area a small spine before growth: extra shafts hung off the existing
    /// ones via doored corridors, spaced a few columns out. Without this the lower areas
    /// grow as a single tall comb off the entrance shaft — and the boss lair has nothing
    /// but the entrance to anchor to. Brinstar reads well precisely because its spine
    /// spreads it across several medium shafts before growth starts.
    /// </summary>
    private void BuildMiniSpine(Area area, int extraShafts)
    {
        for (int n = 0; n < extraShafts; n++)
            if (!TryAddSpineShaft(area))
                throw new GenerationException($"could not build the {area} mini spine");
    }

    private bool TryAddSpineShaft(Area area)
    {
        var profile = GrowthProfiles[area];
        var (minX, maxX, minY, maxY) = bounds[area];
        int topLimit = Math.Max(1, minY - 4);
        int bottomLimit = Math.Min(30, maxY);

        var preferred = rng.Next(100) < profile.WestPercent ? Direction.Left : Direction.Right;
        foreach (var dir in new[] { preferred, Directions.Opposite(preferred) })
        {
            var nearSide = Directions.Opposite(dir);
            if (!CorridorEndExists(area, nearSide, RunKind.MultiVertical)
                || !CorridorEndExists(area, dir, RunKind.MultiVertical)
                || !catalog.HasPiece(area, Scrolling.Vertical,
                    EdgeRequirement.Scroll, EdgeRequirement.Scroll,
                    dir == Direction.Right ? EdgeRequirement.Door : EdgeRequirement.Wall,
                    dir == Direction.Right ? EdgeRequirement.Wall : EdgeRequirement.Door,
                    leftNeighbor: dir == Direction.Right ? RunKind.MultiHorizontal : null,
                    rightNeighbor: dir == Direction.Left ? RunKind.MultiHorizontal : null))
                continue;

            int sign = dir == Direction.Right ? 1 : -1;
            foreach (var anchor in Shuffled(ShaftsOf(area).SelectMany(InteriorCells)))
            {
                if (!CanAddDoor(anchor, dir, RunKind.MultiHorizontal))
                    continue;

                // Retry shorter corridors before abandoning the anchor: cramped bands
                // (Ridley's) rarely fit the long roll, and the spine is a hard
                // requirement, so a failed roll here is a whole-world retry.
                List<Point>? corridorCells = null;
                List<Point>? shaftCells = null;
                foreach (int corrLen in Enumerable.Range(2, Rand(3, 6) - 1).Reverse())
                {
                    var corridorTry = Enumerable.Range(1, corrLen)
                        .Select(i => new Point(anchor.Position.X + sign * i, anchor.Position.Y)).ToList();
                    var farEnd = corridorTry[^1].Step(dir);

                    int maxUp = Math.Max(0, anchor.Position.Y - 1 - topLimit);
                    int maxDown = Math.Max(0, bottomLimit - anchor.Position.Y - 1);
                    int up = Math.Min(Rand(0, 3), maxUp);
                    int down = Math.Min(Rand(2, 6), maxDown);
                    if (up + down < 2)
                    {
                        down = Math.Min(2 - up, maxDown);
                        if (up + down < 2)
                            up = Math.Min(2 - down, maxUp);
                    }
                    if (down < 1 || up + down < 2)
                        continue;
                    int yTop = anchor.Position.Y - up - 1;
                    var column = Enumerable.Range(0, up + down + 3)
                        .Select(i => new Point(farEnd.X, yTop + i)).ToList();

                    var all = corridorTry.Concat(column);
                    if (all.Any(p => p.X < minX || p.X > maxX || p.Y < topLimit || p.Y > bottomLimit)
                        || !all.All(grid.CanPlace))
                        continue;

                    corridorCells = corridorTry;
                    shaftCells = column;
                    break;
                }
                if (corridorCells == null)
                    continue;

                var corridorStart = dir == Direction.Right ? corridorCells[0] : corridorCells[^1];
                var corridor = grid.PlaceRun(area, Scrolling.Horizontal, corridorStart, corridorCells.Count, CellRole.Corridor);
                var nearEnd = dir == Direction.Right ? corridor.Cells[0] : corridor.Cells[^1];
                if (dir == Direction.Right)
                    grid.LinkDoor(anchor, nearEnd);
                else
                    grid.LinkDoor(nearEnd, anchor);

                var shaft = grid.PlaceRun(area, Scrolling.Vertical, shaftCells![0], shaftCells.Count,
                    CellRole.Shaft, capStart: true, capEnd: true);
                var doorCell = shaft.Cells[anchor.Position.Y - shaftCells[0].Y];
                var corridorFar = dir == Direction.Right ? corridor.Cells[^1] : corridor.Cells[0];
                if (dir == Direction.Right)
                    grid.LinkDoor(corridorFar, doorCell);
                else
                    grid.LinkDoor(doorCell, corridorFar);

                ShaftsOf(area).Add(shaft);
                MaybePlaceItem(corridor);
                return true;
            }
        }

        return false;
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
        // Far-from-elevator rows first, with a hefty penalty (not a ban) on the entrance
        // shaft: even its deepest rows read as "the boss hangs off the main shaft under
        // the elevator", and pure distance ordering kept picking them — vertical distance
        // down the same shaft is still the same shaft. The penalty keeps entrance
        // attachment possible but rarer than any other shaft.
        var elevator = landmarks["KraidElevator"];
        foreach (var shaftCell in ShaftsOf(Area.Kraid).SelectMany(InteriorCells)
            .OrderByDescending(c => 2 * Chebyshev(c.Position, elevator) + rng.Next(9)
                - (c.Run.Cells[0].ForcedScreenId == 0x01 ? EntranceAnchorPenalty : 0)))
        {
            {
                if (!CanAddDoor(shaftCell, Direction.Left, RunKind.MultiHorizontal))
                    continue;

                int y = shaftCell.Position.Y;
                // Try every corridor length (shuffled) before giving up on this row: after
                // growth most rows are partially occupied, and insisting on one rolled shape
                // would leave the roomy entrance shaft as the only anchor that ever fits.
                int? fitCorrLen = null;
                foreach (int candidateLen in Shuffled(new[] { 2, 3 }))
                {
                    int candMiniX = shaftCell.Position.X - candidateLen - 1;
                    int candLairX = candMiniX - 1;
                    var candNeeded = new List<Point> { new(candLairX, y), new(candMiniX, y), new(candMiniX, y - 1) };
                    candNeeded.AddRange(Enumerable.Range(candMiniX + 1, candidateLen).Select(x => new Point(x, y)));
                    if (candLairX - 1 >= 0 && grid.CanPlace(new Point(candLairX - 1, y)) && candNeeded.All(grid.CanPlace))
                    {
                        fitCorrLen = candidateLen;
                        break;
                    }
                }
                if (fitCorrLen == null)
                    continue;

                int corrLen = fitCorrLen.Value;
                int miniX = shaftCell.Position.X - corrLen - 1;
                int lairX = miniX - 1;

                // 0x1D (Kraid's Lair) has no real transition on its west side; the engine still
                // scrolls Samus off the screen boundary there regardless of exit tiles, so the
                // cell west of it must stay reserved-empty (matches vanilla's $FF map cell).
                int westX = lairX - 1;

                var lair = grid.PlaceRun(Area.Kraid, Scrolling.Horizontal, new Point(lairX, y), 1, CellRole.Boss);
                lair.Cells[0].ForcedScreenId = 0x1D;
                grid.ReservedEmpty.Add(new Point(westX, y));

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
        // Far-from-elevator rows first, with a hefty penalty (not a ban) on the entrance
        // shaft: even its deepest rows read as "the boss hangs off the main shaft under
        // the elevator", and pure distance ordering kept picking them — vertical distance
        // down the same shaft is still the same shaft. The penalty keeps entrance
        // attachment possible but rarer than any other shaft.
        var elevator = landmarks["RidleyElevator"];
        foreach (var shaftCell in ShaftsOf(Area.Ridley).SelectMany(InteriorCells)
            .OrderByDescending(c => 2 * Chebyshev(c.Position, elevator) + rng.Next(9)
                - (c.Run.Cells[0].ForcedScreenId == 0x01 ? EntranceAnchorPenalty : 0)))
        {
            {
                if (!CanAddDoor(shaftCell, Direction.Left, RunKind.MultiHorizontal))
                    continue;

                int y = shaftCell.Position.Y;
                // Try every complex shape (shuffled) before giving up on this row: Ridley's
                // band is cramped, and insisting on one rolled shape would leave the roomy
                // entrance shaft as the only anchor that ever fits.
                (int East, int Tank)? fitShape = null;
                foreach (var shape in Shuffled(
                    from e in new[] { 2, 3, 4 } from t in new[] { 2, 3 } select (East: e, Tank: t)))
                {
                    int candLairX = shaftCell.Position.X - shape.East - 1;
                    var candNeeded = new List<Point> { new(candLairX, y) };
                    candNeeded.AddRange(Enumerable.Range(candLairX + 1, shape.East).Select(x => new Point(x, y)));
                    candNeeded.AddRange(Enumerable.Range(candLairX - shape.Tank, shape.Tank).Select(x => new Point(x, y)));
                    if (candLairX - shape.Tank >= 0 && candNeeded.All(grid.CanPlace))
                    {
                        fitShape = shape;
                        break;
                    }
                }
                if (fitShape == null)
                    continue;

                int eastLen = fitShape.Value.East;
                int tankLen = fitShape.Value.Tank;
                int lairX = shaftCell.Position.X - eastLen - 1;

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
    /// ceiling â€” shoot and jump up, then the middle piece (0x2E) needs HiJump to reach the
    /// chozo corridor's door, exactly the vanilla Varia climb:
    ///
    ///   [cap, chozo] - red doors - [0x1A .. 0x28] - blue door - 0x2E   &lt;- cap above 0x2E
    ///           west corridor - blue door - 0x1E - blue door - entry corridor   &lt;- below 0x2E
    ///
    /// 0x1E has doors on both sides, like vanilla, with the tower sitting mid-corridor. The
    /// east door is the entry from the anchor shaft; the west door opens a corridor that
    /// terminates in a fresh shaft, so later growth sprawls Brinstar west off it like any
    /// other shaft. A door must always be linked to a real room â€” unlike a scroll opening,
    /// it is not sealed by the engine against an empty cell.
    /// </summary>
    private bool TryPlaceVariaComplex((int Chozo, int PreWest, int PreEast) spec, AbstractCell shaftCell)
    {
        if (!CanAddDoor(shaftCell, Direction.Left, RunKind.MultiHorizontal))
            return false;

        int y = shaftCell.Position.Y;
        var (minX, _, minY, maxY) = bounds[Area.Brinstar];
        if (y - 2 < minY)
            return false;

        int entryLen = Rand(2, 4);
        int tx = shaftCell.Position.X - entryLen - 1; // tower column
        int preLen = Rand(2, 4);
        int capX = tx - preLen - 2;                   // chozo item room cap column

        // West of 0x1E: a corridor (its vanilla left-door context is MultiHorizontal), always
        // placed so the door never seals against empty space. The mandatory footprint is a
        // short 2-cell stub, kept small so it rarely blocks an anchor row; a longer westward
        // shaft is optionally appended off its far end below, when there is room past the chozo.
        int westLen = 2;
        int westCorrStartX = tx - westLen;            // corridor spans [westCorrStartX, tx-1]

        var needed = Enumerable.Range(0, 3).Select(i => new Point(tx, y - 2 + i)).ToList();
        needed.AddRange(Enumerable.Range(tx + 1, entryLen).Select(x => new Point(x, y)));
        needed.AddRange(Enumerable.Range(capX, preLen + 2).Select(x => new Point(x, y - 1)));
        needed.AddRange(Enumerable.Range(westCorrStartX, westLen).Select(x => new Point(x, y))); // west corridor stub

        if (capX < 0 || westCorrStartX < minX || !needed.All(grid.CanPlace)
            || !CorridorEndExists(Area.Brinstar, Direction.Left, RunKind.MultiVertical)
            || !CorridorEndExists(Area.Brinstar, Direction.Right, RunKind.MultiVertical))
            return false;

        var tower = grid.PlaceRun(Area.Brinstar, Scrolling.Vertical, new Point(tx, y - 2), 3, CellRole.Shaft, capStart: true);
        tower.Cells[1].ForcedScreenId = 0x2E; // Left Door Empty Shaft: HiJump to the door
        tower.Cells[2].ForcedScreenId = 0x1E; // Breakable Top Shaft: shoot up, bomb back down

        var entry = grid.PlaceRun(Area.Brinstar, Scrolling.Horizontal, new Point(tx + 1, y), entryLen, CellRole.Corridor);
        grid.LinkDoor(tower.Cells[2], entry.Cells[0]);
        grid.LinkDoor(entry.Cells[^1], shaftCell);

        // Chozo complex at row y-1, placed before the west shaft so its footprint is committed
        // and visible to the west shaft's CanPlace probe (the shaft column may fall under it).
        var pre = grid.PlaceRun(Area.Brinstar, Scrolling.Horizontal, new Point(tx - preLen, y - 1), preLen, CellRole.Corridor);
        pre.Cells[0].ForcedScreenId = spec.PreWest;
        pre.Cells[^1].ForcedScreenId = spec.PreEast;
        grid.LinkDoor(pre.Cells[^1], tower.Cells[1]);

        var itemRoom = grid.PlaceRun(Area.Brinstar, Scrolling.Horizontal, new Point(capX, y - 1), 2, CellRole.Item, capStart: true);
        itemRoom.Cells[1].ForcedScreenId = spec.Chozo;
        grid.LinkDoor(itemRoom.Cells[1], pre.Cells[0], DoorType.Red, DoorType.Red);

        // West side: try a corridor running past the chozo into a fresh vertical shaft that
        // growth can sprawl off; otherwise fall back to the mandatory short stub, capped at
        // its west end.
        if (!TryPlaceVariaWestGrowth(tower.Cells[2], capX))
        {
            var stub = grid.PlaceRun(Area.Brinstar, Scrolling.Horizontal, new Point(westCorrStartX, y), westLen,
                CellRole.Corridor, capStart: true);
            grid.LinkDoor(stub.Cells[^1], tower.Cells[2]);
            MaybePlaceItem(stub);
        }

        MaybePlaceItem(entry);
        landmarks["VariaShaft"] = tower.Cells[2].Position;
        return true;
    }

    /// <summary>
    /// Best-effort westward growth for the Varia tower: a corridor running from 0x1E (at
    /// <paramref name="tower1E"/>) west past the chozo complex (columns capX..) into a fresh
    /// two-way vertical shaft, which is registered so <see cref="GrowAreas"/> sprawls Brinstar
    /// west off it â€” the tower sitting mid-corridor like vanilla. All-or-nothing: places nothing
    /// and returns false if the footprint does not fit (the caller then lays a short capped
    /// stub instead). The chozo must already be committed so the shaft column clears it.
    /// </summary>
    private bool TryPlaceVariaWestGrowth(AbstractCell tower1E, int capX)
    {
        var (minX, _, minY, maxY) = bounds[Area.Brinstar];
        int tx = tower1E.Position.X;
        int y = tower1E.Position.Y;

        int shaftX = capX - 2;                  // one clear column west of the chozo cap (capX-1)
        int corrStartX = shaftX + 1;            // corridor body starts east of the shaft
        int corrLen = tx - corrStartX;          // corridor spans [corrStartX, tx-1]
        if (shaftX < minX || corrLen < 2)
            return false;

        // A two-way shaft body with a right door facing the corridor must exist for the area.
        if (!catalog.HasPiece(Area.Brinstar, Scrolling.Vertical,
                EdgeRequirement.Scroll, EdgeRequirement.Scroll, EdgeRequirement.Wall, EdgeRequirement.Door,
                rightNeighbor: RunKind.MultiHorizontal))
            return false;

        // The shaft's top cap sits one row above the door row (column < capX, so it clears the
        // chozo). Try a deep shaft first, then shorter ones so it can still squeeze into a tight
        // band instead of falling back to a bare stub.
        int top = y - 1;
        if (top < Math.Max(1, minY - 4))
            return false;

        var corridorCells = Enumerable.Range(corrStartX, corrLen).Select(x => new Point(x, y)).ToList();
        if (!corridorCells.All(grid.CanPlace))
            return false;

        int total = 0;
        foreach (int candidate in Enumerable.Range(0, 4).Select(i => Rand(2, 5) + 3 - i).Where(t => t >= 5))
        {
            if (top + candidate - 1 <= maxY
                && Enumerable.Range(0, candidate).Select(i => new Point(shaftX, top + i)).All(grid.CanPlace))
            {
                total = candidate;
                break;
            }
        }
        if (total == 0)
            return false;

        var corridor = grid.PlaceRun(Area.Brinstar, Scrolling.Horizontal, new Point(corrStartX, y), corrLen, CellRole.Corridor);
        grid.LinkDoor(corridor.Cells[^1], tower1E);
        var shaft = grid.PlaceRun(Area.Brinstar, Scrolling.Vertical, new Point(shaftX, top), total,
            CellRole.Shaft, capStart: true, capEnd: true);
        grid.LinkDoor(shaft.Cells[1], corridor.Cells[0]); // Cells[0]=top cap, Cells[1]=row-y body
        ShaftsOf(Area.Brinstar).Add(shaft);
        MaybePlaceItem(corridor);
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
    /// Places the cross-game portal anchors: a single-cell left-door portal room hanging
    /// east off a same-area shaft cell. The cell east of the portal room is reserved
    /// empty because these rooms' right scroll openings rely on the engine's
    /// seal-against-empty behavior. The combo layer decides what each portal connects to.
    /// </summary>
    private void PlacePortalAnchors()
    {
        int placed = 0;
        foreach (var area in PortalAreas)
        {
            bool areaPlaced = false;
            foreach (var shaftCell in Shuffled(ShaftsOf(area).SelectMany(InteriorCells)))
            {
                var portalPos = shaftCell.Position.Step(Direction.Right);
                if (!grid.CanPlace(portalPos) || !grid.CanPlace(portalPos.Step(Direction.Right))
                    || !CanAddDoor(shaftCell, Direction.Right, RunKind.SingleCell))
                    continue;

                var portalRun = grid.PlaceRun(area, Scrolling.Horizontal, portalPos, 1, CellRole.Portal);
                portalRun.Cells[0].ForcedScreenId = PortalScreenFor(area);
                grid.LinkDoor(shaftCell, portalRun.Cells[0]);
                grid.ReservedEmpty.Add(portalPos.Step(Direction.Right));

                landmarks[$"Portal{placed}"] = portalPos;
                placed++;
                areaPlaced = true;
                break;
            }

            if (!areaPlaced)
                throw new GenerationException($"could not place {area} portal anchor");
        }
    }

    private static int PortalScreenFor(Area area) => area switch
    {
        Area.Brinstar => 0x1F, // Left Door Wavers
        Area.Norfair => 0x12,  // Left Door Eyes
        Area.Kraid => 0x17,    // Left Door Blue Geegas
        _ => throw new GenerationException($"{area} cannot host an M1 portal anchor"),
    };

    // ---------------------------------------------------------------- map stations

    /// <summary>
    /// Places one map-station cell per area except Tourian, holding the fixed map pickup
    /// (custom item $CE) that reveals the area's automap. Runs after growth so depth is
    /// meaningful over the final layout: each station prefers a dedicated dead-end
    /// corridor hung off a shaft cell near a per-area ROLLED target depth â€” between
    /// halfway and all the way out by room-graph distance from the entry. The station
    /// still rewards exploration but is no longer always at the absolute far end of the
    /// area. When the grown grid has no space left for a new corridor (common near
    /// saturation), the existing corridor cell nearest the target depth that can hold
    /// an item screen is converted instead â€” consuming a corridor cell EnsureItemCells
    /// could otherwise use; an area too tight for both fails the attempt like any other
    /// placement failure.
    /// </summary>
    private void PlaceMapStations()
    {
        foreach (var area in new[] { Area.Brinstar, Area.Norfair, Area.Kraid, Area.Ridley })
        {
            var depths = AreaDepths(area);
            int maxDepth = depths.Values.Max();
            int targetDepth = Rand(maxDepth / 2, maxDepth);

            if (TryPlaceMapStationRoom(area, depths, targetDepth))
                continue;

            var conversion = ConvertibleCellNear(area, depths, targetDepth);
            if (conversion == null)
                throw new GenerationException($"could not place a {area} map station");

            conversion.Role = CellRole.MapStation;
            landmarks[$"{area}MapStation"] = conversion.Position;
        }
    }

    /// <summary>
    /// Room-graph BFS distance of every cell of <paramref name="area"/> from its entry
    /// point: the start cell for the start area, the elevator platform for areas
    /// entered from above, the elevator host cell for Brinstar when the start is
    /// elsewhere (Brinstar is never an elevator destination). Scroll, door, and
    /// intra-area elevator edges all count as one step.
    /// </summary>
    private Dictionary<Point, int> AreaDepths(Area area)
    {
        var root = area == StartArea
            ? start
            : grid.Links.Where(l => l.Type == LinkType.Elevator)
                .Select(l => l.B.Step(Direction.Down))
                .FirstOrDefault(p => grid.Cell(p)?.Area == area);
        if (grid.Cell(root)?.Area != area && area == Area.Brinstar)
            root = grid.Links.Where(l => l.Type == LinkType.Elevator)
                .Select(l => l.A)
                .FirstOrDefault(p => grid.Cell(p)?.Area == Area.Brinstar);
        if (grid.Cell(root)?.Area != area)
            throw new GenerationException($"no entry cell for {area} depth search");

        var depths = new Dictionary<Point, int> { [root] = 0 };
        var queue = new Queue<Point>();
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            var cell = grid.Cell(p)!;
            foreach (var dir in Directions.All)
            {
                if (cell.Edge(dir) == EdgeRequirement.Wall)
                    continue;
                var neighbor = grid.Cell(p.Step(dir));
                if (neighbor == null || neighbor.Area != area || neighbor.Role == CellRole.Cap
                    || depths.ContainsKey(neighbor.Position))
                    continue;
                depths[neighbor.Position] = depths[p] + 1;
                queue.Enqueue(neighbor.Position);
            }
        }

        return depths;
    }

    /// <summary>
    /// Hangs a dead-end map-station corridor off a shaft cell near the rolled target
    /// depth: door, two or three walkable cells, solid cap at the far end. The station
    /// sits on the fittable cell nearest the cap (reward at the end of the detour, like
    /// item corridors). A placed corridor whose cells turn out not to fit any item
    /// screen simply stays a plain dead-end corridor and the search moves on.
    /// </summary>
    private bool TryPlaceMapStationRoom(Area area, Dictionary<Point, int> depths, int targetDepth)
    {
        // The quarter-depth floor keeps the graceful fallback (nothing fits near the
        // target) from degrading all the way to a station greeting the player at the
        // area entrance.
        int depthFloor = depths.Values.Max() / 4;
        var candidates = ShaftsOf(area).SelectMany(InteriorCells)
            .Where(c => depths.GetValueOrDefault(c.Position) >= depthFloor)
            .OrderBy(c => 2 * Math.Abs(depths.GetValueOrDefault(c.Position) - targetDepth) + rng.Next(4))
            .ToList();

        foreach (var shaftCell in candidates)
        {
            foreach (var side in Shuffled(new[] { Direction.Left, Direction.Right }))
            {
                int walk = Rand(2, 3);
                int y = shaftCell.Position.Y;
                var startX = side == Direction.Right ? shaftCell.Position.X + 1 : shaftCell.Position.X - walk - 1;
                var cells = Enumerable.Range(startX, walk + 1).Select(x => new Point(x, y)).ToList();

                if (cells[0].X < 1 || cells[^1].X > 30 || !cells.All(grid.CanPlace)
                    || !CanAddDoor(shaftCell, side, RunKind.MultiHorizontal)
                    || !CorridorEndExists(area, Directions.Opposite(side), RunKind.MultiVertical))
                    continue;

                var corridor = grid.PlaceRun(area, Scrolling.Horizontal, cells[0], walk + 1, CellRole.Corridor,
                    capStart: side == Direction.Left, capEnd: side == Direction.Right);
                if (side == Direction.Right)
                    grid.LinkDoor(shaftCell, corridor.Cells[0]);
                else
                    grid.LinkDoor(corridor.Cells[^1], shaftCell);

                var cap = corridor.Cells.First(c => c.Role == CellRole.Cap);
                var station = corridor.Cells
                    .Where(c => c.Role == CellRole.Corridor
                        && catalog.ForArea(area).Any(p => p.HasItemLocation && grid.FitsStrict(catalog, p, c)))
                    .OrderBy(c => Math.Abs(c.Position.X - cap.Position.X))
                    .FirstOrDefault();
                if (station == null)
                    continue;

                station.Role = CellRole.MapStation;
                landmarks[$"{area}MapStation"] = station.Position;
                return true;
            }
        }

        return false;
    }

    /// <summary>The existing corridor cell nearest the target depth that could hold the map-station item.</summary>
    private AbstractCell? ConvertibleCellNear(Area area, Dictionary<Point, int> depths, int targetDepth) =>
        grid.CellsOf(area)
            .Where(c => c.Role == CellRole.Corridor && !c.ForcedScreenId.HasValue
                && depths.GetValueOrDefault(c.Position) >= depths.Values.Max() / 4)
            .OrderBy(c => Math.Abs(depths.GetValueOrDefault(c.Position) - targetDepth))
            .FirstOrDefault(c => catalog.ForArea(area).Any(p => p.HasItemLocation && grid.FitsStrict(catalog, p, c)));

    // ---------------------------------------------------------------- filler growth

    /// <summary>
    /// Per-area growth character. Connector corridors make dense vanilla-style ladder
    /// blocks â€” Norfair's identity, but the reason Kraid and Ridley came out boxy, so they
    /// trade connectors for long reaching corridors. The west bias sends Kraid sprawling
    /// into the open quadrant below Tourian; Ridley leans the other way.
    /// </summary>
    /// <param name="ConnectorPercent">Share of growth iterations spent on shaft connectors.</param>
    /// <param name="WestPercent">Chance a new branch grows west instead of east.</param>
    /// <param name="LongCorridorPercent">Chance a branch corridor uses the long length range.</param>
    /// <param name="SpacedBranches">Reject branches directly above/below an existing parallel
    /// corridor, so growth makes spread-out trees instead of stacked ladder blocks.</param>
    /// <param name="PocketPercent">Chance a branch iteration places a one-screen room behind a
    /// door directly off the shaft (vanilla's item-room pattern) instead of a corridor â€”
    /// door-gated pockets that read as maze texture.</param>
    /// <param name="CycleDivisor">Cells per allowed loop (TryConnectShafts full connectors);
    /// smaller means more loops.</param>
    /// <param name="DeadEndDemotePercent">Chance a repeat connector between an already-joined
    /// shaft pair demotes to a dead-end room instead of forming another loop.</param>
    private sealed record GrowthProfile(int ConnectorPercent, int WestPercent, int LongCorridorPercent,
        bool SpacedBranches, int PocketPercent, int CycleDivisor, int DeadEndDemotePercent);

    private static readonly Dictionary<Area, GrowthProfile> GrowthProfiles = new()
    {
        [Area.Brinstar] = new(ConnectorPercent: 35, WestPercent: 50, LongCorridorPercent: 10, SpacedBranches: false,
            PocketPercent: 15, CycleDivisor: 16, DeadEndDemotePercent: 50),
        // Norfair keeps its ladder blocks (connectors are exempt from branch spacing) but
        // spaces its BRANCH corridors so dead-end combs stop stacking into solid slabs.
        [Area.Norfair] = new(ConnectorPercent: 30, WestPercent: 50, LongCorridorPercent: 35, SpacedBranches: true,
            PocketPercent: 12, CycleDivisor: 15, DeadEndDemotePercent: 50),
        // Kraid keeps the high demotion: its long corridors make repeat connectors read
        // as one solid chamber between two shafts, the exact look the demotion exists for.
        [Area.Kraid] = new(ConnectorPercent: 15, WestPercent: 70, LongCorridorPercent: 35, SpacedBranches: true,
            PocketPercent: 15, CycleDivisor: 20, DeadEndDemotePercent: 70),
        // Spaced branches historically starved Ridley below its minimum, but with the
        // band-clamped shaft spawns, upward extensions and the mini spine it now has the
        // room — and its stacked corridor combs were the loudest player complaint. West
        // bias at 45: west-frontier shafts have open west rows, which is what gives the
        // lair complex (always west-extending) anchors besides the entrance shaft.
        [Area.Ridley] = new(ConnectorPercent: 25, WestPercent: 45, LongCorridorPercent: 25, SpacedBranches: true,
            PocketPercent: 15, CycleDivisor: 18, DeadEndDemotePercent: 50),
    };

    /// <summary>
    /// Grows every area toward its size target. Runs in Saturate mode too (before the
    /// saturation rounds) so boss lairs can be placed against a mostly-grown map.
    /// Minimums are NOT checked here â€” the boss lair complexes placed afterwards count
    /// toward area size, so <see cref="EnforceAreaMinimums"/> runs after them.
    /// </summary>
    private void GrowAreasTargeted()
    {
        // Ridley first: it lives in the cramped bottom band and Norfair's shaft extensions
        // would dig into it before it gets a turn. Norfair right after (biggest vanilla area,
        // competes with Brinstar for the rows below the spine); Brinstar last since its
        // spine already guarantees its bulk.
        foreach (var area in new[] { Area.Ridley, Area.Norfair, Area.Kraid, Area.Brinstar })
        {
            var (min, target) = GoalFor(area);
            // Goals are aspirational (growth stops when space runs out; only min is hard),
            // so the floor sits at two thirds of the target â€” otherwise a low roll leaves an
            // area hugging its minimum, which reads as starved.
            int floor = Math.Max(min + 5, target * 2 / 3);
            GrowArea(area, Rand(floor, Math.Max(floor + 1, target)), densityRules: true);
        }
    }

    private void EnforceAreaMinimums()
    {
        foreach (var area in new[] { Area.Ridley, Area.Norfair, Area.Kraid, Area.Brinstar })
        {
            var (min, _) = GoalFor(area);
            if (grid.CellsOf(area).Count() < min)
                throw new GenerationException($"{area} too small: {grid.CellsOf(area).Count()} < {min}");
        }
    }

    /// <summary>
    /// Nightmare growth: rounds of small growth chunks over all four areas until a full
    /// round adds nothing anywhere. The round-robin keeps the space split fairly â€” one
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
            cancellationToken.ThrowIfCancellationRequested();
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

        // How far saturation gets depends on the early layout â€” an unlucky arrangement
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
    private void GrowArea(Area area, int goal, int? iterationBudget = null, bool allowExtend = true,
        bool? densityRules = null)
    {
        var profile = GrowthProfiles[area];
        // Generous iteration budget: at vanilla-size goals most late iterations fail on
        // placement (occupied cells, no fitting screen), so attempts are cheap retries.
        int maxIterations = iterationBudget ?? Math.Max(120, goal * 20);
        // Nightmare trades looks for bulk: the density rules would starve the saturation
        // rounds (cramped areas like Ridley stall well below their minimums with rationed
        // doors). The TARGETED phase keeps the rules on even under Saturate â€” its goals
        // are standard-scale, and rationed doors there leave the row gaps the boss lairs
        // need before saturation densifies everything.
        densityRulesActive = densityRules ?? !Saturate;
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
    /// Extension feature: pops a shaft's end cap and lengthens the run by a few cells,
    /// re-capping the end. Extensions replenish the growth frontier â€” the door-density
    /// rules ration side-door slots per shaft, so without fresh shaft cells growth would
    /// stall once every shaft's slots are spent â€” and they stretch areas along the grid,
    /// which reads as sprawl instead of a block. Mostly downward; a 30% upward roll keeps
    /// bottom-pinned areas (Ridley's shafts start at the band floor) from stalling once
    /// their downward room is gone.
    /// </summary>
    /// <summary>
    /// Extension ceiling: shafts longer than this read as empty transit tubes (few door
    /// slots per cell, long doorless stretches), which is the "shaft heavy" look player
    /// feedback keeps flagging. Entrance shafts may roll slightly longer at birth.
    /// </summary>
    private const int MaxShaftLength = 11;

    private void TryExtendShaft(Area area)
    {
        var areaShafts = ShaftsOf(area);
        if (areaShafts.Count == 0)
            return;

        var shaft = Pick(areaShafts);
        if (shaft.Cells.Count >= MaxShaftLength)
            return;

        var (_, _, minY, maxY) = bounds[area];
        int extra = Math.Min(Rand(2, 5), MaxShaftLength - shaft.Cells.Count);

        if (rng.Next(100) < 30)
        {
            var top = shaft.Cells[0];
            if (top.Role != CellRole.Cap || top.ForcedScreenId.HasValue)
                return;
            int topLimit = Math.Max(1, minY - 4);
            var upCells = Enumerable.Range(1, extra)
                .Select(i => new Point(top.Position.X, top.Position.Y - i)).ToList();
            if (upCells[^1].Y < topLimit || !upCells.All(grid.CanPlace))
                return;
            grid.ExtendRunUp(shaft, extra, CellRole.Shaft);
            return;
        }

        var cap = shaft.Cells[^1];
        if (cap.Role != CellRole.Cap || cap.ForcedScreenId.HasValue)
            return;

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
    /// to only one of the shafts â€” stacked fully-connected rows read as one big chamber,
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
        var connectProfile = GrowthProfiles[area];
        int cycleBudget = Math.Max(2, grid.CellsOf(area).Count() / connectProfile.CycleDivisor);
        bool wantDeadEnd = areaCycles.GetValueOrDefault(area) >= cycleBudget
            || connected >= 2
            || (connected >= 1 && rng.Next(100) < connectProfile.DeadEndDemotePercent);
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
        // unused bottom of the grid instead of clustering at their entrances â€” but only
        // half the time, so mid-shaft branches still appear and areas read as maze
        // rather than bottom-hugging combs.
        var ordered = interior.OrderByDescending(c => c.Position.Y).ToList();
        var cell = rng.Next(2) == 0
            ? Pick(ordered)
            : ordered[Math.Min(rng.Next(rng.Next(ordered.Count) + 1), ordered.Count - 1)];

        var profile = GrowthProfiles[area];
        var dir = rng.Next(100) < profile.WestPercent ? Direction.Left : Direction.Right;

        // Single-room pocket: vanilla's item-room pattern â€” a one-screen room behind a
        // door directly off a shaft cell. One cell, so it fits where corridors cannot
        // (dense grids, Ridley's band), and the extra doors are what make shafts read
        // as maze texture instead of a comb of scroll stubs. The pocket's door faced a
        // shaft in vanilla (context-checked); the shaft side faces a single-cell room,
        // which any door accepts.
        if (rng.Next(100) < profile.PocketPercent)
        {
            var pocketPos = cell.Position.Step(dir);
            var (pMinX, pMaxX, pMinY, pMaxY) = bounds[area];
            if (CanAddDoor(cell, dir, RunKind.SingleCell)
                && SingleDoorRoomExists(area, dir == Direction.Right ? Direction.Left : Direction.Right)
                && pocketPos.X >= pMinX && pocketPos.X <= pMaxX && pocketPos.Y >= pMinY && pocketPos.Y <= pMaxY
                && grid.CanPlace(pocketPos))
            {
                var pocket = grid.PlaceRun(area, Scrolling.Horizontal, pocketPos, 1, CellRole.Corridor);
                if (dir == Direction.Right)
                    grid.LinkDoor(cell, pocket.Cells[0]);
                else
                    grid.LinkDoor(pocket.Cells[0], cell);
                MaybePlaceItem(pocket);
            }
            return;
        }

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

        bool Inside(IEnumerable<Point> ps) =>
            !ps.Any(p => p.X < minX || p.X > maxX || p.Y < minY || p.Y > maxY);

        List<Point>? corridorCells = null;
        List<Point>? shaftCells = null;
        int doorRowIndex = 0;

        foreach (var tryLen in lengths)
        {
            var cells = Enumerable.Range(1, tryLen)
                .Select(i => new Point(cell.Position.X + sign * i, cell.Position.Y)).ToList();
            var farEnd = cells[^1].Step(dir);

            // Without a new shaft the corridor ends in a cap cell.
            if (!extendWithShaft)
                cells.Add(farEnd);

            if (!Inside(cells) || !cells.All(grid.CanPlace))
                continue;

            if (extendWithShaft)
            {
                // Clamp the rolled extents to the area band instead of discarding the
                // feature: cramped bands (Ridley's especially) otherwise roll out-of-bounds
                // shafts so often that whole areas grow without ever spawning a second
                // shaft â€” and the boss lair then has nothing but the entrance to anchor to.
                int topLimit = Math.Max(1, minY - 4);
                int bottomLimit = Math.Min(30, maxY);
                int maxUp = Math.Max(0, cell.Position.Y - 1 - topLimit);
                int maxDown = Math.Max(0, bottomLimit - cell.Position.Y - 1);
                int up = Math.Min(Rand(0, 4), maxUp);
                // Cap total length at MaxShaftLength: a freshly rolled 15-tall tube reads
                // as an empty transit shaft no matter how growth dresses it later.
                int down = Math.Min(Math.Min(Rand(1, 8), MaxShaftLength - 3 - up), maxDown);
                if (up + down < 2)
                {
                    down = Math.Min(2 - up, maxDown);
                    if (up + down < 2)
                        up = Math.Min(2 - down, maxUp);
                }
                if (down < 1 || up + down < 2)
                    continue;
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

        // In dead-end rooms the item goes to the back, behind the walk past the cap â€”
        // the classic reward-at-the-end-of-the-detour feel.
        var cap = corridor.Cells.FirstOrDefault(c => c.Role == CellRole.Cap);
        var pick = cap == null
            ? Pick(itemCandidates)
            : itemCandidates.OrderBy(c =>
                Math.Abs(c.Position.X - cap.Position.X) + Math.Abs(c.Position.Y - cap.Position.Y)).First();
        pick.Role = CellRole.Item;
    }

    /// <summary>
    /// Tops up item locations to a map-size-scaled target. Beating Ridley needs 15 missiles,
    /// so even Small maps retain the historical 31-location safety floor. Standard stays
    /// near vanilla's ~36 locations, while Large and Nightmare maps keep roughly the same
    /// item density instead of spreading the fixed vanilla pool over much larger worlds.
    /// </summary>
    /// <summary>Hard ceiling on item locations, kept in sync with the ItemPooler's pool.</summary>
    private int ItemLocationCap => Saturate ? 54 : Math.Max(31, (int)Math.Round(36 * SizeScale));

    private void EnsureItemCells()
    {
        const int HardMinimum = 31;
        const int AreaMinimum = 2;
        int targetMaximum = ItemLocationCap;
        int targetMinimum = Math.Max(HardMinimum, targetMaximum - 3);
        int target = Rand(targetMinimum, targetMaximum);

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

        // Per-area shares keep the pool where the space is. Without them the edge-biased
        // Interest ordering hoovers items into Ridley's small bottom band (denser in items
        // than areas twice its size — player feedback), so Ridley is damped toward its
        // vanilla sparseness and Norfair/Kraid are favored.
        var shareWeights = new Dictionary<Area, double>
        {
            [Area.Brinstar] = 1.0,
            [Area.Norfair] = 1.2,
            [Area.Kraid] = 1.2,
            [Area.Ridley] = 0.7,
        };
        double weightedTotal = shareWeights.Sum(kv => grid.CellsOf(kv.Key).Count() * kv.Value);
        var areaTarget = shareWeights.ToDictionary(kv => kv.Key, kv =>
            Math.Max(AreaMinimum,
                (int)Math.Round(target * grid.CellsOf(kv.Key).Count() * kv.Value / weightedTotal)));

        // Top up per area toward its share first, then fill any global shortfall from
        // whatever fittable cells remain (share rounding and tight areas leave slack).
        foreach (var cell in candidates)
        {
            if (Count() >= target)
                break;
            if (cell.Role == CellRole.Corridor && AreaCount(cell.Area) < areaTarget.GetValueOrDefault(cell.Area))
                cell.Role = CellRole.Item;
        }
        foreach (var cell in candidates)
        {
            if (Count() >= target)
                break;
            if (cell.Role == CellRole.Corridor)
                cell.Role = CellRole.Item;
        }

        // Maps can organically overshoot through the growth rolls: demote surplus back to
        // plain corridors — cells in over-share areas first, then least interesting spots.
        // Forced item cells are structural (the morph pedestal, chozo rooms) and per-area
        // minimums survive.
        foreach (var cell in grid.Cells.Where(c => c.Role == CellRole.Item && !c.ForcedScreenId.HasValue)
            .OrderByDescending(c => AreaCount(c.Area) - areaTarget.GetValueOrDefault(c.Area, AreaMinimum))
            .ThenBy(c => Interest(c) + rng.Next(10)))
        {
            if (Count() <= target)
                break;
            if (AreaCount(cell.Area) > AreaMinimum)
                cell.Role = CellRole.Corridor;
        }

        // Growth rolls can also land the right TOTAL in the wrong places (Ridley's
        // guaranteed red-gate corridors overshoot its small share): move surplus items
        // from over-share areas into under-share areas at constant total.
        foreach (var cell in grid.Cells.Where(c => c.Role == CellRole.Item && !c.ForcedScreenId.HasValue)
            .OrderByDescending(c => AreaCount(c.Area) - areaTarget.GetValueOrDefault(c.Area, AreaMinimum))
            .ThenBy(c => Interest(c) + rng.Next(10))
            .ToList())
        {
            if (AreaCount(cell.Area) <= Math.Max(AreaMinimum, areaTarget.GetValueOrDefault(cell.Area)))
                continue;
            var receiver = candidates.FirstOrDefault(c => c.Role == CellRole.Corridor
                && AreaCount(c.Area) < areaTarget.GetValueOrDefault(c.Area));
            if (receiver == null)
                break;
            cell.Role = CellRole.Corridor;
            receiver.Role = CellRole.Item;
        }

        if (Count() < HardMinimum)
            throw new GenerationException($"only {Count()} item locations, need {HardMinimum}");
        if (Count() > targetMaximum)
            throw new GenerationException(
                $"{Count()} item locations exceed the {targetMaximum}-item target");
    }

    /// <summary>
    /// Vanilla morph-tunnel triples, forced onto three consecutive corridor body cells.
    /// Interior seams are tunnel-to-tunnel exactly as the screens sat in their vanilla
    /// rooms (Brinstar room 0E03: 0x25|0x24|0x26; Norfair room 1614: 0x14|0x06|0x14), so
    /// no novel seam physics are introduced; the outer seams stay ordinary scroll pairs.
    /// The Kraid triple (0x13|0x19|0x12) is deliberately absent: 0x13 carries a red door
    /// and can only anchor red-gated corridor ends, so Kraid relies on 0x12 instead.
    /// Each entry is one line to disable if emulator verification finds a bad seam.
    /// </summary>
    private static readonly Dictionary<Area, int[]> TunnelChains = new()
    {
        [Area.Brinstar] = [0x25, 0x24, 0x26],
        [Area.Norfair] = [0x14, 0x06, 0x14],
    };

    /// <summary>
    /// Forces vanilla hidden bomb-block walls and gated morph passages into a few corridor
    /// interiors, like the secret passages in vanilla's top-right corridor. Only screens
    /// gated in BOTH directions qualify: a wall with a free direction could drop a
    /// bombless player into a pocket it cannot leave. Plain walls (Brinstar 0x1D and
    /// friends) go on corridor cells as before. Item-bearing gated screens (Kraid 0x12,
    /// Morph one way / bombs back) additionally require a THROUGH run — door links at
    /// both ends of the corridor — because their two directions need different equipment:
    /// a player who crosses with only one of them must be able to continue out the other
    /// side rather than being sealed in a dead end. The logic graph picks the requirements
    /// up from the screen YAML. The start corridor is excluded so the no-equipment start
    /// guarantee (spawn beside the morph pedestal) survives; Tourian keeps its template.
    /// </summary>
    private void PlaceBombPassages()
    {
        var startRun = grid.Cell(start)!.Run;
        int placed = 0;

        bool SymmetricGated(ScreenProfile p) =>
            p.Axis == Scrolling.Horizontal
            && p.Left.Type == ConnectorType.Scroll && p.Right.Type == ConnectorType.Scroll
            && p.Up.Type == ConnectorType.None && p.Down.Type == ConnectorType.None
            && p.EdgesConnected(Direction.Left, Direction.Right)
            && !p.FreeEdgePairs.Contains((Direction.Left, Direction.Right))
            && !p.FreeEdgePairs.Contains((Direction.Right, Direction.Left))
            && !p.HasBossLocation && !p.HasStartLocation
            && !p.HasElevatorPlatform && !p.IsOneWay;

        bool ThroughRun(AbstractCell c) =>
            c.Run.Cells[0].Left == EdgeRequirement.Door
            && c.Run.Cells[^1].Right == EdgeRequirement.Door;

        foreach (var area in new[] { Area.Brinstar, Area.Norfair, Area.Kraid, Area.Ridley })
        {
            var walls = catalog.ForArea(area).Where(p => SymmetricGated(p) && !p.HasItemLocation).ToList();
            var itemWalls = catalog.ForArea(area).Where(p => SymmetricGated(p) && p.HasItemLocation).ToList();

            int wanted = Rand(1, 2);
            int areaPlaced = 0;

            bool Force(AbstractCell cell, List<ScreenProfile> pieces)
            {
                cell.ForcedScreenId = Pick(pieces).ScreenId;
                landmarks[$"HiddenWall{area}{areaPlaced}"] = cell.Position;
                areaPlaced++;
                placed++;
                return areaPlaced >= wanted;
            }

            if (walls.Count > 0)
            {
                foreach (var cell in Shuffled(grid.CellsOf(area).Where(c =>
                    c.Role == CellRole.Corridor && !c.ForcedScreenId.HasValue && c.Run != startRun
                    && c.Left == EdgeRequirement.Scroll && c.Right == EdgeRequirement.Scroll)))
                {
                    if (Force(cell, walls))
                        break;
                }
            }

            if (itemWalls.Count > 0 && areaPlaced < wanted)
            {
                // Prefer an existing item cell on a through run; if none exists, promote a
                // through-run corridor cell to an item location while the pool cap allows.
                var candidates = grid.CellsOf(area).Where(c =>
                    !c.ForcedScreenId.HasValue && c.Run != startRun && ThroughRun(c)
                    && c.Left == EdgeRequirement.Scroll && c.Right == EdgeRequirement.Scroll);
                var itemCell = Shuffled(candidates.Where(c => c.Role == CellRole.Item)).FirstOrDefault();
                if (itemCell == null
                    // Count like EnsureItemCells: Kraid's boss cell holds the energy tank.
                    && grid.Cells.Count(c => c.Role == CellRole.Item
                        || (c.Role == CellRole.Boss && c.ForcedScreenId == 0x1D)) < ItemLocationCap)
                {
                    itemCell = Shuffled(candidates.Where(c => c.Role == CellRole.Corridor)).FirstOrDefault();
                    if (itemCell != null)
                        itemCell.Role = CellRole.Item;
                }
                if (itemCell != null)
                    Force(itemCell, itemWalls);
            }

            PlaceTunnelChain(area, startRun);
        }

        if (placed == 0)
            throw new GenerationException("no hidden bomb wall placed");
    }

    /// <summary>
    /// Forces the area's vanilla morph-tunnel triple onto three consecutive corridor body
    /// cells (scroll seams on both sides of every cell, so runs need enough interior).
    /// Best effort: dense grids without a long enough corridor simply go without. The
    /// gates are symmetric (Morph/bombs both ways in the screen YAML), so either end can
    /// always back out the way it came; the logic graph carries the requirements.
    /// </summary>
    private void PlaceTunnelChain(Area area, Run startRun)
    {
        if (!TunnelChains.TryGetValue(area, out var screens))
            return;

        foreach (var run in Shuffled(grid.Runs.Where(r =>
            r.Axis == Scrolling.Horizontal && r != startRun
            && r.Cells.Count >= screens.Length + 2 && r.Cells[0].Area == area)))
        {
            foreach (int i in Shuffled(Enumerable.Range(1, run.Cells.Count - screens.Length - 1)))
            {
                var span = run.Cells.Skip(i).Take(screens.Length).ToList();
                if (!span.All(c => c.Role == CellRole.Corridor && !c.ForcedScreenId.HasValue
                    && c.Left == EdgeRequirement.Scroll && c.Right == EdgeRequirement.Scroll))
                    continue;

                for (int j = 0; j < screens.Length; j++)
                    span[j].ForcedScreenId = screens[j];
                landmarks[$"TunnelChain{area}"] = span[screens.Length / 2].Position;
                return;
            }
        }
    }

    // ---------------------------------------------------------------- validation

    private void ValidateWorld()
    {
        var fitErrors = grid.ValidateFittability(catalog);
        if (fitErrors.Count > 0)
            throw new GenerationException("fittability: " + fitErrors[0] + $" (+{fitErrors.Count - 1} more)");

        // Per-cell fittability is not enough: directional seam exclusions can leave a run
        // with no compatible screen SEQUENCE (e.g. a length-2 red corridor in Ridley, where
        // the only red-door piece 0x25 may only neighbor 0x26). Such layouts must retry
        // here instead of failing later in ScreenFitter, which has no retry.
        foreach (var run in grid.Runs)
            if (!ScreenFitter.CanFitRun(grid, catalog, run))
                throw new GenerationException(
                    $"no screen sequence fits {run.Area} {run.Axis} run of length {run.Cells.Count}");

        var reachProblems = AxisSolver.Validate(grid, start);
        if (reachProblems.Count > 0)
            throw new GenerationException("reachability: " + reachProblems[0] + $" (+{reachProblems.Count - 1} more)");

        int elevators = grid.Links.Count(l => l.Type == LinkType.Elevator);
        if (elevators != 4)
            throw new GenerationException($"expected 4 elevator links, found {elevators}");

        var requiredLandmarks = new List<string> { "Start", "StatuesGate", "TourianElevator", "MotherBrain", "EscapeShaft",
                                         "Kraid", "Ridley", "ConstructionZone", "VariaShaft",
                                         "HiddenWallBrinstar0", "HiddenWallKraid0",
                                         "BrinstarMapStation", "NorfairMapStation", "KraidMapStation", "RidleyMapStation" };
        // Elevator-arrival starts have no forced pedestal; corridor starts must.
        if (grid.Cell(start)?.Role == CellRole.Start)
            requiredLandmarks.Add("StartPedestal");
        foreach (var required in requiredLandmarks)
            if (!landmarks.ContainsKey(required))
                throw new GenerationException($"missing landmark {required}");

        if (grid.Cell(start)?.Area != StartArea)
            throw new GenerationException($"start cell is not in {StartArea}");
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
