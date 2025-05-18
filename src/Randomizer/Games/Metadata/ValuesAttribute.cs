namespace Randomizer.Games.Metadata;

/// <summary>
/// Specifies the supported values for this setting property.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class ValuesAttribute(params object[] values) : Attribute
{
    public object[] Values { get; } = values;
    public object Default { get; init; } = values.First();
}
