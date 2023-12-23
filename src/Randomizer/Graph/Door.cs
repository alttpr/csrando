namespace Randomizer.Graph;

public sealed class Door
{
    public required Item Unlock { get; init; }
    public HashSet<(Vertex A, Vertex B)> Vertices { get; init; } = new();
}
