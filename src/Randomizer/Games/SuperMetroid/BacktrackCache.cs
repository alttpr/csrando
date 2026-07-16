namespace Randomizer.Games.SuperMetroid;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Randomizer.Graph;

/// <summary>
/// Reverse backtracking data shared by every searcher operating on one seed's
/// graph. Concrete searches are keyed by their exact promoted inventory.
/// </summary>
internal sealed class BacktrackCache
{
    private static readonly ConditionalWeakTable<Graph, BacktrackCache> ByGraph = new();

    private readonly Dictionary<Vertex, BacktrackRegion> _regions = [];
    private readonly ConcurrentDictionary<(Vertex Target, string Inventory),
        Lazy<ReverseBacktrackSearch>> _reverseSearches = [];
    private readonly object _lock = new();

    public static BacktrackCache ForGraph(Graph graph) =>
        ByGraph.GetValue(graph, _ => new BacktrackCache());

    public BacktrackRegion GetRegion(
        Graph graph, Vertex target, BacktrackMetrics metrics)
    {
        lock (_lock)
        {
            if (_regions.TryGetValue(target, out var region))
                return region;

            long started = Stopwatch.GetTimestamp();
            region = BacktrackRegion.Build(graph, target);
            _regions[target] = region;
            metrics.RecordRegionBuild(
                Stopwatch.GetTimestamp() - started, region.Vertices.Count);
            return region;
        }
    }

    public ReverseBacktrackSearch GetReverseSearch(
        Vertex target, BacktrackRegion region, Inventory inventory,
        BacktrackMetrics metrics)
    {
        var key = (target, InventoryKey(inventory, target.World));
        var lazySearch = _reverseSearches.GetOrAdd(key, _ =>
            new Lazy<ReverseBacktrackSearch>(() =>
            {
                long started = Stopwatch.GetTimestamp();
                var built = ReverseBacktrackSearch.Build(
                    region, target, inventory);
                metrics.RecordReverseSearchBuild(
                    Stopwatch.GetTimestamp() - started, built);
                return built;
            }, LazyThreadSafetyMode.ExecutionAndPublication));
        var search = lazySearch.Value;
        if (!search.UsesInventory(inventory))
        {
            throw new InvalidOperationException(
                "Reverse-search inventory cache key collision.");
        }
        return search;
    }

    internal static string InventoryKey(
        Inventory inventory, IWorld world) => string.Join(
        ';', inventory.All()
            .Where(pair => ReferenceEquals(pair.Key.World, world))
            .OrderBy(pair => pair.Key.Name, StringComparer.Ordinal)
            .Select(pair => $"{pair.Key.Name}:{pair.Value}"));
}
