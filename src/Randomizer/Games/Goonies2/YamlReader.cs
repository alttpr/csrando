namespace Randomizer.Games.Goonies2;

using YamlDotNet.Serialization;

using Cfg = global::Randomizer.Config;

public class YamlReader
{
    private static Lazy<string> _dataRoot = new(() =>
    {
        DirectoryInfo? currentDirectory = new(Directory.GetCurrentDirectory());

        do
        {
            string dataRoot = Path.Combine(currentDirectory.FullName, "src/Randomizer/Games/Goonies2/data");
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

    private static readonly Lazy<Dictionary<string, YamlItem>> _cachedItems = new(() =>
    {
        string itemsYML = Path.Combine(DataRoot, ItemsPath);

        var deserializer = new StaticDeserializerBuilder(Cfg.StaticContext).Build();
        using var reader = File.OpenText(itemsYML);
        var result = deserializer.Deserialize<Dictionary<string, YamlItem>>(reader);

        return result;
    });
    public static Dictionary<string, YamlItem> LoadItems() => _cachedItems.Value;

    private static readonly Lazy<Vertices> _cachedVertices = new(() =>
    {
        Vertices result = new();

        var files = Directory.GetFiles(Path.Combine(DataRoot, "Vertices"), "*.yml", SearchOption.AllDirectories).Order();
        foreach (string file in files)
        {
            var currentFileEdges = LoadVerticesFromFile(file);
            MergeVertices(result, currentFileEdges);
        }

        return result;
    });
    private static Vertices LoadVerticesFromFile(string path)
    {
        string verticesYML = Path.IsPathFullyQualified(path) ? path : Path.Combine(DataRoot, path);
        using var reader = File.OpenText(verticesYML);
        var deserializer = new StaticDeserializerBuilder(Cfg.StaticContext).Build();
        return deserializer.Deserialize<Vertices>(reader);
    }
    public static Vertices LoadVertices() => _cachedVertices.Value;
    public static void MergeVertices(Vertices dest, Vertices source)
    {
        dest.Regions.AddRange(source.Regions);
        dest.Meta.AddRange(source.Meta);
    }
}

public class YamlItem
{
    [YamlMember(Alias = "byte")]
    public byte Byte { get; set; }
    [YamlMember(Alias = "gfx")]
    public byte Gfx { get; set; }
    [YamlMember(Alias = "type")]
    public string Type { get; set; } = string.Empty;
}

public partial class Region
{
    [YamlMember(Alias = "name")]
    public required string Name { get; set; }
    [YamlMember(Alias = "dark")]
    public bool Dark { get; set; }
    [YamlMember(Alias = "water")]
    public bool Water { get; set; }
    [YamlMember(Alias = "items")]
    public List<ItemEntry> Items { get; set; } = [];
    [YamlMember(Alias = "connections")]
    public Dictionary<string, List<string>> Connections { get; set; } = [];
}
public partial class ItemEntry
{
    [YamlMember(Alias = "name")]
    public required string Name { get; set; }
    [YamlMember(Alias = "addresses")]
    public List<long> Addresses { get; set; } = [];
    [YamlMember(Alias = "item")]
    public string? Item { get; set; }
    [YamlMember(Alias = "itemset")]
    public List<string> ItemSet { get; set; } = [];
    [YamlMember(Alias = "conditions")]
    public List<string> Conditions { get; set; } = [];
}
public partial class MetaNode
{
    [YamlMember(Alias = "name")]
    public required string Name { get; set; }
    [YamlMember(Alias = "items")]
    public List<string> Items { get; set; } = [];
    [YamlMember(Alias = "connections")]
    public Dictionary<string, List<string>> Connections { get; set; } = [];
}
public class Vertices
{
    [YamlMember(Alias = "regions")]
    public List<Region> Regions { get; set; } = [];
    [YamlMember(Alias = "meta")]
    public List<MetaNode> Meta { get; set; } = [];
}
