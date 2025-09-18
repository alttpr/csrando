namespace Randomizer.Games.Metadata;

// Attributes to describe post-generation settings in a declarative, per-game way.

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class PostGenSettingsForAttribute : Attribute
{
    public string GameId { get; }
    public PostGenSettingsForAttribute(string gameId) => GameId = gameId;
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class PostGenIdAttribute : Attribute
{
    public string Id { get; }
    public PostGenIdAttribute(string id) => Id = id;
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class OnPatchAttribute : Attribute
{
    public string TargetAddress { get; }
    public string Data { get; }
    public OnPatchAttribute(string targetAddress, string data)
    {
        TargetAddress = targetAddress;
        Data = data;
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class OffPatchAttribute : Attribute
{
    public string TargetAddress { get; }
    public string Data { get; }
    public OffPatchAttribute(string targetAddress, string data)
    {
        TargetAddress = targetAddress;
        Data = data;
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class ChoiceAttribute : Attribute
{
    public string Value { get; }
    public string Label { get; }
    public ChoiceAttribute(string value, string label)
    {
        Value = value;
        Label = label;
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public sealed class ChoicePatchAttribute : Attribute
{
    public string TargetAddress { get; }
    public string Data { get; }
    public ChoicePatchAttribute(string targetAddress, string data)
    {
        TargetAddress = targetAddress;
        Data = data;
    }
}

