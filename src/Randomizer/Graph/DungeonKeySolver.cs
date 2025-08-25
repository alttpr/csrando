namespace Randomizer.Graph;

using System.Collections;
using System.Numerics;

/// <summary>
/// Node in a dungeon graph for the key solver
/// </summary>
public sealed class DungeonNode
{
    /// <summary>Gets or sets the unique identifier for this node</summary>
    public int Id { get; init; }
    
    /// <summary>Gets or sets the dungeon identifier this node belongs to</summary>
    public string DungeonId { get; init; } = "";
    
    /// <summary>Gets or sets whether this node represents an item location</summary>
    public bool IsLocation { get; init; }
    
    /// <summary>Gets or sets the static keys available at this node by dungeon</summary>
    public Dictionary<string, int> StaticKeys { get; init; } = new();
}

/// <summary>
/// Edge in a dungeon graph for the key solver
/// </summary>
public sealed class DungeonEdge
{
    /// <summary>Gets or sets the source node ID</summary>
    public int From { get; init; }
    
    /// <summary>Gets or sets the destination node ID</summary>
    public int To { get; init; }
    
    /// <summary>Gets or sets the requirement to traverse this edge (null/"fixed" = free, "KEY" = small key, other = item gate)</summary>
    public string? Req { get; init; }
    
    /// <summary>Gets or sets the dungeon identifier this edge belongs to</summary>
    public string DungeonId { get; init; } = "";
    
    /// <summary>Gets or sets the door group ID for bidirectional doors (null = unique door)</summary>
    public int? DoorGroupId { get; init; }
}

/// <summary>
/// Dungeon graph representation for the key solver
/// </summary>
public sealed class DungeonGraph
{
    /// <summary>Gets or sets the list of nodes in the graph</summary>
    public IReadOnlyList<DungeonNode> Nodes { get; init; } = Array.Empty<DungeonNode>();
    
    /// <summary>Gets or sets the list of edges in the graph</summary>
    public IReadOnlyList<DungeonEdge> Edges { get; init; } = Array.Empty<DungeonEdge>();
}

/// <summary>
/// Door arc in the component meta-graph
/// </summary>
internal sealed class DoorArc
{
    public int FromComp { get; init; }
    public int ToComp { get; init; }
    public int Bit { get; init; }
}

/// <summary>
/// Closure result for a door mask
/// </summary>
internal sealed class ClosureResult
{
    public BitArray ReachableComps { get; init; } = new(0);
    public int GainedStaticKeys { get; init; }
}

/// <summary>
/// Strongly Connected Component solver using iterative algorithms
/// </summary>
internal static class StronglyConnectedComponents
{
    /// <summary>
    /// Compute SCCs using iterative Kosaraju algorithm
    /// </summary>
    public static List<List<int>> ComputeSCCs(int nodeCount, IReadOnlyList<(int From, int To)> edges)
    {
        // Build adjacency lists
        var adj = new List<int>[nodeCount];
        var adjReverse = new List<int>[nodeCount];
        for (int i = 0; i < nodeCount; i++)
        {
            adj[i] = new List<int>();
            adjReverse[i] = new List<int>();
        }

        foreach (var (from, to) in edges)
        {
            adj[from].Add(to);
            adjReverse[to].Add(from);
        }

        // First DFS to get finish order
        var visited = new bool[nodeCount];
        var finishOrder = new Stack<int>();

        for (int i = 0; i < nodeCount; i++)
        {
            if (!visited[i])
                IterativeDFS(i, adj, visited, finishOrder);
        }

        // Second DFS on transpose graph
        visited = new bool[nodeCount];
        var components = new List<List<int>>();

        while (finishOrder.Count > 0)
        {
            int node = finishOrder.Pop();
            if (!visited[node])
            {
                var component = new List<int>();
                IterativeDFSCollect(node, adjReverse, visited, component);
                components.Add(component);
            }
        }

        return components;
    }

    private static void IterativeDFS(int start, List<int>[] adj, bool[] visited, Stack<int> finishOrder)
    {
        var stack = new Stack<(int node, int childIndex, bool finishing)>();
        stack.Push((start, 0, false));

        while (stack.Count > 0)
        {
            var (node, childIndex, finishing) = stack.Pop();

            if (finishing)
            {
                finishOrder.Push(node);
                continue;
            }

            if (visited[node])
                continue;

            visited[node] = true;

            // Push finishing marker
            stack.Push((node, 0, true));

            // Push children in reverse order to maintain DFS order
            for (int i = adj[node].Count - 1; i >= 0; i--)
            {
                int child = adj[node][i];
                if (!visited[child])
                    stack.Push((child, 0, false));
            }
        }
    }

    private static void IterativeDFSCollect(int start, List<int>[] adj, bool[] visited, List<int> component)
    {
        var stack = new Stack<int>();
        stack.Push(start);

        while (stack.Count > 0)
        {
            int node = stack.Pop();

            if (visited[node])
                continue;

            visited[node] = true;
            component.Add(node);

            foreach (int neighbor in adj[node])
            {
                if (!visited[neighbor])
                    stack.Push(neighbor);
            }
        }
    }
}

/// <summary>
/// Always-Accessible Locations Solver for Small-Key Dungeons.
/// 
/// This solver computes the set of item locations that are reachable in every maximal way 
/// a player can spend keys, making them safe for item placement regardless of key-spend order.
/// 
/// The algorithm works by:
/// 1. Condensing the dungeon to strongly connected components (SCCs) 
/// 2. Building a meta-graph of components with door arcs
/// 3. Using budgeted BFS with saturation to enumerate all feasible door combinations
/// 4. Computing the intersection of reachable locations across all maximal key-spend scenarios
/// </summary>
public sealed class DungeonKeySolver
{
    private readonly string _dungeonId;
    private readonly List<DungeonNode> _dungeonNodes;
    private readonly List<DungeonEdge> _dungeonEdges;
    private readonly Dictionary<int, int> _nodeIdToIndex;

    /// <summary>
    /// Create a precomputed solver for a specific dungeon.
    /// The solver can be reused for multiple queries with different item states and key counts.
    /// </summary>
    /// <param name="graph">The complete dungeon graph containing nodes and edges</param>
    /// <param name="dungeonId">The identifier of the dungeon to solve for</param>
    public DungeonKeySolver(DungeonGraph graph, string dungeonId)
    {
        _dungeonId = dungeonId;

        // Filter nodes and edges for this dungeon
        _dungeonNodes = graph.Nodes.Where(n => n.DungeonId == dungeonId).ToList();
        _dungeonEdges = graph.Edges.Where(e => e.DungeonId == dungeonId).ToList();
        
        // Create mapping from original node ID to index in filtered list
        _nodeIdToIndex = _dungeonNodes.Select((n, i) => new { n.Id, Index = i }).ToDictionary(x => x.Id, x => x.Index);
    }

    /// <summary>
    /// Compute which strongly connected components are safe under all feasible key-spend orders.
    /// </summary>
    /// <param name="itemCheck">Function to check if non-key requirements are satisfied</param>
    /// <param name="initialKeys">Number of small keys available from the item pool</param>
    /// <param name="entranceNodeIds">Node IDs where the player can enter the dungeon</param>
    /// <returns>BitArray indicating which components are safe (true = safe)</returns>
    public BitArray SafeComponents(Func<string, bool> itemCheck, int initialKeys, IReadOnlyList<int> entranceNodeIds)
    {
        // Build the component graph based on current item state
        var (components, compOfNode, freeAdj, doorArcs, doorsFromComp, keysInComp, doorCount) = 
            BuildComponentGraph(itemCheck);

        // Find entrance components
        var entranceComps = entranceNodeIds
            .Where(nodeId => _nodeIdToIndex.ContainsKey(nodeId))
            .Select(nodeId => compOfNode[_nodeIdToIndex[nodeId]])
            .ToHashSet();

        if (entranceComps.Count == 0)
            return new BitArray(components.Count);

        // Use budgeted BFS to enumerate feasible door sets
        var maximalClosures = new List<BitArray>();
        var visitedMasks = new HashSet<ulong>();
        var queue = new Queue<ulong>();

        // Start with no doors opened
        ulong initialMask = 0;
        var saturatedMask = SaturateAndComputeClosure(
            initialMask, initialKeys, entranceComps, components, freeAdj, doorArcs, doorsFromComp, keysInComp);
        
        if (visitedMasks.Add(saturatedMask))
        {
            queue.Enqueue(saturatedMask);
        }

        while (queue.Count > 0)
        {
            var mask = queue.Dequeue();
            var closureResult = ComputeClosure(mask, entranceComps, components, freeAdj, doorArcs, doorsFromComp, keysInComp);
            var keysAvailable = initialKeys + closureResult.GainedStaticKeys - BitOperations.PopCount(mask);
            var candidateDoors = CollectCandidateDoors(mask, closureResult.ReachableComps, doorsFromComp, doorArcs);

            if (candidateDoors.Count == 0 || keysAvailable == 0)
            {
                // Maximal state
                maximalClosures.Add(closureResult.ReachableComps);
            }
            else
            {
                // Branch on each candidate door
                foreach (var doorBit in candidateDoors)
                {
                    var newMask = mask | (1UL << doorBit);
                    var newSaturatedMask = SaturateAndComputeClosure(
                        newMask, initialKeys, entranceComps, components, freeAdj, doorArcs, doorsFromComp, keysInComp);
                    
                    if (visitedMasks.Add(newSaturatedMask))
                    {
                        queue.Enqueue(newSaturatedMask);
                    }
                }
            }
        }

        // Compute intersection of all maximal closures
        if (maximalClosures.Count == 0)
            return new BitArray(components.Count);

        var result = (BitArray)maximalClosures[0].Clone();
        for (int i = 1; i < maximalClosures.Count; i++)
        {
            result.And(maximalClosures[i]);
        }

        return result;
    }

    /// <summary>
    /// Get the set of item location node IDs that are safe under all feasible key-spend orders.
    /// This is the main method used for reverse-fill item placement.
    /// </summary>
    /// <param name="itemCheck">Function to check if non-key requirements are satisfied with current world state</param>
    /// <param name="initialKeys">Number of small keys available from the item pool (excludes static keys)</param>
    /// <param name="entranceNodeIds">Node IDs where the player can enter the dungeon</param>
    /// <returns>Set of node IDs that are safe for item placement</returns>
    public HashSet<int> SafeItemLocations(Func<string, bool> itemCheck, int initialKeys, IReadOnlyList<int> entranceNodeIds)
    {
        var (components, compOfNode, freeAdj, doorArcs, doorsFromComp, keysInComp, doorCount) = 
            BuildComponentGraph(itemCheck);

        var safeComps = SafeComponents(itemCheck, initialKeys, entranceNodeIds);
        var result = new HashSet<int>();

        for (int compId = 0; compId < components.Count; compId++)
        {
            if (safeComps[compId])
            {
                foreach (int nodeIndex in components[compId])
                {
                    var node = _dungeonNodes[nodeIndex];
                    if (node.IsLocation)
                    {
                        result.Add(node.Id); // Return original node ID
                    }
                }
            }
        }

        return result;
    }

    private (List<List<int>> components, Dictionary<int, int> compOfNode, int[][] freeAdj, 
        List<DoorArc> doorArcs, Dictionary<int, List<int>> doorsFromComp, int[] keysInComp, int doorCount)
        BuildComponentGraph(Func<string, bool> itemCheck)
    {
        // Build keyless edge list based on current item state
        var keylessEdges = new List<(int From, int To)>();
        var keyEdges = new List<DungeonEdge>();

        foreach (var edge in _dungeonEdges)
        {
            if (!_nodeIdToIndex.ContainsKey(edge.From) || !_nodeIdToIndex.ContainsKey(edge.To))
                continue;

            var from = _nodeIdToIndex[edge.From];
            var to = _nodeIdToIndex[edge.To];

            if (edge.Req == null || edge.Req == "fixed")
            {
                // Free edge
                keylessEdges.Add((from, to));
            }
            else if (edge.Req == "KEY")
            {
                // Key edge for this dungeon - save for door processing
                keyEdges.Add(new DungeonEdge { From = from, To = to, DoorGroupId = edge.DoorGroupId });
            }
            else
            {
                // Non-key item gate - check if passable
                if (itemCheck(edge.Req))
                {
                    keylessEdges.Add((from, to));
                }
            }
        }

        // Compute SCCs
        var components = StronglyConnectedComponents.ComputeSCCs(_dungeonNodes.Count, keylessEdges);

        // Build component mapping
        var compOfNode = new Dictionary<int, int>();
        for (int i = 0; i < components.Count; i++)
        {
            foreach (int nodeIndex in components[i])
            {
                compOfNode[nodeIndex] = i;
            }
        }

        // Build free adjacency lists between components
        var freeAdj = new int[components.Count][];
        var adjLists = new List<HashSet<int>>();
        for (int i = 0; i < components.Count; i++)
        {
            adjLists.Add(new HashSet<int>());
        }

        foreach (var (from, to) in keylessEdges)
        {
            var fromComp = compOfNode[from];
            var toComp = compOfNode[to];
            if (fromComp != toComp)
            {
                adjLists[fromComp].Add(toComp);
            }
        }

        for (int i = 0; i < components.Count; i++)
        {
            freeAdj[i] = adjLists[i].ToArray();
        }

        // Process door edges and group them
        var doorGroups = new Dictionary<int, List<DungeonEdge>>();
        int nextGroupId = 0;

        foreach (var edge in keyEdges)
        {
            int groupId = edge.DoorGroupId ?? nextGroupId++;
            if (!doorGroups.ContainsKey(groupId))
                doorGroups[groupId] = new List<DungeonEdge>();
            doorGroups[groupId].Add(edge);
        }

        // Create door arcs with bit assignments
        var doorArcs = new List<DoorArc>();
        var doorsFromComp = new Dictionary<int, List<int>>();
        var doorCount = doorGroups.Count;

        int bitIndex = 0;
        foreach (var (groupId, edges) in doorGroups)
        {
            foreach (var edge in edges)
            {
                var fromComp = compOfNode[edge.From];
                var toComp = compOfNode[edge.To];

                doorArcs.Add(new DoorArc { FromComp = fromComp, ToComp = toComp, Bit = bitIndex });

                if (!doorsFromComp.ContainsKey(fromComp))
                    doorsFromComp[fromComp] = new List<int>();
                doorsFromComp[fromComp].Add(doorArcs.Count - 1);
            }
            bitIndex++;
        }

        // Compute static keys per component
        var keysInComp = new int[components.Count];
        for (int i = 0; i < components.Count; i++)
        {
            keysInComp[i] = components[i].Sum(nodeIndex => 
                _dungeonNodes[nodeIndex].StaticKeys.GetValueOrDefault(_dungeonId, 0));
        }

        return (components, compOfNode, freeAdj, doorArcs, doorsFromComp, keysInComp, doorCount);
    }

    private static ulong SaturateAndComputeClosure(ulong mask, int initialKeys, HashSet<int> entranceComps,
        List<List<int>> components, int[][] freeAdj, List<DoorArc> doorArcs, 
        Dictionary<int, List<int>> doorsFromComp, int[] keysInComp)
    {
        ulong currentMask = mask;
        
        while (true)
        {
            var closure = ComputeClosure(currentMask, entranceComps, components, freeAdj, doorArcs, doorsFromComp, keysInComp);
            var keysAvailable = initialKeys + closure.GainedStaticKeys - BitOperations.PopCount(currentMask);
            var candidateDoors = CollectCandidateDoors(currentMask, closure.ReachableComps, doorsFromComp, doorArcs);

            if (candidateDoors.Count == 0 || keysAvailable < candidateDoors.Count)
                break;

            // Open all candidate doors
            foreach (var doorBit in candidateDoors)
            {
                currentMask |= (1UL << doorBit);
            }
        }

        return currentMask;
    }

    private static ClosureResult ComputeClosure(ulong doorMask, HashSet<int> entranceComps,
        List<List<int>> components, int[][] freeAdj, List<DoorArc> doorArcs,
        Dictionary<int, List<int>> doorsFromComp, int[] keysInComp)
    {
        var reachable = new BitArray(components.Count);
        var queue = new Queue<int>();

        // Start from entrance components
        foreach (int comp in entranceComps)
        {
            if (!reachable[comp])
            {
                reachable[comp] = true;
                queue.Enqueue(comp);
            }
        }

        // BFS through components
        while (queue.Count > 0)
        {
            var comp = queue.Dequeue();

            // Traverse free adjacencies
            foreach (int neighbor in freeAdj[comp])
            {
                if (!reachable[neighbor])
                {
                    reachable[neighbor] = true;
                    queue.Enqueue(neighbor);
                }
            }

            // Traverse open doors
            if (doorsFromComp.TryGetValue(comp, out var doorIndices))
            {
                foreach (int doorIndex in doorIndices)
                {
                    var door = doorArcs[doorIndex];
                    if ((doorMask & (1UL << door.Bit)) != 0)
                    {
                        if (!reachable[door.ToComp])
                        {
                            reachable[door.ToComp] = true;
                            queue.Enqueue(door.ToComp);
                        }
                    }
                }
            }
        }

        // Calculate gained static keys
        int gainedKeys = 0;
        for (int i = 0; i < components.Count; i++)
        {
            if (reachable[i])
                gainedKeys += keysInComp[i];
        }

        return new ClosureResult { ReachableComps = reachable, GainedStaticKeys = gainedKeys };
    }

    private static List<int> CollectCandidateDoors(ulong doorMask, BitArray reachableComps,
        Dictionary<int, List<int>> doorsFromComp, List<DoorArc> doorArcs)
    {
        var candidates = new HashSet<int>();

        for (int comp = 0; comp < reachableComps.Length; comp++)
        {
            if (!reachableComps[comp])
                continue;

            if (doorsFromComp.TryGetValue(comp, out var doorIndices))
            {
                foreach (int doorIndex in doorIndices)
                {
                    var door = doorArcs[doorIndex];
                    if ((doorMask & (1UL << door.Bit)) == 0) // Door not yet opened
                    {
                        candidates.Add(door.Bit);
                    }
                }
            }
        }

        return candidates.ToList();
    }
}

/// <summary>
/// Static methods for one-shot calls without creating a persistent solver instance.
/// Use these methods when you need to solve for a dungeon only once, or when memory usage is a concern.
/// </summary>
public static class DungeonKeySolverStatic
{
    /// <summary>
    /// One-shot call to get safe item locations for a dungeon.
    /// Creates a temporary solver instance and computes the result.
    /// </summary>
    /// <param name="graph">The complete dungeon graph containing nodes and edges</param>
    /// <param name="dungeonId">The identifier of the dungeon to solve for</param>
    /// <param name="entranceNodeIds">Node IDs where the player can enter the dungeon</param>
    /// <param name="itemCheck">Function to check if non-key requirements are satisfied with current world state</param>
    /// <param name="initialKeys">Number of small keys available from the item pool (excludes static keys)</param>
    /// <returns>Set of node IDs that are safe for item placement</returns>
    public static HashSet<int> SafeItemLocationsForDungeon(
        DungeonGraph graph,
        string dungeonId,
        IReadOnlyList<int> entranceNodeIds,
        Func<string, bool> itemCheck,
        int initialKeys)
    {
        var solver = new DungeonKeySolver(graph, dungeonId);
        return solver.SafeItemLocations(itemCheck, initialKeys, entranceNodeIds);
    }
}

/// <summary>
/// Helper class to convert the main Graph structure to DungeonGraph format for key solver integration
/// </summary>
public static class DungeonGraphConverter
{
    /// <summary>
    /// Extract dungeon subgraph from the main graph for a specific dungeon
    /// </summary>
    /// <param name="graph">The main graph containing all vertices and edges</param>
    /// <param name="dungeonName">Name of the dungeon to extract (e.g., "eastern", "desert", etc.)</param>
    /// <param name="keyItemName">Name of the small key item for this dungeon (e.g., "KeyP1", "KeyP2", etc.)</param>
    /// <returns>DungeonGraph suitable for DungeonKeySolver</returns>
    public static DungeonGraph ExtractDungeonGraph(Graph graph, string dungeonName, string keyItemName)
    {
        var dungeonNodes = new List<DungeonNode>();
        var dungeonEdges = new List<DungeonEdge>();
        var nodeIdMap = new Dictionary<int, int>(); // Original ID -> DungeonGraph ID
        var nextNodeId = 0;
        
        // Find the key item for this dungeon
        var keyItem = graph.AllItems.FirstOrDefault(item => item.Name == keyItemName);
        if (keyItem == null)
        {
            // No keys for this dungeon, return empty graph
            return new DungeonGraph { Nodes = dungeonNodes, Edges = dungeonEdges };
        }
        
        // Extract vertices that belong to this dungeon based on ItemSet
        var dungeonVertices = new List<Vertex>();
        foreach (var vertex in graph.GetVertices())
        {
            bool belongsToDungeon = vertex.ItemSet.Any(itemSet => 
                itemSet.Name.Equals(dungeonName, StringComparison.OrdinalIgnoreCase));
            
            if (belongsToDungeon)
            {
                dungeonVertices.Add(vertex);
            }
        }
        
        // Create DungeonNodes
        foreach (var vertex in dungeonVertices)
        {
            var dungeonNodeId = nextNodeId++;
            nodeIdMap[vertex.Id] = dungeonNodeId;
            
            var staticKeys = new Dictionary<string, int>();
            // Check if this vertex provides static keys
            if (graph.FixedKeys.TryGetValue(keyItem, out var fixedKeyVertices) && 
                fixedKeyVertices.Contains(vertex))
            {
                staticKeys[dungeonName] = 1; // Assuming 1 key per location for now
            }
            
            var dungeonNode = new DungeonNode
            {
                Id = vertex.Id, // Keep original ID for mapping back
                DungeonId = dungeonName,
                IsLocation = IsItemLocation(vertex),
                StaticKeys = staticKeys
            };
            
            dungeonNodes.Add(dungeonNode);
        }
        
        // Create DungeonEdges
        var doorGroupId = 0;
        var processedDoors = new HashSet<(Vertex, Vertex)>();
        
        foreach (var vertex in dungeonVertices)
        {
            foreach (var edge in vertex.Edges)
            {
                if (!dungeonVertices.Contains(edge.To))
                    continue; // Skip edges that leave the dungeon
                
                var req = GetEdgeRequirement(edge, keyItem);
                var doorGroup = GetDoorGroupId(graph, keyItem, vertex, edge.To, processedDoors, ref doorGroupId);
                
                var dungeonEdge = new DungeonEdge
                {
                    From = nodeIdMap[vertex.Id],
                    To = nodeIdMap[edge.To.Id],
                    Req = req,
                    DungeonId = dungeonName,
                    DoorGroupId = doorGroup
                };
                
                dungeonEdges.Add(dungeonEdge);
            }
        }
        
        return new DungeonGraph { Nodes = dungeonNodes, Edges = dungeonEdges };
    }
    
    private static bool IsItemLocation(Vertex vertex)
    {
        // Check if this vertex can hold items (chests, etc.)
        return vertex.Type is VertexType.Chest or VertexType.BigChest or VertexType.Drop or 
               VertexType.Pedestal or VertexType.Standing or VertexType.Pot or VertexType.Item;
    }
    
    private static string? GetEdgeRequirement(Edge edge, IItem keyItem)
    {
        if (edge.Condition.IsUnconditional)
            return "fixed"; // Free edge
        
        var requiredItem = edge.Condition.Item;
        if (requiredItem.Name == keyItem.Name)
            return "KEY"; // Small key requirement
        
        return requiredItem.Name; // Other item requirement
    }
    
    private static int? GetDoorGroupId(Graph graph, IItem keyItem, Vertex from, Vertex to, 
        HashSet<(Vertex, Vertex)> processedDoors, ref int nextDoorGroupId)
    {
        // Check if this is a bidirectional door
        if (!graph.Doors.TryGetValue(keyItem, out var doorsByKey))
            return null;
        
        foreach (var (_, doorPairs) in doorsByKey)
        {
            foreach (var (a, b) in doorPairs)
            {
                bool isThisDoor = (a == from && b == to) || (a == to && b == from);
                if (isThisDoor)
                {
                    var doorKey = (a.Id < b.Id) ? (a, b) : (b, a); // Normalize order
                    if (!processedDoors.Contains(doorKey))
                    {
                        processedDoors.Add(doorKey);
                        return nextDoorGroupId++;
                    }
                    else
                    {
                        // Find existing door group ID
                        // This is a simplification - in practice we'd need to track the mapping
                        return null; // Return null for now, bidirectional handling needs refinement
                    }
                }
            }
        }
        
        return null; // Not a special door
    }
}