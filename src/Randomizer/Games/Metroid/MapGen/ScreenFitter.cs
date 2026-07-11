namespace Randomizer.Games.Metroid.MapGen;

using System;
using System.Collections.Generic;
using System.Linq;
using static Randomizer.Games.Metroid.YamlReader;

/// <summary>
/// Phase 2: assigns a concrete vanilla screen to every cell of a generated topology.
///
/// Because <see cref="WorldGrid.FitsStrict"/> only depends on the abstract grid (edges,
/// roles, door colors, neighbor run kinds and occupancy), cells can be fitted independently
/// — no backtracking. Topology validation already proved every cell has at least one
/// candidate. The fitter's job on top of correctness is taste: prefer screens with fewer
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
            int? previousId = null;

            foreach (var cell in run.Cells)
            {
                var candidates = catalog.ForArea(cell.Area)
                    .Where(p => grid.FitsStrict(catalog, p, cell))
                    .ToList();

                if (candidates.Count == 0)
                {
                    errors.Add($"no screen fits {cell}");
                    continue;
                }

                int Score(ScreenProfile p) =>
                    ExtraOpenings(grid, cell, p) * 10
                    + GatedTraversal(cell, p) * 12
                    + (p.ScreenId == previousId ? 5 : 0);

                int best = candidates.Min(Score);
                var pool = candidates.Where(p => Score(p) == best).ToList();
                cell.AssignedScreen = pool[rng.Next(pool.Count)];
                previousId = cell.AssignedScreen.ScreenId;
            }
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
