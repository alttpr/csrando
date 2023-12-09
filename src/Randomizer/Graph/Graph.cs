namespace Randomizer.Graph;

/// <summary>
/// Graph representation in the world. We only allow additions to
/// the graph, so there is no need to consider removal of edges/vertices. And all
/// edges must be directed!
/// </summary>
public sealed class Graph
{
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
        VertexType.Pot,
        VertexType.Prize,
        VertexType.Refill,
        VertexType.ShopItem,
        VertexType.Standing,
    };

    private readonly HashSet<Vertex> _vertices = new();
    private readonly Dictionary<string, Vertex> _verticesByName = new();
    private readonly Dictionary<string, List<Vertex>> _setLocations = new() { { "*", new() } };
    private Dictionary<Item, HashSet<(Vertex From, Vertex To)>>? _doors;
    public Dictionary<Item, HashSet<(Vertex From, Vertex To)>> Doors
    {
        get
        {
            if (_doors == null)
            {
                _doors = new();

                foreach (var edge in _vertices.SelectMany(v => v.Edges).Where(e => e.Condition.Item.Type == ItemType.SmallKey))
                {
                    var first = edge.From;
                    var second = edge.To;

                    if (edge.From.Name.CompareTo(edge.To.Name) > 0)
                        (first, second) = (second, first);

                    if (!_doors.TryGetValue(edge.Condition.Item, out var doorsForKey))
                    {
                        doorsForKey = new();
                        _doors.Add(edge.Condition.Item, doorsForKey);
                    }
                    doorsForKey.Add((first, second));
                }
            }

            return _doors;
        }
    }

    private Dictionary<Item, HashSet<Vertex>>? _fixedKeys;
    public Dictionary<Item, HashSet<Vertex>> FixedKeys
    {
        get
        {
            if (_fixedKeys == null)
            {
                _fixedKeys = _vertices
                    .Where(v => v.Item?.Type == ItemType.SmallKey)
                    .GroupBy(v => v.Item!)
                    .ToDictionary(k => k.Key, v => v.ToHashSet());
                foreach (var key in Doors)
                {
                    _fixedKeys.TryAdd(key.Key, new HashSet<Vertex>());
                }
            }
            return _fixedKeys;
        }
    }

    public IEnumerable<Vertex> GetVertices()
    {
        return _vertices;
    }

    public IEnumerable<Vertex> GetSetLocations(string set)
    {
        return _setLocations[set];
    }

    public Vertex GetVertex(string name)
    {
        return _verticesByName[name];
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
        _vertices.Add(vertex);
        _verticesByName.Add(vertex.Name, vertex);

        if (ITEM_LOCATIONS.Contains(vertex.SubType ?? vertex.Type))
        {
            _setLocations["*"].Add(vertex);
            foreach (string set in vertex.ItemSet)
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
}
