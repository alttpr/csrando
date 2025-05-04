namespace Randomizer.Games.Metadata;

/// <summary>
/// Identifies a property that contains a random selection for a different <paramref name="PropertyName"/>.
/// Only used when <paramref name="PropertyName"/> is not set.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class RandomizedOptionsForAttribute(string propertyName) : Attribute
{
    public string PropertyName { get; } = propertyName;
}
