namespace Randomizer.Games.SuperMetroid;

using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using Randomizer.Graph;

internal enum BacktrackBenchmarkMode
{
    Legacy,
    Reverse,
    Validate,
}

internal sealed record BacktrackMetricsSnapshot(
    long Checks,
    long StructuralRejects,
    long ReverseProofs,
    long ReverseRejects,
    long FallbackSearches,
    long FallbackSuccesses,
    long FallbackFailures,
    long FallbackElapsedTicks,
    long FallbackStates,
    long RegionBuilds,
    long RegionBuildElapsedTicks,
    long RegionVertices,
    long ReverseSearchBuilds,
    long ReverseSearchBuildElapsedTicks,
    long ReverseFrontierVertices,
    long ReverseFrontierEntries,
    long StructurallyPrunedEnqueues,
    long ForwardPasses,
    long ForwardDequeuedStates,
    long ForwardRequirementEvaluations,
    long ForwardEnqueueAttempts,
    long ForwardEnqueues)
{
    public double FallbackMilliseconds =>
        FallbackElapsedTicks * 1000d / Stopwatch.Frequency;
    public double RegionBuildMilliseconds =>
        RegionBuildElapsedTicks * 1000d / Stopwatch.Frequency;
    public double ReverseSearchBuildMilliseconds =>
        ReverseSearchBuildElapsedTicks * 1000d / Stopwatch.Frequency;
}

/// <summary>
/// Per-graph counters used to measure how much work reverse backtracking avoids.
/// A Graph corresponds to one generated seed, so the weak association naturally
/// separates concurrent generations and does not retain completed seeds.
/// </summary>
internal sealed class BacktrackMetrics
{
    private static readonly ConditionalWeakTable<Graph, BacktrackMetrics> ByGraph = new();

    private long _checks;
    private long _structuralRejects;
    private long _reverseProofs;
    private long _reverseRejects;
    private long _fallbackSearches;
    private long _fallbackSuccesses;
    private long _fallbackFailures;
    private long _fallbackElapsedTicks;
    private long _fallbackStates;
    private long _regionBuilds;
    private long _regionBuildElapsedTicks;
    private long _regionVertices;
    private long _reverseSearchBuilds;
    private long _reverseSearchBuildElapsedTicks;
    private long _reverseFrontierVertices;
    private long _reverseFrontierEntries;
    private long _structurallyPrunedEnqueues;
    private long _forwardPasses;
    private long _forwardDequeuedStates;
    private long _forwardRequirementEvaluations;
    private long _forwardEnqueueAttempts;
    private long _forwardEnqueues;

    public BacktrackBenchmarkMode Mode { get; private set; } = BacktrackBenchmarkMode.Reverse;

    public static BacktrackMetrics ForGraph(Graph graph) =>
        ByGraph.GetValue(graph, _ => new BacktrackMetrics());

    public static BacktrackMetricsSnapshot SnapshotFor(Graph graph) =>
        ForGraph(graph).Snapshot();

    public static void Configure(Graph graph, BacktrackBenchmarkMode mode) =>
        ForGraph(graph).Mode = mode;

    public void RecordCheck() => Interlocked.Increment(ref _checks);
    public void RecordStructuralReject() => Interlocked.Increment(ref _structuralRejects);
    public void RecordReverseProof() => Interlocked.Increment(ref _reverseProofs);
    public void RecordReverseReject() => Interlocked.Increment(ref _reverseRejects);
    public void RecordStructurallyPrunedEnqueue() =>
        Interlocked.Increment(ref _structurallyPrunedEnqueues);
    public void RecordForwardPass() => Interlocked.Increment(ref _forwardPasses);
    public void RecordForwardDequeuedState() =>
        Interlocked.Increment(ref _forwardDequeuedStates);
    public void RecordForwardRequirementEvaluation() =>
        Interlocked.Increment(ref _forwardRequirementEvaluations);
    public void RecordForwardEnqueueAttempt() =>
        Interlocked.Increment(ref _forwardEnqueueAttempts);
    public void RecordForwardEnqueue() => Interlocked.Increment(ref _forwardEnqueues);

    public void RecordFallback(bool success, long elapsedTicks, int states)
    {
        Interlocked.Increment(ref _fallbackSearches);
        if (success)
            Interlocked.Increment(ref _fallbackSuccesses);
        else
            Interlocked.Increment(ref _fallbackFailures);
        Interlocked.Add(ref _fallbackElapsedTicks, elapsedTicks);
        Interlocked.Add(ref _fallbackStates, states);
    }

    public void RecordRegionBuild(long elapsedTicks, int vertices)
    {
        Interlocked.Increment(ref _regionBuilds);
        Interlocked.Add(ref _regionBuildElapsedTicks, elapsedTicks);
        Interlocked.Add(ref _regionVertices, vertices);
    }

    public void RecordReverseSearchBuild(
        long elapsedTicks, ReverseBacktrackSearch search)
    {
        Interlocked.Increment(ref _reverseSearchBuilds);
        Interlocked.Add(ref _reverseSearchBuildElapsedTicks, elapsedTicks);
        Interlocked.Add(ref _reverseFrontierVertices, search.FrontierVertexCount);
        Interlocked.Add(ref _reverseFrontierEntries, search.FrontierEntryCount);
    }

    private BacktrackMetricsSnapshot Snapshot() => new(
        Interlocked.Read(ref _checks),
        Interlocked.Read(ref _structuralRejects),
        Interlocked.Read(ref _reverseProofs),
        Interlocked.Read(ref _reverseRejects),
        Interlocked.Read(ref _fallbackSearches),
        Interlocked.Read(ref _fallbackSuccesses),
        Interlocked.Read(ref _fallbackFailures),
        Interlocked.Read(ref _fallbackElapsedTicks),
        Interlocked.Read(ref _fallbackStates),
        Interlocked.Read(ref _regionBuilds),
        Interlocked.Read(ref _regionBuildElapsedTicks),
        Interlocked.Read(ref _regionVertices),
        Interlocked.Read(ref _reverseSearchBuilds),
        Interlocked.Read(ref _reverseSearchBuildElapsedTicks),
        Interlocked.Read(ref _reverseFrontierVertices),
        Interlocked.Read(ref _reverseFrontierEntries),
        Interlocked.Read(ref _structurallyPrunedEnqueues),
        Interlocked.Read(ref _forwardPasses),
        Interlocked.Read(ref _forwardDequeuedStates),
        Interlocked.Read(ref _forwardRequirementEvaluations),
        Interlocked.Read(ref _forwardEnqueueAttempts),
        Interlocked.Read(ref _forwardEnqueues));
}

internal static class BacktrackMetricsCsv
{
    private const int SchemaVersion = 8;
    private static readonly object WriteLock = new();
    private const string Header =
        "schema_version,timestamp_utc,label,mode,seed,generator,generation_ms," +
        "checks,structural_rejects,reverse_proofs,reverse_rejects," +
        "resolved_without_fallback_pct," +
        "fallback_searches,fallback_successes,fallback_failures,fallback_ms," +
        "fallback_states,avg_fallback_ms,avg_fallback_states," +
        "region_builds,region_build_ms,region_vertices," +
        "reverse_search_builds,reverse_search_build_ms," +
        "reverse_frontier_vertices,reverse_frontier_entries," +
        "structurally_pruned_enqueues,forward_passes," +
        "forward_dequeued_states,forward_requirement_evaluations," +
        "forward_enqueue_attempts,forward_enqueues";

    public static void Append(
        FileInfo file, string label, BacktrackBenchmarkMode mode,
        int seed, string generator,
        TimeSpan generationElapsed, BacktrackMetricsSnapshot metrics)
    {
        file.Directory?.Create();
        lock (WriteLock)
        {
            file.Refresh();
            bool writeHeader = !file.Exists || file.Length == 0;
            using var writer = file.AppendText();
            if (writeHeader)
                writer.WriteLine(Header);

            double resolvedPercent = metrics.Checks == 0
                ? 0
                : (metrics.StructuralRejects + metrics.ReverseProofs
                    + metrics.ReverseRejects) * 100d / metrics.Checks;
            double averageFallbackMs = metrics.FallbackSearches == 0
                ? 0
                : metrics.FallbackMilliseconds / metrics.FallbackSearches;
            double averageFallbackStates = metrics.FallbackSearches == 0
                ? 0
                : (double)metrics.FallbackStates / metrics.FallbackSearches;

            string[] values =
            [
                SchemaVersion.ToString(CultureInfo.InvariantCulture),
                DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                Escape(label),
                mode.ToString(),
                seed.ToString(CultureInfo.InvariantCulture),
                Escape(generator),
                generationElapsed.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture),
                metrics.Checks.ToString(CultureInfo.InvariantCulture),
                metrics.StructuralRejects.ToString(CultureInfo.InvariantCulture),
                metrics.ReverseProofs.ToString(CultureInfo.InvariantCulture),
                metrics.ReverseRejects.ToString(CultureInfo.InvariantCulture),
                resolvedPercent.ToString("F3", CultureInfo.InvariantCulture),
                metrics.FallbackSearches.ToString(CultureInfo.InvariantCulture),
                metrics.FallbackSuccesses.ToString(CultureInfo.InvariantCulture),
                metrics.FallbackFailures.ToString(CultureInfo.InvariantCulture),
                metrics.FallbackMilliseconds.ToString("F3", CultureInfo.InvariantCulture),
                metrics.FallbackStates.ToString(CultureInfo.InvariantCulture),
                averageFallbackMs.ToString("F3", CultureInfo.InvariantCulture),
                averageFallbackStates.ToString("F3", CultureInfo.InvariantCulture),
                metrics.RegionBuilds.ToString(CultureInfo.InvariantCulture),
                metrics.RegionBuildMilliseconds.ToString("F3", CultureInfo.InvariantCulture),
                metrics.RegionVertices.ToString(CultureInfo.InvariantCulture),
                metrics.ReverseSearchBuilds.ToString(CultureInfo.InvariantCulture),
                metrics.ReverseSearchBuildMilliseconds.ToString("F3", CultureInfo.InvariantCulture),
                metrics.ReverseFrontierVertices.ToString(CultureInfo.InvariantCulture),
                metrics.ReverseFrontierEntries.ToString(CultureInfo.InvariantCulture),
                metrics.StructurallyPrunedEnqueues.ToString(CultureInfo.InvariantCulture),
                metrics.ForwardPasses.ToString(CultureInfo.InvariantCulture),
                metrics.ForwardDequeuedStates.ToString(CultureInfo.InvariantCulture),
                metrics.ForwardRequirementEvaluations.ToString(CultureInfo.InvariantCulture),
                metrics.ForwardEnqueueAttempts.ToString(CultureInfo.InvariantCulture),
                metrics.ForwardEnqueues.ToString(CultureInfo.InvariantCulture),
            ];
            writer.WriteLine(string.Join(',', values));
        }
    }

    private static string Escape(string value) =>
        value.IndexOfAny([',', '"', '\r', '\n']) < 0
            ? value
            : $"\"{value.Replace("\"", "\"\"")}\"";
}
