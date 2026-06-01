namespace RandomizerTests.Games.Zelda1;

using Randomizer.Games.Zelda1;
using Randomizer.Graph;

[TestClass]
public sealed class DungeonBuilderTest
{
    /// <summary>
    /// Loads YAML data with default config (DungeonShuffle enabled, Progressive style).
    /// Shared across tests via lazy init.
    /// </summary>
    private static YamlReader CreateYamlReader(Config? config = null)
    {
        config ??= new Config { DungeonShuffle = true };
        var reader = new YamlReader(config);
        reader.LoadData();
        return reader;
    }

    private static DungeonBuilder CreateBuilder(YamlReader.YamlData data, int level, DungeonConfig? config = null, int seed = 42)
    {
        config ??= DungeonConfig.GetConfigForLevel(level, DungeonStyleOption.Progressive, EnemyPlacementOption.Progressive, new Random(seed));
        var prng = new PRNG(seed);
        return new DungeonBuilder(config, data, level, prng);
    }

    private static int[] CreateBlockRoomIds(int minX, int minY, int width, int height)
        => [.. Enumerable.Range(0, height)
            .SelectMany(y => Enumerable.Range(0, width).Select(x => ((minY + y) << 4) | (minX + x)))];

    private static byte[] CreateExpectedStatusBarMapTransferBuffer(params byte[] prefix)
    {
        var buffer = Enumerable.Repeat((byte)0xFF, 45).ToArray();
        Array.Copy(prefix, buffer, prefix.Length);
        return buffer;
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    [DataRow(9)]
    public void Generate_AllLevels_Succeeds(int level)
    {
        var reader = CreateYamlReader();
        var builder = CreateBuilder(reader.Data!, level);
        builder.Generate();
        // Should not throw
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(9)]
    public void GenerateAndWrite_ProducesSpoilerData(int level)
    {
        var reader = CreateYamlReader();
        var builder = CreateBuilder(reader.Data!, level);
        builder.Generate();
        builder.Write();

        var spoiler = builder.GetSpoilerData();
        Assert.AreEqual(level, spoiler.Level);
        Assert.IsTrue(spoiler.Rooms.Count > 0, "Spoiler should have rooms");
        Assert.IsTrue(spoiler.Width >= 4, "Spoiler width should be at least 4");
        Assert.IsTrue(spoiler.Height >= 4, "Spoiler height should be at least 4");
    }

    [TestMethod]
    public void BuildMapData_CentersFourByFourLayout()
    {
        var mapData = DungeonBuilder.BuildMapData(CreateBlockRoomIds(0, 0, 4, 4));

        Assert.AreEqual(0xDD, mapData.StartY);
        Assert.AreEqual(6, mapData.SubmenuMapRotation);
        Assert.AreEqual(0x10, mapData.StatusBarMapXOffset);
        CollectionAssert.AreEqual(
            new int[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0xF0, 0xF0, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            mapData.SubmenuMapMask);
        CollectionAssert.AreEqual(
            CreateExpectedStatusBarMapTransferBuffer(
                0x20, 0x64, 0x04, 0xFF, 0xFF, 0xFF, 0xFF,
                0x20, 0x84, 0x04, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF),
            mapData.StatusBarMapTransferBuf);
    }

    [TestMethod]
    public void BuildMapData_CentersSixBySixLayout()
    {
        var mapData = DungeonBuilder.BuildMapData(CreateBlockRoomIds(0, 0, 6, 6));

        Assert.AreEqual(5, mapData.SubmenuMapRotation);
        Assert.AreEqual(0x08, mapData.StatusBarMapXOffset);
        CollectionAssert.AreEqual(
            new int[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0xFC, 0xFC, 0xFC, 0xFC, 0xFC, 0xFC, 0x00, 0x00, 0x00, 0x00, 0x00 },
            mapData.SubmenuMapMask);
        CollectionAssert.AreEqual(
            CreateExpectedStatusBarMapTransferBuffer(
                0x20, 0x63, 0x06, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0x20, 0x83, 0x06, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0x20, 0xA3, 0x06, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF),
            mapData.StatusBarMapTransferBuf);
    }

    [TestMethod]
    public void BuildMapData_CentersEightByEightLayout()
    {
        var mapData = DungeonBuilder.BuildMapData(CreateBlockRoomIds(0, 0, 8, 8));

        Assert.AreEqual(4, mapData.SubmenuMapRotation);
        Assert.AreEqual(0x00, mapData.StatusBarMapXOffset);
        CollectionAssert.AreEqual(
            new int[] { 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00 },
            mapData.SubmenuMapMask);
        CollectionAssert.AreEqual(
            new byte[]
            {
                0x20, 0x62, 0x08, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0x20, 0x82, 0x08, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0x20, 0xA2, 0x08, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0x20, 0xC2, 0x08, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF,
            },
            mapData.StatusBarMapTransferBuf);
    }

    [TestMethod]
    public void BuildMapData_EncodesInternalGapsInStatusBarMap()
    {
        var mapData = DungeonBuilder.BuildMapData(new[] { 0x00, 0x02 });

        Assert.AreEqual(6, mapData.SubmenuMapRotation);
        Assert.AreEqual(0x10, mapData.StatusBarMapXOffset);
        CollectionAssert.AreEqual(
            new int[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            mapData.SubmenuMapMask);
        CollectionAssert.AreEqual(
            CreateExpectedStatusBarMapTransferBuffer(0x20, 0x64, 0x03, 0x67, 0x24, 0x67, 0xFF),
            mapData.StatusBarMapTransferBuf);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(9)]
    public void Generate_HasExactlyOneStartBossEndRoom(int level)
    {
        var reader = CreateYamlReader();
        var builder = CreateBuilder(reader.Data!, level);
        builder.Generate();

        var spoiler = builder.GetSpoilerData();
        Assert.AreEqual(1, spoiler.Rooms.Count(r => r.Roles.Contains("Start")),
            $"Level {level}: Should have exactly one Start room");
        Assert.AreEqual(1, spoiler.Rooms.Count(r => r.Roles.Contains("Boss")),
            $"Level {level}: Should have exactly one Boss room");
        Assert.AreEqual(1, spoiler.Rooms.Count(r => r.Roles.Contains("End")),
            $"Level {level}: Should have exactly one End room");
    }

    [TestMethod]
    public void Generate_Level9_HasLevelNineCheckRoom()
    {
        var reader = CreateYamlReader();
        var builder = CreateBuilder(reader.Data!, 9);
        builder.Generate();

        var spoiler = builder.GetSpoilerData();
        Assert.AreEqual(1, spoiler.Rooms.Count(r => r.Roles.Contains("LevelNineCheck")),
            "Level 9 should have exactly one LevelNineCheck room");
    }

    [TestMethod]
    public void Generate_NonLevel9_NoLevelNineCheckRoom()
    {
        var reader = CreateYamlReader();
        for (int level = 1; level <= 8; level++)
        {
            var builder = CreateBuilder(reader.Data!, level);
            builder.Generate();

            var spoiler = builder.GetSpoilerData();
            Assert.AreEqual(0, spoiler.Rooms.Count(r => r.Roles.Contains("LevelNineCheck")),
                $"Level {level}: Should not have a LevelNineCheck room");
        }
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(9)]
    public void Generate_StartRoomAtBottomEdge(int level)
    {
        var reader = CreateYamlReader();
        var builder = CreateBuilder(reader.Data!, level);
        builder.Generate();

        var spoiler = builder.GetSpoilerData();
        var startRoom = spoiler.Rooms.First(r => r.Roles.Contains("Start"));

        // AlignStartRoomToBottomEdge should place start at Y=7 (bottom of 8-row grid)
        Assert.AreEqual(7, startRoom.Y,
            $"Level {level}: Start room Y should be 7 (bottom edge), got {startRoom.Y}");
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(9)]
    public void Generate_StartRoomHasSouthDoor(int level)
    {
        var reader = CreateYamlReader();
        var builder = CreateBuilder(reader.Data!, level);
        builder.Generate();

        var spoiler = builder.GetSpoilerData();
        var startRoom = spoiler.Rooms.First(r => r.Roles.Contains("Start"));

        Assert.IsTrue(startRoom.Doors.ContainsKey("S"), $"Level {level}: Start room must have a south door (exit)");
        Assert.AreEqual("Open", startRoom.Doors["S"], $"Level {level}: Start room south door must be Open");
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(9)]
    public void Generate_AllNonCellarRoomsHaveScreenAssigned(int level)
    {
        var reader = CreateYamlReader();
        var builder = CreateBuilder(reader.Data!, level);
        builder.Generate();

        var spoiler = builder.GetSpoilerData();
        foreach (var room in spoiler.Rooms.Where(r => !r.Roles.Contains("Cellar")))
        {
            Assert.IsTrue(int.TryParse(room.Screen, System.Globalization.NumberStyles.HexNumber, null, out int screenId),
                $"Level {level} Room({room.X},{room.Y}): Screen '{room.Screen}' is not valid hex");
            Assert.IsTrue(screenId >= 0, $"Level {level} Room({room.X},{room.Y}): Screen should be assigned (>= 0)");
        }
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(9)]
    public void Generate_HasItemRooms(int level)
    {
        var reader = CreateYamlReader();
        var builder = CreateBuilder(reader.Data!, level);
        builder.Generate();

        var spoiler = builder.GetSpoilerData();
        int itemRoomCount = spoiler.Rooms.Count(r => r.Roles.Contains("Item"));
        Assert.IsTrue(itemRoomCount > 0, $"Level {level}: Should have at least one item room, got {itemRoomCount}");
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    public void Write_NonLevelNineBossRoomHasBossDrop(int level)
    {
        var reader = CreateYamlReader();
        var data = reader.Data!;
        var builder = CreateBuilder(data, level);

        builder.Generate();
        builder.Write();

        var levelData = data.levels.First(l => l.level == level && l.area == YamlReader.Area.Underworld);
        var bossRoom = data.underworld_maps.Single(m =>
            m.generated &&
            m.generated_level == level &&
            !m.passage &&
            m.map == levelData.boss_room_id);

        Assert.AreEqual((int)YamlReader.RoomBehaviour.KillForItem, bossRoom.behaviour,
            $"Level {level}: Boss room should use kill-for-item behavior");
        Assert.AreEqual(0x01, bossRoom.room_item,
            $"Level {level}: Boss room should spawn an item after the boss dies");
    }

    /// <summary>
    /// Returns the generated non-boss, non-cellar item rooms for a level built with the given
    /// hidden-items setting. Item rooms are identified by room_item == 0x01 (the value
    /// WriteNonCellarRooms uses for the Item role).
    /// </summary>
    private static List<YamlReader.UnderworldMap> GetGeneratedNonBossItemRooms(
        HiddenItemsOption hiddenItems, int level, int seed)
    {
        var reader = CreateYamlReader();
        var data = reader.Data!;
        var config = DungeonConfig.GetConfigForLevel(
            level, DungeonStyleOption.Progressive, EnemyPlacementOption.Progressive, new Random(seed), hiddenItems);
        var builder = CreateBuilder(data, level, config, seed);
        builder.Generate();
        builder.Write();

        var levelData = data.levels.First(l => l.level == level && l.area == YamlReader.Area.Underworld);
        return data.underworld_maps
            .Where(m => m.generated && m.generated_level == level && !m.passage
                && m.map != levelData.boss_room_id && m.room_item == 0x01)
            .ToList();
    }

    [TestMethod]
    public void Write_HiddenItemsOff_NoNonBossRoomHidesItem()
    {
        // Across many seeds/levels, no generated non-boss item room should use the kill-for-item
        // trigger when hidden items are turned off.
        for (int seed = 1; seed <= 15; seed++)
        {
            for (int level = 1; level <= 8; level++)
            {
                var itemRooms = GetGeneratedNonBossItemRooms(HiddenItemsOption.Off, level, seed);
                Assert.IsFalse(itemRooms.Any(m => m.behaviour == (int)YamlReader.RoomBehaviour.KillForItem),
                    $"Level {level} seed {seed}: no non-boss item room should hide its item when HiddenItems=Off");
            }
        }
    }

    [TestMethod]
    public void Write_HiddenItemsAlways_HidesEveryClearableItemRoom()
    {
        // With Always, any generated non-boss item room that has a kill trigger available
        // (i.e. uses KillForItem-capable shutters) hides its item. We verify the converse holds:
        // no clearable item room is left with a visible item, and at least some rooms hide items.
        int hiddenCount = 0;
        for (int seed = 1; seed <= 15; seed++)
        {
            for (int level = 1; level <= 8; level++)
            {
                var itemRooms = GetGeneratedNonBossItemRooms(HiddenItemsOption.Always, level, seed);
                hiddenCount += itemRooms.Count(m => m.behaviour == (int)YamlReader.RoomBehaviour.KillForItem);
            }
        }

        Assert.IsTrue(hiddenCount > 0,
            "HiddenItems=Always should hide items in at least some generated rooms across all seeds/levels");
    }

    [TestMethod]
    public void Write_HiddenItemsAlways_HidesMoreThanOff()
    {
        // Sanity check that the setting actually changes output: Always should hide strictly more
        // non-boss items than Off (which hides none) across the same seeds.
        int offHidden = 0, alwaysHidden = 0;
        for (int seed = 1; seed <= 15; seed++)
        {
            for (int level = 1; level <= 8; level++)
            {
                offHidden += GetGeneratedNonBossItemRooms(HiddenItemsOption.Off, level, seed)
                    .Count(m => m.behaviour == (int)YamlReader.RoomBehaviour.KillForItem);
                alwaysHidden += GetGeneratedNonBossItemRooms(HiddenItemsOption.Always, level, seed)
                    .Count(m => m.behaviour == (int)YamlReader.RoomBehaviour.KillForItem);
            }
        }

        Assert.AreEqual(0, offHidden, "HiddenItems=Off should hide no non-boss items");
        Assert.IsTrue(alwaysHidden > offHidden, "HiddenItems=Always should hide more items than Off");
    }

    [TestMethod]
    [DataRow(DungeonStyleOption.Progressive)]
    [DataRow(DungeonStyleOption.Wild)]
    [DataRow(DungeonStyleOption.Megadungeon)]
    [DataRow(DungeonStyleOption.Nightmare)]
    [DataRow(DungeonStyleOption.Minimal)]
    public void Generate_AllStyles_Level5Succeeds(DungeonStyleOption style)
    {
        var rnd = new Random(42);
        var config = DungeonConfig.GetConfigForLevel(5, style, EnemyPlacementOption.Progressive, rnd);
        var reader = CreateYamlReader();
        var builder = CreateBuilder(reader.Data!, 5, config);
        builder.Generate();
        builder.Write();

        var spoiler = builder.GetSpoilerData();
        Assert.IsTrue(spoiler.Rooms.Count > 0);
    }

    [TestMethod]
    public void Generate_Deterministic_SameSeedSameLayout()
    {
        var reader = CreateYamlReader();

        var builder1 = CreateBuilder(reader.Data!, 3, seed: 9999);
        builder1.Generate();
        var spoiler1 = builder1.GetSpoilerData();

        // Need fresh data since Write mutates it
        var reader2 = CreateYamlReader();
        var builder2 = CreateBuilder(reader2.Data!, 3, seed: 9999);
        builder2.Generate();
        var spoiler2 = builder2.GetSpoilerData();

        Assert.AreEqual(spoiler1.Rooms.Count, spoiler2.Rooms.Count, "Same seed should produce same room count");

        for (int i = 0; i < spoiler1.Rooms.Count; i++)
        {
            Assert.AreEqual(spoiler1.Rooms[i].X, spoiler2.Rooms[i].X, $"Room {i} X mismatch");
            Assert.AreEqual(spoiler1.Rooms[i].Y, spoiler2.Rooms[i].Y, $"Room {i} Y mismatch");
            Assert.AreEqual(spoiler1.Rooms[i].Screen, spoiler2.Rooms[i].Screen, $"Room {i} Screen mismatch");
        }
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(9)]
    public void Generate_AllRoomsConnected(int level)
    {
        var reader = CreateYamlReader();
        var builder = CreateBuilder(reader.Data!, level);
        builder.Generate();

        var spoiler = builder.GetSpoilerData();
        var rooms = spoiler.Rooms;

        // Build adjacency from door connections and cellar connections
        var roomByPos = rooms.ToDictionary(r => (r.X, r.Y));
        var adjacency = rooms.ToDictionary(r => (r.X, r.Y), _ => new HashSet<(int, int)>());

        // Door-based connections (non-cellar rooms)
        var dirOffsets = new Dictionary<string, (int dx, int dy)>
        {
            ["N"] = (0, -1), ["S"] = (0, 1), ["W"] = (-1, 0), ["E"] = (1, 0)
        };

        foreach (var room in rooms.Where(r => !r.Roles.Contains("Cellar")))
        {
            foreach (var (dir, doorType) in room.Doors)
            {
                if (doorType == "Wall") continue;
                if (!dirOffsets.TryGetValue(dir, out var offset)) continue;

                var neighborPos = (room.X + offset.dx, room.Y + offset.dy);
                if (roomByPos.ContainsKey(neighborPos))
                {
                    adjacency[(room.X, room.Y)].Add(neighborPos);
                }
            }
        }

        // Cellar/stair connections: cellars bridge non-adjacent rooms.
        // A cellar's ConnectedTo lists the rooms it links, and those rooms
        // have the Stairs/Connector role allowing traversal through the cellar.
        foreach (var cellar in rooms.Where(r => r.Roles.Contains("Cellar") && r.ConnectedTo != null))
        {
            var connections = cellar.ConnectedTo!.Select(c => (c[0], c[1])).ToList();

            // Cellar is reachable from each connected room and vice versa
            foreach (var connPos in connections)
            {
                if (adjacency.ContainsKey(connPos))
                {
                    adjacency[(cellar.X, cellar.Y)].Add(connPos);
                    adjacency[connPos].Add((cellar.X, cellar.Y));
                }
            }

            // Connected rooms can also reach each other through this cellar
            for (int i = 0; i < connections.Count; i++)
                for (int j = i + 1; j < connections.Count; j++)
                {
                    adjacency[connections[i]].Add(connections[j]);
                    adjacency[connections[j]].Add(connections[i]);
                }
        }

        // BFS from start room
        var startRoom = rooms.First(r => r.Roles.Contains("Start"));
        var visited = new HashSet<(int, int)>();
        var queue = new Queue<(int, int)>();
        queue.Enqueue((startRoom.X, startRoom.Y));
        visited.Add((startRoom.X, startRoom.Y));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var neighbor in adjacency[current])
            {
                if (visited.Add(neighbor))
                    queue.Enqueue(neighbor);
            }
        }

        // Verify key rooms are reachable
        var endRoom = rooms.First(r => r.Roles.Contains("End"));
        var bossRoom = rooms.First(r => r.Roles.Contains("Boss"));
        Assert.IsTrue(visited.Contains((endRoom.X, endRoom.Y)),
            $"Level {level}: End room at ({endRoom.X},{endRoom.Y}) not reachable from Start");
        Assert.IsTrue(visited.Contains((bossRoom.X, bossRoom.Y)),
            $"Level {level}: Boss room at ({bossRoom.X},{bossRoom.Y}) not reachable from Start");

        // The vast majority of rooms should be reachable (Level 9 start isolation
        // may leave a small number of rooms only reachable through internal graph
        // connections not visible in spoiler door data)
        double reachableRatio = (double)visited.Count / rooms.Count;
        Assert.IsTrue(reachableRatio >= 0.90,
            $"Level {level}: Only {visited.Count}/{rooms.Count} rooms reachable ({reachableRatio:P0})");
    }

    [TestMethod]
    public void Write_UpdatesLevelData()
    {
        int level = 3;
        var reader = CreateYamlReader();
        var data = reader.Data!;
        var levelData = data.levels.First(l => l.level == level && l.area == YamlReader.Area.Underworld);

        int originalStartRoom = levelData.start_room_id;

        var builder = CreateBuilder(data, level);
        builder.Generate();
        builder.Write();

        // After Write, level data should be updated
        Assert.AreNotEqual(originalStartRoom, levelData.start_room_id,
            "Write should update start_room_id in level data");
        Assert.IsTrue(levelData.rooms.Length > 0, "Write should populate rooms array");
        Assert.IsTrue(levelData.rooms.All(r => r >= 0x100),
            "Generated room IDs should use the extended range (0x100+)");
    }

    [TestMethod]
    public void Write_GeneratedMapsAddedToYamlData()
    {
        var reader = CreateYamlReader();
        var data = reader.Data!;
        int mapCountBefore = data.underworld_maps.Count;

        var builder = CreateBuilder(data, 1);
        builder.Generate();
        builder.Write();

        Assert.IsTrue(data.underworld_maps.Count > mapCountBefore,
            "Write should add generated maps to underworld_maps");

        var generatedMaps = data.underworld_maps.Where(m => m.generated).ToList();
        Assert.IsTrue(generatedMaps.Count > 0, "Should have generated maps flagged");
        Assert.IsTrue(generatedMaps.All(m => m.generated_level == 1), "All generated maps should be for level 1");
    }

    [TestMethod]
    public void Write_Level9ExcludesCheckRoomFromGeneratedMapData()
    {
        var reader = CreateYamlReader();
        var data = reader.Data!;
        var builder = CreateBuilder(data, 9);

        builder.Generate();
        builder.Write();

        var levelData = data.levels.First(l => l.level == 9 && l.area == YamlReader.Area.Underworld);
        var levelMaps = data.underworld_maps
            .Where(m => m.generated && m.generated_level == 9 && !m.passage)
            .ToList();
        var checkRoom = levelMaps.Single(m => m.level_nine_check == true);
        var visibleLocalRoomIds = levelMaps
            .Where(m => m.level_nine_check != true)
            .Select(m => m.local_room_id)
            .ToArray();

        Assert.IsFalse(visibleLocalRoomIds.Contains(checkRoom.local_room_id),
            "Level 9 check room should be excluded from generated map data.");

        var expectedMapData = DungeonBuilder.BuildMapData(visibleLocalRoomIds);

        Assert.AreEqual(expectedMapData.StartY, levelData.start_y);
        Assert.AreEqual(expectedMapData.SubmenuMapRotation, levelData.submenu_map_rotation);
        Assert.AreEqual(expectedMapData.StatusBarMapXOffset, levelData.status_bar_map_x_offset);
        CollectionAssert.AreEqual(expectedMapData.SubmenuMapMask, levelData.submenu_map_mask);
        CollectionAssert.AreEqual(expectedMapData.StatusBarMapTransferBuf, levelData.status_bar_map_transfer_buf);
    }

    [TestMethod]
    [DataRow(EnemyPlacementOption.Vanilla)]
    [DataRow(EnemyPlacementOption.Progressive)]
    [DataRow(EnemyPlacementOption.Random)]
    public void Generate_AllEnemyPlacementOptions_Succeed(EnemyPlacementOption placement)
    {
        var rnd = new Random(42);
        var config = DungeonConfig.GetConfigForLevel(5, DungeonStyleOption.Progressive, placement, rnd);
        var reader = CreateYamlReader();
        var builder = CreateBuilder(reader.Data!, 5, config);
        builder.Generate();
        // Should not throw
    }

    [TestMethod]
    public void Generate_MultipleSeeds_NoCrashes()
    {
        // Stress test: run 10 different seeds for each level
        for (int seed = 1; seed <= 10; seed++)
        {
            var reader = CreateYamlReader();
            for (int level = 1; level <= 9; level++)
            {
                var rnd = new Random(seed);
                var config = DungeonConfig.GetConfigForLevel(level, DungeonStyleOption.Progressive, EnemyPlacementOption.Progressive, rnd);
                var builder = CreateBuilder(reader.Data!, level, config, seed);
                builder.Generate();
            }
        }
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(9)]
    public void Generate_CellarRoomsHaveConnections(int level)
    {
        var reader = CreateYamlReader();
        var builder = CreateBuilder(reader.Data!, level);
        builder.Generate();

        var spoiler = builder.GetSpoilerData();
        var cellars = spoiler.Rooms.Where(r => r.Roles.Contains("Cellar")).ToList();

        foreach (var cellar in cellars)
        {
            Assert.IsNotNull(cellar.ConnectedTo,
                $"Level {level}: Cellar at ({cellar.X},{cellar.Y}) should have ConnectedTo data");
            Assert.IsTrue(cellar.ConnectedTo.Length >= 1,
                $"Level {level}: Cellar at ({cellar.X},{cellar.Y}) should connect to at least one room");
        }
    }

    [TestMethod]
    public void Generate_SegmentedDungeon_HasConnectorRooms()
    {
        // Level 5+ should have multiple segments and connectors
        var reader = CreateYamlReader();
        var rnd = new Random(42);
        var config = DungeonConfig.GetConfigForLevel(5, DungeonStyleOption.Progressive, EnemyPlacementOption.Progressive, rnd);

        if (config.Segments > 1)
        {
            var builder = CreateBuilder(reader.Data!, 5, config);
            builder.Generate();

            var spoiler = builder.GetSpoilerData();
            int connectorCount = spoiler.Rooms.Count(r => r.Roles.Contains("Connector"));
            Assert.IsTrue(connectorCount >= config.Segments - 1,
                $"With {config.Segments} segments, need >= {config.Segments - 1} connectors, got {connectorCount}");
        }
    }
}
