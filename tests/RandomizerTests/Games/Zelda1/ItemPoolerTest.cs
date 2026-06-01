namespace RandomizerTests.Games.Zelda1;

using Randomizer.Games;
using Randomizer.Games.Zelda1;
using Randomizer.Graph;
using Graph = Randomizer.Graph.Graph;

[TestClass]
public sealed class ItemPoolerTest
{
    private static World CreateWorld(bool shuffle, int seed)
    {
        var z1Config = new Config
        {
            DungeonShuffle = shuffle,
            DungeonStyle = DungeonStyleOption.Progressive,
            EnemyPlacement = EnemyPlacementOption.Progressive,
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
}
