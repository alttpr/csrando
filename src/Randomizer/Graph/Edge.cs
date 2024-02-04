namespace Randomizer.Graph;

using Combo.SuperMetroid;
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
public record ItemCondition(Item Item, int Count, ComplexRequirement? ComplexRequirement = null)
{
    public bool IsUnconditional { get; } = Item.Name == "fixed" || (ComplexRequirement != null && ComplexRequirement.IsUnconditional());
}
