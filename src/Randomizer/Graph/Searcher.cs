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

    private bool CollectItems(Inventory inventory, VertexHashSet visited, VertexHashSet collected)
    {
        bool newItemsFound = false;
        var newlyVisited = visited.Clone();
        newlyVisited.ExceptWith(collected);

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

            var (recursiveLocations, recursiveSearchStarts) = RecursiveDoorSearchInternal(inventory, key, _visited, _collected);
            strongLocations.UnionWith(recursiveLocations);
            strongSearchStarts.UnionWith(recursiveSearchStarts);
        }

        _visited.UnionWith(strongLocations);
        _searchStarts.UnionWith(strongSearchStarts);
        bool foundItems = CollectItems(inventory, _visited, _collected);

        return strongLocations.Count != 0 || foundItems;
    }

    private SearchResult RecursiveDoorSearchInternal(Inventory inventory, IItem key, VertexHashSet visitedBeforeDoors, VertexHashSet collectedBeforeDoors, params Vertex[] additionalStarts)
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
