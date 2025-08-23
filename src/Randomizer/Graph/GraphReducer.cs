using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Randomizer.Graph;

/// <summary>
/// Pre-computes reachable subgraphs for each key to dramatically speed up door searches.
/// Instead of searching the entire graph every time, we search only the relevant vertices.
/// </summary>
public class GraphReducer
{
    private static readonly ILogger _logger = ClassLogger.Get();

    private readonly Graph _graph;
    private readonly Dictionary<IItem, VertexHashSet> _keySubgraphs = new();
    private readonly Dictionary<IItem, VertexHashSet> _keySearchStarts = new();
    private readonly Dictionary<string, VertexHashSet> _inventorySubgraphs = new();

    public GraphReducer(Graph graph)
    {
        _graph = graph;
        _logger.LogInformation("Building graph reducer for graph with {VertexCount} vertices", graph.GetVertices().Count());
        BuildKeySubgraphs();
    }

    /// <summary>
    /// Pre-compute which vertices are reachable with each key.
    /// This is the expensive part that happens once during initialization.
    /// </summary>
    private void BuildKeySubgraphs()
    {
        var sw = Stopwatch.StartNew();

        foreach (var (key, doors) in _graph.Doors)
        {
            var sw2 = Stopwatch.StartNew();

            // Create a minimal inventory with just this key
            var testInventory = new Inventory(key);

            // Find all vertices reachable with this key
            var reachableVertices = new VertexHashSet(_graph);
            var searchStarts = new VertexHashSet(_graph);

            // Start from all door vertices this key can unlock
            foreach (var door in doors)
            {
                if (door.Value != null)
                {
                    foreach (var (a, b) in door.Value)
                    {
                        searchStarts.Add(a);
                        searchStarts.Add(b);
                    }
                }
            }

            // Do a breadth-first search to find all reachable vertices
            var visited = new VertexHashSet(_graph);
            var queue = new Queue<Vertex>();

            foreach (var start in searchStarts)
            {
                if (!visited.Contains(start))
                {
                    queue.Enqueue(start);
                    visited.Add(start);
                    reachableVertices.Add(start);
                }
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                // Add all neighbors that don't require additional keys
                foreach (var edge in current.Edges)
                {
                    var neighbor = edge.To;
                    if (!visited.Contains(neighbor) && CanReachWithoutKeys(neighbor, testInventory))
                    {
                        visited.Add(neighbor);
                        reachableVertices.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            _keySubgraphs[key] = reachableVertices;
            _keySearchStarts[key] = searchStarts;

            _logger.LogDebug("Key {Key} subgraph built in {TimeElapsed} - {VertexCount} reachable vertices",
                key.Name, sw2.Elapsed, reachableVertices.Count);
        }

        _logger.LogInformation("Graph reducer built in {TimeElapsed} - {KeyCount} key subgraphs created",
            sw.Elapsed, _keySubgraphs.Count);
    }

    /// <summary>
    /// Check if a vertex can be reached without requiring additional keys.
    /// This is a simplified check - in practice, you might want more sophisticated logic.
    /// </summary>
    private bool CanReachWithoutKeys(Vertex vertex, Inventory inventory)
    {
        // If it's an item location, it's always reachable
        if (vertex.Item != null)
            return true;

        // If it's a key door, we need to check if we have the key
        if (vertex.Type == VertexType.Keydoor || vertex.Type == VertexType.BigKeydoor)
        {
            // For now, assume all key doors are reachable if we have any key
            // This is a simplification - in practice you'd want to check specific key requirements
            return inventory.All().Any();
        }

        // For other vertex types, assume they're reachable
        return true;
    }

    /// <summary>
    /// Get the pre-computed subgraph for a specific key.
    /// This is the fast path - no computation, just lookup!
    /// </summary>
    public VertexHashSet GetSubgraphForKey(IItem key)
    {
        return _keySubgraphs.TryGetValue(key, out var subgraph) ? subgraph : new VertexHashSet(_graph);
    }

    /// <summary>
    /// Get the pre-computed search starts for a specific key.
    /// </summary>
    public VertexHashSet GetSearchStartsForKey(IItem key)
    {
        return _keySearchStarts.TryGetValue(key, out var starts) ? starts : new VertexHashSet(_graph);
    }

    /// <summary>
    /// Get a combined subgraph for multiple keys.
    /// This is useful when we have multiple keys and want to find all reachable locations.
    /// </summary>
    public VertexHashSet GetCombinedSubgraph(IEnumerable<IItem> keys)
    {
        var combined = new VertexHashSet(_graph);
        foreach (var key in keys)
        {
            combined.UnionWith(GetSubgraphForKey(key));
        }
        return combined;
    }

    /// <summary>
    /// Get statistics about the graph reduction.
    /// </summary>
    public (int TotalVertices, int KeySubgraphs, Dictionary<string, int> VerticesPerKey) GetStats()
    {
        var verticesPerKey = _keySubgraphs.ToDictionary(
            kvp => kvp.Key.Name,
            kvp => kvp.Value.Count
        );

        return (_graph.GetVertices().Count(), _keySubgraphs.Count, verticesPerKey);
    }
}
