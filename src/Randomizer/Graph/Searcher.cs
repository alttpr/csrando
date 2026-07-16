namespace Randomizer.Graph;

using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using SearchResult = (VertexHashSet NewlyVisited, VertexHashSet NewSearchStarts);

public class Searcher : ISearcher
{
    private static readonly ILogger _logger = ClassLogger.Get();

    private readonly VertexHashSet _visited;
    private readonly VertexHashSet _collected;
    private readonly Graph _graph;
    private readonly VertexHashSet _searchStarts;
    private readonly Inventory _inventory;
    private readonly SetLocations _setLocations;
    private readonly VertexHashSet _otherWorldLocations;
    private readonly IWorld? _world;
    private readonly Func<Vertex, bool> _collectItemAt;

    // Scratch structures reused across InternalSearch/CollectItems calls (single-threaded, and
    // neither method is reentered while the other holds its scratch), so each BFS pass does not
    // allocate a marker set, a queue, and a visited-minus-collected clone.
    private readonly VertexHashSet _scratchMarked;
    private readonly Queue<Vertex> _scratchQueue = new();
    private readonly VertexHashSet _scratchUncollected;

    // Last (visited count, inventory version) each key type's door search ran against; both
    // only grow, so an unchanged pair means a rerun would contribute nothing new.
    private readonly Dictionary<IItem, (int VisitedCount, int InventoryVersion)> _doorSearchCache = new();

    /// <summary>
    /// I'm a jerk and don't like useful messages.
    /// </summary>
    /// <param name="graph">The graph to search</param>
    /// <param name="start">The starting point to search from</param>
    /// <param name="inventory">The current inventory to use while searching</param>
    public Searcher(Graph graph, Vertex start, Inventory inventory, SetLocations? setLocations = null,
        IWorld? world = null, Func<Vertex, bool>? collectItemAt = null)
    {
        _world = world;
        _collectItemAt = collectItemAt ?? (_ => true);
        _graph = graph;
        _visited = new(graph);
        _collected = new(graph);
        _searchStarts = new(graph) { start };
        _inventory = inventory;
        _setLocations = setLocations ?? new();
        _otherWorldLocations = new(graph);
        _scratchMarked = new(graph);
        _scratchUncollected = new(graph);

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

            if (_world == null || _world is Games.Alttp.World)
            {
                if (DoorSearch(inventory))
                    newItemsFound = true;
            }
        } while (newItemsFound);
    }

    public void ResumeSearch(IEnumerable<Vertex> startAt, Inventory prevInventory)
    {
        foreach (var vertex in startAt)
        {
            if (!_visited.Contains(vertex))
                _searchStarts.Add(vertex);
        }

        bool newItemsFound;
        do
        {
            do
            {
                var (newlyVisited, newSearchStarts) = InternalSearch(_inventory, _visited, _searchStarts);
                _visited.UnionWith(newlyVisited);
                _searchStarts.Clear();
                _searchStarts.UnionWith(newSearchStarts);

                newItemsFound = CollectItems(_inventory, _visited, _collected);
            } while (newItemsFound);

            if (DoorSearch(_inventory))
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
            // Plain loops instead of LINQ Count(predicate): this method runs at the top of
            // every InternalSearch, so closure allocations here add up.
            int lockedDoorCount = 0;
            foreach (var door in doors)
            {
                if (!inventory.Has(door.Key))
                    lockedDoorCount++;
            }

            // If all the doors are already opened, we don't have anything to do
            if (lockedDoorCount == 0)
                continue;

            int keyCount = inventory.GetCount(key);

            // If we have all the randomized keys, mark all the doors as unlockable and spend all the current keys
            // as we would collect the fixed keys while exploring the rest of the dungeon if needed.
            int uncollectedFixedKeys = 0;
            foreach (var fixedKey in visited.Graph.FixedKeys[key])
            {
                if (!visited.Contains(fixedKey))
                    uncollectedFixedKeys++;
            }
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

    private bool CollectItems(Inventory inventory, VertexHashSet visited, VertexHashSet collected)
    {
        bool newItemsFound = false;
        var newlyVisited = _scratchUncollected;
        newlyVisited.CopyFromExcept(visited, collected);

        IWorld? world = null;
        foreach (var itemLocation in newlyVisited)
        {
            collected.Add(itemLocation);
            if (!_collectItemAt(itemLocation))
                continue;
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
    /// Get the inventory resolved by this search, including fixed graph events and
    /// synthetic door capabilities discovered while traversing the world.
    /// </summary>
    public Inventory GetInventory()
    {
        return _inventory.Clone();
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
    private SearchResult InternalSearch(Inventory collected, VertexHashSet visited, IEnumerable<Vertex> startAt)
    {
        if (_world == null || _world is Games.Alttp.World)
        {
            SpendObviousKeys(collected, visited);
        }

        var newlyVisited = new VertexHashSet(visited.Graph);
        var newSearchStarts = new VertexHashSet(visited.Graph);
        var marked = _scratchMarked;
        marked.Clear();
        var queue = _scratchQueue;
        queue.Clear();
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
                if(edge.To.World != vertex.World)
                {
                    _otherWorldLocations.Add(edge.To);                    
                    continue;
                }

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

            // Both inputs only grow, so if neither changed since this key's last run, a rerun
            // would return a result that is already unioned into the searcher state. The outer
            // fixpoint loop guarantees at least one such no-op round per key; skip it.
            var cacheEntry = (_visited.Count, inventory.Version);
            if (_doorSearchCache.TryGetValue(key, out var lastRun) && lastRun == cacheEntry)
                continue;

            var result = SubsetDoorSearch(inventory, key);

            _doorSearchCache[key] = cacheEntry;
            strongLocations.UnionWith(result.NewlyVisited);
            strongSearchStarts.UnionWith(result.NewSearchStarts);
        }

        _visited.UnionWith(strongLocations);
        _searchStarts.UnionWith(strongSearchStarts);
        bool foundItems = CollectItems(inventory, _visited, _collected);

        return strongLocations.Count != 0 || foundItems;
    }

    /// <summary>
    /// State of one key type's door search after opening a specific set of its doors:
    /// the exploration result is a function of the set alone, not the opening order.
    /// </summary>
    private sealed class DoorSubsetState
    {
        /// <summary>All vertices reachable with this door set open.</summary>
        public required VertexHashSet Visited;
        /// <summary>Vertices whose items have been collected while reaching this state.</summary>
        public required VertexHashSet Collected;
        /// <summary>Inventory including collected items and the spent keys.</summary>
        public required Inventory Inventory;
        /// <summary>Accumulated resume points (door frontiers and partially-explored vertices).</summary>
        public required VertexHashSet Starts;
        /// <summary>The last exploration pass's resume points, mirroring the legacy per-door weak starts.</summary>
        public required VertexHashSet LastFrontier;
    }

    /// <summary>
    /// Compute "strong" reachability for one key type: the intersection over all possible
    /// key-spending choices, so that no order of opening doors can soft-lock the player.
    /// Rather than enumerating every spending *order* (factorial in the door count), this
    /// explores each *set* of opened doors once — the state after opening a set of doors is
    /// order-independent — and computes the same intersection over the choices available in
    /// each state.
    /// </summary>
    private SearchResult SubsetDoorSearch(Inventory inventory, IItem key)
    {
        var doors = _graph.Doors[key];
        // The subset is tracked in a 64-bit mask; no real dungeon comes anywhere close.
        if (doors.Count > 63)
            throw new NotSupportedException($"More than 63 doors for a single key type ({key}) is not supported.");

        var doorItems = new IItem[doors.Count];
        var doorPairs = new HashSet<(Vertex A, Vertex B)>[doors.Count];
        int doorIndex = 0;
        foreach (var (unlockItem, pairs) in doors)
        {
            doorItems[doorIndex] = unlockItem;
            doorPairs[doorIndex] = pairs;
            doorIndex++;
        }

        // The entry state references the searcher's sets directly; they are only read here
        // (children clone before mutating).
        var states = new Dictionary<ulong, DoorSubsetState>
        {
            [0] = new DoorSubsetState
            {
                Visited = _visited,
                Collected = _collected,
                Inventory = inventory,
                Starts = new VertexHashSet(_graph),
                LastFrontier = new VertexHashSet(_graph),
            },
        };
        var solved = new Dictionary<ulong, SearchResult>();

        return Solve(0);

        // Returns the strong (guaranteed regardless of play order) newly-visited vertices and
        // resume points, relative to the state with this set of doors open.
        SearchResult Solve(ulong mask)
        {
            if (solved.TryGetValue(mask, out var cached))
                return cached;

            var state = states[mask];
            VertexHashSet? strongLocations = null;
            VertexHashSet? strongSearchStarts = null;

            for (int i = 0; i < doorItems.Length; i++)
            {
                ulong bit = 1UL << i;
                if ((mask & bit) != 0)
                    continue;

                // Skip the door if it's already been opened (directly, or as a side effect of
                // SpendObviousKeys during exploration).
                if (state.Inventory.Has(doorItems[i]))
                    continue;

                List<Vertex> newVerticesFromDoor = new();
                foreach (var (a, b) in doorPairs[i])
                {
                    bool seenA = state.Visited.Contains(a);
                    bool seenB = state.Visited.Contains(b);
                    if (seenA != seenB)
                        newVerticesFromDoor.Add(seenA ? b : a);
                }
                if (newVerticesFromDoor.Count == 0)
                    continue;

                var child = GetChild(mask | bit, state, i, newVerticesFromDoor);

                // Everything newly reachable after opening this door, relative to this state.
                var weakLocations = VertexHashSet.AndNot(child.Visited, state.Visited);
                var weakSearchStarts = child.LastFrontier.Clone();

                if (child.Inventory.GetCount(key) > 0)
                {
                    var (recursiveLocations, recursiveSearchStarts) = Solve(mask | bit);
                    weakLocations.UnionWith(recursiveLocations);
                    weakSearchStarts.UnionWith(recursiveSearchStarts);
                }

                if (weakLocations.Count == 0)
                {
                    SearchResult empty = (new VertexHashSet(_graph), new VertexHashSet(_graph));
                    solved[mask] = empty;
                    return empty;
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

            SearchResult result = (
                strongLocations ?? new VertexHashSet(_graph),
                strongSearchStarts ?? new VertexHashSet(_graph));
            solved[mask] = result;
            return result;
        }

        // Get (or compute once) the state after opening one more door from a parent state.
        DoorSubsetState GetChild(ulong childMask, DoorSubsetState parent, int doorToOpen, List<Vertex> newVerticesFromDoor)
        {
            if (states.TryGetValue(childMask, out var existing))
                return existing;

            var childInventory = parent.Inventory.Clone();
            childInventory.AddItem(doorItems[doorToOpen]);
            childInventory.RemoveItem(key);

            var childVisited = parent.Visited.Clone();
            var childCollected = parent.Collected.Clone();

            Vertex[] startAt = [.. newVerticesFromDoor, .. parent.Starts];
            var lastFrontier = new VertexHashSet(_graph);
            do
            {
                var (newlyVisited, newSearchStarts) = InternalSearch(childInventory, childVisited, startAt);
                childVisited.UnionWith(newlyVisited);
                lastFrontier = newSearchStarts;
            } while (CollectItems(childInventory, childVisited, childCollected));

            var childStarts = parent.Starts.Clone();
            foreach (var vertex in newVerticesFromDoor)
                childStarts.Add(vertex);
            childStarts.UnionWith(lastFrontier);

            var child = new DoorSubsetState
            {
                Visited = childVisited,
                Collected = childCollected,
                Inventory = childInventory,
                Starts = childStarts,
                LastFrontier = lastFrontier,
            };
            states[childMask] = child;
            return child;
        }
    }

    private static readonly string[] _noBombFollowerItems = ["hop", "Flippers", "DarkFlippers"];
    private bool DropOffSearch(IWorld world, Inventory inventory)
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

    public IEnumerable<Vertex> GetOtherWorld() => _otherWorldLocations;
}
