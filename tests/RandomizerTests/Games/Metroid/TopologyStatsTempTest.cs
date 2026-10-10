namespace RandomizerTests.Games.Metroid;

using System.Text;
using System.Text.RegularExpressions;
using Randomizer.Games.Metroid;
using Randomizer.Games.Metroid.MapGen;
using static Randomizer.Games.Metroid.YamlReader;

/// <summary>
/// TEMPORARY measurement harness for the map-shape tuning pass. Not a regression test:
/// it always passes and dumps aggregate topology statistics to TestResults/m1stats-*.txt
/// plus SVG/ASCII maps for the first seeds, so changes compare against archived numbers.
/// Delete when the pass is done.
/// </summary>
[TestClass]
public sealed class TopologyStatsTempTest
{
    private const int Seeds = 200;

    private static readonly Lazy<ScreenCatalog> Catalog = new(() =>
    {
        var reader = new YamlReader(new Config());
        reader.LoadData();
        return ScreenCatalog.Build(reader.Data!.screens, reader.Data!.rooms, reader.Data!.transitions);
    });

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [DataRow(1.0, "standard")]
    [DataRow(1.4, "large")]
    [DataRow(1.0, "nightmare")]
    public void CollectStats(double sizeScale, string label)
    {
        var failureTally = new Dictionary<string, int>();
        var seedFailures = new List<int>();
        var attempts = new List<int>();
        var totals = new List<int>();
        var cycles = new List<int>();
        var pockets = new List<int>();
        var tunnelChains = new List<int>();
        var areaCells = new Dictionary<Area, List<int>>();
        var areaItems = new Dictionary<Area, List<int>>();
        var shaftCounts = new Dictionary<Area, List<int>>();
        var shaftLengths = new List<int>();
        var shaftDoorCoverage = new List<double>();
        var lairElevatorDistance = new Dictionary<string, List<int>> { ["Kraid"] = [], ["Ridley"] = [] };
        var lairOnEntrance = new Dictionary<string, int> { ["Kraid"] = 0, ["Ridley"] = 0 };

        for (int seed = 1; seed <= Seeds; seed++)
        {
            var generator = new TopologyGenerator(Catalog.Value)
            {
                SizeScale = sizeScale,
                Saturate = label == "nightmare",
                AttemptFailed = reason =>
                {
                    string key = Normalize(reason);
                    failureTally[key] = failureTally.GetValueOrDefault(key) + 1;
                }
            };

            GeneratedWorld world;
            try
            {
                world = generator.Generate(seed);
            }
            catch (InvalidOperationException)
            {
                seedFailures.Add(seed);
                continue;
            }

            if (seed <= 12 && label == "standard")
            {
                string svgDir = Path.Combine(FindRepoRoot(), "TestResults", "m1maps");
                Directory.CreateDirectory(svgDir);
                File.WriteAllText(Path.Combine(svgDir, $"seed-{seed}.svg"), MapRenderer.ToSvg(world));
                File.WriteAllText(Path.Combine(svgDir, $"seed-{seed}.txt"), MapRenderer.ToAscii(world));
            }

            var grid = world.Grid;
            attempts.Add(world.Attempts);
            totals.Add(grid.Cells.Count());
            cycles.Add(grid.Links.Count - grid.Runs.Count + 1);
            tunnelChains.Add(world.Landmarks.Keys.Count(k => k.StartsWith("TunnelChain")));
            pockets.Add(grid.Runs.Count(r =>
                r.Axis == Scrolling.Horizontal && r.Cells.Count == 1
                && r.Cells[0].ForcedScreenId == null
                && grid.Links.Count(l => l.Type == LinkType.Door
                    && (l.A == r.Cells[0].Position || l.B == r.Cells[0].Position)) == 1));

            foreach (var area in new[] { Area.Brinstar, Area.Norfair, Area.Kraid, Area.Ridley })
            {
                if (!areaCells.TryGetValue(area, out var list))
                    areaCells[area] = list = [];
                list.Add(grid.CellsOf(area).Count());

                if (!areaItems.TryGetValue(area, out var iList))
                    areaItems[area] = iList = [];
                iList.Add(grid.CellsOf(area).Count(c => c.Role == CellRole.Item
                    || (c.Role == CellRole.Boss && c.ForcedScreenId == 0x1D)));

                var shafts = grid.Runs.Where(r =>
                    r.Axis == Scrolling.Vertical && r.Cells.Count > 2 && r.Cells[0].Area == area).ToList();
                if (!shaftCounts.TryGetValue(area, out var sList))
                    shaftCounts[area] = sList = [];
                sList.Add(shafts.Count);
                foreach (var shaft in shafts)
                {
                    shaftLengths.Add(shaft.Cells.Count);
                    var interior = shaft.Cells.Where(c => c.Role != CellRole.Cap).ToList();
                    if (interior.Count > 0)
                        shaftDoorCoverage.Add(interior.Count(c =>
                            c.Left == EdgeRequirement.Door || c.Right == EdgeRequirement.Door
                            || c.Up == EdgeRequirement.Elevator || c.Down == EdgeRequirement.Elevator)
                            / (double)interior.Count);
                }
            }

            foreach (var (lair, elevator) in new[] { ("Kraid", "KraidElevator"), ("Ridley", "RidleyElevator") })
            {
                if (world.Landmarks.TryGetValue(lair, out var lairPos)
                    && world.Landmarks.TryGetValue(elevator, out var elevPos))
                    lairElevatorDistance[lair].Add(
                        Math.Max(Math.Abs(lairPos.X - elevPos.X), Math.Abs(lairPos.Y - elevPos.Y)));

                // Host shaft: first vertical run (>2 cells, skipping Kraid's 2-cell mini
                // shaft) scanning east along the lair's row. Entrance = forced 0x01 top.
                if (world.Landmarks.TryGetValue(lair, out var pos))
                    for (int x = pos.X + 1; x <= 31; x++)
                    {
                        var cell = grid.Cell(new Point(x, pos.Y));
                        if (cell == null || cell.Run.Axis != Scrolling.Vertical || cell.Run.Cells.Count <= 2)
                            continue;
                        if (cell.Run.Cells[0].ForcedScreenId == 0x01)
                            lairOnEntrance[lair]++;
                        break;
                    }
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine($"== M1 topology stats: scale {sizeScale} ({label}), seeds 1..{Seeds} ==");
        sb.AppendLine($"seed failures: {seedFailures.Count} ({string.Join(",", seedFailures)})");
        sb.AppendLine($"attempts:    {Summary(attempts)}");
        sb.AppendLine($"total cells: {Summary(totals)}");
        foreach (var (area, list) in areaCells)
            sb.AppendLine($"  {area,-8} cells: {Summary(list)}  shafts: {Summary(shaftCounts[area])}  " +
                $"items: {Summary(areaItems[area])} (per100cells {(list.Count == 0 ? 0 : areaItems[area].Zip(list, (i, c) => 100.0 * i / c).Average()):F1})");
        sb.AppendLine($"cycles/100 cells: {(totals.Count == 0 ? 0 : cycles.Zip(totals, (c, t) => 100.0 * c / t).Average()):F2}");
        sb.AppendLine($"shaft length: {Summary(shaftLengths)}");
        sb.AppendLine($"shaft interior door coverage: mean {shaftDoorCoverage.DefaultIfEmpty(0).Average():P0}");
        sb.AppendLine($"single-room door pockets: {Summary(pockets)}");
        sb.AppendLine($"tunnel chains (of 2): {Summary(tunnelChains)}");
        foreach (var (name, list) in lairElevatorDistance)
            sb.AppendLine($"{name} lair Chebyshev distance from elevator: {Summary(list)}  " +
                $"on entrance shaft: {lairOnEntrance[name]}/{list.Count}");
        sb.AppendLine("failed attempts by reason:");
        foreach (var (reason, count) in failureTally.OrderByDescending(kv => kv.Value))
            sb.AppendLine($"  {count,6}  {reason}");

        string output = sb.ToString();
        TestContext.WriteLine(output);
        string dir = FindRepoRoot();
        Directory.CreateDirectory(Path.Combine(dir, "TestResults"));
        File.WriteAllText(Path.Combine(dir, "TestResults", $"m1stats-{label}.txt"), output);
    }

    private static string Summary(List<int> values)
    {
        if (values.Count == 0)
            return "n/a";
        var sorted = values.OrderBy(v => v).ToList();
        return $"min {sorted[0]}  p50 {sorted[sorted.Count / 2]}  p90 {sorted[(int)(sorted.Count * 0.9)]}  max {sorted[^1]}  mean {values.Average():F1}";
    }

    [TestMethod]
    public void FillFailureRate()
    {
        var failures = new List<(int Seed, string Reason)>();
        for (int seed = 1; seed <= 50; seed++)
        {
            try
            {
                var randomizer = new Randomizer.Games.Metroid.GameRandomizer(
                    [new Randomizer.Games.WorldConfig { Metroid = new Config { MapShuffle = true } }],
                    new Randomizer.Graph.PRNG(seed));
                randomizer.Randomize();
                if (!randomizer.IsWinnable())
                    failures.Add((seed, "not winnable"));
            }
            catch (Exception e)
            {
                failures.Add((seed, e.Message.Split('\n')[0]));
            }
        }
        string output = $"fill failures: {failures.Count}/50\n"
            + string.Join("\n", failures.Select(f => $"  seed {f.Seed}: {f.Reason}"));
        TestContext.WriteLine(output);
        File.WriteAllText(Path.Combine(FindRepoRoot(), "TestResults", "m1fill-failures.txt"), output);
    }

    [TestMethod]
    public void DebugStalledPlaythroughSeed8()
    {
        string sourceDataRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "../../../../../src/Randomizer/Games/SuperMetroid/data"));
        if (!Directory.Exists(Path.Combine(sourceDataRoot, "maps")))
            Assert.Inconclusive("Requires the local SM map-rando Avro corpus.");

        var trace = new StringBuilder();
        string oldDataRoot = Randomizer.Games.SuperMetroid.Model.JsonReader.DataRoot;
        try
        {
            Randomizer.Games.SuperMetroid.Model.JsonReader.DataRoot = sourceDataRoot;
            Randomizer.Graph.PlaythroughGenerator.DebugTrace = line => trace.AppendLine(line);
            var config = new Randomizer.Games.WorldConfig
            {
                Combo = new Randomizer.Games.Combo.Config { InitialGame = "sm" },
                SuperMetroid = new Randomizer.Games.SuperMetroid.Config
                {
                    MapRandomizer = Randomizer.Games.SuperMetroid.MapRandomizerSetting.Standard,
                },
                Metroid = new Config { MapShuffle = true },
            };
            var randomizer = new Randomizer.Games.Combo.GameRandomizer(
                [config], new Randomizer.Graph.PRNG(8));
            randomizer.Randomize();
            trace.AppendLine($"winnable: {randomizer.IsWinnable()}");
        }
        finally
        {
            Randomizer.Graph.PlaythroughGenerator.DebugTrace = null;
            Randomizer.Games.SuperMetroid.Model.JsonReader.DataRoot = oldDataRoot;
        }
        File.WriteAllText(Path.Combine(FindRepoRoot(), "TestResults", "m1sm-stall-trace.txt"), trace.ToString());
    }

    [TestMethod]
    public void ScanForEmptyPlaythroughs()
    {
        string sourceDataRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "../../../../../src/Randomizer/Games/SuperMetroid/data"));
        if (!Directory.Exists(Path.Combine(sourceDataRoot, "maps")))
            Assert.Inconclusive("Requires the local SM map-rando Avro corpus.");

        var sb = new StringBuilder();
        string oldDataRoot = Randomizer.Games.SuperMetroid.Model.JsonReader.DataRoot;
        try
        {
            Randomizer.Games.SuperMetroid.Model.JsonReader.DataRoot = sourceDataRoot;
            for (int seed = 1; seed <= 12; seed++)
            {
                var config = new Randomizer.Games.WorldConfig
                {
                    Combo = new Randomizer.Games.Combo.Config { InitialGame = "sm" },
                    SuperMetroid = new Randomizer.Games.SuperMetroid.Config
                    {
                        MapRandomizer = Randomizer.Games.SuperMetroid.MapRandomizerSetting.Standard,
                    },
                    Metroid = new Config { MapShuffle = true },
                };
                try
                {
                    var randomizer = new Randomizer.Games.Combo.GameRandomizer(
                        [config], new Randomizer.Graph.PRNG(seed));
                    randomizer.Randomize();
                    bool winnable = randomizer.IsWinnable();
                    using var playthrough = System.Text.Json.JsonDocument.Parse(
                        randomizer.SpoilerLog!.Spoiler["playthrough"]["data"]);
                    var root = playthrough.RootElement;
                    bool complete = root.GetProperty("complete").GetBoolean();
                    int spheres = root.GetProperty("spheres").GetArrayLength();
                    sb.AppendLine($"seed {seed}: winnable={winnable} complete={complete} spheres={spheres}"
                        + (winnable && spheres == 0 ? "  <-- BUG" : ""));
                }
                catch (Exception e)
                {
                    sb.AppendLine($"seed {seed}: EXCEPTION {e.Message.Split('\n')[0]}");
                }
            }
        }
        finally
        {
            Randomizer.Games.SuperMetroid.Model.JsonReader.DataRoot = oldDataRoot;
        }
        TestContext.WriteLine(sb.ToString());
        File.WriteAllText(Path.Combine(FindRepoRoot(), "TestResults", "m1sm-playthrough-scan.txt"), sb.ToString());
    }

    private static string Normalize(string reason) => Regex.Replace(reason, "[0-9]+", "#");

    private static string FindRepoRoot()
    {
        for (var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); dir != null; dir = dir.Parent)
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
                return dir.FullName;
        return Directory.GetCurrentDirectory();
    }
}
