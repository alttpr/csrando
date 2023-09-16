namespace Randomizer.Graph;

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
    private readonly Dictionary<Vertex, HashSet<Vertex>> _adjacencyMatrix = new();
    private readonly HashSet<Edge> _edges = new();

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

    public bool HasVertex(Vertex vertex) => _vertices.Contains(vertex);

    /**
     * Get the Edges in the Graph.
     */
    public IEnumerable<Edge> GetEdges()
    {
        return _edges;
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
    public Edge AddDirected(Vertex from, Vertex to, Item item, int itemCount = 1)
    {
        var edge = new Edge(from, to, item, itemCount);
        AddEdge(edge);
        return edge;
    }

    public Edge AddDirected(Vertex from, Vertex to, ItemCondition condition)
    {
        var edge = new Edge(from, to, condition);
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
    public Graph GetSubgraph(ItemCondition condition)
    {
        var newGraph = new Graph();

        foreach (var edge in _edges)
        {
            if (edge.Condition != condition)
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
            }
        }

        return newGraph;
    }

    public void MergeWith(params Graph[] graphs)
    {
        foreach (var graph in graphs)
        {
            foreach (var edge in graph._edges)
            {
                if (_edges.Contains(edge))
                {
                    continue;
                }

                AddEdge(edge);
            }
        }
    }

    /**
     * Get new graph with certain edge groups excluded.
     * 
     * @param string ...groups edge groups to exclude
     */
    public Graph Exclude(params Item[] items)
    {
        var newGraph = new Graph();

        foreach (var edge in _edges)
        {
            if (items.Contains(edge.Condition.Item))
            {
                continue;
            }

            newGraph.AddEdge(edge);
        }

        return newGraph;
    }

    public Graph()
    {
    }
    private Graph(Graph other)
    {
        _adjacencyMatrix = other._adjacencyMatrix.ToDictionary(x => x.Key, x => x.Value.ToHashSet());
        _edges = new(other._edges);
        _vertices = new(other._vertices);
        _verticesByName = new(other._verticesByName);
    }
}
