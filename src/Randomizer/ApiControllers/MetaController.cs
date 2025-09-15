namespace Randomizer.ApiControllers;

using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Randomizer.Games;
using Randomizer.Games.Metadata;

[ApiController]
[Route("[controller]")]
public sealed partial class MetaController : ControllerBase
{
    [HttpGet]
    public IEnumerable<MetaRandomizer> Get() => Enum.GetValues<RandomizerTarget>().Select(GameRandomizer);
    private static MetaRandomizer GameRandomizer(RandomizerTarget randomizer) => new(Name(randomizer), Description(randomizer), randomizer);
    private static MetaTarget Game(Game? game, RandomizerTarget? randomizer) => new(Name((Enum?)game ?? randomizer), Description((Enum?)game ?? randomizer), game, randomizer);

    [HttpGet("{randomizer}")]
    public IResult Get(RandomizerTarget randomizer)
    {
        // TODO: all of this lends itself to a source generator that builds a static result once.
        var rootSettings = new List<MetaSetting>();
        var gameSettings = new Dictionary<string, MetaTargetSettings>();
        var postGenSettings = PostGenSettingsBuilder.Build();

        foreach (var rootProperty in SettingProperties(typeof(WorldConfig)))
        {
            if (rootProperty.PropertyType == typeof(RandomizerTarget))
            {
                rootSettings.Add(new(rootProperty.Name, Name(rootProperty), Description(rootProperty), Type(rootProperty), Values: new() { { Name(randomizer), randomizer } }, Default: randomizer));
            }
            else if (rootProperty.GetCustomAttributes<UsableWithAttribute>().Any(a => a.Randomizers.Contains(randomizer)))
            {
                gameSettings.Add(rootProperty.Name, GameSettings(rootProperty));
            }
            else if (rootProperty.GetCustomAttribute<UsableWithAttribute>() is null)
            {
                // top-level likely means "static settings", don't add a "Random" option for those.
                rootSettings.Add(Setting(rootProperty, noRandomValues: true));
            }
        }

        return Results.Ok(new MetaRootSettings(rootSettings, gameSettings, postGenSettings));
    }

    private static MetaSetting Setting(PropertyInfo property, bool noRandomValues = false)
    {
        var (possibleValues, defaultValue) = Values(property);
        var (range, rangeDefaultValue) = Range(property);
        var (visibility, remark) = Visibility(property);
        defaultValue ??= rangeDefaultValue;
        var type = Type(property);
        string? optionsFor = property.GetCustomAttribute<RandomizedOptionsForAttribute>()?.PropertyName;

        if (property.PropertyType.IsArray)
            defaultValue = null;
        if (!noRandomValues && optionsFor == null && !property.PropertyType.IsEnum && possibleValues is { Count: > 0 } && !possibleValues.ContainsKey("Random"))
            possibleValues.Add("Random", null);

        string? description = string.Join('\n', NonNull(Description(property), remark))?.Trim();
        if (string.IsNullOrWhiteSpace(description))
            description = null;

        //string? category = property.GetCustomAttribute<CategoryAttribute>()?.Category;

        MetaCategory? metaCategory = property.GetCustomAttribute<CategoryAttribute>() is { Category: string category, Display: CategoryDisplay display }
            ? new MetaCategory(category, display)
            : null;

        string? subcategory = property.GetCustomAttribute<SubcategoryAttribute>()?.Subcategory;

        MetaDependsOn? dependsOn = null;
        if (property.GetCustomAttribute<DependsOnAttribute>() is { } dependsOnAttr)
        {
            dependsOn = new MetaDependsOn(dependsOnAttr.PropertyName, dependsOnAttr.Values);
        }

        return new MetaSetting(property.Name, Name(property), description, type, range, possibleValues, defaultValue, visibility, optionsFor, metaCategory, subcategory, dependsOn);
    }
    private static IEnumerable<string> NonNull(params IEnumerable<string?> values)
    {
        foreach (string? value in values)
        {
            if (value is not null)
                yield return value;
        }
    }

    private static string Name(PropertyInfo property)
    {
        if (property.GetCustomAttribute<NameAttribute>() is { Name: string name })
            return name;

        return TitleCase(property.Name);
    }
    private static string Name(Enum? @enum)
    {
        if (@enum is null)
            return "(unknown)";
        if (@enum.GetCustomAttribute<NameAttribute>() is { Name: string name })
            return name;

        return TitleCase(Enum.GetName(@enum.GetType(), @enum) ?? @enum.ToString());
    }
    [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(pascalCaseText))]
    private static string? TitleCase(string? pascalCaseText)
    {
        if (string.IsNullOrWhiteSpace(pascalCaseText))
            return pascalCaseText;

        return TitleCaseMatch().Replace(pascalCaseText, "$1 $2");
    }
    // TODO: return a useful description, later also i18n
    private static string? Description(PropertyInfo property)
    {
        if (property.GetCustomAttribute<DescriptionAttribute>() is { Description: string description })
            return description;

        return null;
    }
    private static string? Description(Enum? @enum)
    {
        if (@enum?.GetCustomAttribute<DescriptionAttribute>() is { Description: string description })
            return description;

        return null;
    }
    private static MetaSettingsType Type(PropertyInfo property)
    {
        if (property.GetCustomAttribute<ValueRangeAttribute>() is not null)
            return MetaSettingsType.Slider;
        if (property.GetCustomAttribute<ValuesAttribute>() is not null)
            return property.PropertyType.IsArray ? MetaSettingsType.MultipleChoice : MetaSettingsType.SingleChoice;
        if (property.PropertyType.IsEnum)
            return property.PropertyType.IsArray ? MetaSettingsType.MultipleChoice : MetaSettingsType.SingleChoice;
        if (property.PropertyType == typeof(bool))
            return MetaSettingsType.Toggle;

        return MetaSettingsType.Input;
    }
    private static (Dictionary<string, object?>? PossibleValues, object? DefaultValue) Values(PropertyInfo property)
    {
        var possibleValues = new Dictionary<string, object?>();
        object? defaultValue = null;

        foreach (var attribute in property.GetCustomAttributes<ValuesAttribute>())
        {
            // TODO: give the values a nice name
            foreach (object value in attribute.Values)
                possibleValues[Convert.ToString(value) ?? string.Empty] = value;

            defaultValue ??= attribute.Default;
        }

        if (possibleValues.Count == 0 && property.PropertyType.IsEnum)
        {
            foreach (var enumValue in Enum.GetValues(property.PropertyType).Cast<Enum>())
                possibleValues[Name(enumValue)] = enumValue;
        }

        if (possibleValues.Count == 0)
            possibleValues = null;

        return (possibleValues, defaultValue);
    }
    private static (MetaSettingsRange? Range, int? DefaultValue) Range(PropertyInfo property)
    {
        if (property.GetCustomAttribute<ValueRangeAttribute>() is not { } range)
            return default;

        return (new(range.MinInclusive, range.MaxInclusive), range.Default);
    }
    private static (MetaSettingsVisibility Visibility, string? Remark) Visibility(PropertyInfo property)
    {
        if (property.GetCustomAttribute<ExperimentalAttribute>() is { } experimental)
            return (MetaSettingsVisibility.Experimental, experimental.Reason);
        if (property.GetCustomAttribute<AdvancedAttribute>() is { } advanced)
            return (MetaSettingsVisibility.Advanced, advanced.Reason);

        return (MetaSettingsVisibility.Basic, null);
    }
    private static MetaTargetSettings GameSettings(PropertyInfo gameProperty)
    {
        var (game, randomizer) = GameRandomizer(gameProperty.PropertyType);
        var metaGame = Game(game, randomizer);
        var settings = new List<MetaSetting>();

        foreach (var property in SettingProperties(gameProperty.PropertyType))
        {
            settings.Add(Setting(property));
        }

        return new(metaGame, settings);
    }

    private static (Game? Game, RandomizerTarget? Randomizer) GameRandomizer(Type configType)
        => configType.GetCustomAttribute<TargetGameAttribute>() is { Game: var game, Randomizer: var randomizer }
            ? (game, randomizer)
            : throw new InvalidOperationException($"{configType.FullName} requires a {nameof(TargetGameAttribute)} to indicate which game it is for.");

    private static IEnumerable<PropertyInfo> SettingProperties(Type type) => type.GetProperties().Where(p => p.GetCustomAttribute<IgnoreAttribute>() is null);

    [GeneratedRegex("([a-z])([A-Z0-9])")]
    private static partial Regex TitleCaseMatch();
}

public sealed record MetaTarget(string Name, string? Description, Game? Game, RandomizerTarget? Randomizer);
public sealed record MetaRandomizer(string Name, string? Description, RandomizerTarget Randomizer);

public sealed record MetaRootSettings(
    List<MetaSetting> Settings,
    Dictionary<string, MetaTargetSettings> TargetSettings,
    Dictionary<string, MetaPostGenGameOptions> PostGenSettings
);
public sealed record MetaTargetSettings(MetaTarget Target, List<MetaSetting> Settings);
public sealed record MetaDependsOn(string Key, object[] Values);
public sealed record MetaCategory(string Name, CategoryDisplay? Display);
public sealed record MetaSetting(
    string Key,
    string Name,
    string? Description,
    MetaSettingsType Type,
    MetaSettingsRange? Range = null,
    Dictionary<string, object?>? Values = null,
    object? Default = null,
    MetaSettingsVisibility Visibility = MetaSettingsVisibility.Basic,
    string? OptionsFor = null,
    MetaCategory? Category = null,
    string? Subcategory = null,
    MetaDependsOn? DependsOn = null
);
public enum MetaSettingsType { Input, SingleChoice, MultipleChoice, Toggle, Slider };
public sealed record MetaSettingsRange(int From, int To);
public enum MetaSettingsVisibility { Basic, Advanced, Experimental };
