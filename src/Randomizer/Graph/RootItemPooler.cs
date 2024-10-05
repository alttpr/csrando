namespace Randomizer.Graph;

using global::Randomizer.Games.Alttp;

internal sealed class RootItemPooler : IItemPooler
{
    public PooledItem[] Pool { get; }
    public SetLocations SetLocations { get; }

    public RootItemPooler(IWorld[] worlds, PRNG prng)
    {
        // TODO: get this from the worlds themselves, or some other way that doesn't hardcode every game's pooler here.
        var alttpPooler = new ItemPooler(worlds, prng);

        Pool = alttpPooler.Pool;
        SetLocations = alttpPooler.SetLocations;
    }
}
