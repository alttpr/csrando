namespace Randomizer.Graph;

using System.Diagnostics;
using YamlDotNet.Serialization;

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
public abstract class Vertex : ICloneable
{
    public int Id { get; set; }
    public VertexType Type { get; init; }
    public string Name { get; set; }
    public IWorld World { get; init; }
    public IItem? Item { get; set; }
    public IItem? Trophy { get; init; }
    public ItemSetName[] ItemSet { get; init; } = [];
    public long[]? Addresses { get; init; }

    public List<Edge> Edges = [];

    public override string ToString()
    {
        return $"{Name}:{World.Id}";
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
    public void TrackPlacedItem() => World.TrackPlacedItem(this);
}
