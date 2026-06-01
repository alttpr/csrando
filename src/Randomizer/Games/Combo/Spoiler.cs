namespace Randomizer.Games.Combo;

using System.Globalization;
using System.Text.RegularExpressions;
using Randomizer.Graph;
using BaseSpoilerLog = Graph.SpoilerLog;
using AlttpWorld = Alttp.World;
using MetroidWorld = Metroid.World;
using SuperMetroidWorld = SuperMetroid.World;
using Zelda1World = Zelda1.World;

internal static partial class Spoiler
{
    public static void Log(GameRandomizer randomizer, BaseSpoilerLog spoilerLog)
    {
        ArgumentNullException.ThrowIfNull(randomizer);
        ArgumentNullException.ThrowIfNull(spoilerLog);

        var spoiler = spoilerLog.Spoiler;

        if (randomizer.Worlds[0] is not World comboWorld)
            return;

        var includedGames = new List<string>();

        if (comboWorld.AlttpWorld is AlttpWorld alttpWorld)
        {
            includedGames.Add("A Link to the Past");
            Alttp.Spoiler.Log([alttpWorld], spoilerLog, "A Link to the Past");
        }

        if (comboWorld.SMWorld is SuperMetroidWorld superMetroidWorld)
        {
            includedGames.Add("Super Metroid");
            AppendLocationsForWorld(spoiler, superMetroidWorld, "Super Metroid");
            AppendStartingEquipment(spoiler, superMetroidWorld.Config.StartingEquipment, "Super Metroid");
        }

        if (comboWorld.Z1World is Zelda1World zelda1World)
        {
            includedGames.Add("The Legend of Zelda");
            AppendLocationsForWorld(spoiler, zelda1World, "The Legend of Zelda");
            AppendStartingEquipment(spoiler, zelda1World.Config.StartingEquipment, "The Legend of Zelda");
            if (zelda1World.DungeonSpoilers.Count > 0)
            {
                PopulateDungeonItemPlacements(zelda1World);
                Zelda1.Spoiler.AppendDungeonMaps(spoiler, zelda1World.DungeonSpoilers);
            }
        }

        if (comboWorld.M1World is MetroidWorld metroidWorld)
        {
            includedGames.Add("Metroid");
            AppendLocationsForWorld(spoiler, metroidWorld, "Metroid");
            AppendStartingEquipment(spoiler, metroidWorld.Config.StartingEquipment, "Metroid");
        }

        var metaSection = GetOrCreateSection(spoiler, "meta");
        metaSection.TryAdd("seed", randomizer.PRNG.Seed.ToString("X8", CultureInfo.InvariantCulture));
        AppendWorldMeta(metaSection, comboWorld, includedGames);

        PruneEmptySection(spoiler, "Equipped");
        PruneEmptySection(spoiler, "Locations");
    }

    private static void AppendLocationsForWorld(
        Dictionary<string, Dictionary<string, string>> spoiler,
        IWorld world,
        string gameLabel)
    {
        foreach (var location in world.GetLocationsOfType(VertexType.Item).OrderBy(l => l.Name, StringComparer.Ordinal))
        {
            var parts = location.Name.Split(" - ", 2, StringSplitOptions.TrimEntries);
            var group = parts.Length > 1 ? parts[0] : "Locations";
            var sectionKey = $"{gameLabel} - {group}";
            var section = GetOrCreateSection(spoiler, sectionKey);
            var gameId = location.Item?.World switch
            {
                AlttpWorld _ => "Z3",
                SuperMetroidWorld _ => "SM",
                Zelda1World _ => "Z1",
                MetroidWorld _ => "M1",
                _ => "Unknown"
            };
            section[location.Name] = (location.Item?.Name ?? "Nothing") + $" ({gameId})";
        }
    }

    private static void AppendStartingEquipment(
        Dictionary<string, Dictionary<string, string>> spoiler,
        IReadOnlyList<string> equipment,
        string gameLabel)
    {
        if (equipment is not { Count: > 0 })
            return;

        var section = GetOrCreateSection(spoiler, "Equipped");
        for (int i = 0; i < equipment.Count; i++)
        {
            section[$"{gameLabel} Slot {i + 1}"] = equipment[i];
        }
    }

    private static void AppendWorldMeta(
        Dictionary<string, string> metaSection,
        World comboWorld,
        IReadOnlyList<string> includedGames)
    {
        string prefix = $"world_";
        metaSection[$"{prefix}id"] = comboWorld.Id.ToString(CultureInfo.InvariantCulture);
        metaSection[$"{prefix}initial_game"] = comboWorld.Config.InitialGame;
        metaSection[$"{prefix}game_count"] = includedGames.Count.ToString(CultureInfo.InvariantCulture);
        if (includedGames.Count > 0)
        {
            metaSection[$"{prefix}games"] = string.Join(", ", includedGames);
        }
    }

    private static Dictionary<string, string> GetOrCreateSection(
        Dictionary<string, Dictionary<string, string>> spoiler,
        string key)
    {
        if (!spoiler.TryGetValue(key, out var section))
        {
            section = new Dictionary<string, string>();
            spoiler[key] = section;
        }

        return section;
    }

    private static void PopulateDungeonItemPlacements(Zelda1World zelda1World)
    {
        var dungeonsByLevel = zelda1World.DungeonSpoilers.ToDictionary(d => d.Level);
        foreach (var location in zelda1World.GetLocationsOfType(VertexType.Item))
        {
            if (location.Item is null) continue;
            var match = RoomCoordRegex().Match(location.Name);
            if (!match.Success) continue;

            int level = int.Parse(match.Groups[1].Value);
            int x = int.Parse(match.Groups[2].Value);
            int y = int.Parse(match.Groups[3].Value);

            if (dungeonsByLevel.TryGetValue(level, out var dungeon))
                dungeon.ItemPlacements[$"{x},{y}"] = location.Item.Name;
        }
    }

    [GeneratedRegex(@"Dungeon L(\d+) (?:R|Cellar)\((\d+),(\d+)\)")]
    private static partial Regex RoomCoordRegex();

    private static void PruneEmptySection(Dictionary<string, Dictionary<string, string>> spoiler, string key)
    {
        if (spoiler.TryGetValue(key, out var section) && section.Count == 0)
            spoiler.Remove(key);
    }
}
