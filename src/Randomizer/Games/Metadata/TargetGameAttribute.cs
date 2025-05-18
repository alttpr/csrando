namespace Randomizer.Games.Metadata;

/// <summary>
/// Indicates which game this settings class is for.
/// The randomizer expects one of these for every game-specific settings class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class TargetGameAttribute(Game game) : Attribute
{
    public Game Game { get; } = game;
}
