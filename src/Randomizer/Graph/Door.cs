namespace Randomizer.Graph;

public sealed class Door
{
    public required IItem Unlock { get; init; }
    public HashSet<(Vertex A, Vertex B)> Vertices { get; init; } = [];
}
