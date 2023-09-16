namespace Randomizer.Graph;

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
    public IEnumerable<Vertex> Search()
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
}
