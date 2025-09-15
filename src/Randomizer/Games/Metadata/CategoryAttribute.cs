namespace Randomizer.Games.Metadata;

public enum CategoryDisplay
{
    Static,
    Expanded,
    Collapsed
}

/// <summary>
/// Marks a settings category for grouping
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class CategoryAttribute(string category, CategoryDisplay display = CategoryDisplay.Static) : Attribute
{
    public string Category { get; } = category;
    public CategoryDisplay Display { get; set; } = display;
}
