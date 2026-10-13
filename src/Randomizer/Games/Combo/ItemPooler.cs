using Randomizer.Graph;

namespace Randomizer.Games.Combo;

/// <summary>Get the sets of items to place.</summary>
/// <param name="worlds">worlds to get Item pools for</param>
internal sealed class ItemPooler : IItemPooler
{
    private readonly Dictionary<IWorld, IItemPooler[]> _poolersForWorld = [];

    private readonly PRNG _prng;

    public ItemPooler(IWorld[] worlds, PRNG prng)
    {
        _prng = prng;
        foreach (var world in worlds.OfType<World>())
        {
            _poolersForWorld[world] = world.GameWorlds().Select<IWorld, IItemPooler>(w => w switch
            {
                Alttp.World alttpWorld => new Alttp.ItemPooler([alttpWorld], _prng),
                SuperMetroid.World smWorld => new SuperMetroid.ItemPooler([smWorld], _prng),
                Zelda1.World z1World => new Zelda1.ItemPooler([z1World], _prng),
                Metroid.World m1World => new Metroid.ItemPooler([m1World], _prng),
                _ => throw new NotSupportedException("Unsupported world found."),
            }).ToArray();
        }
        Pool = [.. worlds.OfType<World>().SelectMany(GetPoolForWorld)];
        SetLocations = BuildLocations(worlds);
    }

    private SetLocations BuildLocations(IWorld[] worlds)
    {
        var setLocations = new SetLocations();

        foreach (var world in worlds.OfType<World>())
        {
            if (!_poolersForWorld.TryGetValue(world, out var poolers))
                continue;

            foreach (var (itemSet, locations) in poolers.SelectMany(pooler => pooler.SetLocations.All()))
            {
                foreach (var location in locations)
                {
                    setLocations.Add(location, itemSet);
                }
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
        var pool = new List<PooledItem>();
        if (_poolersForWorld.TryGetValue(world, out var poolers))
            pool.AddRange(poolers.SelectMany(pooler => pooler.Pool));

        // Patch item pool for quad
        if (world.AlttpWorld != null)
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
            else if (world.SMWorld != null)
            {
                // No Zelda game to supply trash: pad with SM ammo packs instead. The
                // in-game pickup adds without clamping, so the pack caps here are the
                // only thing keeping the counters at 255/95/95.
                var smWorld = world.SMWorld;
                int Packs(string name) => pool.Count(item =>
                    ReferenceEquals(item.Item.World, smWorld) && item.Item.Name == name);
                // Largest-headroom-first keeps the mix roughly proportional without
                // spending PRNG; ties resolve in array order.
                var names = new[] { "Missile", "PowerBomb", "Super" };
                var left = new[]
                {
                    SuperMetroid.ItemPooler.MaximumMissilePacks - Packs("Missile"),
                    SuperMetroid.ItemPooler.MaximumPowerBombPacks - Packs("PowerBomb"),
                    SuperMetroid.ItemPooler.MaximumSuperPacks - Packs("Super"),
                };
                foreach (var nothing in m1Nothing)
                {
                    int pick = 0;
                    for (int i = 1; i < left.Length; i++)
                    {
                        if (left[i] > left[pick])
                            pick = i;
                    }

                    if (left[pick] <= 0)
                        break;
                    left[pick]--;
                    pool.Remove(nothing);
                    pool.Add(new PooledItem(ItemSetName.DefaultSet, 9001, smWorld.GetItem(names[pick])));
                }
            }
        }

        return pool;
    }
}
