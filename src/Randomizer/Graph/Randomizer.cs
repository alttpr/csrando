namespace Randomizer.Graph;

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
    private readonly Inventory _startingItems = new();
    private readonly Vertex _start;
    private readonly World[] _worlds;
    private readonly PRNG _prng;

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
        _prng = new PRNG(seed);
        System.Console.WriteLine($"Using seed: {_prng.Seed}");

        Graph = new Graph();
        _start = Graph.AddVertex(new Vertex
        {
            Name = "start",
            Type = VertexType.Meta,
        });

        _worlds = new World[randomizerConfigs.Length];
        for (var i = 0; i < randomizerConfigs.Length; ++i)
        {
            if (randomizerConfigs[i].CrystalsGanon == WorldConfig.RandomCrystals)
                randomizerConfigs[i].CrystalsGanon = _prng.GetRandomInt(7 + 1);

            if (randomizerConfigs[i].CrystalsTower == WorldConfig.RandomCrystals)
                randomizerConfigs[i].CrystalsTower = _prng.GetRandomInt(7 + 1);

            _worlds[i] = new World(i, randomizerConfigs[i], Graph);
            _startingItems = _startingItems.Merge(_worlds[i].CollectedItems);

            ShopFiller.AdjustEdges(_worlds[i], _prng);
            EntranceShuffler.AdjustEdges(_worlds[i], _prng);
            BossShuffler.AdjustEdges(_worlds[i], _prng);
            EnemyShuffler.AdjustEdges(_worlds[i], _prng);
            BunnyGraphifier.AdjustEdges(_worlds[i], _prng);
            PrizePackShuffler.AdjustEdges(_worlds[i], _prng);

            Graph.AddDirected(_start, _worlds[i].Graph.GetVertex($"start:{i}"), _worlds[i].GetItem("fixed"));
        }
    }

    /// <summary>
    /// Randomize the worlds. This handles creating a filler and placing those
    /// items into the worlds.
    /// </summary>
    public void Randomize()
    {
        var filler = new RandomAssumedFiller(this, _prng);
        var sets = new ItemPooler(_worlds, _prng).GetPool();

        filler.FillGraph(sets);
    }

    /// <summary>
    /// Get a graph searched based on the items in the inventory.
    /// </summary>
    public Searcher GetSearcherForInventory(IEnumerable<Item> items)
    {
        return new(Graph, _start, _startingItems.Merge(new Inventory(items.ToArray())));
    }

    public Item GetItemForWorld(string name, int worldId)
    {
        return _worlds[worldId].GetItem(name);
    }

    /// <summary>
    /// Check if the worlds are winnable. This is done by creating a searcher.
    /// </summary>
    public bool IsWinnable()
    {
        Searcher searcher = new(Graph, _start, _startingItems);

        foreach (var world in _worlds)
        {
            if (!searcher.HasFound(world.GetItem("Triforce")))
            {
                return false;
            }
        }

        return true;
    }
}
