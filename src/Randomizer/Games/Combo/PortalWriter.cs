namespace Randomizer.Games.Combo;

using System;
using Randomizer.RomModifications;

/// <summary>
/// Writes every game's cross-game transition table from the world's
/// <see cref="World.PortalConnections"/>. Each connection contributes one row to each of
/// its two sides' tables (see <see cref="PortalSide"/> for the row shape); each present
/// game's table ends with a $0000 terminator. Rows are grouped by partner game in a fixed
/// order to match the layout the original hardcoded tables used.
/// </summary>
internal class PortalWriter
{
    private static readonly (string GameId, int TableAddress, string[] PartnerOrder)[] Tables =
    [
        ("sm",    0x300000, ["alttp", "z1", "m1"]),
        ("alttp", 0x550000, ["sm", "z1", "m1"]),
        ("z1",    0x63A000, ["alttp", "sm", "m1"]),
        ("m1",    Metroid.RomWriter.TransitionTableAddress, ["alttp", "sm", "z1"]),
    ];

    public static void WritePortals(IRom rom, World world)
    {
        foreach (var (gameId, tableAddress, partnerOrder) in Tables)
        {
            if (!GamePresent(world, gameId))
                continue;

            int address = tableAddress;
            foreach (var partner in partnerOrder)
            {
                foreach (var connection in world.PortalConnections)
                {
                    uint[]? row = null;
                    if (connection.A.GameId == gameId && connection.B.GameId == partner)
                        row = connection.A.RowTo(connection.B);
                    else if (connection.B.GameId == gameId && connection.A.GameId == partner)
                        row = connection.B.RowTo(connection.A);

                    if (row == null)
                        continue;

                    foreach (var value in row)
                    {
                        rom.Write(address, BitConverter.GetBytes((ushort)value));
                        address += 2;
                    }
                }
            }

            rom.Write(address, [0x00, 0x00]);
        }
    }

    private static bool GamePresent(World world, string gameId) => gameId switch
    {
        "sm" => world.WorldConfig.SuperMetroid != null,
        "alttp" => world.WorldConfig.Alttp != null,
        "z1" => world.WorldConfig.Zelda1 != null,
        "m1" => world.WorldConfig.Metroid != null,
        _ => false
    };
}
