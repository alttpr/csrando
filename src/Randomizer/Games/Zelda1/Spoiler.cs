namespace Randomizer.Games.Zelda1;

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

internal static class Spoiler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static void AppendDungeonMaps(
        Dictionary<string, Dictionary<string, string>> spoiler,
        List<DungeonSpoilerData> dungeons)
    {
        var json = JsonSerializer.Serialize(dungeons, JsonOptions);
        spoiler["z1DungeonMaps"] = new Dictionary<string, string> { ["data"] = json };
    }
}
