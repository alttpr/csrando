namespace Randomizer.Graph;

using ItemSet = Dictionary<string, /* WeightedSet */ Dictionary<int, List<Item>>>;
using WeightedSet = Dictionary<int, List<Item>>;

/**
 * Get the sets of items to place.
 */
internal sealed class ItemPooler
{
    private readonly World[] _worlds;
    private readonly PRNG _prng;
    /**
     * Create new Item Pooler.
     *
     * @param World[] worlds worlds to get Item pools for
     */
    public ItemPooler(World[] worlds, PRNG prng)
    {
        _worlds = worlds;
        _prng = prng;
    }

    /**
     * Get list of all items in their weighted sets.
     */
    public ItemSet GetPool()
    {
        var sets = new ItemSet();

        foreach (var world in _worlds)
        {
            var world_set = array_merge_recursive(
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
                            { 3, new List<Item>
                                {
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
                                }
                            },
                            { 9001, new[]
                                {
                                    world.GetItem("Boomerang"),
                                    world.GetItem("RedBoomerang"),
                                    world.GetItem("HeartContainer"),
                                }
                                .Concat(Enumerable.Repeat(world.GetItem("ProgressiveSword"), 2))
                                .Concat(Enumerable.Repeat(world.GetItem("ProgressiveArmor"), 2))
                                .Concat(Enumerable.Repeat(world.GetItem("BossHeartContainer"), 10))
                                .Concat(Enumerable.Repeat(world.GetItem("PieceOfHeart"), 24))
                                .ToList()
                            },
                            // order here matters, items at end may get lopped off
                            // if too many items to place
                            { 9999, new[]
                                {
                                    world.GetItem("Arrow"),
                                    world.GetItem("OneHundredRupees"),
                                }
                                .Concat(Enumerable.Repeat(world.GetItem("TenArrows"), 12))
                                .Concat(Enumerable.Repeat(world.GetItem("ThreeBombs"), 17))
                                .Concat(Enumerable.Repeat(world.GetItem("OneRupee"), 2))
                                .Concat(Enumerable.Repeat(world.GetItem("FiveRupees"), 4))
                                .Concat(Enumerable.Repeat(world.GetItem("TwentyRupees"), 28))
                                .Concat(Enumerable.Repeat(world.GetItem("FiftyRupees"), 7))
                                .Concat(Enumerable.Repeat(world.GetItem("ThreeHundredRupees"), 5))
                                .ToList()
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
                float crystal_ratio = world.Config.CrystalsTower / 7f;
                int fill_count;
                if (world.Config.Goal is GoalOption.TriforceHunt or GoalOption.Pedestal)
                {
                    fill_count = _prng.GetRandomInt((int)(15 * crystal_ratio), (int)(25 * crystal_ratio));
                }
                else
                {
                    fill_count = _prng.GetRandomInt((int)(15 * crystal_ratio));
                }
                if (fill_count > 0)
                {
                    var junkFill = _prng.Shuffle(world_set["*"][9999]).Take(fill_count).ToArray();
                    foreach (var key in junkFill)
                    {
                        world_set["gt:" + world.Id].TryAdd(2, new());
                        world_set["gt:" + world.Id][2].Add(key);
                        world_set["*"][9999].Remove(key);
                    }
                }
            }

            sets = array_merge_recursive(
                sets,
                world_set
            );
        }

        return sets;
    }

    private static ItemSet array_merge_recursive(params ItemSet[] itemSets)
    {
        var result = new ItemSet();

        foreach (var itemSet in itemSets)
        {
            foreach (var (location, prioritySets) in itemSet)
            {
                if (!result.TryGetValue(location, out var locationSet))
                    result[location] = locationSet = new WeightedSet();
                foreach (var (priority, items) in prioritySets)
                {
                    if (!locationSet.TryGetValue(priority, out var prioritySet))
                        locationSet[priority] = prioritySet = new List<Item>();

                    prioritySet.AddRange(items);
                }
            }
        }

        return result;
    }

    /**
     * Get Medallions meta locations for what ends up being required for TR/MM
     * entry.
     *
     * @param World world world to get items for
     */
    private ItemSet GetMedallions(World world)
    {
        return new ItemSet
        {
            { "mm-medallion:" + world.Id, new WeightedSet
                {
                    { 0, new List<Item>
                        {
                            world.GetItem(_prng.GetRandomElement(new [] { "MireEntryBombos", "MireEntryEther", "MireEntryQuake" })),
                        }
                    },
                }
            },
            { "tr-medallion:" + world.Id, new WeightedSet
                {
                    { 0, new List<Item>
                        {
                            world.GetItem(_prng.GetRandomElement(new [] { "TurtleRockEntryBombos", "TurtleRockEntryEther", "TurtleRockEntryQuake" })),
                        }
                    },
                }
            },
        };
    }

    /**
     * Get Prizes for a world.
     *
     * @param World world world to get items for
     */
    private ItemSet GetPrizes(World world)
    {
        return new ItemSet
        {
            { "prize:" + world.Id, new WeightedSet
                {
                    { 0, new List<Item>
                         {
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
                         }
                    },
                }
            },
        };
    }

    /**
     * Get Small keys for world in proper placement groups.
     *
     * @param World world world to get items for
     */
    private ItemSet GetSmallKeys(World world)
    {
        var keys = new ItemSet
        {
            { "escape:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { world.GetItem("KeyH2") } },
                }
            },
            { "desert:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { world.GetItem("KeyP2") } },
                }
            },
            { "hera:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { world.GetItem("KeyP3") } },
                }
            },
            { "agahnim:" + world.Id, new WeightedSet
                {
                    { 1, Enumerable.Repeat(world.GetItem("KeyA1"), 2).ToList() },
                }
            },
            { "pod:" + world.Id, new WeightedSet
                {
                    { 1, Enumerable.Repeat(world.GetItem("KeyD1"), 6).ToList() },
                }
            },
            { "swamp:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { world.GetItem("KeyD2") } },
                }
            },
            { "skull:" + world.Id, new WeightedSet
                {
                    { 1, Enumerable.Repeat(world.GetItem("KeyD3"), 3).ToList() },
                }
            },
            { "thieves:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { world.GetItem("KeyD4") } },
                }
            },
            { "ice:" + world.Id, new WeightedSet
                {
                    { 1, Enumerable.Repeat(world.GetItem("KeyD5"), 2).ToList() },
                }
            },
            { "mire:" + world.Id, new WeightedSet
                {
                    { 1, Enumerable.Repeat(world.GetItem("KeyD6"), 3).ToList() },
                }
            },
            { "turtlerock:" + world.Id, new WeightedSet
                {
                    { 1, Enumerable.Repeat(world.GetItem("KeyD7"), 4).ToList() },
                }
            },
            { "gt:" + world.Id, new WeightedSet
                {
                    { 1, Enumerable.Repeat(world.GetItem("KeyA2"), 4).ToList() },
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
                        { 3, keys.SelectMany(dungeon => dungeon.Value.SelectMany(priority => priority.Value)).ToList() }
                    }
                }
            };
        }

        return keys;
    }

    /**
     * Get Big keys for world in proper placement groups.
     *
     * @param World world world to get items for
     */
    private ItemSet GetBigKeys(World world)
    {
        var big_keys = new ItemSet
        {
            { "eastern:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { world.GetItem("BigKeyP1") } },
                }
            },
            { "desert:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { world.GetItem("BigKeyP2") } },
                }
            },
            { "hera:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { world.GetItem("BigKeyP3") } },
                }
            },
            { "pod:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { world.GetItem("BigKeyD1") } },
                }
            },
            { "swamp:" + world.Id, new WeightedSet
                {
                    { 2, new List<Item> { world.GetItem("BigKeyD2") } },
                }
            },
            { "skull:" + world.Id, new WeightedSet
                {
                    { 2, new List<Item> { world.GetItem("BigKeyD3") } },
                }
            },
            { "thieves:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { world.GetItem("BigKeyD4") } },
                }
            },
            { "ice:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { world.GetItem("BigKeyD5") } },
                }
            },
            { "mire:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { world.GetItem("BigKeyD6") } },
                }
            },
            { "turtlerock:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { world.GetItem("BigKeyD7") } },
                }
            },
            { "gt:" + world.Id, new WeightedSet
                {
                    { 0, new List<Item> { world.GetItem("BigKeyA2") } },
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
                        { 3, big_keys.SelectMany(dungeon => dungeon.Value.SelectMany(priority => priority.Value)).ToList() }
                    }
                }
            };
        }

        return big_keys;
    }

    /**
     * Get Maps for world in proper placement groups.
     *
     * @param World world world to get items for
     */
    private ItemSet GetMaps(World world)
    {
        var maps = new ItemSet
        {
            { "escape:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("MapH2") } },
                }
            },
            { "eastern:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("MapP1") } },
                }
            },
            { "desert:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("MapP2") } },
                }
            },
            { "hera:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("MapP3") } },
                }
            },
            { "pod:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("MapD1") } },
                }
            },
            { "swamp:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("MapD2") } },
                }
            },
            { "skull:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("MapD3") } },
                }
            },
            { "thieves:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("MapD4") } },
                }
            },
            { "ice:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("MapD5") } },
                }
            },
            { "mire:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("MapD6") } },
                }
            },
            { "turtlerock:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("MapD7") } },
                }
            },
            { "gt:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("MapA2") } },
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
                        { 3, maps.SelectMany(dungeon => dungeon.Value.SelectMany(priority => priority.Value)).ToList() }
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

    /**
     * Get Compasses for world in proper placement groups.
     *
     * @param World world world to get items for
     */
    private ItemSet GetCompasses(World world)
    {
        var compasses = new ItemSet
        {
            { "eastern:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("CompassP1") } }
                }
            },
            { "desert:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("CompassP2") } }
                }
            },
            { "hera:"+world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("CompassP3") } }
                }
            },
            { "pod:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("CompassD1") } }
                }
            },
            { "swamp:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("CompassD2") } }
                }
            },
            { "skull:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("CompassD3") } }
                }
            },
            { "thieves:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("CompassD4") } }
                }
            },
            { "ice:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("CompassD5") } }
                }
            },
            { "mire:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("CompassD6") } }
                }
            },
            { "turtlerock:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("CompassD7") } }
                }
            },
            { "gt:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { world.GetItem("CompassA2") } }
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
                        { 3, compasses.SelectMany(dungeon => dungeon.Value.SelectMany(priority => priority.Value)).ToList() }
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

    /**
     * Get Bottles for world in proper placement groups.
     *
     * @param World world world to get items for
     */
    private ItemSet GetBottles(World world)
    {
        string[] bottles = {
            "Bottle",
            "BottleWithRedPotion",
            "BottleWithGreenPotion",
            "BottleWithBluePotion",
            "BottleWithBee",
            "BottleWithGoldBee",
            "BottleWithFairy",
        };

        return new ItemSet()
        {
            { "bottle:" + world.Id,
                new WeightedSet()
                {
                    { 0, new List<Item>
                         {
                             world.GetItem("Fairy" + bottles[_prng.GetRandomInt(bottles.Length)]),
                             world.GetItem("Fairy" + bottles[_prng.GetRandomInt(bottles.Length)]),
                         }
                    },
                }
            },
            { "*",
                new WeightedSet()
                {
                    { 3, new List<Item> { world.GetItem(bottles[_prng.GetRandomInt(bottles.Length)]) } },
                    { 9001, new List<Item>
                            {
                                world.GetItem(bottles[_prng.GetRandomInt(bottles.Length)]),
                                world.GetItem(bottles[_prng.GetRandomInt(bottles.Length)]),
                                world.GetItem(bottles[_prng.GetRandomInt(bottles.Length)]),
                            }
                    },
                }
            },
        };
    }

    /**
     * Get Shop Items for world in proper placement groups.
     *
     * @todo verify these counts, they are definitely wrong
     *
     * @param World world world to get items for
     */
    private ItemSet GetShopItems(World world)
    {
        if (world.Config.RegionShopSupply != ShopSupplyOption.Shuffled)
        {
            return new();
        }

        return new ItemSet()
        {
            { "*", new WeightedSet()
                {
                    { 9999, Enumerable.Repeat(world.GetItem("RedPotion"), 6)
                        .Concat(Enumerable.Repeat(world.GetItem("GreenPotion"), 1))
                        .Concat(Enumerable.Repeat(world.GetItem("BluePotion"), 6))
                        .Concat(Enumerable.Repeat(world.GetItem("Heart"), 10))
                        .Concat(Enumerable.Repeat(world.GetItem("TenBombs"), 10))
                        .Concat(Enumerable.Repeat(world.GetItem("BlueShield"), 2))
                        .Concat(Enumerable.Repeat(world.GetItem("RedShield"), 1))
                        .ToList()
                    },
                }
            },
        };
    }
}
