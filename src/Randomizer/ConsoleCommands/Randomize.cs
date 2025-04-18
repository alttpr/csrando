namespace Randomizer.ConsoleCommands;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Randomizer.Graph; // WorldConfig is likely here
using Randomizer.RomModifications;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http; // Added for Results

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
    private readonly Option<bool> _apiMode = new(["api-mode", "--api-mode"], () => false, "run as an API server"); // New option

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
        Add(_apiMode); // Add the new option

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
        if (context.ParseResult.GetValueForOption(_apiMode))
        {
            _logger.LogInformation("Starting in API mode...");
            var baseBPS = context.ParseResult.GetValueForOption(_baseBPS);
            Api.ApiServer.Start(_logger, baseBPS, GetDefaultWorldConfigs);
            return 0; // API server runs asynchronously, main thread can exit
        }
        else
        {
            // Existing CLI logic
            int bulk = Math.Max(context.ParseResult.GetValueForOption(_bulk), 1);
            var baseRom = context.ParseResult.GetValueForOption(_baseRom);
            var baseBPS = context.ParseResult.GetValueForOption(_baseBPS);
            var outputDirectory = context.ParseResult.GetValueForOption(_outputDirectory);
            bool dumpSpoiler = context.ParseResult.GetValueForOption(_dumpSpoiler);

            // Instantiate the FileRomFactory for CLI usage
            var romFactory = new FileRomFactory();

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < bulk; i++)
            {
                var worldConfigs = GetWorldConfigs(context);
                var randomizer = RandomizerFactory.Create(
                    worldConfigs,
                    context.ParseResult.GetValueForOption(_seed),
                    romFactory // Pass the factory instance
                );

                randomizer.Randomize();
                if (!randomizer.IsWinnable())
                    throw new Exception($"Game Unwinnable.");

                if (outputDirectory != null)
                {
                    // Use the provided baseRom/baseBPS first, then try ProvideBaseRom
                    var actualBaseRom = baseRom ?? randomizer.ProvideBaseRom();
                    if (actualBaseRom != null)
                        randomizer.Write(actualBaseRom, baseBPS, outputDirectory);
                    else
                        _logger.LogError("Writing a ROM requires a base ROM. Provide one via --rom or ensure ProvideBaseRom() returns a valid path.");
                }
                if (dumpSpoiler)
                {
                    Console.WriteLine("{0}", JsonSerializer.Serialize(randomizer.SpoilerLog!.Spoiler, new JsonSerializerOptions
                    {
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        WriteIndented = true
                    }));
                }
            }
            _logger.LogInformation("Randomization took {TimeElapsed}", sw.Elapsed);
            return 0;
        }
    }

    // Placeholder for request/response models - Define these properly
    private record RandomizeRequest(int? Seed, bool IncludeSpoiler /* Add other options like settings */);
    // Updated PatchData type to string for Base64 representation
    private record RandomizeResponse(string Seed, object? SpoilerLog, string PatchData);

    // Placeholder for getting default configs - Adapt GetWorldConfigs or create new logic
    private WorldConfig[] GetDefaultWorldConfigs()
    {
        // This needs to load/create default WorldConfig(s)
        // Potentially load from embedded resource or a default file path
        _logger.LogWarning("Using placeholder default world configs for API mode.");
        // Example: Load from default settings file if available
        var defaultSettingsPath = Path.Combine(AppContext.BaseDirectory, "data", "settings.json");
        if (File.Exists(defaultSettingsPath))
        {
            using var settingsStream = File.OpenRead(defaultSettingsPath);
            try
            {
                var singleConfig = JsonSerializer.Deserialize<WorldConfig>(settingsStream, _options);
                if (singleConfig != null) return [singleConfig];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load default settings file for API.");
            }
        }
        // Fallback to a very basic default if file loading fails
        return [new WorldConfig()]; // Assuming WorldConfig() is accessible
    }

    private WorldConfig[] GetWorldConfigs(InvocationContext context)
    {
        var settingsFile = context.ParseResult.GetValueForOption(_settingsFile);
        if (settingsFile == null && File.Exists(Config.SettingsFile))
            settingsFile = new FileInfo(Config.SettingsFile);
        if (settingsFile != null && settingsFile.Exists)
        {
            using var settingsStream = settingsFile.OpenRead();
            try
            {
                // try to read an array first; one entry per world (for multiworld)
                var configArray = JsonSerializer.Deserialize<WorldConfig[]>(settingsStream, _options);
                if (configArray != null)
                {
                    _logger.LogInformation("Read {WorldConfigCount} worlds from passed config file.", configArray.Length);
                    return configArray;
                }
            }
            catch { }
            try
            {
                settingsStream.Seek(0, SeekOrigin.Begin);
                // try to read a single config, duplicate for multiworld as necessary
                var singleConfig = JsonSerializer.Deserialize<WorldConfig>(settingsStream, _options);
                if (singleConfig != null)
                {
                    _logger.LogInformation("Read single world from passed config file.");
                    return Enumerable.Repeat(singleConfig, context.ParseResult.GetValueForOption(_multiworld)).ToArray();
                }
            }
            catch { }
        }

        throw new InvalidOperationException("No usable settings file passed. Either use the --settings option or place a valid file at data/settings.json.");
    }

    private static readonly JsonSerializerOptions _options = new()
    {
        AllowTrailingCommas = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };
}
