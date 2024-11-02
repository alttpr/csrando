namespace Randomizer.Games.Goonies2;

using BaseVertex = Graph.Vertex;

public sealed class Vertex : BaseVertex
{
    public bool Dark { get; init; }
    public bool Water { get; init; }
}
