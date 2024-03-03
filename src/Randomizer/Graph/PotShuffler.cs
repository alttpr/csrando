namespace Randomizer.Graph;

/// <summary>Prepares the world to shuffle items into pots.</summary>
internal sealed class PotShuffler : IWorldModifier
{
    public static void AdjustEdges(World world, PRNG rng)
    {
        if (world.Config.PotShuffle == PotShuffleOption.None)
            return;

        var potVertices = world.Graph.GetVertices().Where(v => v.World == world && v.Type == VertexType.Pot && v.Item != null);
        // FIXME: switches are something different, and shuffling them might have logic implications. ignore them for now.
        potVertices = potVertices.Where(v => v.Sprite?.Name != "FloorSwitch");
        if (!world.Config.RegionWildKeys)
            potVertices = potVertices.Where(v => v.Item!.Type != ItemType.SmallKey);
        //foreach (var potVertex in potVertices)
        potVertices.AsParallel().ForAll(potVertex =>
        {
            // free up the location for placement.
            potVertex.Item = null;
        });
    }
}
