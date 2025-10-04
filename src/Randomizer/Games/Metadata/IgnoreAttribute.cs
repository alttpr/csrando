namespace Randomizer.Games.Metadata;

/// <summary>
/// Marks a setting property as ignored; it wont be returned as possible option.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class IgnoreAttribute(string reason) : Attribute
{
    public string Reason { get; } = reason;
}
