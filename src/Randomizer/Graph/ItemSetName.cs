namespace Randomizer.Graph;

using System.Runtime.InteropServices;

public record ItemSetName(string Name, World? World)
{
    public override string ToString()
    {
        return World != null ? $"{Name}:{World.Id}" : Name;
    }

    public static ItemSetName DefaultSet = new ItemSetName("*", null);
}
