namespace Randomizer.Games.Alttp;

using Randomizer.Graph;
using BaseVertex = Graph.Vertex;

public sealed class Vertex : BaseVertex
{
    public VertexType? SubType { get; init; }
    public bool Dark { get; init; }
    public List<string> ExtraLight { get; init; } = [];
    public bool Switch { get; set; }
    public int? Cost { get; set; }
    public Sprite? Sprite { get; set; }
    public bool MightFall { get; init; }
    public int? RoomId { get; init; }
    public byte? RoomOAM { get; init; }
    public int? Map { get; init; }
    public byte?[] Sheets { get; init; } = [null, null, null, null];
    public bool? MoonPearl { get; init; }
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
    public Position? RoomOffset { get; set; }
}
