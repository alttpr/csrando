namespace Randomizer.Graph;

public record ItemSetName(string Name, IWorld? World)
{
    public override string ToString() => World != null ? $"{Name}:{World.GameId}:{World.Id}" : Name;

    public static readonly ItemSetName DefaultSet = new("*", null);
}
