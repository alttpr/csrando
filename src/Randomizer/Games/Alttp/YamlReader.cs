namespace Randomizer.Games.Alttp;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Randomizer.Graph;
using YamlDotNet.Serialization;

public class YamlReader
{
    private static Lazy<string> _dataRoot = new(() =>
    {
        DirectoryInfo? currentDirectory = new(Directory.GetCurrentDirectory());

        do
        {
            // First try the published output structure (Games/Alttp/data)
            string publishedDataRoot = Path.Combine(currentDirectory.FullName, "Games/Alttp/data");
            if (Directory.Exists(publishedDataRoot))
                return publishedDataRoot;

            // Then try the source structure (src/Randomizer/Games/Alttp/data)
            string dataRoot = Path.Combine(currentDirectory.FullName, "src/Randomizer/Games/Alttp/data");
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
    private const string BossesPath = "Enemizer/bosses.yml";
    private const string EnemiesPath = "Enemizer/enemies.yml";
    private const string TileRoomPatternsPath = "TileRoomPatterns";

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
    private static readonly Lazy<Dictionary<string, YamlBossSprite[]>> _cachedBossSprites = new(() =>
    {
        string itemsYML = Path.Combine(DataRoot, "bosses.yml");

        var deserializer = new DeserializerBuilder().Build();
        using var reader = File.OpenText(itemsYML);
        var result = deserializer.Deserialize<Dictionary<string, YamlBossSprite[]>>(reader);

        return result;
    });

    private static readonly ConcurrentDictionary<string, Dictionary<string, DirectedUndirectedPair>> _cachedEdges = new();
    private static readonly ConcurrentDictionary<string, Dictionary<string, DirectedUndirectedPair>> _cachedTechEdges = new();

    public static Dictionary<string, YamlItem> LoadItems() => _cachedItems.Value;
    public static Dictionary<string, YamlSprite> LoadSprites() => _cachedSprites.Value;
    public static Dictionary<string, YamlBossSprite[]> LoadBossSprites() => _cachedBossSprites.Value;

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
            }
            else
            {
                dest.Add(entry.Key, new()
                {
                    Directed = entry.Value.Directed.ToList(),
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

    private static readonly Lazy<List<TileRoomPattern>> _tileRoomPatterns = new(() =>
    {
        var patterns = new List<TileRoomPattern>();
        var deserializer = new DeserializerBuilder().Build();

        var files = Directory.GetFiles(Path.Combine(DataRoot, TileRoomPatternsPath), "*.yml", SearchOption.AllDirectories).Order();
        foreach (string file in files)
        {
            using var reader = File.OpenText(file);
            var pattern = deserializer.Deserialize<TileRoomPattern>(reader);
            pattern.Name = Path.GetFileNameWithoutExtension(file);
            patterns.Add(pattern);
        }

        return patterns;
    });
    public static IEnumerable<TileRoomPattern> LoadTileRoomPatterns() => _tileRoomPatterns.Value;

    public class GameData
    {
        [YamlMember(Alias = "rooms")] public List<GameRoom> Rooms { get; set; } = new();
        [YamlMember(Alias = "enemy")] public GameEnemyData Enemy { get; set; } = new();
    }
    public class GameRoom
    {
        [YamlMember(Alias = "room")] public int Room { get; set; }
        [YamlMember(Alias = "ptr")] public int Ptr { get; set; }
        [YamlMember(Alias = "tiles_ptr")] public int TilesPtr { get; set; }
        [YamlMember(Alias = "floor1")] public byte Floor1 { get; set; }
        [YamlMember(Alias = "floor2")] public byte Floor2 { get; set; }
        [YamlMember(Alias = "layout")] public byte Layout { get; set; }
        [YamlMember(Alias = "upper_layer")] public byte[] UpperLayer { get; set; } = [];
        [YamlMember(Alias = "lower_layer")] public byte[] LowerLayer { get; set; } = [];
        [YamlMember(Alias = "priority_layer")] public byte[] PriorityLayer { get; set; } = [];
        [YamlMember(Alias = "door_ptr")] public int DoorPtr { get; set; }
        [YamlMember(Alias = "door_ptr_entry_addr")] public int DoorPtrEntryAddress { get; set; }
        [YamlMember(Alias = "door_data")] public byte[] DoorData { get; set; } = [];
    }
    public class GameEnemyData
    {
        [YamlMember(Alias = "health")] public List<byte> Health { get; set; } = new();
        [YamlMember(Alias = "damage")] public List<byte> Damage { get; set; } = new();
    }
    private static readonly Lazy<GameData> _gameData = new(() =>
    {
        var path = Path.Combine(DataRoot, "game_data.yml");
        using var reader = File.OpenText(path);
        var deserializer = new DeserializerBuilder().Build();
        return deserializer.Deserialize<GameData>(reader) ?? new GameData();
    });
    public static GameData LoadGameData() => _gameData.Value;

    private static readonly Lazy<Dictionary<int /* OutletId */, YamlOutletData>> _outletData = new(() =>
    {
        var path = Path.Combine(DataRoot, "outlet_overrides.yml");
        using var reader = File.OpenText(path);
        var deserializer = new DeserializerBuilder().Build();
        return deserializer.Deserialize<Dictionary<int, YamlOutletData>>(reader) ?? new();
    });
    public static Dictionary<int, YamlOutletData> LoadOutletData() => _outletData.Value;

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

public class YamlOutletData
{
    [YamlMember(Alias = "x")]
    public short? X { get; set; }
    [YamlMember(Alias = "y")]
    public short? Y { get; set; }
    [YamlMember(Alias = "camera_x")]
    public short? CameraX { get; set; }
    [YamlMember(Alias = "camera_y")]
    public short? CameraY { get; set; }
    [YamlMember(Alias = "scroll_x")]
    public short? ScrollX { get; set; }
    [YamlMember(Alias = "scroll_y")]
    public short? ScrollY { get; set; }
}

public class YamlItem
{
    [YamlMember(Alias = "bytes")]
    public List<byte> Bytes { get; set; } = new();
    [YamlMember(Alias = "type")]
    public string Type { get; set; } = string.Empty;
    [YamlMember(Alias = "tier")]
    public string? Tier { get; set; }
}

public class YamlSprite
{
    [YamlMember(Alias = "id")]
    public required byte Id { get; set; }
    [YamlMember(Alias = "flags")]
    public YamlSpriteFlags Flags { get; set; }
    [YamlMember(Alias = "subtype")]
    public byte SubType { get; set; } = 0x00;
    [YamlMember(Alias = "sheets")]
    public byte?[] Sheets { get; set; } = [null, null, null, null];
    [YamlMember(Alias = "alternative")]
    public string? AlternativeName { get; set; }
    /// <summary>When set, this sprite is the falling sprite for the one returned here.</summary>
    [YamlMember(Alias = "falling")]
    public string? FallingSpriteFor { get; set; }
    [YamlMember(Alias = "priority")]
    public int Priority { get; set; }
    [YamlMember(Alias = "single_layer_collision")]
    public bool SingleLayerCollision { get; set; }
    [YamlMember(Alias = "ignored_by_killrooms")]
    public bool IgnoredByKillRooms { get; set; }
    [YamlMember(Alias = "persist_offscreen")]
    public bool PersistOffScreenOW { get; set; }
    [YamlMember(Alias = "hitbox")]
    public byte Hitbox { get; set; }
    [YamlMember(Alias = "limited_interaction")]
    public bool LimitedPitConveyorInteraction { get; set; }
    [YamlMember(Alias = "water_check")]
    public bool WaterCheck { get; set; }
    [YamlMember(Alias = "shield_blockable")]
    public bool ShieldBlockable { get; set; }
    [YamlMember(Alias = "boss_damage_sfx")]
    public bool BossDamageSFX { get; set; }
    [YamlMember(Alias = "prize_pack")]
    public byte PrizePack { get; set; }
    [YamlMember(Alias = "not_with")]
    public string[]? NotWith { get; set; }
    [YamlMember(Alias = "weight")]
    public int Weight { get; set; } = 1;
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
    /// <summary>This sprite shouldn't hold item drops that might affect progression.</summary>
    NoDrop = 1 << 4,
    /// <summary>This sprite only appears under a pot.</summary>
    UnderPots = 1 << 5,
}

public class YamlBossSprite
{
    [YamlMember(Alias = "name")]
    public required string Name { get; set; }
    [YamlMember(Alias = "position")]
    public required Position Position { get; set; }
    [YamlMember(Alias = "sprite")]
    public required string Sprite { get; set; }
    [YamlMember(Alias = "lower_layer")]
    public YamlRoomObjectPatch[]? LowerLayer { get; set; }
    [YamlMember(Alias = "blkset")]
    public byte? Blkset { get; set; }
    [YamlMember(Alias = "bg2prop")]
    public byte? BG2Prop { get; set; }
    [YamlMember(Alias = "floor1")]
    public byte? Floor1 { get; set; }
    [YamlMember(Alias = "floor2")]
    public byte? Floor2 { get; set; }
}
public class YamlRoomObjectPatch
{
    [YamlMember(Alias = "id")]
    public required ushort ObjectId { get; set; }
    [YamlMember(Alias = "x")]
    public int OffsetX { get; set; }
    [YamlMember(Alias = "y")]
    public int OffsetY { get; set; }

    [return: NotNullIfNotNull(nameof(left))]
    [return: NotNullIfNotNull(nameof(right))]
    public static Position? operator +(Position? left, YamlRoomObjectPatch? right)
    {
        if (left is null && right is null)
            return null;

        return new Position
        {
            X = (left?.X).GetValueOrDefault() + (right?.OffsetX).GetValueOrDefault(),
            Y = (left?.Y).GetValueOrDefault() + (right?.OffsetY).GetValueOrDefault(),
            Z = left?.Z,
        };
    }
}

public class TileRoomPattern
{
    public string Name { get; set; } = null!;
    [YamlMember(Alias = "speed")]
    public byte Speed { get; set; } = 0xE0;
    [YamlMember(Alias = "tiles")]
    public TileRoomTile[] Tiles { get; set; } = [];
}
public class TileRoomTile
{
    [YamlMember(Alias = "x")]
    public required int X { get; set; }
    [YamlMember(Alias = "y")]
    public required int Y { get; set; }
}

public class DirectedUndirectedPair
{

    [YamlMember(Alias = "directed")]
    public List<List<string>> Directed { get; set; } = new();
}

public class ConnectionGroup
{
    [YamlMember(Alias = "group")]
    public string Group { get; set; } = string.Empty;

    [YamlMember(Alias = "overworld")]
    public List<List<List<string>>> Overworld { get; set; } = new();
    [YamlMember(Alias = "underworld")]
    public List<List<List<string>>> Underworld { get; set; } = new();
}

public class Entrances
{
    [YamlMember(Alias = "fixed")]
    public List<List<string>> Fixed { get; set; } = new();
    [YamlMember(Alias = "scoped")]
    public List<ConnectionGroup> Scoped { get; set; } = new();
    [YamlMember(Alias = "connections")]
    public List<ConnectionGroup> Connections { get; set; } = new();
}

public partial class Map
{
    [YamlMember(Alias = "map")]
    public int MapMap { get; set; }

    [YamlMember(Alias = "moonpearl")]
    public bool Moonpearl { get; set; }

    [YamlMember(Alias = "sheets")]
    public byte?[] Sheets { get; set; } = [null, null, null, null];

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

    [YamlMember(Alias = "oam")]
    public byte OAM { get; set; } = 0x00;

    [YamlMember(Alias = "nodes")]
    public required RoomNodes Nodes { get; set; }

    [YamlMember(Alias = "group")]
    public int? Group { get; set; }

    [YamlMember(Alias = "dark")]
    public bool Dark { get; set; } = false;

    [YamlMember(Alias = "sheets")]
    public byte?[] Sheets { get; set; } = [null, null, null, null];

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

    [YamlMember(Alias = "count")]
    public int Count { get; set; } = 1;

    [YamlMember(Alias = "itemset")]
    public List<string> ItemSet { get; set; } = new();
}

public partial class Entity
{
    [YamlMember(Alias = "name")]
    public required string Name { get; set; }

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

    [return: NotNullIfNotNull(nameof(left))]
    [return: NotNullIfNotNull(nameof(right))]
    public static Position? operator +(Position? left, Position? right)
    {
        if (left is null && right is null)
            return null;

        return new Position
        {
            X = (left?.X).GetValueOrDefault() + (right?.X).GetValueOrDefault(),
            Y = (left?.Y).GetValueOrDefault() + (right?.Y).GetValueOrDefault(),
            Z = (left?.Z).GetValueOrDefault() + (right?.Z).GetValueOrDefault(),
        };
    }
    [return: NotNullIfNotNull(nameof(self))]
    public static Position? operator *(Position? self, int mult)
    {
        if (self is null)
            return null;

        return new Position
        {
            X = self.X * mult,
            Y = self.Y * mult,
            Z = self.Z * mult,
        };
    }
    [return: NotNullIfNotNull(nameof(self))]
    public static Position? operator /(Position? self, int div)
    {
        if (self is null)
            return null;

        return new Position
        {
            X = self.X / div,
            Y = self.Y / div,
            Z = self.Z / div,
        };
    }
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
    public byte? Shopkeeper { get; set; }
    [YamlMember(Alias = "palette")]
    public byte? ShopPalette { get; set; }

    [YamlMember(Alias = "infinite_stock")]
    public bool InfiniteStock { get; set; }
    [YamlMember(Alias = "alt_vram")]
    public bool AlternativeVRAM { get; set; }

    [YamlMember(Alias = "switch")]
    public bool? Switch { get; set; }

    [YamlMember(Alias = "pit")]
    public bool Pit { get; set; } = false;

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
    public List<string> Bosses { get; set; } = new();
    [YamlMember(Alias = "offset")]
    public Position? Offset { get; set; }
}

public class Vertices
{
    [YamlMember(Alias = "maps")]
    public List<Map> Maps { get; set; } = new();
    [YamlMember(Alias = "rooms")]
    public List<Room> Rooms { get; set; } = new();
}
