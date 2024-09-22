namespace Randomizer.Graph;

using System.Diagnostics;

// FIXME: we need a sprite class that does something.
public record class Sprite(string Name, byte Id)
{
    private static readonly Lazy<Dictionary<string, Sprite>> _sprites = new(() => LoadSprites().ToDictionary(k => k.Name));

    public string DefeatName { get; init; } = Name;
    public byte?[] Sheets { get; init; } = [null, null, null, null];
    public YamlSpriteFlags Flags { get; init; }
    public byte SubType { get; init; }
    public string? FallingSpriteFor { get; init; }

    public static Sprite Get(string name)
        => _sprites.Value.GetValueOrDefault(name)
        ?? throw new ArgumentException($"No such sprite: {name}", nameof(name));
    public static IEnumerable<Sprite> All() => _sprites.Value.Values;

    private static IEnumerable<Sprite> LoadSprites()
    {
        var spriteData = YamlReader.LoadSprites();
        foreach (var (name, sprite) in spriteData)
        {
            yield return new(name, sprite.Id)
            {
                Sheets = sprite.Sheets,
                Flags = sprite.Flags,
                SubType = sprite.SubType,
                DefeatName = sprite.AlternativeName ?? name,
                FallingSpriteFor = sprite.FallingSpriteFor,
            };
        }
    }
}

public enum VertexType
{
    BigChest,
    BigKeydoor,
    Bonk,
    Boss,
    Chest,
    Drop,
    Dig,
    Entrance,
    Event,
    Follower,
    Hole,
    Item,
    Keydoor,
    Medallion,
    Meta,
    Mob,
    Npc,
    Outlet,
    Pedestal,
    Pot,
    Prize,
    PrizePack,
    Shop,
    ShopItem,
    Shutter,
    Standing,
    Refill,
    Region,
    Warp,
}

/// <summary>
/// Vertex in Graph.
/// </summary>
[DebuggerDisplay("{Name} ({Type})")]
public sealed class Vertex : ICloneable
{
    public int Id { get; set; }
    public required VertexType Type { get; init; }
    public VertexType? SubType { get; init; }
    public required string Name { get; set; }
    public required World World { get; init; }
    public bool Dark { get; init; }
    public List<string> ExtraLight { get; init; } = [];
    public bool Switch { get; set; }
    public int? Cost { get; set; }
    public Item? Item { get; set; }
    public Item? Trophy { get; init; }
    public Sprite? Sprite { get; set; }
    public bool MightFall { get; init; }
    public int? RoomId { get; init; }
    public byte? RoomOAM { get; init; }
    public int? Map { get; init; }
    public byte?[] Sheets { get; init; } = [null, null, null, null];
    public bool? MoonPearl { get; init; }
    public ItemSetName[] ItemSet { get; init; } = [];
    public long[]? Addresses { get; init; }
    public int? Offset { get; init; }
    public Position? Position { get; init; }
    public int[]? State { get; init; }
    public int? EntranceId { get; init; }
    public int? OutletId { get; init; }
    public int? InletId { get; init; }
    public int[]? EntranceIds { get; init; }
    public int? ShopStyle { get; init; }
    public int? Shopkeeper { get; init; }
    public string[]? Allow { get; init; }
    public string[]? Deny { get; init; }
    public int? Group { get; init; }
    public Item? Key { get; init; }

    public List<Edge> Edges = new();

    public override string ToString()
    {
        return $"{Name}:{World.Id}";
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
