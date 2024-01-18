namespace Randomizer.Graph.Combo.Zelda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class ZeldaYamlReader
{
    private YamlData? data;
    private Dictionary<string, Dictionary<string, object>> vertices = new Dictionary<string, Dictionary<string, object>>();
    private Dictionary<string, DirectedUndirectedPair> edges = new Dictionary<string, DirectedUndirectedPair>();

    class Screen
    {
        public string name;
        public Area area;
        public int screen;
        public NodeCollection nodes;
        public EdgeCollection edges;
    }

    class NodeCollection
    {
        public List<Exit> exits;
        public List<Cave> caves;
        public List<Meta> meta;
    }

    class EdgeCollection
    {
        public Dictionary<string, List<object>> undirected;
        public Dictionary<string, List<object>> directed;
    }

    class Exit
    {
        public string name;
        public ExitType type;
        public Direction direction;
    }

    class Cave
    {
        public string name;
        public CaveType type;
    }

    class Meta
    {
        public string name;
        public MetaType type;
        public string position;
    }

    enum MetaType
    {
        Meta,
        Item,
        Armos,
        Fairy,
        Push,
        Stairs
    }

    enum CaveType
    {
        Bomb,
        Open,
        Grave,
        Tree,
        Push
    }

    enum Area
    {
        Overworld,
        Underworld,
    }

    enum ExitType
    {
        Scroll
    }

    enum Direction
    {
        Up,
        Down,
        Left,
        Right,
    }

    class OverworldMap
    {
        public string name;
        public Area area;
        public int map;
        public int screen;
        public int[] palettes;
        public bool zora;
        public bool waves;
        public int enemies;
        public int enemy_id;
        public int enemy_sides;
        public int enemy_mode;
        public int cave;
        public int stairs;
        public int[] secret;
        public int[] exit;
    }

    class UnderworldMap
    {
        public string name;
        public Area area;
        public int map;
        public int screen;
        public int[] palettes;
        public int[] doors;
        public int enemies;
        public int enemy_id;
        public int enemy_mode;
        public bool push_block;
        public bool dark_room;
        public int boss_sfx;
        public int room_item;
        public int item_pos;
        public int behaviour;
    }

    enum DoorType
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

    enum RoomBehaviour
    {
        None = 0,
        KillForItemShutter = 1,
        Leader = 2,
        GetTriforceShutter = 3,
        PushBlockShutter = 4,
        PushBlockStairs = 5,
        LeaderShutters = 6,
        KillForItemShutterBoss = 7
    }

    class Level
    {
        public string name;
        public int level;
        public Area area;
        public int[] enemy_counts;
        public int start_room_id;
        public int start_y;
        public int boss_room_id;
        public int triforce_room_id;
        public int[] shortcut_or_item_pos_array;
        public int[] cellar_room_id_array;
        public int[] world_flags_addr;
        public int[] submenu_map_mask;
        public int submenu_map_rotation;
        public int status_bar_map_x_offset;
        public byte[] status_bar_map_transfer_buf;
        public byte[] palettes_transfer_buf;
        public byte[] palette_cycles;
        public byte[] death_palette_series;
    }

    class Special
    {
        public int overworld_item_room;
        public int overworld_item_x;
        public int overworld_item_id;
        public int armos_item_room;
        public int armos_item_x;
        public int armos_item_id;
        public int[] armos_stairs;
        public int[] step_ladder;
        public int[] recorder_stairs;
        public int start;
    }

    class YamlData
    {
        public List<OverworldMap> overworld_maps;
        public List<Screen> overworld_screens;

        public List<UnderworldMap> underworld_maps;
        public List<Screen> underworld_screens;

        public List<Level> levels;
        public Special special;
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
            Console.WriteLine($"Error while deserializing: {path}, {e.Message} ({e.InnerException?.Source ?? ""}");
            return default;
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
        var edgeExists = undirected ? edgePair.Undirected.Any(e => e.SequenceEqual(new string[] { (string)from["name"], (string)to["name"] })) :
                                      edgePair.Directed.Any(e => e.SequenceEqual(new string[] { (string)from["name"], (string)to["name"] }));

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

    private void Load()
    {
        var path = Path.Combine(YamlReader.DataRoot, "../Combo/Data/Zelda");
        var levels = LoadFiles<Level>(Path.Combine(path, "Levels"));
        
        var overworldMaps = LoadFiles<OverworldMap>(Path.Combine(path, "Maps/Overworld"));
        var underworldMaps = LoadFiles<UnderworldMap>(Path.Combine(path, "Maps/Underworld"));

        var overworldScreens = LoadFiles<Screen>(Path.Combine(path, "Screens/Overworld"));
        var underworldScreens = LoadFiles<Screen>(Path.Combine(path, "Screens/Underworld"));

        var special = LoadFile<Special>(Path.Combine(path, "Special.yml"));

        data = new YamlData()
        {
            levels = levels,
            overworld_maps = overworldMaps,
            underworld_maps = underworldMaps,
            overworld_screens = overworldScreens,
            underworld_screens = underworldScreens,
            special = special
        };

        BuildGraph();
    }

    public int GetStartMap()
    {
        if (data is null)
        {
            Load();
        }

        return data.special.start;
    }

    public Dictionary<string, DirectedUndirectedPair> GetForWorld(World world)
    {
        if (data is null)
        {
            Load();
        }

        return edges.Select(e => { e.Value.Directed = e.Value.Directed.Select(d => { d[0] = $"Zelda - {d[0]}:{world.Id}"; d[1] = $"Zelda - {d[1]}:{world.Id}"; return d; }).ToList(); e.Value.Undirected = e.Value.Undirected.Select(d => { d[0] = $"Zelda - {d[0]}:{world.Id}"; d[1] = $"Zelda - {d[1]}:{world.Id}"; return d; }).ToList(); return e; }).ToDictionary(e => $"{e.Key}:{world.Id}", e => e.Value);
    }

    public List<Dictionary<string, object>> LoadYmlData(World world)
    {
        if (data is null)
        {
            Load();
        }

        return vertices.Values.Select(v => { v["name"] = $"Zelda - {v["name"]}:{world.Id}"; return v; }).ToList();
    }

    private void BuildGraph()
    {
        foreach (var map in data.overworld_maps)
        {
            BuildOverworldMap(map);
        }
    }

    private void BuildOverworldMap(OverworldMap map)
    {
        var level = data.levels.First();
        var screen = data.overworld_screens.Where(s => s.area == map.area && s.screen == map.screen).First();
        var mapName = $"{map.area} - {map.name}";
        
        var exits = new List<Direction>();
        foreach (var exit in screen.nodes.exits ?? [])
        {
            var exitName = $"{mapName} - {exit.name}";
            var exitNode = FindOrCreateNode(exitName);
            exits.Add(exit.direction);
        }

        foreach (var cave in screen.nodes.caves ?? [])
        {
            if (map.cave > 0 && (cave.type == CaveType.Open || map.secret[0] == 1))
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
                var metaNode = FindOrCreateNode(metaName);
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

                if(fromNode != null && toNode != null)
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

        // Connect caves
        if (map.cave > 0)
        {
            var cave = screen.nodes.caves?.FirstOrDefault() ?? null;
            if (cave != null)
            {
                if (cave.type == CaveType.Open || map.secret[0] == 1)
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
                        var caveEntranceNode = FindOrCreateNode($"Cave {map.cave} - Entrance");
                        var caveNode = FindOrCreateNode($"{mapName} - {cave.name}");

                        AddDirectedEdge(caveEntranceNode, caveNode, "fixed");
                    }
                }
            }
        }

        // Connect stairs
        if (data.special.armos_stairs.Contains(map.map))
        {
            var armosScreenNode = screen.nodes.meta.Where(m => m.type == MetaType.Armos).First();
            var armosNode = FindOrCreateNode($"{mapName} - {armosScreenNode.name}");
            var caveNodeName = map.cave switch
            {
                <= 10 => $"Level {map.cave} - Entrance",
                _ => $"Cave {map.cave} - Entrance"
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
            var armosScreenNode = screen.nodes.meta.Where(m => m.type == MetaType.Armos).First();
            var armosNode = FindOrCreateNode($"{mapName} - {armosScreenNode.name}");
            var itemNode = FindOrCreateNode($"{mapName} - Item");

            AddDirectedEdge(armosNode, itemNode, "fixed");
        }

        // Connect overworld stairs
        if (data.special.overworld_item_room == map.map)
        {
            var itemScreenNode = screen.nodes.meta.Where(m => m.type == MetaType.Item).First();
            var itemNode = FindOrCreateNode($"{mapName} - {itemScreenNode.name}");
            var overworldNode = FindOrCreateNode($"{mapName} - Item");
            
            AddDirectedEdge(itemNode, overworldNode, "fixed");
        }

        // Connect overworld recorder stairs
        if (data.special.recorder_stairs.Contains(map.map))
        {
            var firstExitScreenNode = screen.nodes.exits.First();
            var firstExitNode = FindOrCreateNode($"{mapName} - {firstExitScreenNode.name}");
            var recorderNode = FindOrCreateNode($"{mapName} - Recorder Stairs");

            var caveNodeName = map.cave switch
            {
                <= 10 => $"Level {map.cave} - Entrance",
                _ => $"Cave {map.cave} - Entrance"
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
            };

            var targetMap = data.overworld_maps.Where(m => m.area == map.area && m.map == map.map + offset).FirstOrDefault();
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
        };

        var currentScreen = data.overworld_screens.Where(s => s.area == from.area && s.screen == from.screen).First();
        var currentExits = currentScreen.nodes.exits.Where(e => e.direction == direction).ToList();

        var targetScreen = data.overworld_screens.Where(s => s.area == to.area && s.screen == to.screen).First();
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

}
