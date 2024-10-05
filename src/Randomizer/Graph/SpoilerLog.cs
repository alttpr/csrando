namespace Randomizer.Graph;

using global::Randomizer.Games.Alttp;

/// <summary>
/// Generates a Spoiler log for the randomizer.
/// </summary>
public class SpoilerLog
{
    private readonly Graph _graph;
    public Dictionary<string, Dictionary<string, string>> Spoiler { get; }

    public SpoilerLog(Randomizer randomizer)
    {
        _graph = randomizer.Graph;
        var world = randomizer.Worlds[0];
        // FIXME: this is currently game-specific
        var config = world.WorldConfig.Alttp!;

        Spoiler = new Dictionary<string, Dictionary<string, string>>()
        {
            { "Equipped", new() },
            { "Locations", new() }
        };

        int i = 0;
        foreach (var item in config.StartingEquipment)
        {
            Spoiler["Equipped"][$"Equipment Slot {++i}"] = item;
        }

        foreach (var location in world.GetLocationsOfType(VertexType.Item))
        {
            var parts = location.Name.Split(" - ", 2);
            var group = parts.Length > 1 ? parts[0] : "Locations";
            Spoiler.TryAdd(group, []);
            Spoiler[group][location.Name] = location.Item?.Name ?? "Nothing";
        }

        if (config.EnemyShuffle != EnemyShuffleOption.None)
        {
            foreach (var enemy in world.GetLocationsOfType(VertexType.Mob))
            {
                var parts = enemy.Name.Split(" - ", 2);
                var group = parts.Length > 1 ? parts[0] : "Enemies";
                Spoiler.TryAdd(group, []);
                Spoiler[group][enemy.Name] = enemy.Sprite?.Name ?? "Nothing";
            }
        }

        // @todo implement shops

        if (config.BossShuffle != BossShuffleOption.None)
        {
            Spoiler["Bosses"] = new Dictionary<string, string>()
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

        Spoiler["meta"] = new Dictionary<string, string>()
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

    private static string GetBossAt(IWorld world, string locationName)
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
