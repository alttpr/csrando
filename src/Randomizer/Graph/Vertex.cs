namespace Randomizer.Graph;

using System.Data.Common;
using System.Diagnostics;

// FIXME: we need a sprite class that does something.
public record class Sprite(string Name)
{
    public byte?[] Sheets = [null, null, null, null];
    public static Sprite Get(string name)
    {
        return new(name);
    }
}

public enum PegState
{
    Orange,
    Blue,
}

public enum VertexType
{
    BigChest,
    BigKeydoor,
    Bonk,
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
    public bool Dark { get; init; }
    public List<string> ExtraLight { get; init; } = [];
    public bool Switch { get; set; }
    public int? Cost { get; set; }
    public Item? Item { get; set; }
    public Item? Trophy { get; init; }
    public PegState? Peg { get; init; }
    public Sprite? Sprite { get; set; }
    public string? EnemizerBoss { get; set; }
    public int? RoomId { get; init; }
    public int? Map { get; init; }
    public bool? MoonPearl { get; init; }
    public string[] ItemSet { get; init; } = [];
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
    public int? Group { get; init; }
    public Item? Key { get; init; }

    public List<Edge> Edges = new();

    public object Clone()
    {
        return MemberwiseClone();
    }
}
