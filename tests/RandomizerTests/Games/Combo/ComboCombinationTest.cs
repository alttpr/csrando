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
