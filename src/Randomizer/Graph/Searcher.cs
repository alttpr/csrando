namespace Randomizer.Graph;
public class Searcher
{
    private readonly HashSet<Vertex> _visited = new();
    private readonly HashSet<Vertex> _collected = new();
    private readonly Graph _graph;
    private readonly Vertex _start;
    private readonly Inventory _inventory;
    private readonly Dictionary<Item, HashSet<(Vertex From, Vertex To)>> _doors = [];

    public Searcher(Graph graph, Vertex start, Inventory inventory)
    {
        _graph = graph;
        _start = start;
        _inventory = inventory;

        if (!_graph.HasVertex(_start))
        {
            throw new Exception("Start vertex not in graph");
        }

        FindDoors();

        bool newItemsFound;
        do
        {
            var newlyVisited = InternalSearch(inventory, _visited, _start);
            _visited.UnionWith(newlyVisited);

            newItemsFound = RecursiveDoorSearch(inventory);
            foreach (var itemLocation in _visited.Except(_collected))
            {
                bool foundNewItem = false;
                _collected.Add(itemLocation);
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
                    {
                        inventory.AddItem(item.World.GetItem("BigRedBombActive"));
                    }
                    newItemsFound = true;
                }
            }
        } while (newItemsFound);
    }

    public bool HasFound(Item item) => _inventory.Has(item);

    /// <summary>
    /// Get all vertices that were visited in a given search (which has been called first) from a set starting point.
    /// </summary>
    public IEnumerable<Vertex> GetVisited()
    {
        return _visited;
    }

    private static HashSet<Vertex> InternalSearch(Inventory collected, HashSet<Vertex> visited, params Vertex[] startAt)
    {
        var newlyVisited = new HashSet<Vertex>();
        var marked = new HashSet<Vertex>();
        var pegMarked = new HashSet<Vertex>();
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
                if (vertex.Switch)
                {
                    if (!marked.Contains(vertex))
                    {
                        queue.Enqueue(vertex);
                    }
                }
                if (vertex.Peg == PegState.Orange)
                {
                    continue;
                }
                foreach (var next_vertex in vertex.GetTargets(reachableWithoutKeys))
                {
                    if (!pegMarked.Contains(next_vertex))
                    {
                        peg_queue.Enqueue(next_vertex);
                    }
                }

                if (!visited.Contains(vertex))
                    newlyVisited.Add(vertex);
                pegMarked.Add(vertex);
            }

            while (queue.TryDequeue(out var vertex))
            {
                if (vertex.Switch)
                {
                    if (!pegMarked.Contains(vertex))
                    {
                        peg_queue.Enqueue(vertex);
                    }
                }
                if (vertex.Peg == PegState.Blue)
                {
                    continue;
                }
                foreach (var next_vertex in vertex.GetTargets(reachableWithoutKeys))
                {
                    if (!marked.Contains(next_vertex))
                    {
                        queue.Enqueue(next_vertex);
                    }
                }

                if (!visited.Contains(vertex))
                    newlyVisited.Add(vertex);
                marked.Add(vertex);
            }
        } while (queue.Any() || peg_queue.Any());

        return newlyVisited;

        bool reachableWithoutKeys(Edge edge)
        {
            if (edge.Condition.Item.Type == ItemType.SmallKey)
                return false;

            return collected.Has(edge.Condition);
        }
    }
    private bool RecursiveDoorSearch(Inventory inventory)
    {
        var strongLocations = new HashSet<Vertex>();
        foreach (var (key, edges) in _doors)
        {
            int keyCount = inventory.GetCount(key);
            if (keyCount == 0)
                continue;

            if (keyCount >= edges.Count)
            {
                // we have all keys, unlock everything.
                var behindDoorLocations = edges.SelectMany(e => new[] { e.From, e.To }).ToHashSet();
                strongLocations.UnionWith(InternalSearch(inventory, _visited, [.. behindDoorLocations]));
            }
            else
            {
                strongLocations.UnionWith(RecursiveDoorSearchInternal(inventory, key, edges, keyCount, _visited));
            }
        }
        _visited.UnionWith(strongLocations);

        return strongLocations.Any();
    }
    private HashSet<Vertex> RecursiveDoorSearchInternal(Inventory inventory, Item key, HashSet<(Vertex From, Vertex To)> edges, int keyCount, HashSet<Vertex> visitedBeforeDoors, params Vertex[] additionalStarts)
    {
        if (keyCount == 0)
            return [];

        var reachableDoors = visitedBeforeDoors.SelectMany(v => v.Edges)
            .Where(e => e.Condition.Item == key && !visitedBeforeDoors.Contains(e.To))
            .ToHashSet();
        HashSet<Vertex>? strongLocations = null;
        var visitedBeforeRecursion = visitedBeforeDoors.ToHashSet();
        foreach (var door in reachableDoors)
        {
            Vertex[] startAt = [door.To, .. additionalStarts];
            var weakLocations = InternalSearch(inventory, _visited, startAt);
            visitedBeforeRecursion.UnionWith(weakLocations);
            weakLocations.UnionWith(RecursiveDoorSearchInternal(inventory, key, edges, keyCount - 1, visitedBeforeRecursion, startAt));
            // reset
            visitedBeforeRecursion.IntersectWith(weakLocations);

            if (weakLocations.Count == 0)
                return [];

            if (strongLocations != null)
                strongLocations.IntersectWith(weakLocations);
            else
                strongLocations = weakLocations;
        }

        return strongLocations ?? [];
    }

    private bool DropOffSearch(Item item)
    {
        //var start = _vertices["Bomb Shoppe Lobby:" + item.World.Id];
        //var end = _vertices["Pyramid:" + item.World.Id];
        //visited.Contains(end);
        return true;
    }

    private void FindDoors()
    {
        foreach (var edge in _graph.GetVertices().SelectMany(v => v.Edges).Where(e => e.Condition.Item.Type == ItemType.SmallKey))
        {
            var first = edge.From;
            var second = edge.To;

            if (first.Name.Contains(" - Lit:"))
            {
                first = _graph.GetVertex(first.Name.Replace(" - Lit", ""));
            }
            if (second.Name.Contains(" - Lit:"))
            {
                second = _graph.GetVertex(second.Name.Replace(" - Lit", ""));
            }

            if (edge.From.Name.CompareTo(edge.To.Name) > 0)
            {
                (first, second) = (second, first);
            }

            if (!_doors.TryGetValue(edge.Condition.Item, out var doorsForKey))
            {
                doorsForKey = new();
                _doors.Add(edge.Condition.Item, doorsForKey);
            }
            doorsForKey.Add((first, second));
        }
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
            {
                continue;
            }
            var set_locations = _graph.GetSetLocations(set_name).Where(static (location) => location.Item == null);
            if (set_locations.Count() < set_count)
            {
                throw new Exception($"Not enough set locations available: {set_name}");
            }
            // if a set has the same number of items to place as set locations
            // left, remove it from this return.
            if (itemSet != set_name && set_locations.Count() == set_count)
            {
                empty_locations.RemoveAll(set_locations.Contains);
            }
        }

        return empty_locations.ToArray();
    }
}
