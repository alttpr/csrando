namespace Randomizer.RomModifications;

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

    /// <summary>Returns the underlying byte-length of the passed UTF-8 string.</summary>
    public static int GetRuneLength(this string? str)
    {
        if (string.IsNullOrEmpty(str))
            return 0;

        return str.EnumerateRunes().Sum(rune => rune.Utf8SequenceLength);
    }
}
