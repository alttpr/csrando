namespace RandomizerTests.Games.SuperMetroid;

using Randomizer.Games.SuperMetroid.Model;
using SuperMetroidRom = Randomizer.Games.SuperMetroid.Rom;

[TestClass]
public sealed class MapTileTest
{
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
