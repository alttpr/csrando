namespace Randomizer.ConsoleCommands;

using System.CommandLine;
using System.Diagnostics;
using System.Runtime.InteropServices;

internal sealed class AssembleBaseRom : Command
{
    private readonly Option<FileInfo> _jpRom = new(["-r", "--rom"], "Vanilla JP1.0 ROM file") { IsRequired = true };
    private readonly Option<bool> _featurePatreonSupporters = new(["-p", "--feature-patreon-supporters"], "Feature Patreon supporters in the generated ROM");

    public AssembleBaseRom()
        : base("assemblebaserom", "Assemble the randomizer base rom")
    {
        Add(_jpRom);
        Add(_featurePatreonSupporters);

        this.SetHandler(Assemble, _jpRom, _featurePatreonSupporters);
    }
    private void Assemble(FileInfo jpRom, bool featurePatreonSupporters)
    {
        // TODO: we probably want to get this (and/or the detail paths) from a config file later
        string rootDirectory = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location)!, "../../../../..");

        string asmDirectory = Path.Combine(rootDirectory, "asm");
        string dataDirectory = Path.Combine(rootDirectory, "data");
        string baseRomFile = Path.Combine(dataDirectory, "randomizer.sfc");

        Directory.CreateDirectory(dataDirectory);
        jpRom.CopyTo(baseRomFile, overwrite: true);

        string asar = 0 switch
        {
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "windows/asar.exe",
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "linux/asar",
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "macos/asar",
            _ => throw new Exception("Unsupported operating system"),
        };

        string asarPath = Path.Combine(asmDirectory, "bin", asar);
        var asarArgs = new List<string>
        {
            Path.Combine(asmDirectory, "LTTP_RND_GeneralBugfixes.asm"),
            baseRomFile,
        };

        if (featurePatreonSupporters)
            asarArgs.Insert(0, "-DFEATURE_PATREON_SUPPORTERS=1");

        using var _ = Process.Start(new ProcessStartInfo(asarPath, asarArgs)
        {
            WorkingDirectory = asmDirectory,
        });
    }
}
