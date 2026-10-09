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
public sealed class World : World<Item>, IPortalHost
{
    public Config Config { get; }
    public (byte[] Underworld, byte[] Overworld, byte[] Special, byte[] Sets) SpriteSheets { get; set; } = ([], [], [], []);
    public Dictionary<IItem /* actualKey */, List<(BaseVertex Chest, List<BaseVertex> Regions)>> KeyForKeys { get; } = [];

    /// <summary>
    /// Cross-game portal anchors (see <see cref="Games.PortalAnchor"/>), materialized on
    /// demand by <see cref="Portals"/> when a cross-game edge touches an entrance's
    /// In/Out vertex — any entrance can host a portal.
    /// </summary>
    public List<PortalAnchor> PortalAnchors { get; } = [];

    public readonly Dictionary<string, Sprite> Sprites = [];
    public readonly Dictionary<string, SpriteProperties> SpriteData = [];


    public Sprite GetSprite(string name)
        => Sprites.GetValueOrDefault(name)
        ?? throw new ArgumentException($"No such sprite: {name}", nameof(name));

    public PortalAnchor ResolvePortalAnchor(BaseVertex vertex) => Portals.ResolveVertexAnchor(this, vertex.Name);

    /// <summary>Add all the vertices to the graph for this region.</summary>
    /// <param name="id">id of this world</param>
    /// <param name="randomizerConfig">options for this world</param>
    public World(int id, WorldConfig randomizerConfig, Graph graph, PRNG prng)
        : base(GameIds.Zelda3, id, graph, randomizerConfig)
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
            new EnemyStatRandomizer(),
            new BossShuffler(),
            new EnemyShuffler(),
            new BunnyGraphifier(),
            new PrizePackShuffler(),
            new DoorReplacer(),
            new DungeonPegStateCopier(),
        ];

        foreach (var modifier in modifiers)
            modifier.AdjustEdges(this, prng);


        // fetch and buildsprite data
        var sprInfo = Sprites;
        var sprProp = SpriteData;

        foreach (var (name, sprite) in YamlReader.LoadSpriteData())
        {
            SpriteProperties prop = SpriteProperties.FromYaml(name, sprite);
            sprProp.Add(name, prop); // using Dictionary.Add because we want to detect duplicate keys
        }

        foreach (var (name, sprite) in YamlReader.LoadSprites())
        {
            SpriteProperties? props;
            if (sprite.Properties is string propName)
            {
                if (sprProp.TryGetValue(propName, out var propsData))
                {
                    props = propsData;
                }
                else
                {
                    throw new InvalidOperationException($"{name} contains an invalid value for key 'properties': '{propName}");
                }
            }
            else
            {
                props = null;
            }

            Sprite toAdd = new(name, sprite.Id)
            {
                Sheets = sprite.Sheets,
                Flags = sprite.Flags,
                SubType = sprite.SubType,
                DefeatName = sprite.AlternativeName ?? name,
                FallingSpriteFor = sprite.FallingSpriteFor,
                Priority = sprite.Priority,
                NotWith = sprite.NotWith,
                Weight = sprite.Weight,
                Properties = props,
            };

            sprInfo.Add(name, toAdd); // using Dictionary.Add because we want to detect duplicate keys
        }
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
    public new IEnumerable<Vertex> GetLocationsOfType(VertexType type) => GetLocations().OfType<Vertex>().Where(vertex => vertex.Type == type);

    public override IEnumerable<BaseVertex> GetEmptyLocationsInSet(ISearcher searcher, IItem itemToPlace, ItemSetName itemSet, Dictionary<ItemSetName, int> setCounts)
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

    public override IEnumerable<IItem> GetVictoryItems() => [GetItem("Triforce")];

    public override ISearcher GetSearcherForWorld(Graph graph, BaseVertex? start, Inventory inventory, SetLocations? setLocations = null)
    {
        return new Searcher(graph, start ?? Start, inventory, setLocations, this);
    }
}
