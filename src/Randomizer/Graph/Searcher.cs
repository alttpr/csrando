namespace Randomizer.Graph;

using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using SearchResult = (VertexHashSet NewlyVisited, VertexHashSet NewSearchStarts);

public class Searcher
{
    private static readonly ILogger _logger = ClassLogger.Get();

    private readonly VertexHashSet _visited;
    private readonly VertexHashSet _collected;
    private readonly Graph _graph;
    private readonly VertexHashSet _searchStarts;
    private readonly Inventory _inventory;
    private readonly SetLocations _setLocations;
    
    // Performance optimization: Cache dungeon solver instances and context
    private static readonly Dictionary<(Graph, string), DungeonKeySolver> _solverCache = new();
    private static readonly Dictionary<(Graph, string), DungeonGraph> _dungeonGraphCache = new();
    private static readonly Dictionary<Vertex, string?> _vertexDungeonCache = new();

    /// <summary>
    /// Clear all caches to free memory and handle graph changes
    /// </summary>
    public static void ClearPerformanceCaches()
    {
        _solverCache.Clear();
        _dungeonGraphCache.Clear();
        _vertexDungeonCache.Clear();
        DungeonGraphConverter.ClearCaches();
    }

    /// <summary>
    /// I'm a jerk and don't like useful messages.
    /// </summary>
    /// <param name="graph">The graph to search</param>
    /// <param name="start">The starting point to search from</param>
    /// <param name="inventory">The current inventory to use while searching</param>
    public Searcher(Graph graph, Vertex start, Inventory inventory, SetLocations? setLocations = null)
    {
        _graph = graph;
        _visited = new(graph);
        _collected = new(graph);
        _searchStarts = new(graph) { start };
        _inventory = inventory;
        _setLocations = setLocations ?? new();

        bool newItemsFound;
        do
        {
            do
            {
                var (newlyVisited, newSearchStarts) = InternalSearch(inventory, _visited, _searchStarts);
                _visited.UnionWith(newlyVisited);
                _searchStarts.Clear();
                _searchStarts.UnionWith(newSearchStarts);

                newItemsFound = CollectItems(inventory, _visited, _collected);
            } while (newItemsFound);

            if (DoorSearch(inventory))
                newItemsFound = true;
        } while (newItemsFound);
    }

    /// <summary>
    /// Spend keys from inventory for simple cases.
    /// Case 1: If you have all randomized keys in a set, you should be able to eventually
    /// reach all the fixed keys from pots and enemies and thus open all the doors.
    /// Case 2: If you see both sides of a door, you should be able to burn a key there
    /// to simulate the worst play.
    /// </summary>
    /// <param name="inventory">Current inventory</param>
    /// <param name="visited">Currently visited nodes</param>
    private static void SpendObviousKeys(Inventory inventory, VertexHashSet visited)
    {
        foreach (var (key, doors) in visited.Graph.Doors)
        {
            int lockedDoorCount = doors.Count(d => !inventory.Has(d.Key));

            // If all the doors are already opened, we don't have anything to do
            if (lockedDoorCount == 0)
                continue;

            int keyCount = inventory.GetCount(key);

            // If we have all the randomized keys, mark all the doors as unlockable and spend all the current keys
            // as we would collect the fixed keys while exploring the rest of the dungeon if needed.
            int uncollectedFixedKeys = visited.Graph.FixedKeys[key].Count(v => !visited.Contains(v));
            if (keyCount + uncollectedFixedKeys >= lockedDoorCount)
            {
                _logger.LogTrace("Opening all doors with key {Key}", key);
                foreach (var door in doors)
                {
                    if (!inventory.Has(door.Key))
                        inventory.AddItem(door.Key);
                }
                if (keyCount > 0)
                    inventory.RemoveItem(key, keyCount);
                continue;
            }

            // If we can see both sides of a door, spend a key.
            foreach (var door in doors)
            {
                if (keyCount == 0)
                    break;

                if (inventory.Has(door.Key))
                    continue;

                foreach (var (a, b) in door.Value)
                {
                    if (visited.Contains(a) && visited.Contains(b))
                    {
                        inventory.AddItem(door.Key);
                        inventory.RemoveItem(key, 1);
                        keyCount--;
                        break;
                    }
                }
            }

        }
    }

    private static bool CollectItems(Inventory inventory, VertexHashSet visited, VertexHashSet collected)
    {
        bool newItemsFound = false;
        var newlyVisited = visited.Clone();
        newlyVisited.ExceptWith(collected);

        IWorld? world = null;
        foreach (var itemLocation in newlyVisited)
        {
            collected.Add(itemLocation);
            if (itemLocation.Item is not null)
            {
                newItemsFound = true;
                inventory.AddItem(itemLocation.Item);
                world ??= itemLocation.Item.World;
            }
            if (itemLocation.Trophy is not null)
            {
                newItemsFound = true;
                inventory.AddItem(itemLocation.Trophy);
                world ??= itemLocation.Trophy.World;
            }

            if (newItemsFound && world is not null && inventory.Has(world.GetItem("BigRedBomb")))
            {
                var activeBomb = world.GetItem("BigRedBombActive");
                if (!inventory.Has(activeBomb) && DropOffSearch(world, inventory))
                {
                    inventory.AddItem(activeBomb);
                    newItemsFound = true;
                }
            }
        }

        return newItemsFound;
    }

    public bool HasFound(IItem item)
    {
        return _inventory.Has(item);
    }

    /// <summary>
    /// Get all vertices that were visited in a given search (which has been called first) from a set starting point.
    /// </summary>
    public IEnumerable<Vertex> GetVisited()
    {
        return _visited;
    }

    public bool HasVisited(Vertex vertex)
    {
        return _visited.Contains(vertex);
    }

    /// <summary>
    /// Basic graph searcher. Returns a set of vertices that are absolutely reachable from the given starting points.
    /// Will go through open doors.
    /// </summary>
    /// <param name="collected">Items to use in search, no collecting here</param>
    /// <param name="visited">Locations we believe we have visited before</param>
    /// <param name="startAt">Listy of starting Vertices to search from</param>
    /// <returns>
    /// Returns the list of new reachable nodes and nodes with remaining accessible regions.
    /// </returns>
    private static SearchResult InternalSearch(Inventory collected, VertexHashSet visited, IEnumerable<Vertex> startAt)
    {
        SpendObviousKeys(collected, visited);

        var newlyVisited = new VertexHashSet(visited.Graph);
        var newSearchStarts = new VertexHashSet(visited.Graph);
        var marked = new VertexHashSet(visited.Graph);
        var queue = new Queue<Vertex>();
        foreach (var start in startAt)
        {
            if (!visited.Contains(start))
            {
                marked.Add(start);
                newlyVisited.Add(start);
            }
            queue.Enqueue(start);
        }

        while (queue.TryDequeue(out var vertex))
        {
            int unvisitedEdges = vertex.Edges.Count;

            foreach (var edge in CollectionsMarshal.AsSpan(vertex.Edges))
            {
                if (!edge.Condition.IsUnconditional)
                {
                    if (!collected.Has(edge.Condition))
                        continue;
                }

                unvisitedEdges--;
                if (!marked.Contains(edge.To))
                {
                    marked.Add(edge.To);
                    queue.Enqueue(edge.To);
                }
            }

            // We could remove nodes from newSearchStarts when we have visited
            // all the edges, but the affected nodes are few and it's more
            // work than time saved overall.
            if (unvisitedEdges > 0)
                newSearchStarts.Add(vertex);

            if (!visited.Contains(vertex))
                newlyVisited.Add(vertex);
        }

        return (newlyVisited, newSearchStarts);
    }
    private bool DoorSearch(Inventory inventory)
    {
        var strongLocations = new VertexHashSet(_graph);
        var strongSearchStarts = new VertexHashSet(_graph);
        foreach (var (key, edges) in _graph.Doors)
        {
            int keyCount = inventory.GetCount(key);
            if (keyCount == 0)
                continue;

            // Try using DungeonKeySolver for single-dungeon scenarios
            var dungeonSearchResult = TryDungeonKeySolverSearch(inventory, key, _visited, _collected);
            if (dungeonSearchResult.HasValue)
            {
                var (dungeonLocations, dungeonSearchStarts) = dungeonSearchResult.Value;
                strongLocations.UnionWith(dungeonLocations);
                strongSearchStarts.UnionWith(dungeonSearchStarts);
            }
            else
            {
                // Fall back to recursive search for complex/cross-dungeon scenarios
                var (recursiveLocations, recursiveSearchStarts) = RecursiveDoorSearchInternal(inventory, key, _visited, _collected);
                strongLocations.UnionWith(recursiveLocations);
                strongSearchStarts.UnionWith(recursiveSearchStarts);
            }
        }

        _visited.UnionWith(strongLocations);
        _searchStarts.UnionWith(strongSearchStarts);
        bool foundItems = CollectItems(inventory, _visited, _collected);

        return strongLocations.Count != 0 || foundItems;
    }

    private static SearchResult RecursiveDoorSearchInternal(Inventory inventory, IItem key, VertexHashSet visitedBeforeDoors, VertexHashSet collectedBeforeDoors, params Vertex[] additionalStarts)
    {
        if (inventory.GetCount(key) == 0)
            return InternalSearch(inventory, visitedBeforeDoors, additionalStarts);

        inventory = inventory.Clone();

        VertexHashSet? strongLocations = null;
        VertexHashSet? strongSearchStarts = null;
        var visitedBeforeRecursion = visitedBeforeDoors.Clone();
        var collectedBeforeRecursion = collectedBeforeDoors.Clone();

        foreach (var door in visitedBeforeDoors.Graph.Doors[key])
        {
            // Skip the door if it's already been opened
            if (inventory.Has(door.Key))
                continue;

            List<Vertex> newVerticesFromDoor = new();
            foreach (var (a, b) in door.Value)
            {
                bool seenA = visitedBeforeDoors.Contains(a);
                bool seenB = visitedBeforeDoors.Contains(b);
                if (seenA != seenB)
                    newVerticesFromDoor.Add(seenA ? b : a);
            }
            if (newVerticesFromDoor.Count == 0)
                continue;

            var inventoryForIteration = inventory.Clone();

            // Open the door, consume a key
            inventoryForIteration.AddItem(door.Key);
            inventoryForIteration.RemoveItem(key);

            // Check what's behind the door
            Vertex[] startAt = [.. newVerticesFromDoor, .. additionalStarts];
            var weakLocations = new VertexHashSet(visitedBeforeRecursion.Graph);
            var weakSearchStarts = new VertexHashSet(visitedBeforeRecursion.Graph);
            do
            {
                var (weakLocations2, weakSearchStarts2) = InternalSearch(inventoryForIteration, visitedBeforeRecursion, startAt);

                visitedBeforeRecursion.UnionWith(weakLocations2);
                weakLocations.UnionWith(weakLocations2);
                weakSearchStarts = weakSearchStarts2;
            } while (CollectItems(inventoryForIteration, visitedBeforeRecursion, collectedBeforeRecursion));
            if (inventoryForIteration.GetCount(key) > 0)
            {
                var (recursiveLocations, recursiveSearchStarts) = RecursiveDoorSearchInternal(inventoryForIteration, key, visitedBeforeRecursion, collectedBeforeRecursion, [.. startAt, .. weakSearchStarts]);
                weakLocations.UnionWith(recursiveLocations);
                weakSearchStarts.UnionWith(recursiveSearchStarts);
            }
            // reset
            visitedBeforeRecursion.IntersectWith(visitedBeforeDoors);
            collectedBeforeRecursion.IntersectWith(collectedBeforeDoors);


            if (weakLocations.Count == 0)
                return (new VertexHashSet(visitedBeforeDoors.Graph), new VertexHashSet(visitedBeforeDoors.Graph));

            if (strongLocations != null && strongSearchStarts != null)
            {
                strongLocations.IntersectWith(weakLocations);
                strongSearchStarts.IntersectWith(weakSearchStarts);
            }
            else
            {
                strongLocations = weakLocations;
                strongSearchStarts = weakSearchStarts;
            }
        }

        return (strongLocations ?? new VertexHashSet(visitedBeforeDoors.Graph), strongSearchStarts ?? new VertexHashSet(visitedBeforeDoors.Graph));
    }

    /// <summary>
    /// Try to use DungeonKeySolver for more efficient key search in single-dungeon scenarios.
    /// Returns null if the scenario is too complex for DungeonKeySolver (cross-dungeon, etc.)
    /// Optimized with caching and early bailouts for performance.
    /// </summary>
    private static SearchResult? TryDungeonKeySolverSearch(Inventory inventory, IItem key, VertexHashSet visitedBeforeDoors, VertexHashSet collectedBeforeDoors)
    {
        try
        {
            // Early bailout: Only try for ALttP small keys that match expected patterns
            if (!IsOptimizableKey(key.Name))
                return null;

            // Detect if this is a single-dungeon scenario by examining the doors
            var doors = visitedBeforeDoors.Graph.Doors[key];
            if (doors.Count == 0)
                return null;

            // Quick dungeon detection with early bailout on cross-dungeon scenarios
            string? dungeonName = null;
            int doorCheckCount = 0;
            const int maxDoorChecks = 3; // Limit checks for performance

            foreach (var door in doors)
            {
                if (++doorCheckCount > maxDoorChecks) break; // Early bailout for large door sets

                foreach (var (a, b) in door.Value)
                {
                    var dungeonFromA = GetDungeonName(a);
                    var dungeonFromB = GetDungeonName(b);
                    
                    if (dungeonFromA != null)
                    {
                        if (dungeonName == null)
                            dungeonName = dungeonFromA;
                        else if (dungeonName != dungeonFromA)
                            return null; // Cross-dungeon scenario, immediate fallback
                    }
                    
                    if (dungeonFromB != null)
                    {
                        if (dungeonName == null)
                            dungeonName = dungeonFromB;
                        else if (dungeonName != dungeonFromB)
                            return null; // Cross-dungeon scenario, immediate fallback
                    }
                }
            }

            if (dungeonName == null)
                return null; // Cannot determine dungeon context

            // Use cached dungeon graph if available
            var cacheKey = (visitedBeforeDoors.Graph, dungeonName);
            if (!_dungeonGraphCache.TryGetValue(cacheKey, out var dungeonGraph))
            {
                // Extract dungeon graph and cache it
                dungeonGraph = DungeonGraphConverter.ExtractDungeonGraph(visitedBeforeDoors.Graph, dungeonName, key.Name);
                _dungeonGraphCache[cacheKey] = dungeonGraph;
            }

            if (dungeonGraph.Nodes.Count == 0)
                return null; // No valid dungeon graph

            // Find entrance nodes (nodes that are currently visited/reachable from outside)
            var entranceNodeIds = new List<int>();
            var graphVerticesById = CreateVertexLookup(visitedBeforeDoors.Graph); // Cache vertex lookup
            
            foreach (var node in dungeonGraph.Nodes)
            {
                if (graphVerticesById.TryGetValue(node.Id, out var vertex) && visitedBeforeDoors.Contains(vertex))
                {
                    entranceNodeIds.Add(node.Id);
                }
            }

            if (entranceNodeIds.Count == 0)
                return null; // No accessible entrances

            // Create item check function based on current inventory
            var allItemsById = CreateItemLookup(visitedBeforeDoors.Graph); // Cache item lookup
            Func<string, bool> itemCheck = req =>
            {
                if (req == "fixed" || req == "KEY") return true;
                return allItemsById.TryGetValue(req, out var requiredItem) && inventory.Has(requiredItem);
            };

            // Get available keys
            int availableKeys = inventory.GetCount(key);

            // Use DungeonKeySolver to find safe locations
            var safeNodeIds = DungeonKeySolverStatic.SafeItemLocationsForDungeon(
                dungeonGraph, dungeonName, entranceNodeIds, itemCheck, availableKeys);

            // Convert back to vertices
            var safeLocations = new VertexHashSet(visitedBeforeDoors.Graph);
            var safeSearchStarts = new VertexHashSet(visitedBeforeDoors.Graph);

            foreach (var nodeId in safeNodeIds)
            {
                if (graphVerticesById.TryGetValue(nodeId, out var vertex))
                {
                    safeLocations.Add(vertex);
                    // For search starts, include vertices with outgoing edges that aren't in our safe set
                    if (vertex.Edges.Any(e => !safeNodeIds.Contains(e.To.Id)))
                    {
                        safeSearchStarts.Add(vertex);
                    }
                }
            }

            return (safeLocations, safeSearchStarts);
        }
        catch (Exception ex)
        {
            // Log the error in a production environment and fall back to recursive search
            _logger.LogWarning("DungeonKeySolver failed for key {Key}: {Message}", key.Name, ex.Message);
            return null;
        }
    }
    
    /// <summary>
    /// Check if a key name is optimizable by DungeonKeySolver (ALttP small keys)
    /// </summary>
    private static bool IsOptimizableKey(string keyName)
    {
        return keyName switch
        {
            "KeyP1" or "KeyP2" or "KeyP3" or // Light World dungeons
            "KeyD1" or "KeyD2" or "KeyD3" or "KeyD4" or "KeyD5" or "KeyD6" or "KeyD7" or // Dark World dungeons
            "KeyA2" => true, // Ganon's Tower
            _ => false
        };
    }
    
    /// <summary>
    /// Create an efficient lookup dictionary for vertices by ID
    /// </summary>
    private static Dictionary<int, Vertex> CreateVertexLookup(Graph graph)
    {
        var lookup = new Dictionary<int, Vertex>();
        foreach (var vertex in graph.GetVertices())
        {
            lookup[vertex.Id] = vertex;
        }
        return lookup;
    }
    
    /// <summary>
    /// Create an efficient lookup dictionary for items by name
    /// </summary>
    private static Dictionary<string, IItem> CreateItemLookup(Graph graph)
    {
        var lookup = new Dictionary<string, IItem>();
        foreach (var item in graph.AllItems)
        {
            lookup[item.Name] = item;
        }
        return lookup;
    }

    /// <summary>
    /// Extract dungeon name from a vertex's ItemSet with caching for performance
    /// </summary>
    private static string? GetDungeonName(Vertex vertex)
    {
        // Use cache to avoid repeated ItemSet iteration
        if (_vertexDungeonCache.TryGetValue(vertex, out var cachedDungeon))
            return cachedDungeon;

        // Look for dungeon-specific ItemSet entries
        string? dungeonName = null;
        foreach (var itemSet in vertex.ItemSet)
        {
            if (itemSet.World != null && IsDungeonName(itemSet.Name))
            {
                dungeonName = itemSet.Name;
                break; // Early exit on first match
            }
        }
        
        // Cache the result
        _vertexDungeonCache[vertex] = dungeonName;
        return dungeonName;
    }

    /// <summary>
    /// Check if a name corresponds to a known ALttP dungeon
    /// </summary>
    private static bool IsDungeonName(string name)
    {
        return name switch
        {
            "escape" or "eastern" or "desert" or "hera" or "agahnim" or 
            "pod" or "swamp" or "skull" or "thieves" or "ice" or "mire" or 
            "turtlerock" or "gt" => true,
            _ => false
        };
    }

    private static readonly string[] _noBombFollowerItems = ["hop", "Flippers", "DarkFlippers"];
    private static bool DropOffSearch(IWorld world, Inventory inventory)
    {
        var inventoryWithBombInTow = inventory.Clone();
        foreach (string item in _noBombFollowerItems)
        {
            var itemToRemove = world.GetItem(item);
            if (inventoryWithBombInTow.Has(itemToRemove))
                inventoryWithBombInTow.RemoveItem(itemToRemove);
        }
        var (newlyVisited, newSearchStarts) = InternalSearch(inventoryWithBombInTow, new(world.Graph), new[] { world.GetLocation("Bomb Shoppe Lobby") });
        return newlyVisited.Contains(world.GetLocation("Pyramid"));
    }

    /// <summary>
    /// Get a set of Locations without items that match the given itemSet. Available counts in itemSets is required.
    /// </summary>
    ///
    /// <param name="itemSet">constrain results to item set</param>
    /// <param name="itemSets">counts of items required in each sett</param>
    /// <param name="onlyReachable">only return reachable locations</param>
    public IEnumerable<Vertex> GetEmptyLocationsInSet(ItemSetName itemSet, Dictionary<ItemSetName, int>? itemSets = null, bool onlyReachable = true)
    {
        var emptyLocations = _setLocations[itemSet].Where((vertex) =>
        {
            return (!onlyReachable || _visited.Contains(vertex)) && vertex.Item == null;
        }).OrderBy(v => v.Name).ToList();

        itemSets ??= new();
        foreach (var (setName, setCount) in itemSets)
        {
            if (setName.World == null)
                continue;

            var setLocations = _setLocations[setName].Where(static (location) => location.Item == null);
            if (setLocations.Count() < setCount)
                throw new Exception($"Not enough set locations available: {setName}");
            // if a set has the same number of items to place as set locations
            // left, remove it from this return.
            if (itemSet != setName && setLocations.Count() == setCount)
                emptyLocations.RemoveAll(setLocations.Contains);
        }

        return emptyLocations.ToArray();
    }
}
