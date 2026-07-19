namespace RandomizerTests.Games.Metroid;

using System.Text.Json;
using Randomizer.Games;
using Randomizer.Games.Metroid;
using Randomizer.Games.Metroid.MapGen;
using Randomizer.Graph;
using Randomizer.RomModifications;
using static Randomizer.Games.Metroid.YamlReader;
using GameRandomizer = Randomizer.Games.Metroid.GameRandomizer;

/// <summary>
/// Automap payload acceptance: the composed planes/bounds/seed-id must follow the
/// M1MP data format consumed by asm/multirando-asm/src/m1/randomizer/automap.asm.
/// Glyph selection is checked against the semantic tile groups of the
/// m1_map_tiles.json lookup.
/// </summary>
[TestClass]
public sealed class AutomapComposerTest
{
    private const ushort Attributes = 0x2C00;      // BG3 priority + palette 3
    private const ushort CharacterMask = 0x03FF;

    private static readonly Lazy<ScreenCatalog> Catalog = new(() =>
    {
        var reader = new YamlReader(new Config());
        reader.LoadData();
        return ScreenCatalog.Build(reader.Data!.screens, reader.Data!.rooms, reader.Data!.transitions);
    });

    private static readonly Lazy<Dictionary<string, (int Base, int Count)>> TileGroups = new(() =>
    {
        using var doc = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(DataRoot, "m1_map_tiles.json")));
        var bases = doc.RootElement.GetProperty("groupTileBases");
        var counts = doc.RootElement.GetProperty("groupTileCounts");
        return bases.EnumerateObject().ToDictionary(
            p => p.Name,
            p => (p.Value.GetInt32(), counts.GetProperty(p.Name).GetInt32()));
    });

    private static GeneratedWorld Generate(int seed) =>
        new TopologyGenerator(Catalog.Value).Generate(seed);

    private static ushort WordAt(AutomapPayload payload, Area area, Point p)
    {
        int offset = (int)area * 0x800 + (p.Y * WorldGrid.Size + p.X) * 2;
        return (ushort)(payload.Planes[offset] | payload.Planes[offset + 1] << 8);
    }

    private static bool InGroup(ushort word, string group)
    {
        var (tileBase, count) = TileGroups.Value[group];
        int character = word & CharacterMask;
        return character >= tileBase && character < tileBase + count;
    }

    /// <summary>
    /// Walks up from the test directory to the asm submodule's data directory to find
    /// checked-in map assets the randomizer itself does not need (golden files, the
    /// source-of-truth tile lookup). Null when the submodule is not checked out.
    /// </summary>
    private static string? FindVanillaAsset(string name)
    {
        for (var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); dir != null; dir = dir.Parent)
        {
            string candidate = Path.Combine(dir.FullName, "asm", "multirando-asm", "src", "data", name);
            if (File.Exists(candidate))
                return candidate;
        }
        return null;
    }

    [TestMethod]
    public void MapTilesJson_SnapshotMatchesTheAsmSourceOfTruth()
    {
        // The composer reads the C#-side snapshot while the base ROM's glyphs are
        // assembled from the asm-side original; silent drift between the two shows
        // up only as wrong glyphs in game, so fail loudly here instead.
        string? source = FindVanillaAsset("m1_map_tiles.json");
        if (source == null)
            Assert.Inconclusive("asm/multirando-asm source m1_map_tiles.json not found");

        CollectionAssert.AreEqual(
            File.ReadAllBytes(source!),
            File.ReadAllBytes(Path.Combine(DataRoot, "m1_map_tiles.json")),
            "re-copy asm/multirando-asm/src/data/m1_map_tiles.json to src/Randomizer/Games/Metroid/data/");
    }

    [TestMethod]
    public void Compose_PayloadShapeSeedIdAndDeterminism()
    {
        var payload = AutomapComposer.Compose(Generate(7));
        Assert.AreEqual(5 * 0x800, payload.Planes.Length, "five 2 KiB planes");
        Assert.AreEqual(5 * 4, payload.Bounds.Length, "four bounds bytes per area");
        Assert.AreEqual(AutomapComposer.SeedHash(payload.Planes, payload.Bounds), payload.SeedId,
            "seed id must be the hash of planes followed by bounds");

        var again = AutomapComposer.Compose(Generate(7));
        CollectionAssert.AreEqual(payload.Planes, again.Planes, "planes must be deterministic");
        CollectionAssert.AreEqual(payload.Bounds, again.Bounds, "bounds must be deterministic");
        Assert.AreEqual(payload.SeedId, again.SeedId, "seed id must be deterministic");
    }

    [TestMethod]
    public void Compose_EveryCellDrawnInItsPlaneAndBoundsAreTight()
    {
        for (int seed = 1; seed <= 10; seed++)
        {
            var world = Generate(seed);
            var payload = AutomapComposer.Compose(world);
            var grid = world.Grid;

            // Elevator columns draw decoration words at the partner area's coordinates
            // in both planes involved; everything else must sit exactly on its cell.
            var expectedCoords = new HashSet<(Area, Point)>();
            foreach (var cell in grid.Cells.Where(c => c.Role != CellRole.Cap))
                expectedCoords.Add((cell.Area, cell.Position));
            foreach (var link in grid.Links.Where(l => l.Type == LinkType.Elevator))
            {
                var upper = grid.Cell(link.A)!.Area;
                var lower = grid.Cell(link.B)!.Area;
                foreach (var p in new[] { link.A, link.B, link.B.Step(Direction.Down) })
                {
                    expectedCoords.Add((upper, p));
                    expectedCoords.Add((lower, p));
                }
            }

            for (int area = 0; area < 5; area++)
            {
                int minX = 0xFF, maxX = 0, minY = 0xFF, maxY = 0;
                for (int y = 0; y < WorldGrid.Size; y++)
                {
                    for (int x = 0; x < WorldGrid.Size; x++)
                    {
                        ushort word = WordAt(payload, (Area)area, new Point(x, y));
                        bool expected = expectedCoords.Contains(((Area)area, new Point(x, y)));
                        if (word == 0)
                        {
                            Assert.IsFalse(expected, $"seed {seed}: {(Area)area} ({x},{y}) should be drawn");
                            continue;
                        }

                        Assert.IsTrue(expected, $"seed {seed}: {(Area)area} ({x},{y}) drawn without a cell");
                        Assert.AreEqual(Attributes, word & 0x3C00,
                            $"seed {seed}: {(Area)area} ({x},{y}) must use priority + palette 3");
                        minX = Math.Min(minX, x); maxX = Math.Max(maxX, x);
                        minY = Math.Min(minY, y); maxY = Math.Max(maxY, y);
                    }
                }

                CollectionAssert.AreEqual(
                    new[] { (byte)minX, (byte)maxX, (byte)minY, (byte)maxY },
                    payload.Bounds[(area * 4)..(area * 4 + 4)],
                    $"seed {seed}: {(Area)area} bounds must enclose exactly the populated cells");
            }
        }
    }

    [TestMethod]
    public void Compose_GlyphGroupsMatchCellRoles()
    {
        for (int seed = 1; seed <= 10; seed++)
        {
            var world = Generate(seed);
            var payload = AutomapComposer.Compose(world);
            var grid = world.Grid;

            var elevatorCells = grid.Links.Where(l => l.Type == LinkType.Elevator)
                .SelectMany(l => new[] { l.A, l.B, l.B.Step(Direction.Down) })
                .Append(world.Landmarks["EscapeShaft"])
                .ToHashSet();

            foreach (var cell in grid.Cells.Where(c => c.Role != CellRole.Cap))
            {
                ushort word = WordAt(payload, cell.Area, cell.Position);
                string expectedGroup = elevatorCells.Contains(cell.Position) ? "elevator"
                    : cell.Role switch
                    {
                        CellRole.Item => "item",
                        CellRole.Boss => "boss",
                        CellRole.Portal => "portal",
                        CellRole.MapStation => "mapstation",
                        _ when Directions.All.Any(d => cell.Edge(d) == EdgeRequirement.Door) => "door",
                        _ => "topology",
                    };
                Assert.IsTrue(InGroup(word, expectedGroup),
                    $"seed {seed}: {cell} drew char {word & CharacterMask:X3}, expected group '{expectedGroup}'");
            }
        }
    }

    [TestMethod]
    public void Compose_ElevatorColumnsFollowTheVanillaPattern()
    {
        var world = Generate(1);
        var payload = AutomapComposer.Compose(world);
        var grid = world.Grid;
        int elevatorBase = TileGroups.Value["elevator"].Base;

        var links = grid.Links.Where(l => l.Type == LinkType.Elevator).ToList();
        Assert.AreEqual(4, links.Count, "expected the four area elevators");

        foreach (var link in links)
        {
            var top = grid.Cell(link.A)!;
            var lower = grid.Cell(link.B)!.Area;
            var platform = link.B.Step(Direction.Down);

            // Upper plane: entrance room (H-flipped when its door faces east), the shaft
            // segment under it, then a text marker naming the destination area.
            ushort entrance = WordAt(payload, top.Area, link.A);
            Assert.AreEqual(elevatorBase, entrance & CharacterMask, $"entrance at {link.A}");
            Assert.AreEqual(top.Right == EdgeRequirement.Door, (entrance & 0x4000) != 0,
                $"entrance flip at {link.A} must match its door side");
            Assert.AreEqual(elevatorBase + 2, WordAt(payload, top.Area, link.B) & CharacterMask,
                $"shaft under the entrance at {link.B}");
            Assert.AreEqual((ushort)(0x10 + (lower.ToString()[0] - 'A') | Attributes),
                WordAt(payload, top.Area, platform),
                $"the {top.Area} plane must name the {lower} destination");

            // Lower plane: closed-top shaft piece at the upper room's coordinate (Samus
            // occupies it mid-ride after the area switches), shafts at spacer + platform.
            Assert.AreEqual(elevatorBase + 1, WordAt(payload, lower, link.A) & CharacterMask,
                $"shaft top at {link.A} in the {lower} plane");
            Assert.AreEqual(elevatorBase + 2, WordAt(payload, lower, link.B) & CharacterMask,
                $"shaft at the spacer {link.B}");
            Assert.AreEqual(elevatorBase + 2, WordAt(payload, lower, platform) & CharacterMask,
                $"shaft at the platform {platform}");
        }

        Assert.AreEqual(elevatorBase + 1,
            WordAt(payload, Area.Tourian, world.Landmarks["EscapeShaft"]) & CharacterMask,
            "the escape shaft top renders as the closed-top shaft piece, like vanilla");
    }

    [TestMethod]
    public void Emit_AutomapPatchesTargetTheM1MPBlock()
    {
        var world = Generate(3);
        ScreenFitter.Fit(world.Grid, Catalog.Value, 3);

        var reader = new YamlReader(new Config());
        reader.LoadData();
        var emission = RomEmitter.Emit(world, reader.Data!.rooms);

        // Fixed block layout from map_data.asm: header at $989000 (PC 0x6C1000) with
        // seed id at +$0C and bounds at +$10, planes page-aligned at $989100.
        Assert.AreEqual(0x6C100C, AutomapComposer.SeedIdAddress);
        Assert.AreEqual(0x6C1010, AutomapComposer.BoundsAddress);
        Assert.AreEqual(0x6C1100, AutomapComposer.TilemapsAddress);

        Assert.AreEqual(4, emission.Patches[AutomapComposer.SeedIdAddress].Length);
        Assert.AreEqual(20, emission.Patches[AutomapComposer.BoundsAddress].Length);
        Assert.AreEqual(5 * 0x800, emission.Patches[AutomapComposer.TilemapsAddress].Length);

        // The item-table blob must stay below the automap block it shares the window with.
        Assert.IsTrue(emission.TableSize <= RomEmitter.TableLimit, "blob under the header");

        var payload = AutomapComposer.Compose(world);
        CollectionAssert.AreEqual(payload.Planes, emission.Patches[AutomapComposer.TilemapsAddress],
            "emitted planes must be the composed payload");
        CollectionAssert.AreEqual(payload.Bounds, emission.Patches[AutomapComposer.BoundsAddress],
            "emitted bounds must be the composed payload");
    }

    [TestMethod]
    public void MapShuffle_WrittenRomContainsTheAutomapPayload()
    {
        // Full pipeline: the payload composed at generation time must reach the ROM
        // through the normal patch-write path.
        var randomizer = new GameRandomizer(
            [new WorldConfig { Metroid = new Config { MapShuffle = true } }], new PRNG(99));
        randomizer.Randomize();

        var world = (Randomizer.Games.Metroid.World)randomizer.Worlds[0];
        var rom = new LoggedRom();
        RomWriter.Write(rom, world, new PRNG(99));

        var planes = world.PatchData![AutomapComposer.TilemapsAddress];
        CollectionAssert.AreEqual(planes, rom.Read(AutomapComposer.TilemapsAddress, planes.Length),
            "tilemap planes on the ROM");
        CollectionAssert.AreEqual(world.PatchData[AutomapComposer.SeedIdAddress],
            rom.Read(AutomapComposer.SeedIdAddress, 4), "seed id on the ROM");
        CollectionAssert.AreEqual(world.PatchData[AutomapComposer.BoundsAddress],
            rom.Read(AutomapComposer.BoundsAddress, 20), "bounds on the ROM");
    }
}
