namespace Randomizer.Games.Metadata;

// TODO: should this be on the property, or on the settings class?
//       property gives us the option of using it for other properties as well
//       (such as marking unsupported game-specific settings for multi-game randomizers).
/// <summary>
/// Indicates which randomizers the settings are compatible with.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class UsableWithAttribute(params RandomizerTarget[] randomizers) : Attribute
{
    public RandomizerTarget[] Randomizers { get; } = randomizers;
}
