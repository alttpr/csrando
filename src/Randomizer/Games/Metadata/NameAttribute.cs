namespace Randomizer.Games.Metadata;

/// <summary>Provides a nice, user-friendly name for this member.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class NameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
