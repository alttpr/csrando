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

    protected override void AdditionalGraphDumpMetadata(Dictionary<string, object?> metadata)
    {
        base.AdditionalGraphDumpMetadata(metadata);
        if (SubType.HasValue)
            metadata["Z3.Subtype"] = SubType.Value.ToString();
        if (Dark)
            metadata["Z3.Dark"] = "yes";
        if (MoonPearl.GetValueOrDefault())
            metadata["Z3.MoonPearlRequired"] = "yes";
        if (Cost is int cost and not 0)
            metadata["Z3.Cost"] = cost.ToString();
        if (Type == VertexType.Entrance && EntranceId.HasValue)
            metadata["Z3.Entrance"] = EntranceId.Value.ToString();
        if (EntranceIds is int[] entrances)
            metadata["Z3.Entrances"] = string.Join(", ", entrances);
        if (Type == VertexType.Outlet && OutletId.HasValue)
            metadata["Z3.Outlet"] = OutletId.Value.ToString();
        if (InletId.HasValue && InletId.Value != 0)
            metadata["Z3.Inlet"] = InletId.Value.ToString();
        if (Sprite is { Name: string name })
            metadata["Z3.Sprite"] = name;
    }
}
