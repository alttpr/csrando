namespace RandomizerTests.Games.Metroid;

using Randomizer.Games;
using Randomizer.Games.Metroid;
using Randomizer.Games.Metroid.MapGen;
using Randomizer.Graph;
using Randomizer.RomModifications;
using static Randomizer.Games.Metroid.YamlReader;
using GameRandomizer = Randomizer.Games.Metroid.GameRandomizer;
using MetroidWorld = Randomizer.Games.Metroid.World;

/// <summary>
/// End-to-end coverage for tiered map icons on the M1 automap: the post-fill pass
/// must rewrite item glyphs to their energy/major groups on generated maps (and
/// re-stamp the seed id), and patch the vanilla payload's item cells in place on
/// vanilla layouts.
/// </summary>
[TestClass]
public sealed class MapIconPatcherTest
{
    private static MetroidWorld Generate(Config config, int seed = 99)
    {
        var randomizer = new GameRandomizer([new WorldConfig { Metroid = config }], new PRNG(seed));
        randomizer.Randomize();
        return (MetroidWorld)randomizer.Worlds[0];
    }

    private static int ExpectedGroupBase(AutomapTiles tiles, ItemTier tier) => tier switch
    {
        ItemTier.Major => tiles.MajorTileBase,
        ItemTier.Medium => tiles.EnergyTileBase,
        _ => tiles.ItemTileBase,
    };

    [TestMethod]
    public void MapShuffle_TieredIconsRewriteItemGlyphsAndSeedId()
    {
        var world = Generate(new Config { MapShuffle = true, TieredItems = TieredItemsSetting.On });
        var rom = new LoggedRom();
        RomWriter.Write(rom, world, new PRNG(99));

        var tiles = AutomapTiles.Load();
        var generated = world.GeneratedMap!;
        // The composer reproduces the pre-tier planes; every item cell must differ
        // from them only in its glyph group, keeping connection variant and flips.
        var untiered = AutomapComposer.Compose(generated);
        var cellByAddress = generated.ItemAddresses!.ToDictionary(kv => (long)kv.Value, kv => kv.Key);

        int checkedTiers = 0;
        var seenTiers = new HashSet<ItemTier>();
        foreach (var location in world.GetLocationsOfType(VertexType.Item))
        {
            if (location.Item == null || location.Addresses is not [var address, ..])
                continue;
            var point = cellByAddress[address];
            var area = generated.Grid.Cell(point)!.Area;
            int offset = (int)area * 0x800 + (point.Y * WorldGrid.Size + point.X) * 2;

            ushort original = (ushort)(untiered.Planes[offset] | untiered.Planes[offset + 1] << 8);
            byte[] written = rom.Read(AutomapComposer.TilemapsAddress + offset, 2);
            ushort tiered = (ushort)(written[0] | written[1] << 8);

            int originalCharacter = original & tiles.CharacterMask;
            if (originalCharacter < tiles.ItemTileBase
                || originalCharacter >= tiles.ItemTileBase + tiles.ItemTileCount)
            {
                // An item on a cell drawn with a non-item glyph (boss room) has no
                // tiered variant and must keep its glyph.
                Assert.AreEqual(original, tiered, $"non-item glyph at {point} in {area}");
                continue;
            }

            int expectedCharacter = ExpectedGroupBase(tiles, location.Item.Tier)
                + (originalCharacter - tiles.ItemTileBase);
            Assert.AreEqual((ushort)(original & ~tiles.CharacterMask | expectedCharacter), tiered,
                $"cell {point} in {area} holding {location.Item.Name}");
            checkedTiers++;
            seenTiers.Add(location.Item.Tier);
        }

        Assert.IsTrue(checkedTiers > 10, "the seed placed a meaningful number of items");
        Assert.IsTrue(seenTiers.Contains(ItemTier.Major) && seenTiers.Contains(ItemTier.Medium)
            && seenTiers.Contains(ItemTier.Minor), "all three tiers occurred");

        // The stamped seed id must hash the payload actually written.
        byte[] planes = rom.Read(AutomapComposer.TilemapsAddress, 5 * 0x800);
        byte[] bounds = rom.Read(AutomapComposer.BoundsAddress, 20);
        uint seedId = AutomapComposer.SeedHash(planes, bounds);
        CollectionAssert.AreEqual(
            (byte[])[(byte)seedId, (byte)(seedId >> 8), (byte)(seedId >> 16), (byte)(seedId >> 24)],
            rom.Read(AutomapComposer.SeedIdAddress, 4), "seed id matches the tiered planes");
    }

    private static bool InTilemapRange(int address) =>
        address >= AutomapComposer.TilemapsAddress && address < AutomapComposer.TilemapsAddress + 5 * 0x800;

    private static Dictionary<int, ushort> ExpectedVanillaPatches(
        MetroidWorld world, Func<IItem, ItemTier> resolver)
    {
        var tiles = AutomapTiles.Load();
        var expected = new Dictionary<int, ushort>();
        foreach (var location in world.GetLocationsOfType(VertexType.Item))
        {
            if (location.Item == null || location.Addresses is not [var address, ..])
                continue;
            var tier = resolver(location.Item);
            if (tier == ItemTier.Minor)
                continue;
            Assert.IsTrue(DataLoader.TryGetVanillaItemCell(address, out var area, out var cell),
                $"vanilla cell for {location.Name}");
            int patchAddress = AutomapComposer.TilemapsAddress
                + (int)area * 0x800 + (cell.Y * WorldGrid.Size + cell.X) * 2;
            expected[patchAddress] = (ushort)(ExpectedGroupBase(tiles, tier) + (0x109 - tiles.ItemTileBase)
                | tiles.Attributes);
        }
        return expected;
    }

    private static void AssertVanillaTilemapPatches(MetroidWorld world, Dictionary<int, ushort> expected)
    {
        var actual = world.PatchData!.Where(kv => InTilemapRange(kv.Key))
            .ToDictionary(kv => kv.Key, kv => (ushort)(kv.Value[0] | kv.Value[1] << 8));

        CollectionAssert.AreEquivalent(expected.Keys.ToList(), actual.Keys.ToList(),
            "patched tilemap words");
        foreach (var (address, word) in expected)
            Assert.AreEqual(word, actual[address], $"word at 0x{address:X}");
    }

    [TestMethod]
    public void Vanilla_TieredIconsPatchTheItemCells()
    {
        var world = Generate(new Config { TieredItems = TieredItemsSetting.On });
        RomWriter.Write(new LoggedRom(), world, new PRNG(99));

        var expected = ExpectedVanillaPatches(world, item => item.Tier);
        Assert.IsTrue(expected.Count > 5, "the seed placed majors/energy at vanilla-mapped cells");
        AssertVanillaTilemapPatches(world, expected);

        // Spot-check the encoding: major = $2D29, energy = $2D24 over the vanilla $2D09.
        foreach (var word in expected.Values)
            Assert.IsTrue(word == 0x2D29 || word == 0x2D24, $"vanilla tier word ${word:X4}");
    }

    [TestMethod]
    public void Vanilla_CustomModeOnlyPatchesTheListedItems()
    {
        var world = Generate(new Config
        {
            TieredItems = TieredItemsSetting.Custom,
            CustomItemTiers = "m1:IceBeam=major",
        });
        RomWriter.Write(new LoggedRom(), world, new PRNG(99));

        var resolver = ItemTiers.CreateResolver(TieredItemsSetting.Custom, "m1:IceBeam=major")!;
        var expected = ExpectedVanillaPatches(world, resolver);
        Assert.IsTrue(expected.Count <= 1, "only Ice Beam may be patched");
        AssertVanillaTilemapPatches(world, expected);
    }

    [TestMethod]
    public void Vanilla_OffLeavesTheTilemapsUntouched()
    {
        var world = Generate(new Config());
        RomWriter.Write(new LoggedRom(), world, new PRNG(99));

        Assert.IsFalse(world.PatchData!.Keys.Any(InTilemapRange),
            "no tilemap patches when the feature is off");
    }
}
