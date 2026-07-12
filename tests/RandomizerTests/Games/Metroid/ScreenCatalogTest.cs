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
        return ScreenCatalog.Build(reader.Data!.screens, transitions: reader.Data!.transitions);
    }

    [TestMethod]
    public void Build_LoadsAllPlayableAreas()
    {
        var catalog = Load();
        foreach (var area in ScreenCatalog.PlayableAreas)
            Assert.IsTrue(catalog.ForArea(area).Count > 0, $"No screens for {area}");
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

    private static ScreenTransition Impossible(Area area, int fromScreen, Direction fromDir,
        int toScreen, Direction toDir) => new()
        {
            area = area,
            from = new TransitionEndpoint { screen = fromScreen, direction = fromDir },
            to = new TransitionEndpoint { screen = toScreen, direction = toDir },
            impossible = true
        };

    [TestMethod]
    public void OneWayImpossibleTransition_KeepsThePairPlaceable()
    {
        // 0x11 Up -> 0x03 Down is impossible, but the reverse (0x03 Down -> 0x11 Up) is not:
        // a seam you can fall down but not climb back up. The screens must still be placeable.
        var reader = new YamlReader(new Config());
        reader.LoadData();
        reader.Data!.transitions = [Impossible(Area.Brinstar, 0x11, Direction.Up, 0x03, Direction.Down)];
        var catalog = ScreenCatalog.Build(reader.Data.screens, reader.Data.rooms, reader.Data.transitions);
        var lower = catalog.Find(Area.Brinstar, 0x11)!;
        var upper = catalog.Find(Area.Brinstar, 0x03)!;

        // The pairing is allowed from either query orientation (a placement, not a traversal).
        Assert.IsTrue(catalog.ScreensCanConnect(lower, Direction.Up, upper));
        Assert.IsTrue(catalog.ScreensCanConnect(upper, Direction.Down, lower));

        var grid = new WorldGrid();
        var run = grid.PlaceRun(Area.Brinstar, Scrolling.Vertical, new Point(1, 1), 2, CellRole.Shaft);
        run.Cells[0].ForcedScreenId = 0x03;
        run.Cells[1].ForcedScreenId = 0x11;

        // Fitting the placement must NOT throw — the one-way seam is legal.
        ScreenFitter.Fit(grid, catalog, 1);
    }

    [TestMethod]
    public void BothWayImpossibleTransition_RejectsTheScreenPair()
    {
        // Impossible in both traversal directions => the screens may never be adjacent.
        var reader = new YamlReader(new Config());
        reader.LoadData();
        reader.Data!.transitions =
        [
            Impossible(Area.Brinstar, 0x11, Direction.Up, 0x03, Direction.Down),
            Impossible(Area.Brinstar, 0x03, Direction.Down, 0x11, Direction.Up),
        ];
        var catalog = ScreenCatalog.Build(reader.Data.screens, reader.Data.rooms, reader.Data.transitions);
        var lower = catalog.Find(Area.Brinstar, 0x11)!;
        var upper = catalog.Find(Area.Brinstar, 0x03)!;

        Assert.IsFalse(catalog.ScreensCanConnect(lower, Direction.Up, upper));
        Assert.IsFalse(catalog.ScreensCanConnect(upper, Direction.Down, lower));

        var grid = new WorldGrid();
        var run = grid.PlaceRun(Area.Brinstar, Scrolling.Vertical, new Point(1, 1), 2, CellRole.Shaft);
        run.Cells[0].ForcedScreenId = 0x03;
        run.Cells[1].ForcedScreenId = 0x11;

        Assert.ThrowsException<InvalidOperationException>(() => ScreenFitter.Fit(grid, catalog, 1));
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
