namespace Randomizer.Games.Metroid;

using Randomizer.Graph;
using BaseVertex = Graph.Vertex;
using Graph = Graph.Graph;

/// <summary>Model of a world in which a player would be playing.</summary>
public sealed class World : Randomizer.Graph.World<Item>, IPortalHost
{
    public Config Config { get; }
    public PRNG Prng { get; }
    public YamlReader.YamlData? YamlData { get; set; }
    public Dictionary<int, byte[]>? PatchData { get; set; }

    /// <summary>The generated map when MapShuffle is enabled; drives spoilers and ROM emission.</summary>
    public MapGen.GeneratedWorld? GeneratedMap { get; set; }

    /// <summary>
    /// A physically generated portal room: the door cell coordinates and area the engine
    /// needs, recorded when the room is built (vanilla shaft candidates or map shuffle).
    /// Rooms not connected to a portal remain inert dead ends.
    /// </summary>
    /// <param name="VertexName">The portal room's door vertex.</param>
    /// <param name="Area">The M1 area that hosts this portal room.</param>
    /// <param name="RoomWord">(hostCellX &lt;&lt; 8) | Y — the host cell whose right-hand
    /// door leads to the portal room, matched by SamusEnterDoor when leaving.</param>
    /// <param name="Direction">Door side in the transition table (1 = right-hand door).</param>
    /// <param name="DestinationId">(hostCellX &lt;&lt; 8) | Y — arrivals scroll into the
    /// host room through its right-hand door; the portal room itself is only a safety
    /// net and is normally never entered.</param>
    /// <param name="DestinationArgs">side|alternate-palette|scroll|area bits (see m1 transition_in.asm).</param>
    public record PortalRoom(string Name, string VertexName, YamlReader.Area Area,
        int RoomWord, int Direction, int DestinationId, int DestinationArgs);

    /// <summary>The portal rooms built into this world's map (areas listed in
    /// <see cref="DataLoader.PortalRoomAreas"/> — rooms are physical map content).</summary>
    public List<PortalRoom> PortalRooms { get; } = [];

    /// <summary>Cross-game portal anchors (see <see cref="Games.PortalAnchor"/>),
    /// materialized on demand from portal room doors — or from any plain horizontal
    /// door, which consumes that doorway's outgoing passage.</summary>
    public List<PortalAnchor> PortalAnchors { get; } = [];

    /// <summary>Resolves a door vertex into its portal anchor: a built portal room's
    /// door, or any plain left/right door (see <see cref="ResolvePlainDoor"/>).</summary>
    public PortalAnchor ResolvePortalAnchor(BaseVertex vertex)
    {
        if (this.FindPortalAnchor(vertex) is { } existing)
            return existing;

        var room = PortalRooms.Find(r => r.VertexName == vertex.Name);
        var anchor = room != null
            ? PortalAnchor.M1(room.Name, room.RoomWord, room.Direction,
                room.DestinationId, room.DestinationArgs, room.VertexName)
            : ResolvePlainDoor(vertex);
        PortalAnchors.Add(anchor);

        // Portal arrivals bypass the start vertex, so the Meta hub (ability derivations
        // and the win condition) must be reachable from the arrival door.
        Graph.AddDirected(vertex, GetLocation("Meta - Metroid Meta Locations - Meta (0) - Meta"), GetItem("fixed"));

        return anchor;
    }

    /// <summary>
    /// A portal on a plain door: leaving through it warps cross-game (the transition
    /// table matches this cell + door side; side values 1 = right, 2 = left) and
    /// arrivals walk back in through the same doorway. The doorway's outgoing passage is
    /// consumed, so the edge to the neighboring room's door is severed.
    /// </summary>
    private PortalAnchor ResolvePlainDoor(BaseVertex vertex)
    {
        var (roomName, cell, side) = ParseDoorVertex(vertex.Name)
            ?? throw new Exception($"M1 vertex '{vertex.Name}' is not a left/right door and cannot host a portal");
        var room = YamlData!.rooms.Find(r => r.name == roomName)
            ?? throw new Exception($"M1 room '{roomName}' not found");

        // The outgoing passage now leads cross-game; drop the edge to the neighboring
        // room's door (the way back in stays — the neighbor's own door is untouched).
        vertex.Edges.RemoveAll(e =>
            e.To.World == vertex.World && ParseDoorVertex(e.To.Name)?.Room is { } other && other != roomName);

        return PortalAnchor.M1(vertex.Name,
            roomWord: cell.X << 8 | cell.Y,
            direction: side == DoorSide.Right ? 0x0001 : 0x0002,
            destinationId: cell.X << 8 | cell.Y,
            // Arrivals walk back in through this door: args bit $80 selects the door
            // side (set = left door, per m1_entry_get_door_side); vertical rooms set
            // the scroll bit.
            destinationArgs: (side == DoorSide.Left ? 0x0080 : 0x0000)
                | (room.scroll == YamlReader.Scrolling.Vertical ? 0x0040 : 0x0000)
                | (int)room.area,
            vertexName: vertex.Name);
    }

    private enum DoorSide { Left, Right }

    /// <summary>Parses "{Area} - {Room} - {Screen} ({index}) - Left/Right door" into the
    /// room, the door screen's map cell, and the door side.</summary>
    private (string Room, MapGen.Point Cell, DoorSide Side)? ParseDoorVertex(string vertexName)
    {
        var match = System.Text.RegularExpressions.Regex.Match(vertexName,
            @"^[^-]+ - (?<room>.+) - .+ \((?<index>\d+)\) - (?<side>Left|Right) door$");
        if (!match.Success)
            return null;

        var room = YamlData!.rooms.Find(r => r.name == match.Groups["room"].Value);
        if (room == null)
            return null;

        int index = int.Parse(match.Groups["index"].Value);
        int x = room.position[0] + (room.scroll == YamlReader.Scrolling.Horizontal ? index : 0);
        int y = room.position[1] + (room.scroll == YamlReader.Scrolling.Vertical ? index : 0);
        var side = match.Groups["side"].Value == "Right" ? DoorSide.Right : DoorSide.Left;
        return (room.name, new MapGen.Point(x, y), side);
    }

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

    public override IEnumerable<IItem> GetVictoryItems() => [GetItem("DefeatedSilverTwo")];

    public override ISearcher GetSearcherForWorld(Graph graph, BaseVertex? start, Inventory inventory, SetLocations? setLocations = null)
    {
        return new Searcher(graph, start ?? Start, inventory, setLocations, this);
    }
}
