namespace Randomizer.Graph;

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

        Spoiler = new Dictionary<string, Dictionary<string, string>>(){
            { "Equipped", new() },
            { "Locations", new() }
        };

        int i = 0;
        foreach (var item in world.Config.StartingEquipment)
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

        if (world.Config.EnemyShuffle != EnemyShuffleOption.None)
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

        if (world.Config.BossShuffle != BossShuffleOption.None)
        {
            Spoiler["Bosses"] = new Dictionary<string, string>(){
                { "Eastern Palace", world.GetLocation("Eastern Palace - Boss")?.Sprite?.Name ?? "Unknown" },
                { "Desert Palace", world.GetLocation("Desert Palace - Boss")?.Sprite?.Name ?? "Unknown" },
                { "Tower Of Hera", world.GetLocation("Tower Of Hera - Boss")?.Sprite?.Name ?? "Unknown" },
                { "Hyrule Castle", "Agahnim" },
                { "Palace Of Darkness", world.GetLocation("Palace of Darkness - Boss")?.Sprite?.Name ?? "Unknown" },
                { "Swamp Palace", world.GetLocation("Swamp Palace - Boss")?.Sprite?.Name ?? "Unknown" },
                { "Skull Woods", world.GetLocation("Skull Woods - Boss")?.Sprite?.Name ?? "Unknown" },
                { "Thieves Town", world.GetLocation("Thieves' Town - Boss")?.Sprite?.Name ?? "Unknown" },
                { "Ice Palace", world.GetLocation("Ice Palace - Boss")?.Sprite?.Name ?? "Unknown" },
                { "Misery Mire", world.GetLocation("Misery Mire - Boss")?.Sprite?.Name ?? "Unknown" },
                { "Turtle Rock", world.GetLocation("Turtle Rock - Boss")?.Sprite?.Name ?? "Unknown" },
                { "Ganon's Tower Basement", world.GetLocation("Ganon's Tower - Ice Armos")?.Sprite?.Name ?? "Unknown" },
                { "Ganon's Tower Middle", world.GetLocation("Ganon's Tower - Lanmolas")?.Sprite?.Name ?? "Unknown" },
                { "Ganon's Tower Top", world.GetLocation("Ganon's Tower - Moldorm")?.Sprite?.Name ?? "Unknown" },
                { "Ganon's Tower", "Agahnim 2" },
                { "Ganon", "Ganon" },
            };
        }

        Spoiler["meta"] = new Dictionary<string, string>() {
            { "accessibility", world.Config.Accessibility.ToString() },
            { "goal", world.Config.Goal.ToString() },
            { "mode", world.Config.State.ToString() },
            { "weapons", world.Config.Weapon.ToString() },
            { "world_id", world.Id.ToString() },
            { "crystals_ganon", world.Config.CrystalsGanon.ToString() },
            { "crystals_tower", world.Config.CrystalsTower.ToString() },
            { "size", "2" },
        };
    }

}
