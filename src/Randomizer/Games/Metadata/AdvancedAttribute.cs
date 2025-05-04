namespace Randomizer.Games.Metadata;

/// <summary>
/// Marks a setting property as advanced; most players probably don't want to mess with this.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class AdvancedAttribute(string reason) : Attribute
{
    public string Reason { get; } = reason;
}
