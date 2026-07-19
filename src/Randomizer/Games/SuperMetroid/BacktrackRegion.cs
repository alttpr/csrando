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
        SmSearchModel model,
        bool[] membership,
        IReadOnlySet<Vertex> vertices,
        IReadOnlyDictionary<Vertex, IReadOnlyList<Randomizer.Graph.Edge>> incomingEdges)
    {
        Model = model;
        Membership = membership;
        Vertices = vertices;
        IncomingEdges = incomingEdges;
    }

    internal SmSearchModel Model { get; }
    internal bool[] Membership { get; }

    public IReadOnlySet<Vertex> Vertices { get; }

    /// <summary>
    /// Original directed edges grouped by their destination. An edge found under
    /// vertex A still describes <c>edge.From -&gt; A</c>; its strategies are never
    /// reversed or replaced with those from the opposite direction.
    /// </summary>
    public IReadOnlyDictionary<Vertex, IReadOnlyList<Randomizer.Graph.Edge>> IncomingEdges { get; }

    public static BacktrackRegion Build(Graph graph, Vertex target)
    {
        var model = SmSearchModel.For(target.World, graph);

        var region = new HashSet<Vertex>();
        var membership = new bool[model.Capacity];
        var queue = new Queue<int>();

        AddSeed(target);

        // Target searches currently consider leaving the SM world a successful
        // backtrack (see StatefulSearcher.InternalSearch). Those exits are
        // therefore additional terminals of the reverse traversal.
        foreach (int exitId in model.CrossWorldTerminalIds)
            AddSeed(model.VerticesById[exitId]!);

        while (queue.TryDequeue(out int currentId))
        {
            foreach (var incomingEdge in model.IncomingEdgesById[currentId])
            {
                var predecessor = (Vertex)incomingEdge.From;
                if (!membership[predecessor.Id])
                    AddSeed(predecessor);
            }
        }

        return new BacktrackRegion(
            model,
            membership,
            region,
            model.VertexIds.ToDictionary(
                id => model.VerticesById[id]!,
                id => (IReadOnlyList<Randomizer.Graph.Edge>)
                    model.IncomingEdgesById[id]));

        void AddSeed(Vertex vertex)
        {
            if (membership[vertex.Id])
                return;
            membership[vertex.Id] = true;
            region.Add(vertex);
            queue.Enqueue(vertex.Id);
        }
    }

    internal bool Contains(Vertex vertex) =>
        (uint)vertex.Id < (uint)Membership.Length && Membership[vertex.Id];
}
