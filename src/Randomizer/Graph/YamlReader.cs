namespace Randomizer.Graph;

using System.Collections.Concurrent;
using YamlDotNet.Serialization;

public class YamlReader
{
    private static Lazy<string> _dataRoot = new(() =>
    {
        DirectoryInfo? currentDirectory = new(Directory.GetCurrentDirectory());

        do
        {
            string dataRoot = Path.Combine(currentDirectory.FullName, "src/Randomizer/Graph/data");
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

    private const string ItemsPath = "items.yml";
    private const string VerticesPath = "Vertices";
    private const string EnemiesPath = "Enemizer/enemies.yml";
    private const string SpriteLocationsPath = "Bosses/SpriteLocations.yml";

    private static readonly Lazy<Vertices> _cachedVertices = new(() =>
    {
        Vertices result = new();

        var files = Directory.GetFiles(Path.Combine(DataRoot, VerticesPath), "*.yml", SearchOption.AllDirectories).Order();
        foreach (string file in files)
        {
            var currentFileEdges = LoadVerticesFromFile(file);
            MergeVertices(result, currentFileEdges);
        }

        return result;
    });
    private static readonly Lazy<Dictionary<string, List<string>>> _cachedEnemies = new(() =>
    {
        string enemiesYML = Path.Combine(DataRoot, EnemiesPath);
        using var reader = File.OpenText(enemiesYML);
        var deserializer = new DeserializerBuilder().Build();
        var result = deserializer.Deserialize<Dictionary<string, List<string>>>(reader);
        return result;
    });
    private static readonly ConcurrentDictionary<string, Entrances> _cachedEntrances = new();
    private static readonly Lazy<Dictionary<string, YamlItem>> _cachedItems = new(() =>
    {
        string itemsYML = Path.Combine(DataRoot, ItemsPath);

        var deserializer = new DeserializerBuilder().Build();
        using var reader = File.OpenText(itemsYML);
        var result = deserializer.Deserialize<Dictionary<string, YamlItem>>(reader);

        return result;
    });
    private static readonly Lazy<Dictionary<string, Dictionary<string, List<YamlSprite>>>> _cachedSpriteLocations = new(() =>
    {
        string spritesYML = Path.Combine(DataRoot, SpriteLocationsPath);
        using var reader = File.OpenText(spritesYML);
        var deserializer = new DeserializerBuilder().Build();
        var result = deserializer.Deserialize<Dictionary<string, Dictionary<string, List<YamlSprite>>>>(reader);
        return result;
    });

    private static readonly ConcurrentDictionary<string, Dictionary<string, DirectedUndirectedPair>> _cachedEdges = new();
    private static readonly ConcurrentDictionary<string, Dictionary<string, DirectedUndirectedPair>> _cachedTechEdges = new();

    public static Dictionary<string, YamlItem> LoadItems() => _cachedItems.Value;

    public static Dictionary<string, DirectedUndirectedPair> LoadEdgesFromTech(string name) => _cachedTechEdges.GetOrAdd(name, name =>
    {
        string edgesYML = Path.Combine(DataRoot, "Edges/tech", name + ".yml"); ;

        var deserializer = new DeserializerBuilder().Build();
        using var reader = File.OpenText(edgesYML);
        var result = deserializer.Deserialize<Dictionary<string, DirectedUndirectedPair>>(reader);
        return result;
    });

    private static Dictionary<string, DirectedUndirectedPair> LoadEdgesFromFile(string path)
    {
        var deserializer = new DeserializerBuilder().Build();
        using var reader = File.OpenText(path);
        return deserializer.Deserialize<Dictionary<string, DirectedUndirectedPair>>(reader);
    }

    public static Dictionary<string, DirectedUndirectedPair> LoadEdges(string name) => _cachedEdges.GetOrAdd(name, name =>
    {
        var result = new Dictionary<string, DirectedUndirectedPair>();

        var files = Directory.GetFiles(Path.Combine(DataRoot, "Edges", name), "*.yml", SearchOption.AllDirectories).Order();
        foreach (string file in files)
        {
            var currentFileEdges = LoadEdgesFromFile(file);
            MergeEdges(result, currentFileEdges);
        }

        return result;
    });

    public static void MergeEdges(Dictionary<string, DirectedUndirectedPair> dest, Dictionary<string, DirectedUndirectedPair> source)
    {
        if (source is null)
            return;
        foreach (var entry in source)
        {
            if (dest.TryGetValue(entry.Key, out var resultRair))
            {
                resultRair.Directed.AddRange(entry.Value.Directed);
                resultRair.Undirected.AddRange(entry.Value.Undirected);
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

    public static Entrances LoadEntrances(string name) => _cachedEntrances.GetOrAdd(name, name =>
    {
        string entrancesYML = Path.Combine(DataRoot, "Edges/entrances", name + ".yml");
        var deserializer = new DeserializerBuilder().Build();
        using var reader = File.OpenText(entrancesYML);
        var result = deserializer.Deserialize<Entrances>(reader);
        return result;
    });

    private static Vertices LoadVerticesFromFile(string path)
    {
        string verticesYML = Path.IsPathFullyQualified(path) ? path : Path.Combine(DataRoot, path);
        using var reader = File.OpenText(verticesYML);
        var deserializer = new DeserializerBuilder().Build();
        return deserializer.Deserialize<Vertices>(reader);
    }

    public static Vertices LoadVertices() => _cachedVertices.Value;
    public static void MergeVertices(Vertices dest, Vertices source)
    {
        dest.Maps.AddRange(source.Maps);
        dest.Rooms.AddRange(source.Rooms);
    }

    public static Dictionary<string, List<string>> LoadEnemies() => _cachedEnemies.Value;

    public static Dictionary<string, Dictionary<string, List<YamlSprite>>> LoadSpriteLocations() => _cachedSpriteLocations.Value;
}
public class YamlItem
{
    [YamlMember(Alias = "bytes")]
    public List<byte> Bytes { get; set; } = new();
    [YamlMember(Alias = "type")]
    public string Type { get; set; } = string.Empty;

    // TODO: those are english-only right now, and could probably go elsewhere.
    // TODO: do we want to keep location-specific hints? they all use the same text at the moment.
    [YamlMember(Alias = "pedestalhint")]
    public string? PedestalHintText { get; set; }
    [YamlMember(Alias = "etherhint")]
    public string? EtherTabletHintText { get; set; }
    [YamlMember(Alias = "bomboshint")]
    public string? BombosTabletHintText { get; set; }
    [YamlMember(Alias = "pedestalcredits")]
    public string? PedestalCreditsText { get; set; }
    [YamlMember(Alias = "zoracredits")]
    public string? ZoraCreditsText { get; set; }
    [YamlMember(Alias = "witchcredits")]
    public string? WitchCreditsText { get; set; }
    [YamlMember(Alias = "unclecredits")]
    public string? UncleCreditsText { get; set; }
    [YamlMember(Alias = "kidcredits")]
    public string? KidCreditsText { get; set; }
    [YamlMember(Alias = "flutecredits")]
    public string? FluteCreditsText { get; set; }
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

    [YamlMember(Alias = "meta")]
    public List<MetaEntry> Meta { get; set; } = new();

    [YamlMember(Alias = "prizepacks")]
    public List<Prizepack> Prizepacks { get; set; } = new();
}

public class MetaEntry
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; }

    [YamlMember(Alias = "items")]
    public List<string> Items { get; set; } = new();

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

    [YamlMember(Alias = "items")]
    public List<ItemEntry> Items { get; set; } = new();

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
