namespace Randomizer.Graph;

using System.ComponentModel;

public class Searcher
{
    private readonly HashSet<Vertex> _visited = new();

    private readonly Vertex _start;

    public Searcher(Graph graph, Vertex start)
    {
        Graph = graph;
        _start = start;
    }

    public Graph Graph { get; }

    /**
     * Get the Items found is last search of the Graph.
     * 
     * @param Vertex start where the search was started from
     * @param callable filter if we should filter the items
     */
    public IEnumerable<Item> GetItems()
    {
        foreach (var vertex in _visited)
        {
            if (vertex.Item is not null)
            {
                yield return vertex.Item;
            }
            if (vertex.Trophy is not null)
            {
                yield return vertex.Trophy;
            }
        }
    }

    /**
     * Get all vertices that were visited in a given search (which has been
     * called first) from a set starting point.
     */
    public IEnumerable<Vertex> GetVisited()
    {
        return _visited;
    }

    /**
     * Perform a search of reachable Vertices from a given start. This is really
     * meat an potatoes of the whole class... I"m sure you were expecting good
     * documentation. Eventually my friend, eventually.
     */
    public IEnumerable<Vertex> InternalSearch()
    {
        if (!Graph.HasVertex(_start))
        {
            return Enumerable.Empty<Vertex>();
        }

        var marked = new HashSet<Vertex>();
        var pegMarked = new HashSet<Vertex>();
        if (!_visited.Contains(_start))
        {
            _visited.Add(_start);
            marked.Add(_start);
        }
        var queue = new Queue<Vertex>();
        var peg_queue = new Queue<Vertex>();
        queue.Enqueue(_start);

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
                foreach (var next_vertex in Graph.GetTargets(vertex))
                {
                    if (!pegMarked.Contains(next_vertex))
                    {
                        peg_queue.Enqueue(next_vertex);
                    }
                }

                _visited.Add(vertex);
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
                foreach (var next_vertex in Graph.GetTargets(vertex))
                {
                    if (!marked.Contains(next_vertex))
                    {
                        queue.Enqueue(next_vertex);
                    }
                }

                _visited.Add(vertex);
                marked.Add(vertex);
            }
        } while (queue.Any() || peg_queue.Any());

        return _visited;
    }


    public Searcher Search(Inventory collected)
    {
        bool newItemsFound;
        do
        {
            InternalSearch();

            newItemsFound = false;
            foreach (var item in GetItems())
            {
                if (!collected.Has(item))
                {
                    if (item.Name.StartsWith("BigRedBomb") && DropOffSearch(item))
                    {
                        collected.AddItem(item.World.GetItem("BigRedBombActive"));
                    }
                    newItemsFound = true;
                    collected.AddItem(item);
                }
            }
        } while (newItemsFound);

        return searcher;
    }

    private bool DropOffSearch(Item item)
    {
        var start = _vertices["Bomb Shoppe Lobby:" + item.World.Id];
        var end = _vertices["Pyramid:" + item.World.Id];
        //visited.Contains(end);
        return true;
    }

    private IEnumerable<Vertex> RecursiveDoorSearch(
        Graph searchGraph,
        IEnumerable<Vertex> found,
        Inventory collected,
        IEnumerable<Vertex> lockedDoors,
        Item key,
        int recursionLevel,
        IEnumerable<Vertex>? chain = null
    )
    {
        if (collected.GetCount(key) >= _keyDoors[key].Count)
        {
            var door_graphs = _keyDoorEdges.Where(door_id => _keyDoors[key].Contains(door_id.Key)).Select(k => k.Value).ToArray();
            var searcher = SearchGraph(collected, searchGraph.Merge(door_graphs));
            return searcher.GetVisited();
        }

        var initialKeySearcher = new Searcher(searchGraph, _start);
        var in_graph_locations = initialKeySearcher.Search();
        var sub_found_locations = new Dictionary<Vertex, List<Vertex>>();
        chain ??= Enumerable.Empty<Vertex>();
        found = found.ToArray();
        foreach (var door in lockedDoors.Where(location => in_graph_locations.Contains(location)))
        {
            var current_chain = chain.Concat(new[] { door }).ToArray();
            string chain_id = string.Join('-', current_chain.Select(v => v.GetHashCode()).OrderBy(h => h));
            if (_doorChains.TryGetValue(chain_id, out var door_chain))
            {
                sub_found_locations.Add(door, door_chain);
                continue;
            }
            var keySearcher = SearchGraph(collected, searchGraph.Merge(_keyDoorEdges[door]));
            var found_locations = keySearcher.GetVisited().ToList();
            var new_collected = collected.Merge(CollectItems(found_locations.Except(found)));
            sub_found_locations.Add(door, found_locations);
            int new_found_keys = new_collected.GetCount(key);
            if (lockedDoors.Count() > 1 && new_found_keys > recursionLevel)
            {
                var all_found = found_locations.Concat(found).ToArray();
                // TODO: should this be overwriting the door search result?
                //sub_found_locations.Add(door, this.recursiveDoorSearch(
                sub_found_locations[door] = RecursiveDoorSearch(
                    keySearcher.Graph,
                    all_found,
                    new_collected,
                    lockedDoors.Except(new[] { door }).ToArray(),
                    key,
                    recursionLevel + 1,
                    current_chain
                ).ToList();
            }
            _doorChains.Add(chain_id, sub_found_locations[door]);
        }

        if (sub_found_locations.Any())
        {
            IEnumerable<Vertex> result = sub_found_locations.Values.First();
            foreach (var other in sub_found_locations.Values.Skip(1))
                result = result.Intersect(other);
            return result;
        }

        return found;
    }

    public IEnumerable<Vertex> GetStrongLocations(Inventory collected)
    {
        var searcher = SearchGraph(collected);
        var found_locations = searcher.GetVisited().ToHashSet();
        var new_found_locations = new HashSet<Vertex>();
        do
        {
            _doorChains.Clear();
            new_found_locations.Clear();
            var new_items = CollectItems(found_locations);
            var new_collected = collected.Merge(new_items);
            searcher = SearchGraph(new_collected);
            foreach (var (key, doors) in _keyDoors)
            {
                if (new_collected.Has(key))
                {
                    var strong_locations = RecursiveDoorSearch(
                        searcher.Graph,
                        found_locations,
                        new_collected,
                        doors,
                        key,
                        1
                    ).ToHashSet();
                    strong_locations.ExceptWith(found_locations);
                    found_locations.UnionWith(strong_locations);
                    new_found_locations.UnionWith(strong_locations);
                }
            }
        } while (new_found_locations.Any());

        return found_locations;
    }
}
