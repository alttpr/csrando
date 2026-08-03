namespace RandomizerTests.Games;

using Microsoft.AspNetCore.Http.HttpResults;
using Randomizer.ApiControllers;
using Randomizer.Games;
using Randomizer.Graph;
using AlttpConfig = Randomizer.Games.Alttp.Config;
using AlttpYaml = Randomizer.Games.Alttp.YamlReader;
using MetroidConfig = Randomizer.Games.Metroid.Config;
using MetroidYaml = Randomizer.Games.Metroid.YamlReader;
using MapRandomizerSetting = Randomizer.Games.SuperMetroid.MapRandomizerSetting;
using SmJsonReader = Randomizer.Games.SuperMetroid.Model.JsonReader;
using SuperMetroidConfig = Randomizer.Games.SuperMetroid.Config;
using Zelda1Config = Randomizer.Games.Zelda1.Config;
using Zelda1Yaml = Randomizer.Games.Zelda1.YamlReader;

[TestClass]
public sealed class ItemTiersTest
{
    // ---------- tier declarations in item data ----------

    [TestMethod]
    [DataRow(null, ItemTier.Major)]
    [DataRow("", ItemTier.Major)]
    [DataRow("  ", ItemTier.Major)]
    [DataRow("minor", ItemTier.Minor)]
    [DataRow("medium", ItemTier.Medium)]
    [DataRow("Major", ItemTier.Major)]
    [DataRow("MEDIUM", ItemTier.Medium)]
    public void ParseDeclaration_AcceptsDataFileValues(string? declared, ItemTier expected)
    {
        Assert.AreEqual(expected, ItemTiers.ParseDeclaration(declared));
    }

    [TestMethod]
    public void ParseDeclaration_ThrowsOnTypos()
    {
        Assert.ThrowsExactly<FormatException>(() => ItemTiers.ParseDeclaration("engery"));
    }

    [TestMethod]
    public void ItemDataFiles_DeclareTheExpectedTiers()
    {
        var m1 = MetroidYaml.LoadItems();
        Assert.AreEqual(ItemTier.Medium, ItemTiers.ParseDeclaration(m1["EnergyTank"].Tier));
        Assert.AreEqual(ItemTier.Minor, ItemTiers.ParseDeclaration(m1["Missile"].Tier));
        Assert.AreEqual(ItemTier.Minor, ItemTiers.ParseDeclaration(m1["Nothing"].Tier));
        Assert.AreEqual(ItemTier.Major, ItemTiers.ParseDeclaration(m1["IceBeam"].Tier));

        var z1 = Zelda1Yaml.LoadItems();
        Assert.AreEqual(ItemTier.Medium, ItemTiers.ParseDeclaration(z1["HeartContainer"].Tier));
        Assert.AreEqual(ItemTier.Minor, ItemTiers.ParseDeclaration(z1["Rupee"].Tier));
        Assert.AreEqual(ItemTier.Major, ItemTiers.ParseDeclaration(z1["SwordL1"].Tier));

        var alttp = AlttpYaml.LoadItems();
        Assert.AreEqual(ItemTier.Medium, ItemTiers.ParseDeclaration(alttp["PieceOfHeart"].Tier));
        Assert.AreEqual(ItemTier.Minor, ItemTiers.ParseDeclaration(alttp["OneRupee"].Tier));
        Assert.AreEqual(ItemTier.Minor, ItemTiers.ParseDeclaration(alttp["KeyD1"].Tier));
        Assert.AreEqual(ItemTier.Minor, ItemTiers.ParseDeclaration(alttp["MapD3"].Tier));
        Assert.AreEqual(ItemTier.Major, ItemTiers.ParseDeclaration(alttp["Hookshot"].Tier));
        Assert.AreEqual(ItemTier.Major, ItemTiers.ParseDeclaration(alttp["BigKeyD1"].Tier));
    }

    [TestMethod]
    public void ItemDataFiles_AllDeclaredTiersParse()
    {
        // A typo in any tier: field should fail here, not silently classify as Major.
        foreach (var (name, item) in MetroidYaml.LoadItems())
            _ = ItemTiers.ParseDeclaration(item.Tier);
        foreach (var (name, item) in Zelda1Yaml.LoadItems())
            _ = ItemTiers.ParseDeclaration(item.Tier);
        foreach (var (name, item) in AlttpYaml.LoadItems())
            _ = ItemTiers.ParseDeclaration(item.Tier);

        var smTiers = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
            File.ReadAllText(Path.Combine(SmJsonReader.DataRoot, "item_tiers.json")))!;
        Assert.AreEqual(ItemTier.Medium, ItemTiers.ParseDeclaration(smTiers["ETank"]));
        Assert.AreEqual(ItemTier.Medium, ItemTiers.ParseDeclaration(smTiers["ReserveTank"]));
        Assert.AreEqual(ItemTier.Minor, ItemTiers.ParseDeclaration(smTiers["Missile"]));
        Assert.AreEqual(ItemTier.Minor, ItemTiers.ParseDeclaration(smTiers["Super"]));
        Assert.AreEqual(ItemTier.Minor, ItemTiers.ParseDeclaration(smTiers["PowerBomb"]));
        foreach (var (name, tier) in smTiers)
        {
            if (name != "$comment")
                _ = ItemTiers.ParseDeclaration(tier);
        }
    }

    // ---------- custom override parsing ----------

    [TestMethod]
    public void ParseOverrides_EmptyMeansNoOverrides()
    {
        Assert.AreEqual(0, ItemTiers.ParseOverrides(null).Count);
        Assert.AreEqual(0, ItemTiers.ParseOverrides("").Count);
        Assert.AreEqual(0, ItemTiers.ParseOverrides("  \n ").Count);
    }

    [TestMethod]
    public void ParseOverrides_ParsesAndCanonicalizesEntries()
    {
        var overrides = ItemTiers.ParseOverrides("M1:icebeam=MAJOR, sm:Charge=medium;\nz1:swordl1=minor");

        Assert.AreEqual(3, overrides.Count);
        Assert.AreEqual(ItemTier.Major, overrides[("m1", "IceBeam")]);
        Assert.AreEqual(ItemTier.Medium, overrides[("sm", "Charge")]);
        Assert.AreEqual(ItemTier.Minor, overrides[("z1", "SwordL1")]);
    }

    [TestMethod]
    public void ParseOverrides_NamesEveryBadEntry()
    {
        var ex = Assert.ThrowsExactly<FormatException>(() =>
            ItemTiers.ParseOverrides("m1:IceBaem=major, gb:Missile=minor, m1:Morph=huge, JustGarbage"));

        StringAssert.Contains(ex.Message, "IceBaem");
        StringAssert.Contains(ex.Message, "gb");
        StringAssert.Contains(ex.Message, "huge");
        StringAssert.Contains(ex.Message, "JustGarbage");
    }

    [TestMethod]
    public void CreateResolver_OffIsNull()
    {
        Assert.IsNull(ItemTiers.CreateResolver(TieredItemsSetting.Off, "whatever, not even parsed"));
    }

    // ---------- request validation (the SM+M1-only gate) ----------

    private static WorldConfig ComboConfig(
        TieredItemsSetting sm = TieredItemsSetting.Off,
        TieredItemsSetting m1 = TieredItemsSetting.Off,
        string customList = "",
        bool withZelda = false) => new()
    {
        Game = RandomizerTarget.Combo,
        SuperMetroid = new SuperMetroidConfig
        {
            TieredItems = sm,
            CustomItemTiers = customList,
            // SM tiered icons only exist under map randomization (DependsOn), so the
            // setting is inert — and not validated — without it.
            MapRandomizer = MapRandomizerSetting.Standard,
        },
        Metroid = new MetroidConfig { TieredItems = m1, CustomItemTiers = customList },
        Alttp = withZelda ? new AlttpConfig() : null,
        Zelda1 = withZelda ? new Zelda1Config() : null,
    };

    [TestMethod]
    public void Validate_AllowsMetroidOnlySeeds()
    {
        Assert.IsNull(WorldConfigValidator.Validate(
            [ComboConfig(sm: TieredItemsSetting.On, m1: TieredItemsSetting.On)]));
    }

    [TestMethod]
    public void Validate_RejectsTieredItemsWhenOtherGamesAreActive()
    {
        string? error = WorldConfigValidator.Validate(
            [ComboConfig(sm: TieredItemsSetting.On, withZelda: true)]);
        Assert.IsNotNull(error);
        StringAssert.Contains(error, "Tiered Items");

        Assert.IsNotNull(WorldConfigValidator.Validate(
            [ComboConfig(m1: TieredItemsSetting.Custom, customList: "m1:IceBeam=major", withZelda: true)]));
    }

    [TestMethod]
    public void Validate_InertTieredItemsAreAllowed()
    {
        // Off never offends, and a non-default value whose DependsOn chain is not
        // satisfied (SM tiers without the map randomizer) is ignored by the
        // generator, so it passes validation too.
        Assert.IsNull(WorldConfigValidator.Validate([ComboConfig(withZelda: true)]));

        var inert = new WorldConfig
        {
            Game = RandomizerTarget.Combo,
            SuperMetroid = new SuperMetroidConfig { TieredItems = TieredItemsSetting.On },
            Alttp = new AlttpConfig(),
        };
        Assert.IsNull(WorldConfigValidator.Validate([inert]));
    }

    [TestMethod]
    public void Validate_RejectsBadCustomLists()
    {
        string? error = WorldConfigValidator.Validate(
            [ComboConfig(m1: TieredItemsSetting.Custom, customList: "m1:IceBaem=major")]);
        Assert.IsNotNull(error);
        StringAssert.Contains(error, "IceBaem");

        Assert.IsNull(WorldConfigValidator.Validate(
            [ComboConfig(m1: TieredItemsSetting.Custom, customList: "m1:IceBeam=major")]));
    }

    // ---------- metadata emission ----------

    [TestMethod]
    public void Metadata_TieredItemsCarriesTheGameGateAndWipVisibility()
    {
        var result = (Ok<MetaRootSettings>)new MetaController().Get(RandomizerTarget.Combo);
        var settings = result.Value!.TargetSettings;

        foreach (string gameKey in (string[])["SuperMetroid", "Metroid"])
        {
            var tieredItems = settings[gameKey].Settings.Single(s => s.Key == "TieredItems");
            CollectionAssert.AreEquivalent(
                (string[])["SuperMetroid", "Metroid"], tieredItems.OnlyWithGames,
                $"{gameKey} TieredItems game gate");
            Assert.AreEqual(MetaSettingsVisibility.Wip, tieredItems.Visibility);

            var customList = settings[gameKey].Settings.Single(s => s.Key == "CustomItemTiers");
            CollectionAssert.AreEquivalent(
                (string[])["SuperMetroid", "Metroid"], customList.OnlyWithGames);
            Assert.AreEqual("TieredItems", customList.DependsOn?.Key);
        }
    }
}
