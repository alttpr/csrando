namespace Randomizer.Games.Alttp.WorldModifiers;

using Randomizer.Graph;

internal interface IAlttpWorldModifier : IWorldModifier
{
    void IWorldModifier.AdjustEdges(IWorld world, PRNG prng) => AdjustEdges((World)world, prng);
    void AdjustEdges(World world, PRNG prng);
}
