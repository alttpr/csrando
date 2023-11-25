namespace Randomizer.Graph;
/// <summary>
/// update graph to understand dark rooms and how to get to them.
/// </summary>
internal sealed class DarknessGraphifier : IWorldModifier
{
    /// <summary>
    /// This will create new transition nodes between light rooms and dark
    /// rooms, it will then add the correct edges so everything is connected. It
    /// does modify existing edges.
    /// </summary>
    /// <param name="world"></param>
    /// <param name="prng"></param>
    public static void AdjustEdges(World world, PRNG prng)
    {
        var graph = world.Graph;
        var lightRooms = graph.GetVertices().Where(v => !v.Dark).ToList();
        foreach (var lightRoom in lightRooms)
        {
            foreach (var edge in lightRoom.Edges.Where(e => e.To.Dark))
            {
                var darkRoom = edge.To;
                var transition = new Vertex
                {
                    Type = VertexType.Region,
                    Name = $"{lightRoom.Name} - {darkRoom.Name} - Dark Transition:{world.Id}",
                };

                var edgesToLight = darkRoom.Edges.Where(e => e.To == lightRoom);

                world.Graph.AddVertex(transition);
                edge.To = transition;
                world.Graph.AddDirected(transition, darkRoom, world.GetItem("Lamp"));
                world.Graph.AddDirected(transition, lightRoom, world.GetItem("fixed"));

                if (edgesToLight.Count() > 1)
                    throw new Exception("Uh oh, is the code really correct there?");

                foreach (var edge2 in edgesToLight)
                {
                    edge2.To = transition;
                    var oldCondition = edge2.Condition;
                    edge2.Condition = new ItemCondition(world.GetItem("fixed"), 1);

                    transition.Edges.Add(new Edge(transition, lightRoom, oldCondition));
                }
            }
        }
    }
}
