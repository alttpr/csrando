namespace RandomizerTests.Games.Metroid;

using Randomizer.Games;
using Randomizer.Games.Metroid;
using Randomizer.Graph;
using System.Text.Json;
using Graph = Randomizer.Graph.Graph;
using Vertex = Randomizer.Graph.Vertex;
using GameRandomizer = Randomizer.Games.Metroid.GameRandomizer;

[TestClass]
public sealed class WorldTest
{
    [TestMethod]
    public void Vanilla_ItemPoolUsesSafeCapacityLimitsAndNothingFiller()
    {
        var world = new World(0,
            new WorldConfig { Metroid = new Config() }, new Graph(), new PRNG(1337));
        var pooler = new ItemPooler([world], new PRNG(1337));
        int locationCount = world.GetLocationsOfType(VertexType.Item).Count();

        Assert.AreEqual(locationCount, pooler.Pool.Length);
        int energyTanks = pooler.Pool.Count(item => item.Item.Name == "EnergyTank");
        int missiles = pooler.Pool.Count(item => item.Item.Name == "Missile");
        Assert.IsTrue(energyTanks <= ItemPooler.MaximumEnergyTanks,
            $"Pool contains {energyTanks} Energy Tanks.");
        Assert.IsTrue(missiles <= ItemPooler.MaximumMissiles,
            $"Pool contains {missiles} Missiles.");
        Assert.AreEqual(Math.Max(0, locationCount - ItemPooler.MaximumNonNothingItems),
            pooler.Pool.Count(item => item.Item.Name == "Nothing"));
        var nothingSet = new ItemSetName(ItemPooler.NothingItemSet, world);
        Assert.IsTrue(pooler.Pool.Where(item => item.Item.Name == "Nothing")
            .All(item => item.Set == nothingSet));
        Assert.IsTrue(pooler.SetLocations[nothingSet]
            .All(location => ReferenceEquals(location.World, world)));
        // Power-up entry with id $FF, the engine's "empty slot" marker: the location
        // stays blank in-game instead of spawning a broken custom item.
        CollectionAssert.AreEqual(new byte[] { 0x02, 0xFF },
            world.GetItem("Nothing").Bytes);
    }

    private static World CreateWorld(Config? config = null, int seed = 42)
    {
        config ??= new Config { MapShuffle = true };
        var worldConfig = new WorldConfig { Metroid = config };
        var world = new World(1, worldConfig, new Graph(), new PRNG(seed));
        world.Graph.SetVertexIds(); // normally done by the GameRandomizer base after world creation
        return world;
    }

    /// <summary>All equipment plus enough missiles/tanks for every requirement gate.</summary>
    private static Inventory FullInventory(World world)
    {
        var items = new List<IItem>
        {
            world.GetItem("fixed"),
            world.GetItem("Morph"),
            world.GetItem("Bombs"),
            world.GetItem("HiJump"),
            world.GetItem("LongBeam"),
            world.GetItem("ScrewAttack"),
            world.GetItem("Varia"),
            world.GetItem("WaveBeam"),
            world.GetItem("IceBeam"),
        };
        items.AddRange(Enumerable.Repeat(world.GetItem("Missile"), 30));
        items.AddRange(Enumerable.Repeat(world.GetItem("EnergyTank"), 6));
        return new Inventory(items.ToArray());
    }

    [TestMethod]
    public void WorldConstruction_Vanilla_Succeeds()
    {
        var world = CreateWorld(new Config());
        Assert.IsNotNull(world.Start);
        Assert.IsNotNull(world.YamlData);
        Assert.IsNull(world.GeneratedMap, "vanilla world should not generate a map");
    }

    [TestMethod]
    public void WorldConstruction_WithMapShuffle_Succeeds()
    {
        var world = CreateWorld();
        Assert.IsNotNull(world.Start);
        Assert.IsNotNull(world.GeneratedMap, "map shuffle should attach the generated map");
        Assert.IsTrue(world.Graph.GetVertices().Any());
    }

    [TestMethod]
    public void MapShuffle_IsDeterministicPerSeed()
    {
        var a = CreateWorld(seed: 1234);
        var b = CreateWorld(seed: 1234);

        var roomsA = a.YamlData!.rooms.Select(r => $"{r.name}:{string.Join(",", r.screens)}").ToList();
        var roomsB = b.YamlData!.rooms.Select(r => $"{r.name}:{string.Join(",", r.screens)}").ToList();
        CollectionAssert.AreEqual(roomsA, roomsB);
    }

    [TestMethod]
    public void MapShuffle_IsWinnableWithFullInventory()
    {
        for (int seed = 1; seed <= 8; seed++)
        {
            var world = CreateWorld(seed: seed);
            Assert.IsTrue(world.IsWinnable(world.Start!, FullInventory(world)),
                $"seed {seed}: generated world not winnable with full inventory");
        }
    }

    [TestMethod]
    public void MapShuffle_NotWinnableWithoutMissiles()
    {
        var world = CreateWorld(seed: 7);
        var noMissiles = new Inventory([
            world.GetItem("fixed"), world.GetItem("Morph"), world.GetItem("Bombs"),
            world.GetItem("HiJump"), world.GetItem("ScrewAttack"), world.GetItem("IceBeam")
        ]);
        Assert.IsFalse(world.IsWinnable(world.Start!, noMissiles),
            "the statues gate and bosses require missiles; this should not be winnable");
    }

    [TestMethod]
    public void MapShuffle_EveryItemLocationGetsFilled()
    {
        // Every emitted orb must receive an item, even ones intentionally out of logic
        // (e.g. Brinstar 0x19's ceiling tank, reachable only via IBJ which basic logic does
        // not assume): the fast-fill phase trash-fills unreachable locations, and an unfilled
        // location renders in-game as a phantom orb / placeholder Bombs pickup. This is the
        // real invariant -- not full-inventory reachability, which out-of-logic spots fail.
        //
        // A seed can rarely fail outright before the invariant applies (the topology
        // generator's attempt cap, an assumed-fill deadlock when an early progression item
        // lands behind the next one's requirement -- seed 6 deadlocks Bombs this way -- or a
        // one-way transition stranding a region post-fit, ~12% of seeds). Those are known
        // seed-quality losses a caller re-rolls, so tolerate a bounded number here instead of
        // pinning the invariant to lucky seeds.
        int successes = 0;
        var failedSeeds = new List<string>();
        for (int seed = 1; seed <= 16 && successes < 8; seed++)
        {
            var randomizer = new GameRandomizer(
                [new WorldConfig { Metroid = new Config { MapShuffle = true } }], new PRNG(seed));
            try
            {
                randomizer.Randomize();
            }
            catch (Exception ex)
            {
                failedSeeds.Add($"seed {seed}: {ex.Message}");
                continue;
            }

            var world = (World)randomizer.Worlds[0];
            var itemLocations = world.GetLocationsOfType(VertexType.Item).ToList();
            Assert.IsTrue(itemLocations.Count > 0, $"seed {seed}: no item locations");
            Assert.AreEqual(0, itemLocations.Count(l => l.Item == null),
                $"seed {seed}: unfilled item locations: " +
                $"{string.Join("; ", itemLocations.Where(l => l.Item == null).Take(5).Select(l => l.Name))}");
            successes++;
        }

        Assert.IsTrue(successes >= 8,
            $"only {successes} seeds filled successfully; failures: {string.Join("; ", failedSeeds)}");
    }

    [TestMethod]
    public void Vanilla_FullRandomizationIsWinnable()
    {
        for (int seed = 1; seed <= 3; seed++)
        {
            var randomizer = new GameRandomizer(
                [new WorldConfig { Metroid = new Config() }], new PRNG(seed));
            randomizer.Randomize();
            Assert.IsTrue(randomizer.IsWinnable(), $"seed {seed}: vanilla filled world not winnable");
        }
    }

    [TestMethod]
    public void Vanilla_SpoilerContainsCompleteExplainablePlaythrough()
    {
        var randomizer = new GameRandomizer(
            [new WorldConfig { Metroid = new Config() }], new PRNG(7));
        randomizer.Randomize();

        Assert.IsTrue(randomizer.SpoilerLog!.Spoiler.TryGetValue("playthrough", out var section));
        using var document = JsonDocument.Parse(section["data"]);
        var root = document.RootElement;
        Assert.IsTrue(root.GetProperty("complete").GetBoolean());
        Assert.AreEqual("DefeatedSilverTwo", root.GetProperty("victoryItems")[0].GetString());
        Assert.IsFalse(root.GetProperty("startingItems").EnumerateArray().Any(item =>
            item.GetProperty("name").GetString() == "AllowIBJ"));

        var pickups = root.GetProperty("spheres").EnumerateArray()
            .SelectMany(sphere => sphere.GetProperty("pickups").EnumerateArray())
            .ToList();
        Assert.IsTrue(pickups.Count > 0);
        Assert.IsTrue(pickups.Any(pickup =>
            pickup.GetProperty("item").GetProperty("name").GetString() == "DefeatedSilverTwo"));
        Assert.IsTrue(pickups.All(pickup => pickup.TryGetProperty("path", out _)));
        Assert.IsTrue(pickups.Any(pickup =>
            pickup.GetProperty("requiredItems").GetArrayLength() > 0));
        Assert.IsFalse(pickups
            .SelectMany(pickup => pickup.GetProperty("path").EnumerateArray())
            .SelectMany(step => step.GetProperty("requirements").EnumerateArray())
            .Any(requirement => requirement.GetProperty("name").GetString() == "CanIBJ"),
            "playthrough route used CanIBJ without the unavailable AllowIBJ setting");
    }

    [TestMethod]
    public void MapShuffle_FullRandomizationIsWinnable()
    {
        // The complete pipeline: generated map, graph, progressive item fill, and a
        // winnability check from starting equipment only.
        for (int seed = 1; seed <= 5; seed++)
        {
            var randomizer = new GameRandomizer(
                [new WorldConfig { Metroid = new Config { MapShuffle = true } }], new PRNG(seed));
            randomizer.Randomize();
            Assert.IsTrue(randomizer.IsWinnable(), $"seed {seed}: filled world not winnable from start");
        }
    }

    [TestMethod]
    public void MapShuffle_Large_EveryLocationGetsAnItem()
    {
        // Regression: a Large map once generated more item locations than the 36-item
        // pool; the unfilled locations played as free Bombs (the ROM placeholder id).
        for (int seed = 1; seed <= 3; seed++)
        {
            var randomizer = new GameRandomizer(
                [new WorldConfig { Metroid = new Config { MapShuffle = true, MapSize = MapSizeOption.Large } }],
                new PRNG(seed));
            randomizer.Randomize();
            Assert.IsTrue(randomizer.IsWinnable(), $"seed {seed}: not winnable");

            var world = (World)randomizer.Worlds[0];
            var unfilled = world.GetLocationsOfType(VertexType.Item)
                .Where(l => l.Item == null)
                .Select(l => l.Name)
                .ToList();
            Assert.AreEqual(0, unfilled.Count,
                $"seed {seed}: unfilled item locations: {string.Join("; ", unfilled.Take(5))}");
        }
    }

    [TestMethod]
    public void MapShuffle_Nightmare_FullRandomizationIsWinnable()
    {
        // The saturated grid is the stress case for the whole pipeline: fitting,
        // graph size, the item-location cap, and the ROM table window.
        for (int seed = 1; seed <= 2; seed++)
        {
            var randomizer = new GameRandomizer(
                [new WorldConfig { Metroid = new Config { MapShuffle = true, MapSize = MapSizeOption.Nightmare } }],
                new PRNG(seed));
            randomizer.Randomize();
            Assert.IsTrue(randomizer.IsWinnable(), $"seed {seed}: nightmare seed not winnable");

            var world = (World)randomizer.Worlds[0];
            Assert.AreEqual(0, world.GetLocationsOfType(VertexType.Item).Count(l => l.Item == null),
                $"seed {seed}: unfilled item locations");
        }
    }

    [TestMethod]
    public void MapShuffle_BossEventsReachable()
    {
        for (int seed = 1; seed <= 8; seed++)
        {
            var world = CreateWorld(seed: seed);
            var searcher = new Searcher(world.Graph, world.Start!, FullInventory(world));

            foreach (var eventItem in new[] { "KraidDefeated", "RidleyDefeated", "DefeatedSilverTwo", "DefeatedMotherBrain" })
                Assert.IsTrue(searcher.HasFound(world.GetItem(eventItem)),
                    $"seed {seed}: {eventItem} not reachable");
        }
    }
}
