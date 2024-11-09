namespace Randomizer.RomModifications;

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

        if (world is Games.Alttp.World alttpWorld)
            Games.Alttp.RomWriter.Write(rom, alttpWorld, prng);
        else if (world is Games.Zelda1.World zelda1World)
            Games.Zelda1.RomWriter.Write(rom, zelda1World, prng);

        rom.UpdateChecksum();

        outputDirectory.Create();
        string outputFile = Path.Combine(
            outputDirectory.FullName,
            world is Games.Alttp.World 
                ? $"alttpr_{world.WorldConfig.Alttp.Glitches}_{world.WorldConfig.Alttp.State}_{world.WorldConfig.Alttp.Goal}_{prng.Seed:x08}{worldSuffix}.sfc"
                : $"z1r_{world.WorldConfig.Zelda1.EntranceShuffle}_{prng.Seed:x08}{worldSuffix}.nes");

        rom.Save(outputFile);
    }
}
