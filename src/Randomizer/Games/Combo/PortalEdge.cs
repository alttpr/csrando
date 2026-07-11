namespace Randomizer.Games.Combo;

using Randomizer.Games;

/// <summary>
/// One directed cross-game graph edge after both endpoint vertices have been resolved
/// into their ROM metadata. Each portal edge becomes exactly one transition table row
/// in the source game (see <see cref="PortalAnchor.RowTo"/>); a bidirectional portal is
/// simply two graph edges.
/// </summary>
public record PortalEdge(PortalAnchor From, PortalAnchor To);
