
using Randomizer.Graph.Combo.SuperMetroid.Model;
using System.Data.Common;
using System.Diagnostics;
using static Randomizer.Graph.Game;

namespace Randomizer.Graph;

// FIXME: we need a sprite class that does something.
public record class Sprite(string Name, byte[]? Bytes = null)
{
    public byte?[] Sheets = [null, null, null, null];
    public static Sprite Get(string name)
    {
        var spriteData = YamlReader.LoadSprites();
        byte[]? spriteBytes = null;
        if (spriteData.TryGetValue(name, out var sprite))
            spriteBytes = sprite?.Bytes;
        return new(name, spriteBytes);
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
    Visible,
    Chozo,
    Hidden
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
    public int? RoomId { get; init; }
    public int? Map { get; init; }
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
    public Game? Game { get; init; }
    public Requirement? InteractionRequires { get; init; }
    public string[]? Yields { get; init; }

    public List<Edge> Edges = new();

    public override string ToString()
    {
        return $"{Name}:{World.Id}";
    }

    public object Clone()
    {
        return MemberwiseClone();
    }

    // Not sure if this should go here, but it's ok for now
    // This determines if an item can be placed at this location
    // depending on the current item weight we're placing for
    // A Searcher is passed in so we can check for reachability of other locations
    public bool CanPlace(Item item, int itemWeight, Searcher searcher)
    {
        // Don't place out of world progression items inside GT
        if (item.Game != Alttp && Group == 16 && itemWeight < 9000)
        {
            return false;
        }

        // Don't place out of world progression items inside level 9
        if (item.Game != Zelda && ItemSet.Any(x => x.Name == "z1d9") && itemWeight < 9000)
        {
            return false;
        }

        return true;
    }
}
