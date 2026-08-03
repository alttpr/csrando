namespace Randomizer.Games.Metadata;

using System.Collections.Generic;
using System.Text.Json.Serialization;

// JSON models for the postGenSettings block

public sealed class MetaPostGenPatch
{
    [JsonPropertyName("targetAddress")] public int TargetAddress { get; init; }
    [JsonPropertyName("data")] public List<int> Data { get; init; } = new();
}

public sealed class MetaPostGenPatchGroup
{
    [JsonPropertyName("patches")] public List<MetaPostGenPatch> Patches { get; init; } = new();
}

public sealed class MetaPostGenNumberPatch
{
    [JsonPropertyName("targetAddress")] public int TargetAddress { get; init; }

    /// <summary>Number of little-endian bytes the selected value is written as.</summary>
    [JsonPropertyName("length")] public int Length { get; init; } = 1;
}

public sealed class MetaPostGenSelectChoice
{
    [JsonPropertyName("value")] public string Value { get; init; } = string.Empty;
    [JsonPropertyName("label")] public string Label { get; init; } = string.Empty;
    [JsonPropertyName("patches")] public List<MetaPostGenPatch> Patches { get; init; } = new();
}

public sealed class MetaPostGenSetting
{
    [JsonPropertyName("type")] public string Type { get; init; } = string.Empty; // toggle | select | number
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("description")] public string? Description { get; init; }

    // default is bool for toggle, string for select, int for number
    [JsonPropertyName("default")] public object? Default { get; init; }

    // toggle
    [JsonPropertyName("on")] public MetaPostGenPatchGroup? On { get; init; }
    [JsonPropertyName("off")] public MetaPostGenPatchGroup? Off { get; init; }

    // select
    [JsonPropertyName("choices")] public List<MetaPostGenSelectChoice>? Choices { get; init; }

    // number
    [JsonPropertyName("min")] public int? Min { get; init; }
    [JsonPropertyName("max")] public int? Max { get; init; }
    [JsonPropertyName("step")] public int? Step { get; init; }
    [JsonPropertyName("patches")] public List<MetaPostGenNumberPatch>? Patches { get; init; }
}

public sealed class MetaPostGenGameOptions
{
    [JsonPropertyName("options")] public List<MetaPostGenSetting> Options { get; init; } = new();
}
