namespace Randomizer.Games.SuperMetroid;

using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<(Vertex Target, ReverseCapabilityKey Inventory),
        Lazy<ReverseBacktrackSearch>> _reverseSearches = [];
    private readonly Dictionary<World, ReverseCapabilityProfile> _capabilityProfiles = [];
    private readonly object _lock = new();

    public static BacktrackCache ForGraph(Graph graph) =>
        ByGraph.GetValue(graph, _ => new BacktrackCache());

    public BacktrackRegion GetRegion(Graph graph, Vertex target)
    {
        lock (_lock)
        {
            if (_regions.TryGetValue(target, out var region))
                return region;

            region = BacktrackRegion.Build(graph, target);
            _regions[target] = region;
            return region;
        }
    }

    public ReverseBacktrackSearch GetReverseSearch(
        Vertex target, BacktrackRegion region, Inventory inventory)
    {
        var world = (World)target.World;
        ReverseCapabilityProfile profile;
        lock (_lock)
        {
            if (!_capabilityProfiles.TryGetValue(world, out profile!))
            {
                profile = ReverseCapabilityProfile.Build(world);
                _capabilityProfiles[world] = profile;
            }
        }
        var key = (target, profile.CreateKey(inventory));
        var lazySearch = _reverseSearches.GetOrAdd(key, _ =>
            new Lazy<ReverseBacktrackSearch>(
                () => ReverseBacktrackSearch.Build(region, target, inventory),
                LazyThreadSafetyMode.ExecutionAndPublication));
        return lazySearch.Value;
    }
}
