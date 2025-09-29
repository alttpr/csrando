namespace Randomizer.Games.Metroid;

using Randomizer.Graph;
using Graph = Graph.Graph;
using BaseVertex = Graph.Vertex;

/// <summary>Model of a world in which a player would be playing.</summary>
public sealed class World : Randomizer.Graph.World<Item>
{

    public Config Config { get; }
    public PRNG Prng { get; }
    public YamlReader.YamlData? YamlData { get; set; }
    public Dictionary<int, byte[]>? PatchData { get; set; }

    /// <summary>Add all the vertices to the graph for this region.</summary>
    /// <param name="id">id of this world</param>
    /// <param name="randomizerConfig">options for this world</param>
    public World(int id, WorldConfig randomizerConfig, Graph graph, PRNG prng)
        : base("m1", id, graph, randomizerConfig)
    {
        Config = randomizerConfig.Metroid ?? throw new ArgumentException("This world requires valid settings for Metroid");
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

    protected override Item CreateItem(string name, IWorld world) => new(name, world);

    public override bool IsWinnable(BaseVertex start, Inventory startingInventory)
    {
        var winSearcher = new Searcher(Graph, start, startingInventory);
        return winSearcher.HasFound(GetItem("DefeatedSilverTwo"));
    }

    public override ISearcher GetSearcherForWorld(Graph graph, BaseVertex? start, Inventory inventory, SetLocations? setLocations = null)
    {
        return new Searcher(graph, start ?? Start, inventory, setLocations, this);
    }
}
