namespace Randomizer.ConsoleCommands;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Randomizer.Graph;
using Randomizer.RomModifications;

/// <summary>Run randomizer as command.</summary>
internal sealed class Randomize : Command
{
    private static readonly ILogger _logger = ClassLogger.Get();

    private readonly Option<int> _bulk = new(["bulk", "--bulk"], () => 1, "generate multiple ROMs");
    private readonly Option<int> _multiworld = new(["multiworld", "--multiworld"], () => 1, "multiworld player count");
    private readonly Option<int?> _seed = new(["seed", "--seed"], "set starting seed");
    // NOTE: use assemblebaserom to generate a usable preset; the following two options are mainly for testing of external rom changes.
    private readonly Option<FileInfo> _baseRom = new Option<FileInfo>(["rom", "--rom"], "set base rom").ExistingOnly();
    private readonly Option<FileInfo> _baseBPS = new Option<FileInfo>(["bps", "--bps"], "set base rom patch BPS (for use with a vanilla rom)").ExistingOnly();
    private readonly Option<DirectoryInfo> _outputDirectory = new(["outdir", "--outdir"], "output directory for generated games");
    private readonly Option<FileInfo> _settingsFile = new Option<FileInfo>(["settings", "--settings"], "JSON serialized settings file").ExistingOnly();
    private readonly Option<bool> _dumpSpoiler = new(["spoiler", "--spoiler"], "dump spoiler log");

    public Randomize()
        : base("randomize", "Generate a randomized ROM.")
    {
        Add(_bulk);
        Add(_multiworld);
        Add(_seed);
        Add(_baseRom);
        Add(_baseBPS);
        Add(_outputDirectory);
        Add(_settingsFile);
        Add(_dumpSpoiler);

        AddValidator(Validate);

        this.SetHandler(context => context.ExitCode = Handle(context));
    }

    private void Validate(CommandResult result)
    {
        List<string> errors = [];

        if (result.GetValueForOption(_multiworld) <= 0)
            errors.Add("Multiworld player count needs to be at least 1");

        if (result.GetValueForOption(_bulk) <= 0)
            errors.Add("Bulk count needs to be at least 1");

        result.ErrorMessage = string.Join('\n', errors);
    }

    /// <summary>Execute the console command.</summary>
    public int Handle(InvocationContext context)
    {
        int bulk = Math.Max(context.ParseResult.GetValueForOption(_bulk), 1);
        var baseRom = context.ParseResult.GetValueForOption(_baseRom);
        var baseBPS = context.ParseResult.GetValueForOption(_baseBPS);
        var outputDirectory = context.ParseResult.GetValueForOption(_outputDirectory);
        bool dumpSpoiler = context.ParseResult.GetValueForOption(_dumpSpoiler);

        if (baseRom == null && File.Exists(Config.BaseRomFile))
            baseRom = new FileInfo(Config.BaseRomFile);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < bulk; i++)
        {
            var worldConfigs = GetWorldConfigs(context);
            var randomizer = new Randomizer(
                worldConfigs,
                context.ParseResult.GetValueForOption(_seed)
            );
            randomizer.Randomize();
            if (!randomizer.IsWinnable())
                throw new Exception($"Game Unwinnable.");

            if (outputDirectory != null)
            {
                if (baseRom != null && outputDirectory != null)
                    RomWriter.Write(randomizer, baseRom, baseBPS, outputDirectory);
                else
                    _logger.LogError("Writing a ROM requires all options: {RequiredOptions}", string.Join(", ", [_baseRom.Name, _outputDirectory.Name]));
            }
            if (dumpSpoiler)
            {
                Console.WriteLine("{0}", JsonSerializer.Serialize(randomizer.SpoilerLog!.Spoiler, typeof(Dictionary<string, Dictionary<string, string>>), Config.JsonStaticContext));
            }
        }
        _logger.LogInformation("Randomization took {TimeElapsed}", sw.Elapsed);
        return 0;
    }

    private WorldConfig[] GetWorldConfigs(InvocationContext context)
    {
        var settingsFile = context.ParseResult.GetValueForOption(_settingsFile);
        if (settingsFile == null && File.Exists(Config.SettingsFile))
            settingsFile = new FileInfo(Config.SettingsFile);

        Console.WriteLine("settingsFile: {0}", settingsFile);

        if (settingsFile != null && settingsFile.Exists)
        {
            using var settingsStream = settingsFile.OpenRead();
            using var settingsReader = new StreamReader(settingsStream);
            var settingsString = settingsReader.ReadToEnd();
            try
            {
                // try to read an array first; one entry per world (for multiworld)
                var configArray = JsonSerializer.Deserialize(settingsString, typeof(WorldConfig[]), Config.JsonStaticContext) as WorldConfig[];
                if (configArray != null)
                {
                    _logger.LogInformation("Read {WorldConfigCount} worlds from passed config file.", configArray.Length);
                    return configArray;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }
            try
            {
                settingsStream.Seek(0, SeekOrigin.Begin);
                // try to read a single config, duplicate for multiworld as necessary
                var singleConfig = JsonSerializer.Deserialize(settingsString, typeof(WorldConfig), Config.JsonStaticContext) as WorldConfig;
                if (singleConfig != null)
                {
                    _logger.LogInformation("Read single world from passed config file.");
                    return Enumerable.Repeat(singleConfig, context.ParseResult.GetValueForOption(_multiworld)).ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }
        }

        throw new InvalidOperationException("No usable settings file passed. Either use the --settings option or place a valid file at data/settings.json.");
    }
}
