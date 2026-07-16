namespace Randomizer.Graph;

/// <summary>
/// Graph representation in the world. We only allow additions to
/// the graph, so there is no need to consider removal of edges/vertices. And all
/// edges must be directed!
/// </summary>
public sealed class Graph
{
    private readonly HashSet<Vertex> _vertices = [];
    private Vertex[] _verticesById = [];
    private readonly Dictionary<string, Vertex> _verticesByName = [];
    public Dictionary<IItem /* actualKey */, Dictionary<IItem /* doorSpecificUnlockItem */, HashSet<(Vertex A, Vertex B)>>> Doors { get; } = [];
    public Dictionary<IItem /* actualKey */, HashSet<Vertex>> FixedKeys { get; } = [];

    /// <summary>
    /// All known items, across all worlds and games. Use <see cref="RegisterItem"/> to allocate an item id usable with the <see cref="Inventory"/>.
    /// </summary>
    public HashSet<IItem> AllItems { get; } = [];
    /// <summary>Registers a new item that has no item id yet. Updates (and returns) the item with a graph-specific unique id.</summary>
    public TItem RegisterItem<TItem>(TItem item) where TItem : IItem
    {
        int newId = AllItems.Count;
        if (AllItems.Add(item))
            item.Id = newId;
        return item;
    }

    public IEnumerable<Vertex> GetVertices() => _vertices;
    /// <summary>Number of vertices in the graph. Matches the id space once <see cref="SetVertexIds"/> has run.</summary>
    public int VertexCount => _verticesById.Length > 0 ? _verticesById.Length : _vertices.Count;
    public Vertex GetVertex(string name) => _verticesByName[name];
    public Vertex GetVertex(int id) => _verticesById[id];
    public bool HasVertex(Vertex vertex) => _vertices.Contains(vertex);
    public bool HasVertex(string name) => _verticesByName.ContainsKey(name);

    /// <summary>
    /// Add Vertex to the graph.
    /// </summary>
    ///
    /// <param name="vertex">source Vertex</param>
    public Vertex AddVertex(Vertex vertex)
    {
        if (_verticesById.Length > 0)
            throw new Exception("Adding a vertex after Ids are set");

        _vertices.Add(vertex);
        _verticesByName.Add(vertex.ToString(), vertex);

        return vertex;
    }


    /// <summary>
    /// Create a new Directed Edge in the graph.
    /// </summary>
    ///
    /// <param name="from">source Vertex</param>
    /// <param name="to">target Vertex</param>
    /// <param name="item">Item required to traverse edge</param>
    /// <param name="itemCount">flow of Item to traverse</param>
    public Edge AddDirected(Vertex from, Vertex to, IItem item, int itemCount = 1)
    {
        return AddDirected(from, to, new ItemCondition(item, itemCount));
    }

    /// <summary>
    /// Create a new Directed Edge in the graph.
    /// </summary>
    ///
    /// <param name="from">source Vertex</param>
    /// <param name="to">target Vertex</param>
    /// <param name="condition">ItemCondition required to traverse edge</param>
    public Edge AddDirected(Vertex from, Vertex to, ItemCondition condition)
    {
        var edge = new Edge(from, to, condition);
        from.Edges.Add(edge);
        return edge;
    }

    public void SetVertexIds()
    {
        _verticesById = new Vertex[_vertices.Count];
        int cnt = 0;
        foreach (var vertex in _vertices)
        {
            vertex.Id = cnt++;
            _verticesById[vertex.Id] = vertex;
        }

        FreezeEdges();
    }

    /// <summary>
    /// Flat edge with pre-resolved ids, laid out contiguously per source vertex. The hot BFS
    /// walks these instead of chasing Edge object references; everything it branches on is
    /// resolved once here.
    /// </summary>
    public readonly struct FrozenEdge(int toId, int itemId, int count, bool crossWorld, bool unconditional)
    {
        public readonly int ToId = toId;
        public readonly int ItemId = itemId;
        public readonly int Count = count;
        public readonly bool CrossWorld = crossWorld;
        public readonly bool Unconditional = unconditional;
    }

    private FrozenEdge[] _frozenEdges = [];
    private int[] _frozenEdgeStart = [];

    /// <summary>Edges of a vertex in the frozen view, in the same order as <see cref="Vertex.Edges"/>.</summary>
    public ReadOnlySpan<FrozenEdge> GetFrozenEdges(int vertexId) =>
        _frozenEdges.AsSpan(_frozenEdgeStart[vertexId], _frozenEdgeStart[vertexId + 1] - _frozenEdgeStart[vertexId]);

    /// <summary>
    /// Snapshot every vertex's edges into the flat searchable view. Edges are only mutated
    /// during world construction, which completes before <see cref="SetVertexIds"/>; the one
    /// post-search mutation (Metroid portal-door resolution at ROM-write time) happens after
    /// all searches are done, so a stale frozen view is never observed.
    /// </summary>
    private void FreezeEdges()
    {
        int edgeCount = 0;
        foreach (var vertex in _verticesById)
        {
            edgeCount += vertex.Edges.Count;
        }

        _frozenEdgeStart = new int[_verticesById.Length + 1];
        _frozenEdges = new FrozenEdge[edgeCount];
        int next = 0;
        for (int id = 0; id < _verticesById.Length; id++)
        {
            _frozenEdgeStart[id] = next;
            var vertex = _verticesById[id];
            foreach (var edge in vertex.Edges)
            {
                _frozenEdges[next++] = new FrozenEdge(
                    edge.To.Id,
                    edge.Condition.Item.Id,
                    edge.Condition.Count,
                    edge.To.World != vertex.World,
                    edge.Condition.IsUnconditional);
            }
        }
        _frozenEdgeStart[_verticesById.Length] = next;
    }
}
