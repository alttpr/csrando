namespace Randomizer.ConsoleCommands;

using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BpsNet;

internal sealed class AssembleBaseRoms : Command
{
    private readonly Option<bool> _assembleAlttp = new(["-a", "--assemble-alttp"], "Assemble the ALTTP base rom");
    private readonly Option<bool> _assembleCombo = new(["-c", "--assemble-combo"], "Assemble the Combo base rom");
    private readonly Option<bool> _createBpsPatch = new(["-b", "--create-bps-patch"], "Create a BPS patch and copy to src/web/static");

    private readonly Option<FileInfo> _z3Rom = new(["-r", "--z3-rom"], "Vanilla JP1.0 ALTTP ROM file");
    private readonly Option<FileInfo> _z1Rom = new(["-z", "--z1-rom"], "Vanilla PRG0 Zelda 1 ROM file");
    private readonly Option<FileInfo> _m1Rom = new(["-m", "--m1-rom"], "Vanilla Metroid 1 ROM file");
    private readonly Option<FileInfo> _smRom = new(["-s", "--sm-rom"], "Vanilla (US/JP) Super Metroid ROM file");

    private readonly Option<bool> _featurePatreonSupporters = new(["-p", "--feature-patreon-supporters"], "Feature Patreon supporters in the generated ROM");

    public AssembleBaseRoms()
        : base("assemblebaseroms", "Assemble the randomizer base roms")
    {
        Add(_z3Rom);
        Add(_z1Rom);
        Add(_m1Rom);
        Add(_smRom);
        Add(_assembleAlttp);
        Add(_assembleCombo);
        Add(_createBpsPatch);
        Add(_featurePatreonSupporters);

        this.SetHandler(Assemble, _z3Rom, _z1Rom, _m1Rom, _smRom, _assembleAlttp, _assembleCombo, _createBpsPatch, _featurePatreonSupporters);
    }

    private void Assemble(FileInfo z3Rom, FileInfo z1Rom, FileInfo m1Rom, FileInfo smRom, bool assembleAlttp, bool assembleCombo, bool createBpsPatch, bool featurePatreonSupporters)
    {
        if (assembleAlttp)
        {
            if (z3Rom is null)
            {
                Console.WriteLine("You must provide a JP1.0 ALTTP ROM with --z3-rom to assemble the ALTTP base rom.");
                return;
            }
            AssembleAlttpBaseRom(z3Rom, featurePatreonSupporters);
            if (createBpsPatch)
            {
                string randomizerPatchFile = Path.Combine(Config.DataDirectory, "randomizer.bps");
                CreateBPSPatch([z3Rom], Config.BaseRomFile, randomizerPatchFile);

                string webStaticDirectory = Path.Combine(Config.RootDirectory, "src", "web", "static");
                File.Copy(randomizerPatchFile, Path.Combine(webStaticDirectory, "alttpr.bps"), overwrite: true);
            }
        }

        if (assembleCombo)
        {
            if (z3Rom is null || z1Rom is null || m1Rom is null || smRom is null)
            {
                Console.WriteLine("You must provide a Zelda 3 ROM with --z3-rom, a Zelda 1 ROM with --z1-rom, a Metroid 1 ROM with --m1-rom, and a Super Metroid ROM with --sm-rom to assemble the Combo base rom.");
                return;
            }
            AssembleComboBaseRom(z3Rom, z1Rom, m1Rom, smRom);
            if (createBpsPatch)
            {
                string comboPatchFile = Path.Combine(Config.DataDirectory, "combo.bps");
                CreateBPSPatch([smRom, z3Rom, m1Rom, z1Rom], Config.ComboBaseRomFile, comboPatchFile);

                string webStaticDirectory = Path.Combine(Config.RootDirectory, "src", "web", "static");
                File.Copy(comboPatchFile, Path.Combine(webStaticDirectory, "combo.bps"), overwrite: true);
            }
        }
    }

    private void AssembleAlttpBaseRom(FileInfo z3Rom, bool featurePatreonSupporters)
    {
        string asmDirectory = Path.Combine(Config.AsmDirectory, "z3randomizer");
        string baseRomFile = Config.BaseRomFile;

        z3Rom.CopyTo(baseRomFile, overwrite: true);

        var asarArgs = new List<string>
        {
            Path.Combine(asmDirectory, "LTTP_RND_GeneralBugfixes.asm"),
            baseRomFile,
        };

        if (featurePatreonSupporters)
            asarArgs.Insert(0, "-DFEATURE_PATREON_SUPPORTERS=1");

        RunAsar(asmDirectory, asarArgs);
    }

    private void AssembleComboBaseRom(FileInfo z3Rom, FileInfo z1Rom, FileInfo m1Rom, FileInfo smRom)
    {
        string asmDirectory = Path.Combine(Config.AsmDirectory, "multirando-asm");
        string asarDirectory = Path.Combine(Config.AsmDirectory, "z3randomizer");
        string baseRomFile = Config.ComboBaseRomFile;

        Directory.CreateDirectory(Config.DataDirectory);
        string? baseRomDirectory = Path.GetDirectoryName(baseRomFile);
        if (!string.IsNullOrEmpty(baseRomDirectory))
        {
            Directory.CreateDirectory(baseRomDirectory);
        }

        string resourcesDirectory = Path.Combine(asmDirectory, "resources");
        Directory.CreateDirectory(resourcesDirectory);

        z3Rom.CopyTo(Path.Combine(resourcesDirectory, "zelda3.sfc"), overwrite: true);
        z1Rom.CopyTo(Path.Combine(resourcesDirectory, "zelda1prg0.nes"), overwrite: true);
        m1Rom.CopyTo(Path.Combine(resourcesDirectory, "metroid.nes"), overwrite: true);
        smRom.CopyTo(Path.Combine(resourcesDirectory, "sm.sfc"), overwrite: true);

        var asarArgs = new List<string>
        {
            Path.Combine(asmDirectory, "src/main.asm"),
            baseRomFile,
        };

        RunAsar(asmDirectory, asarArgs, asarDirectory);
    }

    private static string GetAsarExecutable()
    {
        return 0 switch
        {
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => Path.Combine("windows", "asar.exe"),
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => Path.Combine("linux", "asar"),
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => Path.Combine("macos", "asar"),
            _ => throw new InvalidOperationException("Unsupported operating system"),
        };
    }

    private void RunAsar(string workingDirectory, IEnumerable<string> asarArgs, string? asarRootDirectory = null)
    {
        string asarExecutable = GetAsarExecutable();
        string asarDirectory = asarRootDirectory ?? workingDirectory;
        string asarPath = Path.Combine(asarDirectory, "bin", asarExecutable);

        var argumentList = asarArgs as IList<string> ?? asarArgs.ToList();

        using var process = Process.Start(new ProcessStartInfo(asarPath, argumentList)
        {
            WorkingDirectory = workingDirectory,
        });

        if (process is null)
        {
            throw new InvalidOperationException($"Failed to start asar at '{asarPath}'.");
        }

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Asar exited with code {process.ExitCode}.");
        }
    }

    private void CreateBPSPatch(FileInfo[] originalFiles, string modifiedFile, string outputPatchFile)
    {
        // If we get an array of input filenames, we need to combine them into a single original file in the order they were given
        byte[] original;
        using (var ms = new MemoryStream())
        {
            foreach (var originalFile in originalFiles)
            {
                originalFile.OpenRead().CopyTo(ms);
            }
            original = ms.ToArray();
        }

        byte[] modified = File.ReadAllBytes(modifiedFile);
        var patch = BpsPatch.Create(original, modified, "");
        byte[] patchBytes = patch.GetBytes();
        File.WriteAllBytes(outputPatchFile, patchBytes);
    }
}
