namespace AlttpRandomizer.Graph;

using System.Collections.Concurrent;

/**
 * Matrix backed graph. Instead of using a full graph library, this is an
 * attempt at one that is more tuned to our needs. We only allow additions to
 * the graph, so there is no need to consider removal of edges/vertices. And all
 * edges must be directed!
 */
public sealed class Graph
{
    private readonly HashSet<Vertex> _vertices = new();
    private readonly Dictionary<string, Vertex> _verticesByName = new();
    private readonly ConcurrentDictionary<Vertex, HashSet<Vertex>> _visited = new();
    private readonly Dictionary<Vertex, HashSet<Vertex>> _adjacencyMatrix = new();
    private readonly HashSet<Edge> _edges = new();
    private readonly ConcurrentDictionary<Vertex, HashSet<Vertex>> _marked = new();
    private readonly ConcurrentDictionary<Vertex, HashSet<Vertex>> _pegMarked = new();
    private readonly List<Vertex> _recheckNodes = new();

    /**
     * Get the Vertices in the Graph.
     */
    public IEnumerable<Vertex> GetVertices()
    {
        return _vertices;
    }

    /**
     * Get a Vertex by name in the Graph.
     * 
     * @param string name name of vertex to search for
     */
    public Vertex? GetVertex(string name)
    {
        return _verticesByName.GetValueOrDefault(name);
    }

    /**
     * Get the Edges in the Graph.
     */
    public IEnumerable<Edge> GetEdges()
    {
        return _edges;
    }

    /**
     * Get the Items found is last search of the Graph.
     * 
     * @param Vertex start where the search was started from
     * @param callable filter if we should filter the items
     */
    public IEnumerable<Item> GetItems(Vertex start)
    {
        foreach (var vertex in _visited[start])
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
     * Create a new Vertex in this Graph.
     *
     * @param mixed[] attributes attributes for the vertex
     */
    public Vertex NewVertex(Dictionary<string, object>? attributes = null)
    {
        var vertex = new Vertex(attributes);

        AddVertex(vertex);

        return vertex;
    }

    /**
     * Add vertex to graph.
     * 
     * @param Vertex vertex Vertex to add to graph
     */
    public void AddVertex(Vertex vertex)
    {
        _vertices.Add(vertex);
        _verticesByName[vertex.Name] = vertex;
        _adjacencyMatrix.Add(vertex, new());
    }

    public IEnumerable<Vertex> GetTargets(Vertex vertex)
    {
        return _adjacencyMatrix.GetValueOrDefault(vertex) ?? Enumerable.Empty<Vertex>();
    }

    /**
     * Create a new Directed Edge in the graph.
     * 
     * @param Vertex from from vertex
     * @param Vertex to to vertex
     * @param string group edge grouping
     */
    public Edge AddDirected(Vertex from, Vertex to, string group)
    {
        var edge = new Edge(from, to, group);

        AddEdge(edge);

        return edge;
    }

    /**
     * Add already constructed Edge to graph, this will also add the related
     * Vertices to the graph if they aren"t there already.
     * 
     * @param Edge edge Edge to add
     */
    public void AddEdge(Edge edge)
    {
        if (!_vertices.Contains(edge.From))
        {
            AddVertex(edge.From);
        }

        if (!_vertices.Contains(edge.To))
        {
            AddVertex(edge.To);
        }

        _adjacencyMatrix[edge.From].Add(edge.To);
        _edges.Add(edge);
    }

    /**
     * Get a subgraph of the graph filtering on given edge group. Used to split
     * the graph up by item requirements.
     * 
     * @param string group name of group
     */
    public Graph GetSubgraph(string group)
    {
        var newGraph = new Graph();

        foreach (var edge in _edges)
        {
            if (edge.Group != group)
            {
                continue;
            }

            newGraph.AddEdge(edge);
        }

        return newGraph;
    }

    /**
     * Merge this graph with a variable amount of other graphs and return new
     * Graph.
     *
     * @param Graph ...graphs graphs to merge
     */
    public Graph Merge(params Graph[] graphs)
    {
        var newGraph = new Graph(this);

        foreach (var graph in graphs)
        {
            foreach (var edge in graph._edges)
            {
                if (newGraph._edges.Contains(edge))
                {
                    continue;
                }

                newGraph.AddEdge(edge);
                newGraph._recheckNodes.Add(edge.From);
            }
        }

        return newGraph;
    }

    /**
     * Get new graph with certain edge groups excluded.
     * 
     * @param string ...groups edge groups to exclude
     */
    public Graph Exclude(params string[] groups)
    {
        var newGraph = new Graph();

        foreach (var edge in _edges)
        {
            if (groups.Contains(edge.Group))
            {
                continue;
            }

            newGraph.AddEdge(edge);
        }

        return newGraph;
    }

    /**
     * Get all vertices that were visited in a given search (which has been
     * called first) from a set starting point.
     *
     * @param Vertex start vertex where search was started from
     */
    public IEnumerable<Vertex> GetVisited(Vertex start)
    {
        return _visited.GetValueOrDefault(start) ?? Enumerable.Empty<Vertex>();
    }

    /**
     * Perform a search of reachable Vertices from a given start. This is really
     * meat an potatoes of the whole class... I"m sure you were expecting good
     * documentation. Eventually my friend, eventually.
     *
     * @param Vertex start vertex to start search from
     */
    public IEnumerable<Vertex> Search(Vertex start)
    {
        if (!_vertices.Contains(start))
        {
            return Enumerable.Empty<Vertex>();
        }

        if (!_visited.ContainsKey(start))
        {
            _visited.TryAdd(start, new() { start });
            _pegMarked.TryAdd(start, new());
            _marked.TryAdd(start, new() { start });
        }
        var queue = new Queue<Vertex>();
        var peg_queue = new Queue<Vertex>();
        queue.Enqueue(start);
        foreach (var vertex in _marked[start])
        {
            if (_recheckNodes.Contains(vertex))
                queue.Enqueue(vertex);
        }
        foreach (var vertex in _pegMarked[start])
        {
            if (_recheckNodes.Contains(vertex))
                queue.Enqueue(vertex);
        }
        _recheckNodes.Clear();

        do
        {
            while (peg_queue.TryDequeue(out var vertex))
            {
                if (vertex.Switch)
                {
                    if (!_marked.TryGetValue(start, out var markedFromStart) || !markedFromStart.Contains(vertex))
                    {
                        queue.Enqueue(vertex);
                    }
                }
                if (vertex.Peg == "orange")
                {
                    continue;
                }
                foreach (var next_vertex in _adjacencyMatrix.GetValueOrDefault(vertex, new()))
                {
                    if (!_pegMarked.TryGetValue(start, out var pegMarkedFromStart) || !pegMarkedFromStart.Contains(next_vertex))
                    {
                        peg_queue.Enqueue(next_vertex);
                    }
                }
                _visited.GetOrAdd(start, _ => new()).Add(vertex);
                _pegMarked.GetOrAdd(start, _ => new()).Add(vertex);
            }

            while (queue.TryDequeue(out var vertex))
            {
                if (vertex.Switch)
                {
                    if (!_pegMarked.TryGetValue(start, out var pegMarkedFromStart) || !pegMarkedFromStart.Contains(vertex))
                    {
                        peg_queue.Enqueue(vertex);
                    }
                }
                if (vertex.Peg == "blue")
                {
                    continue;
                }
                foreach (var next_vertex in _adjacencyMatrix.GetValueOrDefault(vertex, new()))
                {
                    if (!_marked.TryGetValue(start, out var markedFromStart) || !markedFromStart.Contains(next_vertex))
                    {
                        queue.Enqueue(next_vertex);
                    }
                }
                _visited.GetOrAdd(start, _ => new()).Add(vertex);
                _marked.GetOrAdd(start, _ => new()).Add(vertex);
            }
        } while (queue.Any() || peg_queue.Any());

        return _visited.GetValueOrDefault(start) ?? Enumerable.Empty<Vertex>();
    }

    public Graph()
    {
    }
    private Graph(Graph other)
    {
        // FIXME: adjacency_matrix, marked/peg_marked and visited are not deep copies,
        //        so they mutate a Graph that should be immutable
        _adjacencyMatrix = new(other._adjacencyMatrix);
        _edges = new(other._edges);
        _marked = new(other._marked);
        _pegMarked = new(other._pegMarked);
        _recheckNodes = new(other._recheckNodes);
        _vertices = new(other._vertices);
        _verticesByName = new(other._verticesByName);
        _visited = new(other._visited);
    }
}
