namespace Randomizer.Games.SuperMetroid.Model;

using System.Text.Json.Serialization;

public class NotableStrategy
{
    [JsonPropertyName("room_id")]
    public int RoomId { get; set; }

    [JsonPropertyName("room_name")]
    public string RoomName { get; set; } = "";

    [JsonPropertyName("notable_id")]
    public int NotableId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = "";

    [JsonPropertyName("video_id")]
    public int? VideoId { get; set; }
}
