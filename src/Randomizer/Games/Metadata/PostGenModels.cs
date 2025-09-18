namespace Randomizer.Games.Metadata;

using System.Text.Json.Serialization;

// JSON models for the postGenSettings block

public sealed class MetaPostGenPatch
{
    [JsonPropertyName("targetAddress")] public string TargetAddress { get; init; } = string.Empty;
    [JsonPropertyName("data")] public string Data { get; init; } = string.Empty; // hex string
}

public sealed class MetaPostGenPatchGroup
{
    [JsonPropertyName("patches")] public List<MetaPostGenPatch> Patches { get; init; } = new();
}

public sealed class MetaPostGenSelectChoice
{
    [JsonPropertyName("value")] public string Value { get; init; } = string.Empty;
    [JsonPropertyName("label")] public string Label { get; init; } = string.Empty;
    [JsonPropertyName("patches")] public List<MetaPostGenPatch> Patches { get; init; } = new();
}

public sealed class MetaPostGenSetting
{
    [JsonPropertyName("type")] public string Type { get; init; } = string.Empty; // toggle | select
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("description")] public string? Description { get; init; }

    // default is bool for toggle, string for select
    [JsonPropertyName("default")] public object? Default { get; init; }

    // toggle
    [JsonPropertyName("on")] public MetaPostGenPatchGroup? On { get; init; }
    [JsonPropertyName("off")] public MetaPostGenPatchGroup? Off { get; init; }

    // select
    [JsonPropertyName("choices")] public List<MetaPostGenSelectChoice>? Choices { get; init; }
}

public sealed class MetaPostGenGameOptions
{
    [JsonPropertyName("options")] public List<MetaPostGenSetting> Options { get; init; } = new();
}
