namespace Randomizer.Graph;
public class Searcher
{
    private readonly HashSet<Vertex> _visited = new();
    private readonly HashSet<Vertex> _collected = new();
    private readonly Graph _graph;
    private readonly Vertex _start;
    private readonly Inventory _inventory;

    public Searcher(Graph graph, Vertex start, Inventory inventory)
    {
        _graph = graph;
        _start = start;
        _inventory = inventory;

        if (!_graph.HasVertex(_start))
        {
            throw new Exception("Start vertex not in graph");
        }

        bool newItemsFound;
        do
        {
            InternalSearch(inventory);

            newItemsFound = false;
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

    private void InternalSearch(Inventory collected)
    {
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
                foreach (var next_vertex in vertex.GetTargets(reachableWithoutKeys))
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
                foreach (var next_vertex in vertex.GetTargets(reachableWithoutKeys))
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

        bool reachableWithoutKeys(Edge edge)
        {
            if (edge.Condition.Item.Type == ItemType.SmallKey)
                return false;

            return collected.Has(edge.Condition);
        }
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
