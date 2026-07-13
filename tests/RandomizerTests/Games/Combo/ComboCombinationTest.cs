namespace RandomizerTests.Games.Combo;

using Randomizer.Games;
using Randomizer.Graph;
using Randomizer.RomModifications;
using ComboConfig = Randomizer.Games.Combo.Config;
using M1Config = Randomizer.Games.Metroid.Config;
using SMConfig = Randomizer.Games.SuperMetroid.Config;
using Z1Config = Randomizer.Games.Zelda1.Config;
using Z3Config = Randomizer.Games.Alttp.Config;

[TestClass]
[DoNotParallelize]
public sealed class ComboCombinationTest
{
    private static WorldConfig CreateConfig(bool alttp, bool sm, bool z1, bool m1) => new()
    {
        Game = RandomizerTarget.Combo,
        Combo = new ComboConfig(),
        Alttp = alttp ? new Z3Config() : null,
        SuperMetroid = sm ? new SMConfig() : null,
        Zelda1 = z1 ? new Z1Config { Triforces = "8" } : null,
        Metroid = m1 ? new M1Config() : null,
    };

    [TestMethod]
    public void VanillaAllGames_ItemPoolMatchesEmptyLocations()
    {
        var graph = new Randomizer.Graph.Graph();
        var world = new Randomizer.Games.Combo.World(
            0, CreateConfig(true, true, true, true), graph, new PRNG(1337));
        var pooler = new Randomizer.Games.Combo.ItemPooler([world], new PRNG(1337));
        int emptyLocations = pooler.SetLocations[ItemSetName.DefaultSet]
            .Distinct()
            .Count(location => location.Item == null);
        string countsByGame = string.Join(", ", new IWorld?[]
            { world.AlttpWorld, world.SMWorld, world.Z1World, world.M1World }
            .OfType<IWorld>()
            .Select(subworld =>
                $"{subworld.GameId}: {pooler.Pool.Count(item => item.Item.World == subworld)} items/" +
                $"{pooler.SetLocations[ItemSetName.DefaultSet].Distinct().Count(location => location.World == subworld && location.Item == null)} locations"));

        Assert.AreEqual(emptyLocations, pooler.Pool.Length,
            $"Pool has {pooler.Pool.Length} items for {emptyLocations} empty locations; " +
            countsByGame);
        Assert.IsFalse(pooler.Pool.Any(item =>
            ReferenceEquals(item.Item.World, world.M1World)
            && item.Item.Name == "Nothing"));
        int m1Padding = Math.Max(0,
            world.M1World!.GetLocationsOfType(VertexType.Item).Count()
                - Randomizer.Games.Metroid.ItemPooler.MaximumNonNothingItems);
        Assert.IsTrue(pooler.Pool.Count(item =>
            ReferenceEquals(item.Item.World, world.AlttpWorld)
            && item.Item.Name == "FiveRupees") >= m1Padding);
    }

    [DataTestMethod]
    [DataRow(true, false, "FiveRupees")]
    [DataRow(false, true, "Rupee5")]
    [DataRow(false, false, "Nothing")]
    public void M1Padding_UsesGlobalTrashWhenAvailable(
        bool alttp, bool z1, string expectedFiller)
    {
        var graph = new Randomizer.Graph.Graph();
        var world = new Randomizer.Games.Combo.World(
            0, CreateConfig(alttp, sm: true, z1, m1: true), graph, new PRNG(1337));
        var pooler = new Randomizer.Games.Combo.ItemPooler([world], new PRNG(1337));
        int emptyLocations = pooler.SetLocations[ItemSetName.DefaultSet]
            .Distinct()
            .Count(location => location.Item == null);
        int m1Padding = Math.Max(0,
            world.M1World!.GetLocationsOfType(VertexType.Item).Count()
                - Randomizer.Games.Metroid.ItemPooler.MaximumNonNothingItems);

        Assert.AreEqual(emptyLocations, pooler.Pool.Length);
        if (expectedFiller == "Nothing")
        {
            var nothingSet = new ItemSetName(
                Randomizer.Games.Metroid.ItemPooler.NothingItemSet, world.M1World);
            Assert.IsTrue(pooler.Pool.Where(item => item.Item.Name == "Nothing")
                .All(item => item.Set == nothingSet));
        }
        else
        {
            Assert.IsFalse(pooler.Pool.Any(item =>
                ReferenceEquals(item.Item.World, world.M1World)
                && item.Item.Name == "Nothing"));
            Assert.IsTrue(pooler.Pool.Count(item => item.Item.Name == expectedFiller)
                >= m1Padding);
        }
    }

    [DataTestMethod]
    [DataRow(false, "alttp", "sm", false)]
    [DataRow(false, "alttp", "m1", false)]
    [DataRow(false, "sm", "sm", true)]
    [DataRow(false, "sm", "m1", false)]
    [DataRow(false, "m1", "sm", false)]
    [DataRow(false, "m1", "m1", true)]
    [DataRow(true, "alttp", "sm", true)]
    [DataRow(true, "alttp", "m1", true)]
    public void MorphFrontFill_UsesInitialGameUnlessOverridden(
        bool earlyMorph, string initialGame, string morphGame, bool expected)
    {
        Assert.AreEqual(expected, RandomAssumedFiller.ShouldFrontFillMorph(
            earlyMorph, initialGame, morphGame));
    }

    [DataTestMethod]
    [DataRow(true, false, false, false)]
    [DataRow(false, true, false, false)]
    [DataRow(false, false, true, false)]
    [DataRow(false, false, false, true)]
    [DataRow(true, true, false, false)]
    [DataRow(true, false, true, false)]
    [DataRow(true, false, false, true)]
    [DataRow(false, true, true, false)]
    [DataRow(false, true, false, true)]
    [DataRow(false, false, true, true)]
    [DataRow(true, true, true, false)]
    [DataRow(true, true, false, true)]
    [DataRow(true, false, true, true)]
    [DataRow(false, true, true, true)]
    [DataRow(true, true, true, true)]
    public void ComboFactory_AllNonEmptyGameSubsets_AreAccepted(bool alttp, bool sm, bool z1, bool m1)
    {
        var randomizer = RandomizerFactory.Create([CreateConfig(alttp, sm, z1, m1)], seed: 1);
        var world = (Randomizer.Games.Combo.World)randomizer.Worlds.Single();

        Assert.AreEqual(alttp, world.AlttpWorld != null);
        Assert.AreEqual(sm, world.SMWorld != null);
        Assert.AreEqual(z1, world.Z1World != null);
        Assert.AreEqual(m1, world.M1World != null);
    }

    [DataTestMethod]
    [TestCategory(TestCategories.Slow)]
    [DataRow(false, true, false, false)]
    [DataRow(false, false, true, false)]
    [DataRow(false, false, false, true)]
    [DataRow(false, true, true, false)]
    [DataRow(false, true, false, true)]
    [DataRow(false, false, true, true)]
    [DataRow(false, true, true, true)]
    public void ComboWithoutAlttp_RandomizesAndWrites(bool alttp, bool sm, bool z1, bool m1)
    {
        var randomizer = RandomizerFactory.Create([CreateConfig(alttp, sm, z1, m1)], seed: 12345);

        randomizer.Randomize();

        Assert.IsTrue(randomizer.IsWinnable());

        var broker = new LoggedRomBroker();
        randomizer.Write(broker);

        Assert.AreEqual(1, broker.Worlds.Count);
        Assert.IsTrue(broker.Worlds.Single().Value.IpsPatch.Length > 0);
    }
}
