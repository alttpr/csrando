namespace Randomizer.Games.Metadata;

/// <summary>
/// Marks a settings category for grouping
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class CategoryAttribute(string category) : Attribute
{
    public string Category { get; } = category;
}
