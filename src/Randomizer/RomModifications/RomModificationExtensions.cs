namespace Randomizer.RomModifications;

using System.Diagnostics.CodeAnalysis;

internal static class RomModificationExtensions
{
    [return: NotNullIfNotNull(nameof(str))]
    public static string? MaxLength(this string? str, int maxLength)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        if (str.Length < maxLength)
            return str;

        return str[..maxLength];
    }
}
