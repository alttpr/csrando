namespace Randomizer.Games.Metadata;

/// <summary>
/// Marks a setting property as experimental; this is likely broken and handled be used care.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ExperimentalAttribute(string reason) : Attribute
{
    public string Reason { get; } = reason;
}
