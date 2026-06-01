namespace Randomizer.Games.Zelda1;

using System.Text.RegularExpressions;
using Randomizer.Games;
using Randomizer.Graph;
using Randomizer.RomModifications;
using BaseGameRandomizer = Randomizer.Graph.GameRandomizer;

public sealed partial class GameRandomizer : BaseGameRandomizer
{
    public GameRandomizer(WorldConfig[] randomizerConfigs, PRNG prng)
        : base(randomizerConfigs, prng)
    {
    }

    protected override IItemPooler CreateItemPooler(IWorld[] worlds, PRNG prng) => new ItemPooler(worlds, prng);

    protected override IWorld CreateWorld(int worldId, WorldConfig worldConfig, Graph graph, PRNG prng) => new World(worldId, worldConfig, graph, prng);

    public override void AppendSpoiler(SpoilerLog spoilerLog)
    {
        var spoiler = spoilerLog.Spoiler;

        foreach (var world in Worlds)
        {
            if (world is not World z1World) continue;

            foreach (var location in z1World.GetLocationsOfType(VertexType.Item).OrderBy(l => l.Name, StringComparer.Ordinal))
            {
                var parts = location.Name.Split(" - ", 2, StringSplitOptions.TrimEntries);
                var group = parts.Length > 1 ? parts[0] : "Locations";
                if (!spoiler.TryGetValue(group, out var section))
                {
                    section = new Dictionary<string, string>();
                    spoiler[group] = section;
                }
                section[location.Name] = location.Item?.Name ?? "Nothing";
            }

            if (z1World.DungeonSpoilers.Count > 0)
            {
                // Match placed items back to dungeon rooms by parsing vertex names
                // Vertex names: "Underworld - Level N - Dungeon LN R(x,y) - Item"
                //           or: "Underworld - Level N - Dungeon LN Cellar(x,y) - Passage - Item"
                var dungeonsByLevel = z1World.DungeonSpoilers.ToDictionary(d => d.Level);
                foreach (var location in z1World.GetLocationsOfType(VertexType.Item))
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

                Spoiler.AppendDungeonMaps(spoiler, z1World.DungeonSpoilers);
            }
        }
    }

    [GeneratedRegex(@"Dungeon L(\d+) (?:R|Cellar)\((\d+),(\d+)\)")]
    private static partial Regex RoomCoordRegex();

    protected override void WriteWorldToRom(IWorld world, IRom rom, PRNG prng)
    {
        if (world is not World z1World)
            throw new ArgumentException("Passed world is not for The Legend of Zelda.", nameof(world));

        RomWriter.Write(rom, z1World, prng);
    }

    protected override string CreateFileName(IWorld world, PRNG prng, string? worldSuffix)
        => $"z1r_{world.WorldConfig.Zelda1!.EntranceShuffle}_{prng.Seed:x08}{worldSuffix}.nes";
}
