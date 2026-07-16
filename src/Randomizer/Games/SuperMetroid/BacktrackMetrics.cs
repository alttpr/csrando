namespace Randomizer.Games.SuperMetroid;

using System.Diagnostics;
using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
    private bool _enabled;

    public BacktrackBenchmarkMode Mode { get; private set; } = BacktrackBenchmarkMode.Reverse;
    public bool Enabled => _enabled;

    public static BacktrackMetrics ForGraph(Graph graph) =>
        ByGraph.GetValue(graph, _ => new BacktrackMetrics());

    public static BacktrackMetricsSnapshot SnapshotFor(Graph graph) =>
        ForGraph(graph).Snapshot();

    public static void Configure(
        Graph graph, BacktrackBenchmarkMode mode, bool enabled = true)
    {
        var metrics = ForGraph(graph);
        metrics.Mode = mode;
        metrics._enabled = enabled;
    }

    public void RecordCheck()
    {
        if (_enabled) _checks++;
    }
    public void RecordStructuralReject()
    {
        if (_enabled) _structuralRejects++;
    }
    public void RecordReverseProof()
    {
        if (_enabled) _reverseProofs++;
    }
    public void RecordReverseReject()
    {
        if (_enabled) _reverseRejects++;
    }
    public void RecordForwardWork(
        long dequeuedStates, long requirementEvaluations,
        long enqueueAttempts, long enqueues, long structurallyPrunedEnqueues)
    {
        _forwardPasses++;
        _forwardDequeuedStates += dequeuedStates;
        _forwardRequirementEvaluations += requirementEvaluations;
        _forwardEnqueueAttempts += enqueueAttempts;
        _forwardEnqueues += enqueues;
        _structurallyPrunedEnqueues += structurallyPrunedEnqueues;
    }

    public void RecordFallback(bool success, long elapsedTicks, int states)
    {
        if (!_enabled)
            return;
        _fallbackSearches++;
        if (success)
            _fallbackSuccesses++;
        else
            _fallbackFailures++;
        _fallbackElapsedTicks += elapsedTicks;
        _fallbackStates += states;
    }

    public void RecordRegionBuild(long elapsedTicks, int vertices)
    {
        _regionBuilds++;
        _regionBuildElapsedTicks += elapsedTicks;
        _regionVertices += vertices;
    }

    public void RecordReverseSearchBuild(
        long elapsedTicks, ReverseBacktrackSearch search)
    {
        _reverseSearchBuilds++;
        _reverseSearchBuildElapsedTicks += elapsedTicks;
        _reverseFrontierVertices += search.FrontierVertexCount;
        _reverseFrontierEntries += search.FrontierEntryCount;
    }

    private BacktrackMetricsSnapshot Snapshot() => new(
        _checks,
        _structuralRejects,
        _reverseProofs,
        _reverseRejects,
        _fallbackSearches,
        _fallbackSuccesses,
        _fallbackFailures,
        _fallbackElapsedTicks,
        _fallbackStates,
        _regionBuilds,
        _regionBuildElapsedTicks,
        _regionVertices,
        _reverseSearchBuilds,
        _reverseSearchBuildElapsedTicks,
        _reverseFrontierVertices,
        _reverseFrontierEntries,
        _structurallyPrunedEnqueues,
        _forwardPasses,
        _forwardDequeuedStates,
        _forwardRequirementEvaluations,
        _forwardEnqueueAttempts,
        _forwardEnqueues);
}

internal static class BacktrackMetricsCsv
{
    private const int SchemaVersion = 9;
    private static readonly object WriteLock = new();
    private const string Header =
        "schema_version,timestamp_utc,label,mode,seed,generator,generation_ms," +
        "graph_construction_ms,assumed_fill_ms,spoiler_ms,validation_ms,total_ms," +
        "allocated_bytes,gen0_collections,gen1_collections,gen2_collections," +
        "output_hash,status,failure," +
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
        TimeSpan generationElapsed, TimeSpan graphConstructionElapsed,
        TimeSpan assumedFillElapsed, TimeSpan spoilerElapsed,
        TimeSpan validationElapsed, TimeSpan totalElapsed,
        long allocatedBytes, int gen0Collections, int gen1Collections,
        int gen2Collections, string outputHash, string status, string failure,
        BacktrackMetricsSnapshot metrics)
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
                graphConstructionElapsed.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture),
                assumedFillElapsed.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture),
                spoilerElapsed.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture),
                validationElapsed.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture),
                totalElapsed.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture),
                allocatedBytes.ToString(CultureInfo.InvariantCulture),
                gen0Collections.ToString(CultureInfo.InvariantCulture),
                gen1Collections.ToString(CultureInfo.InvariantCulture),
                gen2Collections.ToString(CultureInfo.InvariantCulture),
                Escape(outputHash),
                Escape(status),
                Escape(failure),
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

    internal static void Append(
        FileInfo file, string label, BacktrackBenchmarkMode mode,
        int seed, string generator, TimeSpan generationElapsed,
        BacktrackMetricsSnapshot metrics) => Append(
        file, label, mode, seed, generator,
        generationElapsed, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero,
        TimeSpan.Zero, generationElapsed, 0, 0, 0, 0,
        "", "success", "", metrics);

    private static string Escape(string value) =>
        value.IndexOfAny([',', '"', '\r', '\n']) < 0
            ? value
            : $"\"{value.Replace("\"", "\"\"")}\"";
}

internal static class SeedOutputHash
{
    public static string Compute(Randomizer.Graph.GameRandomizer randomizer)
    {
        var text = new StringBuilder();
        foreach (var vertex in randomizer.Graph.GetVertices()
                     .Where(vertex => vertex.Item != null)
                     .OrderBy(vertex => vertex.World.GameId, StringComparer.Ordinal)
                     .ThenBy(vertex => vertex.World.Id)
                     .ThenBy(vertex => vertex.Name, StringComparer.Ordinal))
        {
            var item = vertex.Item!;
            text.Append(vertex.World.GameId).Append(':')
                .Append(vertex.World.Id).Append(':')
                .Append(vertex.Name).Append(" => ")
                .Append(item.World.GameId).Append(':')
                .Append(item.World.Id).Append(':')
                .Append(item.Name).Append('\n');
        }

        if (randomizer.SpoilerLog?.Spoiler.TryGetValue(
                "playthrough", out var playthrough) == true
            && playthrough.TryGetValue("data", out var json))
        {
            text.Append("--playthrough--\n");
            using var document = JsonDocument.Parse(json);
            var buffer = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(buffer))
                WriteCanonical(writer, document.RootElement);
            text.Append(Encoding.UTF8.GetString(buffer.WrittenSpan));
        }

        return Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(text.ToString()))).ToLowerInvariant();
    }

    private static void WriteCanonical(Utf8JsonWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject()
                             .OrderBy(property => property.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Name);
                    WriteCanonical(writer, property.Value);
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                    WriteCanonical(writer, item);
                writer.WriteEndArray();
                break;
            default:
                element.WriteTo(writer);
                break;
        }
    }
}
