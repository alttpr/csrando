namespace Randomizer.Games;

/// <summary>
/// Implemented by every game world that can host cross-game portals. The combo layer
/// treats all games uniformly through this interface: it connects vertices with
/// cross-game edges, and each endpoint is resolved into a <see cref="PortalAnchor"/> on
/// demand — any ALttP entrance, any convertible SM room, any Z1 overworld cave, any
/// generated M1 portal room door.
/// </summary>
public interface IPortalHost
{
    /// <summary>Anchors materialized so far — purely a resolution cache, never
    /// pre-populated (standalone games carry no portal wiring). Its order is the anchor
    /// resolution order, which fixes the transition table row order.</summary>
    List<PortalAnchor> PortalAnchors { get; }

    /// <summary>Resolves one of this world's vertices into its portal anchor, creating
    /// the anchor on demand where the game supports it. Throws when the vertex cannot
    /// host a portal.</summary>
    PortalAnchor ResolvePortalAnchor(Graph.Vertex vertex);
}

public static class PortalHostExtensions
{
    /// <summary>The already-materialized anchor a vertex belongs to, if any. A vertex
    /// matches its anchor through either the entry or the exit vertex.</summary>
    public static PortalAnchor? FindPortalAnchor(this IPortalHost host, Graph.Vertex vertex) =>
        host.PortalAnchors.Find(a => a.EntryVertexName == vertex.Name || a.ExitVertexName == vertex.Name);
}

/// <summary>When a portal anchor fires in its own game.</summary>
public enum PortalTrigger
{
    /// <summary>Fires when the player enters the anchor's entrance/door (the common case).</summary>
    OnEnter,
    /// <summary>Fires when the player exits the anchor's interior back out (ALttP only for
    /// now; matched by the game's transition_table_out).</summary>
    OnExit,
}

/// <summary>A ROM write (PC address in the combo ROM) an anchor needs to function.</summary>
public record RomPatch(int Address, byte[] Data);

/// <summary>
/// One game's ROM-facing description of a portal endpoint, derived from a graph vertex.
///
/// The combo layer stores connections as graph edges. When it sees a cross-game edge,
/// each endpoint's world resolves the touched vertex into this metadata: which vertex is
/// the entry, which vertex is the exit, how the source game's transition hook matches
/// the exit, and how other games arrive here. A directed graph edge then yields one
/// transition table row:
///   [RowPrefix..., partner game index, partner destination id, partner args]
/// Endpoints can also carry ROM patches (extra door data, spawn table rows) that are
/// written when — and only when — a graph edge actually uses the endpoint.
/// </summary>
/// <param name="Name">Human-readable name for spoilers/debugging.</param>
/// <param name="GameId">Game id ("sm", "alttp", "z1", "m1").</param>
/// <param name="GameIndex">The game index used in transition tables (sm 0, alttp 1, z1 2, m1 3).</param>
/// <param name="RowPrefix">Leading words of this game's own transition table row — the
/// game-specific key its transition hook matches when the player leaves through this
/// portal (SM: door pointer; ALttP: room + overworld area, or room + 0 for OnExit;
/// Z1: overworld map id; M1: room word + door direction).</param>
/// <param name="DestinationId">Game-specific destination id other games use to arrive here.</param>
/// <param name="DestinationArgs">Game-specific destination arguments.</param>
/// <param name="EntryVertexName">Graph vertex arrivals attach to.</param>
/// <param name="ExitVertexNameOverride">Graph vertex departures leave from, when it differs
/// from the entry vertex (SM portals and ALttP entrances have separate In/Out vertices).</param>
public record PortalAnchor(
    string Name,
    string GameId, int GameIndex,
    uint[] RowPrefix,
    int DestinationId, int DestinationArgs,
    string EntryVertexName,
    string? ExitVertexNameOverride = null)
{
    public string ExitVertexName => ExitVertexNameOverride ?? EntryVertexName;

    /// <summary>When this anchor fires in its own game (selects the transition table
    /// its row is written to).</summary>
    public PortalTrigger Trigger { get; init; } = PortalTrigger.OnEnter;

    /// <summary>ROM writes this anchor needs (portal room door data, arrival spawn table
    /// rows, save station entries). Written by the portal writer for connected anchors.</summary>
    public IReadOnlyList<RomPatch> RomPatches { get; init; } = [];

    /// <summary>This anchor's transition table row for a connection to <paramref name="partner"/>.</summary>
    public uint[] RowTo(PortalAnchor partner) =>
        [.. RowPrefix, (uint)partner.GameIndex, (uint)partner.DestinationId, (uint)partner.DestinationArgs];

    /// <summary>An SM anchor: a portal-converted room. The door pointer and destination id
    /// are the room's portal out/in door data pointers (bank $83).</summary>
    public static PortalAnchor Sm(string name, uint doorPointer, int destinationId, int destinationArgs,
        string entryVertexName, string exitVertexName)
        => new(name, "sm", 0, [doorPointer], destinationId, destinationArgs, entryVertexName, exitVertexName);

    /// <summary>An ALttP anchor. Destination ids index the RoomToOutlet extension rows
    /// ($0200+, z3randomizer tables.asm) whose byte is the entrance's outlet id; the
    /// outlet system then spawns Link at that entrance (see Alttp.Portals).</summary>
    public static PortalAnchor Alttp(string name, uint roomId, uint overworldArea,
        int destinationId, int destinationArgs, string entryVertexName, string? exitVertexName = null)
        => new(name, "alttp", 1, [roomId, overworldArea], destinationId, destinationArgs,
            entryVertexName, exitVertexName);

    /// <summary>A Z1 anchor: any overworld map with a cave. The cave-entry hook matches the
    /// map id; arrivals use the same map id with args bit0 = exit-from-cave, bit1 = stairs.</summary>
    public static PortalAnchor Z1(string name, uint mapId, int destinationArgs, string vertexName)
        => new(name, "z1", 2, [mapId], (int)mapId, destinationArgs, vertexName);

    /// <summary>An M1 anchor: any horizontal door (portal room host doors included).
    /// RoomWord is (cellX &lt;&lt; 8) | Y and direction the door side (1 = right, 2 = left),
    /// matched by SamusEnterDoor when leaving; DestinationId is the arrival cell; args
    /// are side|alternate-palette|scroll|area bits ($80 = arrive through the left door,
    /// $40 = vertical scroll, $20 = alternate area palette, bits 0-2 area index) — see
    /// m1 transition_in/out.asm.</summary>
    public static PortalAnchor M1(string name, int roomWord, int direction,
        int destinationId, int destinationArgs, string vertexName)
        => new(name, "m1", 3, [(uint)roomWord, (uint)direction], destinationId, destinationArgs, vertexName);
}
