namespace Randomizer.Graph;

using System.Diagnostics;

/**
 * Edge in Graph.
 */
[DebuggerDisplay("{Item}|{ItemCount}: {From.Name} -> {To.Name}")]
public sealed class Edge
{
    public Vertex From { get; }
    public Vertex To { get; }
    public ItemCondition Condition { get; set; }
    public Edge(Vertex from, Vertex to, Item item, int itemCount) : this(from, to, new(item, itemCount)) { }

    public Edge(Vertex from, Vertex to, ItemCondition condition)
    {
        (From, To, Condition) = (from, to, condition);
        from.Edges.Add(this);
    }
}

[DebuggerDisplay("{Item.Name}:{Item.WorldId} >= {Count}")]
public record ItemCondition(Item Item, int Count);
