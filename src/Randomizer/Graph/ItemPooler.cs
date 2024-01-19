namespace Randomizer.Graph;

using ItemSet = Dictionary<ItemSetName, /* WeightedSet */ Dictionary<int, List<Item>>>;
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
                    { ItemSetName.DefaultSet, new WeightedSet
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
                    var junkFill = prng.Shuffle(worldSet[ItemSetName.DefaultSet][9999]).Take(fillCount).ToArray();
                    foreach (var key in junkFill)
                    {
                        worldSet[new ItemSetName("gt", world)].TryAdd(2, []);
                        worldSet[new ItemSetName("gt", world)][2].Add(key);
                        worldSet[ItemSetName.DefaultSet][9999].Remove(key);
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
            { new ItemSetName("mm-medallion", world), new WeightedSet
                {
                    { 0, [ world.GetItem(prng.GetRandomElement(_mireEntry)) ] },
                }
            },
            { new ItemSetName("tr-medallion", world), new WeightedSet
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
            { new ItemSetName("prize", world), new WeightedSet
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
            { new ItemSetName("escape", world), new WeightedSet
                {
                    { 1, [ world.GetItem("KeyH2") ] },
                }
            },
            { new ItemSetName("desert", world), new WeightedSet
                {
                    { 1, [ world.GetItem("KeyP2") ] },
                }
            },
            { new ItemSetName("hera", world), new WeightedSet
                {
                    { 1, [ world.GetItem("KeyP3") ] },
                }
            },
            { new ItemSetName("agahnim", world), new WeightedSet
                {
                    { 1, [.. Enumerable.Repeat(world.GetItem("KeyA1"), 2)] },
                }
            },
            { new ItemSetName("pod", world), new WeightedSet
                {
                    { 1, [.. Enumerable.Repeat(world.GetItem("KeyD1"), 6)] },
                }
            },
            { new ItemSetName("swamp", world), new WeightedSet
                {
                    { 1, [ world.GetItem("KeyD2") ] },
                }
            },
            { new ItemSetName("skull", world), new WeightedSet
                {
                    { 1, [.. Enumerable.Repeat(world.GetItem("KeyD3"), 3)] },
                }
            },
            { new ItemSetName("thieves", world), new WeightedSet
                {
                    { 1, [ world.GetItem("KeyD4") ] },
                }
            },
            { new ItemSetName("ice", world), new WeightedSet
                {
                    { 1, [.. Enumerable.Repeat(world.GetItem("KeyD5"), 2)] },
                }
            },
            { new ItemSetName("mire", world), new WeightedSet
                {
                    { 1, [.. Enumerable.Repeat(world.GetItem("KeyD6"), 3)] },
                }
            },
            { new ItemSetName("turtlerock", world), new WeightedSet
                {
                    { 1, [.. Enumerable.Repeat(world.GetItem("KeyD7"), 4)] },
                }
            },
            { new ItemSetName("gt", world), new WeightedSet
                {
                    { 1, [.. Enumerable.Repeat(world.GetItem("KeyA2"), 4)] },
                }
            },
        };

        if (world.Config.RegionWildKeys)
        {
            return new ItemSet
            {
                { ItemSetName.DefaultSet,
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
            { new ItemSetName("eastern", world), new WeightedSet
                {
                    { 1, [ world.GetItem("BigKeyP1") ] },
                }
            },
            { new ItemSetName("desert", world), new WeightedSet
                {
                    { 1, [ world.GetItem("BigKeyP2") ] },
                }
            },
            { new ItemSetName("hera", world), new WeightedSet
                {
                    { 1, [ world.GetItem("BigKeyP3") ] },
                }
            },
            { new ItemSetName("pod", world), new WeightedSet
                {
                    { 1, [ world.GetItem("BigKeyD1") ] },
                }
            },
            { new ItemSetName("swamp", world), new WeightedSet
                {
                    { 2, [ world.GetItem("BigKeyD2") ] },
                }
            },
            { new ItemSetName("skull", world), new WeightedSet
                {
                    { 2, [ world.GetItem("BigKeyD3") ] },
                }
            },
            { new ItemSetName("thieves", world), new WeightedSet
                {
                    { 1, [ world.GetItem("BigKeyD4") ] },
                }
            },
            { new ItemSetName("ice", world), new WeightedSet
                {
                    { 1, [ world.GetItem("BigKeyD5") ] },
                }
            },
            { new ItemSetName("mire", world), new WeightedSet
                {
                    { 1, [ world.GetItem("BigKeyD6") ] },
                }
            },
            { new ItemSetName("turtlerock", world), new WeightedSet
                {
                    { 1, [ world.GetItem("BigKeyD7") ] },
                }
            },
            { new ItemSetName("gt", world), new WeightedSet
                {
                    { 0, [ world.GetItem("BigKeyA2") ] },
                }
            },
        };

        if (world.Config.RegionWildBigKeys)
        {
            return new ItemSet
            {
                { ItemSetName.DefaultSet,
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
            { new ItemSetName("escape", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("MapH2") ] },
                }
            },
            { new ItemSetName("eastern", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("MapP1") ] },
                }
            },
            { new ItemSetName("desert", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("MapP2") ] },
                }
            },
            { new ItemSetName("hera", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("MapP3") ] },
                }
            },
            { new ItemSetName("pod", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("MapD1") ] },
                }
            },
            { new ItemSetName("swamp", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("MapD2") ] },
                }
            },
            { new ItemSetName("skull", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("MapD3") ] },
                }
            },
            { new ItemSetName("thieves", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("MapD4") ] },
                }
            },
            { new ItemSetName("ice", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("MapD5") ] },
                }
            },
            { new ItemSetName("mire", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("MapD6") ] },
                }
            },
            { new ItemSetName("turtlerock", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("MapD7") ] },
                }
            },
            { new ItemSetName("gt", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("MapA2") ] },
                }
            },
        };

        if (world.Config.RegionWildMaps)
        {
            return new ItemSet
            {
                { ItemSetName.DefaultSet,
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
            { new ItemSetName("eastern", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassP1") ] }
                }
            },
            { new ItemSetName("desert", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassP2") ] }
                }
            },
            { new ItemSetName("hera", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassP3") ] }
                }
            },
            { new ItemSetName("pod", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassD1") ] }
                }
            },
            { new ItemSetName("swamp", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassD2") ] }
                }
            },
            { new ItemSetName("skull", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassD3") ] }
                }
            },
            { new ItemSetName("thieves", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassD4") ] }
                }
            },
            { new ItemSetName("ice", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassD5") ] }
                }
            },
            { new ItemSetName("mire", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassD6") ] }
                }
            },
            { new ItemSetName("turtlerock", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassD7") ] }
                }
            },
            { new ItemSetName("gt", world), new WeightedSet
                {
                    { 9010, [ world.GetItem("CompassA2") ] }
                }
            },
        };

        if (world.Config.RegionWildCompasses)
        {
            return new ItemSet
            {
                { ItemSetName.DefaultSet,
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
            { new ItemSetName("bottle", world),
                new WeightedSet()
                {
                    { 0, [
                            world.GetItem("Fairy" + prng.GetRandomElement(_bottles)),
                            world.GetItem("Fairy" + prng.GetRandomElement(_bottles)),
                        ]
                    },
                }
            },
            { ItemSetName.DefaultSet,
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
            { ItemSetName.DefaultSet, new WeightedSet()
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
