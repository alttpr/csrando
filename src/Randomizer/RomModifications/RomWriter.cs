namespace Randomizer.RomModifications;

using Randomizer.Games.Alttp;
using Randomizer.Graph;

public static class RomWriter
{
    private const int RomSize = 2 * 1024 * 1024;

    public static void Write(Randomizer randomizer, FileInfo baseRom, FileInfo? baseBPS, DirectoryInfo outputDirectory)
    {
        foreach (var (i, world) in randomizer.Worlds.Indexed())
            WriteForWorld(world, baseRom, baseBPS, outputDirectory, randomizer.PRNG, randomizer.Worlds.Length > 1 ? $"_W{i + 1}" : null);
    }

    public static void WriteForWorld(IWorld world, FileInfo baseRom, FileInfo? baseBPS, DirectoryInfo outputDirectory, PRNG prng, string? worldSuffix = null)
    {
        using var rom = new Rom(baseRom.FullName);
        // TODO: check hash? do we need that?

        // assume we either have a vanilla rom and a BPS, or an already pre-patched base rom.
        if (baseBPS != null)
        {
            rom.Resize(RomSize);
            rom.ApplyBasePatch(baseBPS);
        }

        // FIXME: this should be a list of game-specific rom writers, not a hardcoded call to Alttp
        if (world is World alttpWorld)
            Games.Alttp.RomWriter.Write(rom, alttpWorld, prng);

        rom.UpdateChecksum();

        outputDirectory.Create();
        string outputFile = Path.Combine(
            outputDirectory.FullName,
            // TODO: this "alttpr" prefix should probably customizable for other games
            $"alttpr_{world.Config.Glitches}_{world.Config.State}_{world.Config.Goal}_{prng.Seed:x08}{worldSuffix}.sfc");
        rom.Save(outputFile);
    }
}
