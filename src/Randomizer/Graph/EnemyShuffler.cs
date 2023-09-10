namespace AlttpRandomizer.Graph;

/**
 * Modify the edges of the graph to shuffle entrances.
 */
internal sealed class EnemyShuffler
{
    private static readonly Dictionary<string, string[]> CHALLENGE_ROOMS = new()
    {
        { "Mini Moldorm Cave Entrance", new[] {
            "Mini Moldorm Cave - Mini Moldorm 1",
            "Mini Moldorm Cave - Mini Moldorm 2",
            "Mini Moldorm Cave - Mini Moldorm 3",
            "Mini Moldorm Cave - Mini Moldorm 4",
        }},
        { "Turtle Rock - Bomb Ambush", new[] {
            "Turtle Rock - Bomb Ambush - Zol 1",
            "Turtle Rock - Bomb Ambush - Zol 2",
            "Turtle Rock - Bomb Ambush - Zol 3",
        }},
        { "Palace of Darkness - Turtle Party", new[] {
            "Palace of Darkness - Turtle Party - Terrorpin TR",
            "Palace of Darkness - Turtle Party - Terrorpin TL",
            "Palace of Darkness - Turtle Party - Terrorpin R",
            "Palace of Darkness - Turtle Party - Terrorpin L",
            "Palace of Darkness - Turtle Party - Terrorpin BR",
            "Palace of Darkness - Turtle Party - Terrorpin BL",
        }},
        { "Ice Palace - Entrance", new[] {
            "Ice Palace - Entrance - Freezor",
        }},
        { "Ice Palace - Bari Key", new[] {
            "Ice Palace - Bari Key - Top Bari",
            "Ice Palace - Bari Key - Middle Bari",
            "Ice Palace - Bari Key - Bottom Bari",
        }},
        { "Palace of Darkness - Mimics 2", new[] {
            "Palace of Darkness - Mimics 2 - Red Eyegore",
            "Palace of Darkness - Mimics 2 - Green Eyegore L",
            "Palace of Darkness - Mimics 2 - Green Eyegore R",
        }},
        // "Ganon's Tower - Ice Armos" // covered by boss shuffle code for now.
        { "Turtle Rock - Pokeys 2", new[] {
            "Turtle Rock - Pokeys 2 - Pokey 1",
            "Turtle Rock - Pokeys 2 - Pokey 2",
            "Turtle Rock - Pokeys 2 - Medusa 3",
        }},
        { "Tower Of Hera - Beetle Gate", new[] {
            "Tower Of Hera - Beetle Gate - Hardhat Beetle R",
            "Tower Of Hera - Beetle Gate - Hardhat Beetle L",
            "Tower Of Hera - Beetle Gate - Hardhat Beetle B",
        }},
        { "Ice Palace - Stalfos Ambush", new[] {
            "Ice Palace - Stalfos Ambush - Stalfos Knight T",
            "Ice Palace - Stalfos Ambush - Stalfos Knight B",
        }},
        { "Thieves' Town - Basement Block Totems", new[] {
            "Thieves' Town - Basement Block Totems - Red Zazak",
            "Thieves' Town - Basement Block Totems - Blue Zazak",
            "Thieves' Town - Basement Block Totems - Stalfos",
        }},
        { "Palace of Darkness - Mimics 1", new[] {
            "Palace of Darkness - Mimics 1 - Red Eyegore",
            "Palace of Darkness - Mimics 1 - Green Eyegore L",
            "Palace of Darkness - Mimics 1 - Green Eyegore R",
        }},
        { "Desert Palace - Popo Genocide", new[] {
            "Desert Palace - Popo Genocide - Beamos",
            "Desert Palace - Popo Genocide - Popo TR",
            "Desert Palace - Popo Genocide - Popo TL",
            "Desert Palace - Popo Genocide - Popo BR",
            "Desert Palace - Popo Genocide - Popo BL",
        }},
        { "Ganon's Tower - Mimics 1", new[] {
            "Ganon's Tower - Mimics 1 - Statue",
            "Ganon's Tower - Mimics 1 - Red Eyegore 2",
            "Ganon's Tower - Mimics 1 - Spike Trap 1",
            "Ganon's Tower - Mimics 1 - Spike Trap 2",
            "Ganon's Tower - Mimics 1 - Red Eyegore 3",
        }},
        { "Ganon's Tower - Mimics 2", new[] {
            "Ganon's Tower - Mimics 2 - Red Eyegore T",
            "Ganon's Tower - Mimics 2 - Beamos TR",
            "Ganon's Tower - Mimics 2 - Beamos BL",
            "Ganon's Tower - Mimics 2 - Red Eyegore B",
        }},
        { "Ganon's Tower - Gauntlet 1", new[] {
            "Ganon's Tower - Gauntlet 1 - Red Zazak",
            "Ganon's Tower - Gauntlet 1 - Stalfos L",
            "Ganon's Tower - Gauntlet 1 - Blue Zazak",
            "Ganon's Tower - Gauntlet 1 - Stalfos R",
        }},
        { "Ganon's Tower - Gauntlet 2", new[] {
            "Ganon's Tower - Gauntlet 2 - Beamos",
            "Ganon's Tower - Gauntlet 2 - Stalfos T",
            "Ganon's Tower - Gauntlet 2 - Stalfos L",
            "Ganon's Tower - Gauntlet 2 - Stalfos B",
        }},
        { "Ganon's Tower - Gauntlet 3", new[] {
            "Ganon's Tower - Gauntlet 3 - Beamos TL",
            "Ganon's Tower - Gauntlet 3 - Blue Zazak TR",
            "Ganon's Tower - Gauntlet 3 - Blue Zazak BL",
            "Ganon's Tower - Gauntlet 3 - Blue Zazak B",
            "Ganon's Tower - Gauntlet 3 - Beamos BR",
        }},
        { "Ganon's Tower - Gauntlet 4", new[] {
            "Ganon's Tower - Gauntlet 4 - Red Zazak TL",
            "Ganon's Tower - Gauntlet 4 - Beamos TR",
            "Ganon's Tower - Gauntlet 4 - Beamos BL",
            "Ganon's Tower - Gauntlet 4 - Red Zazak BR",
        }},
        { "Ganon's Tower - Gauntlet 5", new[] {
            "Ganon's Tower - Gauntlet 5 - Medusa",
            "Ganon's Tower - Gauntlet 5 - Beamos",
            "Ganon's Tower - Gauntlet 5 - Stalfos",
            "Ganon's Tower - Gauntlet 5 - Red Zazak",
            "Ganon's Tower - Gauntlet 5 - Spark",
        }},
        // "Ganon's Tower - Lanmolas" // covered by boss shuffle code for now.
        { "Ice Palace - Penguin Line Up", new[] {
            "Ice Palace - Penguin Line Up - Pengator 1",
            "Ice Palace - Penguin Line Up - Pengator 2",
            "Ice Palace - Penguin Line Up - Pengator 3",
            "Ice Palace - Penguin Line Up - Pengator 4",
            "Ice Palace - Penguin Line Up - Pengator 5",
        }},
        { "Hyrule Castle - Basement Trap", new[] {
            "Hyrule Castle - Basement Trap - Green Guard"
        }},
        { "Hyrule Castle - Basement Statue Trap", new[] {
            "Hyrule Castle - Basement Statue Trap - Guard Key",
        }},
        // skipping room 0x75 (someone else can impl)
        // skipping room 0x7b (someone else can impl)
        // skipping room 0x7d (someone else can impl)
        // skipping room 0x7e (freezor room chest)
        { "Desert Palace - Compass Room", new[] {
            "Desert Palace - Compass Room - Popo TL",
            "Desert Palace - Compass Room - Popo TR",
            "Desert Palace - Compass Room - Popo BL",
            "Desert Palace - Compass Room - Beamos",
        }},
        { "Ganon's Tower - Tile Room", new[] { // chest
            "Ganon's Tower - Tile Room - Tiles",
            "Ganon's Tower - Tile Room - Yomo Medusa T",
            "Ganon's Tower - Tile Room - Antifairy",
            "Ganon's Tower - Tile Room - Bunny Beam",
            "Ganon's Tower - Tile Room - Yomo Medusa B",
            "Ganon's Tower - Tile Room - Wallmaster",
        }},
        { "Ganon's Tower - Wizzrobes 1", new[] {
            "Ganon's Tower - Wizzrobes 1 - Wizzrobe TL",
            "Ganon's Tower - Wizzrobes 1 - Wizzrobe TR",
            "Ganon's Tower - Wizzrobes 1 - Wizzrobe B",
        }},
        { "Ganon's Tower - Wizzrobes 2", new[] {
            "Ganon's Tower - Wizzrobes 2 - Wizzrobe TL",
            "Ganon's Tower - Wizzrobes 2 - Wizzrobe TR",
            "Ganon's Tower - Wizzrobes 2 - Spike Trap",
            "Ganon's Tower - Wizzrobes 2 - Wizzrobe BL",
            "Ganon's Tower - Wizzrobes 2 - Wizzrobe BR",
        }},
        { "Eastern Palace - West Wing - Stalfos Ambush", new[] {
            "Eastern Palace - West Wing - Stalfos Ambush - Stalfos Party",
        }},
        { "Agahnims Tower - Circle of Pots", new[] {
            "Agahnims Tower - Circle of Pots - Keese TL",
            "Agahnims Tower - Circle of Pots - Keese TR",
            "Agahnims Tower - Circle of Pots - Red Spear Guard C",
            "Agahnims Tower - Circle of Pots - Red Spear Guard TR",
        }},
        { "Agahnims Tower - Guards", new[] {
            "Agahnims Tower - Guards - Red Spear Guard B",
            "Agahnims Tower - Guards - Red Spear Guard C",
        }},
        { "Agahnims Tower - Throwing Guards", new[] {
            "Agahnims Tower - Throwing Guards - Keese L",
            "Agahnims Tower - Throwing Guards - Keese R",
            "Agahnims Tower - Throwing Guards - Red Javelin Guard L",
            "Agahnims Tower - Throwing Guards - Red Javelin Guard R",
        }},
        { "Misery Mire - Sluggula Cross", new[] {
            "Misery Mire - Sluggula Cross - Sluggula TL",
            "Misery Mire - Sluggula Cross - Sluggula TR",
            "Misery Mire - Sluggula Cross - Antifairy",
            "Misery Mire - Sluggula Cross - Sluggula BL",
            "Misery Mire - Sluggula Cross - Sluggula BR",
        }},
        // skipping room 0xb6 (someone else can impl tile room)
        // skipping 0xb8 (antifairy circle on pot, need to consider further)
        { "Agahnims Tower - Dark Key Guards", new[] {
            "Agahnims Tower - Dark Key Guards - Blue Guard",
            "Agahnims Tower - Dark Key Guards - Blue Archer R",
            "Agahnims Tower - Dark Key Guards - Blue Archer L",
        }},
        { "Misery Mire - Mire 2", new[] {
            "Misery Mire - Mire 2 - Wizzrobe T",
            "Misery Mire - Mire 2 - Popo TR",
            "Misery Mire - Mire 2 - Wizzrobe TL",
            "Misery Mire - Mire 2 - Wizzrobe TR",
            "Misery Mire - Mire 2 - Beamos",
            "Misery Mire - Mire 2 - Popo C",
            "Misery Mire - Mire 2 - Popo L",
            "Misery Mire - Mire 2 - Wizzrobe B",
            "Misery Mire - Mire 2 - Popo BL",
            "Misery Mire - Mire 2 - Popo BR",
        }},
        { "Eastern Palace - Kill Room 1", new[] {
            "Eastern Palace - Kill Room 1 - Red Eyegore",
            "Eastern Palace - Kill Room 1 - Stalfos T",
            "Eastern Palace - Kill Room 1 - Stalfos B",
        }},
        { "Eastern Palace - Kill Room 2", new[] {
            "Eastern Palace - Kill Room 2 - Red Eyegore L",
            "Eastern Palace - Kill Room 2 - Red Eyegore R",
            "Eastern Palace - Kill Room 2 - Popo B TL",
            "Eastern Palace - Kill Room 2 - Popo B TR",
            "Eastern Palace - Kill Room 2 - Popo B LT",
            "Eastern Palace - Kill Room 2 - Popo B RT",
            "Eastern Palace - Kill Room 2 - Popo LB",
            "Eastern Palace - Kill Room 2 - Popo RB",
        }},
        { "Agahnims Tower - Chain Guards", new[] {
            "Agahnims Tower - Chain Guards - Ball 'n' Chain Guard L",
            "Agahnims Tower - Chain Guards - Ball 'n' Chain Guard R",
        }},
        { "Agahnims Tower - Knife Guards", new[] {
            "Agahnims Tower - Knife Guards - Charging Blue Guard T",
            "Agahnims Tower - Knife Guards - Charging Blue Guard B",
        }},
        { "Swamp Palace - Entrance Ledge", new[] {
            "Swamp Palace - Entrance Ledge - Kyameron",
            "Swamp Palace - Entrance Ledge - Hover 1",
            "Swamp Palace - Entrance Ledge - Hover 2",
            "Swamp Palace - Entrance Ledge - Hover 3",
            "Swamp Palace - Entrance Ledge - Spike Trap",
        }},
        { "Mimic Cave Entrance", new[] {
            "Mimic Cave Entrance - Green Eyegore TL",
            "Mimic Cave Entrance - Green Eyegore TR",
            "Mimic Cave Entrance - Green Eyegore CR",
            "Mimic Cave Entrance - Green Eyegore BR",
        }},
    };
    private readonly Dictionary<string, List<string>> _defeats = new();
    private readonly World _world;

    /**
     * Add all the vertices to the graph for this region.
     * 
     * 1) Rearrange all the sprite sheet sets
     * 2) find out which sprites can be placed with each set now
     * 3) pick a random sheet for a room
     * 4) pick random sprites for room
     *
     * @param World world world to reduce graph for
     *
     * @return void
     */
    public EnemyShuffler(World world)
    {
        _world = world;
        _defeats = YamlReader.LoadEnemies();

        int world_id = _world.Id;
        foreach (string token in _defeats.Keys)
        {
            _world.Graph.NewVertex(new()
            {
                { "name", $"{token}:{world_id}" },
                { "type", "meta" },
                { "item", Item.Get(token, world_id) },
            });
        }
    }

    /**
     * Swap Edges based on new enemy locations settings.
     * 
     * @todo is this going to have a problem with the BunnyGraphifier??
     */
    public void AdjustEdges()
    {
        var from = _world.GetLocation("Meta");
        int world_id = _world.Id;
        foreach (var (token, items) in _defeats)
        {
            var to = _world.Graph.GetVertex($"{token}:{world_id}");
            foreach (string item in items)
            {
                _world.Graph.AddDirected(from, to, $"{item}:{world_id}");
            }
        }

        foreach (var (room, enemies) in CHALLENGE_ROOMS)
        {
            from = _world.GetLocation(room);
            foreach (string enemy in enemies)
            {
                var to = _world.GetLocation(enemy);
                if (to is null)
                {
                    throw new Exception($"Cannot find location for {enemy}: {to}");
                }
                string take = $"Defeat{to.Sprite.Name}:{world_id}";
                _world.Graph.AddDirected(from, to, take);
            }
        }
    }
}
