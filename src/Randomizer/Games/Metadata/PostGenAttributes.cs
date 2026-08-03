namespace Randomizer.Games.Metadata;

using System;
using System.Collections.Generic;
using Randomizer.Games;

// Attributes to describe post-generation settings in a declarative, per-game way.

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class PostGenSettingsForAttribute : Attribute
{
    private RandomizerTarget _target;

    public string GameId { get; }

    /// <summary>
    /// Optional randomizer target this metadata entry applies to. If not provided, the
    /// <see cref="TargetGameAttribute"/> on the declaring type is used as the fallback.
    /// </summary>
    public RandomizerTarget Target
    {
        get => _target;
        set
        {
            _target = value;
            HasTarget = true;
        }
    }

    internal bool HasTarget { get; private set; }

    /// <summary>
    /// Default address offset (in bytes) applied to all patch addresses for the target
    /// unless overridden at the attribute level.
    /// </summary>
    public int AddressOffset { get; set; }

    public PostGenSettingsForAttribute(string gameId)
        => GameId = gameId;
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class PostGenIdAttribute : Attribute
{
    public string Id { get; }
    public PostGenIdAttribute(string id) => Id = id;
}

public abstract class PatchAttributeBase : Attribute
{
    protected PatchAttributeBase(int targetAddress, params int[] data)
    {
        TargetAddress = targetAddress;
        Data = NormalizeData(data);
    }

    protected PatchAttributeBase(RandomizerTarget appliesTo, int targetAddress, params int[] data)
        : this(targetAddress, data)
    {
        AppliesTo = appliesTo;
    }

    /// <summary>ROM address to apply the patch to (prior to offset adjustments).</summary>
    public int TargetAddress { get; }

    /// <summary>Raw patch data bytes.</summary>
    public IReadOnlyList<byte> Data { get; }

    /// <summary>
    /// If specified, the patch only applies when emitting metadata for the matching target.
    /// </summary>
    public RandomizerTarget? AppliesTo { get; }

    /// <summary>
    /// Skip the default offset for this patch. Useful when <see cref="TargetAddress"/> is
    /// already expressed as the final address for the selected target.
    /// </summary>
    public bool SkipDefaultOffset { get; set; }

    /// <summary>
    /// Additional offset (can be negative) applied on top of the default offset when
    /// computing the final patch address.
    /// </summary>
    public int AdditionalOffset { get; set; }

    /// <summary>
    /// Optional base address that this patch replaces for the selected target.
    /// When provided, any base patch with a matching address is ignored for that target.
    /// </summary>
    public int? ReplacesAddress { get; set; }

    /// <summary>
    /// Explicit list of targets that should not receive this patch. Only evaluated when the
    /// patch is target-agnostic (no <see cref="AppliesTo"/> set).
    /// </summary>
    public RandomizerTarget[] ExcludeTargets { get; set; } = Array.Empty<RandomizerTarget>();

    private static IReadOnlyList<byte> NormalizeData(int[] data)
    {
        if (data is null || data.Length == 0)
            return Array.Empty<byte>();

        var bytes = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            int value = data[i];
            if (value is < 0 or > 0xFF)
                throw new ArgumentOutOfRangeException(nameof(data), value, "Patch data bytes must be in the range 0-255.");
            bytes[i] = (byte)value;
        }

        return bytes;
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class OnPatchAttribute : PatchAttributeBase
{
    public OnPatchAttribute(int targetAddress, params int[] data)
        : base(targetAddress, data)
    {
    }

    public OnPatchAttribute(RandomizerTarget appliesTo, int targetAddress, params int[] data)
        : base(appliesTo, targetAddress, data)
    {
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class OffPatchAttribute : PatchAttributeBase
{
    public OffPatchAttribute(int targetAddress, params int[] data)
        : base(targetAddress, data)
    {
    }

    public OffPatchAttribute(RandomizerTarget appliesTo, int targetAddress, params int[] data)
        : base(appliesTo, targetAddress, data)
    {
    }
}

/// <summary>
/// Marks a numeric post-generation setting and the address its value is written to.
/// The selected value is written as <see cref="Length"/> little-endian bytes, so unlike the
/// toggle/select patches there is no fixed payload declared here.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class NumberPatchAttribute : PatchAttributeBase
{
    public NumberPatchAttribute(int targetAddress)
        : base(targetAddress)
    {
    }

    public NumberPatchAttribute(RandomizerTarget appliesTo, int targetAddress)
        : base(appliesTo, targetAddress)
    {
    }

    /// <summary>Number of little-endian bytes the value occupies at the target address.</summary>
    public int Length { get; set; } = 1;
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
public sealed class ChoicePatchAttribute : PatchAttributeBase
{
    public ChoicePatchAttribute(int targetAddress, params int[] data)
        : base(targetAddress, data)
    {
    }

    public ChoicePatchAttribute(RandomizerTarget appliesTo, int targetAddress, params int[] data)
        : base(appliesTo, targetAddress, data)
    {
    }
}
