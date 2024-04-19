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

    private readonly Option<GoalOption> _goal = new("goal", () => GoalOption.Ganon, "set game goal");
    private readonly Option<StateOption> _state = new("state", () => StateOption.Open, "set game state");
    private readonly Option<WeaponOption> _weapons = new("weapons", () => WeaponOption.Randomized, "set weapons mode");
    private readonly Option<GlitchesOption> _glitches = new("glitches", () => GlitchesOption.None, "set glitches");
    private readonly Option<AccessibilityOption> _accessibility = new("accessibility", "set item/location accessibility");
    private readonly Option<BossShuffleOption> _bossShuffle = new("bossshuffle", () => BossShuffleOption.None, "set boss shuffle mode");
    private readonly Option<EntranceShuffleOption> _entranceShuffle = new("entrance", () => EntranceShuffleOption.None, "set entrance shuffle mode");
    private readonly Option<ShopSupplyOption> _shopSupply = new("shopsupply", () => ShopSupplyOption.Normal, "set shop supply shuffle mode");
    private static readonly string[] _crystalAmount = ["random", "0", "1", "2", "3", "4", "5", "6", "7"];
    private static readonly int[] _defaultCrystals = [7];
    private readonly Option<int[]> _crystalsGanon = new Option<int[]>("crystals_ganon", ParseCrystalCount, description: "set ganon crystal requirement") { AllowMultipleArgumentsPerToken = true }.FromAmong(_crystalAmount);
    private readonly Option<int[]> _crystalsTower = new Option<int[]>("crystals_tower", ParseCrystalCount, description: "set ganon tower crystal requirement") { AllowMultipleArgumentsPerToken = true }.FromAmong(_crystalAmount);
    private readonly Option<List<TechOption>> _tech = new Option<List<TechOption>>("tech", "set allowed techs").FromAmong(Enum.GetNames(typeof(TechOption)));
    private readonly Option<List<string>> _startingItems = new("items", "set starting items (comma separated)");
    private readonly Option<int> _bulk = new("bulk", () => 1, "generate multiple ROMs");
    private readonly Option<int> _multiworld = new("multiworld", () => 1, "multiworld player count");
    private readonly Option<int?> _seed = new("seed", "set starting seed");
    private readonly Option<FileInfo> _baseRom = new Option<FileInfo>("rom", "set base rom").ExistingOnly();
    // TODO: we should probably have the base rom patch "built in" and not require a path.
    private readonly Option<FileInfo> _baseBPS = new Option<FileInfo>("bps", "set base rom patch BPS (for use with a vanilla rom)").ExistingOnly();
    private readonly Option<DirectoryInfo> _outputDirectory = new("outdir", "output directory for generated games");
    private readonly Option<FileInfo> _settingsFile = new Option<FileInfo>("settings", "JSON serialized settings file").ExistingOnly();
    private readonly Option<bool> _dumpSpoiler = new("spoiler", "dump spoiler log");

    public Randomize()
        : base("randomize", "Generate a randomized ROM.")
    {
        Add(_goal);
        Add(_state);
        Add(_weapons);
        Add(_glitches);
        Add(_accessibility);
        Add(_bossShuffle);
        Add(_entranceShuffle);
        Add(_shopSupply);
        Add(_crystalsGanon);
        _crystalsGanon.SetDefaultValue(_defaultCrystals);
        Add(_crystalsTower);
        _crystalsTower.SetDefaultValue(_defaultCrystals);
        Add(_tech);
        Add(_startingItems);
        _startingItems.AllowMultipleArgumentsPerToken = true;
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

    private static int[] ParseCrystalCount(ArgumentResult result)
    {
        // option not specified: default to 7
        if (!result.Tokens.Any())
            return [7];

        // option specified as "random": allow any number
        if (result.Tokens.Any(t => "random".Equals(t.Value, StringComparison.OrdinalIgnoreCase)))
            return WorldConfig.RandomCrystals;

        // anything else: the user specified at least one value; we'll use those as possible choices to randomize the count
        // those values are already pre-validated, so they are guaranteed to be integers (or the string "random")
        return result.Tokens.Select(t => int.Parse(t.Value)).ToArray();
    }

    private void Validate(CommandResult result)
    {
        List<string> errors = new();

        if (result.GetValueForOption(_multiworld) <= 0)
        {
            errors.Add("Multiworld player count needs to be at least 1");
        }

        if (result.GetValueForOption(_bulk) <= 0)
        {
            errors.Add("Bulk count needs to be at least 1");
        }

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
            {
                throw new Exception($"Game Unwinnable.");
            }

            if (baseRom != null || outputDirectory != null)
            {
                if (baseRom != null && outputDirectory != null)
                    RomWriter.Write(randomizer, baseRom, baseBPS, outputDirectory);
                else
                    _logger.LogError("Writing a ROM requires all options: {RequiredOptions}", string.Join(", ", [_baseRom.Name, _outputDirectory.Name]));
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

    private static readonly JsonSerializerOptions _options = new()
    {
        AllowTrailingCommas = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };
    private WorldConfig[] GetWorldConfigs(InvocationContext context)
    {
        var settingsFile = context.ParseResult.GetValueForOption(_settingsFile);
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

        _logger.LogInformation("Using directly passed options to construct world.");
        var worldConfigs = Enumerable.Repeat(new WorldConfig
        {
            Accessibility = context.ParseResult.GetValueForOption(_accessibility),
            Goal = context.ParseResult.GetValueForOption(_goal),
            State = context.ParseResult.GetValueForOption(_state),
            Glitches = context.ParseResult.GetValueForOption(_glitches),
            EntranceShuffle = context.ParseResult.GetValueForOption(_entranceShuffle),
            BossShuffle = context.ParseResult.GetValueForOption(_bossShuffle),
            RegionShopSupply = context.ParseResult.GetValueForOption(_shopSupply),
            CrystalsGanonChoices = context.ParseResult.GetValueForOption(_crystalsGanon) ?? WorldConfig.RandomCrystals,
            CrystalsTowerChoices = context.ParseResult.GetValueForOption(_crystalsTower) ?? WorldConfig.RandomCrystals,
            Weapon = context.ParseResult.GetValueForOption(_weapons),
            Techs = context.ParseResult.GetValueForOption(_tech) ?? [],
            StartingEquipment = context.ParseResult.GetValueForOption(_startingItems)?.Select(s => s.Split(",")).SelectMany(s => s).ToList() ?? [],
        }, context.ParseResult.GetValueForOption(_multiworld)).ToArray();

        return worldConfigs;
    }
}
