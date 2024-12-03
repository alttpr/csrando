using Randomizer.Graph;

namespace Randomizer.Games.SuperMetroid;

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
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Varia")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Gravity")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Charge")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Ice")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Wave")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Plasma")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Spazer")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("XRayScope")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Grapple"   )),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SpringBall")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("HiJump")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SpaceJump")),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("PowerBomb")), 4),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Super")), 4),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Missile")), 7),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("ETank")), 8),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("ReserveTank")), 4),

            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("Missile")), 31),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("Super")), 10),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("PowerBomb")), 5),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("ETank")), 6),

            new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("Morph")),
            new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("ScrewAttack")),
            new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("Bombs")),
            new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("SpeedBooster")),
            new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("Missile")),
            new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("Super")),
            new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("PowerBomb")),

        ];

        return worldSet;
    }
}
