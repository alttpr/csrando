namespace Randomizer.Games.Alttp;

using Randomizer.Games;

/// <summary>
/// Creates cross-game portal anchors from ALttP entrances. Any entrance works:
///
/// - Leaving: z3 transition_out matches (interior room id, overworld area at $7EC140) on
///   cave entry — both derived from the live graph, so entrance shuffle is handled
///   automatically. OnExit anchors match the interior room id alone when leaving it.
/// - Arriving: the destination id indexes the RoomToOutlet extension rows ($0200-$029F,
///   z3randomizer tables.asm); the row's byte is the entrance's outlet id, which the
///   outlet system (overworldoutlets.asm NewOutletData) uses to spawn Link at the
///   entrance. The anchor carries that one-byte row as a ROM patch.
/// </summary>
public static class Portals
{
    /// <summary>RoomToOutlet (z3randomizer tables.asm, SNES $30EB00) in the combo ROM:
    /// LoROM PC 0x186B00 plus the ALttP game offset 0x400000.</summary>
    private const int RoomToOutletAddress = 0x586B00;

    /// <summary>First RoomToOutlet extension row available to dynamically created anchors
    /// (0x0200-0x0203, 0x0210 and 0x0220 are the vanilla portal rows).</summary>
    public const int FirstDynamicDestinationId = 0x0230;

    /// <summary>Last usable RoomToOutlet extension row (table ends at 0x029F).</summary>
    public const int LastDestinationId = 0x029F;

    /// <summary>
    /// Destination ids the vanilla portal entrances keep (their RoomToOutlet rows are
    /// prefilled in the base ROM). Purely cosmetic — any free row works — but keeping
    /// them makes the vanilla layout's ROM output stable.
    /// </summary>
    private static readonly Dictionary<string, int> PreferredDestinationIds = new()
    {
        ["Lake Hylia Fortune Teller"] = 0x0200, // SM Crateria Map Station slot
        ["Old Man Home Circle"] = 0x0201,       // SM Norfair Map Station slot
        ["Hint Giver Cave"] = 0x0202,           // SM Maridia Missile Refill slot
        ["Mire Big Fairy"] = 0x0203,            // SM Golden Torizo Refill slot
        ["Lumberjacks House"] = 0x0210,         // M1 slot
        ["Kakariko Fortune Teller"] = 0x0220,   // Z1 slot
    };

    /// <summary>
    /// Resolves a graph vertex to its entrance's portal anchor, creating the anchor on
    /// first use. This is how cross-game edges materialize ALttP portals: point an edge
    /// at any entrance's In/Out vertex and the whole portal follows.
    /// </summary>
    public static PortalAnchor ResolveVertexAnchor(World world, string vertexName)
    {
        string entranceName;
        if (vertexName.EndsWith(" - In"))
            entranceName = vertexName[..^" - In".Length];
        else if (vertexName.EndsWith(" - Out"))
            entranceName = vertexName[..^" - Out".Length];
        else
            throw new Exception($"ALttP vertex '{vertexName}' is not an entrance and cannot host a portal");

        return world.PortalAnchors.Find(a => a.Name == entranceName)
            ?? CreateEntranceAnchor(world, entranceName);
    }

    /// <summary>
    /// Creates a portal anchor for <paramref name="entranceName"/> and registers it on
    /// the world. Must run after the world modifiers (the entrance shuffler wires the
    /// In-vertex to its — possibly shuffled — interior, which the row key needs).
    /// </summary>
    /// <param name="destinationId">RoomToOutlet row to use; the entrance's vanilla row
    /// when it has one, otherwise allocated from <see cref="FirstDynamicDestinationId"/>.</param>
    public static PortalAnchor CreateEntranceAnchor(World world, string entranceName,
        int? destinationId = null, PortalTrigger trigger = PortalTrigger.OnEnter)
    {
        if (destinationId == null && PreferredDestinationIds.TryGetValue(entranceName, out int preferred))
            destinationId = preferred;
        var entranceIn = (Vertex)world.GetLocation($"{entranceName} - In");
        var entranceOut = (Vertex)world.GetLocation($"{entranceName} - Out");

        int overworldArea = entranceIn.Map
            ?? throw new Exception($"Entrance '{entranceName}' has no overworld map");
        int outletId = entranceOut.OutletId
            ?? throw new Exception($"Entrance '{entranceName}' has no outlet id");
        int roomId = FindInteriorRoomId(entranceIn)
            ?? throw new Exception($"Entrance '{entranceName}' leads to no room with a room id");

        int destination = destinationId ?? AllocateDestinationId(world);
        if (destination is < 0x0200 or > LastDestinationId)
            throw new ArgumentOutOfRangeException(nameof(destinationId),
                $"Destination id 0x{destination:X4} outside the RoomToOutlet extension rows");

        var anchor = PortalAnchor.Alttp(entranceName,
            (uint)roomId,
            // OnExit rows are keyed on the interior room alone; the second word is reserved.
            trigger == PortalTrigger.OnEnter ? (uint)overworldArea : 0,
            destination,
            // Bit 6 marks a dark world arrival (unused by the current z3 arrival code,
            // kept for parity with the original hardcoded rows).
            destinationArgs: overworldArea >= 0x40 ? 0x0040 : 0x0000,
            entryVertexName: entranceOut.Name,
            exitVertexName: entranceIn.Name) with
        {
            Trigger = trigger,
            RomPatches = [new RomPatch(RoomToOutletAddress + destination, [(byte)outletId])],
        };

        world.PortalAnchors.Add(anchor);
        return anchor;
    }

    /// <summary>The interior room the entrance leads to in the current (post-shuffle) graph.</summary>
    private static int? FindInteriorRoomId(Vertex entranceIn)
    {
        foreach (var edge in entranceIn.Edges)
        {
            if (edge.To is Vertex { RoomId: not null } direct)
                return direct.RoomId;
            // Some entrances route through a transition node first.
            foreach (var inner in edge.To.Edges)
            {
                if (inner.To is Vertex { RoomId: not null } indirect)
                    return indirect.RoomId;
            }
        }

        return null;
    }

    private static int AllocateDestinationId(World world)
    {
        int next = world.PortalAnchors
            .Select(a => a.DestinationId)
            .Where(id => id >= FirstDynamicDestinationId)
            .DefaultIfEmpty(FirstDynamicDestinationId - 1)
            .Max() + 1;
        if (next > LastDestinationId)
            throw new Exception("No free RoomToOutlet extension rows left for portal anchors");
        return next;
    }
}
