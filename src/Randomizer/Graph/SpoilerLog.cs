namespace Randomizer.Graph;

/// <summary>
/// Generates a Spoiler log for the randomizer.
/// </summary>
public class SpoilerLog
{
    private readonly Graph _graph;
    public Dictionary<string, Dictionary<string, string>> Spoiler { get; }

    public SpoilerLog(GameRandomizer randomizer)
    {
        _graph = randomizer.Graph;
        Spoiler = new Dictionary<string, Dictionary<string, string>>()
        {
            { "Equipped", new() },
            { "Locations", new() }
        };

        // TODO: generate a playthrough.

        randomizer.AppendSpoiler(this);
    }
}
