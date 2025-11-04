namespace Randomizer.Games.Metadata;

/// <summary>
/// Marks a setting property as work in progress; meaning the functionality doesn't work at all or is likely to be broken.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class WipAttribute(string reason) : Attribute
{
    public string Reason { get; } = reason;
}

