namespace Randomizer.Games.Alttp;

using Randomizer.Graph;
using BaseSpoilerLog = Graph.SpoilerLog;

internal static class Spoiler
{
    public static void Log(IWorld[] worlds, BaseSpoilerLog spoilerLog, string groupPrefix = "")
    {
        if (worlds[0] is not World world)
            return;
        if (world.WorldConfig.Alttp is not { } config)
            return;

        var spoiler = spoilerLog.Spoiler;
        string g = groupPrefix == "" ? "" : $"{groupPrefix} - ";

        foreach (var (index, item) in config.StartingEquipment.Indexed())
            spoiler[g + "Equipped"][$"Equipment Slot {index}"] = item;

        foreach (var location in world.GetLocationsOfType(VertexType.Item))
        {
            var parts = location.Name.Split(" - ", 2);
            var group = g + (parts.Length > 1 ? parts[0] : "Locations");
            spoiler.TryAdd(group, []);
            spoiler[group][location.Name] = location.Item?.Name ?? "Nothing";
        }

        if (config.EnemyShuffle != EnemyShuffleOption.None)
        {
            foreach (var enemy in world.GetLocationsOfType(VertexType.Mob))
            {
                var parts = enemy.Name.Split(" - ", 2);
                var group = g + (parts.Length > 1 ? parts[0] : "Enemies");
                spoiler.TryAdd(group, []);
                spoiler[group][enemy.Name] = enemy.Sprite?.Name ?? "Nothing";
            }
        }

        // TODO: implement shops

        if (config.BossShuffle != BossShuffleOption.None)
        {
            spoiler[g + "Bosses"] = new Dictionary<string, string>()
            {
                { "Eastern Palace", GetBossAt(world, "Eastern Palace - Boss Room") },
                { "Desert Palace", GetBossAt(world, "Desert Palace - Boss Room") },
                { "Tower Of Hera", GetBossAt(world, "Tower Of Hera - Boss Room") },
                { "Hyrule Castle", "Agahnim" },
                { "Palace Of Darkness", GetBossAt(world, "Palace of Darkness - Boss Room") },
                { "Swamp Palace", GetBossAt(world, "Swamp Palace - Boss Room") },
                { "Skull Woods", GetBossAt(world, "Skull Woods - Boss Room") },
                // TODO: this one deviates because of bringing the maiden to the boss room.
                { "Thieves Town", GetBossAt(world, "Thieves' Town - Boss Room - Blind Active") },
                //{ "Thieves Town", GetBossAt(world, "Thieves' Town - Boss Room") },
                { "Ice Palace", GetBossAt(world, "Ice Palace - Boss Room") },
                { "Misery Mire", GetBossAt(world, "Misery Mire - Boss Room") },
                { "Turtle Rock", GetBossAt(world, "Turtle Rock - Boss Room") },
                { "Ganon's Tower Basement", GetBossAt(world, "Ganon's Tower - Ice Room") },
                { "Ganon's Tower Middle", GetBossAt(world, "Ganon's Tower - Lanmolas") },
                { "Ganon's Tower Top", GetBossAt(world, "Ganon's Tower - Moldorm - Kill Zone") },
                { "Ganon's Tower", "Agahnim 2" },
                { "Ganon", "Ganon" },
            };
        }

        if (config.EntranceShuffle != EntranceShuffleOption.None)
        {
            var connections = new SortedDictionary<string /* From */, string /* To */>();
            foreach (var entrance in world.GetLocationsOfType(VertexType.Entrance))
            {
                if (entrance.Edges is not [{ To: { } target }])
                {
                    Console.WriteLine("Entrance {0} doesn't have a single target ({1} edges: {2})",
                        entrance.Name,
                        entrance.Edges.Count,
                        string.Join(", ", entrance.Edges.Select(e => e.To.Name)));
                    continue;
                }
                if (target.Type is VertexType.Outlet or VertexType.Meta && target.Edges is [{ To: { } outletTarget }])
                    target = outletTarget;
                connections[entrance.Name] = target.Name;
            }
            foreach (var hole in world.GetLocationsOfType(VertexType.Hole))
            {
                if (hole.Edges is not [{ To: { } target }])
                {
                    Console.WriteLine("Hole {0} doesn't have a single target ({1} edges: {2})",
                        hole.Name,
                        hole.Edges.Count,
                        string.Join(", ", hole.Edges.Select(e => e.To.Name)));
                    continue;
                }
                connections[hole.Name] = target.Name;
            }
            spoiler[g + "Entrances"] = connections.ToDictionary();
        }

        spoiler["meta"] = new Dictionary<string, string>()
        {
            { "accessibility", config.Accessibility.ToString() },
            { "goal", config.Goal.ToString() },
            { "mode", config.State.ToString() },
            { "weapons", config.Weapon.ToString() },
            { "world_id", world.Id.ToString() },
            { "crystals_ganon", config.CrystalsGanon.ToString() },
            { "crystals_tower", config.CrystalsTower.ToString() },
            { "size", "2" },
        };
    }

    private static string GetBossAt(World world, string locationName)
    {
        var location = world.GetLocation(locationName);
        if (location == null)
            return "Unknown Location";

        var defeatCondition = location.Edges.FirstOrDefault(e => e.To.Type == VertexType.Boss);
        if (defeatCondition == null)
            return "Unknown";

        return defeatCondition.Condition.Item.Name.Replace("DarkDefeat", "").Replace("Defeat", "");
    }
}
