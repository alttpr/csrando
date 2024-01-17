namespace Randomizer.RomModifications;

using System.Diagnostics.CodeAnalysis;
using Randomizer.Graph;

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

    /// <summary>Returns the map reveal value for the give <paramref name="vertex"/>.</summary>
    /// <remarks>This uses the room id and only works as long as boss locations are vanilla.</remarks>
    public static ushort GetMapReveal(this Vertex? vertex) => vertex?.RoomId switch
    {
        0x00C8 => 0x2000, // eastern palace
        0x0033 => 0x1000, // desert palace
        0x0006 => 0x0400, // swamp palace
        0x005A => 0x0200, // palace of darkness
        0x0090 => 0x0100, // misery mire
        0x0029 => 0x0080, // skull woods
        0x00DE => 0x0040, // ice palace
        0x0007 => 0x0020, // tower of hera
        0x00AC => 0x0010, // thieves town
        0x00A4 => 0x0008, // turtle rock
        _ => 0x0000,
    };
}
