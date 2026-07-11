namespace Randomizer.Games.Zelda1;

using Randomizer.Games;
using Randomizer.Graph;
using BaseVertex = Graph.Vertex;
using Graph = Graph.Graph;

/// <summary>Model of a world in which a player would be playing.</summary>
public sealed class World : World<Item>, IPortalHost
{
    public Config Config { get; }
    public PRNG Prng { get; }
    public YamlReader.YamlData? YamlData { get; set; }
    internal List<DungeonSpoilerData> DungeonSpoilers { get; } = [];

    /// <summary>Cross-game portal anchors (see <see cref="Games.PortalAnchor"/>),
    /// materialized on demand from overworld cave vertices.</summary>
    public List<PortalAnchor> PortalAnchors { get; } = [];

    /// <summary>Any overworld cave vertex ("Overworld - Map XX - ...") can host a portal:
    /// the transition row keys on the map id alone. Caves outside the always-reserved
    /// candidates (see <see cref="DataLoader.PortalCaveCandidates"/>) keep their possibly
    /// shuffled cave, which becomes unreachable behind the portal.</summary>
    public PortalAnchor ResolvePortalAnchor(BaseVertex vertex)
    {
        if (this.FindPortalAnchor(vertex) is { } existing)
            return existing;

        var match = System.Text.RegularExpressions.Regex.Match(vertex.Name, @"^Overworld - Map ([0-9A-Fa-f]{2}) - ");
        if (!match.Success)
            throw new Exception($"Z1 vertex '{vertex.Name}' is not an overworld cave and cannot host a portal");

        int map = Convert.ToInt32(match.Groups[1].Value, 16);
        if (YamlData?.overworld_maps.Find(m => m.map == map) is not { cave: > 0 })
            throw new Exception($"Z1 map {map:X2} has no cave and cannot host a portal");

        var anchor = PortalAnchor.Z1($"Portal Cave {map:X2}", (uint)map,
            destinationArgs: 0x0003, vertexName: vertex.Name);
        PortalAnchors.Add(anchor);

        // Portal arrivals bypass the start vertex, so the Meta hub must be reachable
        // from the arrival cave.
        Graph.AddDirected(vertex, GetLocation("Overworld - Meta - Meta"), GetItem("fixed"));

        return anchor;
    }

    /// <summary>Add all the vertices to the graph for this region.</summary>
    /// <param name="id">id of this world</param>
    /// <param name="randomizerConfig">options for this world</param>
    public World(int id, WorldConfig randomizerConfig, Graph graph, PRNG prng)
        : base("z1", id, graph, randomizerConfig)
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
