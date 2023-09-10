namespace AlttpRandomizer.Graph;

/**
 * Modify the edges of the graph to place bosses.
 */
internal sealed class BossShuffler
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
    private static readonly string[] NEVER_PLACE = new[]
    {
        "DefeatAgahnim",
        "DefeatAgahnim2",
        "DefeatGanon",
    };
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

    private readonly Dictionary<string, Dictionary<string, List<YamlSprite>>> _bossLocationMap;
    private readonly World _world;

    /**
     * Add all the vertices to the graph for this region.
     *
     * @param World world world to reduce graph for
     *
     * @return void
     */
    public BossShuffler(World world)
    {
        _world = world;
        _bossLocationMap = YamlReader.LoadSpriteLocations()
            .ToDictionary(x => $"{x.Key}:{world.Id}", x => x.Value);
    }

    /**
     * Swap Entrances based on world settings.
     */
    public void AdjustEdges()
    {
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
        if (_world.Config<string>("mode.weapons") == "swordless")
        {
            // remove Ice Palace
            boss_locations.RemoveAt(9);
            PlaceBossItemInLocation("DefeatKholdstare", "Ice Palace - Boss");
        }

        List<string> place_bosses;
        switch (_world.Config<string>("enemizer.bossShuffle"))
        {
            case "random":
                foreach (string location in boss_locations)
                {
                    var bosses = NO_PLACE.TryGetValue(location, out string[]? noPlaceLocations)
                        ? BOSS_ITEMS.Values.Except(noPlaceLocations).Except(NEVER_PLACE)
                        : BOSS_ITEMS.Values.Except(NEVER_PLACE);
                    string boss = bosses.Shuffle().First();
                    PlaceBossItemInLocation(boss, location);
                }
                break;
            case "full": // 1 copy of each, +3 other copies
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
                place_bosses.AddRange(place_bosses.Shuffle().Take(3));

                foreach (string location in boss_locations)
                {
                    var bosses = NO_PLACE.TryGetValue(location, out string[]? noPlaceLocations)
                        ? place_bosses.Except(noPlaceLocations).Except(NEVER_PLACE)
                        : place_bosses.Except(NEVER_PLACE);
                    string boss = bosses.Shuffle().First();
                    place_bosses.Remove(boss);
                    PlaceBossItemInLocation(boss, location);
                }
                break;
            case "simple": // 1:1
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
                    string boss = bosses.Shuffle().First();
                    place_bosses.Remove(boss);
                    PlaceBossItemInLocation(boss, location);
                }
                break;
            case "none":
            default:
                PlaceBossItemInLocation("DefeatArmos", "Eastern Palace - Boss");
                PlaceBossItemInLocation("DefeatLanmolas", "Desert Palace - Boss");
                PlaceBossItemInLocation("DefeatMoldorm", "Tower Of Hera - Boss");
                PlaceBossItemInLocation("DefeatHelmasaur", "Palace of Darkness - Boss");
                PlaceBossItemInLocation("DefeatArrghus", "Swamp Palace - Boss");
                PlaceBossItemInLocation("DefeatMothula", "Skull Woods - Boss");
                PlaceBossItemInLocation("DefeatBlind", "Thieves' Town - Boss");
                PlaceBossItemInLocation("DefeatKholdstare", "Ice Palace - Boss");
                PlaceBossItemInLocation("DefeatVitreous", "Misery Mire - Boss");
                PlaceBossItemInLocation("DefeatTrinexx", "Turtle Rock - Boss");
                PlaceBossItemInLocation("DefeatArmos", "Ganon's Tower - Ice Armos");
                PlaceBossItemInLocation("DefeatLanmolas", "Ganon's Tower - Lanmolas");
                PlaceBossItemInLocation("DefeatMoldorm", "Ganon's Tower - Moldorm");
                break;
        }

        // since we added the bosses at this step we need to recache the
        // vertices.
        _world.RemapVertices();
    }

    /**
     * Place Boss item in location.
     * 
     * @throws Exception If Can't place boss in location
     * 
     * @param string boss_item Boss item name
     * @param string location Location name
     */
    private void PlaceBossItemInLocation(string bossItem, string location)
    {
        string world_boss_item = bossItem + ":" + _world.Id;
        string from_location = BOSS_FROM_LOCATION[location] + ":" + _world.Id;
        var from = _world.Graph.GetVertex(from_location);
        location = location + ":" + _world.Id;
        var to_boss = _world.Graph.GetVertex(location);

        if (from is null || to_boss is null)
        {
            //Log.error("Can't place boss.", [from_location, from, location, to_boss]);

            throw new Exception("Can't place boss.");
        }

        to_boss.EnemizerBoss = BOSS_ITEMS.FirstOrDefault(kvp => kvp.Value == bossItem).Key;
        UpdateSprites(from, bossItem);
        _world.Graph.AddDirected(from, to_boss, world_boss_item);
    }

    private void UpdateSprites(Vertex bossRoom, string boss)
    {
        foreach (var sprite_definition in _bossLocationMap[bossRoom.Name][boss])
        {
            _world.Graph.NewVertex(
                sprite_definition.AsDictionary().Merge(new Dictionary<string, object>()
                {
                    { "type", "mob" },
                    //{ "sprite", Sprite.get(sprite_definition["sprite"]) },
                }));
        }
    }
}
