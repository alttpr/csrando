namespace Randomizer.Console.Commands;

using Randomizer.Graph;
using Randomizer.RomModifications;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>Run randomizer as command.</summary>
internal sealed class Randomize : Command
{
    private readonly Option<GoalOption> _goal = new("goal", () => GoalOption.Ganon, "set game goal");
    private readonly Option<StateOption> _state = new("state", () => StateOption.Open, "set game state");
    private readonly Option<WeaponOption> _weapons = new("weapons", () => WeaponOption.Randomized, "set weapons mode");
    private readonly Option<GlitchesOption> _glitches = new("glitches", () => GlitchesOption.None, "set glitches");
    private readonly Option<AccessibilityOption> _accessibility = new("accessibility", "set item/location accessibility");
    private readonly Option<BossShuffleOption> _bossShuffle = new("bossshuffle", () => BossShuffleOption.None, "set boss shuffle mode");
    private readonly Option<EntranceShuffleOption> _entranceShuffle = new("entrance", () => EntranceShuffleOption.None, "set entrance shuffle mode");
    private readonly Option<ShopSupplyOption> _shopSupply = new("shopsupply", () => ShopSupplyOption.Normal, "set shop supply shuffle mode");
    private static readonly string[] _crystalAmount = ["random", "0", "1", "2", "3", "4", "5", "6", "7"];
    private readonly Option<string> _crystalsGanon = new Option<string>("crystals_ganon", () => "7", "set ganon crystal requirement").FromAmong(_crystalAmount);
    private readonly Option<string> _crystalsTower = new Option<string>("crystals_tower", () => "7", "set ganon tower crystal requirement").FromAmong(_crystalAmount);
    private readonly Option<List<TechOption>> _tech = new Option<List<TechOption>>("tech", "set allowed techs").FromAmong(Enum.GetNames(typeof(TechOption)));
    private readonly Option<List<string>> _startingItems = new Option<List<string>>("items", "set starting items (comma separated)");
    private readonly Option<int> _bulk = new("bulk", () => 1, "generate multiple ROMs");
    private readonly Option<int> _multiworld = new("multiworld", () => 1, "multiworld player count");
    private readonly Option<int?> _seed = new("seed", "set starting seed");
    private readonly Option<FileInfo> _baseRom = new Option<FileInfo>("rom", "set base rom").ExistingOnly();
    // TODO: we should probably have the base rom patch "built in" and not require a path.
    private readonly Option<FileInfo> _baseBPS = new Option<FileInfo>("bps", "set base rom patch BPS (for use with a vanilla rom)").ExistingOnly();
    private readonly Option<DirectoryInfo> _outputDirectory = new Option<DirectoryInfo>("outdir", "output directory for generated games");
    private readonly Option<FileInfo> _settingsFile = new Option<FileInfo>("settings", "JSON serialized settings file").ExistingOnly();

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
        Add(_crystalsTower);
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

        AddValidator(Validate);

        this.SetHandler(context => context.ExitCode = Handle(context));
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

        result.ErrorMessage = String.Join('\n', errors);
    }

    /// <summary>Execute the console command.</summary>
    public int Handle(InvocationContext context)
    {
        int bulk = Math.Max(context.ParseResult.GetValueForOption(_bulk), 1);
        var baseRom = context.ParseResult.GetValueForOption(_baseRom);
        var baseBPS = context.ParseResult.GetValueForOption(_baseBPS);
        var outputDirectory = context.ParseResult.GetValueForOption(_outputDirectory);

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
                    System.Console.WriteLine("Writing a ROM requires all options: {0}", string.Join(", ", [_baseRom.Name, _outputDirectory.Name]));
            }
        }
        Info("Randomization took {0}", sw.Elapsed);

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
                    Info("Read {0} worlds from passed config file.", configArray.Length);
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
                    Info("Read single world from passed config file.");
                    return Enumerable.Repeat(singleConfig, context.ParseResult.GetValueForOption(_multiworld)).ToArray();
                }
            }
            catch { }
        }

        Info("Using directly passed options to construct world.");
        string crystalsGanonS = context.ParseResult.GetValueForOption(_crystalsGanon)!;
        int crystalsGanon = crystalsGanonS == "random" ? WorldConfig.RandomCrystals : int.Parse(crystalsGanonS);

        string crystalsTowerS = context.ParseResult.GetValueForOption(_crystalsTower)!;
        int crystalsTower = crystalsTowerS == "random" ? WorldConfig.RandomCrystals : int.Parse(crystalsTowerS);

        var worldConfigs = Enumerable.Repeat(new WorldConfig
        {
            Accessibility = context.ParseResult.GetValueForOption(_accessibility),
            Goal = context.ParseResult.GetValueForOption(_goal),
            State = context.ParseResult.GetValueForOption(_state),
            Glitches = context.ParseResult.GetValueForOption(_glitches),
            EntranceShuffle = context.ParseResult.GetValueForOption(_entranceShuffle),
            BossShuffle = context.ParseResult.GetValueForOption(_bossShuffle),
            RegionShopSupply = context.ParseResult.GetValueForOption(_shopSupply),
            CrystalsGanon = crystalsGanon,
            CrystalsTower = crystalsTower,
            Weapon = context.ParseResult.GetValueForOption(_weapons),
            Techs = context.ParseResult.GetValueForOption(_tech) ?? [],
            StartingEquipment = context.ParseResult.GetValueForOption(_startingItems)?.Select(s => s.Split(",")).SelectMany(s => s).ToList() ?? [],
        }, context.ParseResult.GetValueForOption(_multiworld)).ToArray();

        return worldConfigs;
    }

    private static void Info(string format, params object[] args)
    {
        System.Console.WriteLine(format, args);
    }
    private static void Error(string format, params object[] args)
    {
        var previousColor = System.Console.ForegroundColor;
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine(format, args);
        System.Console.ForegroundColor = previousColor;
    }
}
