namespace RandomizerTests.Games.Metroid;

using Randomizer.Games.Metroid;
using Randomizer.Games.Metroid.MapGen;
using static Randomizer.Games.Metroid.YamlReader;

/// <summary>
/// A door opening that faces an empty cell is not sealed by the engine (unlike a scroll
/// opening) and scrolls Samus out of bounds. Every fitted screen's doors must land on a
/// real door edge linked to a neighbor, never on a Wall edge.
/// </summary>
[TestClass]
public sealed class DoorSealRegressionTest
{
    private static readonly Lazy<ScreenCatalog> Catalog = new(() =>
    {
        var reader = new YamlReader(new Config());
        reader.LoadData();
        return ScreenCatalog.Build(reader.Data!.screens, reader.Data!.rooms, reader.Data!.transitions);
    });

    [TestMethod]
    public void Fit_NoDoorOpeningLandsOnAWallEdge()
    {
        var catalog = Catalog.Value;
        var offenders = new List<string>();

        for (int seed = 1; seed <= 200; seed++)
        {
            GeneratedWorld world;
            try { world = new TopologyGenerator(catalog).Generate(seed); }
            catch (GenerationException) { continue; } // failure rate measured separately

            ScreenFitter.Fit(world.Grid, catalog, seed);

            foreach (var cell in world.Grid.Cells)
            {
                var screen = cell.AssignedScreen!;
                foreach (var dir in new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right })
                {
                    if (cell.Edge(dir) != EdgeRequirement.Wall)
                        continue;
                    if (screen.Connector(dir).Type != ConnectorType.Door)
                        continue;

                    var target = cell.Position.Step(dir);
                    var facing = world.Grid.Cell(target);
                    string kind = facing == null
                        ? (world.Grid.ReservedEmpty.Contains(target) ? "reserved-empty" : "empty")
                        : facing.Role == CellRole.Cap ? "cap" : $"occupied({facing.Role})";

                    offenders.Add($"seed {seed}: {cell.Area} 0x{screen.ScreenId:X2} at {cell.Position} " +
                        $"door facing {dir} -> {kind}" +
                        (cell.ForcedScreenId.HasValue ? $" [forced 0x{cell.ForcedScreenId:X2}]" : ""));
                }
            }
        }

        Assert.AreEqual(0, offenders.Count,
            "door openings on Wall edges (out-of-bounds softlock):\n" + string.Join("\n", offenders.Take(20)));
    }
}
