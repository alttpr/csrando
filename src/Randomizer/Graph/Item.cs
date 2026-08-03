namespace Randomizer.Graph;

public abstract class Item(string name, IWorld world) : IItem
{
    public int Id { get; set; } = -1;
    public string Name { get; } = name;
    public IWorld World { get; } = world;
    public byte[]? Bytes { get; set; }

    public float HealthValue { get; protected init; } = 0;
    public ItemTier Tier { get; protected init; } = ItemTier.Major;
    public IItem? LogicalItem { get; protected init; }

    public sealed override string ToString() => $"{Name}:{World.GameId}:{World.Id}";
}
