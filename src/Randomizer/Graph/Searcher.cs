namespace Randomizer.Graph;

using SearchResult = (VertexHashSet NewlyVisited, VertexHashSet NewSearchStarts);

public class Searcher
{
    private readonly VertexHashSet _visited;
    private readonly VertexHashSet _collected;
    private readonly Graph _graph;
    private readonly VertexHashSet _searchStarts;
    private readonly Inventory _inventory;

    public Searcher(Graph graph, Vertex start, Inventory inventory)
    {
        _graph = graph;
        _visited = new(graph);
        _collected = new(graph);
        _searchStarts = new(graph);
        _searchStarts.Add(start);
        _inventory = inventory;

        if (!_graph.HasVertex(start))
            throw new Exception("Start vertex not in graph");

        bool newItemsFound;
        do
        {
            var (newlyVisited, newSearchStarts) = InternalSearch(inventory, _visited, _searchStarts);
            _visited.UnionWith(newlyVisited);
            _searchStarts.Clear();
            _searchStarts.UnionWith(newSearchStarts);

            newItemsFound = CollectItems(inventory, _visited, _collected);
            if (RecursiveDoorSearch(inventory))
                newItemsFound = true;
        } while (newItemsFound);
    }

    private bool CollectItems(Inventory inventory, VertexHashSet visited, VertexHashSet collected)
    {
        bool newItemsFound = false;
        var newlyVisited = visited.Clone();
        newlyVisited.ExceptWith(collected);

        foreach (var itemLocation in newlyVisited)
        {
            bool foundNewItem = false;
            collected.Add(itemLocation);
            if (itemLocation.Item is not null)
            {
                foundNewItem = true;
                inventory.AddItem(itemLocation.Item);
            }
            if (itemLocation.Trophy is not null)
            {
                foundNewItem = true;
                inventory.AddItem(itemLocation.Trophy);
            }
            if (foundNewItem)
                newItemsFound = true;

            if (foundNewItem && itemLocation.Item is { } item)
            {
                if (item.Name.StartsWith("BigRedBomb") && DropOffSearch(item))
                    inventory.AddItem(item.World.GetItem("BigRedBombActive"));
                newItemsFound = true;
            }
        }

        return newItemsFound;
    }

    public bool HasFound(Item item) => _inventory.Has(item);

    /// <summary>
    /// Get all vertices that were visited in a given search (which has been called first) from a set starting point.
    /// </summary>
    public IEnumerable<Vertex> GetVisited()
    {
        return _visited;
    }

    private static SearchResult InternalSearch(Inventory collected, VertexHashSet visited, IEnumerable<Vertex> startAt)
    {
        var newlyVisited = new VertexHashSet(visited.Graph);
        var newSearchStarts = new VertexHashSet(visited.Graph);
        var marked = new VertexHashSet(visited.Graph);
        var pegMarked = new VertexHashSet(visited.Graph);
        var queue = new Queue<Vertex>();
        var peg_queue = new Queue<Vertex>();
        foreach (var start in startAt)
        {
            if (!visited.Contains(start))
            {
                marked.Add(start);
                newlyVisited.Add(start);
            }
            queue.Enqueue(start);
        }

        do
        {
            while (peg_queue.TryDequeue(out var vertex))
            {
                if (vertex.Switch && !marked.Contains(vertex))
                    queue.Enqueue(vertex);
                if (vertex.Peg == PegState.Orange)
                    continue;
                int unvisitedEdges = vertex.Edges.Count;
                foreach (var edge in vertex.Edges)
                {
                    if (!edge.Condition.IsUnconditional)
                    {
                        if (edge.Condition.Item.Type == ItemType.SmallKey)
                            continue;
                        if (!collected.Has(edge.Condition))
                            continue;
                    }

                    var next_vertex = edge.To;
                    unvisitedEdges--;
                    if (!pegMarked.Contains(next_vertex))
                        peg_queue.Enqueue(next_vertex);
                }
                if (unvisitedEdges > 0)
                    newSearchStarts.Add(vertex);
                if (!visited.Contains(vertex))
                    newlyVisited.Add(vertex);
                pegMarked.Add(vertex);
            }

            while (queue.TryDequeue(out var vertex))
            {
                if (vertex.Switch && !pegMarked.Contains(vertex))
                    peg_queue.Enqueue(vertex);
                if (vertex.Peg == PegState.Blue)
                    continue;

                int unvisitedEdges = vertex.Edges.Count;

                foreach (var edge in vertex.Edges)
                {
                    if (!edge.Condition.IsUnconditional)
                    {
                        if (edge.Condition.Item.Type == ItemType.SmallKey)
                            continue;
                        if (!collected.Has(edge.Condition))
                            continue;
                    }

                    var next_vertex = edge.To;

                    unvisitedEdges--;
                    if (!marked.Contains(next_vertex))
                        queue.Enqueue(next_vertex);
                }

                if (unvisitedEdges > 0)
                    newSearchStarts.Add(vertex);
                if (!visited.Contains(vertex))
                    newlyVisited.Add(vertex);
                marked.Add(vertex);
            }
        } while (queue.Any() || peg_queue.Any());

        return (newlyVisited, newSearchStarts);
    }
    private bool RecursiveDoorSearch(Inventory inventory)
    {
        var strongLocations = new VertexHashSet(_graph);
        var strongSearchStarts = new VertexHashSet(_graph);
        foreach (var (key, edges) in _graph.Doors)
        {
            int keyCount = inventory.GetCount(key);
            if (keyCount == 0)
                continue;

            if (keyCount + _graph.FixedKeys[key].Count(l => !_visited.Contains(l)) >= edges.Count)
            {
                // we have all keys, unlock everything.
                var behindDoorLocations = edges.Where(e => _visited.Contains(e.From) || _visited.Contains(e.To))
                    .SelectMany(e => new[] { e.From, e.To })
                    .ToHashSet();
                var (newlyVisited, newSearchStarts) = InternalSearch(inventory, _visited, behindDoorLocations);
                strongLocations.UnionWith(newlyVisited);
                strongSearchStarts.UnionWith(newSearchStarts);
            }
            else
            {
                var (recursiveLocations, recursiveSearchStarts) = RecursiveDoorSearchInternal(inventory, key, edges, keyCount, _visited, _collected);
                strongLocations.UnionWith(recursiveLocations);
                strongSearchStarts.UnionWith(recursiveSearchStarts);
            }
        }

        _visited.UnionWith(strongLocations);
        _searchStarts.UnionWith(strongSearchStarts);
        bool foundItems = CollectItems(inventory, _visited, _collected);

        return strongLocations.Any() || foundItems;
    }
    private SearchResult RecursiveDoorSearchInternal(Inventory inventory, Item key, HashSet<(Vertex From, Vertex To)> edges, int keyCount, VertexHashSet visitedBeforeDoors, VertexHashSet collectedBeforeDoors, params Vertex[] additionalStarts)
    {
        if (keyCount == 0)
            return InternalSearch(inventory, visitedBeforeDoors, additionalStarts);

        VertexHashSet? strongLocations = null;
        VertexHashSet? strongSearchStarts = null;
        var visitedBeforeRecursion = visitedBeforeDoors.Clone();
        var collectedBeforeRecursion = collectedBeforeDoors.Clone();

        foreach (var doorsForKey in _graph.Doors[key])
        {
            bool seenA = visitedBeforeDoors.Contains(doorsForKey.From);
            bool seenB = visitedBeforeDoors.Contains(doorsForKey.To);
            if (!seenA && !seenB) continue;
            if (seenA && seenB) continue;

            var to = seenA ? doorsForKey.To : doorsForKey.From;

            var inventoryForIteration = inventory.Clone();
            Vertex[] startAt = [to, .. additionalStarts];
            var (weakLocations, weakSearchStarts) = InternalSearch(inventoryForIteration, visitedBeforeRecursion, startAt);
            visitedBeforeRecursion.UnionWith(weakLocations);
            int keysBefore = inventoryForIteration.GetCount(key);
            // TODO: can we stop recursing here if we didn't find anything?
            CollectItems(inventoryForIteration, visitedBeforeRecursion, collectedBeforeRecursion);
            int keysAfter = inventoryForIteration.GetCount(key);
            int keysFound = keysAfter - keysBefore;
            int keysUsed = 1 - keysFound;
            var (recursiveLocations, recursiveSearchStarts) = RecursiveDoorSearchInternal(inventoryForIteration, key, edges, keyCount - keysUsed, visitedBeforeRecursion, collectedBeforeRecursion, [.. startAt, .. weakSearchStarts]);
            // reset
            visitedBeforeRecursion.IntersectWith(visitedBeforeDoors);
            collectedBeforeRecursion.IntersectWith(collectedBeforeDoors);
            weakLocations.UnionWith(recursiveLocations);
            weakSearchStarts.UnionWith(recursiveSearchStarts);

            if (weakLocations.Count == 0)
                return (new VertexHashSet(_graph), new VertexHashSet(_graph));

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

        return (strongLocations ?? new VertexHashSet(_graph), strongSearchStarts ?? new VertexHashSet(_graph));
    }

    private bool DropOffSearch(Item item)
    {
        //var start = _vertices["Bomb Shoppe Lobby:" + item.World.Id];
        //var end = _vertices["Pyramid:" + item.World.Id];
        //visited.Contains(end);
        return true;
    }

    /// <summary>
    /// Get a set of Locations without items that match the given itemSet. Available counts in itemSets is required.
    /// </summary>
    ///
    /// <param name="itemSet">constrain results to item set</param>
    /// <param name="itemSets">counts of items required in each sett</param>
    /// <param name="reachable">reachable only return reachable locations</param>
    public IEnumerable<Vertex> GetEmptyLocationsInSet(string itemSet = "*", Dictionary<string, int>? itemSets = null, bool reachable = true)
    {
        var empty_locations = _graph.GetSetLocations(itemSet).Where((vertex) =>
        {
            return (!reachable || _visited.Contains(vertex)) && vertex.Item == null;
        }).ToList();

        itemSets ??= new();
        foreach (var (set_name, set_count) in itemSets)
        {
            if (set_name == "*")
                continue;

            var set_locations = _graph.GetSetLocations(set_name).Where(static (location) => location.Item == null);
            if (set_locations.Count() < set_count)
                throw new Exception($"Not enough set locations available: {set_name}");
            // if a set has the same number of items to place as set locations
            // left, remove it from this return.
            if (itemSet != set_name && set_locations.Count() == set_count)
                empty_locations.RemoveAll(set_locations.Contains);
        }

        return empty_locations.ToArray();
    }
}
