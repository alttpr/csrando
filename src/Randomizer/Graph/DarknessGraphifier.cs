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
            foreach (var edge in lightRoom.Edges.Where(e => e.To.Dark).ToList())
            {
                var darkRoom = edge.To;
                var transition = new Vertex
                {
                    Type = VertexType.Region,
                    Name = $"{lightRoom.Name} - {darkRoom.Name} - :Lit",
                };

                world.Graph.AddVertex(transition);
                edge.To = transition;
                world.Graph.AddDirected(darkRoom, transition, world.GetItem("fixed"));
                world.Graph.AddDirected(transition, darkRoom, world.GetItem("Lamp"));
                world.Graph.AddDirected(transition, lightRoom, world.GetItem("fixed"));
            }
        }
    }
}
