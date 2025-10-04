namespace Randomizer.RomModifications;

using Randomizer.Graph;

public sealed class FileRomBroker(FileInfo baseRom, FileInfo? baseBPS, DirectoryInfo outputDirectory) : IRomBroker
{
    public IRom CreateRom(GameRandomizer randomizer)
    {
        var rom = new FileRom(baseRom.FullName);
        if (baseBPS != null && baseBPS.Exists)
            randomizer.ApplyPatch(rom, baseBPS);
        return rom;
    }

    public void SaveRom(IRom rom, string suggestedFileName)
    {
        if (rom is not FileRom fileRom)
            throw new ArgumentException("Rom must be a FileRom", nameof(rom));

        outputDirectory.Create();
        string outputFile = Path.Combine(outputDirectory.FullName, suggestedFileName);
        fileRom.Save(outputFile);
    }
}
