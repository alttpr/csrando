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
        VertexType.Mob,
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
    private readonly HashSet<Edge> _edges = new();
    private readonly Dictionary<string, List<Vertex>> _setLocations = new() { { "*", new() } };

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

    public IEnumerable<Edge> GetEdges()
    {
        return _edges;
    }

    /// <summary>
    /// Add Vertex to the graph.
    /// </summary>
    /// 
    /// <param name="vertex">source Vertex</param> 
    public Vertex AddVertex(Vertex vertex)
    {
        _vertices.Add(vertex);
        _verticesByName[vertex.Name] = vertex;

        if (ITEM_LOCATIONS.Contains(vertex.Type))
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
        return new Edge(from, to, condition);
    }
}
