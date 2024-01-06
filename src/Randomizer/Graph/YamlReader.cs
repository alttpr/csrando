namespace Randomizer.Graph;

using YamlDotNet.Serialization;

public class YamlReader
{
    public static string DataRoot
    {
        get
        {
            if (_dataRoot == null)
            {
                lock (_dataLock)
                {
                    if (_dataRoot == null)
                    {
                        string? new_root = null;

                        DirectoryInfo? current_directory = new(Directory.GetCurrentDirectory());

                        do
                        {
                            string data_root = Path.Combine(current_directory.FullName, "src/Randomizer/Graph/data");
                            if (Directory.Exists(data_root))
                            {
                                new_root = data_root;
                                break;
                            }
                            current_directory = current_directory.Parent;
                        } while (current_directory != null);

                        if (new_root == null)
                        {
                            throw new Exception("Could not find the data directory automatically. Set YamlReader.DataRoot before loading data.");
                        }

                        _dataRoot = new_root;
                    }
                }
            }
            return _dataRoot;
        }
        set
        {
            lock (_dataLock)
            {
                _dataRoot = value;
            }
        }
    }

    private const string ItemsPath = "items.yml";
    private const string VerticesPath = "Vertices";
    private const string EnemiesPath = "Enemizer/enemies.yml";
    private const string SpriteLocationsPath = "Bosses/SpriteLocations.yml";

    private static readonly object _dataLock = new object();
    private static string? _dataRoot;
    private static Vertices? _cachedVertices = null;
    private static Dictionary<string, List<string>>? _cachedEnemies = null;
    private static Entrances? _cachedEntrances = null;
    private static Dictionary<string, YamlItem>? _cachedItems = null;
    private static Dictionary<string, Dictionary<string, List<YamlSprite>>>? _cachedSpriteLocations = null;

    private static readonly ReaderWriterLockSlim _cachedEdgesLock = new ReaderWriterLockSlim();
    private static readonly Dictionary<string, Dictionary<string, DirectedUndirectedPair>> _cachedEdges = new();
    private static readonly Dictionary<string, Dictionary<string, DirectedUndirectedPair>> _cachedTechEdges = new();

    public static Dictionary<string, YamlItem> LoadItems()
    {
        if (_cachedItems != null) { return _cachedItems; }

        lock (_dataLock)
        {
            if (_cachedItems != null) { return _cachedItems; }

            string items_yml = Path.Combine(DataRoot, ItemsPath);

            var result = new Dictionary<string, YamlItem>();

            var deserializer = new DeserializerBuilder().Build();
            using (TextReader reader = File.OpenText(items_yml))
            {
                result = deserializer.Deserialize<Dictionary<string, YamlItem>>(reader);
            }

            _cachedItems = result;

            return result;
        }
    }

    public static Dictionary<string, DirectedUndirectedPair> LoadEdgesFromTech(string name)
    {
        _cachedEdgesLock.EnterReadLock();
        try
        {
            if (_cachedTechEdges.TryGetValue(name, out var cached_result))
            {
                return cached_result;
            }
        }
        finally { _cachedEdgesLock.ExitReadLock(); }

        _cachedEdgesLock.EnterWriteLock();
        try
        {
            if (_cachedTechEdges.TryGetValue(name, out var cached_result2))
            {
                return cached_result2;
            }

            string edges_yml = Path.Combine(DataRoot, "Edges/tech", name + ".yml"); ;

            var deserializer = new DeserializerBuilder().Build();
            using TextReader reader = File.OpenText(edges_yml);
            var result = deserializer.Deserialize<Dictionary<string, DirectedUndirectedPair>>(reader);
            _cachedTechEdges.Add(name, result);

            return result;
        }
        finally { _cachedEdgesLock.ExitWriteLock(); }
    }

    private static Dictionary<string, DirectedUndirectedPair> LoadEdgesFromFile(string path)
    {
        var deserializer = new DeserializerBuilder().Build();
        using TextReader reader = File.OpenText(path);
        return deserializer.Deserialize<Dictionary<string, DirectedUndirectedPair>>(reader);
    }

    public static Dictionary<string, DirectedUndirectedPair> LoadEdges(string name)
    {
        _cachedEdgesLock.EnterReadLock();
        try
        {
            if (_cachedEdges.TryGetValue(name, out var cached_result))
            {
                return cached_result;
            }
        }
        finally { _cachedEdgesLock.ExitReadLock(); }

        _cachedEdgesLock.EnterWriteLock();
        try
        {
            if (_cachedEdges.TryGetValue(name, out var cached_result2))
            {
                return cached_result2;
            }

            var result = new Dictionary<string, DirectedUndirectedPair>();

            foreach (string file in Directory.GetFiles(Path.Combine(DataRoot, "Edges", name), "*.yml", SearchOption.AllDirectories))
            {
                var current_file_edges = LoadEdgesFromFile(file);
                MergeEdges(result, current_file_edges);
            }

            _cachedEdges.Add(name, result);
            return result;
        }
        finally { _cachedEdgesLock.ExitWriteLock(); }
    }

    public static void MergeEdges(Dictionary<string, DirectedUndirectedPair> dest, Dictionary<string, DirectedUndirectedPair> source)
    {
        if (source is null)
            return;
        foreach (var entry in source)
        {
            if (dest.TryGetValue(entry.Key, out var result_pair))
            {
                result_pair.Directed.AddRange(entry.Value.Directed);
                result_pair.Undirected.AddRange(entry.Value.Undirected);
            }
            else
            {
                dest.Add(entry.Key, new()
                {
                    Directed = entry.Value.Directed.ToList(),
                    Undirected = entry.Value.Undirected.ToList(),
                });
            }
        }
    }

    public static Entrances LoadEntrances(string name)
    {
        if (_cachedEntrances != null) { return _cachedEntrances; }

        lock (_dataLock)
        {
            if (_cachedEntrances != null) { return _cachedEntrances; }

            string entrances_yml = Path.Combine(DataRoot, "Edges/entrances", name + ".yml");
            var deserializer = new DeserializerBuilder().Build();
            using TextReader reader = File.OpenText(entrances_yml);
            var result = deserializer.Deserialize<Entrances>(reader);
            _cachedEntrances = result;
            return result;
        }
    }

    private static Vertices LoadVerticesFromFile(string path)
    {
        string vertices_yml = Path.IsPathFullyQualified(path) ? path : Path.Combine(DataRoot, path);
        using TextReader reader = File.OpenText(vertices_yml);
        var deserializer = new DeserializerBuilder().Build();
        return deserializer.Deserialize<Vertices>(reader);
    }

    public static Vertices LoadVertices()
    {
        if (_cachedVertices != null) { return _cachedVertices; }

        lock (_dataLock)
        {
            if (_cachedVertices != null) { return _cachedVertices; }

            Vertices result = new();

            foreach (string file in Directory.GetFiles(Path.Combine(DataRoot, VerticesPath), "*.yml", SearchOption.AllDirectories))
            {
                var current_file_edges = LoadVerticesFromFile(file);
                MergeVertices(result, current_file_edges);
            }

            _cachedVertices = result;
        }

        return _cachedVertices;
    }

    public static void MergeVertices(Vertices dest, Vertices source)
    {
        dest.Maps.AddRange(source.Maps);
        dest.Rooms.AddRange(source.Rooms);
    }

    public static Dictionary<string, List<string>> LoadEnemies()
    {
        if (_cachedEnemies != null) { return _cachedEnemies; }

        lock (_dataLock)
        {
            if (_cachedEnemies != null) { return _cachedEnemies; }

            string enemies_yml = Path.Combine(DataRoot, EnemiesPath);
            using TextReader reader = File.OpenText(enemies_yml);
            var deserializer = new DeserializerBuilder().Build();
            var result = deserializer.Deserialize<Dictionary<string, List<string>>>(reader);
            _cachedEnemies = result;
            return result;
        }
    }

    public static Dictionary<string, Dictionary<string, List<YamlSprite>>> LoadSpriteLocations()
    {
        if (_cachedSpriteLocations != null) { return _cachedSpriteLocations; }

        lock (_dataLock)
        {
            if (_cachedSpriteLocations != null) { return _cachedSpriteLocations; }

            string sprites_yml = Path.Combine(DataRoot, SpriteLocationsPath);
            using TextReader reader = File.OpenText(sprites_yml);
            var deserializer = new DeserializerBuilder().Build();
            var result = deserializer.Deserialize<Dictionary<string, Dictionary<string, List<YamlSprite>>>>(reader);
            _cachedSpriteLocations = result;
            return result;
        }
    }
}
public class YamlItem
{
    [YamlMember(Alias = "bytes")]
    public List<byte> Bytes { get; set; } = new();
    [YamlMember(Alias = "type")]
    public string Type { get; set; } = string.Empty;
}

public class YamlSprite
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; }

    [YamlMember(Alias = "position")]
    public Position Position { get; set; }

    [YamlMember(Alias = "roomid")]
    public int RoomId { get; set; }

    [YamlMember(Alias = "sprite")]
    public string Sprite { get; set; }
}

public class DirectedUndirectedPair
{

    [YamlMember(Alias = "undirected")]
    public List<List<string>> Undirected { get; set; } = new();

    [YamlMember(Alias = "directed")]
    public List<List<string>> Directed { get; set; } = new();
}

public class ConnectionGroup
{
    [YamlMember(Alias = "in")]
    public List<string> In { get; set; } = new();
    [YamlMember(Alias = "out")]
    public List<string> Out { get; set; } = new();
}

public class Entrances
{
    [YamlMember(Alias = "fixed")]
    public List<List<string>> Fixed { get; set; } = new();
    [YamlMember(Alias = "connections")]
    public List<ConnectionGroup> Connections { get; set; } = new();
}

public partial class Map
{
    [YamlMember(Alias = "map")]
    public int MapMap { get; set; }

    [YamlMember(Alias = "moonpearl")]
    public bool Moonpearl { get; set; }

    [YamlMember(Alias = "nodes")]
    public MapNodes Nodes { get; set; }
}

public partial class MapNodes
{
    [YamlMember(Alias = "regions")]
    public List<Region> Regions { get; set; } = new();

    [YamlMember(Alias = "entrances")]
    public List<Entrance> Entrances { get; set; } = new();

    [YamlMember(Alias = "holes")]
    public List<Hole> Holes { get; set; } = new();

    [YamlMember(Alias = "items")]
    public List<ItemEntry> Items { get; set; } = new();

    [YamlMember(Alias = "warps")]
    public List<Warp> Warps { get; set; } = new();

    [YamlMember(Alias = "mobs")]
    public List<Entity> Mobs { get; set; } = new();

    [YamlMember(Alias = "meta")]
    public List<MetaEntry> Meta { get; set; } = new();

    [YamlMember(Alias = "prizepacks")]
    public List<Prizepack> Prizepacks { get; set; } = new();
}

public class MetaEntry
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; }

    [YamlMember(Alias = "item")]
    public string? Item { get; set; }

    [YamlMember(Alias = "connections")]
    public Dictionary<string, List<string>> Connections { get; set; } = new();
}

public class Prizepack
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; }

    [YamlMember(Alias = "offset")]
    public byte Offset { get; set; }

    [YamlMember(Alias = "sprite")]
    public string Sprite { get; set; }
}

public partial class Entrance
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; }

    [YamlMember(Alias = "entranceid")]
    public int EntranceId { get; set; }

    [YamlMember(Alias = "outletid")]
    public int OutletId { get; set; }

    [YamlMember(Alias = "conditions")]
    public List<string> Conditions { get; set; } = new();
}

public partial class Hole
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; }

    [YamlMember(Alias = "entranceids")]
    public List<int> EntranceIds { get; set; } = new();

    [YamlMember(Alias = "conditions")]
    public List<string> Conditions { get; set; } = new();
}

public partial class ItemEntry
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; }

    [YamlMember(Alias = "addresses")]
    public List<long> Addresses { get; set; } = new();

    [YamlMember(Alias = "type")]
    public VertexType Type { get; set; }

    [YamlMember(Alias = "item")]
    public string? Item { get; set; }

    [YamlMember(Alias = "itemset")]
    public List<string> ItemSet { get; set; } = new();

    [YamlMember(Alias = "conditions")]
    public List<string> Conditions { get; set; } = new();
}

public partial class Warp
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; }

    [YamlMember(Alias = "position")]
    public Position Position { get; set; }

    [YamlMember(Alias = "connections")]
    public Dictionary<string, List<string>> Connections { get; set; } = new();
}

public partial class Room
{
    [YamlMember(Alias = "roomid")]
    public int Roomid { get; set; }

    [YamlMember(Alias = "nodes")]
    public RoomNodes Nodes { get; set; }

    [YamlMember(Alias = "group")]
    public int? Group { get; set; }

    [YamlMember(Alias = "dark")]
    public bool Dark { get; set; } = false;

    [YamlMember(Alias = "extralight")]
    public List<string> ExtraLight { get; set; } = new();

    [YamlMember(Alias = "bosses")]
    public object? Bosses { get; set; }
}

public partial class RoomNodes
{
    [YamlMember(Alias = "regions")]
    public List<Region> Regions { get; set; } = new();

    [YamlMember(Alias = "keydoors")]
    public List<Keydoor> Keydoors { get; set; } = new();

    [YamlMember(Alias = "bigkeydoors")]
    public List<Keydoor> BigKeydoors { get; set; } = new();

    [YamlMember(Alias = "shutters")]
    public List<Shutter> Shutters { get; set; } = new();

    [YamlMember(Alias = "items")]
    public List<ItemEntry> Items { get; set; } = new();

    [YamlMember(Alias = "inventory")]
    public List<InventoryEntry> Inventory { get; set; } = new();

    [YamlMember(Alias = "pots")]
    public List<Entity> Pots { get; set; } = new();

    [YamlMember(Alias = "mobs")]
    public List<Entity> Mobs { get; set; } = new();

}

public partial class Keydoor
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; }

    [YamlMember(Alias = "key")]
    public string Key { get; set; }
}

public partial class Shutter
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; }
}

public partial class InventoryEntry
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; }

    [YamlMember(Alias = "type")]
    public VertexType Type { get; set; }

    [YamlMember(Alias = "item")]
    public string Item { get; set; }

    [YamlMember(Alias = "cost")]
    public int Cost { get; set; }

    [YamlMember(Alias = "itemset")]
    public List<string> ItemSet { get; set; } = new();
}

public partial class Entity
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; }

    // TODO: Some entries (pots) use a short, some have an x,y,z triple. Fix data
    [YamlMember(Alias = "position")]
    //public Position Position { get; set; }
    public object Position { get; set; }

    [YamlMember(Alias = "sprite")]
    public string Sprite { get; set; }

    [YamlMember(Alias = "state")]
    public List<int> State { get; set; } = new();

    [YamlMember(Alias = "type")]
    public VertexType? Type { get; set; }

    [YamlMember(Alias = "item")]
    public string? Item { get; set; }

    [YamlMember(Alias = "itemset")]
    public List<string> ItemSet { get; set; } = new();

    [YamlMember(Alias = "deny")]
    public List<string> Deny { get; set; } = new();

    [YamlMember(Alias = "allow")]
    public List<string> Allow { get; set; } = new();

    [YamlMember(Alias = "trophy")]
    public string? Trophy { get; set; }
}

public partial class Position
{
    [YamlMember(Alias = "x")]
    public int X { get; set; }

    [YamlMember(Alias = "y")]
    public int Y { get; set; }

    [YamlMember(Alias = "z")]
    public int? Z { get; set; }
}

public partial class Region
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; }

    [YamlMember(Alias = "inletid")]
    public int? InletId { get; set; }

    [YamlMember(Alias = "type")]
    public VertexType? Type { get; set; }

    [YamlMember(Alias = "shopkeeper")]
    public int? Shopkeeper { get; set; }

    [YamlMember(Alias = "shopstyle")]
    public int? Shopstyle { get; set; }

    [YamlMember(Alias = "switch")]
    public bool? Switch { get; set; }

    [YamlMember(Alias = "mobs")]
    public List<Entity> Mobs { get; set; } = new();

    [YamlMember(Alias = "entrances")]
    public List<Entrance> Entrances { get; set; } = new();

    [YamlMember(Alias = "holes")]
    public List<Hole> Holes { get; set; } = new();

    [YamlMember(Alias = "warps")]
    public List<Warp> Warps { get; set; } = new();

    [YamlMember(Alias = "items")]
    public List<ItemEntry> Items { get; set; } = new();

    [YamlMember(Alias = "pots")]
    public List<Entity> Pots { get; set; } = new();

    [YamlMember(Alias = "inventory")]
    public List<InventoryEntry> Inventory { get; set; } = new();

    [YamlMember(Alias = "connections")]
    public Dictionary<string, List<string>> Connections { get; set; } = new();
}

public class Vertices
{
    [YamlMember(Alias = "maps")]
    public List<Map> Maps { get; set; } = new();
    [YamlMember(Alias = "rooms")]
    public List<Room> Rooms { get; set; } = new();
}
