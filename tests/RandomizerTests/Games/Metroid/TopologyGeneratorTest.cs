namespace RandomizerTests.Games.Metroid;

using Randomizer.Games.Metroid;
using Randomizer.Games.Metroid.MapGen;
using static Randomizer.Games.Metroid.YamlReader;

[TestClass]
public sealed class TopologyGeneratorTest
{
    private static readonly Lazy<ScreenCatalog> Catalog = new(() =>
    {
        var reader = new YamlReader(new Config());
        reader.LoadData();
        return ScreenCatalog.Build(reader.Data!.screens, reader.Data!.rooms);
    });

    private static GeneratedWorld Generate(int seed) =>
        new TopologyGenerator(Catalog.Value).Generate(seed);

    /// <summary>Repo-root TestResults/m1maps folder for rendered maps.</summary>
    private static string OutputDir()
    {
        var dir = new DirectoryInfo(YamlReader.DataRoot);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "ALttPR.sln")))
            dir = dir.Parent;

        var output = Path.Combine(dir?.FullName ?? Path.GetTempPath(), "TestResults", "m1maps");
        Directory.CreateDirectory(output);
        return output;
    }

    [TestMethod]
    public void Generate_ManySeeds_Succeeds()
    {
        for (int seed = 1; seed <= 50; seed++)
        {
            var world = Generate(seed);
            Assert.IsNotNull(world.Grid, $"seed {seed}");
        }
    }

    [TestMethod]
    public void Generate_PlacesAPortalAnchor()
    {
        for (int seed = 1; seed <= 20; seed++)
        {
            var world = Generate(seed);
            Assert.IsTrue(world.Landmarks.TryGetValue("Portal0", out var portalPos),
                $"seed {seed}: no portal anchor landmark");

            var portal = world.Grid.Cell(portalPos)!;
            Assert.AreEqual(CellRole.Portal, portal.Role, $"seed {seed}");
            Assert.AreEqual(0x1F, portal.ForcedScreenId, $"seed {seed}: portal must be the vanilla portal room");
            Assert.AreEqual(Area.Brinstar, portal.Area, $"seed {seed}");

            // The vanilla combo arrangement: door cell directly west, sealed scroll east.
            var doorCell = world.Grid.Cell(portalPos.Step(Direction.Left))!;
            Assert.AreEqual(EdgeRequirement.Door, doorCell.Right, $"seed {seed}: door cell must face the portal");
            Assert.IsTrue(world.Grid.HasLink(doorCell.Position, portalPos), $"seed {seed}: portal door link missing");
            Assert.IsTrue(world.Grid.ReservedEmpty.Contains(portalPos.Step(Direction.Right)),
                $"seed {seed}: cell east of the portal must stay empty for the sealed scroll opening");
        }
    }

    [TestMethod]
    public void Generate_IsDeterministicPerSeed()
    {
        var a = Generate(123);
        var b = Generate(123);
        Assert.AreEqual(MapRenderer.ToAscii(a), MapRenderer.ToAscii(b));
    }

    [TestMethod]
    public void Generate_AllCellsReachableAndFittable()
    {
        for (int seed = 1; seed <= 20; seed++)
        {
            var world = Generate(seed);
            var fitErrors = world.Grid.ValidateFittability(Catalog.Value);
            Assert.AreEqual(0, fitErrors.Count, $"seed {seed}: {string.Join("; ", fitErrors.Take(3))}");

            var reachErrors = AxisSolver.Validate(world.Grid, world.Start);
            Assert.AreEqual(0, reachErrors.Count, $"seed {seed}: {string.Join("; ", reachErrors.Take(3))}");
        }
    }

    [TestMethod]
    public void Generate_LandmarksAndElevators()
    {
        for (int seed = 1; seed <= 20; seed++)
        {
            var world = Generate(seed);
            foreach (var landmark in new[] { "Start", "StatuesGate", "TourianElevator", "MotherBrain", "EscapeShaft",
                                             "Kraid", "Ridley", "KraidElevator", "NorfairElevator", "RidleyElevator" })
                Assert.IsTrue(world.Landmarks.ContainsKey(landmark), $"seed {seed}: missing {landmark}");

            Assert.AreEqual(4, world.Grid.Links.Count(l => l.Type == LinkType.Elevator), $"seed {seed}");

            // No occupied coordinate may belong to two areas (grid placement enforces it, but
            // assert the per-area sums add up to the total).
            int total = world.Grid.Cells.Count();
            int sum = ScreenCatalog.PlayableAreas.Sum(a => world.Grid.CellsOf(a).Count());
            Assert.AreEqual(total, sum, $"seed {seed}");
        }
    }

    [TestMethod]
    public void Generate_TourianIsGatedByStatues()
    {
        for (int seed = 1; seed <= 20; seed++)
        {
            var world = Generate(seed);
            var gate = world.Landmarks["StatuesGate"];
            var grid = world.Grid;

            // Removing the gate's door links must make every Tourian cell unreachable.
            var links = grid.Links.Where(l => l.A == gate || l.B == gate).ToList();
            Assert.AreEqual(2, links.Count, $"seed {seed}: gate should have exactly two door links");
            foreach (var link in links)
                grid.Links.Remove(link);

            var reached = AxisSolver.Solve(grid, world.Start);
            foreach (var cell in grid.CellsOf(Area.Tourian))
                Assert.IsFalse(reached.Contains(cell.Position), $"seed {seed}: Tourian {cell.Position} reachable without gate");

            foreach (var link in links)
                grid.Links.Add(link);
        }
    }

    [TestMethod]
    public void Generate_EscapeShaftConnectsToMotherBrain()
    {
        for (int seed = 1; seed <= 20; seed++)
        {
            var world = Generate(seed);
            var mb = world.Landmarks["MotherBrain"];
            var escapeBottom = new Point(mb.X - 1, mb.Y);

            var bottomCell = world.Grid.Cell(escapeBottom);
            Assert.IsNotNull(bottomCell, $"seed {seed}: no escape cell west of Mother Brain");
            Assert.AreEqual(CellRole.Escape, bottomCell.Role, $"seed {seed}");
            Assert.AreEqual(0x0F, bottomCell.ForcedScreenId, $"seed {seed}");
            Assert.IsTrue(world.Grid.HasLink(escapeBottom, mb), $"seed {seed}: escape door link missing");

            // The shaft climbs up to the escape elevator entrance 0x0E.
            var top = world.Landmarks["EscapeShaft"];
            var topCell = world.Grid.Cell(top);
            Assert.IsNotNull(topCell, $"seed {seed}");
            Assert.AreEqual(0x0E, topCell.ForcedScreenId, $"seed {seed}");
            Assert.IsTrue(world.Grid.ReservedEmpty.Contains(new Point(top.X, top.Y - 1)), $"seed {seed}");
        }
    }

    [TestMethod]
    public void Generate_EveryAreaHasItems()
    {
        for (int seed = 1; seed <= 10; seed++)
        {
            var world = Generate(seed);
            foreach (var area in new[] { Area.Brinstar, Area.Norfair, Area.Kraid, Area.Ridley })
                Assert.IsTrue(world.Grid.CellsOf(area).Any(c => c.Role == CellRole.Item),
                    $"seed {seed}: no item cells in {area}");
        }
    }

    [TestMethod]
    public void Generate_SizeScaleKnob()
    {
        for (int seed = 1; seed <= 5; seed++)
        {
            var small = new TopologyGenerator(Catalog.Value) { SizeScale = 0.7 }.Generate(seed);
            var large = new TopologyGenerator(Catalog.Value) { SizeScale = 1.2 }.Generate(seed);

            int smallTotal = small.Grid.Cells.Count();
            int largeTotal = large.Grid.Cells.Count();
            Assert.IsTrue(largeTotal > smallTotal,
                $"seed {seed}: SizeScale 1.2 ({largeTotal}) not larger than 0.7 ({smallTotal})");
        }
    }

    [TestMethod]
    public void Generate_NightmareSaturatesTheGrid()
    {
        for (int seed = 1; seed <= 6; seed++)
        {
            var large = new TopologyGenerator(Catalog.Value) { SizeScale = 1.2 }.Generate(seed);
            var nightmare = new TopologyGenerator(Catalog.Value) { Saturate = true }.Generate(seed);

            int largeTotal = large.Grid.Cells.Count();
            int nightmareTotal = nightmare.Grid.Cells.Count();
            Console.WriteLine($"seed {seed}: nightmare {nightmareTotal}, large {largeTotal}");
            // The generator itself retries layouts below the saturation floor, so the
            // guarantee is absolute; Large rolls can occasionally land in the same range.
            Assert.IsTrue(nightmareTotal >= TopologyGenerator.SaturationMinimum,
                $"seed {seed}: Nightmare only reached {nightmareTotal} cells");
        }
    }

    [TestMethod]
    public void Fit_ElevatorScreensOnlyOnElevatorCells()
    {
        // An elevator screen in a regular shaft leaves a real elevator hole in the floor
        // with no platform — the engine does not seal elevator openings like it seals
        // scroll/door openings against empty cells.
        for (int seed = 1; seed <= 30; seed++)
        {
            var world = Generate(seed);
            ScreenFitter.Fit(world.Grid, Catalog.Value, seed);

            foreach (var cell in world.Grid.Cells)
            {
                if (cell.ForcedScreenId.HasValue)
                    continue;

                var screen = cell.AssignedScreen!;
                foreach (var dir in new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right })
                {
                    if (screen.Connector(dir).Type == ConnectorType.Elevator)
                        Assert.AreEqual(EdgeRequirement.Elevator, cell.Edge(dir),
                            $"seed {seed}: {cell.Area} screen 0x{screen.ScreenId:X2} at {cell.Position} " +
                            $"({cell.Role}) has an elevator opening {dir} on a {cell.Edge(dir)} edge");
                }
            }
        }
    }

    [TestMethod]
    public void Generate_ItemLocationsNeverExceedThePool()
    {
        // The pool is fixed at 36 items; surplus locations would stay unfilled and the
        // ROM tables would render them as free Bombs (the placeholder id). Large maps
        // organically roll many item cells during growth, so they are the risky case.
        foreach (var scale in new[] { 1.0, 1.2 })
        {
            for (int seed = 1; seed <= 10; seed++)
            {
                var world = new TopologyGenerator(Catalog.Value) { SizeScale = scale }.Generate(seed);
                int items = world.Grid.Cells.Count(c => c.Role == CellRole.Item
                    || (c.Role == CellRole.Boss && c.ForcedScreenId == 0x1D));
                Assert.IsTrue(items is >= 31 and <= 36,
                    $"scale {scale} seed {seed}: {items} item locations for a 36-item pool");
            }
        }
    }

    [TestMethod]
    public void Generate_ChozoItemRoomsExist()
    {
        for (int seed = 1; seed <= 20; seed++)
        {
            var world = Generate(seed);

            foreach (var (area, chozoScreen) in new[] { (Area.Brinstar, 0x0A), (Area.Norfair, 0x04) })
            {
                var chozos = world.Grid.CellsOf(area)
                    .Where(c => c.ForcedScreenId == chozoScreen && c.Role == CellRole.Item).ToList();
                Assert.IsTrue(chozos.Count >= 1, $"seed {seed}: no chozo item room in {area}");

                foreach (var chozo in chozos)
                {
                    // Red door on the chozo's east side into the pre-item corridor.
                    Assert.AreEqual(EdgeRequirement.Door, chozo.Right, $"seed {seed}");
                    Assert.AreEqual(DoorType.Red, chozo.RightDoorColor, $"seed {seed}");
                    Assert.IsTrue(world.Grid.HasLink(chozo.Position, chozo.Position.Step(Direction.Right)), $"seed {seed}");

                    // Solid wall cap west of the statue.
                    var cap = world.Grid.Cell(chozo.Position.Step(Direction.Left));
                    Assert.IsNotNull(cap, $"seed {seed}");
                    Assert.AreEqual(CellRole.Cap, cap.Role, $"seed {seed}");
                }
            }
        }
    }

    [TestMethod]
    public void Generate_PlacesConstructionZoneShaft()
    {
        for (int seed = 1; seed <= 20; seed++)
        {
            var world = Generate(seed);
            Assert.IsTrue(world.Landmarks.TryGetValue("ConstructionZone", out var pos),
                $"seed {seed}: no Construction Zone landmark");

            // Vanilla 0x18 on top: doors on both sides, bombable floor into the shaft below.
            var top = world.Grid.Cell(pos)!;
            Assert.AreEqual(0x18, top.ForcedScreenId, $"seed {seed}");
            Assert.AreEqual(EdgeRequirement.Door, top.Left, $"seed {seed}");
            Assert.AreEqual(EdgeRequirement.Door, top.Right, $"seed {seed}");
            Assert.AreEqual(EdgeRequirement.Scroll, top.Down, $"seed {seed}");
            Assert.IsTrue(world.Grid.HasLink(pos, pos.Step(Direction.Left)), $"seed {seed}");
            Assert.IsTrue(world.Grid.HasLink(pos, pos.Step(Direction.Right)), $"seed {seed}");

            // The shaft descends to a solid cap, all in one run.
            Assert.AreEqual(top.Run.Cells[0], top, $"seed {seed}: 0x18 must be the run's top cell");
            Assert.IsTrue(top.Run.Cells.Count >= 4, $"seed {seed}: shaft too short ({top.Run.Cells.Count})");
            Assert.AreEqual(CellRole.Cap, top.Run.Cells[^1].Role, $"seed {seed}: shaft must end in a cap");
        }
    }

    [TestMethod]
    public void Generate_PlacesVariaTower()
    {
        for (int seed = 1; seed <= 20; seed++)
        {
            var world = Generate(seed);
            Assert.IsTrue(world.Landmarks.TryGetValue("VariaShaft", out var pos),
                $"seed {seed}: no Varia tower landmark");

            // Tower bottom 0x1E: breakable ceiling, entry door east, sealed door west.
            var bottom = world.Grid.Cell(pos)!;
            Assert.AreEqual(0x1E, bottom.ForcedScreenId, $"seed {seed}");
            Assert.AreEqual(EdgeRequirement.Scroll, bottom.Up, $"seed {seed}");
            Assert.IsTrue(world.Grid.HasLink(pos, pos.Step(Direction.Right)), $"seed {seed}");
            Assert.IsTrue(world.Grid.ReservedEmpty.Contains(pos.Step(Direction.Left)),
                $"seed {seed}: cell behind 0x1E's sealed door must stay empty");

            // 0x2E above with the chozo pre-item corridor on its left, capped above.
            var middle = world.Grid.Cell(pos.Step(Direction.Up))!;
            Assert.AreEqual(0x2E, middle.ForcedScreenId, $"seed {seed}");
            Assert.IsTrue(world.Grid.HasLink(middle.Position, middle.Position.Step(Direction.Left)), $"seed {seed}");
            Assert.AreEqual(CellRole.Cap, world.Grid.Cell(middle.Position.Step(Direction.Up))!.Role, $"seed {seed}");

            var preEast = world.Grid.Cell(middle.Position.Step(Direction.Left))!;
            Assert.AreEqual(0x28, preEast.ForcedScreenId, $"seed {seed}: pre-item corridor east end");
        }
    }

    [TestMethod]
    public void Generate_PlacesHiddenBombWalls()
    {
        for (int seed = 1; seed <= 20; seed++)
        {
            var world = Generate(seed);
            var walls = world.Landmarks.Where(kv => kv.Key.StartsWith("HiddenWall")).ToList();
            Assert.IsTrue(walls.Count >= 1, $"seed {seed}: no hidden bomb walls");

            var startRun = world.Grid.Cell(world.Start)!.Run;
            foreach (var (name, pos) in walls.Select(kv => (kv.Key, kv.Value)))
            {
                var cell = world.Grid.Cell(pos)!;
                Assert.IsTrue(cell.ForcedScreenId.HasValue, $"seed {seed}: {name} not forced");
                Assert.AreEqual(EdgeRequirement.Scroll, cell.Left, $"seed {seed}: {name}");
                Assert.AreEqual(EdgeRequirement.Scroll, cell.Right, $"seed {seed}: {name}");
                Assert.AreNotEqual(startRun, cell.Run, $"seed {seed}: {name} in the start corridor");

                // Both directions must be bomb-gated (never free) so a player cannot fall
                // into a pocket without the bombs to leave it.
                var profile = Catalog.Value.Find(cell.Area, cell.ForcedScreenId!.Value)!;
                Assert.IsFalse(profile.EdgesFreelyConnected(Direction.Left, Direction.Right),
                    $"seed {seed}: {name} screen 0x{profile.ScreenId:X2} has a free direction");
                Assert.IsTrue(profile.EdgesConnected(Direction.Left, Direction.Right),
                    $"seed {seed}: {name} screen 0x{profile.ScreenId:X2} not passable with full inventory");
            }
        }
    }

    [TestMethod]
    public void Render_DumpSampleMaps()
    {
        var dir = OutputDir();
        for (int seed = 1; seed <= 8; seed++)
        {
            var world = Generate(seed);
            File.WriteAllText(Path.Combine(dir, $"seed-{seed}.svg"), MapRenderer.ToSvg(world));
            File.WriteAllText(Path.Combine(dir, $"seed-{seed}.txt"),
                MapRenderer.ToAscii(world) + "\n" + string.Join("\n", world.Diagnostics));
        }

        var sample = Generate(1);
        Console.WriteLine(MapRenderer.ToAscii(sample));
        Console.WriteLine(string.Join("\n", sample.Diagnostics));
        Console.WriteLine($"maps written to {dir}");
    }
}
