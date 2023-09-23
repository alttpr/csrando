namespace Randomizer.Graph;

/**
 * This is the primary entry point for randomization. A new object is created
 * with a config array dictating how the worlds should be created and prepping
 * all graph infomation for those worlds.
 *
 * Walk thru walls: 7E037F01
 */
public sealed class Randomizer
{
    private static readonly HashSet<VertexType> ITEM_LOCATIONS = new()
    {
        VertexType.BigChest,
        VertexType.Bonk,
        VertexType.Chest,
        VertexType.Drop,
        VertexType.Dig,
        VertexType.Event,
        VertexType.Medallion,
        // VertexType.Mob // enabling this will allow enemies for major items
        VertexType.Npc,
        VertexType.Pedestal,
        VertexType.Pot,
        VertexType.Prize,
        VertexType.Refill,
        VertexType.ShopItem,
        VertexType.Standing,
    };

    public Graph Graph { get; private set; }
    private Inventory _assumedItems = new();
    private HashSet<Vertex> _foundLocations = new();
    private readonly Dictionary<string, Vertex> _vertices;
    private readonly Inventory _collectedItems = new();
    private readonly Vertex _start;
    private readonly Dictionary<string, List<Vertex>> _setLocations = new() { { "*", new() } };
    private readonly World[] _worlds;
    private readonly PRNG _prng;

    /**
     * Set up the Randomizer. This involves:
     * 1. Creating a new Graph
     * 2. Creating a starting vertex
     * 3. and keeping track of all the vertices in the graph
     *
     * @param array configs options for the worlds
     *
     * @return void
     */
    public Randomizer(WorldConfig[] randomizerConfigs, int? seed = null)
    {
        _prng = new PRNG(seed);
        System.Console.WriteLine($"Using seed: {_prng.Seed}");

        Graph = new Graph();
        _start = Graph.NewVertex(new()
        {
            { "name", "start" },
            { "type", VertexType.Meta },
        });

        _vertices = new Dictionary<string, Vertex>
        {
            { _start.Name, _start },
        };

        _worlds = new World[randomizerConfigs.Length];
        for (var i = 0; i < randomizerConfigs.Length; ++i)
        {
            if (randomizerConfigs[i].CrystalsGanon == WorldConfig.RandomCrystals)
                randomizerConfigs[i].CrystalsGanon = _prng.GetRandomInt(7 + 1);

            if (randomizerConfigs[i].CrystalsTower == WorldConfig.RandomCrystals)
                randomizerConfigs[i].CrystalsTower = _prng.GetRandomInt(7 + 1);

            _worlds[i] = new World(i, randomizerConfigs[i]);
            _collectedItems = _collectedItems.Merge(_worlds[i].CollectedItems);

            ShopFiller.AdjustEdges(_worlds[i], _prng);
            EntranceShuffler.AdjustEdges(_worlds[i], _prng);
            BossShuffler.AdjustEdges(_worlds[i], _prng);
            EnemyShuffler.AdjustEdges(_worlds[i], _prng);
            BunnyGraphifier.AdjustEdges(_worlds[i], _prng);
            PrizePackShuffler.AdjustEdges(_worlds[i], _prng);

            Graph = Graph.Merge(_worlds[i].Graph);
            Graph.AddDirected(_start, Graph.GetVertex($"start:{i}"), _worlds[i].GetItem("fixed"));
        }

        foreach (var location in Graph.GetVertices())
        {
            _vertices[location.Name] = location;
            if (!ITEM_LOCATIONS.Contains(location.Type))
            {
                continue;
            }

            _setLocations["*"].Add(location);
            foreach (string set in location.ItemSet)
            {
                _setLocations.TryAdd(set, new());
                _setLocations[set].Add(location);
            }
        }
    }

    /**
     * Randomize the worlds. This handles creating a filler and placing those
     * items into the worlds.
     * 
     * @return World[]
     */
    public World[] Randomize()
    {
        var filler = new RandomAssumedFiller(this, _prng);
        var sets = new ItemPooler(_worlds, _prng).GetPool();

        filler.FillGraph(sets);

        return _worlds;
    }

    /**
     * Assume a set of items and search for locations that would be accessable.
     *
     * @param array<Item> items items to assume
     */
    public void AssumeItems(IEnumerable<Item> items)
    {
        Searcher searcher = new(Graph, _start);
        _assumedItems = new Inventory(items.ToArray());
        _foundLocations = searcher.Search(_collectedItems.Merge(_assumedItems)).ToHashSet();
    }

    public Searcher GetSearcherForInventory(IEnumerable<Item> items)
    {
        Searcher searcher = new(Graph, _start);
        searcher.Search(_collectedItems.Merge(new Inventory(items.ToArray())));

        return searcher;
    }

    /// <summary>Get a set of Locations without items that match the given itemSet. Available counts in itemSets is required.</summary>
    /// <param name="itemSet">constrain results to item set</param> 
    /// <param name="itemSets">counts of items required in each sett</param> 
    /// <param name="reachable">reachable only return reachable locations</param> 
    public IEnumerable<Vertex> GetEmptyLocationsInSet(string itemSet = "*", Dictionary<string, int>? itemSets = null, bool reachable = true)
    {
        var empty_locations = _setLocations[itemSet].Where((vertex) =>
        {
            return (!reachable || _foundLocations.Contains(vertex)) && vertex.Item == null;
        }).ToList();

        itemSets ??= new();
        foreach (var (set_name, set_count) in itemSets)
        {
            if (set_name == "*")
            {
                continue;
            }
            var set_locations = _setLocations[set_name].Where(static (location) => location.Item == null);
            if (set_locations.Count() < set_count)
            {
                throw new Exception($"Not enough set locations available: {set_name}");
            }
            // if a set has the same number of items to place as set locations
            // left, remove it from this return.
            if (itemSet != set_name && set_locations.Count() == set_count)
            {
                empty_locations.RemoveAll(set_locations.Contains);
            }
        }

        return empty_locations.ToArray();
    }

    /**
     * Determine if a location is considered reachable after last assumeItems
     * call.
     *
     * @param string location_name location to check reachability of
     */
    public bool CanReachLocation(string locationName)
    {
        return _foundLocations.Contains(_vertices[locationName]);
    }

    /**
     * Get array of locations that currently have placed items.
     *
     * @param array? locations filtered locations to check, otherwise all locations
     */
    public IEnumerable<Item> ItemsFromLocations(IEnumerable<Vertex>? locations = null)
    {
        return (locations ?? _foundLocations)
            .Where((location) => location.Item is not null || location.Trophy is not null)
            .Select(location => location.Item ?? location.Trophy!);
    }

    // return all items for locations that have items
    private List<Item> GetItems(IEnumerable<Vertex>? locations = null)
    {
        var items = new List<Item>();
        items.AddRange(ItemsFromLocations(locations));
        return items;
    }
    /**
     * Search world and get all items that are found.
     *
     * @param array? locations filtered locations to check, otherwise all locations
     */
    public Inventory CollectItems(IEnumerable<Vertex>? locations = null)
    {
        var items = GetItems(locations);
        return new Inventory(items.ToArray());
    }

    public Item GetItemForWorld(string name, int worldId)
    {
        return _worlds[worldId].GetItem(name);
    }
}

internal static class LookupExtensions
{
    // .NET 8 has this in the PCL, .NET 7 does not.
    public static void Deconstruct<TKey, TElement>(this IGrouping<TKey, TElement> grouping, out TKey key, out IEnumerable<TElement> elements)
    {
        key = grouping.Key;
        elements = grouping;
    }
}
