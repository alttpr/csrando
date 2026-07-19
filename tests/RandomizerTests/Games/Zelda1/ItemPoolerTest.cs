namespace RandomizerTests.Games.Zelda1;

using Randomizer.Games;
using Randomizer.Games.Zelda1;
using Randomizer.Graph;
using Graph = Randomizer.Graph.Graph;

[TestClass]
public sealed class ItemPoolerTest
{
    private static World CreateWorld(bool shuffle, int seed, ShopShuffleOption shop = ShopShuffleOption.Off,
        MapPlacementOption mapPlacement = MapPlacementOption.Off)
    {
        var z1Config = new Config
        {
            DungeonShuffle = shuffle,
            DungeonStyle = DungeonStyleOption.Progressive,
            EnemyPlacement = EnemyPlacementOption.Progressive,
            ShopShuffle = shop,
            MapPlacement = mapPlacement,
            Triforces = "8",
        };
        z1Config.SelectRandomValues(new PRNG(seed));
        var worldConfig = new WorldConfig { Zelda1 = z1Config };
        return new World(1, worldConfig, new Graph(), new PRNG(seed));
    }

    // The number of generated item locations varies per seed, so the filler portion of the
    // pool is sized dynamically to exactly fill the empty locations. If the pool is larger than
    // the empty locations, surplus filler is silently dropped; if smaller, locations are left
    // empty and show up as "Nothing". Either way the pool must match the empty-location count.
    [TestMethod]
    [DataRow(false, 42)]
    [DataRow(true, 1)]
    [DataRow(true, 3)]
    [DataRow(true, 100)]
    [DataRow(true, 999)]
    public void Pool_ExactlyMatchesEmptyLocations(bool shuffle, int seed)
    {
        var world = CreateWorld(shuffle, seed);
        var pooler = new ItemPooler([world], new PRNG(seed));

        int emptyLocations = world.GetLocationsOfType(VertexType.Item).Count(v => v.Item == null);

        Assert.AreEqual(emptyLocations, pooler.Pool.Length,
            $"Pool size ({pooler.Pool.Length}) must equal the number of empty item locations " +
            $"({emptyLocations}) so every location is filled and no item is dropped.");
    }

    // Shop shuffle adds shop slots as empty locations; the pool sizing must still match exactly so
    // every slot (shop or not) is filled. Junk mode additionally constrains its slots to a
    // consumables-only set, which must be sized to exactly cover those slots.
    [TestMethod]
    [DataRow(ShopShuffleOption.Junk, 42)]
    [DataRow(ShopShuffleOption.Junk, 7)]
    [DataRow(ShopShuffleOption.Full, 42)]
    [DataRow(ShopShuffleOption.Full, 7)]
    public void Pool_ExactlyMatchesEmptyLocations_WithShopShuffle(ShopShuffleOption shop, int seed)
    {
        var world = CreateWorld(shuffle: false, seed, shop);
        var pooler = new ItemPooler([world], new PRNG(seed));

        int emptyLocations = world.GetLocationsOfType(VertexType.Item).Count(v => v.Item == null);

        Assert.AreEqual(emptyLocations, pooler.Pool.Length,
            $"Pool size ({pooler.Pool.Length}) must equal the number of empty item locations " +
            $"({emptyLocations}) with shop shuffle {shop}.");
    }

    // With MapPlacement enabled each dungeon's Map must be pooled into the tighter z1d{level}m set,
    // and that set must have at least one location (the entrance-adjacent item room tagged by the
    // builder) — otherwise placement would fail with "No locations for Map". Pool size is unchanged
    // because the Map is just reassigned to a different set, not added or removed.
    [TestMethod]
    [DataRow(MapPlacementOption.Early, 1)]
    [DataRow(MapPlacementOption.Early, 42)]
    [DataRow(MapPlacementOption.Closest, 7)]
    [DataRow(MapPlacementOption.Closest, 999)]
    public void Pool_MapPlacementOn_MapConstrainedToEntranceSet(MapPlacementOption placement, int seed)
    {
        var world = CreateWorld(shuffle: true, seed, mapPlacement: placement);
        var pooler = new ItemPooler([world], new PRNG(seed));

        var mapItem = world.GetItem("Map");
        var mapEntries = pooler.Pool.Where(p => p.Item == mapItem).ToList();
        Assert.AreEqual(9, mapEntries.Count, "There should be one Map per dungeon (9 total).");

        foreach (var entry in mapEntries)
        {
            Assert.IsTrue(entry.Set.Name.EndsWith('m') && entry.Set.Name.StartsWith("z1d"),
                $"Map should be pooled into a z1d{{level}}m set, but was in '{entry.Set.Name}'.");
            Assert.IsTrue(pooler.SetLocations[entry.Set].Count > 0,
                $"Set '{entry.Set.Name}' must have at least one location for the Map to be placed.");
        }

        int emptyLocations = world.GetLocationsOfType(VertexType.Item).Count(v => v.Item == null);
        Assert.AreEqual(emptyLocations, pooler.Pool.Length,
            "Reassigning the Map to a tighter set must not change the overall pool size.");
    }
}
