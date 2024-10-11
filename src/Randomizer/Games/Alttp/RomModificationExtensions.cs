namespace Randomizer.Games.Alttp;

using System.Diagnostics.CodeAnalysis;

internal static class RomModificationExtensions
{
    /// <summary>Returns at most <paramref name="maxLength"/> characters from <paramref name="str"/>.</summary>
    [return: NotNullIfNotNull(nameof(str))]
    public static string? MaxLength(this string? str, int maxLength)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        if (str.Length < maxLength)
            return str;

        return str[..maxLength];
    }

    /// <summary>Returns the map reveal value for the given <paramref name="vertex"/>.</summary>
    public static ushort GetMapReveal(this Vertex? vertex) => vertex.GetDungeonFromBossRoom() switch
    {
        Dungeon.EasternPalace => 0x2000,
        Dungeon.DesertPalace => 0x1000,
        Dungeon.SwampPalace => 0x0400,
        Dungeon.PalaceOfDarkness => 0x0200,
        Dungeon.MiseryMire => 0x0100,
        Dungeon.SkullWoods => 0x0080,
        Dungeon.IcePalace => 0x0040,
        Dungeon.TowerOfHera => 0x0020,
        Dungeon.ThievesTown => 0x0010,
        Dungeon.TurtleRock => 0x0008,
        _ => 0x0000,
    };

    /// <summary>Returns the addresses where music is written to for a given <paramref name="vertex"/>.</summary>
    public static int[]? GetDungeonMusicAddresses(this Vertex? vertex) => vertex.GetDungeonFromBossRoom() switch
    {
        Dungeon.EasternPalace => [0x1559A],
        Dungeon.DesertPalace => [0x1559B, 0x1559C, 0x1559D, 0x1559E],
        Dungeon.SwampPalace => [0x155B7],
        Dungeon.PalaceOfDarkness => [0x155B8],
        Dungeon.MiseryMire => [0x155B9],
        Dungeon.SkullWoods => [0x155BA, 0x155BB, 0x155BC, 0x155BD, 0x15608, 0x15609, 0x1560A, 0x1560B],
        Dungeon.IcePalace => [0x155BF],
        Dungeon.TowerOfHera => [0x155C5, 0x1107A, 0x10B8C],
        Dungeon.ThievesTown => [0x155C6],
        Dungeon.TurtleRock => [0x155C7, 0x155A7, 0x155AA, 0x155AB],
        _ => null,
    };

    /// <summary>
    /// Returns the <see cref="Dungeon"/> this boss room is in.
    /// Doesn't return anything for non-boss rooms.
    /// </summary>
    /// <remarks>
    /// This only includes dungeons that give a prize (Pendant or Crystal).
    /// Because it uses the room id, it only works as long as boss locations are vanilla.
    /// </remarks>
    internal static Dungeon GetDungeonFromBossRoom(this Vertex? bossRoom) => bossRoom?.RoomId switch
    {
        0x00C8 => Dungeon.EasternPalace, // eastern palace
        0x0033 => Dungeon.DesertPalace, // desert palace
        0x0006 => Dungeon.SwampPalace, // swamp palace
        0x005A => Dungeon.PalaceOfDarkness, // palace of darkness
        0x0090 => Dungeon.MiseryMire, // misery mire
        0x0029 => Dungeon.SkullWoods, // skull woods
        0x00DE => Dungeon.IcePalace, // ice palace
        0x0007 => Dungeon.TowerOfHera, // tower of hera
        0x00AC => Dungeon.ThievesTown, // thieves town
        0x00A4 => Dungeon.TurtleRock, // turtle rock
        _ => Dungeon.None,
    };

    [return: NotNullIfNotNull(nameof(vertex))]
    internal static string? GetRegion(this Vertex? vertex, string language)
    {
        if (vertex == null)
            return null;

        var regionNames = YamlReader.LoadRegions(language);
        if (regionNames.TryGetValue(vertex.Name, out var region))
            return region;

        if (!vertex.Name.Contains(" - "))
        {
            if (vertex.MoonPearl == true)
                return vertex.World.WorldConfig.Alttp!.State == StateOption.Inverted ? "Light World" : "Dark World";
            else
                return vertex.World.WorldConfig.Alttp!.State == StateOption.Inverted ? "Dark World" : "Light World";
        }
        // TODO: this only works because of our naming convention "Region - Location"; preferably the region file has a match already.
        return vertex.Name.Split(" - ")[0];
    }
}

// TODO: this (and associated methods) should be data, get rid of those.
internal enum Dungeon
{
    None = 0,
    EasternPalace,
    DesertPalace,
    TowerOfHera,
    PalaceOfDarkness,
    SwampPalace,
    SkullWoods,
    ThievesTown,
    IcePalace,
    MiseryMire,
    TurtleRock,
}
