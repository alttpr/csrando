namespace AlttpRandomizer.Graph;

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
    public Edge(Vertex from, Vertex to, Item item, int itemCount)
    {
        (From, To, Condition) = (from, to, new(item, itemCount));
    }
    public Edge(Vertex from, Vertex to, ItemCondition condition)
    {
        From = from;
        To = to;
        Condition = condition;
    }
}

[DebuggerDisplay("{Item.Name}:{Item.WorldId} >= {Count}")]
public record ItemCondition(Item Item, int Count);
