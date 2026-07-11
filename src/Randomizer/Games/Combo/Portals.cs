namespace Randomizer.Games.Combo;

/// <summary>
/// One game's side of a cross-game portal connection.
///
/// Every game's transition table row has the same shape:
///   [SourceRowPrefix..., partner game index, partner destination id, partner args]
/// SM and Z1 use a one-word prefix (door pointer / room), ALttP and M1 a two-word prefix
/// (room + overworld scroll position / map cell + door direction). A connection between
/// two sides therefore fully determines one row in each side's table — which is what
/// makes portal locations and connections randomizable: pick sides, pair them, and the
/// tables and graph edges follow.
/// </summary>
/// <param name="GameId">Game id ("sm", "alttp", "z1", "m1").</param>
/// <param name="GameIndex">The game index used in transition tables (sm 0, alttp 1, z1 2, m1 3).</param>
/// <param name="SourceRowPrefix">Leading words of this side's own transition table row.</param>
/// <param name="DestinationId">Game-specific destination id other games use to arrive here.</param>
/// <param name="DestinationArgs">Game-specific destination arguments.</param>
/// <param name="VertexName">Graph vertex cross-game edges attach to, when the game models
/// its portal as a real location (M1 anchors do; the legacy sides wire their own edges).</param>
public record PortalSide(string GameId, int GameIndex, uint[] SourceRowPrefix,
    int DestinationId, int DestinationArgs, string? VertexName = null)
{
    /// <summary>This side's transition table row for a connection to <paramref name="partner"/>.</summary>
    public uint[] RowTo(PortalSide partner) =>
        [.. SourceRowPrefix, (uint)partner.GameIndex, (uint)partner.DestinationId, (uint)partner.DestinationArgs];
}

/// <summary>A two-way cross-game portal: each side gets a row pointing at the other.</summary>
public record PortalConnection(PortalSide A, PortalSide B);

/// <summary>
/// The fixed (vanilla) portal sides of the games whose anchors are not generated yet.
/// M1 sides come from <see cref="Metroid.World.PortalAnchors"/> instead.
/// </summary>
public static class VanillaPortalSides
{
    // SM anchors and the ALttP sides they pair with, in vanilla table order.
    public static readonly (PortalSide Sm, PortalSide Alttp)[] SmAlttp =
    [
        (new("sm", 0, [0xAE0C], 0xAE00, 0x0000),
         new("alttp", 1, [0x0122, 0x0035], 0x0200, 0x0000)),
        (new("sm", 0, [0xAF0C], 0xAF00, 0x0000),
         new("alttp", 1, [0x00E5, 0x0003], 0x0201, 0x0000)),
        (new("sm", 0, [0xAF8C], 0xAF80, 0x0000),
         new("alttp", 1, [0x010E, 0x0077], 0x0202, 0x0040)),
        (new("sm", 0, [0xB00C], 0xB000, 0x0000),
         new("alttp", 1, [0x0115, 0x0070], 0x0203, 0x0040)),
    ];

    public static readonly PortalSide Z1 = new("z1", 2, [0x0066], 0x0066, 0x0003);
    public static readonly PortalSide AlttpToZ1 = new("alttp", 1, [0x0122, 0x0011], 0x0220, 0x0000);

    /// <summary>The ALttP side of the M1 connection (room 0x011F, overworld scroll 0x0002).</summary>
    public static readonly PortalSide AlttpToM1 = new("alttp", 1, [0x011F, 0x0002], 0x0210, 0x0000);
}
