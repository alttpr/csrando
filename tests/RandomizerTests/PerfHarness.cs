namespace RandomizerTests;

using System.Diagnostics;
using System.Text.Json;
using Randomizer.Games;
using Randomizer.Graph;

/// <summary>
/// Performance measurement and behavior-diff harness. Generates full seeds over a fixed seed
/// range, recording per-seed timings and canonicalized spoiler logs so that optimization work
/// can be compared run-over-run (timings) and verified placement-identical (spoiler diff).
///
/// Excluded from the default test run (Slow); run via:
///   dotnet test --settings tests/RandomizerTests/Perf.runsettings
///
/// Environment variables:
///   CSRANDO_PERF_LABEL  output subdirectory for this run, e.g. "baseline" (default "dev")
///   CSRANDO_PERF_SEEDS  number of seeds to generate per config (default 100)
///
/// Output: &lt;repo&gt;/TestResults/perf/&lt;label&gt;/&lt;config&gt;/seed-NNNN.json plus summary.json.
/// Compare two runs with a recursive directory diff of the config subdirectories.
/// </summary>
[TestClass]
public class PerfHarness
{
    private const string DefaultLabel = "dev";
    private const int DefaultSeedCount = 50;

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [TestCategory(TestCategories.Slow)]
    [TestCategory("Perf")]
    public void OpenDefault() => Run("open-default", () => new WorldConfig
    {
        Game = RandomizerTarget.Alttpr,
        Alttp = new(),
    });

    [TestMethod]
    [TestCategory(TestCategories.Slow)]
    [TestCategory("Perf")]
    public void OpenWildKeys() => Run("open-wildkeys", () => new WorldConfig
    {
        Game = RandomizerTarget.Alttpr,
        Alttp = new()
        {
            RegionWildKeys = true,
            RegionWildBigKeys = true,
            RegionWildMaps = true,
            RegionWildCompasses = true,
        },
    });

    private void Run(string configName, Func<WorldConfig> makeConfig)
    {
        string label = Environment.GetEnvironmentVariable("CSRANDO_PERF_LABEL") ?? DefaultLabel;
        int seedCount = int.TryParse(Environment.GetEnvironmentVariable("CSRANDO_PERF_SEEDS"), out int n)
            ? n : DefaultSeedCount;

        string outDir = Path.Combine(FindRepoRoot(), "TestResults", "perf", label, configName);
        Directory.CreateDirectory(outDir);

        var results = new List<SeedResult>(seedCount);

        // The randomizer logs one console line per placement; silence it so measurements
        // reflect search/fill work rather than log formatting.
        var realOut = Console.Out;
        Console.SetOut(TextWriter.Null);
        try
        {
            for (int seed = 1; seed <= seedCount; seed++)
            {
                // Config instances hold per-seed randomized state (SelectRandomValues), so
                // build a fresh one per seed, mirroring the CLI's bulk loop.
                var configs = new[] { makeConfig() };

                long allocBefore = GC.GetAllocatedBytesForCurrentThread();
                var sw = Stopwatch.StartNew();
                var randomizer = Randomizer.Graph.RandomizerFactory.Create(configs, seed);
                double graphMs = sw.Elapsed.TotalMilliseconds;

                sw.Restart();
                randomizer.Randomize();
                double fillMs = sw.Elapsed.TotalMilliseconds;
                long allocBytes = GC.GetAllocatedBytesForCurrentThread() - allocBefore;

                bool winnable = randomizer.IsWinnable();

                results.Add(new SeedResult(seed, graphMs, fillMs, allocBytes, winnable));

                string spoilerJson = CanonicalSpoilerJson(randomizer.SpoilerLog!.Spoiler);
                File.WriteAllText(Path.Combine(outDir, $"seed-{seed:D4}.json"), spoilerJson);
            }
        }
        finally
        {
            Console.SetOut(realOut);
        }

        var summary = new
        {
            label,
            config = configName,
            seedCount,
            unwinnableSeeds = results.Where(r => !r.Winnable).Select(r => r.Seed).ToArray(),
            graphMs = Stats(results.Select(r => r.GraphMs)),
            fillMs = Stats(results.Select(r => r.FillMs)),
            allocatedMB = Stats(results.Select(r => r.AllocBytes / (1024.0 * 1024.0))),
            perSeed = results,
        };
        string summaryJson = JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(outDir, "summary.json"), summaryJson);

        // Unwinnable seeds are normal (callers retry with a new seed); they are recorded in the
        // summary so behavior changes show up in the run-over-run diff rather than failing here.
        TestContext.WriteLine($"[{label}/{configName}] seeds={seedCount} " +
            $"fill total={summary.fillMs.Total:F0}ms median={summary.fillMs.Median:F1}ms p95={summary.fillMs.P95:F1}ms | " +
            $"graph total={summary.graphMs.Total:F0}ms | alloc median={summary.allocatedMB.Median:F1}MB | " +
            $"unwinnable=[{string.Join(", ", summary.unwinnableSeeds)}]");
    }

    private sealed record SeedResult(int Seed, double GraphMs, double FillMs, long AllocBytes, bool Winnable);

    private sealed record StatLine(double Total, double Median, double P95, double Max);

    private static StatLine Stats(IEnumerable<double> values)
    {
        double[] sorted = values.Order().ToArray();
        return new StatLine(
            Total: sorted.Sum(),
            Median: sorted[sorted.Length / 2],
            P95: sorted[(int)(sorted.Length * 0.95) is var i && i < sorted.Length ? i : sorted.Length - 1],
            Max: sorted[^1]);
    }

    /// <summary>
    /// Serialize the spoiler with sorted keys so the diff is insensitive to dictionary
    /// insertion order (which incidental code changes may reorder) but sensitive to any
    /// actual placement or playthrough change.
    /// </summary>
    private static string CanonicalSpoilerJson(Dictionary<string, Dictionary<string, string>> spoiler)
    {
        var sorted = new SortedDictionary<string, SortedDictionary<string, string>>(StringComparer.Ordinal);
        foreach (var (section, entries) in spoiler)
            sorted[section] = new SortedDictionary<string, string>(entries, StringComparer.Ordinal);
        return JsonSerializer.Serialize(sorted, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, ".git")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Repository root not found from " + AppContext.BaseDirectory);
    }
}
