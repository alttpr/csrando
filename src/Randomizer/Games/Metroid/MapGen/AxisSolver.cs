namespace Randomizer.Games.Metroid.MapGen;

using System;
using System.Collections.Generic;
using static Randomizer.Games.Metroid.YamlReader;

/// <summary>
/// Physical reachability over the abstract grid.
///
/// Engine model (verified against the M1 disassembly, Bank07):
/// - The engine has one active scroll axis (ScrollDir $49). Scrolling between the cells of
///   a run requires the active axis to match the run's axis.
/// - Door tiles come in two kinds baked into each screen: $A0 (the axis flips relative to
///   the room you came from) and $A1 (the axis is forced horizontal; used by item rooms,
///   lairs and other single-screen rooms).
/// - Elevator rides leave the axis vertical going down; the stop handler toggles it when
///   arriving from below, so an up ride leaves it horizontal for the door that follows.
/// - Single-cell rooms never scroll, so any arrival axis works inside them.
///
/// The door tile kind is not in the YAML, so the generator only pairs doors with the kind
/// of neighbor they had in vanilla (see <see cref="ScreenCatalog.DoorAccepts"/>), which
/// guarantees the arrival axis always matches the destination run. Under that invariant,
/// physical reachability reduces to a search over runs and committed links; this solver
/// performs that search and respects one-way scroll cells.
/// </summary>
public static class AxisSolver
{
    /// <summary>
    /// Returns every cell reachable from <paramref name="start"/>. When a
    /// <paramref name="catalog"/> is supplied, directional <c>impossible</c> transitions between
    /// the cells' assigned screens are treated as one-way blocks (the crossing is skipped in the
    /// blocked direction only), matching what the logic graph builds.
    /// </summary>
    public static HashSet<Point> Solve(WorldGrid grid, Point start, ScreenCatalog? catalog = null)
    {
        if (grid.Cell(start) == null)
            throw new ArgumentException($"start {start} is empty");

        var reached = new HashSet<Point>();
        var queue = new Queue<Point>();

        void Enqueue(Point p)
        {
            if (reached.Add(p))
                queue.Enqueue(p);
        }

        // A one-way impossible transition blocks scrolling from cell into neighbor in `dir`.
        // The screens must be assigned (post-fit) to know their ids; if not, nothing is blocked.
        bool Blocked(AbstractCell cell, AbstractCell neighbor, Direction dir) =>
            catalog != null
            && cell.AssignedScreen is { } from && neighbor.AssignedScreen is { } to
            && catalog.TransitionBlocked(cell.Area, from.ScreenId, dir, to.ScreenId);

        Enqueue(start);

        while (queue.Count > 0)
        {
            var pos = queue.Dequeue();
            var cell = grid.Cell(pos)!;

            foreach (var dir in Directions.All)
            {
                var neighborPos = pos.Step(dir);
                var neighbor = grid.Cell(neighborPos);
                if (neighbor == null)
                    continue;
                if (Blocked(cell, neighbor, dir))
                    continue;

                switch (cell.Edge(dir))
                {
                    // Scroll or in-run elevator chaining: same run only.
                    case EdgeRequirement.Scroll when neighbor.Run == cell.Run:
                    case EdgeRequirement.Elevator when neighbor.Run == cell.Run:
                        Enqueue(neighborPos);
                        break;

                    // Doors and elevator pairs must be committed links with matching back edges.
                    case EdgeRequirement.Door when neighbor.Edge(Directions.Opposite(dir)) == EdgeRequirement.Door
                                                   && grid.HasLink(pos, neighborPos):
                    case EdgeRequirement.Elevator when neighbor.Edge(Directions.Opposite(dir)) == EdgeRequirement.Elevator
                                                       && grid.HasLink(pos, neighborPos):
                        Enqueue(neighborPos);
                        break;
                }
            }
        }

        return reached;
    }

    /// <summary>
    /// Validates that every non-cap cell is physically reachable from the start cell.
    /// Returns human-readable problems; empty means the layout passes. Pass the
    /// <paramref name="catalog"/> after screen fitting to also enforce one-way impossible
    /// transitions (a region reachable only through a blocked crossing counts as stranded).
    /// </summary>
    public static List<string> Validate(WorldGrid grid, Point start, ScreenCatalog? catalog = null)
    {
        var problems = new List<string>();
        var reached = Solve(grid, start, catalog);

        foreach (var cell in grid.Cells)
        {
            if (cell.Role == CellRole.Cap || cell.Role == CellRole.Escape)
                continue;
            if (!reached.Contains(cell.Position))
                problems.Add($"Unreachable: {cell}");
        }

        return problems;
    }
}
