namespace Randomizer.Graph;
/// <summary>
/// update graph to understand dark rooms and how to get to them.
/// </summary>
internal sealed class DarknessGraphifier : IWorldModifier
{

    public static void AdjustEdges(World world, PRNG prng)
    {
        var graph = world.Graph;
        var darkRooms = graph.GetVertices().Where(v => v.Dark).ToList();
    }
}
