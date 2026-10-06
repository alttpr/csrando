namespace Randomizer.Games.Metadata;

using System;

/// <summary>
/// Indicates this property is a packed bit field reliant on other properties to control its value, 
/// and it should not be set directly by the generator except during initialization sequences.
/// <para>
/// Properties specified by <see cref="ControllingPropertyName"/> should be marked with
/// <see cref="PackedBitfieldAccessorAttribute"/>.
/// </para>
/// </summary>
/// <remarks>
/// This attribute is intended for annotating binary data that is already packed in a game's
/// underlying architecture and should not be seen as encouraging unnecessary size optimization of types.
/// </remarks>
/// <param name="propertyName">The name of the property </param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
internal class PackedBitfieldAttribute(string propertyName) : Attribute
{
    public string ControllingPropertyName => propertyName;
}


/// <summary>
/// Indicates this property is used to access a portion of a packed bitfield.
/// </summary>
/// <inheritdoc cref="PackedBitfieldAttribute" path="//remarks"/>
/// <param name="propertyName">The packed bit field this property helps control.</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal class PackedBitfieldAccessorAttribute(string propertyName) : Attribute
{
    public string ControlledPropertyName => propertyName;
}

