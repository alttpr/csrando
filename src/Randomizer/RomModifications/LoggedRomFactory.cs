namespace Randomizer.RomModifications;

using System.IO;

/// <summary>
/// Factory for creating LoggedRom instances used for generating patches.
/// </summary>
public class LoggedRomFactory : IRomFactory
{
    public IRom CreateRom(FileInfo? baseRom, FileInfo? baseBPS)
    {
        // baseRom is not strictly needed by LoggedRom itself, but might be needed
        // by the calling code later if it intends to generate a combined patch client-side.
        // We don't use baseRom here directly.

        var loggedRom = new LoggedRom();

        // Store base patch if provided
        if (baseBPS != null)
        {
            loggedRom.ApplyBasePatch(baseBPS);
        }

        // Resize is ignored by LoggedRom, so no need to call it.

        return loggedRom;
    }
}
