namespace RandomizerTests.Games.Metroid;

using Randomizer.Games.Metroid;
using Randomizer.Games.Metroid.MapGen;
using static Randomizer.Games.Metroid.YamlReader;

[TestClass]
public sealed class ScreenCatalogTest
{
    private static ScreenCatalog Load()
    {
        var reader = new YamlReader(new Config());
        reader.LoadData();
        return ScreenCatalog.Build(reader.Data!.screens);
    }

    [TestMethod]
    public void Build_LoadsAllPlayableAreas()
    {
        var catalog = Load();
        foreach (var area in ScreenCatalog.PlayableAreas)
            Assert.IsTrue(catalog.ForArea(area).Count > 0, $"No screens for {area}");
    }

    [TestMethod]
    public void Build_DumpCatalog()
    {
        var catalog = Load();
        Console.WriteLine(catalog.Dump());
    }

    [TestMethod]
    public void Landmarks_ClassifyAsExpected()
    {
        var catalog = Load();

        var start = catalog.Find(Area.Brinstar, 0x09);
        Assert.IsNotNull(start);
        Assert.IsTrue(start.HasStartLocation);
        Assert.AreEqual(Scrolling.Horizontal, start.Axis);
        Assert.AreEqual(ConnectorType.Scroll, start.Left.Type);
        Assert.AreEqual(ConnectorType.Scroll, start.Right.Type);

        var statues = catalog.Find(Area.Brinstar, 0x2B);
        Assert.IsNotNull(statues);
        Assert.AreEqual(ConnectorType.Door, statues.Left.Type);
        Assert.AreEqual(ConnectorType.Door, statues.Right.Type);

        var tourianElevator = catalog.Find(Area.Brinstar, 0x2C);
        Assert.IsNotNull(tourianElevator);
        Assert.AreEqual(ConnectorType.Door, tourianElevator.Right.Type);
        Assert.AreEqual(ConnectorType.Elevator, tourianElevator.Down.Type);
        Assert.IsTrue(tourianElevator.HasElevatorPlatform);

        var brinstarElevatorShaft = catalog.Find(Area.Brinstar, 0x01);
        Assert.IsNotNull(brinstarElevatorShaft);
        Assert.AreEqual(ConnectorType.Elevator, brinstarElevatorShaft.Up.Type);
        Assert.AreEqual(ConnectorType.Elevator, brinstarElevatorShaft.Down.Type);

        var kraid = catalog.Find(Area.Kraid, 0x1D);
        Assert.IsNotNull(kraid);
        Assert.IsTrue(kraid.HasBossLocation);
        Assert.IsTrue(kraid.HasItemLocation);
        Assert.AreEqual(ConnectorType.Door, kraid.Right.Type);
        Assert.AreEqual(DoorType.Red, kraid.Right.Color);

        var ridley = catalog.Find(Area.Ridley, 0x12);
        Assert.IsNotNull(ridley);
        Assert.IsTrue(ridley.HasBossLocation);

        var motherBrain = catalog.Find(Area.Tourian, 0x04);
        Assert.IsNotNull(motherBrain);
        Assert.IsTrue(motherBrain.HasBossLocation);
    }

    [TestMethod]
    public void OneWay_RidleyFallShaftIsFlagged()
    {
        var catalog = Load();
        var fallShaft = catalog.Find(Area.Ridley, 0x04);
        Assert.IsNotNull(fallShaft);
        Assert.IsTrue(fallShaft.IsOneWay);
        Assert.AreEqual(Direction.Down, fallShaft.OneWayDirection);
    }

    [TestMethod]
    public void Vocabulary_EveryAreaHasShaftBodiesAndCorridors()
    {
        var catalog = Load();
        foreach (var area in ScreenCatalog.PlayableAreas)
        {
            Assert.IsTrue(catalog.HasPiece(area, Scrolling.Vertical,
                EdgeRequirement.Scroll, EdgeRequirement.Scroll, EdgeRequirement.Wall, EdgeRequirement.Wall),
                $"{area} lacks a plain vertical shaft body");

            Assert.IsTrue(catalog.HasPiece(area, Scrolling.Horizontal,
                EdgeRequirement.Wall, EdgeRequirement.Wall, EdgeRequirement.Scroll, EdgeRequirement.Scroll),
                $"{area} lacks a plain horizontal corridor body");
        }
    }
}
