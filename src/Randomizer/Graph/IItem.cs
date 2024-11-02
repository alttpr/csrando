namespace Randomizer.Graph;

public interface IItem
{
    int Id { get; set; }
    string Name { get; }
    IWorld World { get; }
    /// <summary>The permanent health value this item grants the player. Use <c>0</c> (zero) to indicate this item does not add health.</summary>
    float HealthValue { get; }
}
