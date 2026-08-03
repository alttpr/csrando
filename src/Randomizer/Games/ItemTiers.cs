namespace Randomizer.Games;

using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Games.Combo;
using Randomizer.Graph;

public enum TieredItemsSetting
{
    Off,
    On,
    Custom,
}

/// <summary>
/// Resolves placed items to <see cref="ItemTier"/>s for a game's tiered-items
/// setting. The standard classification lives in each game's item data (the
/// item's own <see cref="IItem.Tier"/>); this class adds the user-facing modes
/// and the custom override syntax. Knows nothing about any particular consumer;
/// each consumer (map icons today) translates tiers into its own representation.
/// </summary>
public static class ItemTiers
{
    /// <summary>
    /// Parses a <c>tier:</c> declaration from a game's item data. Absent means
    /// <see cref="ItemTier.Major"/>; anything unparseable throws so data typos
    /// surface loudly instead of silently classifying as Major.
    /// </summary>
    public static ItemTier ParseDeclaration(string? tierName) =>
        string.IsNullOrWhiteSpace(tierName) ? ItemTier.Major
            : Enum.TryParse<ItemTier>(tierName, ignoreCase: true, out var tier) ? tier
            : throw new FormatException($"unknown item tier '{tierName}'");

    /// <summary>
    /// Parses a user-supplied override list of <c>game:ItemName=tier</c> entries
    /// separated by commas, semicolons or newlines. Game, item name and tier are
    /// matched case-insensitively; item names are canonicalized against the combo
    /// item catalog. Throws a <see cref="FormatException"/> naming every bad entry.
    /// </summary>
    public static Dictionary<(string Game, string Item), ItemTier> ParseOverrides(string? list)
    {
        var overrides = new Dictionary<(string, string), ItemTier>();
        if (string.IsNullOrWhiteSpace(list))
            return overrides;

        var errors = new List<string>();
        foreach (string rawEntry in list.Split([',', ';', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries))
        {
            string entry = rawEntry.Trim();
            if (entry.Length == 0)
                continue;

            string[] assignment = entry.Split('=', 2);
            string[] qualified = assignment[0].Split(':', 2);
            if (assignment.Length != 2 || qualified.Length != 2)
            {
                errors.Add($"'{entry}' is not of the form game:ItemName=tier");
                continue;
            }

            string game = qualified[0].Trim().ToLowerInvariant();
            string itemName = qualified[1].Trim();
            string tierName = assignment[1].Trim();

            if (!ItemMapper.KnownGameIds.Contains(game))
            {
                errors.Add($"'{entry}' names unknown game '{qualified[0].Trim()}' (expected one of: {string.Join(", ", ItemMapper.KnownGameIds)})");
                continue;
            }
            if (!Enum.TryParse<ItemTier>(tierName, ignoreCase: true, out var tier))
            {
                errors.Add($"'{entry}' names unknown tier '{tierName}' (expected one of: {string.Join(", ", Enum.GetNames<ItemTier>()).ToLowerInvariant()})");
                continue;
            }

            string? canonical = ItemMapper.KnownItemNames(game)
                .FirstOrDefault(name => name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
            if (canonical == null)
            {
                errors.Add($"'{entry}' names unknown {game} item '{itemName}'");
                continue;
            }

            overrides[(game, canonical)] = tier;
        }

        if (errors.Count > 0)
            throw new FormatException($"Invalid item tier override(s): {string.Join("; ", errors)}");
        return overrides;
    }

    /// <summary>
    /// Builds the tier lookup for a game's tiered-items setting, or null when the
    /// feature is off so consumers can skip their tier pass entirely.
    /// </summary>
    public static Func<IItem, ItemTier>? CreateResolver(TieredItemsSetting mode, string? customList)
    {
        switch (mode)
        {
            case TieredItemsSetting.Off:
                return null;
            case TieredItemsSetting.On:
                return item => item.Tier;
            default:
                // Custom starts from all-minor; only listed items stand out.
                var overrides = ParseOverrides(customList);
                return item => overrides.TryGetValue((item.World.GameId, item.Name), out var tier)
                    ? tier : ItemTier.Minor;
        }
    }
}
