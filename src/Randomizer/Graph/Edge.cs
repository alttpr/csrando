namespace AlttpRandomizer.Graph;

using System.Diagnostics;

/**
 * Edge in Graph.
 */
[DebuggerDisplay("{Group}: {From.Name} -> {To.Name}")]
public sealed class Edge
{
    public Vertex From { get; }
    public Vertex To { get; }
    public string Group { get; set; }
    public Edge(Vertex from, Vertex to, string group)
    {
        (From, To, Group) = (from, to, group);
    }
}
