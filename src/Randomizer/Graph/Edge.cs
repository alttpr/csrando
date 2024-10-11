namespace Randomizer.Graph;

using System.Diagnostics;

/// <summary>Edge in Graph.</summary>
[DebuggerDisplay("{Condition}: {From.Name} -> {To.Name}")]
public sealed class Edge
{
    public Vertex From { get; }
    public Vertex To { get; set; }
    public ItemCondition Condition { get; set; }

    public Edge(Vertex from, Vertex to, ItemCondition condition)
    {
        (From, To, Condition) = (from, to, condition);
    }

    public void Deconstruct(out Vertex from, out Vertex to) => (from, to) = (From, To);
    public void Deconstruct(out Vertex from, out Vertex to, out ItemCondition condition) => (from, to, condition) = (From, To, Condition);
}

[DebuggerDisplay("{Item.Name}:{Item.World.Id} >= {Count}")]
public record ItemCondition(IItem Item, int Count)
{
    public bool IsUnconditional { get; } = Item.Name == "fixed";
}
