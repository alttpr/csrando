namespace Randomizer.Graph;

using ItemSet = Dictionary<string, /* WeightedSet */ Dictionary<int, List<Item>>>;
using WeightedSet = Dictionary<int, List<Item>>;

/// <summary>Get the sets of items to place.</summary>
/// <param name="worlds">worlds to get Item pools for</param>
internal sealed class ItemPooler(World[] worlds, PRNG prng)
{
    /// <summary>Get list of all items in their weighted sets.</summary>
    public ItemSet GetPool()
    {
        var sets = new ItemSet();

        foreach (var world in worlds)
        {
            var worldSet = RecursivelyMerge(
                GetMedallions(world),
                GetPrizes(world),
                GetSmallKeys(world),
                GetBigKeys(world),
                GetMaps(world),
                GetCompasses(world),
                GetBottles(world),
                GetShopItems(world),
                new ItemSet
                {
                    { "*", new WeightedSet
                        {
                            // placing behind keys for now.
                            { 3, [
                                    world.GetItem("Hammer"),
                                    world.GetItem("Hookshot"),
                                    world.GetItem("Flippers"),
                                    world.GetItem("FireRod"),
                                    world.GetItem("IceRod"),
                                    world.GetItem("ProgressiveBow"),
                                    world.GetItem("ProgressiveBow"),
                                    world.GetItem("ProgressiveSword"),
                                    world.GetItem("ProgressiveSword"),
                                    world.GetItem("ProgressiveShield"),
                                    world.GetItem("ProgressiveShield"),
                                    world.GetItem("ProgressiveShield"),
                                    world.GetItem("PegasusBoots"),
                                    world.GetItem("BookOfMudora"),
                                    world.GetItem("ProgressiveGlove"),
                                    world.GetItem("ProgressiveGlove"),
                                    world.GetItem("CaneOfSomaria"),
                                    world.GetItem("CaneOfByrna"),
                                    world.GetItem("Cape"),
                                    world.GetItem("Lamp"),
                                    world.GetItem("Bombos"),
                                    world.GetItem("Ether"),
                                    world.GetItem("Quake"),
                                    world.GetItem("Mushroom"),
                                    world.GetItem("MoonPearl"),
                                    world.GetItem("MagicMirror"),
                                    world.GetItem("OcarinaInactive"),
                                    world.GetItem("Shovel"),
                                    world.GetItem("BugCatchingNet"),
                                    world.GetItem("Powder"),
                                    world.GetItem("HalfMagic"),
                                ]
                            },
                            { 9001, [
                                    world.GetItem("Boomerang"),
                                    world.GetItem("RedBoomerang"),
                                    world.GetItem("HeartContainer"),
                                    .. Enumerable.Repeat(world.GetItem("ProgressiveSword"), 2),
                                    .. Enumerable.Repeat(world.GetItem("ProgressiveArmor"), 2),
                                    .. Enumerable.Repeat(world.GetItem("BossHeartContainer"), 10),
                                    .. Enumerable.Repeat(world.GetItem("PieceOfHeart"), 24),
                                ]
                            },
                            // order here matters, items at end may get lopped off
                            // if too many items to place
                            { 9999, [
                                    world.GetItem("Arrow"),
                                    world.GetItem("OneHundredRupees"),
                                    .. Enumerable.Repeat(world.GetItem("TenArrows"), 12),
                                    .. Enumerable.Repeat(world.GetItem("ThreeBombs"), 17),
                                    .. Enumerable.Repeat(world.GetItem("OneRupee"), 2),
                                    .. Enumerable.Repeat(world.GetItem("FiveRupees"), 4),
                                    .. Enumerable.Repeat(world.GetItem("TwentyRupees"), 28),
                                    .. Enumerable.Repeat(world.GetItem("FiftyRupees"), 7),
                                    .. Enumerable.Repeat(world.GetItem("ThreeHundredRupees"), 5),
                                ]
                            },
                        }
                    },
                }
            );

            if (
                world.Config.Glitches != GlitchesOption.None
                && (world.Config.State == StateOption.Inverted
                    || !(world.Config.Glitches is GlitchesOption.Overworld or GlitchesOption.Major))
            )
            {
                float crystalRatio = world.Config.CrystalsTower / 7f;
                int fillCount = world.Config.Goal is GoalOption.TriforceHunt or GoalOption.Pedestal
                    ? prng.GetRandomInt((int)(15 * crystalRatio), (int)(25 * crystalRatio))
                    : prng.GetRandomInt((int)(15 * crystalRatio));
                if (fillCount > 0)
                {
                    var junkFill = prng.Shuffle(worldSet["*"][9999]).Take(fillCount).ToArray();
                    foreach (var key in junkFill)
                    {
                        worldSet["gt:" + world.Id].TryAdd(2, []);
                        worldSet["gt:" + world.Id][2].Add(key);
                        worldSet["*"][9999].Remove(key);
                    }
                }
            }

            sets = RecursivelyMerge(
                sets,
                worldSet
            );
        }

        return sets;
    }

    private static ItemSet RecursivelyMerge(params ItemSet[] itemSets)
    {
        var result = new ItemSet();

        foreach (var itemSet in itemSets)
        {
            foreach (var (location, prioritySets) in itemSet)
            {
                if (!result.TryGetValue(location, out var locationSet))
                    result[location] = locationSet = [];
                foreach (var (priority, items) in prioritySets)
                {
                    if (!locationSet.TryGetValue(priority, out var prioritySet))
                        locationSet[priority] = prioritySet = [];

                    prioritySet.AddRange(items);
                }
            }
        }

        return result;
    }

    private static readonly string[] _mireEntry = ["MireEntryBombos", "MireEntryEther", "MireEntryQuake"];
    private static readonly string[] _trEntry = ["TurtleRockEntryBombos", "TurtleRockEntryEther", "TurtleRockEntryQuake"];
    /// <summary>
    /// Get Medallions meta locations for what ends up being required for TR/MM
    /// entry.
    /// </summary>
    /// <param name="world">world to get items for</param>
    private ItemSet GetMedallions(World world)
    {
        return new ItemSet
        {
            { "mm-medallion:" + world.Id, new WeightedSet
                {
                    { 0, [ world.GetItem(prng.GetRandomElement(_mireEntry)) ] },
                }
            },
            { "tr-medallion:" + world.Id, new WeightedSet
                {
                    { 0, [ world.GetItem(prng.GetRandomElement(_trEntry)) ] },
                }
            },
        };
    }

    /// <summary>Get Prizes for a world.</summary>
    /// <param name="world">world to get items for</param>
    private ItemSet GetPrizes(World world)
    {
        return new ItemSet
        {
            { "prize:" + world.Id, new WeightedSet
                {
                    { 0, [
                             world.GetItem("PendantOfCourage"),
                             world.GetItem("PendantOfWisdom"),
                             world.GetItem("PendantOfPower"),
                             world.GetItem("Crystal1"),
                             world.GetItem("Crystal2"),
                             world.GetItem("Crystal3"),
                             world.GetItem("Crystal4"),
                             world.GetItem("Crystal5"),
                             world.GetItem("Crystal6"),
                             world.GetItem("Crystal7"),
                        ]
                    },
                }
            },
        };
    }

    /// <summary>Get Small keys for world in proper placement groups.</summary>
    /// <param name="world">world to get items for</param>
    private ItemSet GetSmallKeys(World world)
    {
        var keys = new ItemSet
        {
            { "escape:" + world.Id, new WeightedSet
                {
                    { 1, [ world.GetItem("KeyH2") ] },
                }
            },
            { "desert:" + world.Id, new WeightedSet
                {
                    { 1, [ world.GetItem("KeyP2") ] },
                }
            },
            { "hera:" + world.Id, new WeightedSet
                {
                    { 1, [ world.GetItem("KeyP3") ] },
                }
            },
            { "agahnim:" + world.Id, new WeightedSet
                {
                    { 1, [.. Enumerable.Repeat(world.GetItem("KeyA1"), 2)] },
                }
            },
            { "pod:" + world.Id, new WeightedSet
                {
                    { 1, [.. Enumerable.Repeat(world.GetItem("KeyD1"), 6)] },
                }
            },
            { "swamp:" + world.Id, new WeightedSet
                {
                    { 1, [ world.GetItem("KeyD2") ] },
                }
            },
            { "skull:" + world.Id, new WeightedSet
                {
                    { 1, [.. Enumerable.Repeat(world.GetItem("KeyD3"), 3)] },
                }
            },
            { "thieves:" + world.Id, new WeightedSet
                {
                    { 1, [ world.GetItem("KeyD4") ] },
                }
            },
            { "ice:" + world.Id, new WeightedSet
                {
                    { 1, [.. Enumerable.Repeat(world.GetItem("KeyD5"), 2)] },
                }
            },
            { "mire:" + world.Id, new WeightedSet
                {
                    { 1, [.. Enumerable.Repeat(world.GetItem("KeyD6"), 3)] },
                }
            },
            { "turtlerock:" + world.Id, new WeightedSet
                {
                    { 1, [.. Enumerable.Repeat(world.GetItem("KeyD7"), 4)] },
                }
            },
            { "gt:" + world.Id, new WeightedSet
                {
                    { 1, [.. Enumerable.Repeat(world.GetItem("KeyA2"), 4)] },
                }
            },
        };

        if (world.Config.RegionWildKeys)
        {
            return new ItemSet
            {
                { "*",
                    new WeightedSet
                    {
                        { 3, [.. keys.Values.SelectMany(dungeon => dungeon.Values.SelectMany(item => item))] }
                    }
                }
            };
        }

        return keys;
    }

    /// <summary>Get Big keys for world in proper placement groups.</summary>
    /// <param name="world">world to get items for</param>
    private ItemSet GetBigKeys(World world)
    {
        var bigKeys = new ItemSet
        {
            { "eastern:" + world.Id, new WeightedSet
                {
                    { 1, [ world.GetItem("BigKeyP1") ] },
                }
            },
            { "desert:" + world.Id, new WeightedSet
                {
                    { 1, [ world.GetItem("BigKeyP2") ] },
                }
            },
            { "hera:" + world.Id, new WeightedSet
                {
                    { 1, [ world.GetItem("BigKeyP3") ] },
                }
            },
            { "pod:" + world.Id, new WeightedSet
                {
                    { 1, [ world.GetItem("BigKeyD1") ] },
                }
            },
            { "swamp:" + world.Id, new WeightedSet
                {
                    { 2, [ world.GetItem("BigKeyD2") ] },
                }
            },
            { "skull:" + world.Id, new WeightedSet
                {
                    { 2, [ world.GetItem("BigKeyD3") ] },
                }
            },
            { "thieves:" + world.Id, new WeightedSet
                {
                    { 1, [ world.GetItem("BigKeyD4") ] },
                }
            },
            { "ice:" + world.Id, new WeightedSet
                {
                    { 1, [ world.GetItem("BigKeyD5") ] },
                }
            },
            { "mire:" + world.Id, new WeightedSet
                {
                    { 1, [ world.GetItem("BigKeyD6") ] },
                }
            },
            { "turtlerock:" + world.Id, new WeightedSet
                {
                    { 1, [ world.GetItem("BigKeyD7") ] },
                }
            },
            { "gt:" + world.Id, new WeightedSet
                {
                    { 0, [ world.GetItem("BigKeyA2") ] },
                }
            },
        };

        if (world.Config.RegionWildBigKeys)
        {
            return new ItemSet
            {
                { "*",
                    new WeightedSet
                    {
                        { 3, [.. bigKeys.Values.SelectMany(dungeon => dungeon.Values.SelectMany(item => item))] }
                    }
                }
            };
        }

        return bigKeys;
    }

    /// <summary>Get Maps for world in proper placement groups.</summary>
    /// <param name="world">world to get items for</param>
    private ItemSet GetMaps(World world)
    {
        var maps = new ItemSet
        {
            { "escape:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("MapH2") ] },
                }
            },
            { "eastern:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("MapP1") ] },
                }
            },
            { "desert:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("MapP2") ] },
                }
            },
            { "hera:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("MapP3") ] },
                }
            },
            { "pod:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("MapD1") ] },
                }
            },
            { "swamp:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("MapD2") ] },
                }
            },
            { "skull:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("MapD3") ] },
                }
            },
            { "thieves:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("MapD4") ] },
                }
            },
            { "ice:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("MapD5") ] },
                }
            },
            { "mire:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("MapD6") ] },
                }
            },
            { "turtlerock:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("MapD7") ] },
                }
            },
            { "gt:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("MapA2") ] },
                }
            },
        };

        if (world.Config.RegionWildMaps)
        {
            return new ItemSet
            {
                { "*",
                    new WeightedSet
                    {
                        { 3, [.. maps.Values.SelectMany(dungeon => dungeon.Values.SelectMany(item => item))] }
                    }
                }
            };
        }

        if (world.Config.Accessibility == AccessibilityOption.Items)
        {
            foreach (var (_, parts) in maps)
            {
                if (parts.Remove(9010, out var items))
                    parts.Add(9999, items);
            }
        }

        return maps;
    }

    /// <summary>Get Compasses for world in proper placement groups.</summary>
    /// <param name="world">world to get items for</param>
    private ItemSet GetCompasses(World world)
    {
        var compasses = new ItemSet
        {
            { "eastern:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassP1") ] }
                }
            },
            { "desert:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassP2") ] }
                }
            },
            { "hera:"+world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassP3") ] }
                }
            },
            { "pod:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassD1") ] }
                }
            },
            { "swamp:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassD2") ] }
                }
            },
            { "skull:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassD3") ] }
                }
            },
            { "thieves:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassD4") ] }
                }
            },
            { "ice:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassD5") ] }
                }
            },
            { "mire:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassD6") ] }
                }
            },
            { "turtlerock:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassD7") ] }
                }
            },
            { "gt:" + world.Id, new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassA2") ] }
                }
            },
        };

        if (world.Config.RegionWildCompasses)
        {
            return new ItemSet
            {
                { "*",
                    new WeightedSet
                    {
                        { 3,  [.. compasses.Values.SelectMany(dungeon => dungeon.Values.SelectMany(item => item))] }
                    }
                }
            };
        }

        if (world.Config.Accessibility == AccessibilityOption.Items)
        {
            foreach (var (_, parts) in compasses)
            {
                if (parts.Remove(9010, out var items))
                    parts.Add(9999, items);
            }
        }

        return compasses;
    }

    private static readonly string[] _bottles = [
        "Bottle",
        "BottleWithRedPotion",
        "BottleWithGreenPotion",
        "BottleWithBluePotion",
        "BottleWithBee",
        "BottleWithGoldBee",
        "BottleWithFairy",
    ];
    /// <summary>Get Bottles for world in proper placement groups.</summary>
    /// <param name="world">world to get items for</param>
    private ItemSet GetBottles(World world)
    {
        return new ItemSet()
        {
            { "bottle:" + world.Id,
                new WeightedSet()
                {
                    { 0, [
                            world.GetItem("Fairy" + prng.GetRandomElement(_bottles)),
                            world.GetItem("Fairy" + prng.GetRandomElement(_bottles)),
                        ]
                    },
                }
            },
            { "*",
                new WeightedSet()
                {
                    { 3, [ world.GetItem(prng.GetRandomElement(_bottles)) ] },
                    { 9001, [
                            world.GetItem(prng.GetRandomElement(_bottles)),
                            world.GetItem(prng.GetRandomElement(_bottles)),
                            world.GetItem(prng.GetRandomElement(_bottles)),
                        ]
                    },
                }
            },
        };
    }

    /// <summary>Get Shop Items for world in proper placement groups.</summary>
    /// <param name="world">world to get items for</param>
    private ItemSet GetShopItems(World world)
    {
        if (world.Config.RegionShopSupply != ShopSupplyOption.Shuffled)
            return [];

        return new ItemSet()
        {
            { "*", new WeightedSet()
                {
                    // TODO verify these counts, they are definitely wrong
                    { 9999, [
                        .. Enumerable.Repeat(world.GetItem("RedPotion"), 6),
                        .. Enumerable.Repeat(world.GetItem("GreenPotion"), 1),
                        .. Enumerable.Repeat(world.GetItem("BluePotion"), 6),
                        .. Enumerable.Repeat(world.GetItem("Heart"), 10),
                        .. Enumerable.Repeat(world.GetItem("TenBombs"), 10),
                        .. Enumerable.Repeat(world.GetItem("BlueShield"), 2),
                        .. Enumerable.Repeat(world.GetItem("RedShield"), 1)
                        ]
                    },
                }
            },
        };
    }
}
