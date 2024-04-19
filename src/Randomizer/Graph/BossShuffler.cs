namespace Randomizer.Graph;

using Microsoft.Extensions.Logging;

/// <summary>Modify the edges of the graph to place bosses.</summary>
internal sealed class BossShuffler : IWorldModifier
{
    private static readonly ILogger _logger = ClassLogger.Get();

    private const string BOSS_SHUFFLER_ITEM_CONDITION = "bossKillShufflerOverwrite";
    private static readonly Dictionary<string, string> VANILLA_BOSSES = new()
    {
        { "Eastern Palace - Boss Room", "DefeatArmosKnight" },
        { "Desert Palace - Boss Room", "DefeatLanmolas" },
        { "Tower Of Hera - Boss Room", "DefeatMoldorm" },
        { "Palace of Darkness - Boss Room", "DefeatHelmasaur" },
        { "Swamp Palace - Boss Room", "DefeatArrghus" },
        { "Skull Woods - Boss Room", "DefeatMothula" },
        // TODO: this one deviates because of bringing the maiden to the boss room.
        { "Thieves' Town - Boss Room - Blind Active", "DefeatBlind" },
        //{ "Thieves' Town - Boss Room", "DefeatBlind" },
        { "Ice Palace - Boss Room", "DefeatKholdstare" },
        { "Misery Mire - Boss Room", "DefeatVitreous" },
        { "Turtle Rock - Boss Room", "DefeatTrinexx" },
        { "Ganon's Tower - Ice Room", "DefeatArmosKnight" },
        { "Ganon's Tower - Lanmolas", "DefeatLanmolas" },
        { "Ganon's Tower - Moldorm - Kill Zone", "DefeatMoldorm" },
    };

    /// <summary>Swap Entrances based on world settings.</summary>
    public static void AdjustEdges(World world, PRNG prng)
    {
        var bossRooms = world.GetLocations()
            .Where(v => v.World == world
                     && v.Edges.Any(e => e.Condition.Item.Name == BOSS_SHUFFLER_ITEM_CONDITION))
            .ToList();

        // force Kholdstare for swordless to be in Ice Palace
        // we'd like to have him elsewhere, but that requires us to make Bombos work in the room and put a tile down first.
        if (world.Config.Weapon == WeaponOption.Swordless)
        {
            foreach (var bossRoom in bossRooms)
            {
                if (VANILLA_BOSSES[bossRoom.Name] == "DefeatKholdstare")
                    continue;

                bossRoom.Edges.RemoveAll(e => e.Condition.Item.Name == "DefeatKholdstare");
            }
        }

        // most restrictive first
        var bossLocations = bossRooms.OrderBy(v => v.Edges.Count(e => e.Condition.Item.Name.StartsWith("Defeat")));

        List<string> placeBosses;
        switch (world.Config.BossShuffle)
        {
            case BossShuffleOption.Random:
                placeBosses = new()
                {
                    "DefeatArmosKnight",
                    "DefeatLanmolas",
                    "DefeatMoldorm",
                    "DefeatHelmasaur",
                    "DefeatArrghus",
                    "DefeatMothula",
                    "DefeatBlind",
                    "DefeatKholdstare",
                    "DefeatVitreous",
                    "DefeatTrinexx",
                };
                foreach (var location in bossLocations)
                {
                    var bosses = placeBosses.Intersect(location.Edges.Select(e => e.Condition.Item.Name));
                    string boss = prng.Shuffle(bosses).First();
                    PlaceBossItemInLocation(boss, location, world);
                }
                break;
            case BossShuffleOption.Full: // 1 copy of each, +3 other copies
                placeBosses = new()
                {
                    "DefeatArmosKnight",
                    "DefeatLanmolas",
                    "DefeatMoldorm",
                    "DefeatHelmasaur",
                    "DefeatArrghus",
                    "DefeatMothula",
                    "DefeatBlind",
                    "DefeatKholdstare",
                    "DefeatVitreous",
                    "DefeatTrinexx",
                };
                placeBosses.AddRange(prng.Shuffle(placeBosses).Take(3));

                foreach (var location in bossLocations)
                {
                    var bosses = placeBosses.Intersect(location.Edges.Select(e => e.Condition.Item.Name));
                    string boss = prng.Shuffle(bosses).First();
                    placeBosses.Remove(boss);
                    PlaceBossItemInLocation(boss, location, world);
                }
                break;
            case BossShuffleOption.Simple: // 1:1
                placeBosses = new()
                {
                    "DefeatArmosKnight",
                    "DefeatLanmolas",
                    "DefeatMoldorm",
                    "DefeatHelmasaur",
                    "DefeatArrghus",
                    "DefeatMothula",
                    "DefeatBlind",
                    "DefeatKholdstare",
                    "DefeatVitreous",
                    "DefeatTrinexx",
                    "DefeatArmosKnight",
                    "DefeatLanmolas",
                    "DefeatMoldorm",
                };

                foreach (var location in bossLocations)
                {
                    var bosses = placeBosses.Intersect(location.Edges.Select(e => e.Condition.Item.Name));
                    string boss = prng.Shuffle(bosses).First();
                    placeBosses.Remove(boss);
                    PlaceBossItemInLocation(boss, location, world);
                }
                break;
            case BossShuffleOption.None:
            default:
                foreach (var location in bossLocations)
                {
                    string boss = VANILLA_BOSSES[location.Name];
                    PlaceBossItemInLocation(boss, location, world);
                }
                break;
        }
    }

    /// <summary>Place Boss item in location.</summary>
    /// <param name="bossItem">Boss item name</param>
    /// <param name="location">Location name</param>
    /// <param name="world">World</param>
    /// <exception cref="Exception">If can't place boss in location</exception>
    private static void PlaceBossItemInLocation(string bossItem, Vertex from, World world)
    {
        if (from is null)
            throw new Exception("Can't place boss.");

        var worldBossItem = world.GetItem(bossItem);
        var bossEdge = from.Edges.Find(e => e.Condition.Item.Name == BOSS_SHUFFLER_ITEM_CONDITION);
        if (bossEdge is null)
            throw new Exception($"Can't place boss in {from.Name}, missing the boss connection with condition {BOSS_SHUFFLER_ITEM_CONDITION}.");

        from.Edges.RemoveAll(isDifferentBoss);
        bossEdge.Condition = new ItemCondition(worldBossItem, 1);
        _logger.LogInformation("[BS] Placing {Boss} in '{Location}'", bossItem.Replace("Defeat", ""), from.Name);

        bool isDifferentBoss(Edge edge)
        {
            if (edge.Condition.IsUnconditional)
                return false;
            if (edge.Condition.Item.Name == bossItem)
                return false;

            return edge.Condition.Item.Name.StartsWith("Defeat");
        }
    }
}
