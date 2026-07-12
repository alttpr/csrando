namespace Randomizer.Games.Metroid.MapGen;

using System;
using System.Collections.Generic;
using System.Linq;
using static Randomizer.Games.Metroid.YamlReader;

/// <summary>
/// Phase 2: assigns a concrete vanilla screen to every cell of a generated topology.
///
/// Most cells can be fitted independently. Directional transition exclusions add a small
/// adjacency constraint, so fitting backtracks within a run only if an otherwise preferred
/// screen leaves no valid choice for the next cell. The fitter's job on top of correctness
/// is taste: prefer screens with fewer
/// extra openings (sealed openings look like dead walls in game) and avoid repeating the
/// same screen back to back inside a run.
/// </summary>
public static class ScreenFitter
{
    /// <summary>Assigns <see cref="AbstractCell.AssignedScreen"/> for every cell in the grid.</summary>
    public static void Fit(WorldGrid grid, ScreenCatalog catalog, int seed)
    {
        var rng = new Random(unchecked(seed * 48271 + 11));
        var errors = new List<string>();

        foreach (var run in grid.Runs)
        {
            var failedStates = new HashSet<(int Index, int? PreviousScreen)>();

            bool FitCell(int index, ScreenProfile? previous)
            {
                if (index == run.Cells.Count)
                    return true;
                if (failedStates.Contains((index, previous?.ScreenId)))
                    return false;

                var cell = run.Cells[index];
                var candidates = catalog.ForArea(cell.Area)
                    .Where(p => grid.FitsStrict(catalog, p, cell))
                    .Where(p => previous == null || catalog.ScreensCanConnect(previous,
                        run.Axis == Scrolling.Horizontal ? Direction.Right : Direction.Down, p))
                    .ToList();

                if (candidates.Count == 0)
                {
                    failedStates.Add((index, previous?.ScreenId));
                    return false;
                }

                int Score(ScreenProfile p) =>
                    ExtraOpenings(grid, cell, p) * 10
                    + GatedTraversal(cell, p) * 12
                    + (p.ScreenId == previous?.ScreenId ? 5 : 0);

                // Try the same random best candidate the old greedy fitter would choose first.
                // Remaining candidates only matter when a later transition forces backtracking.
                var ordered = candidates.OrderBy(Score).ToList();
                int best = Score(ordered[0]);
                int bestCount = ordered.TakeWhile(p => Score(p) == best).Count();
                int first = rng.Next(bestCount);
                (ordered[0], ordered[first]) = (ordered[first], ordered[0]);

                foreach (var candidate in ordered)
                {
                    cell.AssignedScreen = candidate;
                    if (FitCell(index + 1, candidate))
                        return true;
                }

                cell.AssignedScreen = null;
                failedStates.Add((index, previous?.ScreenId));
                return false;
            }

            if (!FitCell(0, null))
                errors.Add($"no compatible screen sequence fits {run.Area} {run.Axis} run {run.Id}");
        }

        if (errors.Count > 0)
            throw new InvalidOperationException("Screen fitting failed:\n" + string.Join("\n", errors));
    }

    /// <summary>
    /// Counts the cell's committed edge pairs the screen only connects with items or in
    /// one direction — fall pieces and freeze-to-climb shafts. They are legal (the logic
    /// graph carries the requirements), but a shaft built out of them plays as one-way or
    /// heavily item-locked, so they only get picked when nothing free fits.
    /// </summary>
    private static int GatedTraversal(AbstractCell cell, ScreenProfile profile)
    {
        var committed = new List<Direction>(4);
        foreach (var dir in Directions.All)
            if (cell.Edge(dir) != EdgeRequirement.Wall)
                committed.Add(dir);

        int count = 0;
        for (int i = 0; i < committed.Count; i++)
            for (int j = i + 1; j < committed.Count; j++)
                if (!profile.EdgesFreelyConnected(committed[i], committed[j]))
                    count++;
        return count;
    }

    /// <summary>
    /// Counts connectors the screen exposes on edges the cell does not require. These face
    /// empty cells or caps (anything else was rejected by FitsStrict), so they are sealed
    /// by the engine — legal, but fewer is better. Doors weigh double: a sealed door frame
    /// is more visually misleading than a sealed scroll opening. Tunnels are free since
    /// they are ability-gated screen internals.
    /// </summary>
    private static int ExtraOpenings(WorldGrid grid, AbstractCell cell, ScreenProfile profile)
    {
        int count = 0;
        foreach (var dir in Directions.All)
        {
            if (cell.Edge(dir) != EdgeRequirement.Wall)
                continue;

            count += profile.Connector(dir).Type switch
            {
                ConnectorType.Door => 2,
                ConnectorType.Scroll or ConnectorType.Elevator => 1,
                _ => 0
            };
        }
        return count;
    }
}
