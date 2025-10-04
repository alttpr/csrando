namespace Randomizer.Games.Metadata;

/// <summary>
/// Specifies the supported value range for this setting property.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class ValueRangeAttribute(int minInclusive, int maxInclusive) : Attribute
{
    public int MinInclusive { get;} = minInclusive;
    public int MaxInclusive { get; } = maxInclusive;
    public int Default { get; init; } = maxInclusive;
}
