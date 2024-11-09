namespace Randomizer.Graph;

using System.Diagnostics;
using Microsoft.Extensions.Logging;

/// <summary>
/// This is the primary entry point for randomization. A new object is created
/// with a config array dictating how the worlds should be created and prepping
/// all graph infomation for those worlds.
/// </summary>
public sealed class Randomizer
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
    public Randomizer(WorldConfig[] randomizerConfigs, int? seed = null)
    {
        var sw = Stopwatch.StartNew();
        PRNG = new PRNG(seed);
        _logger.LogInformation("Using seed: {Seed}", PRNG.Seed);

        Graph = new Graph();
        var rootWorld = new RootWorld(Graph);
        _start = rootWorld.Start;

        Worlds = new IWorld[randomizerConfigs.Length];
        for (int i = 0; i < randomizerConfigs.Length; ++i)
        {
            Worlds[i] = WorldFactory.CreateWorld(i, randomizerConfigs[i], Graph, PRNG);
            _startingItems = _startingItems.Merge(Worlds[i].StartingItems);

            Graph.AddDirected(_start, Worlds[i].Start, Worlds[i].GetItem("fixed"));
        }

        Graph.SetVertexIds();
        _itemPooler = new RootItemPooler(Worlds, PRNG);

        _logger.LogInformation("Graph configuration took {TimeElapsed}", sw.Elapsed);
    }

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
    public Searcher GetSearcherForInventory(IEnumerable<IItem> items, Vertex? start = null)
    {
        return new(Graph, start ?? _start, _startingItems.Merge(new Inventory(items.ToArray())), _itemPooler.SetLocations);
    }

    /// <summary>
    /// Check if the worlds are winnable. This is mostly a sanity check, since items should never be placed in a way that makes the game unwinnable.
    /// </summary>
    public bool IsWinnable() => Worlds.All(world => world.IsWinnable(_start, _startingItems));
}
