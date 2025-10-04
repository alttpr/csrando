namespace Randomizer.Games.Metadata;

/// <summary>
/// Marks a settings subcategory for grouping
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SubcategoryAttribute(string subcategory) : Attribute
{
    public string Subcategory { get; } = subcategory;
}
