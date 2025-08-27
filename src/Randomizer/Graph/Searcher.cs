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
    
    // Streamlined caching for performance-critical paths only
    private static readonly Dictionary<string, bool> _optimizableKeyCache = new();
    private static readonly Dictionary<(Inventory, IItem, VertexHashSet, VertexHashSet, int), SearchResult> _recursiveResultCache = new();
    private static int _cacheHits = 0;
    private static int _cacheMisses = 0;

    /// <summary>
    /// Clear all caches to free memory and handle graph changes
    /// </summary>
    public static void ClearPerformanceCaches()
    {
        _optimizableKeyCache.Clear();
        _recursiveResultCache.Clear();
        _cacheHits = 0;
        _cacheMisses = 0;
        DungeonGraphConverter.ClearCaches();
    }

    /// <summary>
    /// Get cache statistics for performance monitoring
    /// </summary>
    public static (int hits, int misses, double hitRatio) GetCacheStats()
    {
        var total = _cacheHits + _cacheMisses;
        var hitRatio = total > 0 ? (double)_cacheHits / total : 0.0;
        return (_cacheHits, _cacheMisses, hitRatio);
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

            // Use optimized approach for expensive scenarios only
            SearchResult searchResult;
            
            if (ShouldUseOptimizedPath(key.Name, keyCount))
            {
                // Try optimized DungeonKeySolver for complex scenarios only
                var optimizedResult = TryOptimizedDoorSearch(inventory, key, _visited, _collected);
                if (optimizedResult.HasValue)
                {
                    searchResult = optimizedResult.Value;
                }
                else
                {
                    // Fallback to enhanced recursive search with memoization
                    searchResult = RecursiveDoorSearchInternal(inventory, key, _visited, _collected);
                }
            }
            else
            {
                // Use enhanced recursive search for most cases
                searchResult = RecursiveDoorSearchInternal(inventory, key, _visited, _collected);
            }
            
            var (locations, searchStarts) = searchResult;
            strongLocations.UnionWith(locations);
            strongSearchStarts.UnionWith(searchStarts);
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

        // Memoization: Check cache for this exact scenario
        var cacheKey = (inventory, key, visitedBeforeDoors, collectedBeforeDoors, additionalStarts.Length);
        if (_recursiveResultCache.TryGetValue(cacheKey, out var cachedResult))
        {
            _cacheHits++;
            return cachedResult;
        }
        _cacheMisses++;

        inventory = inventory.Clone();

        VertexHashSet? strongLocations = null;
        VertexHashSet? strongSearchStarts = null;
        var visitedBeforeRecursion = visitedBeforeDoors.Clone();
        var collectedBeforeRecursion = collectedBeforeDoors.Clone();

        var doors = visitedBeforeDoors.Graph.Doors[key];
        var remainingDoors = doors.Where(door => !inventory.Has(door.Key)).ToList();
        
        // Enhanced early termination conditions
        int availableKeys = inventory.GetCount(key);
        if (remainingDoors.Count == 0)
        {
            // No doors to open, return empty result immediately
            var emptyResult = (new VertexHashSet(visitedBeforeDoors.Graph), new VertexHashSet(visitedBeforeDoors.Graph));
            if (_recursiveResultCache.Count < 1000)
                _recursiveResultCache[cacheKey] = emptyResult;
            return emptyResult;
        }
        
        // Intelligent pruning: If we have too many doors relative to keys, consider selective optimization
        if (remainingDoors.Count > availableKeys + 3) // Allow some room for static keys
        {
            // For very complex scenarios, try to use a simplified heuristic approach
            var accessibleDoors = remainingDoors.Where(door => 
                door.Value.Any(pair => 
                    visitedBeforeDoors.Contains(pair.Item1) || visitedBeforeDoors.Contains(pair.Item2))).ToList();
            
            if (accessibleDoors.Count > availableKeys + 2)
            {
                // Too complex, return conservative empty result
                var emptyResult = (new VertexHashSet(visitedBeforeDoors.Graph), new VertexHashSet(visitedBeforeDoors.Graph));
                if (_recursiveResultCache.Count < 1000)
                    _recursiveResultCache[cacheKey] = emptyResult;
                return emptyResult;
            }
            
            // Use only accessible doors for optimization
            remainingDoors = accessibleDoors;
        }

        foreach (var door in remainingDoors)
        {
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

            // Check what's behind the door with early bailout
            Vertex[] startAt = [.. newVerticesFromDoor, .. additionalStarts];
            var weakLocations = new VertexHashSet(visitedBeforeRecursion.Graph);
            var weakSearchStarts = new VertexHashSet(visitedBeforeRecursion.Graph);
            
            // Limit iterations to prevent excessive computation
            int iterationLimit = 20;
            int iterations = 0;
            
            do
            {
                var (weakLocations2, weakSearchStarts2) = InternalSearch(inventoryForIteration, visitedBeforeRecursion, startAt);

                visitedBeforeRecursion.UnionWith(weakLocations2);
                weakLocations.UnionWith(weakLocations2);
                weakSearchStarts = weakSearchStarts2;
                
                iterations++;
                if (iterations >= iterationLimit) break; // Prevent runaway computation
                
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
            {
                var emptyResult = (new VertexHashSet(visitedBeforeDoors.Graph), new VertexHashSet(visitedBeforeDoors.Graph));
                _recursiveResultCache[cacheKey] = emptyResult;
                return emptyResult;
            }

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

        var result = (strongLocations ?? new VertexHashSet(visitedBeforeDoors.Graph), strongSearchStarts ?? new VertexHashSet(visitedBeforeDoors.Graph));
        
        // Cache the result if the cache isn't too large
        if (_recursiveResultCache.Count < 1000)
        {
            _recursiveResultCache[cacheKey] = result;
        }
        
        return result;
    }

    /// <summary>
    /// Decide whether to use optimized path based on complexity heuristics
    /// </summary>
    private static bool ShouldUseOptimizedPath(string keyName, int keyCount)
    {
        // Only use optimized path for ALttP small keys
        if (!IsOptimizableKey(keyName))
            return false;
            
        // Use optimization for scenarios likely to benefit:
        // 1. Multiple keys (exponential recursion)
        // 2. Complex dungeons (many doors)
        if (keyCount >= 2)
            return true;
            
        // For single keys, only optimize complex dungeons
        return keyName switch
        {
            "KeyD3" or "KeyD7" or "KeyA2" => true, // Skull Woods, Turtle Rock, Ganon's Tower
            _ => false
        };
    }

    /// <summary>
    /// Simplified optimized door search for complex scenarios only
    /// </summary>
    private static SearchResult? TryOptimizedDoorSearch(Inventory inventory, IItem key, VertexHashSet visitedBeforeDoors, VertexHashSet collectedBeforeDoors)
    {
        try
        {
            var keyName = key.Name;
            
            // Quick check: only proceed for complex scenarios
            int keyCount = inventory.GetCount(key);
            if (keyCount < 2) // Only optimize multi-key scenarios
                return null;

            // Simplified dungeon detection for ALttP dungeons
            var dungeonName = GetDungeonForKey(keyName);
            if (dungeonName == null)
                return null;

            // Extract dungeon graph (simplified, no heavy caching)
            var dungeonGraph = DungeonGraphConverter.ExtractDungeonGraph(visitedBeforeDoors.Graph, dungeonName, keyName);
            if (dungeonGraph.Nodes.Count == 0)
                return null;

            // Find entrance nodes
            var entranceNodeIds = dungeonGraph.Nodes
                .Where(node => visitedBeforeDoors.Any(v => v.Id == node.Id))
                .Select(node => node.Id)
                .ToList();

            if (entranceNodeIds.Count == 0)
                return null;

            // Simple item check
            Func<string, bool> itemCheck = req =>
            {
                if (req == "fixed" || req == "KEY") return true;
                var requiredItem = visitedBeforeDoors.Graph.AllItems.FirstOrDefault(item => item.Name == req);
                return requiredItem != null && inventory.Has(requiredItem);
            };

            // Use DungeonKeySolver for complex scenarios
            var solver = new DungeonKeySolver(dungeonGraph, dungeonName);
            var safeNodeIds = solver.SafeItemLocations(itemCheck, keyCount, entranceNodeIds);

            // Convert back to vertices
            var safeLocations = new VertexHashSet(visitedBeforeDoors.Graph);
            var safeSearchStarts = new VertexHashSet(visitedBeforeDoors.Graph);

            foreach (var nodeId in safeNodeIds)
            {
                var vertex = visitedBeforeDoors.Graph.GetVertices().FirstOrDefault(v => v.Id == nodeId);
                if (vertex != null)
                {
                    safeLocations.Add(vertex);
                    if (vertex.Edges.Any(edge => !safeNodeIds.Contains(edge.To.Id)))
                    {
                        safeSearchStarts.Add(vertex);
                    }
                }
            }

            return (safeLocations, safeSearchStarts);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Optimized door search failed for key {Key}: {Message}", key.Name, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Get dungeon name for a key (simplified mapping)
    /// </summary>
    private static string? GetDungeonForKey(string keyName)
    {
        return keyName switch
        {
            "KeyP1" => "eastern",
            "KeyP2" => "desert", 
            "KeyP3" => "hera",
            "KeyD1" => "pod",
            "KeyD2" => "swamp",
            "KeyD3" => "skull",
            "KeyD4" => "thieves",
            "KeyD5" => "ice",
            "KeyD6" => "mire",
            "KeyD7" => "turtlerock",
            "KeyA2" => "gt",
            _ => null
        };
    }
    
    /// <summary>
    /// Check if a key name is optimizable by DungeonKeySolver (ALttP small keys)
    /// Uses caching for repeated calls
    /// </summary>
    private static bool IsOptimizableKey(string keyName)
    {
        if (_optimizableKeyCache.TryGetValue(keyName, out var cached))
            return cached;

        var result = keyName switch
        {
            "KeyP1" or "KeyP2" or "KeyP3" or // Light World dungeons
            "KeyD1" or "KeyD2" or "KeyD3" or "KeyD4" or "KeyD5" or "KeyD6" or "KeyD7" or // Dark World dungeons
            "KeyA2" => true, // Ganon's Tower
            _ => false
        };
        
        _optimizableKeyCache[keyName] = result;
        return result;
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
