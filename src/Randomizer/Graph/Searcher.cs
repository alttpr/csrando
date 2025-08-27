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
    private Inventory _inventory;
    private readonly SetLocations _setLocations;
    private readonly Queue<Vertex> _queue = new();

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
        _searchStarts = new(graph);
        _setLocations = setLocations ?? new();
        Recompute(inventory, start);
    }

    public void Recompute(Inventory inventory, Vertex? start = null)
    {
        _inventory = inventory;
        _visited.Clear();
        _collected.Clear();
        _searchStarts.Clear();
        _searchStarts.Add(start ?? _graph.GetVertices().First());

        bool newItemsFound;
        do
        {
            do
            {
                var (newlyVisited, newSearchStarts) = InternalSearchWithQueue(_inventory, _visited, _searchStarts);
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
        // Only iterate vertices visited in this round, not the full set
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

    // Classic collection removed; optimized delta collection is the default.

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
    private SearchResult InternalSearchWithQueue(Inventory collected, VertexHashSet visited, IEnumerable<Vertex> startAt)
    {
        SpendObviousKeys(collected, visited);

        var newlyVisited = new VertexHashSet(visited.Graph);
        var newSearchStarts = new VertexHashSet(visited.Graph);
        var marked = new VertexHashSet(visited.Graph);
        _queue.Clear();
        foreach (var start in startAt)
        {
            if (!visited.Contains(start))
            {
                marked.Add(start);
                newlyVisited.Add(start);
            }
            _queue.Enqueue(start);
        }

        while (_queue.TryDequeue(out var vertex))
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
                    _queue.Enqueue(edge.To);
                }
            }

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

            var (recursiveLocations, recursiveSearchStarts) = RecursiveDoorSearchInternal(inventory, key, _visited, _collected);
            strongLocations.UnionWith(recursiveLocations);
            strongSearchStarts.UnionWith(recursiveSearchStarts);
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

        // Reusable temporaries to reduce allocations inside the loop
        var newVerticesFromDoor = new List<Vertex>(8);
        var weakLocations = new VertexHashSet(visitedBeforeRecursion.Graph);
        var weakSearchStarts = new VertexHashSet(visitedBeforeRecursion.Graph);

        foreach (var door in visitedBeforeDoors.Graph.Doors[key])
        {
            // Skip the door if it's already been opened
            if (inventory.Has(door.Key))
                continue;

            newVerticesFromDoor.Clear();
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
            weakLocations.Clear();
            weakSearchStarts.Clear();
            do
            {
                var (weakLocations2, weakSearchStarts2) = InternalSearch(
                    inventoryForIteration,
                    visitedBeforeRecursion,
                    EnumerateStarts(newVerticesFromDoor, additionalStarts)
                );

                visitedBeforeRecursion.UnionWith(weakLocations2);
                weakLocations.UnionWith(weakLocations2);
                weakSearchStarts = weakSearchStarts2;
            } while (CollectItems(inventoryForIteration, visitedBeforeRecursion, collectedBeforeRecursion));

            if (inventoryForIteration.GetCount(key) > 0)
            {
                var (recursiveLocations, recursiveSearchStarts) = RecursiveDoorSearchInternal(
                    inventoryForIteration,
                    key,
                    visitedBeforeRecursion,
                    collectedBeforeRecursion,
                    EnumerateStarts3(newVerticesFromDoor, additionalStarts, weakSearchStarts).ToArray()
                );
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

                // Early-exit: if nothing remains guaranteed reachable, deeper work is moot
                if (strongLocations.Count == 0)
                    return (new VertexHashSet(visitedBeforeDoors.Graph), new VertexHashSet(visitedBeforeDoors.Graph));
            }
            else
            {
                // Clone once to decouple from reusable weak* sets
                strongLocations = weakLocations.Clone();
                strongSearchStarts = weakSearchStarts.Clone();
            }
        }

        return (strongLocations ?? new VertexHashSet(visitedBeforeDoors.Graph), strongSearchStarts ?? new VertexHashSet(visitedBeforeDoors.Graph));
    }

    // Yield-concatenation helpers to avoid temporary arrays when combining start vertices
    private static IEnumerable<Vertex> EnumerateStarts(List<Vertex> first, Vertex[] second)
    {
        foreach (var v in first) yield return v;
        for (int i = 0; i < second.Length; i++) yield return second[i];
    }

    private static IEnumerable<Vertex> EnumerateStarts3(List<Vertex> first, Vertex[] second, IEnumerable<Vertex> third)
    {
        foreach (var v in first) yield return v;
        for (int i = 0; i < second.Length; i++) yield return second[i];
        foreach (var v in third) yield return v;
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
        // Build candidate list without ordering to reduce overhead
        var source = _setLocations[itemSet];
        var emptyLocations = new List<Vertex>(source.Count);
        for (int i = 0; i < source.Count; i++)
        {
            var vertex = source[i];
            if ((!onlyReachable || _visited.Contains(vertex)) && vertex.Item == null)
                emptyLocations.Add(vertex);
        }

        itemSets ??= new();
        if (itemSets.Count > 0 && emptyLocations.Count > 0)
        {
            // First pass: determine tight sets and validate availability
            List<ItemSetName>? tightSets = null;
            foreach (var (setName, setCount) in itemSets)
            {
                if (setName.World == null)
                    continue;

                int available = 0;
            var list = _setLocations[setName];
            for (int i = 0; i < list.Count; i++)
            {
                var loc = list[i];
                if (loc.Item == null)
                {
                    available++;
                    if (available > setCount) break;
                }
            }
                if (available < setCount)
                    throw new Exception($"Not enough set locations available: {setName}");
                if (!itemSet.Equals(setName) && available == setCount)
                {
                    tightSets ??= new List<ItemSetName>(4);
                    tightSets.Add(setName);
                }
            }

            if (tightSets != null)
            {
                // Build a membership bitset for union of all tight sets (empties only)
                var membership = new VertexHashSet(_graph);
                foreach (var ts in tightSets)
                {
                    var list = _setLocations[ts];
                    for (int i = 0; i < list.Count; i++)
                    {
                        var loc = list[i];
                        if (loc.Item == null) membership.Add(loc);
                    }
                }
                // Filter emptyLocations in place by removing members of the tight-set union
                int write = 0;
                for (int read = 0; read < emptyLocations.Count; read++)
                {
                    var v = emptyLocations[read];
                    if (!membership.Contains(v))
                        emptyLocations[write++] = v;
                }
                if (write < emptyLocations.Count)
                    emptyLocations.RemoveRange(write, emptyLocations.Count - write);
            }
        }

        return emptyLocations;
    }

    public int GetEmptyLocationsInSetCount(ItemSetName itemSet, Dictionary<ItemSetName, int>? itemSets = null, bool onlyReachable = true)
    {
        // Count empties in the primary set quickly
        var source = _setLocations[itemSet];
        int count = 0;
        for (int i = 0; i < source.Count; i++)
        {
            var vertex = source[i];
            if ((!onlyReachable || _visited.Contains(vertex)) && vertex.Item == null)
                count++;
        }

        if (count == 0)
            return 0;

        itemSets ??= new();
        if (itemSets.Count == 0)
            return count;

        // Identify tight sets (available == setCount) and validate availability
        List<ItemSetName>? tightSets = null;
        foreach (var (setName, setCount) in itemSets)
        {
            if (setName.World == null)
                continue;

            int available = 0;
            var list = _setLocations[setName];
            // Early exit when availability exceeds requirement
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Item == null)
                {
                    available++;
                    if (available > setCount) break;
                }
            }
            if (available < setCount)
                throw new Exception($"Not enough set locations available: {setName}");
            if (!itemSet.Equals(setName) && available == setCount)
            {
                tightSets ??= new List<ItemSetName>(4);
                tightSets.Add(setName);
            }
        }

        if (tightSets == null || tightSets.Count == 0)
            return count;

        // Build a union membership bitset for all tight sets (empties only)
        var membership = new VertexHashSet(_graph);
        for (int t = 0; t < tightSets.Count; t++)
        {
            var ts = tightSets[t];
            var list = _setLocations[ts];
            for (int i = 0; i < list.Count; i++)
            {
                var v = list[i];
                if (v.Item == null)
                    membership.Add(v);
            }
        }

        int filtered = 0;
        for (int i = 0; i < source.Count; i++)
        {
            var v = source[i];
            if ((!onlyReachable || _visited.Contains(v)) && v.Item == null)
            {
                if (!membership.Contains(v))
                    filtered++;
            }
        }
        return filtered;
    }

    public List<Vertex> GetEmptyLocationsInSetList(ItemSetName itemSet, Dictionary<ItemSetName, int>? itemSets = null, bool onlyReachable = true)
    {
        var source = _setLocations[itemSet];
        var emptyLocations = new List<Vertex>(source.Count);
        for (int i = 0; i < source.Count; i++)
        {
            var vertex = source[i];
            if ((!onlyReachable || _visited.Contains(vertex)) && vertex.Item == null)
                emptyLocations.Add(vertex);
        }

        itemSets ??= new();
        if (itemSets.Count > 0 && emptyLocations.Count > 0)
        {
            List<ItemSetName>? tightSets = null;
            foreach (var (setName, setCount) in itemSets)
            {
                if (setName.World == null)
                    continue;

                int available = 0;
            var list = _setLocations[setName];
            for (int i = 0; i < list.Count; i++)
            {
                var loc = list[i];
                if (loc.Item == null)
                {
                    available++;
                    if (available > setCount) break;
                }
            }
                if (available < setCount)
                    throw new Exception($"Not enough set locations available: {setName}");
                if (!itemSet.Equals(setName) && available == setCount)
                {
                    tightSets ??= new List<ItemSetName>(4);
                    tightSets.Add(setName);
                }
            }

            if (tightSets != null)
            {
                var membership = new VertexHashSet(_graph);
                foreach (var ts in tightSets)
                {
                    var list = _setLocations[ts];
                    for (int i = 0; i < list.Count; i++)
                    {
                        var loc = list[i];
                        if (loc.Item == null) membership.Add(loc);
                    }
                }
                int write = 0;
                for (int read = 0; read < emptyLocations.Count; read++)
                {
                    var v = emptyLocations[read];
                    if (!membership.Contains(v))
                        emptyLocations[write++] = v;
                }
                if (write < emptyLocations.Count)
                    emptyLocations.RemoveRange(write, emptyLocations.Count - write);
            }
        }

        return emptyLocations;
    }

    // Removed duplicate count helper in favor of GetEmptyLocationsInSetCount
}
