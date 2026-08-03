namespace Randomizer.Games.Metadata;

/// <summary>
/// Restricts a setting to seeds whose selected games are a subset of the listed
/// games. The web UI hides the setting (and omits its value) as soon as any game
/// outside the list is part of the seed; the generator enforces it server-side.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class OnlyWithGamesAttribute(params Game[] games) : Attribute
{
    public Game[] Games { get; } = games;
}
