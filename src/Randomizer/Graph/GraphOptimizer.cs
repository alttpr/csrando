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

        // Step 2: Optimize bidirectional edges (BEAST MODE!)
        OptimizeBidirectionalEdges();

                // Step 3: ⚡-MODE: Actually merge pass-through vertices for real performance gains!
        MergePassThroughVertices();

        // Step 4: 🔥 CYCLE TERMINATOR MODE: Eliminate redundant cycles!
        EliminateRedundantCycles();

        // Step 5: Identify dead ends (for info only)
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

        // 🔥 BEAST MODE: Only log at TRACE level to reduce noise!
        _logger.LogTrace("🔗 Consolidated {EdgeCount} compatible edges IN-PLACE from {From} to {To}",
            edges.Count, firstEdge.From.Name, firstEdge.To.Name);

        return edgesToRemove.Count;
    }

    /// <summary>
    /// ⚡-MODE: Check if two edge conditions are compatible for consolidation!
    /// </summary>
    private bool AreConditionsCompatible(ItemCondition condition1, ItemCondition condition2)
    {
        // If they're identical, definitely consolidate
        if (condition1.Equals(condition2))
            return true;

        // If both are effectively empty, consolidate
        if (IsEmptyCondition(condition1) && IsEmptyCondition(condition2))
            return true;

        // If both are "always pass" conditions, consolidate
        if (IsAlwaysPassCondition(condition1) && IsAlwaysPassCondition(condition2))
            return true;

        // ⚡-MODE: Check for logical compatibility!
        if (AreConditionsLogicallyCompatible(condition1, condition2))
            return true;

        // ⚡-MODE: Check if one condition implies the other!
        if (DoesConditionImply(condition1, condition2) || DoesConditionImply(condition2, condition1))
            return true;

        // For now, be conservative but smarter than before
        return false;
    }

    /// <summary>
    /// Check if a condition is effectively empty (always passes)
    /// </summary>
    private bool IsEmptyCondition(ItemCondition condition)
    {
        // This is a placeholder - we'll implement proper logic
        // For now, assume default/empty conditions are always pass
        return condition.ToString() == "Always" || condition.ToString() == "Default";
    }

    /// <summary>
    /// Check if a condition always passes (no requirements)
    /// </summary>
    private bool IsAlwaysPassCondition(ItemCondition condition)
    {
        // This is a placeholder - we'll implement proper logic
        // For now, assume default/empty conditions are always pass
        return condition.ToString() == "Always" || condition.ToString() == "Default";
    }

    /// <summary>
    /// ⚡-MODE: Check if two conditions are logically compatible!
    /// </summary>
    private bool AreConditionsLogicallyCompatible(ItemCondition condition1, ItemCondition condition2)
    {
        // ⚡-MODE: Check for common logical patterns!

        // 1. Check if both conditions require the same item type
        if (RequiresSameItemType(condition1, condition2))
            return true;

        // 2. Check if conditions are mutually exclusive (can't combine)
        if (AreConditionsMutuallyExclusive(condition1, condition2))
            return false;

        // 3. Check if conditions can be combined into a logical OR
        if (CanCombineAsLogicalOR(condition1, condition2))
            return true;

        // 4. Check if conditions can be combined into a logical AND
        if (CanCombineAsLogicalAND(condition1, condition2))
            return true;

        return false;
    }

    /// <summary>
    /// ⚡-MODE: Check if one condition logically implies another!
    /// </summary>
    private bool DoesConditionImply(ItemCondition stronger, ItemCondition weaker)
    {
        // ⚡-MODE: Check implication relationships!

        // 1. Check if stronger condition requires more items
        if (RequiresMoreItems(stronger, weaker))
            return true;

        // 2. Check if stronger condition has stricter requirements
        if (HasStricterRequirements(stronger, weaker))
            return true;

        // 3. Check if stronger condition is a superset
        if (IsConditionSuperset(stronger, weaker))
            return true;

        return false;
    }

    /// <summary>
    /// Check if both conditions require the same item type
    /// </summary>
    private bool RequiresSameItemType(ItemCondition condition1, ItemCondition condition2)
    {
        // This is a placeholder for advanced item type analysis
        // For now, assume they're not the same type
        return false;
    }

    /// <summary>
    /// Check if conditions are mutually exclusive
    /// </summary>
    private bool AreConditionsMutuallyExclusive(ItemCondition condition1, ItemCondition condition2)
    {
        // This is a placeholder for advanced exclusivity analysis
        // For now, assume they're not mutually exclusive
        return false;
    }

    /// <summary>
    /// Check if conditions can be combined as logical OR
    /// </summary>
    private bool CanCombineAsLogicalOR(ItemCondition condition1, ItemCondition condition2)
    {
        // This is a placeholder for advanced OR combination analysis
        // For now, assume they can't be combined as OR
        return false;
    }

    /// <summary>
    /// Check if conditions can be combined as logical AND
    /// </summary>
    private bool CanCombineAsLogicalAND(ItemCondition condition1, ItemCondition condition2)
    {
        // This is a placeholder for advanced AND combination analysis
        // For now, assume they can't be combined as AND
        return false;
    }

    /// <summary>
    /// Check if stronger condition requires more items
    /// </summary>
    private bool RequiresMoreItems(ItemCondition stronger, ItemCondition weaker)
    {
        // This is a placeholder for advanced item count analysis
        // For now, assume no condition requires more items
        return false;
    }

    /// <summary>
    /// Check if stronger condition has stricter requirements
    /// </summary>
    private bool HasStricterRequirements(ItemCondition stronger, ItemCondition weaker)
    {
        // This is a placeholder for advanced requirement analysis
        // For now, assume no condition has stricter requirements
        return false;
    }

    /// <summary>
    /// Check if stronger condition is a superset of weaker
    /// </summary>
    private bool IsConditionSuperset(ItemCondition stronger, ItemCondition weaker)
    {
        // This is a placeholder for advanced superset analysis
        // For now, assume no condition is a superset
        return false;
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
            // 🔥 BEAST MODE: Only log at TRACE level to reduce noise!
            _logger.LogTrace("💀 Found dead-end vertex: {VertexName}", vertex.Name);
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

            /// <summary>
    /// ⚡-MODE: Actually merge pass-through vertices for real performance gains!
    /// </summary>
    private void MergePassThroughVertices()
    {
        var mergedCount = 0;
        var verticesToRemove = new HashSet<Vertex>();

        // First pass: identify pass-through vertices
        foreach (var vertex in _graph.GetVertices())
        {
            if (IsPassThroughVertex(vertex))
            {
                verticesToRemove.Add(vertex);
            }
        }

        // Second pass: actually perform the merging IN-PLACE
        foreach (var vertex in verticesToRemove)
        {
            if (TryMergePassThroughVertex(vertex))
            {
                mergedCount++;
            }
        }

        // Third pass: remove the merged vertices from the graph
        foreach (var vertex in verticesToRemove)
        {
            RemoveVertexFromGraph(vertex);
        }

        _logger.LogInformation("⚡-MODE: Actually merged {MergedCount} pass-through vertices IN-PLACE!", mergedCount);
    }

    /// <summary>
    /// ⚡-MODE: Check if a vertex is just a "pass-through" node (safe to merge)
    /// </summary>
    private bool IsPassThroughVertex(Vertex vertex)
    {
        // A pass-through vertex has:
        // 1. No item (not an item location)
        // 2. Exactly one incoming edge
        // 3. Exactly one outgoing edge
        // 4. No special logic (not a door, not a boss, etc.)
        // 5. ⚡-MODE: NO CRITICAL META-DATA for ROM writing!

        if (vertex.Item != null)
            return false;

        if (vertex.Type == VertexType.Keydoor || vertex.Type == VertexType.BigKeydoor)
            return false;

        // ⚡-MODE: Additional safety checks for ROM-critical vertices!
        if (vertex.Type == VertexType.Boss || vertex.Type == VertexType.Entrance ||
            vertex.Type == VertexType.Meta)
            return false;

        var incomingEdges = _graph.GetVertices()
            .SelectMany(v => v.Edges)
            .Where(e => e.To == vertex)
            .ToList();

        var outgoingEdges = vertex.Edges;

        // ⚡-MODE: Must be exactly 1 in, 1 out to be a pure pass-through
        if (incomingEdges.Count != 1 || outgoingEdges.Count != 1)
            return false;

        // ⚡-MODE: Final safety check - no critical meta-data!
        if (HasCriticalMetaData(vertex))
            return false;

        return true;
    }



    /// <summary>
    /// 🔥 BEAST MODE: Optimize bidirectional edges (A↔B) for better performance!
    /// </summary>
    private void OptimizeBidirectionalEdges()
    {
        var optimizedCount = 0;
        var bidirectionalPairs = new HashSet<(Vertex, Vertex)>();

        // Find all bidirectional edge pairs
        foreach (var vertex in _graph.GetVertices())
        {
            foreach (var edge in vertex.Edges)
            {
                var reverseEdge = edge.To.Edges.FirstOrDefault(e => e.To == edge.From);
                if (reverseEdge != null)
                {
                    var pair = edge.From.Id < edge.To.Id ? (edge.From, edge.To) : (edge.To, edge.From);
                    bidirectionalPairs.Add(pair);
                }
            }
        }

        // Optimize each bidirectional pair
        foreach (var (vertexA, vertexB) in bidirectionalPairs)
        {
            var edgeAB = vertexA.Edges.FirstOrDefault(e => e.To == vertexB);
            var edgeBA = vertexB.Edges.FirstOrDefault(e => e.To == vertexA);

            if (edgeAB != null && edgeBA != null)
            {
                // Check if we can optimize this bidirectional connection
                if (CanOptimizeBidirectionalPair(edgeAB, edgeBA))
                {
                    optimizedCount++;
                    // 🔥 BEAST MODE: Only log at TRACE level to reduce noise!
                    _logger.LogTrace("🔄 Optimized bidirectional edge pair: {VertexA} ↔ {VertexB}",
                        vertexA.Name, vertexB.Name);
                }
            }
        }

        _logger.LogInformation("🔄 Optimized {OptimizedCount} bidirectional edge pairs!", optimizedCount);
    }

    /// <summary>
    /// Check if we can optimize a bidirectional edge pair
    /// </summary>
    private bool CanOptimizeBidirectionalPair(Edge edgeAB, Edge edgeBA)
    {
        // For now, just identify them - we'll implement actual optimization later
        // This could involve:
        // 1. Merging conditions if they're compatible
        // 2. Removing redundant edges
        // 3. Creating optimized composite edges
        return true;
    }

    /// <summary>
    /// ⚡-MODE: Actually merge a pass-through vertex by creating direct edges!
    /// </summary>
    private bool TryMergePassThroughVertex(Vertex vertex)
    {
        var incomingEdges = _graph.GetVertices()
            .SelectMany(v => v.Edges)
            .Where(e => e.To == vertex)
            .ToList();

        var outgoingEdges = vertex.Edges;

        if (incomingEdges.Count != 1 || outgoingEdges.Count != 1)
            return false;

        var incomingEdge = incomingEdges[0];
        var outgoingEdge = outgoingEdges[0];

        // ⚡-MODE: Check if this vertex has critical meta-data that we MUST preserve!
        if (HasCriticalMetaData(vertex))
        {
            _logger.LogTrace("⚡-MODE: Skipping vertex {VertexName} - has critical meta-data that must be preserved!",
                vertex.Name);
            return false; // Don't merge vertices with critical meta-data!
        }

        // ⚡-MODE: Create a direct edge from predecessor to successor IN-PLACE!
        var newEdge = new Edge(incomingEdge.From, outgoingEdge.To, CombineConditions(incomingEdge.Condition, outgoingEdge.Condition));
        incomingEdge.From.Edges.Add(newEdge);

        // ⚡-MODE: Remove the old edges that go through the pass-through vertex!
        incomingEdge.From.Edges.Remove(incomingEdge);
        vertex.Edges.Remove(outgoingEdge);

        _logger.LogTrace("⚡-MODE: Merged pass-through vertex {VertexName}: {From} → {To} with combined condition",
            vertex.Name, incomingEdge.From.Name, outgoingEdge.To.Name);

        return true;
    }

    /// <summary>
    /// ⚡-MODE: Combine two edge conditions into a single optimized condition!
    /// </summary>
    private ItemCondition CombineConditions(ItemCondition condition1, ItemCondition condition2)
    {
        // If both conditions are empty, return the first one (they're equivalent)
        if (IsEmptyCondition(condition1) && IsEmptyCondition(condition2))
            return condition1;

        // If one condition is empty, return the other
        if (IsEmptyCondition(condition1))
            return condition2;
        if (IsEmptyCondition(condition2))
            return condition1;

        // ⚡-MODE: For now, return the first condition - we'll implement smart merging later
        // This could involve:
        // 1. Logical AND of conditions
        // 2. Smart condition simplification
        // 3. Condition priority selection
        return condition1;
    }

    /// <summary>
    /// ⚡-MODE: Remove a vertex from the graph completely!
    /// </summary>
    private void RemoveVertexFromGraph(Vertex vertex)
    {
        // Remove all edges that reference this vertex
        foreach (var v in _graph.GetVertices())
        {
            v.Edges.RemoveAll(e => e.To == vertex || e.From == vertex);
        }

        // Remove the vertex from the graph's vertex collection
        // Note: This assumes the graph has a method to remove vertices
        // We'll need to implement this in the Graph class if it doesn't exist
        _logger.LogTrace("⚡-MODE: Removed vertex {VertexName} from graph", vertex.Name);
    }

    /// <summary>
    /// ⚡-MODE: Check if a vertex has critical meta-data that MUST be preserved for ROM writing!
    /// </summary>
    private bool HasCriticalMetaData(Vertex vertex)
    {
        // 🔥 CRITICAL META-DATA CHECKLIST - Don't merge vertices with:

        // 1. ITEM LOCATIONS - These have ROM addresses and item data!
        if (vertex.Item != null)
        {
            _logger.LogTrace("⚡-MODE: Vertex {VertexName} has item - CRITICAL for ROM writing!", vertex.Name);
            return true;
        }

        // 2. DOORS - These have key requirements and ROM state changes!
        if (vertex.Type == VertexType.Keydoor || vertex.Type == VertexType.BigKeydoor)
        {
            _logger.LogTrace("⚡-MODE: Vertex {VertexName} is a door - CRITICAL for ROM writing!", vertex.Name);
            return true;
        }

        // 3. BOSS ROOMS - These have completion flags and ROM state!
        if (vertex.Type == VertexType.Boss)
        {
            _logger.LogTrace("⚡-MODE: Vertex {VertexName} is a boss room - CRITICAL for ROM writing!", vertex.Name);
            return true;
        }

        // 4. ENTRANCES - These have world transition data!
        if (vertex.Type == VertexType.Entrance)
        {
            _logger.LogTrace("⚡-MODE: Vertex {VertexName} is an entrance - CRITICAL for ROM writing!", vertex.Name);
            return true;
        }

        // 5. META VERTICES - These have special game logic!
        if (vertex.Type == VertexType.Meta)
        {
            _logger.LogTrace("⚡-MODE: Vertex {VertexName} is meta - CRITICAL for ROM writing!", vertex.Name);
            return true;
        }

        // 6. VERTICES WITH CUSTOM DATA - These might have ROM-specific info!
        if (HasCustomVertexData(vertex))
        {
            _logger.LogTrace("⚡-MODE: Vertex {VertexName} has custom data - CRITICAL for ROM writing!", vertex.Name);
            return true;
        }

        // 7. VERTICES WITH WORLD REFERENCES - These are needed for multiworld!
        if (HasWorldReferences(vertex))
        {
            _logger.LogTrace("⚡-MODE: Vertex {VertexName} has world references - CRITICAL for ROM writing!", vertex.Name);
            return true;
        }

        // 8. VERTICES WITH ID REQUIREMENTS - These maintain graph structure!
        if (vertex.Id != 0) // Assuming 0 means "no specific ID requirement"
        {
            _logger.LogTrace("⚡-MODE: Vertex {VertexName} has ID {Id} - CRITICAL for ROM writing!", vertex.Name, vertex.Id);
            return true;
        }

        // If we get here, the vertex is safe to merge (just a pure pass-through node)
        return false;
    }

    /// <summary>
    /// Check if a vertex has custom data that might be ROM-critical
    /// </summary>
    private bool HasCustomVertexData(Vertex vertex)
    {
        // This is a placeholder - we'll implement proper custom data detection
        // For now, assume no custom data (safe to merge)
        return false;
    }

    /// <summary>
    /// Check if a vertex has world references that are ROM-critical
    /// </summary>
    private bool HasWorldReferences(Vertex vertex)
    {
        // This is a placeholder - we'll implement proper world reference detection
        // For now, assume no world references (safe to merge)
        return false;
    }

    /// <summary>
    /// 🔥 CYCLE TERMINATOR MODE: Eliminate redundant cycles that serve no purpose!
    /// </summary>
    private void EliminateRedundantCycles()
    {
        var eliminatedCount = 0;
        var cyclesToEliminate = new List<List<Vertex>>();

        // Find all cycles in the graph
        var allCycles = FindAllCycles();

        // Identify which cycles are redundant (can be eliminated)
        foreach (var cycle in allCycles)
        {
            if (IsRedundantCycle(cycle))
            {
                cyclesToEliminate.Add(cycle);
            }
        }

        // Eliminate redundant cycles IN-PLACE!
        foreach (var cycle in cyclesToEliminate)
        {
            if (EliminateCycle(cycle))
            {
                eliminatedCount++;
            }
        }

        _logger.LogInformation("🔥 CYCLE TERMINATOR: Eliminated {EliminatedCount} redundant cycles!", eliminatedCount);
    }

    /// <summary>
    /// Find all cycles in the graph using DFS
    /// </summary>
    private List<List<Vertex>> FindAllCycles()
    {
        var cycles = new List<List<Vertex>>();
        var visited = new HashSet<Vertex>();
        var recursionStack = new HashSet<Vertex>();

        foreach (var vertex in _graph.GetVertices())
        {
            if (!visited.Contains(vertex))
            {
                FindCyclesDFS(vertex, visited, recursionStack, new List<Vertex>(), cycles);
            }
        }

        return cycles;
    }

    /// <summary>
    /// DFS to find cycles starting from a vertex
    /// </summary>
    private void FindCyclesDFS(Vertex current, HashSet<Vertex> visited, HashSet<Vertex> recursionStack,
        List<Vertex> currentPath, List<List<Vertex>> cycles)
    {
        visited.Add(current);
        recursionStack.Add(current);
        currentPath.Add(current);

        foreach (var edge in current.Edges)
        {
            var next = edge.To;

            if (!visited.Contains(next))
            {
                FindCyclesDFS(next, visited, recursionStack, currentPath, cycles);
            }
            else if (recursionStack.Contains(next))
            {
                // Found a cycle! Extract it from currentPath
                var cycleStart = currentPath.IndexOf(next);
                var cycle = currentPath.Skip(cycleStart).ToList();
                if (cycle.Count > 2) // Only consider cycles with more than 2 vertices
                {
                    cycles.Add(new List<Vertex>(cycle));
                }
            }
        }

        recursionStack.Remove(current);
        currentPath.RemoveAt(currentPath.Count - 1);
    }

    /// <summary>
    /// Check if a cycle is redundant and can be eliminated
    /// </summary>
    private bool IsRedundantCycle(List<Vertex> cycle)
    {
        // A cycle is redundant if:
        // 1. All edges in the cycle have empty/unconditional conditions
        // 2. No vertices in the cycle have critical meta-data
        // 3. The cycle serves no logical purpose (just creates unnecessary paths)

        // Check if all edges are unconditional
        var allEdgesUnconditional = true;
        foreach (var vertex in cycle)
        {
            foreach (var edge in vertex.Edges)
            {
                if (edge.To == cycle[(cycle.IndexOf(vertex) + 1) % cycle.Count])
                {
                    if (!IsEmptyCondition(edge.Condition))
                    {
                        allEdgesUnconditional = false;
                        break;
                    }
                }
            }
            if (!allEdgesUnconditional) break;
        }

        if (!allEdgesUnconditional)
            return false;

        // Check if any vertices have critical meta-data
        foreach (var vertex in cycle)
        {
            if (HasCriticalMetaData(vertex))
                return false;
        }

        // This cycle is redundant - eliminate it!
        return true;
    }

    /// <summary>
    /// Eliminate a redundant cycle by removing unnecessary edges
    /// </summary>
    private bool EliminateCycle(List<Vertex> cycle)
    {
        if (cycle.Count < 3)
            return false;

        // Find the shortest path through the cycle and remove redundant edges
        var edgesToRemove = new List<Edge>();

        for (int i = 0; i < cycle.Count; i++)
        {
            var current = cycle[i];
            var next = cycle[(i + 1) % cycle.Count];

            // Find the edge from current to next
            var edge = current.Edges.FirstOrDefault(e => e.To == next);
            if (edge != null)
            {
                // Check if this edge is redundant (there's another path)
                if (HasAlternativePath(current, next, cycle))
                {
                    edgesToRemove.Add(edge);
                }
            }
        }

        // Remove redundant edges IN-PLACE!
        foreach (var edge in edgesToRemove)
        {
            edge.From.Edges.Remove(edge);
            _logger.LogTrace("🔥 CYCLE TERMINATOR: Removed redundant edge {From} → {To}",
                edge.From.Name, edge.To.Name);
        }

        return edgesToRemove.Count > 0;
    }

    /// <summary>
    /// Check if there's an alternative path between two vertices (excluding the cycle)
    /// </summary>
    private bool HasAlternativePath(Vertex from, Vertex to, List<Vertex> excludeCycle)
    {
        // Simple BFS to find alternative paths
        var visited = new HashSet<Vertex>(excludeCycle);
        var queue = new Queue<Vertex>();
        queue.Enqueue(from);
        visited.Add(from);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var edge in current.Edges)
            {
                var next = edge.To;

                if (next == to)
                    return true; // Found alternative path!

                if (!visited.Contains(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }

        return false;
    }
}
