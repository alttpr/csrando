namespace Randomizer.Games.Combo;

using Randomizer.RomModifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

internal class PortalWriter
{
    // TODO: Instead of hardcoding the portal data, we should generate it depending on the graph configuration for portals or something similar
    private static Dictionary<(string, string), uint[][]> _portalData = new()
    {
        [("sm", "alttp")] =
            [
            //   door    game    dest    args
                [0x8976, 0x0001, 0x0200, 0x0000],
                [0x9306, 0x0001, 0x0201, 0x0000],
                [0xa8f4, 0x0001, 0x0202, 0x0040],
                [0xae00, 0x0001, 0x0203, 0x0040]
            ],
        [("sm", "z1")] = [],
        [("sm", "m1")] = [],
        [("alttp", "sm")] =
            [
            //   room    owscrl  game    dest    args
                [0x0122, 0x0035, 0x0000, 0x8bce, 0x0000],
                [0x00e5, 0x0003, 0x0000, 0x97c2, 0x0000],
                [0x010e, 0x0077, 0x0000, 0xa894, 0x0000],
                [0x0115, 0x0070, 0x0000, 0xae0c, 0x0000]
            ],
        [("alttp", "m1")] =
            [
            //   room    owscrl  game    dest    args
                [0x011f, 0x0002, 0x0003, 0x0c0c, 0x0000]
            ],
        [("alttp", "z1")] =
            [
            //   room    owscrl  game    dest    args
                [0x0122, 0x0011, 0x0002, 0x0066, 0x0003]
            ],
        [("z1", "alttp")] =
            [
            //   room    game    dest    args
                [0x0066, 0x0001, 0x0220, 0x0000]
            ],
        [("z1", "sm")] = [],
        [("z1", "m1")] = [],
        [("m1", "alttp")] =
            [
            //   room    dir     game    dest    args
                [0x0b0c, 0x0004, 0x0001, 0x0210, 0x0000]
            ],
        [("m1", "sm")] = [],
        [("m1", "z1")] = [],

    };

    public static void WritePortals(RomModifications.Rom rom, World world)
    {
        // sm = 0x300000
        // alttp (in) = 0x550000
        // alttp (out) = 0x552000
        // z1 (int) = 0x63A000
        // z1 (out) = 0x63B000
        // m1 = 0x6C7000

        if (world.Config.Games!.SuperMetroid != null)
        {
            int address = 0x300000;
            if (world.Config.Games.Alttp != null)
            {
                address = WritePortals(rom, address, _portalData[("sm", "alttp")]);
            }

            if (world.Config.Games.Zelda1 != null)
            {
                address = WritePortals(rom, address, _portalData[("sm", "z1")]);
            }

            if (world.Config.Games.Metroid != null)
            {
                address = WritePortals(rom, address, _portalData[("sm", "m1")]);
            }
            rom.Write(address, [0x00, 0x00]);
        }

        if (world.Config.Games.Alttp != null)
        {
            int address = 0x550000;
            if (world.Config.Games.SuperMetroid != null)
            {
                address = WritePortals(rom, address, _portalData[("alttp", "sm")]);
            }
            if (world.Config.Games.Zelda1 != null)
            {
                address = WritePortals(rom, address, _portalData[("alttp", "z1")]);
            }
            if (world.Config.Games.Metroid != null)
            {
                address = WritePortals(rom, address, _portalData[("alttp", "m1")]);
            }
            rom.Write(address, [0x00, 0x00]);
        }

        if (world.Config.Games.Zelda1 != null)
        {
            int address = 0x63A000;
            if (world.Config.Games.Alttp != null)
            {
                address = WritePortals(rom, address, _portalData[("z1", "alttp")]);
            }
            if (world.Config.Games.SuperMetroid != null)
            {
                address = WritePortals(rom, address, _portalData[("z1", "sm")]);
            }
            if (world.Config.Games.Metroid != null)
            {
                address = WritePortals(rom, address, _portalData[("z1", "m1")]);
            }
            rom.Write(address, [0x00, 0x00]);
        }

        if (world.Config.Games.Metroid != null)
        {
            int address = 0x6C7000;
            if (world.Config.Games.Alttp != null)
            {
                address = WritePortals(rom, address, _portalData[("m1", "alttp")]);
            }
            if (world.Config.Games.SuperMetroid != null)
            {
                address = WritePortals(rom, address, _portalData[("m1", "sm")]);
            }
            if (world.Config.Games.Zelda1 != null)
            {
                address = WritePortals(rom, address, _portalData[("m1", "z1")]);
            }
            rom.Write(address, [0x00, 0x00]);
        }
    }

    private static int WritePortals(RomModifications.Rom rom, int address, uint[][] portalData)
    {
        if(portalData.Length == 0)
        {
            return address;
        }

        foreach (var portal in portalData)
        {
            foreach (var portalValue in portal)
            {
                rom.Write(address, BitConverter.GetBytes(portalValue));
                address += 2;
            }
        }

        return address;
    }
}
