namespace Randomizer.Games.Combo;

using Randomizer.Graph;
using Graph = Graph.Graph;
using BaseVertex = Graph.Vertex;

using AlttpWorld = Randomizer.Games.Alttp.World;
using SMWorld = Randomizer.Games.SuperMetroid.World;
using Z1World = Randomizer.Games.Zelda1.World;
using M1World = Randomizer.Games.Metroid.World;

/// <summary>Model of a world in which a player would be playing.</summary>
public sealed class World : IWorld
{

    public int Id { get; }
    public string GameId { get; } = "combo";
    public Graph Graph { get; }
    public Inventory StartingItems { get; }
    public WorldConfig WorldConfig { get; }
    public Config Config { get; }
    public PRNG Prng { get; }
    public ushort PlacedItemCount { get; set; }
    private readonly Dictionary<string, Item> _allItems = new();
    public BaseVertex Start { get; }

    public WorldConfig GameConfig { get; init; }

    public AlttpWorld? AlttpWorld { get;  init; }
    public SMWorld? SMWorld { get; init; }
    public Z1World? Z1World { get; init; }
    public M1World? M1World { get; init; }

    /// <summary>Add all the vertices to the graph for this region.</summary>
    /// <param name="id">id of this world</param>
    /// <param name="randomizerConfig">options for this world</param>
    public World(int id, WorldConfig randomizerConfig, Graph graph, PRNG prng)
    {
        Id = id;
        WorldConfig = randomizerConfig;
        Config = randomizerConfig.Combo ?? throw new ArgumentException("This world requires valid settings for Combo");
        GameConfig = Config.Games ?? throw new ArgumentException("This world requires valid settings for Games");
        Graph = graph;
        Prng = prng;
        Start = graph.AddVertex(new Vertex()
        {
            Name = "start",
            Type = VertexType.Meta,
            World = this,
        });

        StartingItems = new Inventory([GetItem("fixed")]);

        if (Config.Games.Alttp != null)
        {
            AlttpWorld = new AlttpWorld(id, Config.Games, graph, prng);
            StartingItems = StartingItems.Merge(AlttpWorld.StartingItems);
        }
        if (Config.Games.SuperMetroid != null)
        {
            SMWorld = new SMWorld(id, Config.Games, graph, prng);
            StartingItems = StartingItems.Merge(SMWorld.StartingItems);
        }
        if (Config.Games.Zelda1 != null)
        {
            Z1World = new Z1World(id, Config.Games, graph, prng);
            StartingItems = StartingItems.Merge(Z1World.StartingItems);
        }
        if (Config.Games.Metroid != null)
        {
            M1World = new M1World(id, Config.Games, graph, prng);
            StartingItems = StartingItems.Merge(M1World.StartingItems);
        }

        if(Config.Games.SuperMetroid != null && Config.Games.Alttp != null)
        {
            // Add cross-game portals if both these games are enabled

            // Add norfair map to death moutain portal
            Graph.AddDirected(SMWorld!.GetLocation("Norfair - Business Center - Middle Left Door"), AlttpWorld!.GetLocation("West Death Mountain"), SMWorld!.GetItem("fixed"));
            Graph.AddDirected(AlttpWorld!.GetLocation("West Death Mountain"), SMWorld!.GetLocation("Norfair - Business Center - Middle Left Door"), AlttpWorld!.GetItem("fixed"));

            // Add maridia missile refill to dark world shopping mall
            Graph.AddDirected(SMWorld!.GetLocation("Maridia - Halfie Climb Room - Bottom Right Door"), AlttpWorld!.GetLocation("Dark Shopping Mall"), SMWorld!.GetItem("fixed"));
            Graph.AddDirected(AlttpWorld!.GetLocation("Dark Shopping Mall"), SMWorld!.GetLocation("Maridia - Halfie Climb Room - Bottom Right Door"), AlttpWorld!.GetItem("fixed"));

            // Add lower norfair refill to mire area
            Graph.AddDirected(SMWorld!.GetLocation("Norfair - Screw Attack Room - Middle Right Door"), AlttpWorld!.GetLocation("Mire"), SMWorld!.GetItem("fixed"));
            Graph.AddDirected(AlttpWorld!.GetLocation("Mire"), SMWorld!.GetLocation("Norfair - Screw Attack Room - Middle Right Door"), AlttpWorld!.GetItem("fixed"));
        }

    }

    public Inventory ComputeStartingItems()
    {
        var inventory = new Inventory([GetItem("fixed")]);
        if(AlttpWorld != null)
        {
            inventory.Merge(AlttpWorld.ComputeStartingItems());
        } 
        if(SMWorld != null)
        {
            inventory.Merge(SMWorld.ComputeStartingItems());
        }
        if (Z1World != null)
        {
            inventory.Merge(Z1World.ComputeStartingItems());
        }
        if (M1World != null)
        {
            inventory.Merge(M1World.ComputeStartingItems());
        }

        return inventory;
    }

    /// <summary>
    /// Get a vertex by name in this world.
    /// </summary>
    /// <param name="locationName">name to search for</param>
    public BaseVertex GetLocation(string locationName)
    {
        return Graph.GetVertex($"{locationName}:{Id}");
    }

    public bool HasLocation(string locationName)
    {
        return Graph.HasVertex($"{locationName}:{Id}");
    }

    /// <summary>Get all vertices in this world.</summary>
    /// <returns></returns>
    public IEnumerable<BaseVertex> GetLocations() => Graph.GetVertices().Where(vertex => vertex.World.Id == this.Id);
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
    
    public IEnumerable<BaseVertex> GetEmptyLocationsInSet(ISearcher searcher, IItem itemToPlace, ItemSetName itemSet, Dictionary<ItemSetName, int> setCounts)
    {
        var locations = new List<BaseVertex>();

        locations.AddRange(searcher.GetEmptyLocationsInSet(itemSet, setCounts));

        return locations;
    }

    public void TrackPlacedItem(BaseVertex location)
    {
        location.World.PlacedItemCount++;
    }
    
    public bool IsWinnable(BaseVertex start, Inventory startingInventory)
    {
        var searcher = GetSearcherForWorld(Graph, Start, startingInventory);
        if (AlttpWorld != null && !searcher.HasFound(AlttpWorld.GetItem("Triforce")))
        {
            return false;
        }

        if (Z1World != null && !searcher.HasFound(Z1World.GetItem("Zelda")))
        {
            return false;
        }

        if (SMWorld != null && !searcher.HasFound(SMWorld.GetItem("f_DefeatedMotherBrain")))
        {
            return false;
        }

        if (M1World != null && !searcher.HasFound(M1World.GetItem("DefeatedSilverTwo")))
        {
            return false;
        }

        return true;
    }

    public ISearcher GetSearcherForWorld(Graph graph, BaseVertex? start, Inventory inventory, SetLocations? setLocations = null)
    {
        return new ComboSearcher(graph, (Vertex)(start ?? Start), inventory, setLocations);
    }

}
