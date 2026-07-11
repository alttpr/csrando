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
    public MapInfo ? MapInfo { get; set; }
    public RequirementHandler RequirementHandler { get; } = new();

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
        StartingItems.AddItem(GetItem("f_TourianOpen"));

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
        }

        var preprocessor = new GraphPreprocessor(JsonData, this);
        preprocessor.Preprocess();

        Start = GetLocation(Map != null
            ? "Crateria - Crateria Map Room - Left Door"
            : "Crateria - Landing Site - Bottom Left Door");
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
