namespace Randomizer.Games.Combo;

using System;
using System.Linq;
using Randomizer.Games;
using Randomizer.RomModifications;

/// <summary>
/// Writes every game's cross-game transition tables from the portal layout derived from
/// the graph (see <see cref="World.DerivePortalEdges"/>). Each portal edge contributes
/// one row to its source game's table (see <see cref="PortalAnchor.RowTo"/> for the row
/// shape); each present game's table ends with a $0000 terminator. Rows are grouped by
/// partner game in a fixed order to match the layout the original hardcoded tables used.
/// OnExit anchors go to their game's "out" table (the portal fires when leaving the
/// interior) instead of the "in" table. Connected anchors' ROM patches (portal room
/// data, arrival spawn rows, save stations) are written last.
/// </summary>
internal class PortalWriter
{
    private static readonly (string GameId, int TableAddress, int? OutTableAddress, string[] PartnerOrder)[] Tables =
    [
        // SM/M1 door transitions have no separate exit concept; Z1's out table has no
        // reserved ROM space yet (code follows the terminator at PC 0x63B002).
        ("sm",    0x300000, null, ["alttp", "z1", "m1"]),
        ("alttp", 0x550000, 0x542000, ["sm", "z1", "m1"]),
        ("z1",    0x63A000, null, ["alttp", "sm", "m1"]),
        ("m1",    Metroid.RomWriter.TransitionTableAddress, null, ["alttp", "sm", "z1"]),
    ];

    public static void WritePortals(IRom rom, World world)
    {
        var portalEdges = world.DerivePortalEdges();

        foreach (var (gameId, tableAddress, outTableAddress, partnerOrder) in Tables)
        {
            if (world.GameWorld(gameId) == null)
                continue;

            WriteTable(rom, portalEdges, gameId, tableAddress, partnerOrder, PortalTrigger.OnEnter);

            if (outTableAddress != null)
                WriteTable(rom, portalEdges, gameId, outTableAddress.Value, partnerOrder, PortalTrigger.OnExit);
            else if (portalEdges.Any(e => e.From.GameId == gameId && e.From.Trigger == PortalTrigger.OnExit))
                throw new NotSupportedException($"Game '{gameId}' has no on-exit transition table");
        }

        WriteAnchorPatches(rom, portalEdges);

        // The base patch pre-converts four SM rooms into portal rooms. Any of them the
        // layout did not link (e.g. no ALttP present) still carries an unused portal door
        // that loops back into the room; revert those to plain rooms. A room is linked
        // when a connected SM anchor names its converted portal room.
        if (world.SMWorld is { } sm)
        {
            var connectedNames = portalEdges
                .SelectMany(edge => new[] { edge.From, edge.To })
                .Where(anchor => anchor.GameId == "sm")
                .Select(anchor => anchor.Name)
                .ToHashSet();
            var linkedRoomNames = sm.PortalRooms
                .Where(room => connectedNames.Contains(room.Name))
                .Select(room => room.RoomName)
                .ToHashSet();
            SuperMetroid.Portals.RevertUnusedBaseConversions(rom, linkedRoomNames);
        }
    }

    private static void WriteTable(IRom rom, IReadOnlyList<PortalEdge> portalEdges, string gameId,
        int tableAddress, string[] partnerOrder, PortalTrigger trigger)
    {
        int address = tableAddress;
        foreach (var partner in partnerOrder)
        {
            foreach (var (from, to) in portalEdges)
            {
                if (from.GameId != gameId || from.Trigger != trigger || to.GameId != partner)
                    continue;

                foreach (var value in from.RowTo(to))
                {
                    rom.Write(address, BitConverter.GetBytes((ushort)value));
                    address += 2;
                }
            }
        }

        rom.Write(address, [0x00, 0x00]);
    }

    private static void WriteAnchorPatches(IRom rom, IReadOnlyList<PortalEdge> portalEdges)
    {
        var written = new HashSet<PortalAnchor>(ReferenceEqualityComparer.Instance);
        foreach (var (from, to) in portalEdges)
        {
            foreach (var anchor in new[] { from, to })
            {
                if (!written.Add(anchor))
                    continue;
                foreach (var patch in anchor.RomPatches)
                    rom.Write(patch.Address, patch.Data);
            }
        }
    }
}
