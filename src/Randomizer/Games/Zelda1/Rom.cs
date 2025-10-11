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
}
