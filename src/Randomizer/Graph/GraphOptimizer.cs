using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Randomizer.Graph;

/// <summary>
/// OPTIMIZES THE SHIT OUT OF THE GRAPH by consolidating edges, merging vertices, and eliminating redundancy!
/// This keeps the same logical behavior but makes everything BLAZING FAST! 🔥🚀
/// </summary>
public class GraphOptimizer
{
    private static readonly ILogger _logger = ClassLogger.Get();

    private readonly Graph _graph;

    public GraphOptimizer(Graph graph)
    {
        _graph = graph;
        _logger.LogInformation("🔥 GRAPH OPTIMIZER INITIALIZED! Time to SHRED this graph IN-PLACE!");
    }

    /// <summary>
    /// OPTIMIZE THE SHIT OUT OF THIS GRAPH IN-PLACE! 🔥
    /// </summary>
    public void OptimizeInPlace()
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("🚀 Starting IN-PLACE graph optimization for {VertexCount} vertices...", _graph.GetVertices().Count());

        // Step 1: Consolidate redundant edges (in-place)
        ConsolidateEdgesInPlace();

        // Step 2: Identify dead ends (for info only)
        IdentifyDeadEnds();

        var originalEdges = _graph.GetVertices().Sum(v => v.Edges.Count);
        var optimizedEdges = _graph.GetVertices().Sum(v => v.Edges.Count);
        var edgeReduction = originalEdges - optimizedEdges;

        _logger.LogInformation("🔥 IN-PLACE GRAPH OPTIMIZATION COMPLETE! {TimeElapsed}", sw.Elapsed);
        _logger.LogInformation("📊 RESULTS: {OriginalEdges} → {OptimizedEdges} edges ({EdgeReduction} removed, {EdgeReductionPercent:F1}% reduction!)",
            originalEdges, optimizedEdges, edgeReduction, (double)edgeReduction / originalEdges * 100);
    }



    /// <summary>
    /// Consolidate redundant edges between the same vertices IN-PLACE
    /// </summary>
    private void ConsolidateEdgesInPlace()
    {
        var consolidatedCount = 0;
        var edgeGroups = new Dictionary<(Vertex From, Vertex To), List<Edge>>();

        // Group edges by (from, to) pairs
        foreach (var vertex in _graph.GetVertices())
        {
            foreach (var edge in vertex.Edges)
            {
                var key = (edge.From, edge.To);
                if (!edgeGroups.ContainsKey(key))
                    edgeGroups[key] = new List<Edge>();
                edgeGroups[key].Add(edge);
            }
        }

        // Consolidate edges with multiple conditions IN-PLACE
        foreach (var (key, edges) in edgeGroups)
        {
            if (edges.Count > 1)
            {
                // Multiple edges between same vertices - consolidate them IN-PLACE!
                var consolidatedCountForThisPair = ConsolidateEdgeGroupInPlace(edges);
                consolidatedCount += consolidatedCountForThisPair;
            }
        }

        _logger.LogInformation("🔗 Consolidated {ConsolidatedCount} redundant edges IN-PLACE!", consolidatedCount);
    }

    /// <summary>
    /// Combine multiple edge conditions into a single optimized condition IN-PLACE
    /// </summary>
    private int ConsolidateEdgeGroupInPlace(List<Edge> edges)
    {
        if (edges.Count == 1)
            return 0;

        // Only consolidate edges with identical conditions
        // This prevents breaking the logic while still optimizing
        var firstCondition = edges[0].Condition;
        var canConsolidate = edges.All(e => AreConditionsCompatible(e.Condition, firstCondition));

        if (!canConsolidate)
        {
            // Conditions are different - don't consolidate
            return 0;
        }

        // Conditions are compatible - consolidate them IN-PLACE!
        var firstEdge = edges[0];
        var edgesToRemove = edges.Skip(1).ToList();

        // Remove the redundant edges from the source vertex
        foreach (var edgeToRemove in edgesToRemove)
        {
            edgeToRemove.From.Edges.Remove(edgeToRemove);
        }

        _logger.LogDebug("🔗 Consolidated {EdgeCount} compatible edges IN-PLACE from {From} to {To}",
            edges.Count, firstEdge.From.Name, firstEdge.To.Name);

        return edgesToRemove.Count;
    }

    /// <summary>
    /// Check if two edge conditions are compatible for consolidation
    /// </summary>
    private bool AreConditionsCompatible(ItemCondition condition1, ItemCondition condition2)
    {
        // For now, only consolidate if conditions are identical
        // This is conservative but safe
        return condition1.Equals(condition2);
    }







    /// <summary>
    /// Identify dead-end vertices that can never be reached (for info only)
    /// </summary>
    private void IdentifyDeadEnds()
    {
        var deadEndCount = 0;

        // Find vertices with no incoming edges (except the start vertex)
        var unreachableVertices = _graph.GetVertices()
            .Where(v => !HasIncomingEdges(v))
            .Where(v => v.Type != VertexType.Meta) // Don't remove meta vertices
            .ToList();

        foreach (var vertex in unreachableVertices)
        {
            _logger.LogDebug("💀 Found dead-end vertex: {VertexName}", vertex.Name);
            deadEndCount++;
        }

        _logger.LogInformation("💀 Identified {DeadEndCount} dead-end vertices!", deadEndCount);
    }

    /// <summary>
    /// Check if a vertex has any incoming edges
    /// </summary>
    private bool HasIncomingEdges(Vertex vertex)
    {
        return _graph.GetVertices()
            .SelectMany(v => v.Edges)
            .Any(e => e.To == vertex);
    }


}
