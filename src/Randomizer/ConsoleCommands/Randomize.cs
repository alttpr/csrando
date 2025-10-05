namespace Randomizer.ConsoleCommands;

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Randomizer.Games;
using Randomizer.Graph;
using Randomizer.Graph.Json;
using Randomizer.RomModifications;

/// <summary>Run randomizer as command.</summary>
internal sealed class Randomize : Command
{
    private static readonly ILogger _logger = ClassLogger.Get();

    private readonly Option<int> _bulk = new("bulk", "--bulk") { Description = "generate multiple ROMs", DefaultValueFactory = _ => 1 };
    private readonly Option<int> _multiworld = new("multiworld", "--multiworld") { Description = "multiworld player count", DefaultValueFactory = _ => 1 };
    private readonly Option<int?> _seed = new("seed", "--seed") { Description = "set starting seed" };
    // NOTE: use assemblebaserom to generate a usable preset; the following two options are mainly for testing of external rom changes.
    private readonly Option<FileInfo> _baseRom = new Option<FileInfo>("rom", "--rom") { Description = "set base rom" }.AcceptExistingOnly();
    private readonly Option<FileInfo> _baseBPS = new Option<FileInfo>("bps", "--bps") { Description = "set base rom patch BPS (for use with a vanilla rom)" }.AcceptExistingOnly();
    private readonly Option<DirectoryInfo> _outputDirectory = new("outdir", "--outdir") { Description = "output directory for generated games" };
    private readonly Option<FileInfo> _settingsFile = new Option<FileInfo>("settings", "--settings") { Description = "JSON serialized settings file" }.AcceptExistingOnly();
    private readonly Option<bool> _dumpSpoiler = new("spoiler", "--spoiler") { Description = "dump spoiler log" };
    private readonly Option<bool> _dumpLocationsAsJson = new("locations-json", "--locations-json") { Description = "dump location JSON Graph file" };

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
        Add(_dumpLocationsAsJson);

        _multiworld.Validators.Add(r =>
        {
            if (r.GetValueOrDefault<int>() <= 0)
                r.AddError("Multiworld player count needs to be at least 1");
        });
        _bulk.Validators.Add(r =>
        {
            if (r.GetValueOrDefault<int>() <= 0)
                r.AddError("Bulk count needs to be at least 1");
        });

        SetAction(Handle);
    }

    /// <summary>Execute the console command.</summary>
    public int Handle(ParseResult parseResult)
    {
        int bulk = Math.Max(parseResult.GetValue(_bulk), 1);
        var baseRom = parseResult.GetValue(_baseRom);
        var baseBPS = parseResult.GetValue(_baseBPS);
        var outputDirectory = parseResult.GetValue(_outputDirectory);
        bool dumpSpoiler = parseResult.GetValue(_dumpSpoiler);
        bool dumpLocations = parseResult.GetValue(_dumpLocationsAsJson);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < bulk; i++)
        {
            var worldConfigs = GetWorldConfigs(parseResult);
            var randomizer = RandomizerFactory.Create(
                worldConfigs,
                parseResult.GetValue(_seed)
            );
            randomizer.Randomize();
            if (!randomizer.IsWinnable())
                throw new Exception($"Game Unwinnable (seed: {randomizer.PRNG.Seed}).");

            if (outputDirectory != null)
            {
                baseRom ??= randomizer.ProvideBaseRom();
                if (baseRom == null)
                {
                    _logger.LogError("Writing a ROM requires all options: {RequiredOptions}", string.Join(", ", [_baseRom.Name, _outputDirectory.Name]));
                    continue;
                }

                var fileRomBroker = new FileRomBroker(baseRom, baseBPS, outputDirectory);
                randomizer.Write(fileRomBroker);
            }
            if (dumpSpoiler)
            {
                Console.WriteLine("{0}", JsonSerializer.Serialize(randomizer.SpoilerLog!.Spoiler, new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                }));
            }
            if (dumpLocations)
            {
                DumpLocationJsonGraph(randomizer.Graph, outputDirectory);
            }
        }
        _logger.LogInformation("Randomization took {TimeElapsed}", sw.Elapsed);
        return 0;
    }

    private void DumpLocationJsonGraph(Graph graph, DirectoryInfo? outputDirectory)
    {
        string outputFile = Path.Combine(outputDirectory == null ? Directory.GetCurrentDirectory() : outputDirectory.FullName, "locations.json");
        var edges = new List<JsonEdge>();
        var nodes = new Dictionary<string, JsonNode>();

        foreach (var vertex in graph.GetVertices())
        {
            nodes.Add(vertex.Id.ToString(), new JsonNode
            {
                Id = vertex.Id.ToString(),
                Label = vertex.Name,
                // TODO: get this from the vertex (or the randomizer)
                Metadata = new Dictionary<string, object?>
                {
                    { "name", vertex.Name },
                    { "game", vertex.World.GameId },
                    { "type", vertex.Type.ToString() },
                    { "item", vertex.Item?.ToString() },
                    { "trophy", vertex.Trophy?.ToString() },
                },
            });

            foreach (var edge in vertex.Edges)
            {
                edges.Add(new()
                {
                    Id = $"{edge.From.Id}->{edge.To.Id}",
                    Directed = true,
                    Relation = edge.Condition.IsUnconditional ? "" : edge.Condition.Count > 1 ? $"{edge.Condition.Count}x{edge.Condition.Item?.Name}" : edge.Condition.Item?.Name,
                    Source = edge.From.Id.ToString(),
                    Target = edge.To.Id.ToString(),
                    Metadata = new Dictionary<string, object?>
                    {
                        { "relation", edge.Condition.IsUnconditional ? "" : edge.Condition.Count > 1 ? $"{edge.Condition.Count}x{edge.Condition.Item?.Name}" : edge.Condition.Item?.Name },
                    },
                });
            }
        }

        var jsonRoot = new JsonRoot();
        jsonRoot.Graph.Id = "ALTTPR";
        jsonRoot.Graph.Directed = true;
        jsonRoot.Graph.Nodes = nodes;
        jsonRoot.Graph.Edges = edges;

        File.WriteAllText(outputFile, JsonSerializer.Serialize(jsonRoot));
    }

    private static readonly JsonSerializerOptions _options = new()
    {
        AllowTrailingCommas = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };
    private WorldConfig[] GetWorldConfigs(ParseResult parseResult)
    {
        var settingsFile = parseResult.GetValue(_settingsFile);
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
                    return Enumerable.Repeat(singleConfig, parseResult.GetValue(_multiworld)).ToArray();
                }
            }
            catch { }
        }

        throw new InvalidOperationException("No usable settings file passed. Either use the --settings option or place a valid file at data/settings.json.");
    }
}
