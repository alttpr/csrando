namespace Randomizer.Graph;

internal interface IWorldModifier
{
    void AdjustEdges(IWorld world, PRNG rng);
}
