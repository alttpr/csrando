using Randomizer.Graph;

namespace Randomizer.Games.Zelda1;

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

    private static readonly int[] VanillaKeyCounts = [4, 3, 4, 3, 5, 4, 3, 4, 2];

    private int GetKeyCountForLevel(World world, int level)
    {
        if (!world.Config.DungeonShuffle || world.YamlData == null)
            return VanillaKeyCounts[level - 1];

        var levelData = world.YamlData.levels.First(l => l.level == level);
        var lockedDoors = world.YamlData.underworld_maps
            .Where(m => levelData.rooms.Contains(m.map))
            .Sum(m => m.doors.Count(d => d == (int)YamlReader.DoorType.Locked || d == (int)YamlReader.DoorType.Locked2));

        // Each locked door is counted from both sides, so divide by 2; add 1 extra for safety
        return Math.Max(1, lockedDoors / 2 + 1);
    }

    /// <summary>Get list of all items for <paramref name="world"/> in their weighted sets.</summary>
    private List<PooledItem> GetPoolForWorld(World world)
    {
        List<PooledItem> worldSet = [];

        for (int level = 1; level <= 9; level++)
        {
            var setName = new ItemSetName($"z1d{level}", world);
            int keyCount = GetKeyCountForLevel(world, level);

            // With MapPlacement enabled, the Map is constrained to the tighter z1d{level}m set,
            // which DungeonBuilder/YamlReader attach only to item rooms near the dungeon entrance.
            // It is also placed first (weight 0, before keys/compass) so the assumed filler still
            // sees the keys in its inventory and can reach an entrance room that sits behind a
            // locked door; once keys are placed elsewhere such a room could become unreachable.
            bool constrainMap = world.Config.DungeonShuffle && world.Config.MapPlacement != MapPlacementOption.Off;
            var mapSetName = constrainMap ? new ItemSetName($"z1d{level}m", world) : setName;
            int mapWeight = constrainMap ? 0 : 1;

            worldSet.Add(new PooledItem(mapSetName, mapWeight, world.GetItem("Map")));
            worldSet.Add(new PooledItem(setName, 1, world.GetItem("Compass")));
            worldSet.AddRange(Enumerable.Repeat(new PooledItem(setName, 1, world.GetItem("Key")), keyCount));
        }

        worldSet.AddRange([

            new PooledItem(ItemSetName.DefaultSet, 4, world.GetItem("SwordL1")),

            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Bombs")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("StepLadder")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Raft")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Recorder")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SwordL2")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SwordL3")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("BlueCandle")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("RedCandle")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SilverArrows")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Bow")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Arrows")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("MagicalKey")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Rod")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Book")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("BlueRing")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("RedRing")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("PowerBracelet")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Letter")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("MagicShield")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Boomerang")),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("MagicBoomerang")),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("HeartContainer")), 9),
        ]);

        // Consumables-only caves (take-anys, Junk shops, Full-mode repeatable shops): stock each
        // cave's per-cave "z1c{cave}" set with distinct consumables. The set reservation keeps
        // progression out and the distinct draw avoids duplicate items within a cave. Full-mode
        // buy-once shops use "z1shop" and are left unconstrained, so they can hold progression.
        // Take-anys exist regardless of the shop shuffle setting and must always be reserved;
        // shop marker sets simply have no locations when shop shuffle is off.
        AddConsumableCaveItems(world, worldSet, ["z1takeany", "z1shopjunk", "z1shoprepeat"]);

        // Fill the remaining empty locations with filler. The number of generated locations
        // varies per seed (especially with DungeonShuffle), so size the filler to exactly fill
        // what's left after the logic items above — otherwise we either run short (empty
        // locations get "Nothing") or overflow (surplus filler is silently dropped).
        worldSet.AddRange(BuildFiller(world, emptyLocationCount: CountEmptyLocations(world), nonFillerCount: worldSet.Count));

        return worldSet;
    }

    // Filler distribution: weights are relative proportions of the leftover locations.
    private static readonly (string Item, int Weight)[] FillerRatio =
    [
        ("HeartContainer", 4),
        ("Bombs", 20),
        ("Key", 8),
        ("Rupee", 5),
        ("Rupee5", 12),
    ];

    private static int CountEmptyLocations(World world) =>
        world.GetLocationsOfType(VertexType.Item).Count(v => v.Item == null);

    // Consumables sold in junk-mode shops and other consumable caves. Never gate progression.
    private static readonly string[] ShopJunkItems =
        ["RedPotion", "BluePotion", "BlueRing", "Bombs", "Arrows", "Rupee", "Rupee5", "Heart", "Key", "MagicShield"];

    // Shop marker sets whose caves restock on re-entry when the cave ID is an original one
    // (< 0x24; the synthesized buy-once IDs >= 0x24 are emptied after a single purchase).
    private static readonly string[] RepeatableShopMarkers = ["z1shopjunk", "z1shoprepeat"];

    // First cave ID synthesized by ShopShuffler for buy-once shops. IDs below it restock.
    private const int FirstBuyOnceCaveId = 0x24;

    private static int CaveIdOfSet(string caveSetName) =>
        Convert.ToInt32(caveSetName["z1c".Length..], 16);

    /// <summary>
    /// Does any dungeon room in this world hold a passage-blocking enemy (the Hungry Goriya)?
    /// Checked against the world's actual level data, so this keeps working if dungeon shuffle
    /// ever starts placing such enemies in generated layouts.
    /// </summary>
    private static bool WorldNeedsBait(World world)
    {
        var data = world.YamlData;
        if (data == null)
            return !world.Config.DungeonShuffle; // no data means vanilla layout, which has the Goriya

        var blockingIds = data.enemies.enemies.Where(e => e.blocks_passage).Select(e => e.id).ToHashSet();
        var levelRooms = data.levels.Where(l => l.area == YamlReader.Area.Underworld)
            .SelectMany(l => l.rooms).ToHashSet();

        return data.underworld_maps.Any(m =>
        {
            if (!levelRooms.Contains(m.map) || m.passage)
                return false;
            int effectiveId = (m.enemy_mode << 6) | m.enemy_id;
            if (effectiveId >= 0x62) // enemy list: check its members
            {
                return data.enemies.enemy_lists.FirstOrDefault(l => l.id == effectiveId - 0x62)
                    ?.data.Any(blockingIds.Contains) == true;
            }

            return blockingIds.Contains(effectiveId);
        });
    }

    /// <summary>
    /// Stock each consumable cave with distinct consumables drawn into its per-cave "z1c{cave}" set.
    /// A cave qualifies if its locations carry one of <paramref name="markerSets"/>.
    /// </summary>
    private void AddConsumableCaveItems(World world, List<PooledItem> worldSet, string[] markerSets)
    {
        var caveGroups = world.GetLocationsOfType(VertexType.Item)
            .OfType<Vertex>()
            .Where(v => v.ItemSet.Any(s => markerSets.Contains(s.Name) && s.World == world))
            .GroupBy(v => v.ItemSet.First(s => s.Name.StartsWith("z1c") && s.World == world).Name)
            .ToList();

        // If the world contains a passage-blocking enemy, guarantee Bait in a REPEATABLE shop:
        // each feeding consumes the Bait, so the player must be able to buy another — a one-shot
        // source caps the number of feedable Goriyas at one. Weight 0 places it before the
        // progression pass, so later placements see its real location instead of assuming it.
        string? baitCaveSet = null;
        if (WorldNeedsBait(world))
        {
            var repeatableShops = caveGroups
                .Where(g => CaveIdOfSet(g.Key) < FirstBuyOnceCaveId
                            && g.Any(v => v.ItemSet.Any(s => RepeatableShopMarkers.Contains(s.Name))))
                .Select(g => g.Key)
                .ToList();
            if (repeatableShops.Count > 0)
            {
                baitCaveSet = _prng.GetRandomElement(repeatableShops);
                worldSet.Add(new PooledItem(new ItemSetName(baitCaveSet, world), 0, world.GetItem("Bait")));
            }
            else if (world.Config.ShopShuffle != ShopShuffleOption.Off)
            {
                // No repeatable shop rolled (vanishingly rare) — fall back to a one-shot pool
                // Bait. Enough for the single vanilla Goriya; revisit if multiple ever exist.
                worldSet.Add(new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Bait")));
            }
            // With shops vanilla there are no shop locations at all: the vanilla Bait shops keep
            // their wares and YamlReader models the purchase directly, so nothing to add here.
        }

        foreach (var group in caveGroups)
        {
            int slots = group.Count();
            var caveSet = new ItemSetName(group.Key, world);

            // The forced Bait occupies one of this cave's slots.
            if (group.Key == baitCaveSet)
                slots--;

            // Distinct draw per cave; fall back to repeats only if a cave somehow has more slots
            // than the consumable list (it won't, with 3 slots and 10 items).
            var picks = _prng.Shuffle(ShopJunkItems).Take(slots).ToList();
            while (picks.Count < slots)
                picks.Add(_prng.GetRandomElement(ShopJunkItems));

            foreach (var itemName in picks)
                worldSet.Add(new PooledItem(caveSet, 9001, world.GetItem(itemName)));
        }
    }

    /// <summary>
    /// Generate exactly <c>emptyLocationCount - nonFillerCount</c> filler items, distributed
    /// across the filler item types according to <see cref="FillerRatio"/>.
    /// </summary>
    private static IEnumerable<PooledItem> BuildFiller(World world, int emptyLocationCount, int nonFillerCount)
    {
        int fillerNeeded = emptyLocationCount - nonFillerCount;
        if (fillerNeeded <= 0)
            yield break;

        int ratioTotal = FillerRatio.Sum(f => f.Weight);

        // Distribute proportionally, tracking the running total so rounding never leaves us
        // short or over: the last filler type takes whatever remains.
        int placed = 0;
        for (int i = 0; i < FillerRatio.Length; i++)
        {
            var (itemName, ratioWeight) = FillerRatio[i];
            int count = i == FillerRatio.Length - 1
                ? fillerNeeded - placed
                : (int)((long)fillerNeeded * ratioWeight / ratioTotal);

            for (int n = 0; n < count; n++)
                yield return new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem(itemName));

            placed += count;
        }
    }
}
