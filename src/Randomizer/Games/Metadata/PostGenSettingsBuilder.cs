namespace Randomizer.Games.Metadata;

using System.Reflection;
using Randomizer.Games.Metadata;

public static class PostGenSettingsBuilder
{
    public static Dictionary<string, MetaPostGenGameOptions> Build()
    {
        var result = new Dictionary<string, MetaPostGenGameOptions>(StringComparer.OrdinalIgnoreCase);

        var asm = typeof(PostGenSettingsBuilder).Assembly;
        foreach (var type in asm.GetTypes())
        {
            var pgAttr = type.GetCustomAttribute<PostGenSettingsForAttribute>();
            if (pgAttr is null)
                continue;

            // Ensure it is tied to a known target game for clarity/consistency
            if (type.GetCustomAttribute<TargetGameAttribute>() is null)
                continue;

            var gameKey = pgAttr.GameId;
            var instance = Activator.CreateInstance(type);
            if (instance is null)
                continue;

            var gameOptions = new MetaPostGenGameOptions();
            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var setting = BuildSetting(prop, instance);
                if (setting is not null)
                    gameOptions.Options.Add(setting);
            }

            result[gameKey] = gameOptions;
        }

        return result;
    }

    private static MetaPostGenSetting? BuildSetting(PropertyInfo prop, object instance)
    {
        var id = prop.GetCustomAttribute<PostGenIdAttribute>()?.Id;
        id ??= Slug(prop.Name); // auto-generate id from property name

        var name = prop.GetCustomAttribute<NameAttribute>()?.Name ?? Title(prop.Name);
        var description = prop.GetCustomAttribute<DescriptionAttribute>()?.Description;

        if (prop.PropertyType == typeof(bool))
        {
            var def = (bool?)prop.GetValue(instance) ?? false;
            var onPatches = prop.GetCustomAttributes<OnPatchAttribute>().Select(a => new MetaPostGenPatch { TargetAddress = a.TargetAddress, Data = a.Data }).ToList();
            var offPatches = prop.GetCustomAttributes<OffPatchAttribute>().Select(a => new MetaPostGenPatch { TargetAddress = a.TargetAddress, Data = a.Data }).ToList();

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
                var patches = field.GetCustomAttributes<ChoicePatchAttribute>()
                    .Select(a => new MetaPostGenPatch { TargetAddress = a.TargetAddress, Data = a.Data })
                    .ToList();

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
