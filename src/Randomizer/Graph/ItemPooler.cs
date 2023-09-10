namespace AlttpRandomizer.Graph;

using ItemSet = Dictionary<string, /* WeightedSet */ Dictionary<int, List<Item>>>;
using WeightedSet = Dictionary<int, List<Item>>;

/**
 * Get the sets of items to place.
 */
internal sealed class ItemPooler
{
    private readonly World[] _worlds;
    /**
     * Create new Item Pooler.
     * 
     * @param World[] worlds worlds to get Item pools for
     */
    public ItemPooler(World[] worlds)
    {
        _worlds = worlds;
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
                                    Item.Get("Hammer", world.Id),
                                    Item.Get("Hookshot", world.Id),
                                    Item.Get("Flippers", world.Id),
                                    Item.Get("FireRod", world.Id),
                                    Item.Get("IceRod", world.Id),
                                    Item.Get("ProgressiveBow", world.Id),
                                    Item.Get("ProgressiveBow", world.Id),
                                    Item.Get("ProgressiveSword", world.Id),
                                    Item.Get("ProgressiveSword", world.Id),
                                    Item.Get("ProgressiveShield", world.Id),
                                    Item.Get("ProgressiveShield", world.Id),
                                    Item.Get("ProgressiveShield", world.Id),
                                    Item.Get("PegasusBoots", world.Id),
                                    Item.Get("BookOfMudora", world.Id),
                                    Item.Get("ProgressiveGlove", world.Id),
                                    Item.Get("ProgressiveGlove", world.Id),
                                    Item.Get("CaneOfSomaria", world.Id),
                                    Item.Get("CaneOfByrna", world.Id),
                                    Item.Get("Cape", world.Id),
                                    Item.Get("Lamp", world.Id),
                                    Item.Get("Bombos", world.Id),
                                    Item.Get("Ether", world.Id),
                                    Item.Get("Quake", world.Id),
                                    Item.Get("Mushroom", world.Id),
                                    Item.Get("MoonPearl", world.Id),
                                    Item.Get("MagicMirror", world.Id),
                                    Item.Get("OcarinaInactive", world.Id),
                                    Item.Get("Shovel", world.Id),
                                    Item.Get("BugCatchingNet", world.Id),
                                    Item.Get("Powder", world.Id),
                                    Item.Get("HalfMagic", world.Id),
                                }
                            },
                            { 9001, new[]
                                {
                                    Item.Get("Boomerang", world.Id),
                                    Item.Get("RedBoomerang", world.Id),
                                    Item.Get("HeartContainer", world.Id),
                                }
                                .Concat(Enumerable.Repeat(Item.Get("ProgressiveSword", world.Id), 2))
                                .Concat(Enumerable.Repeat(Item.Get("ProgressiveArmor", world.Id), 2))
                                .Concat(Enumerable.Repeat(Item.Get("BossHeartContainer", world.Id), 10))
                                .Concat(Enumerable.Repeat(Item.Get("PieceOfHeart", world.Id), 24))
                                .ToList()
                            },
                            // order here matters, items at end may get lopped off
                            // if too many items to place
                            { 9999, new[]
                                {
                                    Item.Get("Arrow", world.Id),
                                    Item.Get("OneHundredRupees", world.Id),
                                }
                                .Concat(Enumerable.Repeat(Item.Get("TenArrows", world.Id), 12))
                                .Concat(Enumerable.Repeat(Item.Get("ThreeBombs", world.Id), 17))
                                .Concat(Enumerable.Repeat(Item.Get("OneRupee", world.Id), 2))
                                .Concat(Enumerable.Repeat(Item.Get("FiveRupees", world.Id), 4))
                                .Concat(Enumerable.Repeat(Item.Get("TwentyRupees", world.Id), 28))
                                .Concat(Enumerable.Repeat(Item.Get("FiftyRupees", world.Id), 7))
                                .Concat(Enumerable.Repeat(Item.Get("ThreeHundredRupees", world.Id), 5))
                                .ToList()
                            },
                        }
                    },
                }
            );

            if (
                world.Config<string>("logic") != "None"
                && (world.Config<string>("mode.state") == "inverted"
                    || !(world.Config<string>("logic") is "OverworldGlitches" or "MajorGlitches"))
            )
            {
                float crystal_ratio = world.Config("crystals.tower", 7) / 7f;
                int fill_count;
                if (world.Config<string>("goal") is "triforce-hunt" or "pedestal")
                {
                    fill_count = PHP.get_random_int((int)(15 * crystal_ratio), (int)(25 * crystal_ratio));
                }
                else
                {
                    fill_count = PHP.get_random_int((int)(15 * crystal_ratio));
                }
                if (fill_count > 0)
                {
                    var junkFill = world_set["*"][9999].Shuffle().Take(fill_count).ToArray();
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
                            Item.Get(new [] { "MireEntryBombos", "MireEntryEther", "MireEntryQuake" }[PHP.get_random_int(2)], world.Id),
                        }
                    },
                }
            },
            { "tr-medallion:" + world.Id, new WeightedSet
                {
                    { 0, new List<Item>
                        {
                            Item.Get(new [] { "TurtleRockEntryBombos", "TurtleRockEntryEther", "TurtleRockEntryQuake" }[PHP.get_random_int(2)], world.Id),
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
                             Item.Get("PendantOfCourage", world.Id),
                             Item.Get("PendantOfWisdom", world.Id),
                             Item.Get("PendantOfPower", world.Id),
                             Item.Get("Crystal1", world.Id),
                             Item.Get("Crystal2", world.Id),
                             Item.Get("Crystal3", world.Id),
                             Item.Get("Crystal4", world.Id),
                             Item.Get("Crystal5", world.Id),
                             Item.Get("Crystal6", world.Id),
                             Item.Get("Crystal7", world.Id),
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
                    { 1, new List<Item> { Item.Get("KeyH2", world.Id) } },
                }
            },
            { "desert:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { Item.Get("KeyP2", world.Id) } },
                }
            },
            { "hera:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { Item.Get("KeyP3", world.Id) } },
                }
            },
            { "agahnim:" + world.Id, new WeightedSet
                {
                    { 1, Enumerable.Repeat(Item.Get("KeyA1", world.Id), 2).ToList() },
                }
            },
            { "pod:" + world.Id, new WeightedSet
                {
                    { 1, Enumerable.Repeat(Item.Get("KeyD1", world.Id), 6).ToList() },
                }
            },
            { "swamp:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { Item.Get("KeyD2", world.Id) } },
                }
            },
            { "skull:" + world.Id, new WeightedSet
                {
                    { 1, Enumerable.Repeat(Item.Get("KeyD3", world.Id), 3).ToList() },
                }
            },
            { "thieves:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { Item.Get("KeyD4", world.Id) } },
                }
            },
            { "ice:" + world.Id, new WeightedSet
                {
                    { 1, Enumerable.Repeat(Item.Get("KeyD5", world.Id), 2).ToList() },
                }
            },
            { "mire:" + world.Id, new WeightedSet
                {
                    { 1, Enumerable.Repeat(Item.Get("KeyD6", world.Id), 3).ToList() },
                }
            },
            { "turtlerock:" + world.Id, new WeightedSet
                {
                    { 1, Enumerable.Repeat(Item.Get("KeyD7", world.Id), 4).ToList() },
                }
            },
            { "gt:" + world.Id, new WeightedSet
                {
                    { 1, Enumerable.Repeat(Item.Get("KeyA2", world.Id), 4).ToList() },
                }
            },
        };

        if (world.Config("region.wildKeys", false))
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
                    { 1, new List<Item> { Item.Get("BigKeyP1", world.Id) } },
                }
            },
            { "desert:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { Item.Get("BigKeyP2", world.Id) } },
                }
            },
            { "hera:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { Item.Get("BigKeyP3", world.Id) } },
                }
            },
            { "pod:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { Item.Get("BigKeyD1", world.Id) } },
                }
            },
            { "swamp:" + world.Id, new WeightedSet
                {
                    { 2, new List<Item> { Item.Get("BigKeyD2", world.Id) } },
                }
            },
            { "skull:" + world.Id, new WeightedSet
                {
                    { 2, new List<Item> { Item.Get("BigKeyD3", world.Id) } },
                }
            },
            { "thieves:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { Item.Get("BigKeyD4", world.Id) } },
                }
            },
            { "ice:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { Item.Get("BigKeyD5", world.Id) } },
                }
            },
            { "mire:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { Item.Get("BigKeyD6", world.Id) } },
                }
            },
            { "turtlerock:" + world.Id, new WeightedSet
                {
                    { 1, new List<Item> { Item.Get("BigKeyD7", world.Id) } },
                }
            },
            { "gt:" + world.Id, new WeightedSet
                {
                    { 0, new List<Item> { Item.Get("BigKeyA2", world.Id) } },
                }
            },
        };

        if (world.Config("region.wildBigKeys", false))
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
                    { 9010, new List<Item> { Item.Get("MapH2", world.Id) } },
                }
            },
            { "eastern:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("MapP1", world.Id) } },
                }
            },
            { "desert:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("MapP2", world.Id) } },
                }
            },
            { "hera:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("MapP3", world.Id) } },
                }
            },
            { "pod:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("MapD1", world.Id) } },
                }
            },
            { "swamp:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("MapD2", world.Id) } },
                }
            },
            { "skull:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("MapD3", world.Id) } },
                }
            },
            { "thieves:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("MapD4", world.Id) } },
                }
            },
            { "ice:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("MapD5", world.Id) } },
                }
            },
            { "mire:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("MapD6", world.Id) } },
                }
            },
            { "turtlerock:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("MapD7", world.Id) } },
                }
            },
            { "gt:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("MapA2", world.Id) } },
                }
            },
        };

        if (world.Config("region.wildMaps", false))
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

        if (world.Config<string>("accessibility") == "items")
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
                    { 9010, new List<Item> { Item.Get("CompassP1", world.Id) } }
                }
            },
            { "desert:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("CompassP2", world.Id) } }
                }
            },
            { "hera:"+world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("CompassP3", world.Id) } }
                }
            },
            { "pod:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("CompassD1", world.Id) } }
                }
            },
            { "swamp:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("CompassD2", world.Id) } }
                }
            },
            { "skull:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("CompassD3", world.Id) } }
                }
            },
            { "thieves:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("CompassD4", world.Id) } }
                }
            },
            { "ice:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("CompassD5", world.Id) } }
                }
            },
            { "mire:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("CompassD6", world.Id) } }
                }
            },
            { "turtlerock:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("CompassD7", world.Id) } }
                }
            },
            { "gt:" + world.Id, new WeightedSet
                {
                    { 9010, new List<Item> { Item.Get("CompassA2", world.Id) } }
                }
            },
        };

        if (world.Config("region.wildCompasses", false))
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

        if (world.Config<string>("accessibility") == "items")
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
                             Item.Get("Fairy" + bottles[PHP.get_random_int(bottles.Length)], world.Id),
                             Item.Get("Fairy" + bottles[PHP.get_random_int(bottles.Length)], world.Id),
                         }
                    },
                }
            },
            { "*",
                new WeightedSet()
                {
                    { 3, new List<Item> { Item.Get(bottles[PHP.get_random_int(bottles.Length)], world.Id) } },
                    { 9001, new List<Item>
                            {
                                Item.Get(bottles[PHP.get_random_int(bottles.Length)], world.Id),
                                Item.Get(bottles[PHP.get_random_int(bottles.Length)], world.Id),
                                Item.Get(bottles[PHP.get_random_int(bottles.Length)], world.Id),
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
        if (world.Config<string>("region.shopSupply") != "shuffled")
        {
            return new();
        }

        return new ItemSet()
        {
            { "*", new WeightedSet()
                {
                    { 9999, Enumerable.Repeat(Item.Get("RedPotion", world.Id), 6)
                        .Concat(Enumerable.Repeat(Item.Get("GreenPotion", world.Id), 1))
                        .Concat(Enumerable.Repeat(Item.Get("BluePotion", world.Id), 6))
                        .Concat(Enumerable.Repeat(Item.Get("Heart", world.Id), 10))
                        .Concat(Enumerable.Repeat(Item.Get("TenBombs", world.Id), 10))
                        .Concat(Enumerable.Repeat(Item.Get("BlueShield", world.Id), 2))
                        .Concat(Enumerable.Repeat(Item.Get("RedShield", world.Id), 1))
                        .ToList()
                    },
                }
            },
        };
    }
}
