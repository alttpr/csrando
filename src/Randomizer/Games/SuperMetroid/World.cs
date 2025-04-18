namespace Randomizer.Games.SuperMetroid;

using Randomizer.Graph;

using Graph = Graph.Graph;
using BaseVertex = Graph.Vertex;
using Randomizer.Games.SuperMetroid.Model;
using System.Diagnostics;

/// <summary>Model of a world in which a player would be playing.</summary>
public sealed class World : IWorld
{

    public int Id { get; }
    public string GameId { get; } = "sm";
    public Graph Graph { get; }
    public Inventory StartingItems { get; }
    public WorldConfig WorldConfig { get; }
    public Config Config { get; }
    public PRNG Prng { get; }
    public JsonReader JsonData { get; set; }
    public ushort PlacedItemCount { get; set; }
    public BaseVertex Start { get; }
    private readonly Dictionary<string, Item> _allItems = new();
    public List<string> AllowedTechs { get; init; }
    public Map? Map { get; set; }

    /// <summary>Add all the vertices to the graph for this region.</summary>
    /// <param name="id">id of this world</param>
    /// <param name="randomizerConfig">options for this world</param>
    public World(int id, WorldConfig randomizerConfig, Graph graph, PRNG prng)
    {
        Id = id;
        WorldConfig = randomizerConfig;
        Config = randomizerConfig.SuperMetroid ?? throw new ArgumentException("This world requires valid settings for Super Metroid");
        Graph = graph;
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

    /// <summary>
    /// Get a vertex by name in this world.
    /// </summary>
    /// <param name="locationName">name to search for</param>
    public BaseVertex GetLocation(string locationName)
    {
        return Graph.GetVertex($"{locationName}:{GameId}:{Id}");
    }

    public bool HasLocation(string locationName)
    {
        return Graph.HasVertex($"{locationName}:{GameId}:{Id}");
    }

    /// <summary>Get all vertices in this world.</summary>
    /// <returns></returns>
    public IEnumerable<BaseVertex> GetLocations() => Graph.GetVertices().Where(vertex => vertex.World == this);
    /// <summary>Get all vertices of a given type in this world.</summary>
    /// <param name="type">type to search for</param>
    public IEnumerable<Vertex> GetLocationsOfType(VertexType type) => GetLocations().OfType<Vertex>().Where(vertex => vertex.Type == type);

    public IItem GetItem(string name)
    {
        if (_allItems.TryGetValue(name, out var matchingItem))
        {
            return matchingItem;
        }

        // allow made up items
        var item = Graph.RegisterItem(new Item(name, this));
        _allItems.Add(item.Name, item);

        return item;
    }

    public IItem? GetItemOrNull(string? name)
    {
        if (name != null)
            return GetItem(name);
        return null;
    }

    public IItem? GetExistingItem(string name)
    {
        if (_allItems.TryGetValue(name, out var item))
            return item;
        return null;
    }

    public IEnumerable<Item> GetAllItems()
    {
        return _allItems.Values;
    }   

    public void TrackPlacedItem(BaseVertex location)
    {
        location.World.PlacedItemCount++;
    }
    
    public bool IsWinnable(BaseVertex start, Inventory startingInventory)
    {
        var winSearcher = new StatefulSearcher(Graph, (Vertex)this.Start, startingInventory);
        return winSearcher.HasFound(GetItem("f_DefeatedMotherBrain"));
        //return true;
    }

    public IEnumerable<BaseVertex> GetEmptyLocationsInSet(ISearcher searcher, IItem itemToPlace, ItemSetName itemSet, Dictionary<ItemSetName, int> setCounts)
    {
        var locations = new List<BaseVertex>();

        locations.AddRange(searcher.GetEmptyLocationsInSet(itemSet, setCounts));

        return locations;
    }

    public ISearcher GetSearcherForWorld(Graph graph, BaseVertex? start, Inventory inventory, SetLocations? setLocations = null)
    {
        return new StatefulSearcher(graph, (Vertex)(start ?? Start), inventory, setLocations);
    }
}
