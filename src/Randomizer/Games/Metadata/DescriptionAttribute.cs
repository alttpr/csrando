namespace Randomizer.Games.Metadata;

/// <summary>Provides a nice, user-friendly description for this member.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class DescriptionAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}
