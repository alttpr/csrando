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
        _verticesByName.Add($"{vertex.Name}:{vertex.World.GameId}:{vertex.World.Id}", vertex);

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
    }
}
