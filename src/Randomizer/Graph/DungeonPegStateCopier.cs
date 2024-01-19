namespace Randomizer.Graph;

/// <summary>
/// Duplicate regions in dungeons with switches to be able to explore both peg states.
/// Dependent vertices such as pots and enemies stay the same, but are accessible from both version of the same region.
/// Pseudo transitions PegBlue and PegOrange are replaced in data with fixed transition in the corresponding version.
/// </summary>
internal sealed class DungeonPegStateCopier : IWorldModifier
{
    public static void AdjustEdges(World world, PRNG prng)
    {
        int nextGroup = 0;
        Dictionary<Vertex, int> vertexToGroup = new();
        Dictionary<int, HashSet<Vertex>> groups = new();

        foreach (var vertex in world.Graph.GetVertices().Where(v => v.EntranceId != null))
        {
            if (!vertexToGroup.ContainsKey(vertex))
            {
                int currentGroup = nextGroup++;
                vertexToGroup.Add(vertex, currentGroup);
                groups.Add(currentGroup, [vertex]);

                Queue<Vertex> nextVertices = new();
                nextVertices.Enqueue(vertex);

                while (nextVertices.TryDequeue(out var next))
                {
                    foreach (var edge in next.Edges)
                    {
                        if (edge.To.OutletId != null)
                            continue;
                        if (edge.To.Map != null)
                            continue;

                        if (vertexToGroup.TryGetValue(edge.To, out int toGroup))
                        {
                            if (toGroup == currentGroup)
                                continue;

                            foreach (var oldGroupVertex in groups[currentGroup])
                            {
                                vertexToGroup[oldGroupVertex] = toGroup;
                            }
                            groups[toGroup].UnionWith(groups[currentGroup]);
                            groups.Remove(currentGroup);
                            currentGroup = toGroup;
                        }
                        else
                        {
                            vertexToGroup.Add(edge.To, currentGroup);
                            groups[currentGroup].Add(edge.To);
                            nextVertices.Enqueue(edge.To);
                        }
                    }
                }
            }
        }

        foreach (var dungeonsWithSwitches in groups.Where(i => i.Value.Any(v => v.Switch)))
        {
            if (dungeonsWithSwitches.Value.Any())
                TransformRegionWithPegs(world, dungeonsWithSwitches.Value);
        }
    }

    static void TransformRegionWithPegs(World world, HashSet<Vertex> vertices)
    {
        List<Vertex> orangeVertices = new();
        List<Vertex> blueVertices = new();

        foreach (var v in vertices)
        {
            if (v.Type != VertexType.Region)
                continue;

            var blueVertex = (Vertex)v.Clone();
            blueVertex.Name = $"{blueVertex.Name} (Blue)";
            blueVertices.Add(blueVertex);
            world.Graph.AddVertex(blueVertex);
            orangeVertices.Add(v);
        }

        foreach (var v in blueVertices)
        {
            List<Edge> newEdges = new();
            foreach (var edge in v.Edges)
            {
                if (edge.Condition.Item.Name == "PegOrange")
                    continue;

                var edgeTo = edge.To;
                var edgeCondition = edge.Condition;
                if (edge.To.Type == VertexType.Region)
                {
                    edgeTo = world.GetLocation($"{edge.To.Name} (Blue)");
                    if (edge.Condition.Item.Name == "PegBlue")
                    {
                        edgeCondition = new ItemCondition(world.GetItem("fixed"), 1);
                    }
                    if (edge.Condition.Item.Name.StartsWith("UnlockDoor:"))
                    {
                        var first = v;
                        var second = edgeTo;
                        if (edge.From.Name.CompareTo(edge.To.Name) > 0)
                            (first, second) = (second, first);

                        world.Graph.Doors.SelectMany(e => e.Value)
                            .Where(e => e.Key == edge.Condition.Item)
                            .First().Value.Add((first, second));
                    }
                }
                newEdges.Add(new Edge(v, edgeTo, edgeCondition));
            }
            if (v.Switch)
            {
                v.Switch = false;
                newEdges.Add(new Edge(v, world.GetLocation(v.Name.Replace(" (Blue)", "")), new ItemCondition(world.GetItem("fixed"), 1)));
            }
            v.Edges = newEdges;
        }

        foreach (var v in orangeVertices)
        {
            List<Edge> newEdges = new();
            foreach (var edge in v.Edges)
            {
                if (edge.Condition.Item.Name == "PegBlue")
                    continue;

                var edgeCondition = edge.Condition;
                if (edge.Condition.Item.Name == "PegOrange")
                {
                    edgeCondition = new ItemCondition(world.GetItem("fixed"), 1);
                }

                newEdges.Add(new Edge(edge.From, edge.To, edgeCondition));
            }
            if (v.Switch)
            {
                v.Switch = false;
                newEdges.Add(new Edge(v, world.GetLocation($"{v.Name} (Blue)"), new ItemCondition(world.GetItem("fixed"), 1)));
            }
            v.Edges = newEdges;
        }
    }
}
