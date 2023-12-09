namespace Randomizer.Graph;

/**
 * Modify the edges of the graph to place bosses.
 */
internal sealed class BossShuffler : IWorldModifier
{
    private static readonly Dictionary<string, string> BOSS_ITEMS = new()
    {
        { "Armos", "DefeatArmos" },
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
    private static readonly string[] NEVER_PLACE =
    [
        "DefeatAgahnim",
        "DefeatAgahnim2",
        "DefeatGanon",
    ];
    private static readonly Dictionary<string, string[]> NO_PLACE = new()
    {
        {  "Ganon's Tower - Moldorm", new[] {
            "DefeatArmos",
            "DefeatArrghus",
            "DefeatBlind",
            "DefeatLanmolas",
            "DefeatTrinexx",
        } },
        { "Ganon's Tower - Lanmolas", new[] {
            "DefeatBlind",
        } },
        { "Ganon's Tower - Ice Armos", new[] {
        "DefeatTrinexx",
        } },
        { "Skull Woods - Boss", new[] {
        "DefeatTrinexx",
        } },
        { "Tower Of Hera - Boss", new[] {
            "DefeatArmos",
            "DefeatArrghus",
            "DefeatBlind",
            "DefeatLanmolas",
            "DefeatTrinexx",
        } },
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

    /**
     * Swap Entrances based on world settings.
     */
    public static void AdjustEdges(World world, PRNG prng)
    {
        var bossLocationMap = YamlReader.LoadSpriteLocations()
            .ToDictionary(x => $"{x.Key}:{world.Id}", x => x.Value);

        // most restrictive first
        var boss_locations = new List<string>()
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
            boss_locations.RemoveAt(9);
            PlaceBossItemInLocation("DefeatKholdstare", "Ice Palace - Boss", world, bossLocationMap);
        }

        List<string> place_bosses;
        switch (world.Config.BossShuffle)
        {
            case BossShuffleOption.Random:
                foreach (string location in boss_locations)
                {
                    var bosses = NO_PLACE.TryGetValue(location, out string[]? noPlaceLocations)
                        ? BOSS_ITEMS.Values.Except(noPlaceLocations).Except(NEVER_PLACE)
                        : BOSS_ITEMS.Values.Except(NEVER_PLACE);
                    string boss = prng.Shuffle(bosses).First();
                    PlaceBossItemInLocation(boss, location, world, bossLocationMap);
                }
                break;
            case BossShuffleOption.Full: // 1 copy of each, +3 other copies
                place_bosses = new()
                {
                    "DefeatArmos",
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
                place_bosses.AddRange(prng.Shuffle(place_bosses).Take(3));

                foreach (string location in boss_locations)
                {
                    var bosses = NO_PLACE.TryGetValue(location, out string[]? noPlaceLocations)
                        ? place_bosses.Except(noPlaceLocations).Except(NEVER_PLACE)
                        : place_bosses.Except(NEVER_PLACE);
                    string boss = prng.Shuffle(bosses).First();
                    place_bosses.Remove(boss);
                    PlaceBossItemInLocation(boss, location, world, bossLocationMap);
                }
                break;
            case BossShuffleOption.Simple: // 1:1
                place_bosses = new()
                {
                    "DefeatArmos",
                    "DefeatLanmolas",
                    "DefeatMoldorm",
                    "DefeatHelmasaur",
                    "DefeatArrghus",
                    "DefeatMothula",
                    "DefeatBlind",
                    "DefeatKholdstare",
                    "DefeatVitreous",
                    "DefeatTrinexx",
                    "DefeatArmos",
                    "DefeatLanmolas",
                    "DefeatMoldorm",
                };

                foreach (string location in boss_locations)
                {
                    var bosses = NO_PLACE.TryGetValue(location, out string[]? noPlaceLocations)
                        ? place_bosses.Except(noPlaceLocations).Except(NEVER_PLACE)
                        : place_bosses.Except(NEVER_PLACE);
                    string boss = prng.Shuffle(bosses).First();
                    place_bosses.Remove(boss);
                    PlaceBossItemInLocation(boss, location, world, bossLocationMap);
                }
                break;
            case BossShuffleOption.None:
            default:
                PlaceBossItemInLocation("DefeatArmos", "Eastern Palace - Boss", world, bossLocationMap);
                PlaceBossItemInLocation("DefeatLanmolas", "Desert Palace - Boss", world, bossLocationMap);
                PlaceBossItemInLocation("DefeatMoldorm", "Tower Of Hera - Boss", world, bossLocationMap);
                PlaceBossItemInLocation("DefeatHelmasaur", "Palace of Darkness - Boss", world, bossLocationMap);
                PlaceBossItemInLocation("DefeatArrghus", "Swamp Palace - Boss", world, bossLocationMap);
                PlaceBossItemInLocation("DefeatMothula", "Skull Woods - Boss", world, bossLocationMap);
                PlaceBossItemInLocation("DefeatBlind", "Thieves' Town - Boss", world, bossLocationMap);
                PlaceBossItemInLocation("DefeatKholdstare", "Ice Palace - Boss", world, bossLocationMap);
                PlaceBossItemInLocation("DefeatVitreous", "Misery Mire - Boss", world, bossLocationMap);
                PlaceBossItemInLocation("DefeatTrinexx", "Turtle Rock - Boss", world, bossLocationMap);
                PlaceBossItemInLocation("DefeatArmos", "Ganon's Tower - Ice Armos", world, bossLocationMap);
                PlaceBossItemInLocation("DefeatLanmolas", "Ganon's Tower - Lanmolas", world, bossLocationMap);
                PlaceBossItemInLocation("DefeatMoldorm", "Ganon's Tower - Moldorm", world, bossLocationMap);
                break;
        }
    }

    /**
     * Place Boss item in location.
     *
     * @throws Exception If Can't place boss in location
     *
     * @param string boss_item Boss item name
     * @param string location Location name
     */
    private static void PlaceBossItemInLocation(string bossItem, string location, World world, Dictionary<string, Dictionary<string, List<YamlSprite>>> bossLocationMap)
    {
        var world_boss_item = world.GetItem(bossItem);
        string from_location = BOSS_FROM_LOCATION[location] + ":" + world.Id;
        var from = world.Graph.GetVertex(from_location);
        location = location + ":" + world.Id;
        var to_boss = world.Graph.GetVertex(location);

        if (from is null || to_boss is null)
        {
            //Log.error("Can't place boss.", [from_location, from, location, to_boss]);

            throw new Exception("Can't place boss.");
        }

        to_boss.EnemizerBoss = BOSS_ITEMS.FirstOrDefault(kvp => kvp.Value == bossItem).Key;
        UpdateSprites(from, bossItem, world, bossLocationMap);
        world.Graph.AddDirected(from, to_boss, world_boss_item);
    }

    private static void UpdateSprites(Vertex bossRoom, string boss, World world, Dictionary<string, Dictionary<string, List<YamlSprite>>> bossLocationMap)
    {
        foreach (var sprite_definition in bossLocationMap[bossRoom.Name][boss])
        {
            world.Graph.AddVertex(new Vertex
            {
                Type = VertexType.Mob,
                Name = $"{sprite_definition.Name}",
                Position = sprite_definition.Position,
                RoomId = sprite_definition.RoomId,
                Sprite = Sprite.Get(sprite_definition.Sprite),
            });
        }
    }
}
