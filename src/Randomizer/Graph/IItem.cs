namespace Randomizer.Graph;

public interface IItem
{
    int Id { get; set; }
    string Name { get; }
    IWorld World { get; }
}
