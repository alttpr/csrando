namespace Randomizer.Graph;

using System.Diagnostics;
using System.Runtime.CompilerServices;


/**
 * Edge in Graph.
 */
[DebuggerDisplay("{Condition}: {From.Name} -> {To.Name}")]
public sealed class Edge
{
    public Vertex From { get; }
    public Vertex To { get; set; }
    public ItemCondition Condition { get; set; }
    public string FromPath { get; }
    public int FromLine { get; }

    public Edge(Vertex from, Vertex to, ItemCondition condition, [CallerFilePath] string fromPath = "unknown", [CallerLineNumber] int fromLine = 0)
    {
        (From, To, Condition) = (from, to, condition);
        (FromPath, FromLine) = (fromPath, fromLine);
    }
}

[DebuggerDisplay("{Item.Name}:{Item.World.Id} >= {Count}")]
public record ItemCondition(Item Item, int Count);
