namespace Randomizer.Games.Alttp;

using Randomizer.Games.Alttp.WorldModifiers;
using Randomizer.Graph;
using Graph = Graph.Graph;

/// <summary>Model of a world in which a player would be playing.</summary>
public sealed class World : IWorld
{
    public int Id { get; }
    public Graph Graph { get; }
    public Inventory StartingItems { get; }
    public WorldConfig Config { get; }
    private readonly Dictionary<string, Item> _allItems = new();
    public ushort PlacedItemCount { get; set; }
    public (byte[] Underworld, byte[] Overworld, byte[] Sets) SpriteSheets { get; set; } = ([], [], []);

    /// <summary>Add all the vertices to the graph for this region.</summary>
    /// <param name="id">id of this world</param>
    /// <param name="randomizerConfig">options for this world</param>
    public World(int id, WorldConfig randomizerConfig, Graph graph)
    {
        Id = id;
        Config = randomizerConfig;
        Graph = graph;

        List<Item> items = [GetItem("fixed")];
        items.Add(GetItem($"ConfigWorldWeapon{Config.Weapon}"));
        items.Add(GetItem($"ConfigWorldState{Config.State}"));
        items.Add(GetItem($"ConfigWorldGlitches{Config.Glitches}"));
        items.Add(GetItem($"ConfigWorldEnemyShuffle{Config.EnemyShuffle}"));
        items.Add(GetItem($"ConfigWorldTowerEntryRequired{Config.CrystalsTower}"));
        items.Add(GetItem($"ConfigWorldGanonVulnerableRequired{Config.CrystalsGanon}"));
        foreach (var tech in Config.Techs)
        {
            items.Add(GetItem($"ConfigWorldTech{tech}"));
        }

        items.AddRange(randomizerConfig.StartingEquipment.Select(x => GetItem(x)));
        StartingItems = new Inventory(items.ToArray());

        foreach (var vertex in VertexCollector.LoadYmlData(this))
        {
            Graph.AddVertex(vertex);
        }

        var edges = new EdgeCollector().GetForWorld(this);
        foreach (var (condition, data) in edges)
        {
            foreach (var edgeData in data.Directed)
            {
                var from = GetLocation(edgeData[0]);
                var to = GetLocation(edgeData[1]);
                if (from is null || to is null)
                {
                    throw new Exception(
                        "Name Connection Mismatch: " +
                        $"({edgeData[0]}, {edgeData[1]}) => " +
                        $"({from}, {to})");
                }
                Graph.AddDirected(from, to, condition);
            }
            foreach (var edgeData in data.Undirected)
            {
                var from = GetLocation(edgeData[0]);
                var to = GetLocation(edgeData[1]);
                if (from is null || to is null)
                {
                    throw new Exception(
                        "Undirected Name Connection Mismatch: " +
                        $"({edgeData[0]}, {edgeData[1]}) => " +
                        $"({from}, {to})");
                }
                Graph.AddDirected(from, to, condition);
                Graph.AddDirected(to, from, condition);
            }
        }

        PruneConfigEdges();
    }

    private void PruneConfigEdges()
    {
        foreach (var v in GetLocations())
        {
            v.Edges = v.Edges.Where(e => !e.Condition.Item.Name.StartsWith("ConfigWorld") || StartingItems.Has(e.Condition.Item)).ToList();
        }
    }

    public Inventory ComputeStartingItems()
    {
        var inventory = new Inventory([GetItem("fixed"), .. Config.StartingEquipment.Select(GetItem)]);
        var searcher = new Searcher(Graph, GetLocation("DefaultItems"), inventory);
        return inventory;
    }

    /// <summary>
    /// Get a vertex by name in this world.
    /// </summary>
    /// <param name="locationName">name to search for</param>
    public Vertex GetLocation(string locationName)
    {
        return Graph.GetVertex($"{locationName}:{Id}");
    }

    public bool HasLocation(string locationName)
    {
        return Graph.HasVertex($"{locationName}:{Id}");
    }

    /// <summary>Get all vertices in this world.</summary>
    /// <returns></returns>
    public IEnumerable<Vertex> GetLocations() => Graph.GetVertices().Where(vertex => vertex.World == this);
    /// <summary>Get all vertices of a given type in this world.</summary>
    /// <param name="type">type to search for</param>
    public IEnumerable<Vertex> GetLocationsOfType(VertexType type) => GetLocations().Where(vertex => vertex.Type == type);

    public Item GetItem(string name)
    {
        if (_allItems.TryGetValue(name, out var matchingItem))
        {
            return matchingItem;
        }

        // allow made up items
        var item = new Item(name, this);
        _allItems.Add(item.Name, item);
        item.Id = Graph.AllItems.Count;
        Graph.AllItems.Add(item);

        return item;
    }

    public Item? GetItemOrNull(string? name)
    {
        if (name != null)
            return GetItem(name);
        return null;
    }

    public Item? GetExistingItem(string name)
    {
        if (_allItems.TryGetValue(name, out var item))
            return item;
        return null;
    }

    public IEnumerable<Item> GetAllItems()
    {
        return _allItems.Values;
    }
    public void ApplyWorldModifications(PRNG prng)
    {
        List<IWorldModifier> modifiers =
        [
            new GameWinnerer(),
            new ShopFiller(),
            new DoorShuffler(),
            new EntranceShuffler(),
            new DarknessGraphifier(),
            // EnemyShuffler will adjust sprite sheets, which relies on the BossShuffler running first
            // (and placing bosses in their respective rooms already)
            new BossShuffler(),
            new EnemyShuffler(),
            new BunnyGraphifier(),
            new PrizePackShuffler(),
            new DoorReplacer(),
            new DungeonPegStateCopier(),
        ];

        foreach (var modifier in modifiers)
            modifier.AdjustEdges(this, prng);
    }
}
