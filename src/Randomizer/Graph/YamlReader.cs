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
    private const string BossesPath = "bosses.yml";
    private const string EnemiesPath = "Enemizer/enemies.yml";

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
    private static readonly Lazy<Dictionary<string, List<string>>> _cachedBosses = new(() =>
    {
        string bossesYML = Path.Combine(DataRoot, BossesPath);
        using var reader = File.OpenText(bossesYML);
        var deserializer = new DeserializerBuilder().Build();
        var result = deserializer.Deserialize<Dictionary<string, List<string>>>(reader);
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
    private static readonly Lazy<Dictionary<string, YamlSprite>> _cachedSprites = new(() =>
    {
        string itemsYML = Path.Combine(DataRoot, "sprites.yml");

        var deserializer = new DeserializerBuilder().Build();
        using var reader = File.OpenText(itemsYML);
        var result = deserializer.Deserialize<Dictionary<string, YamlSprite>>(reader);

        return result;
    });

    private static readonly ConcurrentDictionary<string, Dictionary<string, DirectedUndirectedPair>> _cachedEdges = new();
    private static readonly ConcurrentDictionary<string, Dictionary<string, DirectedUndirectedPair>> _cachedTechEdges = new();

    public static Dictionary<string, YamlItem> LoadItems() => _cachedItems.Value;
    public static Dictionary<string, YamlSprite> LoadSprites() => _cachedSprites.Value;

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

    public static bool EntranceDataFileExists(string name) => File.Exists(Path.Combine(DataRoot, "Edges/entrances", name + ".yml"));

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

    public static Dictionary<string, List<string>> LoadBosses() => _cachedBosses.Value;
    public static Dictionary<string, List<string>> LoadEnemies() => _cachedEnemies.Value;

    public static IReadOnlyDictionary<string, string> LoadCreditsForFluteSpot(string language) => LoadKeyedLocalizedText(Path.Combine("text", language, "credits", "flute.yml"));
    public static IReadOnlyDictionary<string, string> LoadCreditsForPedestal(string language) => LoadKeyedLocalizedText(Path.Combine("text", language, "credits", "pedestal.yml"));
    public static IReadOnlyDictionary<string, string> LoadCreditsForSickKid(string language) => LoadKeyedLocalizedText(Path.Combine("text", language, "credits", "sick-kid.yml"));
    public static IReadOnlyDictionary<string, string> LoadCreditsForUncle(string language) => LoadKeyedLocalizedText(Path.Combine("text", language, "credits", "uncle.yml"));
    public static IReadOnlyDictionary<string, string> LoadCreditsForWitchHut(string language) => LoadKeyedLocalizedText(Path.Combine("text", language, "credits", "witch.yml"));
    public static IReadOnlyDictionary<string, string> LoadCreditsForZora(string language) => LoadKeyedLocalizedText(Path.Combine("text", language, "credits", "zora.yml"));
    public static IReadOnlyList<string> LoadCreditsForDMBridge(string language) => LoadLocalizedText(Path.Combine("text", language, "credits", "bridge.yml"));
    public static IReadOnlyList<string> LoadCreditsForHyruleCastle(string language) => LoadLocalizedText(Path.Combine("text", language, "credits", "castle.yml"));
    public static IReadOnlyList<string> LoadCreditsForKakariko(string language) => LoadLocalizedText(Path.Combine("text", language, "credits", "kakariko.yml"));
    public static IReadOnlyList<string> LoadCreditsForLumberjacks(string language) => LoadLocalizedText(Path.Combine("text", language, "credits", "lumberjacks.yml"));
    public static IReadOnlyList<string> LoadCreditsForSanctuary(string language) => LoadLocalizedText(Path.Combine("text", language, "credits", "sanctuary.yml"));
    public static IReadOnlyList<string> LoadCreditsForSmithy(string language) => LoadLocalizedText(Path.Combine("text", language, "credits", "smithy.yml"));
    public static IReadOnlyList<string> LoadCreditsForFairyWell(string language) => LoadLocalizedText(Path.Combine("text", language, "credits", "well.yml"));
    public static IReadOnlyList<string> LoadCreditsForLostWoods(string language) => LoadLocalizedText(Path.Combine("text", language, "credits", "woods.yml"));
    public static IReadOnlyDictionary<string, string> LoadHintTemplates(string language) => LoadKeyedLocalizedText(Path.Combine("text", language, "hints", "templates.yml"));
    public static IReadOnlyDictionary<string, string[]> LoadHintsForLocations(string language) => LoadKeyedLocalizedListText(Path.Combine("text", language, "hints", "locations.yml"));
    public static IReadOnlyDictionary<string, string[]> LoadHintsForItems(string language) => LoadKeyedLocalizedListText(Path.Combine("text", language, "hints", "items.yml"));
    public static IReadOnlyDictionary<string, string[]> LoadHintableLocations(string language) => LoadKeyedLocalizedListText(Path.Combine("text", language, "hints", "hintable_locations.yml"));
    public static IReadOnlyDictionary<string, string> LoadHintsForPedestal(string language) => LoadKeyedLayeredLocalizedText(Path.Combine("text", language, "hints", "base.yml"), Path.Combine("text", language, "hints", "pedestal.yml"));
    public static IReadOnlyDictionary<string, string> LoadHintsForBombosTablet(string language) => LoadKeyedLayeredLocalizedText(Path.Combine("text", language, "hints", "base.yml"), Path.Combine("text", language, "hints", "bombos-tablet.yml"));
    public static IReadOnlyDictionary<string, string> LoadHintsForEtherTablet(string language) => LoadKeyedLayeredLocalizedText(Path.Combine("text", language, "hints", "base.yml"), Path.Combine("text", language, "hints", "ether-tablet.yml"));
    public static IReadOnlyDictionary<string, string> LoadHintsForProgression(string language) => LoadKeyedLocalizedText(Path.Combine("text", language, "hints", "progression.yml"));
    public static IReadOnlyDictionary<string, string> LoadHintsForBoots(string language) => LoadKeyedLocalizedText(Path.Combine("text", language, "hints", "boots.yml"));
    public static IReadOnlyDictionary<string, string> LoadDialogText(string language) => LoadKeyedLocalizedText(Path.Combine("text", language, "dialog", "base.yml"));
    public static IReadOnlyDictionary<string, string> LoadDialogForInverted(string language) => LoadKeyedLocalizedText(Path.Combine("text", language, "dialog", "inverted.yml"));
    public static IReadOnlyDictionary<string, string> LoadDialogForMystery(string language) => LoadKeyedLocalizedText(Path.Combine("text", language, "dialog", "mystery.yml"));
    public static IReadOnlyList<string> LoadJokeHints(string language) => LoadLocalizedText(Path.Combine("text", language, "hints", "jokes.yml"));
    public static IReadOnlyList<string> LoadHintLocations(string language) => LoadLocalizedText(Path.Combine("text", language, "hints", "tiles.yml"));
    public static IReadOnlyList<string> LoadRandomDialogForUncle(string language) => LoadLocalizedText(Path.Combine("text", language, "dialog", "uncle.yml"));
    public static IReadOnlyList<string> LoadRandomDialogForBlind(string language) => LoadLocalizedText(Path.Combine("text", language, "dialog", "blind.yml"));
    public static IReadOnlyList<string> LoadRandomDialogForFortuneTeller(string language) => LoadLocalizedText(Path.Combine("text", language, "dialog", "fortune.yml"));
    public static IReadOnlyList<string> LoadRandomDialogForGanonFallIn(string language) => LoadLocalizedText(Path.Combine("text", language, "dialog", "ganon_fall_in.yml"));
    public static IReadOnlyList<string> LoadRandomDialogForGanonPhase3NoSilvers(string language) => LoadLocalizedText(Path.Combine("text", language, "dialog", "ganon_phase_3_no_silvers.yml"));
    public static IReadOnlyList<string> LoadRandomDialogForGanonPhase3NoGoal(string language) => LoadLocalizedText(Path.Combine("text", language, "dialog", "ganon_phase_3_alt.yml"));
    public static IReadOnlyList<string> LoadRandomDialogForTavernMan(string language) => LoadLocalizedText(Path.Combine("text", language, "dialog", "tavern_man.yml"));
    public static IReadOnlyList<string> LoadRandomDialogForTriforce(string language) => LoadLocalizedText(Path.Combine("text", language, "dialog", "triforce.yml"));
    public static IReadOnlyDictionary<string, string> LoadRegions(string language) => LoadKeyedLocalizedText(Path.Combine("text", language, "regions.yml"));

    private static readonly ConcurrentDictionary<string /* language/type/file.yml */, IReadOnlyDictionary<string, string>> _keyedLocalizedText = new();
    public static IReadOnlyDictionary<string, string> LoadKeyedLocalizedText(string path) => _keyedLocalizedText.GetOrAdd(path, LoadKeyedText);
    public static IReadOnlyDictionary<string, string> LoadKeyedLayeredLocalizedText(string basePath, string specificPath) => _keyedLocalizedText.GetOrAdd(specificPath, s =>
    {
        // load a copy of the base text...
        var values = new Dictionary<string, string>(LoadKeyedLocalizedText(basePath));
        // ...then load the specific text...
        var specificValues = LoadKeyedText(s);
        // ...and overlay them on top
        foreach (var (key, value) in specificValues)
            values[key] = value;

        return values;
    });
    private static IReadOnlyDictionary<string, string> LoadKeyedText(string path)
    {
        string keyedTextYML = Path.IsPathFullyQualified(path) ? path : Path.Combine(DataRoot, path);
        using var reader = File.OpenText(keyedTextYML);
        var deserializer = new DeserializerBuilder().Build();
        return deserializer.Deserialize<Dictionary<string, string>>(reader) ?? [];
    }
    private static readonly ConcurrentDictionary<string /* language/type/file.yml */, IReadOnlyDictionary<string, string[]>> _keyedLocalizedListText = new();
    public static IReadOnlyDictionary<string, string[]> LoadKeyedLocalizedListText(string path) => _keyedLocalizedListText.GetOrAdd(path, LoadKeyedListText);
    private static IReadOnlyDictionary<string, string[]> LoadKeyedListText(string path)
    {
        string keyedTextYML = Path.IsPathFullyQualified(path) ? path : Path.Combine(DataRoot, path);
        using var reader = File.OpenText(keyedTextYML);
        var deserializer = new DeserializerBuilder().Build();
        return deserializer.Deserialize<Dictionary<string, string[]>>(reader) ?? [];
    }
    private static readonly ConcurrentDictionary<string /* language/type/file.yml */, IReadOnlyList<string>> _localizedText = new();
    public static IReadOnlyList<string> LoadLocalizedText(string path) => _localizedText.GetOrAdd(path, LoadText);
    private static IReadOnlyList<string> LoadText(string path)
    {
        string textYML = Path.IsPathFullyQualified(path) ? path : Path.Combine(DataRoot, path);
        using var reader = File.OpenText(textYML);
        var deserializer = new DeserializerBuilder().Build();
        return deserializer.Deserialize<List<string>>(reader) ?? [];
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
    [YamlMember(Alias = "bytes")]
    public required byte[] Bytes { get; set; }
    [YamlMember(Alias = "flags")]
    public YamlSpriteFlags Flags { get; set; }
}
[Flags]
public enum YamlSpriteFlags
{
    /// <summary>Nothing special.</summary>
    None = 0,
    /// <summary>Never place this sprite when randomizing sprites.</summary>
    NoPlace = 1 << 0,
    /// <summary>This sprite only works correctly on the overworld.</summary>
    OverworldOnly = 1 << 1,
    /// <summary>This sprite is an Overlord.</summary>
    Overlord = 1 << 2,
    /// <summary>This sprite may be placed in a challenge room.</summary>
    Challenge = 1 << 3,
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
    public List<List<string>> In { get; set; } = new();
    [YamlMember(Alias = "out")]
    public List<List<string>> Out { get; set; } = new();
}

public class Entrances
{
    [YamlMember(Alias = "fixed")]
    public List<List<string>> Fixed { get; set; } = new();
    [YamlMember(Alias = "connections")]
    public List<ConnectionGroup> Connections { get; set; } = new();
    [YamlMember(Alias = "multi")]
    public ConnectionGroup Multi { get; set; } = new();
}

public partial class Map
{
    [YamlMember(Alias = "map")]
    public int MapMap { get; set; }

    [YamlMember(Alias = "moonpearl")]
    public bool Moonpearl { get; set; }

    [YamlMember(Alias = "nodes")]
    public required MapNodes Nodes { get; set; }
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
    public required string Name { get; set; }

    [YamlMember(Alias = "items")]
    public List<string> Items { get; set; } = new();

    [YamlMember(Alias = "connections")]
    public Dictionary<string, List<string>> Connections { get; set; } = new();
}

public class Prizepack
{
    [YamlMember(Alias = "name")]
    public required string Name { get; set; }

    [YamlMember(Alias = "addresses")]
    public required long[] Addresses { get; set; }

    [YamlMember(Alias = "sprite")]
    public required string Sprite { get; set; }

    [YamlMember(Alias = "deny")]
    public List<string> Deny { get; set; } = [];

    [YamlMember(Alias = "allow")]
    public List<string> Allow { get; set; } = [];
}

public partial class Entrance
{
    [YamlMember(Alias = "name")]
    public required string Name { get; set; }

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
    public required string Name { get; set; }

    [YamlMember(Alias = "entranceids")]
    public List<int> EntranceIds { get; set; } = new();

    [YamlMember(Alias = "conditions")]
    public List<string> Conditions { get; set; } = new();
}

public partial class ItemEntry
{
    [YamlMember(Alias = "name")]
    public required string Name { get; set; }

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
    public required string Name { get; set; }

    [YamlMember(Alias = "position")]
    public Position? Position { get; set; }

    [YamlMember(Alias = "connections")]
    public Dictionary<string, List<string>> Connections { get; set; } = new();
}

public partial class Room
{
    [YamlMember(Alias = "roomid")]
    public int Roomid { get; set; }

    [YamlMember(Alias = "nodes")]
    public required RoomNodes Nodes { get; set; }

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

public partial class InventoryEntry
{
    [YamlMember(Alias = "name")]
    public required string Name { get; set; }

    [YamlMember(Alias = "type")]
    public VertexType Type { get; set; }

    [YamlMember(Alias = "item")]
    public required string Item { get; set; }

    [YamlMember(Alias = "cost")]
    public int Cost { get; set; }

    [YamlMember(Alias = "itemset")]
    public List<string> ItemSet { get; set; } = new();
}

public partial class Entity
{
    [YamlMember(Alias = "name")]
    public required string Name { get; set; }

    [YamlMember(Alias = "addresses")]
    public List<int> Addresses { get; set; } = new();

    [YamlMember(Alias = "position")]
    public required Position Position { get; set; }

    [YamlMember(Alias = "sprite")]
    public required string Sprite { get; set; }

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
    public required string Name { get; set; }

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

    [YamlMember(Alias = "bosses")]
    public Dictionary<string, List<Entity>> Bosses { get; set; } = new();
}

public class Vertices
{
    [YamlMember(Alias = "maps")]
    public List<Map> Maps { get; set; } = new();
    [YamlMember(Alias = "rooms")]
    public List<Room> Rooms { get; set; } = new();
}
