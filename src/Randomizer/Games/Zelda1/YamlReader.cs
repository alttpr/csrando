namespace Randomizer.Games.Zelda1;

using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Graph;
using YamlDotNet.Serialization;

public class YamlReader
{
    public YamlReader(Config config)
    {
        this.config = config;
    }

    private readonly Config config;
    private YamlData? data;
    private readonly Dictionary<string, Dictionary<string, object>> vertices = [];
    private readonly Dictionary<string, DirectedUndirectedPair> edges = [];

    public YamlData? Data { get { return data; } }

    private static Lazy<string> _dataRoot = new(() =>
    {
        DirectoryInfo? currentDirectory = new(Directory.GetCurrentDirectory());

        do
        {
            // First try the published output structure (Games/Zelda1/data)
            string publishedDataRoot = Path.Combine(currentDirectory.FullName, "Games/Zelda1/data");
            if (Directory.Exists(publishedDataRoot))
                return publishedDataRoot;

            // Then try the source structure (src/Randomizer/Games/Zelda1/data)
            string dataRoot = Path.Combine(currentDirectory.FullName, "src/Randomizer/Games/Zelda1/data");
            if (Directory.Exists(dataRoot))
                return dataRoot;

            currentDirectory = currentDirectory.Parent;
        } while (currentDirectory != null);

        throw new Exception("Could not find the data directory automatically. Set YamlReader.DataRoot before loading data.");
    });

    public static string DataRoot
    {
        get => _dataRoot.Value;
        set => _dataRoot = new(value);
    }

    public class Screen
    {
        public required string name;
        public Area area;
        public int screen;
        public required NodeCollection nodes;
        public required EdgeCollection edges;

        // Overworld walkability, generated offline from the vanilla ROM (decoded by
        // OverworldTilemap, emitted by the OverworldWalkabilityGenerator test). 11 rows of
        // 16 chars each: '.' = Link can stand here, '#' = blocked. Null for underworld
        // screens (which use the region model instead).
        public List<string>? walkable;

        // Square positions [col,row] where an entrance (stairs/cave/dungeon) is or
        // can be drawn on this overworld screen. Used to align cave/any-road exit
        // positions with the visible entrance.
        public List<int[]>? entrances;
    }

    public class NodeCollection
    {
        public required List<Exit> exits;
        public required List<Cave> caves;
        public required List<Region> regions;
        public required List<Meta> meta;
        public List<int[]>? blocks;
    }

    public class EdgeCollection
    {
        public required Dictionary<string, List<object>> undirected;
        public required Dictionary<string, List<object>> directed;
    }

    public class Exit
    {
        public required string name;
        public ExitType type;
        public Direction direction;
    }

    public class Cave
    {
        public required string name;
        public CaveType type;
    }

    public class Meta
    {
        public required string name;
        public MetaType type;
        public required string position;
        public string? item;
    }

    public class Region
    {
        public required string name;
        public RegionType type;
        public required int[] from;
        public required int[] to;
    }

    public enum MetaType
    {
        Meta,
        Item,
        Armos,
        Fairy,
        Push,
        Stairs
    }

    public enum CaveType
    {
        Bomb,
        Open,
        Grave,
        Tree,
        Push
    }

    public enum Area
    {
        Overworld,
        Underworld,
    }

    public enum ExitType
    {
        Scroll,
        Entrance,
        Exit
    }

    public enum RegionType
    {
        Region,
        NoPlace,
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
    }

    public class OverworldMap
    {
        public required string name;
        public required Area area;
        public required int map;
        public required int screen;
        public required int[] palettes;
        public required bool zora;
        public required bool waves;
        public required int enemies;
        public required int enemy_id;
        public required int enemy_sides;
        public required int enemy_mode;
        public required int cave;
        public required int stairs;
        public required int[] secret;
        public required int[] exit;
        public required int level_info_e;
    }

    public class UnderworldMap
    {
        public required string name;
        public required Area area;
        public required int map;
        public required int screen;
        public required int[] palettes;
        public required int[] doors;
        public required bool passage;
        public required int passage_left;
        public required int passage_right;
        public required int enemies;
        public required int enemy_id;
        public required int enemy_mode;
        public required bool push_block;
        public required bool dark_room;
        public required int boss_sfx;
        public required int room_item;
        public required int item_pos;
        public required int behaviour;
        public bool? level_nine_check;
        public bool generated;
        public int generated_level;
        public int local_room_id;
        public Dictionary<Direction, int>? neighbor_map_ids;
        // When true, this room's item location also joins the tighter z1d{level}m set so the
        // dungeon Map is forced here (MapPlacement Early/Closest). See DungeonBuilder.MarkMapEarlyRoom.
        public bool map_early;
    }

    public enum DoorType : int
    {
        Open = 0,
        Wall = 1,
        PassThrough = 2,
        PassThroughNoSound = 3,
        Bombable = 4,
        Locked = 5,
        Locked2 = 6,
        Shutter = 7
    }

    public enum RoomBehaviour : int
    {
        None = 0,
        // Secret trigger "all dead": opens shutters when every enemy is killed, but the
        // room item (if any) stays visible from the moment you enter. Use for pure shutter gates.
        KillForShutter = 1,
        Leader = 2,
        GetTriforceShutter = 3,
        PushBlockShutter = 4,
        PushBlockStairs = 5,
        LeaderShutters = 6,
        // Secret trigger "foes for item": opens shutters AND keeps the room item hidden until
        // every enemy is killed. This is the only non-boss trigger that actually hides an item.
        // (Engine also uses this for the boss-drop item.) See CreateRoomObjects in the Z1 disasm.
        KillForItem = 7
    }

    public class Level
    {
        public required string name;
        public required int level;
        public required Area area;
        public required int[] rooms;
        public required int[] enemy_counts;
        public required int start_room_id;
        public required int start_y;
        public required int boss_room_id;
        public required int triforce_room_id;
        public required int[] shortcut_or_item_pos_array;
        public required int[] cellar_room_id_array;
        public required int[] world_flags_addr;
        public required int[] submenu_map_mask;
        public required int submenu_map_rotation;
        public required int status_bar_map_x_offset;
        public required byte[] status_bar_map_transfer_buf;
        public required byte[] palettes_transfer_buf;
        public required byte[] palette_cycles;
        public required byte[] death_palette_series;
    }

    public class Special
    {
        public required int overworld_item_room;
        public required int overworld_item_x;
        public required int overworld_item_id;
        public required int armos_item_room;
        public required int armos_item_x;
        public required int armos_item_id;
        public required int[] armos_stairs;
        public required int[] armos_x_pos;
        public required int[] step_ladder;
        public required int[] recorder_stairs;
        public required int[] recorder_dests;
        public required int[] recorder_y_pos;
        public required int[] any_road_x_pos;
        public required int start;
    }

    public class CaveData
    {
        public required string name;
        public required int cave;
        public required int[] items;
        public required int[] flags;
        public required int[] prices;
        public required int text;

        // Set by ShopShuffler: a single-purchase shop that sells exactly one item (the middle slot)
        // and empties after purchase, so it can safely hold count/progression items without a
        // "wrong choice" softlock. Repeatable shops leave this false.
        public bool buyOnce;

        public CaveFlags Flag => (CaveFlags)((text & 0xC0) >> 6 | flags[2] >> 4 | flags[1] >> 2 | flags[0]);
    }

    [Flags]
    public enum CaveFlags : int
    {
        PickItem = 0x01,
        Shop = 0x02,
        ShowItems = 0x04,
        ShowPrices = 0x08,
        MoneyGame = 0x10,
        Hint = 0x20,
        HeartRequirement = 0x40,
        NegativeAmounts = 0x80,
    }

    public class YamlData
    {
        public required List<OverworldMap> overworld_maps;
        public required List<Screen> overworld_screens;

        public required List<UnderworldMap> underworld_maps;
        public required List<Screen> underworld_screens;

        public required List<Level> levels;
        public required List<CaveData> caves;
        public required Special special;
        public required EnemyData enemies;
    }

    public class EnemyData
    {
        public required List<Enemy> enemies;
        public required List<EnemyList> enemy_lists;
    }

    public class Enemy
    {
        public required int id;
        public required string name;
        public required List<int> allowed_levels;
        [YamlMember(Alias = "kill_items")]
        public List<string> kill_items { get; set; } = new();
    }

    public class EnemyList
    {
        public required int id;
        public required int address;
        public required List<int> data;
        public required List<int> allowed_levels;
    }

    private List<T> LoadFiles<T>(string path)
    {
        var yamlData = new List<T>();
        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();

        foreach (var fileName in Directory.GetFiles(path, "*.yml", SearchOption.AllDirectories))
        {
            var fileContents = File.ReadAllText(fileName);
            try
            {
                var data = deserializer.Deserialize<T>(fileContents);
                yamlData.Add(data);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while deserializing: {fileName}, {e.Message} ({e.InnerException?.Source ?? ""}");
            }
        }

        return yamlData;
    }

    private T LoadFile<T>(string path)
    {
        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();
        try
        {
            var data = deserializer.Deserialize<T>(File.ReadAllText(path));
            return data;
        }
        catch (Exception e)
        {
            throw new Exception($"Error while deserializing: {path}, {e.Message} ({e.InnerException?.Source ?? ""}");
        }
    }

    private Dictionary<string, object> CreateNode(Dictionary<string, object> nodeData)
    {
        vertices.Add((string)nodeData["name"], nodeData);
        return nodeData;
    }

    private Dictionary<string, object>? FindNode(string name)
    {
        if (vertices.TryGetValue(name, out var vertexData))
        {
            return vertexData;
        }

        return null;
    }

    private Dictionary<string, object> FindOrCreateNode(string name, string? item = null)
    {
        if (!vertices.TryGetValue(name, out var vertexData))
        {
            vertexData = new Dictionary<string, object>
            {
                { "name", name },
                { "type", VertexType.Meta },
                { "item", item! },
            };
            vertices.Add(name, vertexData);
        }

        return vertexData;
    }

    private void AddEdge(Dictionary<string, object> from, Dictionary<string, object> to, string edgeGroup, bool undirected = false)
    {
        if (!edges.TryGetValue(edgeGroup, out var edgePair))
        {
            edgePair = new DirectedUndirectedPair();
            edges.Add(edgeGroup, edgePair);
        }

        // Check if the edge already exists
        var edgeExists = undirected ? edgePair.Undirected.Any(e => e.SequenceEqual([(string)from["name"], (string)to["name"]])) :
                                      edgePair.Directed.Any(e => e.SequenceEqual([(string)from["name"], (string)to["name"]]));

        if (!edgeExists)
        {
            if (undirected)
            {
                edgePair.Undirected.Add([(string)from["name"], (string)to["name"]]);
            }
            else
            {
                edgePair.Directed.Add([(string)from["name"], (string)to["name"]]);
            }
        }
    }

    private void AddDirectedEdge(Dictionary<string, object> from, Dictionary<string, object> to, string edgeGroup)
    {
        AddEdge(from, to, edgeGroup);
    }

    private void AddUndirectedEdge(Dictionary<string, object> from, Dictionary<string, object> to, string edgeGroup)
    {
        AddEdge(from, to, edgeGroup, true);
    }

    private const string ItemsPath = "Items.yml";

    private static readonly Lazy<Dictionary<string, YamlItem>> _cachedItems = new(() =>
    {
        string itemsYML = Path.Combine(DataRoot, ItemsPath);

        var deserializer = new DeserializerBuilder().Build();
        using var reader = File.OpenText(itemsYML);
        var result = deserializer.Deserialize<Dictionary<string, YamlItem>>(reader);

        return result;
    });

    public static Dictionary<string, YamlItem> LoadItems() => _cachedItems.Value;

    public void LoadData()
    {
        var path = DataRoot;
        var levels = LoadFiles<Level>(Path.Combine(path, "Levels"));

        var overworldMaps = LoadFiles<OverworldMap>(Path.Combine(path, "Maps/Overworld"));
        var underworldMaps = LoadFiles<UnderworldMap>(Path.Combine(path, "Maps/Underworld"));

        var overworldScreens = LoadFiles<Screen>(Path.Combine(path, "Screens/Overworld"));
        var underworldScreens = ConvertUnderworldExits(LoadFiles<Screen>(Path.Combine(path, "Screens/Underworld")));

        var caves = LoadFile<List<CaveData>>(Path.Combine(path, "Caves.yml"));
        var special = LoadFile<Special>(Path.Combine(path, "Special.yml"));
        var enemies = LoadFile<EnemyData>(Path.Combine(path, "Enemies.yaml"));

        data = new YamlData()
        {
            levels = levels,
            overworld_maps = overworldMaps,
            underworld_maps = underworldMaps,
            overworld_screens = overworldScreens,
            underworld_screens = underworldScreens,
            special = special,
            caves = caves,
            enemies = enemies
        };

    }

    // Convert "Scroll" exits in underworld screens into entrance/exit node pairs so that
    // room-to-room connections can be modelled as directed edges (enter one room, leave another).
    private List<Screen> ConvertUnderworldExits(IEnumerable<Screen> screens)
    {
        return screens.Select(ConvertScrollNodes).ToList();
    }

    private Screen ConvertScrollNodes(Screen screen)
    {
        var convertedScreen = new Screen
        {
            name = screen.name,
            area = screen.area,
            screen = screen.screen,
            nodes = new NodeCollection
            {
                caves = screen.nodes.caves,
                meta = screen.nodes.meta,
                regions = screen.nodes.regions,
                blocks = screen.nodes.blocks,
                exits = new List<Exit>(screen.nodes.exits.Where(e => e.type != ExitType.Scroll))
            },
            edges = screen.edges
        };

        foreach (var node in screen.nodes.exits.Where(e => e.type == ExitType.Scroll).ToList())
        {
            // Create entrance/exit pair for this direction
            var entranceNode = new Exit
            {
                name = $"{node.name} - Entrance",
                type = ExitType.Entrance,
                direction = node.direction
            };

            var exitNode = new Exit
            {
                name = $"{node.name} - Exit",
                type = ExitType.Exit,
                direction = node.direction
            };

            // Replace the scroll node with the entrance node, and add the exit node to the list
            convertedScreen.nodes.exits.Add(entranceNode);
            convertedScreen.nodes.exits.Add(exitNode);

            // Create a new meta node for the old scroll node that links to both exit/entrance nodes
            convertedScreen.nodes.meta ??= new List<Meta>();
            convertedScreen.nodes.meta.Add(new Meta
            {
                name = node.name,
                type = MetaType.Meta,
                position = ""
            });

            // Route the entrance through the meta node and on to the exit (entrance -> meta -> exit)
            convertedScreen.edges.directed ??= new Dictionary<string, List<object>>();
            if (!convertedScreen.edges.directed.TryGetValue("fixed", out var directedEdges))
            {
                directedEdges = [];
                convertedScreen.edges.directed["fixed"] = directedEdges;
            }

            directedEdges.Add(new List<object> { entranceNode.name, node.name });
            directedEdges.Add(new List<object> { node.name, exitNode.name });
        }

        return convertedScreen;
    }

    public int GetStartMap()
    {
        return data?.special?.start ?? 0;
    }

    public Dictionary<string, DirectedUndirectedPair> GetEdges(World world)
    {
        return edges.Select(e => { e.Value.Directed = e.Value.Directed.Select(d => { d[0] = $"{d[0]}"; d[1] = $"{d[1]}"; return d; }).ToList(); e.Value.Undirected = e.Value.Undirected.Select(d => { d[0] = $"{d[0]}"; d[1] = $"{d[1]}"; return d; }).ToList(); return e; }).ToDictionary(e => $"{e.Key}", e => e.Value);
    }

    public List<Dictionary<string, object>> GetVertices(World world)
    {
        return vertices.Values.Select(v => { v["name"] = $"{v["name"]}"; return v; }).ToList();
    }

    public void BuildGraph(IReadOnlySet<int>? reservedPortalCaveMaps = null)
    {
        if (data == null)
        {
            throw new Exception("Data not loaded");
        }

        reservedPortalCaveMaps ??= new HashSet<int>();

        BuildEnemyGraph();

        foreach (var map in data.overworld_maps)
        {
            BuildOverworldMap(map, reservedPortalCaveMaps);
        }

        foreach (var level in data.levels.Where(l => l.level > 0))
        {
            foreach (var map in data.underworld_maps.Where(m => level.rooms.Contains(m.map)))
            {
                BuildUnderworldMap(map, level);
            }

            // Connect level entrance
            var levelEntranceNode = FindOrCreateNode($"Level {level.level} - Entrance");

            // Get start room from map
            var startMap = data.underworld_maps.Where(m => m.area == Area.Underworld && m.map == level.start_room_id).First();
            var startScreen = data.underworld_screens.Where(s => s.area == startMap.area && s.screen == startMap.screen).First();

            var startNode = startScreen.nodes.exits.Where(e => e.direction == Direction.Down && e.type == ExitType.Entrance).First();
            var startExitNode = startScreen.nodes.exits.Where(e => e.direction == Direction.Down && e.type == ExitType.Exit).First();

            var levelRequirement = level.level switch
            {
                9 => "Triforce|" + config.Triforces.ToString(),
                _ => "fixed"
            };

            AddDirectedEdge(levelEntranceNode, FindOrCreateNode($"{startMap.area} - {level.name} - {startMap.name} - {startNode.name}"), levelRequirement);
            AddDirectedEdge(FindOrCreateNode($"{startMap.area} - {level.name} - {startMap.name} - {startExitNode.name}"), levelEntranceNode, "fixed");
        }

        var farmingWeapons = GetFarmingWeapons();

        // Base PC address of the relocated extended cave items table ($8A9600). The ASM cave-load
        // hook reads every cave's wares from here, so all cave item locations (take-one/take-any and
        // shop) write their item byte at CaveShopItemsBase + (cave - 0x10) * 3 + slot. Keep in sync
        // with Rom.CaveShopItemsBase.
        const int caveShopItemsBase = 0x651600;

        foreach (var cave in data.caves)
        {
            var caveEntranceNode = FindNode($"Cave {cave.cave:X2} - Entrance");
            var caveNode = FindOrCreateNode($"Cave {cave.cave:X2}");

            if (caveEntranceNode == null)
            {
                // With shop shuffle, a cave can be left without an entrance (a synthesized shop, or a
                // vanilla shop ID whose only screen got reassigned) — just skip it. With it off, every
                // vanilla cave must have an entrance, so a missing one is a real data error.
                if (config.ShopShuffle != ShopShuffleOption.Off)
                    continue;
                throw new Exception($"Cave {cave.cave:X2} entrance not found");
            }

            AddDirectedEdge(caveEntranceNode, caveNode, "fixed");

            // Take-one / take-any caves: pick up an item for free (no rupee cost).
            // (Heart-requirement caves like the white/magical sword still gate on heart count.)
            if (cave.Flag.HasFlag(CaveFlags.PickItem) && cave.Flag.HasFlag(CaveFlags.ShowItems) && !cave.Flag.HasFlag(CaveFlags.Shop) && !cave.Flag.HasFlag(CaveFlags.MoneyGame) && !cave.Flag.HasFlag(CaveFlags.Hint))
            {
                bool takeAny = cave.items.Count(i => i != 0x2F) > 1;
                var caveTypeName = takeAny ? "Take Any Item" : "Take One Item";
                var caveItemSet = takeAny ? "z1takeany" : "z1takeone";
                int caveItemIndex = 0;
                foreach (var item in cave.items)
                {
                    if (item != 0x2F)
                    {
                        // Leave the item unset; the ItemPooler fills these (PRNG-seeded) so the
                        // contents vary between caves and are reproducible per seed.
                        var itemNode = CreateNode(new()
                        {
                            { "name", $"Cave {cave.cave:X2} - {caveTypeName} - Item {caveItemIndex:X2}" },
                            { "type", VertexType.Item },
                            { "address", caveShopItemsBase + ((cave.cave - 0x10) * 3) + caveItemIndex },
                            { "itemset", (string[])["zelda", $"z1c{cave.cave:X2}", caveItemSet] },
                        });

                        if (cave.Flag.HasFlag(CaveFlags.HeartRequirement))
                        {
                            var heartRequirement = cave.cave switch
                            {
                                0x12 => "HeartContainer|2",
                                0x13 => "HeartContainer|9",
                                _ => "fixed"
                            };
                            AddDirectedEdge(caveNode, itemNode, heartRequirement);
                        }
                        else
                        {
                            AddDirectedEdge(caveNode, itemNode, "fixed");
                        }
                    }
                    caveItemIndex++;
                }
            }
            // Shop caves (built as logical locations only under shop shuffle). Rupees are infinitely
            // farmable, so the price gates nothing — the real requirement is a farming weapon to earn
            // rupees, expressed as parallel edges (any one weapon satisfies the OR).
            else if (config.ShopShuffle != ShopShuffleOption.Off
                     && cave.Flag.HasFlag(CaveFlags.Shop)
                     && cave.Flag.HasFlag(CaveFlags.ShowItems)
                     && !cave.Flag.HasFlag(CaveFlags.MoneyGame)
                     && !cave.Flag.HasFlag(CaveFlags.Hint))
            {
                if (cave.buyOnce)
                {
                    // Single-purchase shop: exactly one item (the middle slot, index 1) so the player
                    // can't take a "wrong" item and strand progression, and the cave empties after one
                    // buy. Safe to hold count/progression items. Full -> main pool; Junk -> consumables.
                    const int buyOnceSlot = 1;
                    var shopItemSet = config.ShopShuffle == ShopShuffleOption.Full ? "z1shop" : "z1shopjunk";
                    var itemNode = CreateNode(new()
                    {
                        { "name", $"Cave {cave.cave:X2} - Shop - Item {buyOnceSlot:X2}" },
                        { "type", VertexType.Item },
                        { "address", caveShopItemsBase + ((cave.cave - 0x10) * 3) + buyOnceSlot },
                        { "itemset", (string[])["zelda", $"z1c{cave.cave:X2}", shopItemSet] },
                    });
                    foreach (var weapon in farmingWeapons)
                    {
                        AddDirectedEdge(caveNode, itemNode, weapon);
                    }
                }
                else
                {
                    // Repeatable shop: each non-empty slot is an independent purchasable location.
                    // Because purchases repeat, the stock is restricted to repeatable-safe items
                    // (uniques + consumables, no count items) via the z1shoprepeat set; Junk uses the
                    // consumables-only set.
                    var shopItemSet = config.ShopShuffle == ShopShuffleOption.Full ? "z1shoprepeat" : "z1shopjunk";
                    int caveItemIndex = 0;
                    foreach (var item in cave.items)
                    {
                        if (item != 0x2F)
                        {
                            var itemNode = CreateNode(new()
                            {
                                { "name", $"Cave {cave.cave:X2} - Shop - Item {caveItemIndex:X2}" },
                                { "type", VertexType.Item },
                                { "address", caveShopItemsBase + ((cave.cave - 0x10) * 3) + caveItemIndex },
                                { "itemset", (string[])["zelda", $"z1c{cave.cave:X2}", shopItemSet] },
                            });
                            foreach (var weapon in farmingWeapons)
                            {
                                AddDirectedEdge(caveNode, itemNode, weapon);
                            }
                        }
                        caveItemIndex++;
                    }
                }
            }
        }
    }

    // Weapons that can farm rupees indefinitely: free to use (unlike arrows), reusable (unlike bombs),
    // and able to kill the common overworld enemies. GetFarmingWeapons further filters this by the
    // enemy kill-item data.
    private static readonly HashSet<string> FreeInfiniteWeapons =
        ["SwordL1", "SwordL2", "SwordL3", "Rod", "RedCandle"];

    private List<string> GetFarmingWeapons()
    {
        if (data == null)
            throw new Exception("Data not loaded");

        // Overworld enemies that reliably drop rupees and appear in farmable spots.
        var overworldEnemies = data.enemies.enemies
            .Where(e => e.allowed_levels.Contains(0) && e.kill_items.Count > 0)
            .ToList();

        // A weapon qualifies if it is in the free+infinite allow-list AND can kill at least most of
        // the overworld enemies (so the player isn't stuck farming a single rare spawn).
        var candidates = FreeInfiniteWeapons
            .Where(w => overworldEnemies.Count(e => e.kill_items.Contains(w)) >= overworldEnemies.Count / 2)
            .ToList();

        // Fall back to the allow-list if the data ever fails to yield anything, so shops never become
        // unreachable in logic by accident.
        return candidates.Count > 0 ? candidates : FreeInfiniteWeapons.ToList();
    }

    private void BuildOverworldMap(OverworldMap map, IReadOnlySet<int> reservedPortalCaveMaps)
    {
        if (data == null)
        {
            throw new Exception("Data not loaded");
        }

        var level = data.levels.Single(l => l.level == 0 && l.area == Area.Overworld);
        var screen = data.overworld_screens.First(s => s.area == map.area && s.screen == map.screen);
        var mapName = $"{map.area} - {map.name}";

        var exits = new List<Direction>();
        foreach (var exit in screen.nodes.exits ?? [])
        {
            var exitName = $"{mapName} - {exit.name}";
            var exitNode = FindOrCreateNode(exitName);
            exits.Add(exit.direction);
        }

        // Level-info F bits 0x80 (secret[1]) and 0x40 (secret[0]) are the quest "secret detection"
        // flags. An entrance present ONLY in second quest is secret[1] == 1 with secret[0] == 0; the
        // overworld level-info table is shared between quests, so these screens still carry a cave
        // node (e.g. the burn-bush Level 8 entrance on screen 0x67), but in first quest the engine
        // never reveals them and the player cannot enter. Skip wiring those into the logic graph so
        // they don't become phantom dungeon/cave entrances. A screen flagged for BOTH quests
        // (secret[1] == 1 AND secret[0] == 1) is reachable in first quest, so it is kept.
        bool isSecondQuestOnlyEntrance = map.secret[1] == 1 && map.secret[0] == 0;
        bool suppressCaveContents = config.ShopShuffle != ShopShuffleOption.Off
            && reservedPortalCaveMaps.Contains(map.map);

        foreach (var cave in screen.nodes.caves ?? [])
        {
            if (!isSecondQuestOnlyEntrance && map.cave > 0 && (cave.type == CaveType.Open || cave.type == CaveType.Push || cave.type == CaveType.Bomb || cave.type == CaveType.Tree || cave.type == CaveType.Grave || map.secret[0] == 1))
            {
                var caveName = $"{mapName} - {cave.name}";
                var caveNode = FindOrCreateNode(caveName);
            }
        }

        foreach (var meta in screen.nodes.meta ?? [])
        {
            if (meta.type == MetaType.Armos)
            {
                if (data.special.armos_stairs.Contains(map.map) || data.special.armos_item_room == map.map)
                {
                    var metaName = $"{mapName} - {meta.name}";
                    var metaNode = FindOrCreateNode(metaName);
                }
            }
            else
            {
                var metaName = $"{mapName} - {meta.name}";
                var metaNode = FindOrCreateNode(metaName, meta.item);
            }
        }

        // Add all the undirected edges in the room
        foreach (var undirected in screen.edges.undirected ?? [])
        {
            var requirement = undirected.Key;
            var edges = undirected.Value;
            foreach (List<object> edge in edges ?? [])
            {
                var fromString = (string)edge[0];
                var toString = (string)edge[1];

                var fromName = $"{mapName} - {fromString}";
                var toName = $"{mapName} - {toString}";

                var fromNode = FindNode(fromName);
                var toNode = FindNode(toName);

                if (fromNode != null && toNode != null)
                {
                    AddUndirectedEdge(fromNode, toNode, requirement);
                }
            }
        }

        // Add all the directed edges in the room
        foreach (var directed in screen.edges.directed ?? [])
        {
            var requirement = directed.Key;
            var edges = directed.Value;
            foreach (List<object> edge in edges ?? [])
            {
                var fromString = (string)edge[0];
                var toString = (string)edge[1];

                var fromName = $"{mapName} - {fromString}";
                var toName = $"{mapName} - {toString}";

                var fromNode = FindNode(fromName);
                var toNode = FindNode(toName);

                if (fromNode != null && toNode != null)
                {
                    AddDirectedEdge(fromNode, toNode, requirement);
                }
            }
        }

        // Connect caves (skip second-quest-only hidden entrances; see isSecondQuestOnlyEntrance above).
        // Combo portal caves are also skipped when shop shuffle is active: the cave-entry hook sends
        // the player through the portal instead of into the cave, so exposing a shuffled shop here
        // would create a logical item location that cannot be collected in-game.
        if (map.cave > 0 && !isSecondQuestOnlyEntrance && !suppressCaveContents)
        {
            var cave = screen.nodes.caves?.FirstOrDefault() ?? null;
            if (cave != null)
            {
                if (cave.type == CaveType.Open || cave.type == CaveType.Push || cave.type == CaveType.Bomb || cave.type == CaveType.Tree || cave.type == CaveType.Grave || map.secret[0] == 1)
                {
                    if (map.cave < 10)
                    {
                        // This is a dungeon entrance
                        var dungeonStartNode = FindOrCreateNode($"Level {map.cave} - Entrance");
                        var caveNode = FindOrCreateNode($"{mapName} - {cave.name}");

                        AddUndirectedEdge(caveNode, dungeonStartNode, "fixed");
                    }
                    else
                    {
                        var caveEntranceNode = FindOrCreateNode($"Cave {map.cave:X2} - Entrance");
                        var caveNode = FindOrCreateNode($"{mapName} - {cave.name}");

                        AddDirectedEdge(caveNode, caveEntranceNode, "fixed");
                    }
                }
            }
        }

        // Connect stairs
        if (data.special.armos_stairs.Contains(map.map))
        {
            var armosScreenNode = screen.nodes.meta!.First(m => m.type == MetaType.Armos);
            var armosNode = FindOrCreateNode($"{mapName} - {armosScreenNode.name}");
            var caveNodeName = map.cave switch
            {
                <= 10 => $"Level {map.cave} - Entrance",
                _ => $"Cave {map.cave:X2} - Entrance"
            };

            var caveNode = FindOrCreateNode(caveNodeName);

            AddDirectedEdge(armosNode, caveNode, "fixed");
            if (map.cave <= 10)
            {
                AddDirectedEdge(caveNode, armosNode, "fixed");
            }
        }

        // Connect armos item
        if (data.special.armos_item_room == map.map)
        {
            var armosScreenNode = screen.nodes.meta!.First(m => m.type == MetaType.Armos);
            var armosNode = FindOrCreateNode($"{mapName} - {armosScreenNode.name}");

            var itemNode = CreateNode(new()
            {
                { "name", $"{mapName} - {armosScreenNode.name} - Item" },
                { "type", VertexType.Item },
                { "item", null! },
                { "address", 0x620CF5 },
                { "itemset", (string[])["zelda"] },
            });

            AddDirectedEdge(armosNode, itemNode, "fixed");
        }

        // Connect overworld item
        if (data.special.overworld_item_room == map.map)
        {
            var itemScreenNode = screen.nodes.meta!.First(m => m.type == MetaType.Item);
            var itemNode = FindOrCreateNode($"{mapName} - {itemScreenNode.name}");

            var overworldNode = CreateNode(new()
            {
                { "name", $"{mapName} - {itemScreenNode.name} - Item" },
                { "type", VertexType.Item },
                { "item", null! },
                { "address", 0x62B88A },
                { "itemset", (string[])["zelda"] },
            });

            AddDirectedEdge(itemNode, overworldNode, "fixed");
        }

        // Connect overworld recorder stairs
        if (data.special.recorder_stairs.Contains(map.map))
        {
            var firstExitScreenNode = screen.nodes.exits!.First();
            var firstExitNode = FindOrCreateNode($"{mapName} - {firstExitScreenNode.name}");
            var recorderNode = FindOrCreateNode($"{mapName} - Recorder Stairs");

            var caveNodeName = map.cave switch
            {
                <= 10 => $"Level {map.cave} - Entrance",
                _ => $"Cave {map.cave:X2} - Entrance"
            };

            var caveNode = FindOrCreateNode(caveNodeName);

            AddDirectedEdge(firstExitNode, recorderNode, "Recorder");
            AddDirectedEdge(recorderNode, caveNode, "fixed");

            if (map.cave <= 10)
            {
                AddDirectedEdge(caveNode, firstExitNode, "fixed");
            }
        }

        foreach (var exit in exits)
        {
            var offset = exit switch
            {
                Direction.Left => -1,
                Direction.Right => 1,
                Direction.Up => -16,
                Direction.Down => 16,
                _ => throw new Exception("Invalid exit direction")
            };

            var targetMap = data.overworld_maps.FirstOrDefault(m => m.area == map.area && m.map == map.map + offset);
            if (targetMap != null)
            {
                ConnectOWMaps(map, targetMap, exit);
            }
        }
    }

    private void ConnectOWMaps(OverworldMap from, OverworldMap to, Direction direction)
    {
        var oppositeDirection = direction switch
        {
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            _ => throw new Exception("Invalid exit direction")
        };

        var currentScreen = data!.overworld_screens.First(s => s.area == from.area && s.screen == from.screen);
        var currentExits = currentScreen.nodes.exits.Where(e => e.direction == direction).ToList();

        var targetScreen = data.overworld_screens.First(s => s.area == to.area && s.screen == to.screen);
        var targetExits = targetScreen.nodes.exits.Where(e => e.direction == oppositeDirection).ToList();

        // Create undirected connections between all these exits
        foreach (var currentExit in currentExits)
        {
            foreach (var targetExit in targetExits)
            {
                var currentExitNode = FindOrCreateNode($"{from.area} - {from.name} - {currentExit.name}");
                var targetExitNode = FindOrCreateNode($"{to.area} - {to.name} - {targetExit.name}");

                AddUndirectedEdge(currentExitNode, targetExitNode, "fixed");
            }
        }
    }

    private void BuildUnderworldMap(UnderworldMap map, Level level)
    {
        var mapName = $"{map.area} - {level.name} - {map.name}";

        if (!map.passage)
        {
            var screen = data!.underworld_screens.First(s => s.area == map.area && s.screen == map.screen);

            // Go through doors and create nodes for them
            for (int i = 0; i <= 3; i++)
            {
                var door = (DoorType)map.doors[i];
                var direction = (Direction)i;

                // Pre-create the entrance node so the screen's intra-room edges (wired below
                // with FindNode) can reference it, and so neighbouring rooms can connect to it.
                var entrance = screen.nodes.exits.FirstOrDefault(e => e.direction == direction && e.type == ExitType.Entrance);
                if (entrance != null)
                {
                    FindOrCreateNode($"{mapName} - {entrance.name}");
                }

                // Find an exit node that matches this door
                var exit = screen.nodes.exits.FirstOrDefault(e => e.direction == direction && e.type == ExitType.Exit);
                if (exit == null)
                {
                    continue;
                }

                var exitName = $"{mapName} - {exit.name}";
                var exitNode = FindOrCreateNode(exitName);

                if (door == DoorType.Wall)
                {
                    continue;
                }

                // Connect this exit to the other room
                string? doorKillRequirement = null;
                if (door == DoorType.Shutter && (map.behaviour == (int)RoomBehaviour.KillForShutter || map.behaviour == (int)RoomBehaviour.KillForItem))
                {
                    doorKillRequirement = CreateRoomClearLogic(map, mapName);
                }

                ConnectUWMaps(map, exitNode, door, direction, level, doorKillRequirement);
            }

            foreach (var meta in screen.nodes.meta ?? [])
            {
                var metaName = $"{mapName} - {meta.name}";
                var metaNode = FindOrCreateNode(metaName);

                if (meta.name == "Triforce" && level.triforce_room_id == map.map)
                {
                    // Create a Triforce Item Node and link it up
                    var triforceNode = CreateNode(new()
                    {
                        { "name", $"{mapName} - {meta.name} - Triforce" },
                        { "type", VertexType.Meta },
                        { "item", "Triforce" },
                        { "itemset", (string[])["zelda", "z1triforce"] },
                    });

                    AddDirectedEdge(metaNode, triforceNode, "fixed");
                }

                if (meta.name == "Zelda" && level.triforce_room_id == map.map)
                {
                    // Create a Zelda Item Node and link it up
                    var zeldaNode = CreateNode(new()
                    {
                        { "name", $"{mapName} - {meta.name} - Zelda" },
                        { "type", VertexType.Meta },
                        { "item", "Zelda" },
                        { "itemset", (string[])["zelda"] },
                    });

                    AddDirectedEdge(metaNode, zeldaNode, "fixed");
                }

                if (meta.name == "Ganon")
                {
                    var ganonNode = CreateNode(new()
                    {
                        { "name", $"{mapName} - {meta.name} - Ganon" },
                        { "type", VertexType.Meta },
                        { "item", "GanonTriforce" },
                        { "itemset", (string[])["zelda"] },
                    });

                    AddDirectedEdge(metaNode, ganonNode, "fixed");
                }
            }

            // Create all region nodes
            foreach (var region in screen.nodes.regions)
            {
                var regionName = $"{mapName} - {region.name}";
                var regionNode = FindOrCreateNode(regionName);
            }

            // Does this room have an item? (This should be 2F when writing back combo data)
            if (map.room_item != 0x03 && level.triforce_room_id != map.map && map.screen != 0x28)
            {
                var itemName = (RoomBehaviour)map.behaviour switch
                {
                    RoomBehaviour.KillForItem => (level.boss_room_id == map.map) ? $"{mapName} - Boss - Item" : $"{mapName} - Kill - Item",
                    _ => $"{mapName} - Item"
                };

                var roomItemNode = CreateNode(new()
                {
                    { "name", itemName },
                    { "type", VertexType.Item },
                    { "item", null! },
                    { "address", map.generated ? 0x651000 + map.generated_level * 0x80 + map.local_room_id : 0x650000 + map.map },
                    { "itemset", BuildDungeonItemSet(level.level, map.map_early) },
                });


                // Look up item position for this room
                var levelItemPositions = level.shortcut_or_item_pos_array[map.item_pos];
                var itemX = (levelItemPositions >> 4) - 2;
                var itemY = (levelItemPositions & 0x0F) - 6;

                // Find the region on the screen that contains the coordinates of the item
                // A region has a from [x,y] and a to [x,y] coordinate that defines a rectangle

                var itemRegionNode = screen.nodes.regions.FirstOrDefault(r => r.from[0] <= itemX && r.from[1] <= itemY && r.to[0] >= itemX && r.to[1] >= itemY)
                    ?? throw new Exception($"Could not find region for item in room {mapName}");
                var itemRegionNodeName = $"{mapName} - {itemRegionNode.name}";
                var itemRegionNodeNode = FindOrCreateNode(itemRegionNodeName);

                var roomItemRequirement = (RoomBehaviour)map.behaviour switch
                {
                    RoomBehaviour.KillForItem => CreateRoomClearLogic(map, mapName),
                    _ => "fixed"
                };

                AddDirectedEdge(itemRegionNodeNode, roomItemNode, roomItemRequirement);
            }

            // Add all undirected edges in the room
            foreach (var undirected in screen.edges.undirected ?? [])
            {
                var requirement = undirected.Key;
                var edges = undirected.Value;
                foreach (List<object> edge in edges ?? [])
                {
                    var fromString = (string)edge[0];
                    var toString = (string)edge[1];

                    var fromName = $"{mapName} - {fromString}";
                    var toName = $"{mapName} - {toString}";

                    var fromNode = FindNode(fromName);
                    var toNode = FindNode(toName);

                    if (fromNode != null && toNode != null)
                    {
                        AddUndirectedEdge(fromNode, toNode, requirement);
                    }
                }
            }

            // Add all directed edges in the room
            foreach (var directed in screen.edges.directed ?? [])
            {
                var requirement = directed.Key;
                var edges = directed.Value;
                foreach (List<object> edge in edges ?? [])
                {
                    var fromString = (string)edge[0];
                    var toString = (string)edge[1];

                    var fromName = $"{mapName} - {fromString}";
                    var toName = $"{mapName} - {toString}";

                    var fromNode = FindNode(fromName);
                    var toNode = FindNode(toName);

                    if (fromNode != null && toNode != null)
                    {
                        AddDirectedEdge(fromNode, toNode, requirement);
                    }
                }
            }
        }
        else
        {
            if (map.screen == 0x3E)
            {
                // This is a passage
                // For generated maps, passage_left/right already contain full map IDs; skip +0x80 adjustment
                var left_room = map.generated ? map.passage_left : map.passage_left + (level.level >= 7 ? 0x80 : 0x00);
                var right_room = map.generated ? map.passage_right : map.passage_right + (level.level >= 7 ? 0x80 : 0x00);

                // Create nodes for left and right
                var leftNode = FindOrCreateNode($"{mapName} - Passage - Left");
                var rightNode = FindOrCreateNode($"{mapName} - Passage - Right");

                // Connect the left and right nodes to the left and right room stairs
                var leftRoom = data!.underworld_maps.Where(m => m.area == map.area && m.map == left_room).First();
                var leftStairsName = FindStairsOrRegionName(leftRoom, level);
                var leftStairsRequirement = GetStairsAccessRequirement(leftRoom, level);

                var rightRoom = data.underworld_maps.Where(m => m.area == map.area && m.map == right_room).First();
                var rightStairsName = FindStairsOrRegionName(rightRoom, level);
                var rightStairsRequirement = GetStairsAccessRequirement(rightRoom, level);

                // Going into a hidden-stairs room may require a push block, but leaving the passage never should.
                ConnectPassageToRoom(leftNode, FindOrCreateNode($"{map.area} - {level.name} - {leftRoom.name} - {leftStairsName}"), leftStairsRequirement);
                ConnectPassageToRoom(rightNode, FindOrCreateNode($"{map.area} - {level.name} - {rightRoom.name} - {rightStairsName}"), rightStairsRequirement);

                // Connect the passage nodes
                AddUndirectedEdge(leftNode, rightNode, "fixed");

            }
            else
            {
                // This is an item room
                var left_room = map.generated ? map.passage_left : map.passage_left + (level.level >= 7 ? 0x80 : 0x00);

                // Create nodes for left and the item in the room
                var leftNode = FindOrCreateNode($"{mapName} - Passage - Left");
                var itemNode = CreateNode(new()
                {
                    { "name", $"{mapName} - Passage - Item" },
                    { "type", VertexType.Item },
                    { "item", null! },
                    { "address", map.generated ? 0x651000 + map.generated_level * 0x80 + map.local_room_id : 0x650000 + map.map },
                    { "itemset", BuildDungeonItemSet(level.level, map.map_early) },
                });

                // Connect the left and right nodes to the left
                var leftRoom = data!.underworld_maps.Where(m => m.area == map.area && m.map == left_room).First();
                var leftStairsName = FindStairsOrRegionName(leftRoom, level);
                var leftStairsRequirement = GetStairsAccessRequirement(leftRoom, level);
                ConnectPassageToRoom(leftNode, FindOrCreateNode($"{map.area} - {level.name} - {leftRoom.name} - {leftStairsName}"), leftStairsRequirement);

                // Connect the left node to the item
                AddUndirectedEdge(leftNode, itemNode, "fixed");


            }
        }
    }

    /// <summary>
    /// Item-set names for a dungeon item location. Every dungeon location carries z1d{level};
    /// "map early" locations additionally carry z1d{level}m, the tighter set the Map is pooled
    /// into when MapPlacement is enabled (see ItemPooler and DungeonBuilder.MarkMapEarlyRoom).
    /// </summary>
    private static string[] BuildDungeonItemSet(int level, bool mapEarly) =>
        mapEarly
            ? ["zelda", $"z1d{level}", $"z1d{level}m"]
            : ["zelda", $"z1d{level}"];

    /// <summary>
    /// Finds the Stairs meta node name on a room's screen, or falls back to a region name.
    /// Used when connecting passage/cellar rooms to their parent rooms.
    /// </summary>
    private string FindStairsOrRegionName(UnderworldMap room, Level level)
    {
        var screen = data!.underworld_screens.First(s => s.area == room.area && s.screen == room.screen);
        var behaviour = (RoomBehaviour)room.behaviour;

        // Try to find a Stairs meta node
        var stairs = (screen.nodes.meta ?? []).FirstOrDefault(m => m.type == MetaType.Stairs);
        if (stairs != null)
            return stairs.name;

        if (behaviour == RoomBehaviour.PushBlockStairs)
        {
            var itemPosition = level.shortcut_or_item_pos_array[room.item_pos];
            var itemX = (itemPosition >> 4) - 2;
            var itemY = (itemPosition & 0x0F) - 6;

            var hiddenStairsRegion = screen.nodes.regions
                .FirstOrDefault(r => r.from[0] <= itemX && r.from[1] <= itemY && r.to[0] >= itemX && r.to[1] >= itemY);
            if (hiddenStairsRegion != null)
                return hiddenStairsRegion.name;
        }

        // Fallback: find a region covering the top-right area (11,0)
        var region = screen.nodes.regions
            .FirstOrDefault(r => r.from[0] <= 11 && r.from[1] <= 0 && r.to[0] >= 11 && r.to[1] >= 0);
        if (region != null)
            return region.name;

        // Final fallback: use the first region
        if (screen.nodes.regions.Count > 0)
            return screen.nodes.regions[0].name;

        throw new Exception($"No stairs or regions found on screen {room.screen:X2} for room {room.name}");
    }

    private string GetStairsAccessRequirement(UnderworldMap room, Level level)
    {
        var behaviour = (RoomBehaviour)room.behaviour;
        if (behaviour != RoomBehaviour.PushBlockStairs)
            return "fixed";

        var roomName = $"{room.area} - {level.name} - {room.name}";
        return CreatePushBlockLogic(room, roomName);
    }

    private void ConnectPassageToRoom(Dictionary<string, object> passageNode, Dictionary<string, object> roomNode, string requirement)
    {
        AddDirectedEdge(roomNode, passageNode, requirement);
        AddDirectedEdge(passageNode, roomNode, "fixed");
    }

    private void ConnectUWMaps(UnderworldMap from, Dictionary<string, object> exitNode, DoorType door, Direction direction, Level level, string? killRequirement = null)
    {
        var offset = direction switch
        {
            Direction.Left => -1,
            Direction.Right => 1,
            Direction.Up => -16,
            Direction.Down => 16,
            _ => throw new Exception("Invalid exit direction")
        };

        var oppositeDirection = direction switch
        {
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            _ => throw new Exception("Invalid exit direction")
        };

        var sourceBehaviour = (RoomBehaviour)from.behaviour;
        var fromMapName = $"{from.area} - {level.name} - {from.name}";

        // Use neighbor_map_ids for generated maps, otherwise fall back to offset-based calculation
        UnderworldMap? target;
        if (from.neighbor_map_ids != null && from.neighbor_map_ids.TryGetValue(direction, out int neighborMapId))
        {
            target = data!.underworld_maps.FirstOrDefault(m => m.area == from.area && m.map == neighborMapId && level.rooms.Contains(m.map));
        }
        else
        {
            target = data!.underworld_maps.FirstOrDefault(m => m.area == from.area && m.map == from.map + offset && level.rooms.Contains(m.map));
        }
        if (target != null)
        {
            var targetMapName = $"{target.area} - {level.name} - {target.name}";

            // Find the entrance node on the target room that this door leads into
            var targetScreen = data.underworld_screens.First(s => s.area == target.area && s.screen == target.screen);

            var targetEntrance = targetScreen.nodes.exits.First(e => e.direction == oppositeDirection && e.type == ExitType.Entrance);
            var targetEntranceName = $"{targetMapName} - {targetEntrance.name}";
            var targetEntranceNode = FindOrCreateNode(targetEntranceName);

            var sourceRequirement = door switch
            {
                DoorType.Open => "fixed",
                DoorType.Wall => "Never",
                DoorType.PassThrough => "fixed",
                DoorType.PassThroughNoSound => "fixed",
                DoorType.Bombable => "UseBombs",
                DoorType.Locked => "Key",
                DoorType.Locked2 => "Key",
                DoorType d when d == DoorType.Shutter && (from.level_nine_check ?? false) => "Triforce|" + config.Triforces.ToString(),
                DoorType d when d == DoorType.Shutter && sourceBehaviour == RoomBehaviour.None => "Never",
                DoorType d when d == DoorType.Shutter && sourceBehaviour == RoomBehaviour.PushBlockShutter => CreatePushBlockLogic(from, fromMapName),
                DoorType d when d == DoorType.Shutter && sourceBehaviour > RoomBehaviour.None => killRequirement ?? "fixed",
                _ => throw new Exception("Unknown door type")
            };

            // Connect the source exit to the target entrance
            AddDirectedEdge(exitNode, targetEntranceNode, sourceRequirement);
        }
    }

    private string CreatePushBlockLogic(UnderworldMap map, string mapName)
    {
        var pushBlockItem = $"Map{map.map:X2}Push";
        if (FindNode($"{mapName} - Push - Final") != null)
            return pushBlockItem;

        var screen = data!.underworld_screens.First(s => s.area == map.area && s.screen == map.screen);
        var pushMeta = (screen.nodes.meta ?? []).FirstOrDefault(m => m.type == MetaType.Push)
            ?? throw new Exception($"Room {mapName} requires push-block logic but screen {map.screen:X2} has no push block meta");

        var pushNode = FindOrCreateNode($"{mapName} - {pushMeta.name}");
        var pushItemNode = FindOrCreateNode($"{mapName} - Push - Final", pushBlockItem);
        AddDirectedEdge(pushNode, pushItemNode, "fixed");
        return pushBlockItem;
    }

    private string CreateRoomClearLogic(UnderworldMap map, string mapName)
    {
        var enemies = GetEnemiesInRoom(map).Distinct().ToList();
        if (enemies.Count == 0)
        {
            return "fixed";
        }

        var killableEnemies = new List<string>();
        foreach (var enemyName in enemies)
        {
            var enemy = data!.enemies.enemies.FirstOrDefault(e => e.name == enemyName);
            if (enemy != null && enemy.kill_items != null && enemy.kill_items.Count > 0)
            {
                killableEnemies.Add(enemyName);
            }
        }

        // If there are enemies but none can be killed (e.g., Bubbles, Traps),
        // treat as auto-clear for generated dungeons since these rooms shouldn't
        // have shutter doors in vanilla. For vanilla rooms this shouldn't occur.
        if (killableEnemies.Count == 0)
        {
            return "fixed";
        }

        var roomClearItem = $"Map{map.map:X2}Clear";

        // Check if we already created this logic
        if (FindNode($"{mapName} - Clear - Final") != null)
        {
            return roomClearItem;
        }

        var chainStart = FindOrCreateNode($"{mapName} - Clear");

        // Connect all regions to chain start
        var screen = data!.underworld_screens.Where(s => s.area == map.area && s.screen == map.screen).First();
        foreach (var region in screen.nodes.regions)
        {
            var regionNode = FindOrCreateNode($"{mapName} - {region.name}");
            AddDirectedEdge(regionNode, chainStart, "fixed");
        }

        var currentNode = chainStart;
        foreach (var enemyName in killableEnemies)
        {
            var nextNode = FindOrCreateNode($"{mapName} - Clear - Defeated {enemyName}");
            AddDirectedEdge(currentNode, nextNode, $"Defeat{enemyName}");
            currentNode = nextNode;
        }

        // The last node provides the item
        var clearItemNode = FindOrCreateNode($"{mapName} - Clear - Final", roomClearItem);

        // A screen with a stepladder-gated path can spawn kill-required enemies across the gap.
        // The item drop may sit on the accessible side, but you can't clear the room (and thus
        // spawn the item / open the shutters) without reaching every enemy, so the clear requires
        // the ladder. Ranged weapons (arrows, sword beams) could in theory reach across, but the
        // logic models neither enemy positions nor ranged kills, so we require the ladder for now.
        // TODO: allow ranged kills to satisfy this once enemy-position/ranged logic exists.
        bool needsLadder = screen.edges.undirected != null
            && screen.edges.undirected.TryGetValue("StepLadder", out var ladderEdges)
            && ladderEdges.Count > 0;

        AddDirectedEdge(currentNode, clearItemNode, needsLadder ? "StepLadder" : "fixed");

        return roomClearItem;
    }

    private void BuildEnemyGraph()
    {
        var enemiesNode = FindOrCreateNode("Enemies");
        var metaNode = FindOrCreateNode("Overworld - Meta - Meta");
        AddDirectedEdge(metaNode, enemiesNode, "fixed");

        foreach (var enemy in data!.enemies.enemies)
        {
            if (enemy.kill_items != null && enemy.kill_items.Count > 0)
            {
                var fightNodeName = $"Fight{enemy.name}";
                var fightNode = FindOrCreateNode(fightNodeName);
                AddDirectedEdge(enemiesNode, fightNode, "fixed");

                var defeatNodeName = $"Defeat{enemy.name}";
                var defeatNode = FindOrCreateNode(defeatNodeName, defeatNodeName);

                foreach (var item in enemy.kill_items)
                {
                    AddDirectedEdge(fightNode, defeatNode, item);
                }
            }
        }
    }

    private List<string> GetEnemiesInRoom(UnderworldMap map)
    {
        var enemies = new List<string>();

        // Find the level for this map to get enemy counts
        var level = data!.levels.FirstOrDefault(l => l.rooms.Contains(map.map));
        if (level == null) return enemies;

        int effectiveId = (map.enemy_mode << 6) | map.enemy_id;

        // Get the actual number of enemies
        int countIndex = map.enemies;
        if (countIndex < 0 || countIndex >= level.enemy_counts.Length) return enemies;
        int enemyCount = level.enemy_counts[countIndex];

        if (effectiveId >= 0x62)
        {
            int listId = effectiveId - 0x62;
            var list = data!.enemies.enemy_lists.FirstOrDefault(l => l.id == listId);
            if (list != null)
            {
                // Take the first N enemies from the list
                for (int i = 0; i < enemyCount && i < list.data.Count; i++)
                {
                    var enemyId = list.data[i];
                    var enemy = data.enemies.enemies.FirstOrDefault(e => e.id == enemyId);
                    if (enemy != null && enemy.name != "Nothing") enemies.Add(enemy.name);
                }
            }
        }
        else
        {
            var enemy = data!.enemies.enemies.FirstOrDefault(e => e.id == effectiveId);
            if (enemy != null && enemy.name != "Nothing")
            {
                // Add N copies of the enemy
                for (int i = 0; i < enemyCount; i++)
                {
                    enemies.Add(enemy.name);
                }
            }
        }

        return enemies;
    }
}

public class DirectedUndirectedPair
{
    [YamlMember(Alias = "directed")]
    public List<List<string>> Directed { get; set; } = [];
    [YamlMember(Alias = "undirected")]
    public List<List<string>> Undirected { get; set; } = [];
}

public class YamlItem
{
    [YamlMember(Alias = "byte")]
    public byte Byte { get; set; }
    [YamlMember(Alias = "type")]
    public string Type { get; set; } = string.Empty;
}
