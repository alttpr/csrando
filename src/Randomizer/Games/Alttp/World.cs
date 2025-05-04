namespace Randomizer.Games.Alttp;

using Randomizer.Games;
using Randomizer.Games.Alttp.WorldModifiers;
using Randomizer.Graph;
using BaseVertex = Graph.Vertex;
using Graph = Graph.Graph;

/// <summary>
/// Model of a world in which a player would be playing.
///
/// Walk thru walls: 7E037F01
/// </summary>
public sealed class World : World<Item>
{
    public Config Config { get; }
    public (byte[] Underworld, byte[] Overworld, byte[] Special, byte[] Sets) SpriteSheets { get; set; } = ([], [], [], []);
    public Dictionary<IItem /* actualKey */, List<(BaseVertex Chest, List<BaseVertex> Regions)>> KeyForKeys { get; } = [];

    /// <summary>Add all the vertices to the graph for this region.</summary>
    /// <param name="id">id of this world</param>
    /// <param name="randomizerConfig">options for this world</param>
    public World(int id, WorldConfig randomizerConfig, Graph graph, PRNG prng)
        : base("Zelda3", id, graph, randomizerConfig)
    {
        Config = randomizerConfig.Alttp ?? throw new ArgumentException("This world requires valid settings for The Legend of Zelda: A Link to the Past");
        Config.SelectRandomValues(prng);

        List<IItem> items = [GetItem("fixed")];
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

        items.AddRange(Config.StartingEquipment.Select(GetItem));
        StartingItems = new Inventory(items.ToArray());

        Start = DataLoader.Fill(this);

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

    public Inventory ComputeStartingItems()
    {
        var inventory = new Inventory([GetItem("fixed"), .. Config.StartingEquipment.Select(GetItem)]);
        var searcher = new Searcher(Graph, GetLocation("DefaultItems"), inventory);
        return inventory;
    }

    protected override Item CreateItem(string name, IWorld world) => new(name, world);

    /// <summary>Get all vertices of a given type in this world.</summary>
    /// <param name="type">type to search for</param>
    public IEnumerable<Vertex> GetLocationsOfType(VertexType type) => GetLocations().OfType<Vertex>().Where(vertex => vertex.Type == type);

    public override IEnumerable<BaseVertex> GetEmptyLocationsInSet(Searcher searcher, IItem itemToPlace, ItemSetName itemSet, Dictionary<ItemSetName, int> setCounts)
    {
        var locations = new List<BaseVertex>();
        var item = (Item)itemToPlace;

        bool onlyReachable = Config.Accessibility != AccessibilityOption.None || !searcher.HasFound(GetItem("Triforce"));
        locations.AddRange(searcher.GetEmptyLocationsInSet(itemSet, setCounts, onlyReachable));

        if (Config.Accessibility != AccessibilityOption.Locations && (item.Type == ItemType.SmallKey || item.Type == ItemType.BigKey))
        {
            if (KeyForKeys.TryGetValue(item, out var keyForKeys))
            {
                var chests = keyForKeys.Where(v => v.Chest.Item == null && (v.Regions.Count == 0 || v.Regions.Any(v2 => searcher.HasVisited(v2)))).Select(v => v.Chest);
                locations.AddRange(chests);
            }
        }

        return locations;
    }
    protected override bool ShouldTrack(BaseVertex location)
    {
        return location is Vertex { SubType: var subType } && subType is not VertexType.Medallion and not VertexType.Refill and not VertexType.Prize;
    }

    public override bool IsWinnable(BaseVertex start, Inventory startingInventory)
    {
        Searcher searcher = new(Graph, start, startingInventory);

        if (!searcher.HasFound(GetItem("Triforce")))
        {
#if DEBUG
            string[] interrestingItems =
            [
                "Crystal1", "Crystal2", "Crystal3", "Crystal4", "Crystal5", "Crystal6", "Crystal7",
                "PendantOfCourage", "PendantOfWisdom", "PendantOfPower",
                "AgahnimDefeated", "Agahnim2Defeated",
            ];
            foreach (string item in interrestingItems)
            {
                var worldItem = GetItem(item);
                Console.WriteLine("World {0}: {1} {2}obtainable at {3}",
                    Id,
                    item,
                    searcher.HasFound(worldItem) ? "" : "NOT ",
                    Graph.GetVertices().FirstOrDefault(v => v.World == (World?)this && v.Item == worldItem)?.Name);
            }
            string[] interrestingLocations =
            [
                "Ganon's Tower - Bob's Torch", "Ganon's Tower - Pre-Moldorm Chest", "Ganon's Tower - Moldorm Chest"
            ];
            foreach (string location in interrestingLocations)
            {
                var locationVertex = GetLocation(location);
                Console.WriteLine("World {0}: {1} {2}reachable at {3}",
                    Id,
                    locationVertex.Item?.Name ?? "location",
                    searcher.HasVisited(locationVertex) ? "" : "NOT ",
                    locationVertex.Name);
            }
#endif
            return false;
        }

        return true;
    }
}
