using Randomizer.Graph;

namespace Randomizer.Games.Alttp.WorldModifiers;

/// <summary>
/// Duplicate regions in dungeons with switches to be able to explore both peg states.
/// Dependent vertices such as pots and enemies stay the same, but are accessible from both version of the same region.
/// Pseudo transitions PegBlue and PegOrange are replaced in data with fixed transition in the corresponding version.
/// </summary>
internal sealed class DungeonPegStateCopier : IAlttpWorldModifier
{
    public void AdjustEdges(World world, PRNG prng)
    {
        int nextGroup = 0;
        Dictionary<Vertex, int> vertexToGroup = [];
        Dictionary<int, HashSet<Vertex>> groups = [];

        foreach (var vertex in world.Graph.GetVertices().OfType<Vertex>().Where(v => v.World == world && v.EntranceId != null))
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
                    foreach (var (_, baseTo) in next.Edges)
                    {
                        var to = (Vertex)baseTo;
                        if (to.OutletId != null)
                            continue;
                        if (to.Map != null)
                            continue;

                        if (vertexToGroup.TryGetValue(to, out int toGroup))
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
                            vertexToGroup.Add(to, currentGroup);
                            groups[currentGroup].Add(to);
                            nextVertices.Enqueue(to);
                        }
                    }
                }
            }
        }

        foreach (var dungeonsWithSwitches in groups.Where(i => i.Value.Any(v => v.Switch)))
        {
            if (dungeonsWithSwitches.Value.Count != 0)
                TransformRegionWithPegs(world, dungeonsWithSwitches.Value);
        }
    }

    static void TransformRegionWithPegs(World world, HashSet<Vertex> vertices)
    {
        List<Vertex> orangeVertices = [];
        List<Vertex> blueVertices = [];

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
            List<Edge> newEdges = [];
            foreach (var (from, baseTo, condition) in v.Edges)
            {
                var to = (Vertex)baseTo;
                if (condition.Item.Name == "PegOrange")
                    continue;
                if (to.OutletId != null)
                    continue;
                if (to.Map != null)
                    continue;

                var edgeTo = to;
                var edgeCondition = condition;
                if (to.Type == VertexType.Region)
                {
                    edgeTo = (Vertex)world.GetLocation($"{to.Name} (Blue)");
                    if (edgeCondition.Item.Name == "PegBlue")
                    {
                        edgeCondition = new ItemCondition(world.GetItem("fixed"), 1);
                    }
                    else if (edgeCondition.Item.Name.StartsWith("UnlockDoor:"))
                    {
                        var first = v;
                        var second = edgeTo;
                        if (from.Name.CompareTo(to.Name) > 0)
                            (first, second) = (second, first);

                        world.Graph.Doors.SelectMany(e => e.Value)
                            .Where(e => e.Key == edgeCondition.Item)
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
            List<Edge> newEdges = [];
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
