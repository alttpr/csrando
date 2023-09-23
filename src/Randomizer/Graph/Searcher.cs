namespace Randomizer.Graph;
public class Searcher
{
    private readonly HashSet<Vertex> _visited = new();
    private readonly Graph _graph;
    private readonly Vertex _start;

    public Searcher(Graph graph, Vertex start)
    {
        _graph = graph;
        _start = start;
    }

    /// <summary>Get the Items found is last search of the Graph.</summary>
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


    /// <summary>Get all vertices that were visited in a given search (which has been called first) from a set starting point.</summary>
    public IEnumerable<Vertex> GetVisited()
    {
        return _visited;
    }

    private void InternalSearch()
    {
        if (!_graph.HasVertex(_start))
        {
            return;
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
                foreach (var next_vertex in _graph.GetTargets(vertex))
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
                foreach (var next_vertex in _graph.GetTargets(vertex))
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

        return this;
    }

    private bool DropOffSearch(Item item)
    {
        //var start = _vertices["Bomb Shoppe Lobby:" + item.World.Id];
        //var end = _vertices["Pyramid:" + item.World.Id];
        //visited.Contains(end);
        return true;
    }

    private IEnumerable<Vertex> RecursiveDoorSearch(
        IEnumerable<Vertex> found
    )
    {
        return found;
    }

    public IEnumerable<Vertex> GetStrongLocations(Inventory collected)
    {
        var searcher = new Searcher(this._graph, this._start);
        var found_locations = searcher.GetVisited().ToHashSet();
        return found_locations;
    }
}
