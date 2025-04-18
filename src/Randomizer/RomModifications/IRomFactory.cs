namespace Randomizer.RomModifications;

using System.IO;

/// <summary>
/// Defines a factory for creating IRom instances.
/// </summary>
public interface IRomFactory
{
    /// <summary>
    /// Creates an instance of a class implementing IRom.
    /// </summary>
    /// <param name="baseRom">FileInfo for the base ROM. Required by some implementations.</param>
    /// <param name="baseBPS">Optional FileInfo for a base BPS patch to apply or store.</param>
    /// <returns>An object implementing the IRom interface.</returns>
    IRom CreateRom(FileInfo? baseRom, FileInfo? baseBPS);
}
