using Randomizer.Graph;

namespace Randomizer.Games.Metroid;

/// <summary>Get the sets of items to place.</summary>
/// <param name="worlds">worlds to get Item pools for</param>
internal sealed class ItemPooler : IItemPooler
{

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
                setLocations.Add(vertex, [ItemSetName.DefaultSet, .. vertex.ItemSet]);
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
        List<PooledItem> worldSet =
        [
            new PooledItem(ItemSetName.DefaultSet, 4, world.GetItem("Morph")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("IceBeam")),

            new PooledItem(ItemSetName.DefaultSet, 4, world.GetItem("Bombs")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Varia")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("HiJump")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("LongBeam")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("WaveBeam")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("ScrewAttack")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("EnergyTank")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Missile")),
            
            ..Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("Missile")), 20),
            ..Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("EnergyTank")), 6),

        ];

        return worldSet;
    }
}
