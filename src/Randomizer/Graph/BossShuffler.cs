namespace Randomizer.Graph;

/// <summary>Modify the edges of the graph to place bosses.</summary>
internal sealed class BossShuffler : IWorldModifier
{
    private static readonly Dictionary<string, string> BOSS_ITEMS = new()
    {
        { "ArmosKnight", "DefeatArmosKnight" },
        { "Lanmola", "DefeatLanmolas" },
        { "Moldorm", "DefeatMoldorm" },
        { "Agahnim", "DefeatAgahnim" },
        { "Helmasaur", "DefeatHelmasaur" },
        { "Arrghus", "DefeatArrghus" },
        { "Mothula", "DefeatMothula" },
        { "Blind", "DefeatBlind" },
        { "Kholdstare", "DefeatKholdstare" },
        { "Vitreous", "DefeatVitreous" },
        { "Trinexx", "DefeatTrinexx" },
        { "Agahnim2", "DefeatAgahnim2" },
        { "Ganon", "DefeatGanon" },
    };
    private static readonly Dictionary<string, string> BOSS_FROM_LOCATION = new()
    {
        { "Ganon's Tower - Moldorm", "Ganon's Tower - Moldorm - Kill Zone" },
        { "Ganon's Tower - Lanmolas", "Ganon's Tower - Gauntlet Refill" },
        { "Tower Of Hera - Boss", "Tower Of Hera - Boss Room" },
        { "Skull Woods - Boss", "Skull Woods - Boss Room" },
        { "Eastern Palace - Boss", "Eastern Palace - Boss Room" },
        { "Desert Palace - Boss", "Desert Palace - Boss Room" },
        { "Palace of Darkness - Boss", "Palace of Darkness - Boss Room" },
        { "Swamp Palace - Boss", "Swamp Palace - Boss Room" },
        { "Thieves' Town - Boss", "Thieves' Town - Boss Room" },
        { "Ice Palace - Boss", "Ice Palace - Boss Room" },
        { "Misery Mire - Boss", "Misery Mire - Boss Room" },
        { "Turtle Rock - Boss", "Turtle Rock - Boss Room" },
        { "Ganon's Tower - Ice Armos", "Ganon's Tower - Ice Room" },
    };

    /// <summary>Swap Entrances based on world settings.</summary>
    public static void AdjustEdges(World world, PRNG prng)
    {
        // most restrictive first
        var bossLocations = new List<string>()
        {
            "Ganon's Tower - Moldorm",
            "Ganon's Tower - Lanmolas",
            "Tower Of Hera - Boss",
            "Skull Woods - Boss",
            "Eastern Palace - Boss",
            "Desert Palace - Boss",
            "Palace of Darkness - Boss",
            "Swamp Palace - Boss",
            "Thieves' Town - Boss",
            "Ice Palace - Boss",
            "Misery Mire - Boss",
            "Turtle Rock - Boss",
            "Ganon's Tower - Ice Armos",
        };

        // force Kholdstare for swordless to be in Ice Palace
        if (world.Config.Weapon == WeaponOption.Swordless)
        {
            // remove Ice Palace
            bossLocations.RemoveAt(9);
            PlaceBossItemInLocation("DefeatKholdstare", "Ice Palace - Boss", world);
        }

        List<string> placeBosses;
        switch (world.Config.BossShuffle)
        {
            case BossShuffleOption.Random:
                foreach (string location in bossLocations)
                {
                    var bosses = BOSS_ITEMS.Values;
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

                foreach (string location in bossLocations)
                {
                    var bosses = placeBosses;
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

                foreach (string location in bossLocations)
                {
                    var bosses = placeBosses;
                    string boss = prng.Shuffle(bosses).First();
                    placeBosses.Remove(boss);
                    PlaceBossItemInLocation(boss, location, world);
                }
                break;
            case BossShuffleOption.None:
            default:
                PlaceBossItemInLocation("DefeatArmosKnight", "Eastern Palace - Boss", world);
                PlaceBossItemInLocation("DefeatLanmolas", "Desert Palace - Boss", world);
                PlaceBossItemInLocation("DefeatMoldorm", "Tower Of Hera - Boss", world);
                PlaceBossItemInLocation("DefeatHelmasaur", "Palace of Darkness - Boss", world);
                PlaceBossItemInLocation("DefeatArrghus", "Swamp Palace - Boss", world);
                PlaceBossItemInLocation("DefeatMothula", "Skull Woods - Boss", world);
                PlaceBossItemInLocation("DefeatBlind", "Thieves' Town - Boss", world);
                if (world.Config.Weapon != WeaponOption.Swordless)
                    PlaceBossItemInLocation("DefeatKholdstare", "Ice Palace - Boss", world);
                PlaceBossItemInLocation("DefeatVitreous", "Misery Mire - Boss", world);
                PlaceBossItemInLocation("DefeatTrinexx", "Turtle Rock - Boss", world);
                PlaceBossItemInLocation("DefeatArmosKnight", "Ganon's Tower - Ice Armos", world);
                PlaceBossItemInLocation("DefeatLanmolas", "Ganon's Tower - Lanmolas", world);
                PlaceBossItemInLocation("DefeatMoldorm", "Ganon's Tower - Moldorm", world);
                break;
        }
    }

    /// <summary>Place Boss item in location.</summary>
    /// <param name="bossItem">Boss item name</param>
    /// <param name="location">Location name</param>
    /// <param name="world">World</param>
    /// <exception cref="Exception">If can't place boss in location</exception>
    private static void PlaceBossItemInLocation(string bossItem, string location, World world)
    {
        var worldBossItem = world.GetItem(bossItem);
        string fromLocation = BOSS_FROM_LOCATION[location];
        var from = world.GetLocation(fromLocation);
        var toBoss = world.GetLocation(location);

        if (from is null || toBoss is null)
        {
            throw new Exception("Can't place boss.");
        }

        from.Edges.RemoveAll(e => e.To != toBoss);
        from.Edges.Find(e => e.To == toBoss)!.Condition = new ItemCondition(worldBossItem, 1);
    }
}
