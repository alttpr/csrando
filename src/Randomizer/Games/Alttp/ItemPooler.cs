using Randomizer.Graph;

namespace Randomizer.Games.Alttp;

/// <summary>Get the sets of items to place.</summary>
/// <param name="worlds">worlds to get Item pools for</param>
internal sealed class ItemPooler : IItemPooler
{
    // these are item locations that will ALWAYS receive items, regardless of randomizer options.
    // TODO: there's probably a few types in here that shouldn't be, like Event?
    private static readonly HashSet<VertexType> ITEM_LOCATIONS =
    [
        VertexType.BigChest,
        VertexType.Bonk,
        VertexType.Chest,
        VertexType.Drop,
        VertexType.Dig,
        VertexType.Event,
        VertexType.Medallion,
        VertexType.Npc,
        VertexType.Pedestal,
        VertexType.Prize,
        VertexType.Refill,
        VertexType.Standing,
    ];

    private readonly PRNG _prng;
    private readonly Dictionary<IWorld, HashSet<VertexType>> _itemLocationTypes;

    public ItemPooler(IWorld[] worlds, PRNG prng)
    {
        var z3Worlds = worlds.OfType<World>().ToArray();
        _prng = prng;
        _itemLocationTypes = z3Worlds.ToDictionary(k => (IWorld)k, GetLocationTypesForWorld);
        Pool = [.. z3Worlds.SelectMany(GetPoolForWorld)];
        SetLocations = BuildLocations(worlds);
    }

    private static HashSet<VertexType> GetLocationTypesForWorld(World world)
    {
        var locationTypes = new HashSet<VertexType>(ITEM_LOCATIONS);
        // TODO: there's a certain value in randomizing (or not randomizing) the potion shop.
        //       if we make this an option, this would need to be smart enough to add/remove it.
        if (world.Config.RegionShopSupply != ShopSupplyOption.Normal)
            locationTypes.Add(VertexType.ShopItem);
        return locationTypes;
    }

    private SetLocations BuildLocations(IWorld[] worlds)
    {
        var setLocations = new SetLocations();
        foreach (var vertex in worlds.SelectMany(world => world.GetLocations()).OfType<Vertex>())
        {
            var itemType = vertex.SubType ?? vertex.Type;
            if (_itemLocationTypes[vertex.World].Contains(itemType))
            {
                // FIXME: shops have item in data, but we don't use them yet.
                //        leaving them in means we don't place another item.
                //        blindly deleting everything (not limited to shop items) breaks randomization because it deletes placed keys.
                //        it might be time to rework how the pooler determines locations where items can go
                //        (and maybe even place all vanilla items, then let it pick them up for the pooled items).
                if (vertex.Type == VertexType.ShopItem)
                    vertex.Item = null;
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
            .. GetMedallions(world),
            .. GetPrizes(world),
            .. GetSmallKeys(world),
            .. GetBigKeys(world),
            .. GetMaps(world),
            .. GetCompasses(world),
            .. GetBottles(world),
            .. GetShopItems(world),
            // placing behind keys for now.
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Hammer")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Hookshot")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Flippers")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("FireRod")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("IceRod")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("ProgressiveBow")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("ProgressiveBow")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("ProgressiveSword")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("ProgressiveSword")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("ProgressiveShield")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("ProgressiveShield")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("ProgressiveShield")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("PegasusBoots")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("BookOfMudora")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("ProgressiveGlove")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("ProgressiveGlove")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("CaneOfSomaria")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("CaneOfByrna")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Cape")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Lamp")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Bombos")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Ether")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Quake")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Mushroom")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("MoonPearl")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("MagicMirror")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("OcarinaInactive")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Shovel")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("BugCatchingNet")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Powder")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("HalfMagic")),

            new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("Boomerang")),
            new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("RedBoomerang")),
            new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("HeartContainer")),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("ProgressiveSword")), 2),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("ProgressiveArmor")), 2),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("BossHeartContainer")), 10),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("PieceOfHeart")), 24),
            // order here matters, items at end may get lopped off
            // if too many items to place
            new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("Arrow")),
            new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("OneHundredRupees")),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("TenArrows")), 12),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("ThreeBombs")), 17),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("OneRupee")), 2),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("FiveRupees")), 4),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("TwentyRupees")), 28),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("FiftyRupees")), 7),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("ThreeHundredRupees")), 5),
        ];

        switch (world.Config.Weapon)
        {
            case WeaponOption.Assured:
                var assuredSword = worldSet.First(p => p.Item.Name == "ProgressiveSword");
                worldSet.Remove(assuredSword);
                worldSet.Add((ItemSetName.DefaultSet, 9999, world.GetItem("FiftyRupees")));
                world.Config.StartingEquipment.Add("ProgressiveSword");
                break;
            case WeaponOption.Vanilla:
                var uncleSword = worldSet.First(p => p.Item.Name == "ProgressiveSword");
                worldSet.Remove(uncleSword);
                world.GetLocation("Link's Uncle").Item = world.GetItem("UncleSword");
                world.PlacedItemCount++;

                var masterSword = worldSet.First(p => p.Item.Name == "ProgressiveSword");
                worldSet.Remove(masterSword);
                if (world.GetLocation("Master Sword Pedestal") is { Item.Name: not "Triforce" } pedestal)
                {
                    pedestal.Item = masterSword.Item;
                    world.PlacedItemCount++;
                }
                else
                {
                    worldSet.Add((ItemSetName.DefaultSet, 9999, world.GetItem("TwentyRupees")));
                }

                var baconSword = worldSet.First(p => p.Item.Name == "ProgressiveSword");
                worldSet.Remove(baconSword);
                world.GetLocation("Blacksmith Item").Item = baconSword.Item;
                world.PlacedItemCount++;

                var goldSword = worldSet.First(p => p.Item.Name == "ProgressiveSword");
                worldSet.Remove(goldSword);
                world.GetLocation("Pyramid Fairy - Left").Item = goldSword.Item;
                world.PlacedItemCount++;
                break;
            case WeaponOption.Swordless:
                var swordLess = worldSet.Where(p => p.Item.Name == "ProgressiveSword").ToArray();
                worldSet.AddRange(swordLess.Select(s => (s.Set, s.Weight, (IItem)world.GetItem("TwentyRupees2"))));
                worldSet.RemoveAll(swordLess.Contains);
                // TODO: v31 forces SilverArrowUpgrade in here when there are no silvers in the pool.
                break;
        }

        // TODO: does the config option region.requireBetterSword mean anything in v32?
        // TODO: remove swords and place them in take-any caves? (region.takeAnys)

        if (world.Config.Goal is GoalOption.TriforceHunt or GoalOption.Trifecta)
        {
            ushort triforcePiecesToPlace = Math.Max(world.Config.TriforcePieces, world.Config.GoalRequiredCount);
            worldSet.AddRange(Enumerable.Repeat((ItemSetName.DefaultSet, 3, (IItem)world.GetItem("TriforcePiece")), triforcePiecesToPlace));
        }

        if (
            world.Config.Glitches != GlitchesOption.None
            && (world.Config.State == StateOption.Inverted
                || !(world.Config.Glitches is GlitchesOption.Overworld or GlitchesOption.Major))
        )
        {
            float crystalRatio = int.Parse(world.Config.CrystalsTower) / 7f;
            int fillCount = world.Config.Goal is GoalOption.TriforceHunt or GoalOption.Pedestal or GoalOption.Trifecta
                ? _prng.GetRandomInt((int)(15 * crystalRatio), (int)(25 * crystalRatio))
                : _prng.GetRandomInt((int)(15 * crystalRatio));
            if (fillCount > 0)
            {
                var junkItems = worldSet.Where(p => p.Weight == 9999).ToArray();
                for (int i = 0; i < fillCount; i++)
                {
                    var junkItem = _prng.GetRandomElement(junkItems);
                    worldSet.Remove(junkItem);
                    worldSet.Add(new(new ItemSetName("gt", world), 2, junkItem.Item));
                }
            }
        }

        return worldSet;
    }

    private static readonly string[] _mireEntry = ["MireEntryBombos", "MireEntryEther", "MireEntryQuake"];
    private static readonly string[] _trEntry = ["TurtleRockEntryBombos", "TurtleRockEntryEther", "TurtleRockEntryQuake"];
    /// <summary>
    /// Get Medallions meta locations for what ends up being required for TR/MM
    /// entry.
    /// </summary>
    /// <param name="world">world to get items for</param>
    private PooledItem[] GetMedallions(IWorld world)
    {
        return
        [
            new PooledItem(new ItemSetName("mm-medallion", world), 0, world.GetItem(_prng.GetRandomElement(_mireEntry))),
            new PooledItem(new ItemSetName("tr-medallion", world), 0, world.GetItem(_prng.GetRandomElement(_trEntry))),
        ];
    }

    /// <summary>Get Prizes for a world.</summary>
    /// <param name="world">world to get items for</param>
    private PooledItem[] GetPrizes(IWorld world)
    {
        return
        [
            new PooledItem(new ItemSetName("prize", world), 0, world.GetItem("PendantOfCourage")),
            new PooledItem(new ItemSetName("prize", world), 0, world.GetItem("PendantOfWisdom")),
            new PooledItem(new ItemSetName("prize", world), 0, world.GetItem("PendantOfPower")),
            new PooledItem(new ItemSetName("prize", world), 0, world.GetItem("Crystal1")),
            new PooledItem(new ItemSetName("prize", world), 0, world.GetItem("Crystal2")),
            new PooledItem(new ItemSetName("prize", world), 0, world.GetItem("Crystal3")),
            new PooledItem(new ItemSetName("prize", world), 0, world.GetItem("Crystal4")),
            new PooledItem(new ItemSetName("prize", world), 0, world.GetItem("Crystal5")),
            new PooledItem(new ItemSetName("prize", world), 0, world.GetItem("Crystal6")),
            new PooledItem(new ItemSetName("prize", world), 0, world.GetItem("Crystal7")),
        ];
    }

    /// <summary>Get Small keys for world in proper placement groups.</summary>
    /// <param name="world">world to get items for</param>
    private PooledItem[] GetSmallKeys(World world)
    {
        PooledItem[] keys =
        [
            new PooledItem(new ItemSetName("escape", world), 1, world.GetItem("KeyH2")),
            new PooledItem(new ItemSetName("desert", world), 1, world.GetItem("KeyP2")),
            new PooledItem(new ItemSetName("hera", world), 1, world.GetItem("KeyP3")),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("agahnim", world), 1, world.GetItem("KeyA1")), 2),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("pod", world), 1, world.GetItem("KeyD1")), 6),
            new PooledItem(new ItemSetName("swamp", world), 1, world.GetItem("KeyD2")),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("skull", world), 1, world.GetItem("KeyD3")), 3),
            new PooledItem(new ItemSetName("thieves", world), 1, world.GetItem("KeyD4")),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("ice", world), 1, world.GetItem("KeyD5")), 2),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("mire", world), 1, world.GetItem("KeyD6")), 3),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("turtlerock", world), 1, world.GetItem("KeyD7")), 4),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("gt", world), 1, world.GetItem("KeyA2")), 4),
        ];

        if (world.Config.RegionWildKeys)
            keys = [.. keys.Select(key => new PooledItem(ItemSetName.DefaultSet, 3, key.Item))];

        return keys;
    }

    /// <summary>Get Big keys for world in proper placement groups.</summary>
    /// <param name="world">world to get items for</param>
    private PooledItem[] GetBigKeys(World world)
    {
        PooledItem[] bigKeys =
        [
            new PooledItem(new ItemSetName("eastern", world), 1, world.GetItem("BigKeyP1")),
            new PooledItem(new ItemSetName("desert", world), 1, world.GetItem("BigKeyP2")),
            new PooledItem(new ItemSetName("hera", world), 1, world.GetItem("BigKeyP3")),
            new PooledItem(new ItemSetName("pod", world), 1, world.GetItem("BigKeyD1")),
            new PooledItem(new ItemSetName("swamp", world), 2, world.GetItem("BigKeyD2")),
            new PooledItem(new ItemSetName("skull", world), 2, world.GetItem("BigKeyD3")),
            new PooledItem(new ItemSetName("thieves", world), 1, world.GetItem("BigKeyD4")),
            new PooledItem(new ItemSetName("ice", world), 1, world.GetItem("BigKeyD5")),
            new PooledItem(new ItemSetName("mire", world), 1, world.GetItem("BigKeyD6")),
            new PooledItem(new ItemSetName("turtlerock", world), 1, world.GetItem("BigKeyD7")),
            new PooledItem(new ItemSetName("gt", world), 0, world.GetItem("BigKeyA2")),
        ];

        if (world.Config.RegionWildBigKeys)
            bigKeys = [.. bigKeys.Select(bigKey => new PooledItem(ItemSetName.DefaultSet, 3, bigKey.Item))];

        return bigKeys;
    }

    /// <summary>Get Maps for world in proper placement groups.</summary>
    /// <param name="world">world to get items for</param>
    private PooledItem[] GetMaps(World world)
    {
        int priority = world.Config.Accessibility == AccessibilityOption.Items ? 9999 : 9010;
        PooledItem[] maps =
        [
            new PooledItem(new ItemSetName("escape", world), priority, world.GetItem("MapH2")),
            new PooledItem(new ItemSetName("eastern", world), priority, world.GetItem("MapP1")),
            new PooledItem(new ItemSetName("desert", world), priority, world.GetItem("MapP2")),
            new PooledItem(new ItemSetName("hera", world), priority, world.GetItem("MapP3")),
            new PooledItem(new ItemSetName("pod", world), priority, world.GetItem("MapD1")),
            new PooledItem(new ItemSetName("swamp", world), priority, world.GetItem("MapD2")),
            new PooledItem(new ItemSetName("skull", world), priority, world.GetItem("MapD3")),
            new PooledItem(new ItemSetName("thieves", world), priority, world.GetItem("MapD4")),
            new PooledItem(new ItemSetName("ice", world), priority, world.GetItem("MapD5")),
            new PooledItem(new ItemSetName("mire", world), priority, world.GetItem("MapD6")),
            new PooledItem(new ItemSetName("turtlerock", world), priority, world.GetItem("MapD7")),
            new PooledItem(new ItemSetName("gt", world), priority, world.GetItem("MapA2")),
        ];

        if (world.Config.RegionWildMaps)
            maps = [.. maps.Select(map => new PooledItem(ItemSetName.DefaultSet, 3, map.Item))];

        return maps;
    }

    /// <summary>Get Compasses for world in proper placement groups.</summary>
    /// <param name="world">world to get items for</param>
    private PooledItem[] GetCompasses(World world)
    {
        int priority = world.Config.Accessibility == AccessibilityOption.Items ? 9999 : 9010;
        PooledItem[] compasses =
        [
            new PooledItem(new ItemSetName("eastern", world), priority, world.GetItem("CompassP1")),
            new PooledItem(new ItemSetName("desert", world), priority, world.GetItem("CompassP2")),
            new PooledItem(new ItemSetName("hera", world), priority, world.GetItem("CompassP3")),
            new PooledItem(new ItemSetName("pod", world), priority, world.GetItem("CompassD1")),
            new PooledItem(new ItemSetName("swamp", world), priority, world.GetItem("CompassD2")),
            new PooledItem(new ItemSetName("skull", world), priority, world.GetItem("CompassD3")),
            new PooledItem(new ItemSetName("thieves", world), priority, world.GetItem("CompassD4")),
            new PooledItem(new ItemSetName("ice", world), priority, world.GetItem("CompassD5")),
            new PooledItem(new ItemSetName("mire", world), priority, world.GetItem("CompassD6")),
            new PooledItem(new ItemSetName("turtlerock", world), priority, world.GetItem("CompassD7")),
            new PooledItem(new ItemSetName("gt", world), priority, world.GetItem("CompassA2")),
        ];

        if (world.Config.RegionWildCompasses)
            compasses = [.. compasses.Select(compass => new PooledItem(ItemSetName.DefaultSet, 3, compass.Item))];

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
    private PooledItem[] GetBottles(World world)
    {
        return
        [
            new PooledItem(new ItemSetName("bottle", world), 0, world.GetItem("Fairy" + _prng.GetRandomElement(_bottles))),
            new PooledItem(new ItemSetName("bottle", world), 0, world.GetItem("Fairy" + _prng.GetRandomElement(_bottles))),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem(_prng.GetRandomElement(_bottles))),
            new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem(_prng.GetRandomElement(_bottles))),
            new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem(_prng.GetRandomElement(_bottles))),
            new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem(_prng.GetRandomElement(_bottles))),
        ];
    }

    /// <summary>Get Shop Items for world in proper placement groups.</summary>
    /// <param name="world">world to get items for</param>
    private PooledItem[] GetShopItems(World world)
    {
        if (world.Config.RegionShopSupply != ShopSupplyOption.Shuffled)
            return [];

        return
        [
            // we include the potion shop in the shuffle, one of each potion
            // TODO: we might choose to not include the potion shop. remove these if that's the case.
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("BluePotion")), 1),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("GreenPotion")), 1),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("RedPotion")), 1),
            // almost all shops carry the red potion and 10 bombs
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("RedPotion")), 8),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("TenBombs")), 8),
            // half the shops have a recovery heart, while the other half has a blue shield
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("Heart")), 4),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("BlueShield")), 4),
            // special case: the Orchard shop has a red shield for sale
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("TenArrows")), 1),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("Bee")), 1),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("RedShield")), 1),
            // upgrade shop has 7 each
            // TODO: adding 14 total means 12 extra items in the shuffle.
            //       the fast fill at the end will then discard 12 random items.
            //       do we just add 1 each? or do we let the garbage fill do its job leaving 12 unobtainable garbage items?
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("ArrowUpgrade5")), 7),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9999, world.GetItem("BombUpgrade5")), 7)
        ];
    }
}
