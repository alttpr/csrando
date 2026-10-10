namespace Randomizer.Games.SuperMetroid;

using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;
using BaseVertex = Graph.Vertex;
using Graph = Graph.Graph;

/// <summary>Model of a world in which a player would be playing.</summary>
public sealed class World : Randomizer.Graph.World<Item>, IPortalHost
{

    public Config Config { get; }
    public PRNG Prng { get; }
    public JsonReader JsonData { get; set; }
    public List<string> AllowedTechs { get; init; }
    public Map? Map { get; set; }
    public MapInfo? MapInfo { get; set; }
    public RequirementHandler RequirementHandler { get; } = new();

    /// <summary>The save station the seed starts at, or null for the default start
    /// (the Ship, or the Crateria Map Room under map randomization). Resolved by
    /// <see cref="ApplyConfiguredStartStation"/> after the combo portal edges exist.</summary>
    public SaveStation? StartStation { get; private set; }

    /// <summary>
    /// A room converted into a portal room by <see cref="Portals.ConvertRoom"/>: a second
    /// (portal) door was added so linking a portal here does not consume the room's real
    /// doorway. The patches apply the conversion and ride along when the room is linked.
    /// </summary>
    public record PortalRoom(string Name, string RoomName, string VertexName,
        ushort DoorOutPointer, ushort DoorInPointer, bool PortalOnLeft, int SaveStationSlot,
        IReadOnlyList<RomPatch> Patches);

    /// <summary>Portal rooms created for this world (creation is separate from linking;
    /// unlinked conversions write nothing to the ROM).</summary>
    public List<PortalRoom> PortalRooms { get; } = [];

    /// <summary>Cross-game portal anchors (see <see cref="Games.PortalAnchor"/>),
    /// materialized on demand by <see cref="Portals"/> from portal rooms — or from any
    /// plain door, which consumes that doorway's outgoing passage.</summary>
    public List<PortalAnchor> PortalAnchors { get; } = [];

    public PortalAnchor ResolvePortalAnchor(BaseVertex vertex) => Portals.ResolveVertexAnchor(this, vertex);

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

        JsonData = new JsonReader(Config);
        JsonData.Load();

        var randomizerDependent = JsonData.Helpers.HelperCategories
            .First(c => c.Name == "Randomizer Dependent");

        if (Config.SpawnAllItems)
        {
            foreach (ref var helper in randomizerDependent.Helpers.AsSpan())
            {
                if (helper.Name == "h_AllItemsSpawned")
                {
                    helper = helper with { Requires = new Requirement.Always() };
                    break;
                }
            }
        }


        AllowedTechs = Config.LogicTechs[Config.Logic].Concat(Config.CustomTech).ToList();
        RequirementHandler.Initialize(JsonData, this);

        if (Config.MapRandomizer == MapRandomizerSetting.Standard)
        {
            var mapRandomizer = new MapRandomizer(JsonData, this, prng);
            mapRandomizer.Randomize();
            if (Map != null)
                StartingItems.AddItem(GetItem("f_TourianOpen"));
        }

        var preprocessor = new GraphPreprocessor(JsonData, this);
        preprocessor.Preprocess();

        Start = GetLocation(Map != null
            ? "Crateria - Crateria Map Room - Left Door"
            : "Crateria - Landing Site - Bottom Left Door");
    }

    /// <summary>
    /// Resolves the configured start station and moves <see cref="World.Start"/> to
    /// it. Called by the combo world after the cross-game portal edges exist, because
    /// station viability (see <see cref="IsViableStart"/>) includes reaching a portal
    /// from the bare start pocket.
    /// </summary>
    public void ApplyConfiguredStartStation(PRNG prng)
    {
        StartStation = ResolveStartStation(prng);
        if (StartStation != null)
            Start = GetLocation(StartStation.VertexName);
    }

    /// <summary>
    /// The station requested by <see cref="Config.StartLocation"/>, or null for the
    /// default start. Draws from the PRNG only for a requested random start, so
    /// existing configs keep their seeds.
    /// </summary>
    private SaveStation? ResolveStartStation(PRNG prng)
    {
        if (!Config.ApplyStartLocation || !Config.StartLocationRequested)
            return null;

        var stations = SaveStations.EligibleStartStations(JsonData);
        if (Config.StartLocation == Config.RandomStartLocation)
        {
            var viable = stations.Where(IsViableStart).ToList();
            if (viable.Count == 0)
                throw new InvalidOperationException(
                    "No save station is a viable start under the current logic and map");
            return prng.GetRandomElement(viable);
        }

        var station = stations.Find(s => s.RoomName == Config.StartLocation)
            ?? throw new ArgumentException($"Unknown Super Metroid start location '{Config.StartLocation}'");
        if (!IsViableStart(station))
            throw new ArgumentException(
                $"Start location '{station.RoomName}' cannot reach an item location (and, in combo seeds, "
                + $"a cross-game portal) with an empty inventory under {Config.Logic} logic, "
                + "so no seed from it can be filled");
        return station;
    }

    /// <summary>
    /// A station is viable when an empty-inventory run from it reaches an item
    /// location and — in combo seeds — a cross-game portal. Both are filler
    /// invariants: the Morph front fill needs an itemless-reachable slot, and the
    /// last assumed-fill placements happen with a nearly empty inventory, so partner
    /// games must stay enterable from the bare pocket (why the default map-randomizer
    /// start is the Crateria Map Room portal). Evaluated per seed on the built graph.
    /// </summary>
    private bool IsViableStart(SaveStation station)
    {
        ISearcher searcher = new StatefulSearcher(Graph,
            (Vertex)GetLocation(station.VertexName), ComputeStartingItems());
        if (!searcher.GetVisited().Any(v => v.World == this && v.Type == VertexType.Item))
            return false;

        bool comboSeed = WorldConfig.Alttp != null || WorldConfig.Zelda1 != null || WorldConfig.Metroid != null;
        return !comboSeed || searcher.GetOtherWorld().Any();
    }

    public Inventory ComputeStartingItems()
    {
        var inventory = new Inventory([GetItem("fixed"), .. Config.StartingEquipment.Select(GetItem)]);
        inventory.AddItem(GetItem("f_ZebesAwake"));
        if (Map != null)
            inventory.AddItem(GetItem("f_TourianOpen"));
        return inventory;
    }

    protected override Item CreateItem(string name, IWorld world) => new(name, world);

    public new IEnumerable<Vertex> GetLocationsOfType(VertexType type) => GetLocations().Where(vertex => vertex.Type == type).Cast<Vertex>();

    public override bool IsWinnable(BaseVertex start, Inventory startingInventory)
    {
        var winSearcher = new StatefulSearcher(Graph, (Vertex)this.Start, startingInventory);
        return winSearcher.HasFound(GetItem("f_DefeatedMotherBrain"));
    }

    public override IEnumerable<IItem> GetVictoryItems() => [GetItem("f_DefeatedMotherBrain")];

    public override ISearcher GetSearcherForWorld(Graph graph, BaseVertex? start, Inventory inventory, SetLocations? setLocations = null)
    {
        return new StatefulSearcher(graph, (Vertex)(start ?? Start), inventory, setLocations);
    }
}
