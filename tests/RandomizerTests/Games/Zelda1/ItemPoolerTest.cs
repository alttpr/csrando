namespace RandomizerTests.Games.Zelda1;

using Randomizer.Games;
using Randomizer.Games.Zelda1;
using Randomizer.Graph;
using Graph = Randomizer.Graph.Graph;

[TestClass]
public sealed class ItemPoolerTest
{
    private static World CreateWorld(bool shuffle, int seed, ShopShuffleOption shop = ShopShuffleOption.Off)
    {
        var z1Config = new Config
        {
            DungeonShuffle = shuffle,
            DungeonStyle = DungeonStyleOption.Progressive,
            EnemyPlacement = EnemyPlacementOption.Progressive,
            ShopShuffle = shop,
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
    [DataTestMethod]
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
    [DataTestMethod]
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
}
