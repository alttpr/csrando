namespace Randomizer.Games.SuperMetroid;

using Randomizer.Games.SuperMetroid.Model;
using BaseVertex = Graph.Vertex;

public sealed class Vertex : BaseVertex
{
    public int RoomId { get; init; }
    public int NodeId { get; init; }
    public Node? Node { get; init; }
}
