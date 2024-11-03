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
        var z1Pooler = new Games.Zelda1.ItemPooler(worlds, prng);

        Pool = z1Pooler.Pool;
        SetLocations = z1Pooler.SetLocations;
    }
}
