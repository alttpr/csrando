namespace Randomizer.Games;

using System.Reflection;
using Randomizer.Games.Metadata;

/// <summary>
/// Request-time validation of game settings against their own metadata attributes —
/// the same [Values] and [OnlyWithGames] rules the web UI enforces, applied here for
/// direct API and preset payloads. Deterministic config errors must be rejected
/// before the generation retry loop, where they would fail identically on every
/// attempt.
/// </summary>
public static class WorldConfigValidator
{
    /// <summary>Returns an error message for the first invalid setting, or null.</summary>
    public static string? Validate(WorldConfig[] configs)
    {
        foreach (var config in configs)
        {
            if (ValidateConfig(config) is { } error)
                return error;
        }
        return null;
    }

    private static string? ValidateConfig(WorldConfig config)
    {
        var selectedGames = GameConfigProperties.Value
            .Where(entry => entry.Property.GetValue(config) != null)
            .ToArray();

        foreach (var (gameProperty, game) in selectedGames)
        {
            object gameConfig = gameProperty.GetValue(config)!;
            object defaults = Defaults(gameProperty.PropertyType);

            foreach (var setting in SettingProperties(gameProperty.PropertyType))
            {
                if (UnknownValueError(game, gameConfig, setting) is { } unknownValue)
                    return unknownValue;

                var onlyWith = setting.GetCustomAttribute<OnlyWithGamesAttribute>();
                if (onlyWith == null
                    || Equals(setting.GetValue(gameConfig), setting.GetValue(defaults))
                    || !IsActive(gameConfig, setting)
                    || selectedGames.All(selected => onlyWith.Games.Contains(selected.Game)))
                {
                    continue;
                }

                return $"{DisplayName(game)} {DisplayName(setting)} requires a seed with only "
                    + $"{string.Join(", ", onlyWith.Games.Select(DisplayName))}; "
                    + "reset the setting or remove the other games.";
            }
        }

        return ValidateCustomItemTiers(config);
    }

    /// <summary>
    /// The one rule metadata cannot express: custom item tier override lists must
    /// parse (see <see cref="ItemTiers.ParseOverrides"/>).
    /// </summary>
    private static string? ValidateCustomItemTiers(WorldConfig config)
    {
        foreach (var (game, mode, customList) in (ReadOnlySpan<(Game, TieredItemsSetting?, string?)>)
        [
            (Game.SuperMetroid, config.SuperMetroid?.TieredItems, config.SuperMetroid?.CustomItemTiers),
            (Game.Metroid, config.Metroid?.TieredItems, config.Metroid?.CustomItemTiers),
        ])
        {
            if (mode != TieredItemsSetting.Custom)
                continue;
            try
            {
                ItemTiers.ParseOverrides(customList);
            }
            catch (FormatException ex)
            {
                return $"{DisplayName(game)} Custom Item Tiers: {ex.Message}";
            }
        }
        return null;
    }

    private static string? UnknownValueError(Game game, object gameConfig, PropertyInfo setting)
    {
        if (setting.PropertyType != typeof(string) || setting.GetValue(gameConfig) is not string value)
            return null;

        var allowed = setting.GetCustomAttributes<ValuesAttribute>()
            .SelectMany(attribute => attribute.Values)
            .ToList();
        if (allowed.Count == 0 || allowed.Contains(value))
            return null;

        return $"Unknown {DisplayName(game)} {DisplayName(setting)} '{value}'.";
    }

    /// <summary>Whether the setting takes effect: its DependsOn chain (if any) is
    /// satisfied by the config's current values.</summary>
    private static bool IsActive(object gameConfig, PropertyInfo setting)
    {
        if (setting.GetCustomAttribute<DependsOnAttribute>() is not { } dependsOn)
            return true;

        var parent = setting.DeclaringType!.GetProperty(dependsOn.PropertyName)
            ?? throw new InvalidOperationException(
                $"{setting.DeclaringType.Name}.{setting.Name} depends on unknown property '{dependsOn.PropertyName}'");
        object? parentValue = parent.GetValue(gameConfig);
        return dependsOn.Values.Any(value => Equals(value, parentValue)) && IsActive(gameConfig, parent);
    }

    /// <summary>The game-config properties of <see cref="WorldConfig"/> (the Combo
    /// wrapper is not a selectable game and carries no settings of its own).</summary>
    private static readonly Lazy<(PropertyInfo Property, Game Game)[]> GameConfigProperties = new(() =>
        typeof(WorldConfig).GetProperties()
            .Select(p => (Property: p, Game: p.PropertyType.GetCustomAttribute<TargetGameAttribute>()?.Game))
            .Where(p => p.Game is not null and not Game.Combo)
            .Select(p => (p.Property, p.Game!.Value))
            .ToArray());

    private static IEnumerable<PropertyInfo> SettingProperties(Type configType) =>
        configType.GetProperties().Where(p => p.GetCustomAttribute<IgnoreAttribute>() == null);

    private static readonly Dictionary<Type, object> _defaults = [];

    private static object Defaults(Type configType)
    {
        lock (_defaults)
        {
            if (!_defaults.TryGetValue(configType, out object? defaults))
                _defaults[configType] = defaults = Activator.CreateInstance(configType)!;
            return defaults;
        }
    }

    private static string DisplayName(Game game) =>
        game.GetCustomAttribute<DescriptionAttribute>()?.Description ?? game.ToString();

    private static string DisplayName(PropertyInfo setting) =>
        setting.GetCustomAttribute<NameAttribute>()?.Name ?? setting.Name;
}
