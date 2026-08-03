namespace Randomizer.Games.Metadata;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Randomizer.Games;

public static class PostGenSettingsBuilder
{
    public static Dictionary<string, MetaPostGenGameOptions> Build(RandomizerTarget randomizer)
    {
        var result = new Dictionary<string, MetaPostGenGameOptions>(StringComparer.OrdinalIgnoreCase);

        var asm = typeof(PostGenSettingsBuilder).Assembly;
        foreach (var type in asm.GetTypes())
        {
            var pgAttributes = type.GetCustomAttributes<PostGenSettingsForAttribute>().ToArray();
            if (pgAttributes.Length == 0)
                continue;

            var targetGameAttr = type.GetCustomAttribute<TargetGameAttribute>();

            var instance = Activator.CreateInstance(type);
            if (instance is null)
                continue;

            foreach (var pgAttr in pgAttributes)
            {
                var context = BuildContext(pgAttr, targetGameAttr);
                if (context.Target.HasValue && context.Target.Value != randomizer)
                    continue;

                var gameOptions = new MetaPostGenGameOptions();
                foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var setting = BuildSetting(prop, instance, context);
                    if (setting is not null)
                        gameOptions.Options.Add(setting);
                }

                result[context.GameId] = gameOptions;
            }
        }

        return result;
    }

    private static PostGenBuildContext BuildContext(PostGenSettingsForAttribute attr, TargetGameAttribute? targetGameAttr)
    {
        RandomizerTarget? target = attr.HasTarget
            ? attr.Target
            : targetGameAttr?.Randomizer;
        return new PostGenBuildContext(attr.GameId, target, attr.AddressOffset);
    }

    private static MetaPostGenSetting? BuildSetting(PropertyInfo prop, object instance, PostGenBuildContext context)
    {
        var id = prop.GetCustomAttribute<PostGenIdAttribute>()?.Id;
        id ??= Slug(prop.Name); // auto-generate id from property name

        var name = prop.GetCustomAttribute<NameAttribute>()?.Name ?? Title(prop.Name);
        var description = prop.GetCustomAttribute<DescriptionAttribute>()?.Description;

        if (prop.PropertyType == typeof(bool))
        {
            var def = (bool?)prop.GetValue(instance) ?? false;
            var onPatches = BuildPatches(prop.GetCustomAttributes<OnPatchAttribute>(), context);
            var offPatches = BuildPatches(prop.GetCustomAttributes<OffPatchAttribute>(), context);

            return new MetaPostGenSetting
            {
                Type = "toggle",
                Id = id!,
                Name = name,
                Description = description,
                Default = def,
                On = onPatches.Count > 0 ? new MetaPostGenPatchGroup { Patches = onPatches } : null,
                Off = offPatches.Count > 0 ? new MetaPostGenPatchGroup { Patches = offPatches } : null,
            };
        }

        if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(byte))
        {
            // Only numeric properties that declare where their value goes become settings.
            var patches = BuildNumberPatches(prop.GetCustomAttributes<NumberPatchAttribute>(), context);
            if (patches.Count == 0)
                return null;

            // Without an explicit range a numeric setting covers a single byte; multi-byte
            // patches must declare their own. ValueRangeAttribute.Default is deliberately not
            // consulted: like the toggle and select branches, the property initializer is the
            // source of truth for the default.
            var range = prop.GetCustomAttribute<ValueRangeAttribute>();
            var min = range?.MinInclusive ?? 0;
            var max = range?.MaxInclusive ?? 0xFF;
            var def = Math.Clamp(Convert.ToInt32(prop.GetValue(instance) ?? min), min, max);

            return new MetaPostGenSetting
            {
                Type = "number",
                Id = id!,
                Name = name,
                Description = description,
                Default = def,
                Min = min,
                Max = max,
                Step = 1,
                Patches = patches,
            };
        }

        if (prop.PropertyType.IsEnum)
        {
            var defValue = prop.GetValue(instance);
            var defName = defValue?.ToString() ?? string.Empty;
            var defField = prop.PropertyType.GetField(defName);
            // default choice value id
            var defChoiceValue = defField?.GetCustomAttribute<ChoiceAttribute>()?.Value ?? defName.ToLowerInvariant();

            var choices = new List<MetaPostGenSelectChoice>();
            foreach (var field in prop.PropertyType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var choiceAttr = field.GetCustomAttribute<ChoiceAttribute>();
                var valueId = choiceAttr?.Value ?? field.Name.ToLowerInvariant();
                var label = choiceAttr?.Label ?? Title(field.Name);
                var patches = BuildPatches(field.GetCustomAttributes<ChoicePatchAttribute>(), context);

                choices.Add(new MetaPostGenSelectChoice
                {
                    Value = valueId,
                    Label = label,
                    Patches = patches,
                });
            }

            return new MetaPostGenSetting
            {
                Type = "select",
                Id = id!,
                Name = name,
                Description = description,
                Default = defChoiceValue,
                Choices = choices,
            };
        }

        return null;
    }

    private static List<MetaPostGenPatch> BuildPatches(IEnumerable<PatchAttributeBase> attributes, PostGenBuildContext context)
    {
        var attrList = attributes.ToList();
        if (attrList.Count == 0)
            return new List<MetaPostGenPatch>();

        var patches = new List<MetaPostGenPatch>(attrList.Count);
        var target = context.Target;

        var replacements = target is null
            ? new HashSet<int>()
            : attrList
                .Where(a => a.AppliesTo == target && a.ReplacesAddress.HasValue)
                .Select(a => a.ReplacesAddress!.Value)
                .ToHashSet();

        foreach (var attr in attrList)
        {
            if (!ShouldEmit(attr, target, replacements))
                continue;

            var address = ResolveAddress(attr, context.AddressOffset);
            patches.Add(new MetaPostGenPatch
            {
                TargetAddress = address,
                Data = attr.Data.Select(b => (int)b).ToList(),
            });
        }

        return patches;
    }

    private static List<MetaPostGenNumberPatch> BuildNumberPatches(
        IEnumerable<NumberPatchAttribute> attributes,
        PostGenBuildContext context)
    {
        var attrList = attributes.ToList();
        if (attrList.Count == 0)
            return new List<MetaPostGenNumberPatch>();

        var target = context.Target;

        // Same target-override handling as BuildPatches: a target-specific patch suppresses the
        // target-agnostic one it replaces.
        var replacements = target is null
            ? new HashSet<int>()
            : attrList
                .Where(a => a.AppliesTo == target && a.ReplacesAddress.HasValue)
                .Select(a => a.ReplacesAddress!.Value)
                .ToHashSet();

        var patches = new List<MetaPostGenNumberPatch>(attrList.Count);
        foreach (var attr in attrList)
        {
            if (!ShouldEmit(attr, target, replacements))
                continue;

            // The client schema only accepts 1-4 byte widths; a wider declaration would fail
            // metadata validation in the browser instead of failing here, so reject it now.
            if (attr.Length is < 1 or > 4)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(NumberPatchAttribute.Length),
                    attr.Length,
                    "Numeric post-generation patches must be 1-4 bytes wide.");
            }

            patches.Add(new MetaPostGenNumberPatch
            {
                TargetAddress = ResolveAddress(attr, context.AddressOffset),
                Length = attr.Length,
            });
        }

        return patches;
    }

    private static bool ShouldEmit(
        PatchAttributeBase attr,
        RandomizerTarget? target,
        IReadOnlySet<int> replacements)
    {
        if (attr.AppliesTo.HasValue)
            return target.HasValue && attr.AppliesTo.Value == target.Value;

        if (target.HasValue && attr.ExcludeTargets.Contains(target.Value))
            return false;

        if (target.HasValue && replacements.Contains(attr.TargetAddress))
            return false;

        return true;
    }

    private static int ResolveAddress(PatchAttributeBase attr, int defaultOffset)
    {
        var offset = attr.SkipDefaultOffset ? 0 : defaultOffset;
        if (attr.AdditionalOffset != 0)
            offset += attr.AdditionalOffset;

        return attr.TargetAddress + offset;
    }

    private static string Title(string pascal)
    {
        if (string.IsNullOrWhiteSpace(pascal))
            return pascal;
        // Insert spaces between lower-to-upper transitions and before digits
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < pascal.Length; i++)
        {
            char c = pascal[i];
            if (i > 0 && ((char.IsLetterOrDigit(c) && char.IsUpper(c) && char.IsLower(pascal[i - 1])) || (char.IsDigit(c) && char.IsLetter(pascal[i - 1]))))
                result.Append(' ');
            result.Append(c);
        }
        return result.ToString();
    }

    private static string Slug(string pascal)
    {
        if (string.IsNullOrWhiteSpace(pascal))
            return pascal;
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < pascal.Length; i++)
        {
            char c = pascal[i];
            if (i > 0 && (
                (char.IsLetterOrDigit(c) && char.IsUpper(c) && char.IsLower(pascal[i - 1])) ||
                (char.IsDigit(c) && char.IsLetter(pascal[i - 1])) ||
                (char.IsUpper(c) && char.IsDigit(pascal[i - 1]))
            ))
                result.Append('-');
            result.Append(char.ToLowerInvariant(c));
        }
        return result.ToString();
    }
}

internal sealed record PostGenBuildContext(string GameId, RandomizerTarget? Target, int AddressOffset);
