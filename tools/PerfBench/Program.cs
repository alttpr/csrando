using System.Diagnostics;

namespace PerfBench;

internal static class Program
{
    public static int Main(string[] args)
    {
        // Options
        int count = 20;
        int bulk = 1;
        string repoRoot = GetRepoRoot();
        string randomizerDll = Path.Combine(repoRoot, "src", "Randomizer", "bin", "Release", "net9.0", "Randomizer.dll");
        string settings = Path.Combine(repoRoot, "tmp_settings.json");
        bool buildFirst = true;

        // Parse minimal args: --count, --bulk, --settings, --no-build
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--count": count = int.Parse(args[++i]); break;
                case "--bulk": bulk = int.Parse(args[++i]); break;
                case "--settings": settings = args[++i]; break;
                case "--no-build": buildFirst = false; break;
            }
        }

        if (buildFirst)
        {
            var sln = Path.Combine(repoRoot, "ALttPR.sln");
            var b = Run("dotnet", $"build {Quote(sln)} -c Release --nologo", repoRoot, captureStdout: false);
            if (b != 0) return b;
        }

        if (!File.Exists(settings))
        {
            // Create a minimal default ALttP settings file
            var minimal = "[ { \"Language\": \"en\", \"Alttp\": { } } ]";
            File.WriteAllText(settings, minimal);
        }

        if (!File.Exists(randomizerDll))
        {
            Console.Error.WriteLine($"Randomizer not found: {randomizerDll}");
            return 2;
        }

        var durations = new List<double>(count);
        long maxRss = 0;
        var swAll = Stopwatch.StartNew();
        for (int i = 0; i < count; i++)
        {
            // Spread seeds deterministically
            int seed = 123456 + i;
            var sw = Stopwatch.StartNew();
            // Run the Randomizer for a single seed; prefer full process run to include app startup cost
            // Use --log-level Warning to limit console overhead
            var code = Run("dotnet", $"{Quote(randomizerDll)} randomize --settings {Quote(settings)} --seed {seed} --log-level Warning" + (bulk > 1 ? $" --bulk {bulk}" : ""), repoRoot, captureStdout: false, rssKb: out var rss);
            sw.Stop();
            durations.Add(sw.Elapsed.TotalMilliseconds);
            if (rss > maxRss) maxRss = rss;
            if (code != 0)
            {
                Console.Error.WriteLine($"Run {i + 1}/{count} exited with {code}");
            }
        }
        swAll.Stop();

        durations.Sort();
        var mean = durations.Average();
        double StdDev(IReadOnlyList<double> xs, double m)
        {
            double s = 0; foreach (var x in xs) { var d = x - m; s += d * d; } return Math.Sqrt(s / xs.Count);
        }
        var std = StdDev(durations, mean);
        double P(IReadOnlyList<double> xs, double p)
        {
            if (xs.Count == 0) return 0; var idx = (int)Math.Floor((xs.Count - 1) * p); return xs[idx];
        }
        var p50 = P(durations, 0.5);
        var p90 = P(durations, 0.9);
        var p95 = P(durations, 0.95);

        Console.WriteLine("Benchmark results (ms):");
        Console.WriteLine($"  runs={count}, bulk={bulk}");
        Console.WriteLine($"  mean={mean:F1}, p50={p50:F1}, p90={p90:F1}, p95={p95:F1}, std={std:F1}");
        Console.WriteLine($"  maxrss(KB)={maxRss}, wall_s={swAll.Elapsed.TotalSeconds:F2}");

        return 0;
    }

    static int Run(string file, string args, string workdir, bool captureStdout)
        => Run(file, args, workdir, captureStdout, out _);

    static int Run(string file, string args, string workdir, bool captureStdout, out long rssKb)
    {
        var psi = new ProcessStartInfo(file, args)
        {
            WorkingDirectory = workdir,
            RedirectStandardOutput = captureStdout,
            RedirectStandardError = captureStdout,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        using var p = Process.Start(psi)!;
        p.WaitForExit();
        rssKb = TryReadMaxRssKb(p);
        return p.ExitCode;
    }

    static long TryReadMaxRssKb(Process p)
    {
        try
        {
            // Best-effort: on Linux, parse /proc/<pid>/status for VmHWM after exit (may be gone), fallback 0
            var path = $"/proc/{p.Id}/status";
            if (File.Exists(path))
            {
                foreach (var line in File.ReadLines(path))
                {
                    if (line.StartsWith("VmHWM:"))
                    {
                        var parts = line.Split((char[])null!, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && long.TryParse(parts[1], out var kb)) return kb;
                    }
                }
            }
        }
        catch { }
        return 0;
    }

    static string Quote(string s) => s.Contains(' ') ? $"\"{s}\"" : s;
    static string GetRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        // Walk up until we find ALttPR.sln
        var d = new DirectoryInfo(dir);
        while (d != null)
        {
            if (File.Exists(Path.Combine(d.FullName, "ALttPR.sln"))) return d.FullName;
            d = d.Parent;
        }
        return Directory.GetCurrentDirectory();
    }
}
