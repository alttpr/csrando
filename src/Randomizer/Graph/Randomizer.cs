namespace Randomizer.Graph;

using System.Collections.Concurrent;

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
    private readonly Graph _startingGraph;
    private readonly Dictionary<ItemCondition, Graph> _graphs;
    private readonly ConcurrentDictionary<Item, HashSet<Vertex>> _keyDoors = new();
    /**
     * Key edges for just this door.
     */
    private readonly Dictionary<Vertex, Graph> _keyDoorEdges = new();
    /** @var Collection<Vertex> */
    private readonly Dictionary<string, Vertex> _vertices;
    private Inventory _assumedItems;
    private readonly Inventory _collectedItems;
    private HashSet<Vertex> _foundLocations;
    private readonly Vertex _start;
    /** @var array<array<Vertex>> */
    private readonly Dictionary<string, List<Vertex>> _setLocations = new() { { "*", new() } };
    private readonly World[] _worlds;
    private readonly Dictionary<string, List<Vertex>> _doorChains = new();
    private readonly PRNG _prng;
    //static array door_cache = [];

    /**
     * Set up the Randomizer. This involves:
     * 1. Creating a new Graph
     * 2. Creating a starting vertex
     * 3. Creating a mapping of placement groups and their vertices
     * 4. using an array of array type configs for all the worlds we would like
     *    to randomize and building them out
     * 5. and keeping track of all the vertices in the graph
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
        _collectedItems = new Inventory();
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

        // @todo this needs simplified!
        var graphs = new Dictionary<ItemCondition, Graph>();
        var edgesByGroup = Graph.GetEdges().ToLookup(edge => edge.Condition);
        foreach (var (condition, edges) in edgesByGroup)
        {
            if (graphs.ContainsKey(condition))
            {
                continue;
            }

            if (condition.Item.Name.StartsWith("Key") && !condition.Item.Name.StartsWith("KeyForKey"))
            {
                var key_edges = edges;

                var keyDoorsForGroup = _keyDoors.GetOrAdd(condition.Item, _ => new());
                foreach (var edge in key_edges)
                {
                    keyDoorsForGroup.Add(edge.From);
                }

                foreach (var key_door in keyDoorsForGroup)
                {
                    var graphForKeyDoor = new Graph();
                    // TODO: should this realy overwrite the graph?
                    //this.key_door_edges.Add(key_door, graphForKeyDoor);
                    _keyDoorEdges[key_door] = graphForKeyDoor;
                    foreach (var edge in key_edges)
                    {
                        if (edge.From == key_door)
                        {
                            graphForKeyDoor.AddEdge(edge);
                        }
                    }
                }
            }
            else
            {
                graphs.Add(condition, Graph.GetSubgraph(condition));
            }
        }

        _graphs = graphs;

        // TODO: handle multiworld
        _startingGraph = _graphs[new(_worlds[0].GetItem("fixed"), 1)];
        _startingGraph = SearchGraph(_collectedItems);
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
     * Get a complete connected graph that has been searched based on a supplied
     * inventory. One may supply a starting graph to continue searching from
     * along with item locations within the graph that have already been
     * accounted for.
     *
     * @todo Speed this up!!! Every 1ms in here ~= 100ms of total time
     *
     * @param Inventory collected Items assumed to be collected before the search has started
     * @param Graph? starting_graph A graph that is already partially searched
     * @param array collected_item_map a listing of locations that are already accounted for in collected
     */
    private Graph SearchGraph(Inventory collected, Graph? startingGraph = null)
    {
        var searchGraph = startingGraph ?? _startingGraph;
        var graphs = new Dictionary<ItemCondition, Graph>(_graphs);
        bool newItemsFound;
        do
        {
            var sub_graphs = new List<Graph>();
            foreach (var (condition, sub_graph) in graphs)
            {
                if (collected.HasAtLeast(condition.Item, condition.Count))
                {
                    sub_graphs.Add(sub_graph);
                    graphs.Remove(condition);
                }
            }

            searchGraph.MergeWith(sub_graphs.ToArray());
            searchGraph.Search(_start);

            newItemsFound = false;
            foreach (var item in searchGraph.GetItems(_start))
            {
                if (!collected.Has(item))
                {
                    if (item.Name.StartsWith("BigRedBomb") && DropOffSearch(item, searchGraph))
                    {
                        collected.AddItem(item.World.GetItem("BigRedBombActive"));
                    }
                    newItemsFound = true;
                    collected.AddItem(item);
                }
            }
        } while (newItemsFound);

        return searchGraph;
    }

    private bool DropOffSearch(Item item, Graph searchGraph)
    {
        var start = _vertices["Bomb Shoppe Lobby:" + item.World.Id];
        var end = _vertices["Pyramid:" + item.World.Id];
        // @todo tidy up this exclude by only being hops/entrances.
        Item[] exclude = {
            item.World.GetItem("hop"),
            item.World.GetItem("Flippers"),
            item.World.GetItem("DarkFlippers"),
        };
        var bomb_search_graph = searchGraph.Exclude(exclude);
        bomb_search_graph.Search(start);

        return bomb_search_graph.GetVisited(start).Contains(end);
    }

    /**
     * Search the graph starting with a set number of keys, and determine the
     * shared outcome if one were to use their keys in every possible way.
     *
     * @param Graph searchGraph starting search
     * @param array found previously found locations
     * @param Inventory collected already collected items
     * @param array lockedDoors currently locked doors
     * @param string key which key we are unlocking for
     * @param int recursionLevel how deep in this %#*& we are
     * @param array chain listing of doors already opened
     */
    private IEnumerable<Vertex> RecursiveDoorSearch(
        Graph searchGraph,
        IEnumerable<Vertex> found,
        Inventory collected,
        IEnumerable<Vertex> lockedDoors,
        Item key,
        int recursionLevel,
        IEnumerable<Vertex>? chain = null
    )
    {
        if (collected.GetCount(key) >= _keyDoors[key].Count)
        {
            var door_graphs = _keyDoorEdges.Where(door_id => _keyDoors[key].Contains(door_id.Key)).Select(k => k.Value).ToArray();
            var graph = SearchGraph(collected, searchGraph.Merge(door_graphs));
            return graph.GetVisited(_start);
        }

        var in_graph_locations = searchGraph.GetVisited(_start);
        var sub_found_locations = new Dictionary<Vertex, List<Vertex>>();
        chain ??= Enumerable.Empty<Vertex>();
        found = found.ToArray();
        foreach (var door in lockedDoors.Where(location => in_graph_locations.Contains(location)))
        {
            var current_chain = chain.Concat(new[] { door }).ToArray();
            string chain_id = string.Join('-', current_chain.Select(v => v.GetHashCode()).OrderBy(h => h));
            if (_doorChains.TryGetValue(chain_id, out var door_chain))
            {
                sub_found_locations.Add(door, door_chain);
                continue;
            }
            var graph = SearchGraph(collected, searchGraph.Merge(_keyDoorEdges[door]));
            var found_locations = graph.GetVisited(_start).ToList();
            var new_collected = collected.Merge(CollectItems(found_locations.Except(found)));
            sub_found_locations.Add(door, found_locations);
            int new_found_keys = new_collected.GetCount(key);
            if (lockedDoors.Count() > 1 && new_found_keys > recursionLevel)
            {
                var all_found = found_locations.Concat(found).ToArray();
                // TODO: should this be overwriting the door search result?
                //sub_found_locations.Add(door, this.recursiveDoorSearch(
                sub_found_locations[door] = RecursiveDoorSearch(
                    graph,
                    all_found,
                    new_collected,
                    lockedDoors.Except(new[] { door }).ToArray(),
                    key,
                    recursionLevel + 1,
                    current_chain
                ).ToList();
            }
            _doorChains.Add(chain_id, sub_found_locations[door]);
        }

        if (sub_found_locations.Any())
        {
            IEnumerable<Vertex> result = sub_found_locations.Values.First();
            foreach (var other in sub_found_locations.Values.Skip(1))
                result = result.Intersect(other);
            return result;
        }

        return found;
    }

    /**
     * Get locations that are accessible no matter how one uses the keys
     * available.
     *
     * @param Inventory collected Assumed collected items
     */
    public IEnumerable<Vertex> GetStrongLocations(Inventory collected)
    {
        var search_graph = SearchGraph(collected);
        var found_locations = search_graph.GetVisited(_start).ToHashSet();
        var new_found_locations = new HashSet<Vertex>();
        do
        {
            _doorChains.Clear();
            new_found_locations.Clear();
            var new_items = CollectItems(found_locations);
            var new_collected = collected.Merge(new_items);
            search_graph = SearchGraph(new_collected, null);
            foreach (var (key, doors) in _keyDoors)
            {
                if (new_collected.Has(key))
                {
                    var strong_locations = RecursiveDoorSearch(
                        search_graph,
                        found_locations,
                        new_collected,
                        doors,
                        key,
                        1
                    ).ToHashSet();
                    strong_locations.ExceptWith(found_locations);
                    found_locations.UnionWith(strong_locations);
                    new_found_locations.UnionWith(strong_locations);
                }
            }
        } while (new_found_locations.Any());

        return found_locations;
    }

    /**
     * Assume a set of items and search for locations that would be accessable.
     *
     * @param array<Item> items items to assume
     */
    public void AssumeItems(IEnumerable<Item> items)
    {
        _assumedItems = new Inventory(items.ToArray());
        _foundLocations = GetStrongLocations(_collectedItems.Merge(_assumedItems)).ToHashSet();
    }

    /**
     * Get locations in an item set, one may toggle reachability of these
     * locations.
     *
     * @param string item_set contrain results to item set
     * @param array item_sets counts of items required in each set
     * @param bool reachable only return reachable locations
     *
     * @throws Exception if there are no available locations in a set
     *
     * @return array<Vertex>
     */
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
            var set_locations = _setLocations[set_name].Where(static (location) =>
            {
                return location.Item == null;
            });
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
