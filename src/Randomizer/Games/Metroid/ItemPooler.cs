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
        // The assumed filler places lower weights FIRST, while the assumed inventory is
        // still rich; the items placed last must land in locations reachable with almost
        // nothing. So the heaviest progression gates (Morph, Missile for red doors, Bombs)
        // go first and the situational upgrades go last. Generated map-shuffle worlds have
        // a small "reachable with nothing" sphere, so placing Morph or Bombs last would
        // leave them with no legal location.
        List<PooledItem> worldSet =
        [
            new PooledItem(ItemSetName.DefaultSet, 1, world.GetItem("Morph")),
            new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("Missile")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Bombs")),
            new PooledItem(ItemSetName.DefaultSet, 4, world.GetItem("IceBeam")),
            new PooledItem(ItemSetName.DefaultSet, 5, world.GetItem("Varia")),
            new PooledItem(ItemSetName.DefaultSet, 5, world.GetItem("HiJump")),
            new PooledItem(ItemSetName.DefaultSet, 5, world.GetItem("LongBeam")),
            new PooledItem(ItemSetName.DefaultSet, 5, world.GetItem("WaveBeam")),
            new PooledItem(ItemSetName.DefaultSet, 5, world.GetItem("ScrewAttack")),
            new PooledItem(ItemSetName.DefaultSet, 5, world.GetItem("EnergyTank")),

            ..Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("Missile")), 20),
            ..Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("EnergyTank")), 6),

        ];

        return worldSet;
    }
}
