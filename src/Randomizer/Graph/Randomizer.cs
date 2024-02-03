namespace Randomizer.Graph;

using System.Diagnostics;

/// <summary>
/// This is the primary entry point for randomization. A new object is created
/// with a config array dictating how the worlds should be created and prepping
/// all graph infomation for those worlds.
///
/// Walk thru walls: 7E037F01
/// </summary>
public sealed class Randomizer
{
    public Graph Graph { get; private set; }
    public World[] Worlds { get; }
    public PRNG PRNG { get; }

    private readonly Inventory _startingItems = new();
    private readonly Vertex _start;

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
        System.Console.WriteLine($"Using seed: {PRNG.Seed}");

        Graph = new Graph();
        _start = Graph.AddVertex(new Vertex
        {
            Name = "start",
            World = new World(Graph),
            Type = VertexType.Meta,
        });

        Worlds = new World[randomizerConfigs.Length];
        for (var i = 0; i < randomizerConfigs.Length; ++i)
        {
            randomizerConfigs[i].SelectRandomValues(PRNG);

            Worlds[i] = new World(i, randomizerConfigs[i], Graph);
            _startingItems = _startingItems.Merge(Worlds[i].StartingItems);

            GameWinnerer.AdjustEdges(Worlds[i], PRNG);
            ShopFiller.AdjustEdges(Worlds[i], PRNG);
            DoorShuffler.AdjustEdges(Worlds[i], PRNG);
            EntranceShuffler.AdjustEdges(Worlds[i], PRNG);
            DarknessGraphifier.AdjustEdges(Worlds[i], PRNG);
            EnemyShuffler.AdjustEdges(Worlds[i], PRNG);
            BossShuffler.AdjustEdges(Worlds[i], PRNG);
            BunnyGraphifier.AdjustEdges(Worlds[i], PRNG);
            PrizePackShuffler.AdjustEdges(Worlds[i], PRNG);
            DoorReplacer.AdjustEdges(Worlds[i], PRNG);
            DungeonPegStateCopier.AdjustEdges(Worlds[i], PRNG);

            Graph.AddDirected(_start, Worlds[i].GetLocation("start"), Worlds[i].GetItem("fixed"));
        }

        Graph.SetVertexIds();

        System.Console.WriteLine($"Graph configuration took {sw.Elapsed}");
    }

    /// <summary>
    /// Randomize the worlds. This handles creating a filler and placing those
    /// items into the worlds.
    /// </summary>
    public void Randomize()
    {
        var filler = new RandomAssumedFiller(this, PRNG);
        var sets = new ItemPooler(Worlds, PRNG).GetPool();

        filler.FillGraph(sets);
    }

    /// <summary>
    /// Get a graph searched based on the items in the inventory.
    /// </summary>
    public Searcher GetSearcherForInventory(IEnumerable<Item> items, World? world = null)
    {
        return new(Graph, world?.GetLocation("start") ?? _start, _startingItems.Merge(new Inventory(items.ToArray())));
    }

    /// <summary>
    /// Check if the worlds are winnable. This is done by creating a searcher.
    /// </summary>
    public bool IsWinnable()
    {
        Searcher searcher = new(Graph, _start, _startingItems);

        foreach (var world in Worlds)
        {
            if (!searcher.HasFound(world.GetItem("Triforce")))
            {
                return false;
            }
        }

        return true;
    }
}
