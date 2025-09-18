namespace Randomizer.Games.Metadata;

/// <summary>
/// Indicates which game this settings class is for.
/// The randomizer expects one of these for every game-specific settings class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class TargetGameAttribute : Attribute
{
    public Game? Game { get; init; }
    public RandomizerTarget? Randomizer { get; init; }

    public TargetGameAttribute(Game game)
    {
        Game = game;
    }
    public TargetGameAttribute(RandomizerTarget randomizer)
    {
        Randomizer = randomizer;
    }
}
