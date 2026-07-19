namespace Randomizer.Games.SuperMetroid;

using Randomizer.Games.SuperMetroid.Model;
using BaseVertex = Graph.Vertex;

public sealed class Vertex : BaseVertex
{
    public int RoomId { get; init; }
    public int NodeId { get; init; }
    public Node? Node { get; init; }

    /// <summary>
    /// User-facing name for internal state vertices which represent arriving at a
    /// door with a particular entrance condition.
    /// </summary>
    public string? LogicalName { get; init; }
}
