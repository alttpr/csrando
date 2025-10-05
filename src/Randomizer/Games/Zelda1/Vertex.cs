namespace Randomizer.Games.Zelda1;

using BaseVertex = Graph.Vertex;

public sealed class Vertex : BaseVertex
{
    public bool Dark { get; init; }
    protected override void AdditionalGraphDumpMetadata(Dictionary<string, object?> metadata)
    {
        base.AdditionalGraphDumpMetadata(metadata);
        if (Dark)
            metadata["Z1.Dark"] = "yes";
    }
}
