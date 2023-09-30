namespace Randomizer.Graph;

internal interface IWorldModifier
{
    abstract static void AdjustEdges(World world, PRNG rng);
}
