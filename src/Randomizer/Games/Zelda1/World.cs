namespace Randomizer.Games.Zelda1;

using Randomizer.Games;
using Randomizer.Graph;
using Graph = Graph.Graph;
using BaseVertex = Graph.Vertex;

/// <summary>Model of a world in which a player would be playing.</summary>
public sealed class World : World<Item>
{
    public Config Config { get; }
    public PRNG Prng { get; }
    public YamlReader.YamlData? YamlData { get; set; }

    /// <summary>Add all the vertices to the graph for this region.</summary>
    /// <param name="id">id of this world</param>
    /// <param name="randomizerConfig">options for this world</param>
    public World(int id, WorldConfig randomizerConfig, Graph graph, PRNG prng)
        : base("Zelda1", id, graph, randomizerConfig)
    {
        Config = randomizerConfig.Zelda1 ?? throw new ArgumentException("This world requires valid settings for The Legend of Zelda");
        Prng = prng;

        List<IItem> items = [GetItem("fixed")];
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

    /// <summary>
    /// Get a vertex by name in this world.
    /// </summary>
    /// <param name="locationName">name to search for</param>
    protected override Item CreateItem(string name, IWorld world) => new(name, world);

    public override IEnumerable<BaseVertex> GetEmptyLocationsInSet(ISearcher searcher, IItem itemToPlace, ItemSetName itemSet, Dictionary<ItemSetName, int> setCounts)
    {
        var locations = new List<BaseVertex>();

        locations.AddRange(searcher.GetEmptyLocationsInSet(itemSet, setCounts));

        return locations;
    }

    public override bool IsWinnable(BaseVertex start, Inventory startingInventory)
    {
        var winSearcher = new Searcher(Graph, start, startingInventory);
        return winSearcher.HasFound(GetItem("Zelda"));
    }

    public override ISearcher GetSearcherForWorld(Graph graph, BaseVertex? start, Inventory inventory, SetLocations? setLocations = null)
    {
        return new Searcher(graph, start ?? Start, inventory, setLocations, this);
    }

}
