namespace Randomizer.RomModifications;

using System.IO;

/// <summary>
/// Factory for creating standard file-based Rom instances.
/// </summary>
public class FileRomFactory : IRomFactory
{
    private const int DefaultRomSize = 8 * 1024 * 1024; // Example default size, adjust as needed

    public IRom CreateRom(FileInfo? baseRom, FileInfo? baseBPS)
    {
        if (baseRom == null)
        {
            throw new ArgumentNullException(nameof(baseRom), "A base ROM file is required for FileRomFactory.");
        }

        var rom = new Rom(baseRom.FullName);

        // Apply base patch if provided
        if (baseBPS != null)
        {
            // Determine the expected size - this might need game-specific logic
            // For now, using a default. Consider making size configurable or passed in.
            rom.Resize(DefaultRomSize);
            rom.ApplyBasePatch(baseBPS);
        }

        return rom;
    }
}
