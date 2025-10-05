namespace Randomizer.Games.Goonies2;

using BaseVertex = Graph.Vertex;

public sealed class Vertex : BaseVertex
{
    public bool Dark { get; init; }
    public bool Water { get; init; }

    protected override void AdditionalGraphDumpMetadata(Dictionary<string, object?> metadata)
    {
        base.AdditionalGraphDumpMetadata(metadata);
        if (Dark)
            metadata["G2.Dark"] = "yes";
        if (Water)
            metadata["G2.Water"] = "yes";
    }
}
