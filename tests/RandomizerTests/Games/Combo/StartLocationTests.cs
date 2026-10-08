namespace RandomizerTests.Games.Combo;

using System.Buffers.Binary;
using Randomizer.ApiControllers;
using Randomizer.Games;
using Randomizer.Graph;
using Randomizer.RomModifications;
using ComboConfig = Randomizer.Games.Combo.Config;
using ComboWorld = Randomizer.Games.Combo.World;
using Graph = Randomizer.Graph.Graph;
using M1Area = Randomizer.Games.Metroid.YamlReader.Area;
using M1Config = Randomizer.Games.Metroid.Config;
using SMConfig = Randomizer.Games.SuperMetroid.Config;
using SMRom = Randomizer.Games.SuperMetroid.Rom;
using Z3Config = Randomizer.Games.Alttp.Config;

// Combo world construction builds every present game's world and is by far the heaviest
// setup in the suite; keep these out of the parallel schedule (see PortalTests).
[TestClass]
[DoNotParallelize]
public sealed class StartLocationTests
{
    private sealed class MemoryRom : IRom
    {
        private readonly byte[] _data = new byte[8 * 1024 * 1024];

        public byte[] Read(Address address, int length) => _data[address.Value..(address.Value + length)];
        public void Write(Address address, in ReadOnlySpan<byte> data) => data.CopyTo(_data.AsSpan(address.Value));
        public void ApplyBasePatch(FileInfo baseBPS) => throw new NotSupportedException();
        public void Resize(int size) { }
        public void UpdateChecksum() { }
        public void Dispose() { }

        public void Write(Address address, byte value)
        {
            _data[address.Value] = value;
        }

        public void WriteUInt16(Address address, in ReadOnlySpan<ushort> data)
        {
            Span<byte> tmp = _data.AsSpan(address.Value, data.Length * sizeof(ushort));

            if (BitConverter.IsLittleEndian)
            {
                System.Runtime.InteropServices.MemoryMarshal.AsBytes(data).CopyTo(tmp);
            }
            else
            {
                foreach (ushort value in data)
                {
                    BinaryPrimitives.WriteUInt16LittleEndian(tmp, value);
                    tmp = tmp[2..];
                }
            }
        }

    }

    private static WorldConfig CreateConfig(
        bool alttp = false, SMConfig? sm = null, M1Config? m1 = null) => new()
    {
        Game = RandomizerTarget.Combo,
        Combo = new ComboConfig(),
        Alttp = alttp ? new Z3Config() : null,
        SuperMetroid = sm,
        Metroid = m1,
    };

    private static ComboWorld CreateWorld(WorldConfig config, int seed = 42) =>
        new(0, config, new Graph(), new PRNG(seed));

    private static byte[] Words(params int[] words) =>
        words.SelectMany(w => new[] { (byte)(w & 0xFF), (byte)(w >> 8 & 0xFF) }).ToArray();

    [TestMethod]
    public void Sm_StartLocationValues_MatchDataCatalog()
    {
        var data = new Randomizer.Games.SuperMetroid.Model.JsonReader(new SMConfig());
        data.Load();

        var catalog = Randomizer.Games.SuperMetroid.SaveStations.EligibleStartStations(data);

        CollectionAssert.AreEqual(
            SMConfig.StartLocationValues.Skip(1).ToList(),
            catalog.Select(s => s.RoomName).ToList(),
            "Config.StartLocationValues (after Ship) must list the data-derived stations in (area, slot) order");
        Assert.IsTrue(catalog.All(s => s.Area != 5), "no Tourian stations");
        Assert.IsTrue(catalog.All(s => s.VertexName.Contains(s.RoomName)));
    }

    [TestMethod]
    public void StartValueAttributes_MatchTheStaticLists()
    {
        // Attribute arguments must be literals, so the [Values] lists repeat the
        // names; this pins them to the arrays the code actually validates against.
        static object[] AttributeValues(Type config, string property) =>
            config.GetProperty(property)!
                .GetCustomAttributes(typeof(Randomizer.Games.Metadata.ValuesAttribute), false)
                .Cast<Randomizer.Games.Metadata.ValuesAttribute>().Single().Values;

        CollectionAssert.AreEqual(
            new[] { SMConfig.VanillaStartLocation, SMConfig.RandomStartLocation }
                .Concat(SMConfig.StartLocationValues.Skip(1)).ToList(),
            AttributeValues(typeof(SMConfig), nameof(SMConfig.StartLocation)));

        CollectionAssert.AreEqual(
            new[] { M1Config.VanillaStartArea, M1Config.RandomStartArea }
                .Concat(M1Config.StartAreaValues.Skip(1)).ToList(),
            AttributeValues(typeof(M1Config), nameof(M1Config.StartArea)));
    }

    /// <summary>Points JsonReader at the source data root so the (gitignored) map
    /// corpus is available, or inconclusive when this checkout lacks it.</summary>
    private static IDisposable UseMapCorpusOrInconclusive()
    {
        string sourceDataRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "../../../../../src/Randomizer/Games/SuperMetroid/data"));
        string mapCorpus = Path.Combine(sourceDataRoot, "maps");
        if (!Directory.Exists(mapCorpus)
            || !Directory.EnumerateFiles(mapCorpus, "*.avro", SearchOption.AllDirectories).Any())
        {
            Assert.Inconclusive("Requires the local SM map-rando Avro corpus.");
        }

        string oldDataRoot = Randomizer.Games.SuperMetroid.Model.JsonReader.DataRoot;
        Randomizer.Games.SuperMetroid.Model.JsonReader.DataRoot = sourceDataRoot;
        return new DataRootRestore(oldDataRoot);
    }

    private sealed class DataRootRestore(string oldDataRoot) : IDisposable
    {
        public void Dispose() => Randomizer.Games.SuperMetroid.Model.JsonReader.DataRoot = oldDataRoot;
    }

    private static SMConfig MapRandoSmConfig(string startLocation) => new()
    {
        StartLocation = startLocation,
        MapRandomizer = Randomizer.Games.SuperMetroid.MapRandomizerSetting.Standard,
    };

    /// <summary>Builds a combo world with an SM random start, retrying seeds when a
    /// map has no viable station (that legitimately throws and re-rolls in prod).</summary>
    private static ComboWorld CreateRandomStartWorld(int firstSeed, out int usedSeed)
    {
        for (int seed = firstSeed; seed < firstSeed + 20; seed++)
        {
            try
            {
                var world = CreateWorld(CreateConfig(
                    sm: MapRandoSmConfig(SMConfig.RandomStartLocation)), seed);
                usedSeed = seed;
                return world;
            }
            catch (InvalidOperationException)
            {
                // No viable station on this map; next seed.
            }
        }
        throw new AssertFailedException("No map with a viable start station in 20 seeds");
    }

    [TestMethod]
    public void Sm_RandomStation_MovesStartAndWritesSram()
    {
        using var _ = UseMapCorpusOrInconclusive();
        var world = CreateRandomStartWorld(1, out int seed);
        var sm = world.SMWorld!;

        Assert.AreEqual("sm", world.EffectiveInitialGame);
        Assert.IsNotNull(sm.StartStation);
        Assert.AreEqual(sm.StartStation.VertexName, sm.Start.Name);

        // Determinism: the same seed resolves the same station.
        var again = CreateWorld(CreateConfig(sm: MapRandoSmConfig(SMConfig.RandomStartLocation)), seed);
        Assert.AreEqual(sm.StartStation.RoomName, again.SMWorld!.StartStation?.RoomName);

        var rom = new MemoryRom();
        new SMRom(rom, 0).WriteStartingLocation(sm);

        // The initial SRAM template ($F9:9000, PC 0x799000) holds the boot load
        // station: slot at +0x166, area at +0x168, map area at +0x96E.
        const int initialSram = 0x799000;
        int mapRoomIndex = sm.Map!.room_id.FindIndex(id => id == sm.StartStation.RoomId);
        Assert.IsTrue(mapRoomIndex >= 0);
        CollectionAssert.AreEqual(Words(sm.StartStation.Slot), rom.Read(initialSram + 0x166, 2), "station slot");
        CollectionAssert.AreEqual(Words(sm.StartStation.Area), rom.Read(initialSram + 0x168, 2), "area");
        CollectionAssert.AreEqual(Words(sm.Map.room_area[mapRoomIndex]), rom.Read(initialSram + 0x96E, 2), "map area");
    }

    [TestMethod]
    public void Sm_StationWithoutMapRandomizer_IsIgnored()
    {
        // On the vanilla layout no station satisfies the filler's itemless-pocket
        // invariants, so the setting only takes effect with the map randomizer.
        var world = CreateWorld(CreateConfig(
            alttp: true, sm: new SMConfig { StartLocation = "Bubble Mountain Save Room" }));

        Assert.AreEqual("alttp", world.EffectiveInitialGame);
        Assert.IsNull(world.SMWorld!.StartStation);
        Assert.AreEqual("Crateria - Landing Site - Bottom Left Door", world.SMWorld.Start.Name);
    }

    [TestMethod]
    public void M1_ExplicitArea_MovesStartAndConfigByte()
    {
        var world = CreateWorld(CreateConfig(m1: new M1Config { StartArea = "Kraid" }));
        var m1 = world.M1World!;

        Assert.AreEqual("m1", world.EffectiveInitialGame);
        Assert.AreEqual(M1Area.Kraid, m1.StartingArea);
        Assert.IsTrue(m1.Start.Edges.Any(e =>
            e.To.Name == "Kraid - Elevator Shaft - Elevator Entrance (1) - Elevator Platform"),
            "the start meta vertex must lead to the Kraid elevator arrival");
        CollectionAssert.AreEqual(new byte[] { 2 },
            m1.PatchData![Randomizer.Games.Metroid.RomWriter.StartAreaConfigAddress],
            "config_m1_start_area byte (InArea order)");
    }

    [TestMethod]
    public void BothRandom_ExactlyOneGameMovesItsStart()
    {
        using var _ = UseMapCorpusOrInconclusive();
        int smWins = 0, m1Wins = 0;
        for (int seed = 1; seed <= 12; seed++)
        {
            ComboWorld world;
            try
            {
                world = CreateWorld(CreateConfig(
                    sm: MapRandoSmConfig(SMConfig.RandomStartLocation),
                    m1: new M1Config { StartArea = M1Config.RandomStartArea }), seed);
            }
            catch (InvalidOperationException)
            {
                continue; // SM won the tie-break on a map with no viable station.
            }

            CollectionAssert.Contains(new[] { "sm", "m1" }, world.EffectiveInitialGame);
            if (world.EffectiveInitialGame == "sm")
            {
                smWins++;
                Assert.IsNotNull(world.SMWorld!.StartStation);
                Assert.AreEqual(M1Area.Brinstar, world.M1World!.StartingArea,
                    "the game that lost the tie-break keeps its vanilla start");
            }
            else
            {
                m1Wins++;
                Assert.IsNull(world.SMWorld!.StartStation,
                    "the game that lost the tie-break keeps its vanilla start");
                CollectionAssert.AreEqual(new byte[] { (byte)world.M1World!.StartingArea },
                    world.M1World.PatchData![Randomizer.Games.Metroid.RomWriter.StartAreaConfigAddress]);
            }
        }

        Assert.IsTrue(smWins > 0 && m1Wins > 0,
            $"tie-break should hit both games across seeds (sm {smWins}, m1 {m1Wins})");
    }

    [TestMethod]
    public void NoMovedStart_KeepsLegacyDefaults()
    {
        var world = CreateWorld(CreateConfig(alttp: true, sm: new SMConfig(), m1: new M1Config()));

        Assert.AreEqual("alttp", world.EffectiveInitialGame);
        Assert.IsNull(world.SMWorld!.StartStation);
        Assert.AreEqual("Crateria - Landing Site - Bottom Left Door", world.SMWorld.Start.Name);
        Assert.AreEqual(M1Area.Brinstar, world.M1World!.StartingArea);
        CollectionAssert.AreEqual(new byte[] { 0 },
            world.M1World.PatchData![Randomizer.Games.Metroid.RomWriter.StartAreaConfigAddress]);
    }

    [TestMethod]
    public void M1_MapShuffle_GeneratesTheStartInTheChosenArea()
    {
        var world = CreateWorld(CreateConfig(
            m1: new M1Config { MapShuffle = true, StartArea = "Kraid" }));
        var m1 = world.M1World!;

        Assert.AreEqual("m1", world.EffectiveInitialGame);
        Assert.AreEqual(M1Area.Kraid, m1.StartingArea);

        var generated = m1.GeneratedMap!;
        var startCell = generated.Grid.Cell(generated.Start)!;
        Assert.AreEqual(M1Area.Kraid, startCell.Area, "the generated start cell must be in the chosen area");

        // Two start variants exist: a corridor pair with a forced zero-equipment
        // pedestal, or the area's elevator arrival platform (vanilla style).
        if (startCell.Role == Randomizer.Games.Metroid.MapGen.CellRole.Start)
        {
            var pedestal = generated.Grid.Cell(generated.Landmarks["StartPedestal"])!;
            Assert.AreEqual(M1Area.Kraid, pedestal.Area, "the pedestal sits next to the start");
        }
        else
        {
            Assert.IsTrue(generated.Grid.Links.Any(l =>
                l.Type == Randomizer.Games.Metroid.MapGen.LinkType.Elevator
                && l.B.Step(Randomizer.Games.Metroid.YamlReader.Direction.Down) == generated.Start),
                "a non-corridor start must be an elevator arrival platform");
        }

        // Boot config: area byte in InArea order, per-area respawn orientation table,
        // and the boot area's $95D7 cell = the generated start (not a portal override).
        CollectionAssert.AreEqual(new byte[] { 2 },
            m1.PatchData![Randomizer.Games.Metroid.RomWriter.StartAreaConfigAddress]);
        byte[] orientation = m1.PatchData[Randomizer.Games.Metroid.RomWriter.StartAreaConfigAddress + 1];
        Assert.HasCount(5, orientation);
        Assert.AreEqual(startCell.Run.Axis == Randomizer.Games.Metroid.YamlReader.Scrolling.Vertical ? 1 : 0,
            orientation[(int)M1Area.Kraid], "orientation must match the start room's axis");
        const int kraidStartPosition = 0x680000 + 4 * 0x8000 + (0x95D7 - 0x8000);
        CollectionAssert.AreEqual(new byte[] { (byte)generated.Start.X, (byte)generated.Start.Y },
            m1.PatchData[kraidStartPosition]);
    }

    /// <summary>
    /// A Ridley start's itemless pocket is a single item location that Morph does not
    /// open — the way out of lower Norfair needs High Jump or Ice Beam. The front fill
    /// must therefore place the opener there and let Morph follow, rather than burning
    /// the only slot on a Morph that unlocks nothing.
    /// </summary>
    [TestMethod]
    public void M1_RidleyStart_FrontFillsTheOpenerBeforeMorph()
    {
        var world = CreateWorld(CreateConfig(m1: new M1Config { StartArea = "Ridley" }));
        var m1 = world.M1World!;
        Assert.AreEqual(M1Area.Ridley, m1.StartingArea);

        var pool = new[] { "Morph", "Bombs", "HiJump", "IceBeam", "Varia", "Missile" }
            .Select(m1.GetItem).ToList();
        var opener = m1.FindStartOpener(pool);

        Assert.IsNotNull(opener, "a Ridley start must be openable");
        CollectionAssert.Contains(new[] { "HiJump", "IceBeam" }, opener.Name,
            $"expected a mobility opener, got {opener.Name}");
    }

    [TestMethod]
    [DataRow("Brinstar")]
    [DataRow("Norfair")]
    [DataRow("Kraid")]
    public void M1_OrdinaryStarts_StillFrontFillMorph(string area)
    {
        // Morph opens these pockets, so it stays the front-filled item and the fill
        // order of every seed that already generated is unchanged.
        var world = CreateWorld(CreateConfig(m1: new M1Config { StartArea = area }));
        var m1 = world.M1World!;

        var pool = new[] { "Morph", "Bombs", "HiJump", "IceBeam", "Varia", "Missile" }
            .Select(m1.GetItem).ToList();

        Assert.AreEqual("Morph", m1.FindStartOpener(pool)?.Name);
    }

    /// <summary>
    /// Tourian is the one area no map can rescue: the Silver Two statue bridge is gated
    /// in both directions and the Tourian elevator lands behind it, so the start reaches
    /// zero item locations even holding every item in the game.
    /// </summary>
    [TestMethod]
    public void M1_TourianStart_IsRejectedAsUnopenable()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            CreateWorld(CreateConfig(m1: new M1Config { StartArea = "Tourian" })));

        StringAssert.Contains(ex.Message, "Tourian");
    }

    [TestMethod]
    public void Validate_RejectsUnknownNamesAndAlttpCombos()
    {
        Assert.IsNotNull(WorldConfigValidator.Validate(
            [CreateConfig(sm: new SMConfig { StartLocation = "Not A Room" })]));
        Assert.IsNotNull(WorldConfigValidator.Validate(
            [CreateConfig(m1: new M1Config { StartArea = "Zebes" })]));
        Assert.IsNotNull(WorldConfigValidator.Validate(
            [CreateConfig(m1: new M1Config { StartArea = "Tourian" })]),
            "Tourian is boot-supported but logic-impossible, so it is not a valid option");
        Assert.IsNull(WorldConfigValidator.Validate(
            [CreateConfig(
                sm: MapRandoSmConfig("Draygon Save Room"),
                m1: new M1Config { StartArea = M1Config.RandomStartArea })]));
        Assert.IsNull(WorldConfigValidator.Validate([CreateConfig(alttp: true)]));

        // Non-ALttP initial games cannot yet fill ALttP's set items, so moved starts
        // are rejected on seeds containing ALttP (the settings' OnlyWithGames gate).
        Assert.IsNotNull(WorldConfigValidator.Validate(
            [CreateConfig(alttp: true, m1: new M1Config { StartArea = "Norfair" })]));
        Assert.IsNotNull(WorldConfigValidator.Validate(
            [CreateConfig(alttp: true, sm: MapRandoSmConfig(SMConfig.RandomStartLocation))]));
        // Without the map randomizer the SM setting is inert (DependsOn), so ALttP
        // stays allowed.
        Assert.IsNull(WorldConfigValidator.Validate(
            [CreateConfig(alttp: true, sm: new SMConfig { StartLocation = "Draygon Save Room" })]));
    }

    [TestMethod]
    [TestCategory(TestCategories.Slow)]
    public void Sm_RandomStationStart_GeneratesWinnableSeed()
    {
        // Random station starts succeed on roughly one seed in five to eight (viability
        // re-rolls plus fill deadlocks); the production retry budget absorbs that.
        using var _ = UseMapCorpusOrInconclusive();
        AssertSomeSeedIsWinnable(seed => RandomizerFactory.Create(
            [CreateConfig(
                sm: MapRandoSmConfig(SMConfig.RandomStartLocation),
                m1: new M1Config())], seed),
            attempts: 24);
    }

    [TestMethod]
    [TestCategory(TestCategories.Slow)]
    [DataRow("Norfair")]
    [DataRow("Kraid")]
    [DataRow("Ridley")]
    public void M1_AreaStart_GeneratesWinnableSeed(string area)
    {
        AssertSomeSeedIsWinnable(seed => RandomizerFactory.Create(
            [CreateConfig(sm: new SMConfig(), m1: new M1Config { StartArea = area })], seed));
    }

    [TestMethod]
    [TestCategory(TestCategories.Slow)]
    [DataRow("Ridley")]
    [DataRow("Random")]
    public void M1_MapShuffleAreaStart_GeneratesWinnableSeed(string area)
    {
        AssertSomeSeedIsWinnable(seed => RandomizerFactory.Create(
            [CreateConfig(sm: new SMConfig(),
                m1: new M1Config { MapShuffle = true, StartArea = area })], seed));
    }

    /// <summary>
    /// Moved starts constrain the fill, so a fraction of seeds legitimately fail —
    /// mirror the production retry loop instead of pinning one lucky seed.
    /// </summary>
    private static void AssertSomeSeedIsWinnable(Func<int, GameRandomizer> create, int attempts = 5)
    {
        for (int seed = 1; seed <= attempts; seed++)
        {
            try
            {
                var randomizer = create(seed);
                randomizer.Randomize();
                if (randomizer.IsWinnable())
                    return;
            }
            catch (Exception)
            {
                // An unfillable seed throws from the filler; try the next seed like
                // the production retry loop does.
            }
        }

        Assert.Fail($"No winnable seed among {attempts} attempts");
    }
}
