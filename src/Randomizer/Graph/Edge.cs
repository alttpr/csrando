namespace Randomizer.Graph;

using System.Diagnostics;

/**
 * Edge in Graph.
 */
[DebuggerDisplay("{Condition}: {From.Name} -> {To.Name}")]
public sealed class Edge
{
    public Vertex From { get; set; }
    public Vertex To { get; set; }
    public ItemCondition Condition { get; set; }

    public Edge(Vertex from, Vertex to, ItemCondition condition)
    {
        (From, To, Condition) = (from, to, condition);
    }
}

[DebuggerDisplay("{Item.Name}:{Item.World.Id} >= {Count}")]
public record ItemCondition(IItem Item, int Count)
{
    public bool IsUnconditional { get; } = Item.Name == "fixed";
}
