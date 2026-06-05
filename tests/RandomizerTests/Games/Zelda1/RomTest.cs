namespace RandomizerTests.Games.Zelda1;

using Randomizer.Games.Zelda1;
using Randomizer.Graph;
using Randomizer.RomModifications;

[TestClass]
public sealed class RomTest
{
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

    [TestMethod]
    public void WriteLevelData_WritesGeneratedMapFieldsAtVanillaOffsets()
    {
        const int level = 3;
        const int levelInfoBase = 0x631300;
        const int levelInfoPerLevel = 0xFC;

        var reader = CreateYamlReader();
        var data = reader.Data!;
        var builder = CreateBuilder(data, level);
        builder.Generate();
        builder.Write();

        var levelData = data.levels.First(l => l.level == level && l.area == YamlReader.Area.Underworld);
        int levelOffset = levelInfoBase + level * levelInfoPerLevel;

        using var loggedRom = new LoggedRom();
        var rom = new Rom(loggedRom, 0);
        rom.WriteLevelData(data);

        Assert.AreEqual((byte)levelData.start_y, loggedRom.Read(levelOffset + 0x28, 1)[0]);
        Assert.AreEqual((byte)(levelData.start_room_id & 0x7F), loggedRom.Read(levelOffset + 0x2F, 1)[0]);
        Assert.AreEqual((byte)(levelData.triforce_room_id & 0x7F), loggedRom.Read(levelOffset + 0x30, 1)[0]);
        Assert.AreEqual((byte)(levelData.boss_room_id & 0x7F), loggedRom.Read(levelOffset + 0x3E, 1)[0]);
        Assert.AreEqual((byte)levelData.submenu_map_mask[0], loggedRom.Read(levelOffset + 0x3F, 1)[0]);
        Assert.AreEqual((byte)levelData.submenu_map_mask[15], loggedRom.Read(levelOffset + 0x4E, 1)[0]);
        Assert.AreEqual(levelData.status_bar_map_transfer_buf[0], loggedRom.Read(levelOffset + 0x4F, 1)[0]);

        CollectionAssert.AreEqual(
            levelData.submenu_map_mask.Select(value => (byte)value).ToArray(),
            loggedRom.Read(levelOffset + 0x3F, 16));
        CollectionAssert.AreEqual(
            levelData.status_bar_map_transfer_buf,
            loggedRom.Read(levelOffset + 0x4F, 45));
    }

    [TestMethod]
    public void Write_Level9CheckRoom_UsesVanillaCheckRoomPayload()
    {
        const int level = 9;

        var reader = CreateYamlReader();
        var data = reader.Data!;
        var builder = CreateBuilder(data, level);
        builder.Generate();
        builder.Write();

        var checkRoom = data.underworld_maps.Single(m =>
            m.generated &&
            m.generated_level == level &&
            !m.passage &&
            m.level_nine_check == true);

        Assert.AreEqual(0x26, checkRoom.screen, "Level 9 check room should use screen 0x26.");
        Assert.AreEqual(0x0B, checkRoom.enemy_id, "Level 9 check room should use the vanilla check-room NPC/object payload.");
        Assert.AreEqual(0x01, checkRoom.enemy_mode, "Level 9 check room should use the vanilla check-room enemy mode.");
        Assert.AreEqual(0x00, checkRoom.enemies, "Level 9 check room should not spawn normal enemies.");
        Assert.AreEqual((int)YamlReader.RoomBehaviour.None, checkRoom.behaviour, "Level 9 check room should keep vanilla behaviour.");

        Assert.AreEqual((int)YamlReader.DoorType.Open, checkRoom.doors[1], "Level 9 check room south door should remain open.");
        foreach (var door in new[] { checkRoom.doors[0], checkRoom.doors[2], checkRoom.doors[3] })
        {
            if (door == (int)YamlReader.DoorType.Wall)
                continue;

            Assert.AreEqual((int)YamlReader.DoorType.Shutter, door,
            "Level 9 check room non-south exits should remain shutters.");
        }
    }

    [TestMethod]
    public void WriteLevelData_Level9WritesGanonSpecialDropToCustomItemTable()
    {
        const int level = 9;
        const int roomItemsUwExtendedCustom = 0x651000;

        var reader = CreateYamlReader();
        var data = reader.Data!;
        var builder = CreateBuilder(data, level);
        builder.Generate();
        builder.Write();

        var levelData = data.levels.First(l => l.level == level && l.area == YamlReader.Area.Underworld);
        int bossRoomLocal = levelData.boss_room_id & 0x7F;

        using var loggedRom = new LoggedRom();
        var rom = new Rom(loggedRom, 0);
        rom.WriteLevelData(data);

        Assert.AreEqual(0x0E, loggedRom.Read(roomItemsUwExtendedCustom + level * 0x80 + bossRoomLocal, 1)[0],
            "Level 9 boss room should write special item code 0x0E into the generated dungeon item table.");
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
    public void Write_EndRoom_IsOnlyReachedThroughShutteredBossExit(int level)
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
        var zeldaRoom = data.underworld_maps.Single(m =>
            m.generated &&
            m.generated_level == level &&
            !m.passage &&
            m.map == levelData.triforce_room_id);

        int dx = (bossRoom.local_room_id & 0x0F) - (zeldaRoom.local_room_id & 0x0F);
        int dy = ((bossRoom.local_room_id >> 4) & 0x0F) - ((zeldaRoom.local_room_id >> 4) & 0x0F);
        Assert.AreEqual(1, Math.Abs(dx) + Math.Abs(dy), $"Level {level}: Boss room must be adjacent to the end room.");

        int bossDoorIndex;
        int endDoorIndex;
        if (dx == 1)
        {
            bossDoorIndex = 2; // W
            endDoorIndex = 3;  // E
        }
        else if (dx == -1)
        {
            bossDoorIndex = 3; // E
            endDoorIndex = 2;  // W
        }
        else if (dy == 1)
        {
            bossDoorIndex = 0; // N
            endDoorIndex = 1;  // S
        }
        else
        {
            bossDoorIndex = 1; // S
            endDoorIndex = 0;  // N
        }

        Assert.AreEqual((int)YamlReader.DoorType.Shutter, bossRoom.doors[bossDoorIndex],
            $"Level {level}: Boss-room exit into the end room should be shuttered.");

        // The end-room side stays open for levels 1-8 so the exit isn't trapped shut on entry
        // (its behaviour is None, so a shutter there would never reopen). Only level 9's end room
        // uses a kill-trigger that reopens its shutter, so it keeps the shutter.
        int expectedEndDoor = level == 9
            ? (int)YamlReader.DoorType.Shutter
            : (int)YamlReader.DoorType.Open;
        Assert.AreEqual(expectedEndDoor, zeldaRoom.doors[endDoorIndex],
            $"Level {level}: End-room entrance from the boss room has the expected door type.");

        int nonWallDoorsOnEndRoom = zeldaRoom.doors.Count(d => d != (int)YamlReader.DoorType.Wall);
        Assert.AreEqual(1, nonWallDoorsOnEndRoom,
            $"Level {level}: End room should only have one non-wall exit.");

        bool cellarConnectsToEndRoom = data.underworld_maps.Any(m =>
            m.generated &&
            m.generated_level == level &&
            m.passage &&
            (m.passage_left == zeldaRoom.map || m.passage_right == zeldaRoom.map));
        Assert.IsFalse(cellarConnectsToEndRoom,
            $"Level {level}: End room should not be reachable through any cellar connection.");
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
    public void WriteUnderworldMapData_PassageExitsAreValidLocalRoomIds(int level)
    {
        const int generatedBlockDataBase = 0x660000;
        const int blockDataPerLevel = 0x300;

        var reader = CreateYamlReader();
        var data = reader.Data!;
        var builder = CreateBuilder(data, level);
        builder.Generate();
        builder.Write();

        var levelMaps = data.underworld_maps.Where(m => m.generated && m.generated_level == level).ToList();
        var nonPassageLocalIds = levelMaps.Where(m => !m.passage).Select(m => m.local_room_id).ToHashSet();
        var passageMaps = levelMaps.Where(m => m.passage).ToList();

        using var loggedRom = new LoggedRom();
        var rom = new Rom(loggedRom, 0);
        rom.WriteUnderworldMapData(data);

        int levelBase = generatedBlockDataBase + ((level - 1) * blockDataPerLevel);

        foreach (var passage in passageMaps)
        {
            int roomId = passage.local_room_id;
            byte writtenLeft = loggedRom.Read(levelBase + 0x00 + roomId, 1)[0];
            byte writtenRight = loggedRom.Read(levelBase + 0x80 + roomId, 1)[0];

            Assert.IsTrue(writtenLeft < 0x80,
                $"Level {level}: Passage at local_room_id=0x{roomId:X2} has left exit 0x{writtenLeft:X2} which is >= 0x80 (invalid local room ID)");
            Assert.IsTrue(writtenRight < 0x80,
                $"Level {level}: Passage at local_room_id=0x{roomId:X2} has right exit 0x{writtenRight:X2} which is >= 0x80 (invalid local room ID)");

            Assert.IsTrue(nonPassageLocalIds.Contains(writtenLeft),
                $"Level {level}: Passage at local_room_id=0x{roomId:X2} has left exit 0x{writtenLeft:X2} which doesn't match any non-passage room");
            Assert.IsTrue(nonPassageLocalIds.Contains(writtenRight),
                $"Level {level}: Passage at local_room_id=0x{roomId:X2} has right exit 0x{writtenRight:X2} which doesn't match any non-passage room");
        }
    }
}
