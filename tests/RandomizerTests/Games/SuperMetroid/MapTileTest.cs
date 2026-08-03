namespace RandomizerTests.Games.SuperMetroid;

using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;
using SuperMetroidRom = Randomizer.Games.SuperMetroid.Rom;

[TestClass]
public sealed class MapTileTest
{
    [TestMethod]
    // First and last item-dot chars map to the first and last tiered icons.
    [DataRow(0x0051, ItemTier.Major, 0x0300)]
    [DataRow(0x0051, ItemTier.Medium, 0x0330)]
    [DataRow(0x01F8, ItemTier.Major, 0x0323)]
    [DataRow(0x01F8, ItemTier.Medium, 0x0353)]
    // Flip bits carry over: the tiered pages copy the base tiles' outlines.
    [DataRow(0x4095, ItemTier.Major, 0x430A)]
    [DataRow(0x8053, ItemTier.Medium, 0x8331)]
    // Minor keeps the plain dot.
    [DataRow(0x0051, ItemTier.Minor, 0x0051)]
    // Non-item characters pass through untouched (map station, boss tile, and the
    // graphics-garbage char the old items:0x0152 table entry pointed at).
    [DataRow(0x001D, ItemTier.Major, 0x001D)]
    [DataRow(0x01DF, ItemTier.Major, 0x01DF)]
    [DataRow(0x0152, ItemTier.Major, 0x0152)]
    public void ApplyTier_SwapsItemDotsForTieredIcons(int tileWord, ItemTier tier, int expected)
    {
        Assert.AreEqual((ushort)expected, MapTile.ApplyTier((ushort)tileWord, tier));
    }

    [TestMethod]
    public void GetTileValue_VerticalOpenItemTile_UsesTheItemVariant()
    {
        // Regression: this shape's items variant was 0x0152 (unrelated graphics);
        // the item variant of the $15F outline is $160, which the tier table maps.
        var tile = new MapTile
        {
            Coords = [0, 0],
            Left = TileEdge.Empty,
            Right = TileEdge.Empty,
            Top = TileEdge.Door,
            Bottom = TileEdge.Door,
            Interior = TileInterior.Item,
        };

        Assert.AreEqual((ushort)0x0160, tile.GetTileValue());
        Assert.AreEqual((ushort)0x0319, MapTile.ApplyTier(0x0160, ItemTier.Major));
        Assert.AreEqual((ushort)0x0349, MapTile.ApplyTier(0x0160, ItemTier.Medium));
    }

    [TestMethod]
    [DataRow(TileInterior.MapStation, true, 0x001D)]
    [DataRow(TileInterior.MapStation, false, 0x401D)]
    [DataRow(TileInterior.EnergyRefill, true, 0x001E)]
    [DataRow(TileInterior.EnergyRefill, false, 0x401E)]
    [DataRow(TileInterior.SaveStation, true, 0x0176)]
    [DataRow(TileInterior.SaveStation, false, 0x0177)]
    public void GetPortalTileValue_UsesStationTileForPortalSide(
        TileInterior interior, bool portalOnLeft, int expected)
    {
        var tile = new MapTile
        {
            Coords = [0, 0],
            Interior = interior,
        };

        Assert.AreEqual((ushort)expected, tile.GetPortalTileValue(portalOnLeft));
    }

    [TestMethod]
    [DataRow(null, 0x081C)]
    [DataRow(true, 0x081D)]
    [DataRow(false, 0x481D)]
    public void GetMapStationDecoTileValue_PreservesPortalIcon(
        bool? portalOnLeft, int expected)
    {
        Assert.AreEqual((ushort)expected,
            SuperMetroidRom.GetMapStationDecoTileValue(portalOnLeft));
    }
}
