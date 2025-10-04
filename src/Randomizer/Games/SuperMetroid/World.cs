namespace Randomizer.Games.SuperMetroid;

using Randomizer.Graph;

using Graph = Graph.Graph;
using BaseVertex = Graph.Vertex;
using Randomizer.Games.SuperMetroid.Model;
using System.Diagnostics;

/// <summary>Model of a world in which a player would be playing.</summary>
public sealed class World : Randomizer.Graph.World<Item>
{

    public Config Config { get; }
    public PRNG Prng { get; }
    public JsonReader JsonData { get; set; }
    public List<string> AllowedTechs { get; init; }
    public Map? Map { get; set; }

    /// <summary>Add all the vertices to the graph for this region.</summary>
    /// <param name="id">id of this world</param>
    /// <param name="randomizerConfig">options for this world</param>
    public World(int id, WorldConfig randomizerConfig, Graph graph, PRNG prng)
        : base("sm", id, graph, randomizerConfig)
    {
        Config = randomizerConfig.SuperMetroid ?? throw new ArgumentException("This world requires valid settings for Super Metroid");
        Prng = prng;

        List<IItem> items = [GetItem("fixed")];
        items.AddRange(Config.StartingEquipment.Select(GetItem));
        StartingItems = new Inventory(items.ToArray());
        StartingItems.AddItem(GetItem("f_ZebesAwake"));
        StartingItems.AddItem(GetItem("f_TourianOpen"));

        JsonData = new JsonReader(Config);
        JsonData.Load();

        AllowedTechs = Config.LogicTechs[Config.Logic].Concat(Config.CustomTech).ToList();

        RequirementHandler.Initialize(JsonData, this);

        if (Config.MapRandomizer == MapRandomizerSetting.Standard)
        {
            var mapRandomizer = new MapRandomizer(JsonData, this, prng);
            mapRandomizer.Randomize();
        }

        var preprocessor = new GraphPreprocessor(JsonData, this);
        preprocessor.Preprocess();

        Start = GetLocation("Crateria - Landing Site - Bottom Left Door");
    }

    public Inventory ComputeStartingItems()
    {
        var inventory = new Inventory([GetItem("fixed"), .. Config.StartingEquipment.Select(GetItem)]);
        return inventory;
    }

    protected override Item CreateItem(string name, IWorld world) => new(name, world);

    public new IEnumerable<Vertex> GetLocationsOfType(VertexType type) => GetLocations().Where(vertex => vertex.Type == type).Cast<Vertex>();

    public override bool IsWinnable(BaseVertex start, Inventory startingInventory)
    {
        var winSearcher = new StatefulSearcher(Graph, (Vertex)this.Start, startingInventory);
        return winSearcher.HasFound(GetItem("f_DefeatedMotherBrain"));
    }

    public override ISearcher GetSearcherForWorld(Graph graph, BaseVertex? start, Inventory inventory, SetLocations? setLocations = null)
    {
        return new StatefulSearcher(graph, (Vertex)(start ?? Start), inventory, setLocations);
    }
}
