namespace Randomizer.Games.Combo;

using MathNet.Numerics.Distributions;
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
    private static readonly Dictionary<(string, string), uint[][]> _portalData = new()
    {
        [("sm", "alttp")] =
            [
            //   door    game    dest    args
                [0xae0c, 0x0001, 0x0200, 0x0000],
                [0xaf0c, 0x0001, 0x0201, 0x0000],
                [0xaf8c, 0x0001, 0x0202, 0x0040],
                [0xb00c, 0x0001, 0x0203, 0x0040]
            ],
        [("sm", "z1")] = [],
        [("sm", "m1")] = [],
        [("alttp", "sm")] =
            [
            //   room    owscrl  game    dest    args
                [0x0122, 0x0035, 0x0000, 0xae00, 0x0000],
                [0x00e5, 0x0003, 0x0000, 0xaf00, 0x0000],
                [0x010e, 0x0077, 0x0000, 0xaf80, 0x0000],
                [0x0115, 0x0070, 0x0000, 0xb000, 0x0000]
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

    public static void WritePortals(IRom rom, World world)
    {
        // sm = 0x300000
        // alttp (in) = 0x550000
        // alttp (out) = 0x552000
        // z1 (int) = 0x63A000
        // z1 (out) = 0x63B000
        // m1 = 0x6C7000

        if (world.WorldConfig!.SuperMetroid != null)
        {
            int address = 0x300000;
            if (world.WorldConfig.Alttp != null)
            {
                address = WritePortals(rom, address, _portalData[("sm", "alttp")]);
                //address = WriteDynamicsPortals(rom, address, world, ("sm", "alttp"));
            }

            if (world.WorldConfig.Zelda1 != null)
            {
                address = WritePortals(rom, address, _portalData[("sm", "z1")]);
            }

            if (world.WorldConfig.Metroid != null)
            {
                address = WritePortals(rom, address, _portalData[("sm", "m1")]);
            }
            rom.Write(address, [0x00, 0x00]);
        }

        if (world.WorldConfig.Alttp != null)
        {
            int address = 0x550000;
            if (world.WorldConfig.SuperMetroid != null)
            {
                address = WritePortals(rom, address, _portalData[("alttp", "sm")]);
                //address = WriteDynamicsPortals(rom, address, world, ("alttp", "sm"));
            }
            if (world.WorldConfig.Zelda1 != null)
            {
                address = WritePortals(rom, address, _portalData[("alttp", "z1")]);
            }
            if (world.WorldConfig.Metroid != null)
            {
                address = WritePortals(rom, address, _portalData[("alttp", "m1")]);
            }
            rom.Write(address, [0x00, 0x00]);
        }

        if (world.WorldConfig.Zelda1 != null)
        {
            int address = 0x63A000;
            if (world.WorldConfig.Alttp != null)
            {
                address = WritePortals(rom, address, _portalData[("z1", "alttp")]);
            }
            if (world.WorldConfig.SuperMetroid != null)
            {
                address = WritePortals(rom, address, _portalData[("z1", "sm")]);
            }
            if (world.WorldConfig.Metroid != null)
            {
                address = WritePortals(rom, address, _portalData[("z1", "m1")]);
            }
            rom.Write(address, [0x00, 0x00]);
        }

        if (world.WorldConfig.Metroid != null)
        {
            int address = 0x6C7000;
            if (world.WorldConfig.Alttp != null)
            {
                address = WritePortals(rom, address, _portalData[("m1", "alttp")]);
            }
            if (world.WorldConfig.SuperMetroid != null)
            {
                address = WritePortals(rom, address, _portalData[("m1", "sm")]);
            }
            if (world.WorldConfig.Zelda1 != null)
            {
                address = WritePortals(rom, address, _portalData[("m1", "z1")]);
            }
            rom.Write(address, [0x00, 0x00]);
        }
    }

    private static int WritePortals(IRom rom, int address, uint[][] portalData)
    {
        if (portalData.Length == 0)
        {
            return address;
        }

        foreach (var portal in portalData)
        {
            foreach (var portalValue in portal)
            {
                rom.Write(address, BitConverter.GetBytes((UInt16)portalValue));
                address += 2;
            }
        }

        return address;
    }

    //TODO: This is broken, we need to fix it later
    private static int WriteDynamicsPortals(IRom rom, int address, World world, (string from, string to) gamePair)
    {
        if (gamePair.from == "sm" && gamePair.to == "alttp")
        {
            int index = 0;
            foreach ((var portalFrom, var portalTo) in world.Portals)
            {
                SuperMetroid.Vertex smPortal;
                Randomizer.Graph.Vertex otherPortal;

                if (portalFrom.World.GameId == "sm")
                {
                    smPortal = (SuperMetroid.Vertex)portalFrom;
                    otherPortal = portalTo;
                }
                else
                {
                    continue;
                }

                //   door    game    dest    args
                //uint doorPtrIn = uint.Parse(smPortal.Node?.NodeAddress?.Substring(2) ?? "0", System.Globalization.NumberStyles.HexNumber) & 0xFFFF;
                //var otherDoor = (SuperMetroid.Vertex)smPortal.Edges.First(x => ((SuperMetroid.Vertex)x.From).RoomId != ((SuperMetroid.Vertex)x.To).RoomId).To;
                //uint doorPtrOut = uint.Parse(otherDoor.Node?.NodeAddress?.Substring(2) ?? "0", System.Globalization.NumberStyles.HexNumber) & 0xFFFF;

                long doorPtr = portalFrom.Addresses![0];

                //var orginalData = _portalData[gamePair][index];
                //var newData = new uint[] { (uint)(doorPtr & 0xFFFF), 1, orginalData[2], orginalData[3] };



                //foreach (var portalValue in newData)
                //{
                //    rom.Write(address, BitConverter.GetBytes(portalValue));
                //    address += 2;
                //}

                index++;
            }
        }
        else if (gamePair.from == "alttp" && gamePair.to == "sm")
        {
            int index = 0;
            foreach ((var portalFrom, var portalTo) in world.Portals)
            {
                SuperMetroid.Vertex smPortal;
                Randomizer.Graph.Vertex otherPortal;

                if (portalFrom.World.GameId == "sm")
                {
                    smPortal = (SuperMetroid.Vertex)portalFrom;
                    otherPortal = portalTo;
                }
                else
                {
                    smPortal = (SuperMetroid.Vertex)portalTo;
                    otherPortal = portalFrom;
                }

                // door    game dest    args
                uint doorPtrIn = uint.Parse(smPortal.Node?.NodeAddress?.Substring(2) ?? "0", System.Globalization.NumberStyles.HexNumber) & 0xFFFF;
                var otherDoor = (SuperMetroid.Vertex)smPortal.Edges.First(x => ((SuperMetroid.Vertex)x.From).RoomId != ((SuperMetroid.Vertex)x.To).RoomId).To;
                uint doorPtrOut = uint.Parse(otherDoor.Node?.NodeAddress?.Substring(2) ?? "0", System.Globalization.NumberStyles.HexNumber) & 0xFFFF;

                var orginalData = _portalData[gamePair][index];
                var newData = new uint[] { orginalData[0], orginalData[1], orginalData[2], doorPtrOut & 0xFFFF, orginalData[4] };

                foreach (var portalValue in newData)
                {
                    rom.Write(address, BitConverter.GetBytes(portalValue));
                    address += 2;
                }

                index++;
            }
        }

        return address;
    }
}
