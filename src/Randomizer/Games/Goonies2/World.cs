namespace Randomizer.Games.Goonies2;

using Randomizer.Games;
using Randomizer.Graph;
using BaseVertex = Graph.Vertex;
using Graph = Graph.Graph;

/// <summary>Model of a world in which a player would be playing.</summary>
public sealed class World : World<Item>
{
    public Config Config { get; }

    /// <summary>Add all the vertices to the graph for this region.</summary>
    /// <param name="id">id of this world</param>
    /// <param name="randomizerConfig">options for this world</param>
    public World(int id, WorldConfig randomizerConfig, Graph graph, PRNG prng)
        : base("Goonies2", id, graph, randomizerConfig)
    {
        Config = randomizerConfig.Goonies2 ?? throw new ArgumentException("This world requires valid settings for The Goonies II: The Fratellis' Last Stand");

        List<IItem> items = [GetItem("fixed")];
        items.Add(GetItem($"ConfigWorldEnemyShuffle{Config.EnemyShuffle}"));

        items.AddRange(Config.StartingEquipment.Select(GetItem));
        StartingItems = new Inventory(items.ToArray());

        Start = DataLoader.Fill(this);
    }

    public Inventory ComputeStartingItems()
    {
        var inventory = new Inventory([GetItem("fixed"), .. Config.StartingEquipment.Select(GetItem)]);
        var searcher = new Searcher(Graph, GetLocation("DefaultItems"), inventory);
        return inventory;
    }

    protected override Item CreateItem(string name, IWorld world) => new(name, world);

    /// <summary>Get all vertices of a given type in this world.</summary>
    /// <param name="type">type to search for</param>
    public IEnumerable<Vertex> GetLocationsOfType(VertexType type) => GetLocations().OfType<Vertex>().Where(vertex => vertex.Type == type);

    public override IEnumerable<BaseVertex> GetEmptyLocationsInSet(ISearcher searcher, IItem itemToPlace, ItemSetName itemSet, Dictionary<ItemSetName, int> setCounts)
    {
        var locations = new List<BaseVertex>();

        locations.AddRange(searcher.GetEmptyLocationsInSet(itemSet, setCounts));

        return locations;
    }

    public override bool IsWinnable(BaseVertex start, Inventory startingInventory)
    {
        throw new NotImplementedException("Veetorp doesn't know if anyone can win Goonies 2");
    }
    public override ISearcher GetSearcherForWorld(Graph graph, BaseVertex? start, Inventory inventory, SetLocations? setLocations = null)
    {
        return new Searcher(graph, start ?? Start, inventory, setLocations);
    }
}
