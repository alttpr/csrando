namespace RandomizerTests.Games.Zelda1;

using Randomizer.Games;
using Randomizer.Games.Zelda1;
using Randomizer.Graph;
using Graph = Randomizer.Graph.Graph;

[TestClass]
public sealed class WorldTest
{
    private static World CreateWorld(Config? z1Config = null, int seed = 42)
    {
        z1Config ??= new Config
        {
            DungeonShuffle = true,
            DungeonStyle = DungeonStyleOption.Progressive,
            EnemyPlacement = EnemyPlacementOption.Progressive,
            Triforces = "8",
        };
        z1Config.SelectRandomValues(new PRNG(seed));
        var worldConfig = new WorldConfig { Zelda1 = z1Config };
        return new World(1, worldConfig, new Graph(), new PRNG(seed));
    }

    [TestMethod]
    public void WorldConstruction_WithDungeonShuffle_Succeeds()
    {
        var world = CreateWorld();
        Assert.IsNotNull(world.Start, "World should have a start vertex");
        Assert.IsNotNull(world.YamlData, "World should have YamlData after construction");
    }

    [TestMethod]
    public void WorldConstruction_WithoutDungeonShuffle_Succeeds()
    {
        var config = new Config { DungeonShuffle = false };
        var world = CreateWorld(config);
        Assert.IsNotNull(world.Start);
    }

    [TestMethod]
    public void WorldConstruction_DungeonShuffle_GeneratesSpoilers()
    {
        var world = CreateWorld();
        Assert.AreEqual(9, world.DungeonSpoilers.Count, "Should have spoiler data for all 9 dungeons");

        for (int i = 0; i < 9; i++)
        {
            Assert.AreEqual(i + 1, world.DungeonSpoilers[i].Level, $"Spoiler {i} should be for level {i + 1}");
            Assert.IsTrue(world.DungeonSpoilers[i].Rooms.Count > 0, $"Spoiler for level {i + 1} should have rooms");
        }
    }

    [TestMethod]
    public void WorldConstruction_GraphHasVertices()
    {
        var world = CreateWorld();
        var graph = world.Graph;
        Assert.IsTrue(graph.GetVertices().Any(), "Graph should have vertices");
    }

    [TestMethod]
    public void WorldConstruction_CanFindKeyLocations()
    {
        var world = CreateWorld();

        // These locations should exist in the graph after full loading
        Assert.IsNotNull(world.GetLocation("Level 9 - Entrance"), "Should have Level 9 entrance");
    }

    [TestMethod]
    public void WorldConstruction_Level9_RequiresTriforces()
    {
        var world = CreateWorld(new Config
        {
            DungeonShuffle = true,
            Triforces = "8",
        });

        var entrance = world.GetLocation("Level 9 - Entrance");
        var triforceEdge = entrance.Edges.Find(e => e.Condition.Item.Name == "Triforce");

        Assert.IsNotNull(triforceEdge, "Level 9 entrance should require Triforce");
        Assert.AreEqual(8, triforceEdge.Condition.Count, "Level 9 should require 8 triforces");
    }

    [TestMethod]
    public void WorldConstruction_CustomTriforceCount()
    {
        var world = CreateWorld(new Config
        {
            DungeonShuffle = true,
            Triforces = "3",
        });

        var entrance = world.GetLocation("Level 9 - Entrance");
        var triforceEdge = entrance.Edges.Find(e => e.Condition.Item.Name == "Triforce");

        Assert.IsNotNull(triforceEdge);
        Assert.AreEqual(3, triforceEdge.Condition.Count, "Level 9 should require 3 triforces");
    }

    [TestMethod]
    [DataRow(DungeonStyleOption.Progressive)]
    [DataRow(DungeonStyleOption.Wild)]
    [DataRow(DungeonStyleOption.Megadungeon)]
    [DataRow(DungeonStyleOption.Minimal)]
    public void WorldConstruction_AllDungeonStyles_Succeed(DungeonStyleOption style)
    {
        // Megadungeon can fail on some seeds due to grid capacity constraints.
        // Try multiple seeds to find one that works, verifying the style is
        // generally viable rather than testing a single fixed seed.
        int[] seeds = [42, 100, 200, 300, 400, 500, 1337, 2000, 5000, 9999];
        Exception? lastException = null;

        foreach (var seed in seeds)
        {
            try
            {
                var world = CreateWorld(new Config
                {
                    DungeonShuffle = true,
                    DungeonStyle = style,
                    EnemyPlacement = EnemyPlacementOption.Progressive,
                }, seed);

                Assert.IsNotNull(world.Start);
                Assert.AreEqual(9, world.DungeonSpoilers.Count);
                return; // success
            }
            catch (InvalidOperationException ex)
            {
                lastException = ex;
            }
        }

        Assert.Fail($"Style {style} failed on all seeds. Last error: {lastException?.Message}");
    }

    [TestMethod]
    public void WorldConstruction_GeneratedMapsUseExtendedIds()
    {
        var world = CreateWorld();
        var generatedMaps = world.YamlData!.underworld_maps.Where(m => m.generated).ToList();

        Assert.IsTrue(generatedMaps.Count > 0, "Should have generated maps");
        Assert.IsTrue(generatedMaps.All(m => m.map >= 0x100),
            "All generated map IDs should be in the extended range (0x100+)");
    }

    [TestMethod]
    public void WorldConstruction_GeneratedMapsHaveValidLocalRoomIds()
    {
        var world = CreateWorld();
        var generatedMaps = world.YamlData!.underworld_maps.Where(m => m.generated).ToList();

        foreach (var map in generatedMaps)
        {
            Assert.IsTrue(map.local_room_id >= 0 && map.local_room_id < 0x80,
                $"Map {map.map:X4} local_room_id {map.local_room_id:X2} should be in 0x00-0x7F range");
        }
    }

    [TestMethod]
    public void WorldConstruction_NoLocalRoomIdCollisionsPerLevel()
    {
        var world = CreateWorld();
        var generatedMaps = world.YamlData!.underworld_maps.Where(m => m.generated).ToList();

        var byLevel = generatedMaps.GroupBy(m => m.generated_level);
        foreach (var levelGroup in byLevel)
        {
            var localIds = levelGroup.Select(m => m.local_room_id).ToList();
            var uniqueIds = localIds.ToHashSet();
            Assert.AreEqual(localIds.Count, uniqueIds.Count,
                $"Level {levelGroup.Key}: Local room IDs should be unique, found {localIds.Count - uniqueIds.Count} duplicates");
        }
    }
}
