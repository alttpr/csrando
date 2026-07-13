using Randomizer.Graph;

namespace Randomizer.Games.Combo;

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

        foreach(var world in worlds.OfType<World>())
        {
            if(world.AlttpWorld != null)
            {
                var alttpPooler = new Alttp.ItemPooler([world.AlttpWorld], _prng);
                var alttpLocations = alttpPooler.SetLocations.All();
                foreach (var (itemSet, locations) in alttpLocations)
                {
                    foreach (var location in locations)
                    {
                        setLocations.Add(location, itemSet);
                    }
                }
            }
            if (world.SMWorld != null)
            {
                var smPooler = new SuperMetroid.ItemPooler([world.SMWorld], _prng);
                var smLocations = smPooler.SetLocations.All();
                foreach (var (itemSet, locations) in smLocations)
                {
                    foreach (var location in locations)
                    {
                        setLocations.Add(location, itemSet);
                    }
                }
            }
            if (world.Z1World != null)
            {
                var z1Pooler = new Zelda1.ItemPooler([world.Z1World], _prng);
                var z1Locations = z1Pooler.SetLocations.All();
                foreach (var (itemSet, locations) in z1Locations)
                {
                    foreach (var location in locations)
                    {
                        setLocations.Add(location, itemSet);
                    }
                }
            }
            if (world.M1World != null)
            {
                var m1Pooler = new Metroid.ItemPooler([world.M1World], _prng);
                var m1Locations = m1Pooler.SetLocations.All();
                foreach (var (itemSet, locations) in m1Locations)
                {
                    foreach (var location in locations)
                    {
                        setLocations.Add(location, itemSet);
                    }
                }
            }
            return setLocations;
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
        var pool = new List<PooledItem>();
        pool.AddRange(world.AlttpWorld == null ? [] : new Alttp.ItemPooler([world.AlttpWorld], _prng).Pool);
        pool.AddRange(world.SMWorld == null ? [] : new SuperMetroid.ItemPooler([world.SMWorld], _prng).Pool);
        pool.AddRange(world.Z1World == null ? [] : new Zelda1.ItemPooler([world.Z1World], _prng).Pool);
        pool.AddRange(world.M1World == null ? [] : new Metroid.ItemPooler([world.M1World], _prng).Pool);

        // Patch item pool for quad
        if(world.AlttpWorld != null)
        {
            pool.RemoveAll(p => p.Item.Name == "ProgressiveBow");
            pool.Add((ItemSetName.DefaultSet, 3, world.AlttpWorld.GetItem("Bow")));
            pool.Add((ItemSetName.DefaultSet, 3, world.AlttpWorld.GetItem("SilverArrowUpgrade")));
        }

        // M1 pads maps larger than its safe capacity-upgrade pool with scoped Nothing
        // items. In Combo, prefer globally placeable junk from a game that has it so
        // those slots participate in the shared filler pool without leaking Nothing.
        var m1Nothing = world.M1World == null
            ? []
            : pool.Where(item => ReferenceEquals(item.Item.World, world.M1World)
                    && item.Item.Name == "Nothing")
                .ToList();
        if (m1Nothing.Count > 0)
        {
            IItem? trash = world.AlttpWorld?.GetItem("FiveRupees");
            trash ??= world.Z1World?.GetItem("Rupee5");
            if (trash != null)
            {
                foreach (var nothing in m1Nothing)
                    pool.Remove(nothing);
                pool.AddRange(Enumerable.Repeat(
                    new PooledItem(ItemSetName.DefaultSet, 9999, trash),
                    m1Nothing.Count));
            }
        }

        return pool;
    }
}
