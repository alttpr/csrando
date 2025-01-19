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
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Grapple")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SpringBall")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("HiJump")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SpaceJump")),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("PowerBomb")), 6),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Super")), 6),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Missile")), 11),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("ETank")), 10),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("ReserveTank")), 4),

            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("Missile")), 28),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("Super")), 9),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("PowerBomb")), 4),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("ETank")), 4),
            new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("XRayScope")),

            new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("Morph")),
            new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("ScrewAttack")),
            new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("Bombs")),
            new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("SpeedBooster")),

        ];

        // Add SM keycards to the pool if enabled
        if (world.Config.Keycards == Keycards.All)
        {
            worldSet.AddRange(
            [
                new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("CrateriaL1")),
                new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("CrateriaL2")),
                new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("CrateriaBoss")),

                new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("BrinstarL1")),
                new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("BrinstarL2")),
                new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("BrinstarBoss")),

                new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("NorfairL1")),
                new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("NorfairL2")),
                new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("NorfairBoss")),

                new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("WreckedShipL1")),
                new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("WreckedShipBoss")),

                new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("MaridiaL1")),
                new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("MaridiaL2")),
                new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("MaridiaBoss")),

                new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("LowerNorfairL1")),
                new PooledItem(ItemSetName.DefaultSet, 2, world.GetItem("LowerNorfairBoss")),
            ]);
        }

        return worldSet;
    }
}
