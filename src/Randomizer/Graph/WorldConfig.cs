namespace Randomizer.Graph;

using global::Randomizer.Games.Alttp;

public class WorldConfig
{
    // game/text language. english only at the moment.
    public string Language { get; init; } = "en";
    public AlttpConfig? Alttp { get; init; }

    public void SelectRandomValues(PRNG prng)
    {
        Alttp?.SelectRandomValues(prng);
    }
}
