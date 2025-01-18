namespace Randomizer.Graph;

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Randomizer.Games.SuperMetroid;
using Randomizer.RomModifications;

/// <summary>
/// This is the primary entry point for randomization. A new object is created
/// with a config array dictating how the worlds should be created and prepping
/// all graph infomation for those worlds.
/// </summary>
public abstract class GameRandomizer
{
    private static readonly ILogger _logger = ClassLogger.Get();

    public Graph Graph { get; }
    public IWorld[] Worlds { get; }
    public PRNG PRNG { get; }

    private readonly Inventory _startingItems = new();
    private readonly Vertex _start;
    private readonly IItemPooler _itemPooler;
    public SpoilerLog? SpoilerLog;

    /// <summary>
    /// Set up the Randomizer. This involves:
    /// 1. Creating a new Graph
    /// 2. Creating a starting vertex
    /// 3. and keeping track of all the vertices in the graph
    /// </summary>
    ///
    /// <param name="randomizerConfigs">All the configuration for the each world generation</param>
    /// <param name="seed">Seeded again, eh?</param>
    protected GameRandomizer(WorldConfig[] randomizerConfigs, PRNG prng)
    {
        var sw = Stopwatch.StartNew();
        PRNG = prng;
        _logger.LogInformation("Using seed: {Seed}", PRNG.Seed);

        Graph = new Graph();
        var rootWorld = new RootWorld(Graph);
        _start = rootWorld.Start;

        Worlds = new IWorld[randomizerConfigs.Length];
        for (int i = 0; i < randomizerConfigs.Length; ++i)
        {
            Worlds[i] = CreateWorld(i, randomizerConfigs[i], Graph, PRNG);
            _startingItems = _startingItems.Merge(Worlds[i].StartingItems);

            Graph.AddDirected(_start, Worlds[i].Start, Worlds[i].GetItem("fixed"));
        }

        Graph.SetVertexIds();
        _itemPooler = CreateItemPooler(Worlds, PRNG);

        _logger.LogInformation("Graph configuration took {TimeElapsed}", sw.Elapsed);
    }

    protected abstract IWorld CreateWorld(int worldId, WorldConfig worldConfig, Graph graph, PRNG prng);
    protected abstract IItemPooler CreateItemPooler(IWorld[] worlds, PRNG prng);

    /// <summary>
    /// Randomize the worlds. This handles creating a filler and placing those
    /// items into the worlds.
    /// </summary>
    public void Randomize()
    {
        var filler = new RandomAssumedFiller(this, PRNG);
        var sets = _itemPooler.Pool;

        filler.FillGraph(sets);

        SpoilerLog = new SpoilerLog(this);
    }

    /// <summary>
    /// Get a graph searched based on the items in the inventory.
    /// </summary>
    public ISearcher GetSearcherForInventory(IWorld world, IEnumerable<IItem> items, Vertex? start = null)
    {
        return world.GetSearcherForWorld(Graph, start ?? _start, _startingItems.Merge(new Inventory(items.ToArray())), _itemPooler.SetLocations);
    }

    /// <summary>
    /// Check if the worlds are winnable. This is mostly a sanity check, since items should never be placed in a way that makes the game unwinnable.
    /// </summary>
    public bool IsWinnable() => Worlds.All(world => world.IsWinnable(_start, _startingItems));

    public void Write(FileInfo baseRom, FileInfo? baseBPS, DirectoryInfo outputDirectory)
    {
        foreach (var (i, world) in Worlds.Indexed())
            WriteForWorld(world, baseRom, baseBPS, outputDirectory, PRNG, Worlds.Length > 1 ? $"_W{i + 1}" : null);
    }

    private void WriteForWorld(IWorld world, FileInfo baseRom, FileInfo? baseBPS, DirectoryInfo outputDirectory, PRNG prng, string? worldSuffix = null)
    {
        // TODO: baseRom and baseBPS would likely have to be game-specific, so they might be candidates for moving into the derived classes as well.
        using var rom = CreateRom(baseRom, baseBPS);

        WriteWorldToRom(world, rom, prng);

        //rom.UpdateChecksum();

        outputDirectory.Create();
        string outputFile = Path.Combine(outputDirectory.FullName, CreateFileName(world, prng, worldSuffix));
        rom.Save(outputFile);
    }

    protected virtual string CreateFileName(IWorld world, PRNG prng, string? worldSuffix) => $"{GetType().Name}_{prng.Seed:x08}.rom";
    protected abstract void WriteWorldToRom(IWorld world, Rom rom, PRNG prng);
    // NOTE: this ignores the BPS and assumes baseRom is ready for use. if this isn't the case, override and adjust per game.
    protected virtual Rom CreateRom(FileInfo baseRom, FileInfo? baseBPS) => new(baseRom.FullName);
    /// <summary>Returns (or produces) a usable base rom path for this randomizer. <c>null</c> if the base rom must be provided by the caller.</summary>
    public virtual FileInfo? ProvideBaseRom() => null;
    public abstract void AppendSpoiler(SpoilerLog spoilerLog);
}
