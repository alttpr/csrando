namespace Randomizer.Games.SuperMetroid;

using Randomizer.Graph;

/// <summary>
/// The vertices from which a target search can structurally reach its target.
///
/// This reverses only the graph index: every incoming entry still represents the
/// original directed <c>edge.From -&gt; edge.To</c> transition. Requirements and
/// state are deliberately not interpreted here, so membership is a necessary,
/// but not sufficient, condition for a successful backtrack.
/// </summary>
internal sealed class BacktrackRegion
{
    private BacktrackRegion(
        IReadOnlySet<Vertex> vertices,
        IReadOnlyDictionary<Vertex, IReadOnlyList<Randomizer.Graph.Edge>> incomingEdges)
    {
        Vertices = vertices;
        IncomingEdges = incomingEdges;
    }

    public IReadOnlySet<Vertex> Vertices { get; }

    /// <summary>
    /// Original directed edges grouped by their destination. An edge found under
    /// vertex A still describes <c>edge.From -&gt; A</c>; its strategies are never
    /// reversed or replaced with those from the opposite direction.
    /// </summary>
    public IReadOnlyDictionary<Vertex, IReadOnlyList<Randomizer.Graph.Edge>> IncomingEdges { get; }

    public static BacktrackRegion Build(Graph graph, Vertex target)
    {
        var vertices = graph.GetVertices()
            .OfType<Vertex>()
            .Where(vertex => vertex.World == target.World)
            .ToArray();
        var incoming = vertices.ToDictionary(
            vertex => vertex,
            _ => new List<Randomizer.Graph.Edge>());

        foreach (var from in vertices)
        {
            foreach (var edge in from.Edges)
            {
                if (edge.To.World == from.World && edge.To is Vertex to
                    && incoming.TryGetValue(to, out var predecessors))
                {
                    // Keep the direction of the original edge. Walking this list
                    // backwards means that `from` can potentially reach `to`.
                    predecessors.Add(edge);
                }
            }
        }

        var region = new HashSet<Vertex>();
        var queue = new Queue<Vertex>();

        AddSeed(target);

        // Target searches currently consider leaving the SM world a successful
        // backtrack (see StatefulSearcher.InternalSearch). Those exits are
        // therefore additional terminals of the reverse traversal.
        foreach (var exit in vertices.Where(vertex =>
                     vertex.Edges.Any(edge => edge.To.World != vertex.World)))
        {
            AddSeed(exit);
        }

        while (queue.TryDequeue(out var current))
        {
            foreach (var incomingEdge in incoming[current])
            {
                var predecessor = (Vertex)incomingEdge.From;
                if (region.Add(predecessor))
                    queue.Enqueue(predecessor);
            }
        }

        return new BacktrackRegion(
            region,
            incoming.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyList<Randomizer.Graph.Edge>)pair.Value));

        void AddSeed(Vertex vertex)
        {
            if (region.Add(vertex))
                queue.Enqueue(vertex);
        }
    }
}
