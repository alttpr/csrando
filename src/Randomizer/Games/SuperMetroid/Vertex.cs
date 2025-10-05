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

    protected override void AdditionalGraphDumpMetadata(Dictionary<string, object?> metadata)
    {
        base.AdditionalGraphDumpMetadata(metadata);
        if (Node is { Name: string name })
            metadata["SM.Node"] = name;
        if (!string.IsNullOrWhiteSpace(LogicalName))
            metadata["SM.LogicalName"] = LogicalName;
    }
}
