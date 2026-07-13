namespace Randomizer.Graph;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Generates a Spoiler log for the randomizer.
/// </summary>
public class SpoilerLog
{
    public Dictionary<string, Dictionary<string, string>> Spoiler { get; } = [];

    public SpoilerLog(GameRandomizer randomizer)
    {
        var playthrough = PlaythroughGenerator.Generate(randomizer);
        Spoiler["playthrough"] = new Dictionary<string, string>
        {
            ["data"] = JsonSerializer.Serialize(playthrough, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            })
        };
        randomizer.AppendSpoiler(this);
    }
}
