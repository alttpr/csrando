namespace Randomizer.Games.Metroid;

using Randomizer.Graph;
using BaseVertex = Graph.Vertex;
using Graph = Graph.Graph;

/// <summary>
/// One cross-game portal anchor in this M1 world. The combo layer pairs anchors across
/// games into connections, wires the graph through <paramref name="VertexName"/>, and
/// emits the transition tables from the ROM fields (see transition_tables.asm /
/// transition_in.asm):
/// </summary>
/// <param name="Name">Human-readable name for spoilers.</param>
/// <param name="VertexName">The portal room's door vertex; cross-game edges attach here.</param>
/// <param name="RoomWord">(doorCellX &lt;&lt; 8) | Y — matched by SamusInDoor when leaving M1.</param>
/// <param name="Direction">Door direction value in the transition table ($0004 = east-side door).</param>
/// <param name="DestinationId">(portalCellX &lt;&lt; 8) | Y — other games target this to enter M1.</param>
/// <param name="DestinationArgs">D|S|area bits: bit15 entry direction (0 = through an east-side
/// door), bit14 portal room scroll (0 = horizontal), bits 0-2 area index.</param>
public record PortalAnchor(string Name, string VertexName, int RoomWord, int Direction, int DestinationId, int DestinationArgs);

/// <summary>Model of a world in which a player would be playing.</summary>
public sealed class World : Randomizer.Graph.World<Item>
{
    public Config Config { get; }
    public PRNG Prng { get; }
    public YamlReader.YamlData? YamlData { get; set; }
    public Dictionary<int, byte[]>? PatchData { get; set; }

    /// <summary>The generated map when MapShuffle is enabled; drives spoilers and ROM emission.</summary>
    public MapGen.GeneratedWorld? GeneratedMap { get; set; }

    /// <summary>
    /// Cross-game portal anchors: the vanilla fixed anchor, or the generated ones under
    /// map shuffle. Standalone seeds leave them as inert dead-end rooms.
    /// </summary>
    public List<PortalAnchor> PortalAnchors { get; } = [];

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

    public Inventory ComputeStartingItems() =>
        new Inventory([GetItem("fixed"), .. Config.StartingEquipment.Select(GetItem)]);

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
