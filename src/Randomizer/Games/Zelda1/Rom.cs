namespace Randomizer.Games.Zelda1;

using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Graph;
using Randomizer.RomModifications;

public static class RomExtensions
{
    public static ReadOnlySpan<byte> ToBytes(this IEnumerable<int> data)
    {
        return data.Select(x => (byte)x).ToArray();
    }
}

public class Rom : GameRom
{
    public Rom(IRom rom, int offset) : base(rom, offset)
    {

    }

    public void WriteItems(World world)
    {
        foreach (var location in world.GetLocationsOfType(VertexType.Item).Where(l => l.Item != null))
        {
            if (location.Item!.Bytes == null)
                continue;

            if (location.Addresses == null)
                continue;

            Write((Address)location.Addresses[0], location.Item!.Bytes);
        }
    }

    public void WriteTriforceGoal(World world)
    {
        var triforceGoal = int.Parse(world.Config.Triforces);
        Write(0x657000, [(byte)triforceGoal]);
    }

    public void WriteSpecial(World world, PRNG prng, Zelda1.YamlReader.YamlData data)
    {
        var special = data.special;
        Write(0x62b89a, [(byte)special.overworld_item_room]);
        Write(0x62b88e, [(byte)special.overworld_item_x]);
        //rom.Write(0x62b88a, [(byte)special.overworld_item_id], 0);
        Write(0x620cb2, [(byte)special.armos_item_room]);
        Write(0x620cb9, [(byte)special.armos_item_x]);
        //rom.Write(0x620cf5, [(byte)special.armos_item_id], 0);
        Write(0x620cb3, special.armos_stairs.ToBytes());
        Write(0x620cba, special.armos_x_pos.ToBytes());
        Write(0x63b20d, special.step_ladder.ToBytes());
        Write(0x63af66, special.recorder_stairs.ToBytes());
        Write(0x60A010, special.recorder_dests.ToBytes());
        Write(0x60A119, special.recorder_y_pos.ToBytes());
        Write(0x628F35, [(byte)special.any_road_x_pos[0]]);
        Write(0x628F3A, [(byte)special.any_road_x_pos[1]]);
        Write(0x628F3F, [(byte)special.any_road_x_pos[2]]);
        // Missing start


        // This should probably go into its own WriteLevelData later when it's complete
        // Write the any road targets back to the ROM
        Write(0x631334, data.levels[0].cellar_room_id_array.ToBytes());
    }

    public void WriteOverworldMapData(World world, PRNG prng, Zelda1.YamlReader.YamlData data)
    {
        /* Write out overworld map data */
        foreach (var map in data.overworld_maps.Where(x => x.name != "Meta"))
        {
            var levelInfoA_offset = 0x630400 + map.map;
            var levelInfoB_offset = 0x630480 + map.map;
            var levelInfoC_offset = 0x630500 + map.map;
            var levelInfoD_offset = 0x630580 + map.map;
            var levelInfoE_offset = 0x630600 + map.map;
            var levelInfoF_offset = 0x630680 + map.map;

            byte levelInfoA_data = (byte)(map.zora ? 0x08 : 0x00);
            levelInfoA_data |= (byte)(map.waves ? 0x04 : 0x00);
            levelInfoA_data |= (byte)map.palettes[1];
            levelInfoA_data |= (byte)(map.exit[0] << 4);

            byte levelInfoB_data = (byte)map.palettes[0];
            levelInfoB_data |= (byte)(map.cave << 2);

            byte levelInfoC_data = (byte)(map.enemies << 6);
            levelInfoC_data |= (byte)map.enemy_id;

            byte levelInfoD_data = (byte)(map.enemy_mode << 7);
            levelInfoD_data |= (byte)map.screen;

            byte levelInfoE_data = (byte)map.level_info_e;

            byte levelInfoF_data = (byte)(map.secret[1] == 1 ? 0x80 : 0x00);
            levelInfoF_data |= (byte)(map.secret[0] == 1 ? 0x40 : 0x00);
            levelInfoF_data |= (byte)(map.stairs << 4);
            levelInfoF_data |= (byte)(map.enemy_sides << 3);
            levelInfoF_data |= (byte)map.exit[1];

            Write(levelInfoA_offset, [levelInfoA_data]);
            Write(levelInfoB_offset, [levelInfoB_data]);
            Write(levelInfoC_offset, [levelInfoC_data]);
            Write(levelInfoD_offset, [levelInfoD_data]);
            Write(levelInfoE_offset, [levelInfoE_data]);
            Write(levelInfoF_offset, [levelInfoF_data]);

        }
    }

    // ─── Generated Dungeon ROM Layout ───
    //
    // The NES engine loads level block data using a 16-bit address pointer (LevelBlockAddrsQ1)
    // and a bank byte (LevelBlockBanksQ1 at $8A8600). Each level has 6 attribute tables of
    // 0x80 bytes each = 0x300 bytes total, indexed by room ID (0x00-0x7F).
    //
    // Vanilla NES table format (ref: Z1M1 DungeonRoom / BaseDungeonRoom):
    //   Table 1: base + 0x000  DoorN[7:5] | DoorS[4:2] | OuterPalette[1:0]
    //   Table 2: base + 0x080  DoorW[7:5] | DoorE[4:2] | InnerPalette[1:0]
    //   Table 3: base + 0x100  EnemyQuant[7:6] | EnemyCode[5:0]
    //   Table 4: base + 0x180  EnemyMode[7] | MovableBlock[6] | ScreenNum[5:0]
    //   Table 5: base + 0x200  DarkRoom[7] | BossCry[6:5] | RoomItem[4:0]
    //   Table 6: base + 0x280  Unused[7:6] | ItemPos[5:4] | Unused[3] | Behavior[2:0]
    //
    // We place all 9 levels' data starting at $8C8000, 0x300 bytes per level.
    // Bank byte for level N = $8B, address = N * 0x300 within the bank.

    private const int GeneratedBlockDataBase = 0x660000; // 0x8C8000 in the SA-1 map
    private const int BlockDataPerLevel = 0x300;
    private const int LevelBlockBanksQ1 = 0x650600; // 0x8A8600; // Bank byte per level (10 entries: OW + 9 dungeons)
    private const int LevelBlockAddrsQ1 = 0x630000; // 0x868000; // Address pointer per level (10 entries: OW + 9 dungeons)

    // Vanilla level info: 0xFC bytes per level, 10 entries (OW + 9 dungeons).
    private const int LevelInfoBase = 0x631300;
    private const int LevelInfoPerLevel = 0xFC;

    // RoomItemsUW_extended: 256-byte table at $8A8000 (PC 0x650000).
    // Slots 0x00-0x7F for levels 1-6, 0x80-0xFF for levels 7-9 (vanilla only).
    private const int RoomItemsUWExtended = 0x650000;

    // RoomItemsUW_extended_custom: per-level table at $8A9000 (PC 0x651000).
    // Indexed as level * 0x80 + room_id. Used by generated dungeons.
    private const int RoomItemsUWExtendedCustom = 0x651000;

    private const byte NoItemCode = 0x2F;
    private const byte TriforceItemCode = 0x1B;

    /// <summary>
    /// Helper: looks up a generated UnderworldMap by its extended map ID and returns its local_room_id.
    /// </summary>
    private static int GetLocalRoomId(Zelda1.YamlReader.YamlData data, int extendedMapId)
    {
        var map = data.underworld_maps.FirstOrDefault(m => m.map == extendedMapId && m.generated);
        return map?.local_room_id ?? (extendedMapId & 0x7F);
    }

    private static byte[] ValidateByteArray(byte[] data, int expectedLength, string name, int level)
    {
        if (data.Length != expectedLength)
            throw new InvalidOperationException($"Level {level} {name} length was {data.Length}, expected {expectedLength}.");

        return data;
    }

    private static byte[] ValidateIntArray(IEnumerable<int> data, int expectedLength, string name, int level)
    {
        var bytes = data.ToBytes().ToArray();
        if (bytes.Length != expectedLength)
            throw new InvalidOperationException($"Level {level} {name} length was {bytes.Length}, expected {expectedLength}.");

        return bytes;
    }

    public void WriteUnderworldMapData(Zelda1.YamlReader.YamlData data)
    {
        for (int level = 1; level <= 9; level++)
        {
            var levelMaps = data.underworld_maps.Where(m => m.generated && m.generated_level == level).ToList();
            if (levelMaps.Count == 0)
                continue;

            int levelBase = GeneratedBlockDataBase + ((level - 1) * BlockDataPerLevel);

            // Initialize all 6 tables to 0x00 (0x80 bytes each)
            var emptyTable = new byte[0x80];
            for (int t = 0; t < 6; t++)
                Write(levelBase + t * 0x80, emptyTable);

            foreach (var map in levelMaps)
            {
                int roomId = map.local_room_id;
                if (roomId < 0 || roomId >= 0x80)
                    continue;

                // Vanilla NES block table format (verified against Z1M1 / NES disassembly):
                //
                // Table 1 (+0x000): DoorN[7:5] | DoorS[4:2] | OuterPalette[1:0]
                // Table 2 (+0x080): DoorW[7:5] | DoorE[4:2] | InnerPalette[1:0]
                // Table 3 (+0x100): EnemyQuant[7:6] | EnemyCode[5:0]
                // Table 4 (+0x180): EnemyMode[7] | MovableBlock[6] | ScreenNum[5:0]
                // Table 5 (+0x200): DarkRoom[7] | BossCry[6:5] | RoomItem[4:0]
                // Table 6 (+0x280): Unused[7:6] | ItemPos[5:4] | Unused[3] | Behavior[2:0]

                // doors[0]=N, doors[1]=S, doors[2]=W, doors[3]=E

                if (!map.passage)
                {
                    // Table 1: DoorN[7:5] | DoorS[4:2] | OuterPalette[1:0]
                    byte table1 = (byte)(((map.doors[0] & 0x07) << 5) | ((map.doors[1] & 0x07) << 2) | (map.palettes[1] & 0x03));
                    Write(levelBase + 0x00 + roomId, [table1]);

                    // Table 2: DoorW[7:5] | DoorE[4:2] | InnerPalette[1:0]
                    byte table2 = (byte)(((map.doors[2] & 0x07) << 5) | ((map.doors[3] & 0x07) << 2) | (map.palettes[0] & 0x03));
                    Write(levelBase + 0x80 + roomId, [table2]);

                    // Table 3: EnemyQuant[7:6] | EnemyCode[5:0]
                    byte table3 = (byte)(((map.enemies & 0x03) << 6) | (Math.Max(0, map.enemy_id) & 0x3F));
                    Write(levelBase + 0x100 + roomId, [table3]);
                }
                else
                {
                    // passage_left/right contain extended map IDs (0x100+); convert to local room IDs for the NES engine
                    byte table1 = (byte)GetLocalRoomId(data, map.passage_left);
                    Write(levelBase + 0x00 + roomId, [table1]);

                    byte table2 = (byte)GetLocalRoomId(data, map.passage_right);
                    Write(levelBase + 0x80 + roomId, [table2]);

                    // Table 3: EnemyQuant[7:6] | EnemyCode[5:0]
                    // FIXME: (Not correct, this should be ExitXPos (top 4 bits) | ExitYPos (bottom 4 bits))
                    byte table3 = (byte)(((map.enemies & 0x03) << 6) | (Math.Max(0, map.enemy_id) & 0x3F));
                    Write(levelBase + 0x100 + roomId, [table3]);
                }

                // Table 4: EnemyMode[7] | MovableBlock[6] | ScreenNum[5:0]
                byte table4 = (byte)(((map.enemy_mode & 0x01) << 7) | ((map.push_block ? 1 : 0) << 6) | (map.screen & 0x3F));
                Write(levelBase + 0x180 + roomId, [table4]);

                // Table 5: DarkRoom[7] | BossCry[6:5] | RoomItem[4:0]
                byte table5 = (byte)(((map.dark_room ? 1 : 0) << 7) | ((map.boss_sfx & 0x03) << 5) | (map.room_item & 0x1F));
                Write(levelBase + 0x200 + roomId, [table5]);

                // Table 6: Unused[7:6] | ItemPos[5:4] | Unused[3] | Behavior[2:0]
                byte table6 = (byte)(((map.item_pos & 0x03) << 4) | (map.behaviour & 0x07));
                Write(levelBase + 0x280 + roomId, [table6]);
            }

            // Update the bank byte for this level to point to bank $8C
            Write(LevelBlockBanksQ1 + level, [0x8C]);

            // Update the 16-bit address pointer for this level (little-endian)
            // Each level's data starts at $8000 + (level-1) * 0x300 within the bank
            int addrInBank = 0x8000 + ((level - 1) * BlockDataPerLevel);
            Write(LevelBlockAddrsQ1 + level * 2, [(byte)(addrInBank & 0xFF), (byte)(addrInBank >> 8)]);
        }
    }

    public void WriteLevelData(Zelda1.YamlReader.YamlData data)
    {
        for (int level = 1; level <= 9; level++)
        {
            var levelData = data.levels.FirstOrDefault(l => l.level == level && l.area == Zelda1.YamlReader.Area.Underworld);
            if (levelData == null)
                continue;

            // Only write level info for generated dungeons
            var hasGenerated = data.underworld_maps.Any(m => m.generated && m.generated_level == level);
            if (!hasGenerated)
                continue;

            // Vanilla level info layout (0xFC bytes per level, ref: Z1M1 DungeonLevelData):
            //
            // +0x00-0x23: palettes_transfer_buf (36 bytes: PPU addr + len + 32 palette bytes + FF)
            // +0x24-0x27: enemy_counts (4 bytes)
            // +0x28:      start_y                           [DungeonInfo.StartY]
            // +0x29-0x2C: shortcut_or_item_pos_array (4 bytes) [DungeonInfo.ItemTiles]
            // +0x2D:      submenu_map_rotation              [DungeonInfo.MainMapShift]
            // +0x2E:      status_bar_map_x_offset           [DungeonInfo.MiniMapShift]
            // +0x2F:      start_room_id (local)             [DungeonInfo.EntranceLoc]
            // +0x30:      triforce_room_id (local)          [DungeonInfo.TriforceLoc]
            // +0x31-0x32: world_flags_addr (2 bytes)        [DungeonInfo.Reserved/RAM offset]
            // +0x33:      level number                      [DungeonInfo.LevelNum]
            // +0x34-0x3D: cellar_room_id_array (10 bytes)   [DungeonInfo.StairwayList]
            // +0x3E:      boss_room_id (local)              [DungeonInfo.BossLoc]
            // +0x3F-0x4E: submenu_map_mask (16 bytes)       [DungeonInfo.MainMapData]
            // +0x4F-0x7B: status_bar_map_transfer_buf (45b) [DungeonInfo.MiniMapData]
            // +0x7C-0xDB: palette_cycles (96 bytes)         [FadePalettes]
            // +0xDC-0xFB: death_palette_series (32 bytes)   [remaining data]
            int levelOffset = LevelInfoBase + (level * LevelInfoPerLevel);

            // Convert extended map IDs to local room IDs for NES-side level info
            byte startRoomLocal = (byte)GetLocalRoomId(data, levelData.start_room_id);
            byte bossRoomLocal = (byte)GetLocalRoomId(data, levelData.boss_room_id);
            byte triforceRoomLocal = (byte)GetLocalRoomId(data, levelData.triforce_room_id);

            // Write 0x1B (triforce) to the triforce room's slot in RoomItemsUW_extended_custom.
            // The triforce room vertex is type=Meta (not Item) so WriteItems() skips it;
            // we must write 0x1B explicitly so the ASM hook spawns the triforce object.
            // Level 9 has no triforce — Zelda is the goal, not a triforce pickup.
            if (level != 9)
                Write(RoomItemsUWExtendedCustom + level * 0x80 + triforceRoomLocal, [TriforceItemCode]);
            else
                // Level 9's boss room uses special item code 0x0E after Ganon dies.
                // Like the triforce room above, this is not a regular item location, so it
                // must be written explicitly into the generated dungeon item table.
                Write(RoomItemsUWExtendedCustom + level * 0x80 + bossRoomLocal, [0x0E]);

            // +0x00: palettes_transfer_buf (36 bytes)
            Write(levelOffset + 0x00, ValidateByteArray(levelData.palettes_transfer_buf, 36, nameof(levelData.palettes_transfer_buf), level));

            // +0x24: enemy_counts (4 bytes)
            Write(levelOffset + 0x24, ValidateIntArray(levelData.enemy_counts, 4, nameof(levelData.enemy_counts), level));

            // +0x28: start_y
            Write(levelOffset + 0x28, [(byte)levelData.start_y]);

            // +0x29: shortcut_or_item_pos_array (4 bytes)
            Write(levelOffset + 0x29, ValidateIntArray(levelData.shortcut_or_item_pos_array, 4, nameof(levelData.shortcut_or_item_pos_array), level));

            // +0x2D: submenu_map_rotation
            Write(levelOffset + 0x2D, [(byte)levelData.submenu_map_rotation]);

            // +0x2E: status_bar_map_x_offset
            Write(levelOffset + 0x2E, [(byte)levelData.status_bar_map_x_offset]);

            // +0x2F: start_room_id (local)
            Write(levelOffset + 0x2F, [startRoomLocal]);

            // +0x30: triforce_room_id (local)
            Write(levelOffset + 0x30, [triforceRoomLocal]);

            // +0x31-0x32: world_flags_addr (2 bytes)
            Write(levelOffset + 0x31, ValidateIntArray(levelData.world_flags_addr, 2, nameof(levelData.world_flags_addr), level));

            // +0x33: level number
            Write(levelOffset + 0x33, [(byte)level]);

            // +0x34-0x3D: cellar_room_id_array (10 bytes, local room IDs, 0xFF terminated)
            var cellarData = new byte[10];
            Array.Fill(cellarData, (byte)0xFF);
            for (int i = 0; i < Math.Min(levelData.cellar_room_id_array.Length, 10); i++)
            {
                cellarData[i] = (byte)GetLocalRoomId(data, levelData.cellar_room_id_array[i]);
            }
            Write(levelOffset + 0x34, cellarData);

            // +0x3E: boss_room_id (local)
            Write(levelOffset + 0x3E, [bossRoomLocal]);

            // +0x3F-0x4E: submenu_map_mask (16 bytes)
            Write(levelOffset + 0x3F, ValidateIntArray(levelData.submenu_map_mask, 16, nameof(levelData.submenu_map_mask), level));

            // +0x4F-0x7B: status_bar_map_transfer_buf (45 bytes)
            Write(levelOffset + 0x4F, ValidateByteArray(levelData.status_bar_map_transfer_buf, 45, nameof(levelData.status_bar_map_transfer_buf), level));

            // +0x7C-0xDB: palette_cycles (96 bytes)
            Write(levelOffset + 0x7C, ValidateByteArray(levelData.palette_cycles, 96, nameof(levelData.palette_cycles), level));

            // +0xDC: death_palette_series (32 bytes)
            Write(levelOffset + 0xDC, ValidateByteArray(levelData.death_palette_series, 32, nameof(levelData.death_palette_series), level));
        }
    }
}
