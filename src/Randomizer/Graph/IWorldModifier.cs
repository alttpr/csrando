namespace Randomizer.Graph;

internal interface IWorldModifier
{
    void AdjustEdges(World world, PRNG rng);
}
