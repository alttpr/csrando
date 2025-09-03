namespace Randomizer.Games.Metadata;

/// <summary>
/// Indicates that a property depends on another property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DependsOnAttribute : Attribute
{
    public string PropertyName { get; }
    public object[] Values { get; }

    public DependsOnAttribute(string propertyName, params object[] values)
    {
        PropertyName = propertyName;
        Values = values;
    }
}
