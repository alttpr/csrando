namespace Randomizer.Games.Alttp;

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
    
    /// <summary>Mapping of dungeon ItemSet names to their small key items for DungeonKeySolver integration</summary>
    private static readonly Dictionary<string, string> DungeonKeyMapping = new()
    {
        ["escape"] = "KeyH2",
        ["eastern"] = "KeyP1", 
        ["desert"] = "KeyP2",
        ["hera"] = "KeyP3",
        ["agahnim"] = "KeyA1",
        ["pod"] = "KeyD1",
        ["swamp"] = "KeyD2",
        ["skull"] = "KeyD3",
        ["thieves"] = "KeyD4",
        ["ice"] = "KeyD5",
        ["mire"] = "KeyD6",
        ["turtlerock"] = "KeyD7",
        ["gt"] = "KeyA2"
    };

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
        
        // Check if this itemSet corresponds to a small-key dungeon that can benefit from DungeonKeySolver
        bool useDungeonKeySolver = onlyReachable && 
                                  itemSet.World != null && 
                                  DungeonKeyMapping.ContainsKey(itemSet.Name) &&
                                  item.Type != ItemType.SmallKey; // Don't apply to key placement itself to avoid recursion
        
        if (useDungeonKeySolver)
        {
            // Use DungeonKeySolver to filter locations to only safe ones
            var safeLocs = GetSafeLocationsForDungeon(searcher, itemSet.Name, setCounts);
            locations.AddRange(safeLocs.Where(loc => loc.Item == null));
        }
        else
        {
            // Use standard reachability logic
            locations.AddRange(searcher.GetEmptyLocationsInSet(itemSet, setCounts, onlyReachable));
        }

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
    
    /// <summary>
    /// Get safe item locations for a specific dungeon using DungeonKeySolver
    /// </summary>
    private IEnumerable<BaseVertex> GetSafeLocationsForDungeon(Searcher searcher, string dungeonName, Dictionary<ItemSetName, int> setCounts)
    {
        try
        {
            if (!DungeonKeyMapping.TryGetValue(dungeonName, out var keyItemName))
                return Enumerable.Empty<BaseVertex>();
            
            // Extract dungeon graph
            var dungeonGraph = Randomizer.Graph.DungeonGraphConverter.ExtractDungeonGraph(Graph, dungeonName, keyItemName);
            if (dungeonGraph.Nodes.Count == 0)
                return Enumerable.Empty<BaseVertex>();
            
            // Determine available small keys for this dungeon
            var keyItem = GetItem(keyItemName);
            int availableKeys = CalculateAvailableKeys(dungeonName, setCounts, keyItem);
            
            // Find dungeon entrances (start points for the solver)
            var entranceNodeIds = FindDungeonEntrances(dungeonGraph, searcher);
            if (entranceNodeIds.Count == 0)
                return Enumerable.Empty<BaseVertex>();
            
            // Create item check function
            Func<string, bool> itemCheck = req =>
            {
                if (req == "fixed" || req == "KEY") return true;
                var requiredItem = Graph.AllItems.FirstOrDefault(i => i.Name == req);
                return requiredItem != null && searcher.HasFound(requiredItem);
            };
            
            // Get safe location node IDs
            var safeNodeIds = Randomizer.Graph.DungeonKeySolverStatic.SafeItemLocationsForDungeon(
                dungeonGraph, dungeonName, entranceNodeIds, itemCheck, availableKeys);
            
            // Convert back to actual vertices
            var safeLocations = new List<BaseVertex>();
            foreach (var nodeId in safeNodeIds)
            {
                if (Graph.GetVertices().FirstOrDefault(v => v.Id == nodeId) is BaseVertex vertex)
                {
                    safeLocations.Add(vertex);
                }
            }
            
            return safeLocations;
        }
        catch (Exception ex)
        {
            // Log error and fall back to standard behavior
            // In production, you'd use proper logging
            Console.WriteLine($"DungeonKeySolver error for {dungeonName}: {ex.Message}");
            return Enumerable.Empty<BaseVertex>();
        }
    }
    
    /// <summary>
    /// Calculate how many small keys are available for placement in the given dungeon
    /// </summary>
    private int CalculateAvailableKeys(string dungeonName, Dictionary<ItemSetName, int> setCounts, IItem keyItem)
    {
        // Count how many keys are still to be placed for this dungeon
        var keySetName = new ItemSetName(dungeonName, this);
        if (setCounts.TryGetValue(keySetName, out var remainingKeys))
        {
            return remainingKeys;
        }
        
        // Fallback: count all keys for this dungeon that haven't been placed yet
        int totalKeys = 0;
        foreach (var vertex in Graph.GetVertices())
        {
            if (vertex.Item?.Name == keyItem.Name)
                totalKeys++;
        }
        
        return totalKeys;
    }
    
    /// <summary>
    /// Find entrance nodes for the dungeon based on reachable vertices
    /// </summary>
    private List<int> FindDungeonEntrances(DungeonGraph dungeonGraph, Searcher searcher)
    {
        var entrances = new List<int>();
        
        foreach (var node in dungeonGraph.Nodes)
        {
            var vertex = Graph.GetVertices().FirstOrDefault(v => v.Id == node.Id);
            if (vertex != null && searcher.HasVisited(vertex))
            {
                // This node is reachable from outside the dungeon, so it's an entrance
                entrances.Add(node.Id);
            }
        }
        
        // If no entrances found, try to find typical entrance vertices
        if (entrances.Count == 0)
        {
            foreach (var node in dungeonGraph.Nodes)
            {
                var vertex = Graph.GetVertices().FirstOrDefault(v => v.Id == node.Id);
                if (vertex?.Type == VertexType.Entrance)
                {
                    entrances.Add(node.Id);
                }
            }
        }
        
        return entrances;
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
