namespace Randomizer.Graph;
/// <summary>
/// Add new nodes and edges to the graph based on Blue/Orange switches.
/// </summary>
internal sealed class CrystalSwitchGraphifier : IWorldModifier
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
            System.Console.WriteLine($"Found dungeon with switch that contains {dungeonsWithSwitches.Value.First().Name}");
        }

        System.Console.WriteLine($"Found {groups.Count} groups");
    }
}
