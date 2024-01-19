namespace Randomizer.Graph;

/// <summary>
/// Graph representation in the world. We only allow additions to
/// the graph, so there is no need to consider removal of edges/vertices. And all
/// edges must be directed!
/// </summary>
public sealed class Graph
{
    // TODO: this (and _setLocations) feel like they shouldn't be here.
    //       they probably make more sense over in the ItemPooler.
    private static readonly HashSet<VertexType> ITEM_LOCATIONS = new()
    {
        VertexType.BigChest,
        VertexType.Bonk,
        VertexType.Chest,
        VertexType.Drop,
        VertexType.Dig,
        VertexType.Event,
        VertexType.Medallion,
        //VertexType.Mob,
        VertexType.Npc,
        VertexType.Pedestal,
        //VertexType.Pot,
        VertexType.Prize,
        VertexType.Refill,
        VertexType.ShopItem,
        VertexType.Standing,
    };

    private readonly HashSet<Vertex> _vertices = new();
    private Vertex[] _verticesById = [];
    private readonly Dictionary<string, Vertex> _verticesByName = new();
    private readonly Dictionary<ItemSetName, List<Vertex>> _setLocations = new() { { ItemSetName.DefaultSet, new() } };
    public Dictionary<Item /* actualKey */, Dictionary<Item /* doorSpecificUnlockItem */, HashSet<(Vertex A, Vertex B)>>> Doors { get; } = new();
    public Dictionary<Item /* actualKey */, HashSet<Vertex>> FixedKeys = new();
    public HashSet<Item> AllItems { get; set; } = new();

    public IEnumerable<Vertex> GetVertices()
    {
        return _vertices;
    }

    public IEnumerable<Vertex> GetSetLocations(ItemSetName set)
    {
        return _setLocations[set];
    }

    public Vertex GetVertex(string name)
    {
        return _verticesByName[name];
    }

    public Vertex GetVertex(int id)
    {
        return _verticesById[id];
    }

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
        _verticesByName.Add(vertex.Name, vertex);

        if (ITEM_LOCATIONS.Contains(vertex.SubType ?? vertex.Type))
        {
            _setLocations[ItemSetName.DefaultSet].Add(vertex);
            foreach (var set in vertex.ItemSet)
            {
                _setLocations.TryAdd(set, new());
                _setLocations[set].Add(vertex);
            }
        }

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
    public Edge AddDirected(Vertex from, Vertex to, Item item, int itemCount = 1)
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
