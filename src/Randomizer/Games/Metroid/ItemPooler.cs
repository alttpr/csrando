using Randomizer.Graph;

namespace Randomizer.Games.Metroid;

/// <summary>Get the sets of items to place.</summary>
/// <param name="worlds">worlds to get Item pools for</param>
internal sealed class ItemPooler : IItemPooler
{
    internal const string NothingItemSet = "m1-nothing";
    internal const int MaximumMissiles = 21;
    internal const int MaximumEnergyTanks = 8;
    internal const int MaximumNonNothingItems = 37;

    private readonly PRNG _prng;

    public ItemPooler(IWorld[] worlds, PRNG prng)
    {
        _prng = prng;
        Pool = [.. worlds.OfType<World>().SelectMany(GetPoolForWorld)];
        SetLocations = BuildLocations(worlds);
    }

    private SetLocations BuildLocations(IWorld[] worlds)
    {
        var setLocations = new SetLocations();
        foreach (var vertex in worlds.SelectMany(world => world.GetLocations()).OfType<Vertex>())
        {
            if (vertex.Type == VertexType.Item)
            {
                setLocations.Add(vertex,
                    [ItemSetName.DefaultSet,
                        new ItemSetName(NothingItemSet, vertex.World), .. vertex.ItemSet]);
            }
        }
        return setLocations;
    }

    /// <summary>Get a list possible locations, keyed by item set.</summary>
    public SetLocations SetLocations { get; }
    /// <summary>Get list of all items in their weighted sets.</summary>
    public PooledItem[] Pool { get; }

    /// <summary>Get list of all items for <paramref name="world"/> in their weighted sets.</summary>
    private List<PooledItem> GetPoolForWorld(World world)
    {
        int locationCount = world.GetLocationsOfType(VertexType.Item).Count();
        const int progressionCount = 10;
        int fillerCount = Math.Max(0, locationCount - progressionCount);
        // Never exceed the historical capacity pool: one Missile and Energy Tank are
        // progression-weighted below, with at most 20 and 7 more as filler. Small maps
        // trim capacity filler; large maps use Nothing instead of unsafe extra tanks.
        int capacityFillerCount = Math.Min(
            fillerCount, MaximumNonNothingItems - progressionCount);
        int energyTanks = Math.Min(
            MaximumEnergyTanks - 1, Math.Max(0, capacityFillerCount - 14));
        int missiles = capacityFillerCount - energyTanks;
        int nothing = fillerCount - capacityFillerCount;

        // Morph is placed late so assumed fill tends to put it somewhere accessible
        // early. Constrained M1 starts front-fill Morph before this ordering.
        List<PooledItem> worldSet =
        [
            new PooledItem(ItemSetName.DefaultSet, 4, world.GetItem("Morph")),
            new PooledItem(ItemSetName.DefaultSet, 4, world.GetItem("Bombs")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("IceBeam")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Varia")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("HiJump")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("LongBeam")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("WaveBeam")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("ScrewAttack")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("EnergyTank")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Missile")),

            ..Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("Missile")), missiles),
            ..Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("EnergyTank")), energyTanks),
            ..Enumerable.Repeat(new PooledItem(
                new ItemSetName(NothingItemSet, world), 9999,
                world.GetItem("Nothing")), nothing),

        ];

        return worldSet;
    }
}
