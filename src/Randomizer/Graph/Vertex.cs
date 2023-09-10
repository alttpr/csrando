namespace AlttpRandomizer.Graph;

using System.Diagnostics;

// FIXME: we need a sprite class that does something.
public record class Sprite(string Name)
{
    public byte?[] Sheets = new byte?[4] { null, null, null, null };
    public static Sprite Get(string name)
    {
        return new(name);
    }
}

/**
 * Vertex in Graph.
 */
[DebuggerDisplay("{Name} ({Type})")]
public sealed class Vertex
{
    public string Type { get; }
    public string Name { get; }
    public bool Switch { get; }
    public int? Cost { get; set; }
    public Item? Item { get; set; }
    public Item? Trophy { get; }
    public string? Peg { get; }
    public Sprite? Sprite { get; set; }
    public string? EnemizerBoss { get; set; }
    public int? Roomid { get; }
    public int? Map { get; }
    public bool? MoonPearl { get; }
    public string[]? ItemSet { get; }
    public long[]? Addresses { get; }
    public int? Offset { get; }
    public Position? Position { get; }
    public int[]? State { get; }
    public int? EntranceId { get; }
    public int? OutletId { get; }
    public int? InletId { get; }
    public int[]? EntranceIds { get; }
    public int? ShopStyle { get; }
    public int? Shopkeeper { get; }
    public int? Group { get; }

    public Vertex(Dictionary<string, object>? attributes = null)
    {
        attributes ??= new();
        Switch = (bool)(attributes.GetValueOrDefault("switch") ?? false);
        Peg = (string?)attributes.GetValueOrDefault("peg");
        Cost = (int?)attributes.GetValueOrDefault("cost");
        Type = (string?)attributes.GetValueOrDefault("type") ?? "unknown";
        Name = (string?)attributes.GetValueOrDefault("name") ?? $"vertex{GetHashCode()}";
        if (attributes.TryGetValue("sprite", out object? spriteObj))
        {
            if (spriteObj is Sprite sprite)
                Sprite = sprite;
            else if (spriteObj is string spriteName)
                Sprite = Sprite.Get(spriteName);
        }
        Item = (Item?)attributes.GetValueOrDefault("item");
        Trophy = (Item?)attributes.GetValueOrDefault("trophy");
        Roomid = (int?)attributes.GetValueOrDefault("roomid");
        Map = (int?)attributes.GetValueOrDefault("map");
        MoonPearl = (bool?)attributes.GetValueOrDefault("moonpearl");
        ItemSet = (string[]?)attributes.GetValueOrDefault("itemset");
        Addresses = ((List<long>?)attributes.GetValueOrDefault("addresses"))?.ToArray();
        Offset = (byte?)attributes.GetValueOrDefault("offset");
        if (attributes.TryGetValue("position", out object? positionObj) && positionObj is Position position)
            Position = position;
        State = ((List<int>?)attributes.GetValueOrDefault("state"))?.ToArray();
        EntranceId = (int?)attributes.GetValueOrDefault("entranceid");
        OutletId = (int?)attributes.GetValueOrDefault("outletid");
        InletId = (int?)attributes.GetValueOrDefault("inletid");
        EntranceIds = ((List<int>?)attributes.GetValueOrDefault("entranceids"))?.ToArray();
        ShopStyle = (int?)attributes.GetValueOrDefault("shopstyle");
        Shopkeeper = (int?)attributes.GetValueOrDefault("shopkeeper");
        Group = (int?)attributes.GetValueOrDefault("group");
    }
}
