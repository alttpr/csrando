namespace Randomizer.Graph;

/// <summary>
/// Generates a Spoiler log for the randomizer.
/// </summary>
public class SpoilerLog
{
    public Dictionary<string, Dictionary<string, string>> Spoiler { get; } = [];

    public SpoilerLog(GameRandomizer randomizer)
    {
        // TODO: generate a playthrough.
        randomizer.AppendSpoiler(this);
    }
}
