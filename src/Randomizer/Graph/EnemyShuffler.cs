namespace Randomizer.Graph;

using System.Diagnostics;
using System.Security.Cryptography;

/// <summary>
/// Modify the edges of the graph to shuffle entrances.
/// </summary>
internal sealed class EnemyShuffler : IWorldModifier
{
    /// <summary>
    /// Swap Edges based on new enemy locations settings.
    ///
    /// 1) Rearrange all the sprite sheet sets
    /// 2) find out which sprites can be placed with each set now
    /// 3) pick a random sheet for a room
    /// 4) pick random sprites for room
    /// </summary>
    /// <param name="world">World to modify</param>
    /// <param name="prng">PRNG to use</param>
    public static void AdjustEdges(World world, PRNG prng)
    {
        var defeats = YamlReader.LoadEnemies();

        foreach (string token in defeats.Keys)
        {
            if (!world.HasLocation($"Defeat{token}"))
            {
                world.Graph.AddVertex(new Vertex
                {
                    Type = VertexType.Meta,
                    Name = $"Defeat{token}",
                    World = world,
                    Item = world.GetItem($"Defeat{token}"),
                });
            }

            if (!world.HasLocation($"DarkDefeat{token}"))
            {
                world.Graph.AddVertex(new Vertex
                {
                    Type = VertexType.Meta,
                    Name = $"DarkDefeat{token}",
                    World = world,
                    Item = world.GetItem($"DarkDefeat{token}"),
                });
            }
        }

        var enemiesRoot = world.GetLocation("Enemies");
        foreach (var (token, items) in defeats)
        {
            var enemyRoot = new Vertex
            {
                Type = VertexType.Meta,
                Name = $"Fight{token}",
                World = world,
            };
            world.Graph.AddVertex(enemyRoot);
            enemiesRoot.Edges.Add(new Edge(enemiesRoot, enemyRoot, new ItemCondition(world.GetItem("fixed"), 1)));

            var to = world.GetLocation($"Defeat{token}");
            var toDark = world.GetLocation($"DarkDefeat{token}");
            foreach (string item in items)
            {
                enemyRoot.Edges.Add(new Edge(enemyRoot, to, new ItemCondition(world.GetItem(item), 1)));
                enemyRoot.Edges.Add(new Edge(enemyRoot, toDark, new ItemCondition(world.GetItem(BunnyGraphifier.ToDarkItem(item) ?? item), 1)));
            }
        }

        CreateBosses(world);

        // Replaces the fixed condition to mobs with a Defeat condition
        // if one exists.
        foreach (var vertex in world.Graph.GetVertices().Where(v => v.World == world))
        {
            foreach (var edge in vertex.Edges)
            {
                if (edge.To.Type != VertexType.Mob)
                    continue;
                if (!edge.Condition.IsUnconditional)
                    continue;
                if (edge.To.Sprite == null)
                    continue;

                var item = world.GetExistingItem($"Defeat{edge.To.Sprite!.Name}");
                if (item != null)
                {
                    edge.Condition = new ItemCondition(item, 1);
                }
            }
        }
    }

    private static void CreateBosses(World world)
    {
        var bosses = YamlReader.LoadBosses();
        var enemiesRoot = world.GetLocation("Enemies");

        {
            // Helmasaur
            var enemyRoot = new Vertex
            {
                Type = VertexType.Meta,
                Name = "FightHelmasaur",
                World = world,
            };
            world.Graph.AddVertex(enemyRoot);
            enemiesRoot.Edges.Add(new Edge(enemiesRoot, enemyRoot, new ItemCondition(world.GetItem("fixed"), 1)));

            {
                var breakShell = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = "DefeatHelmasaurShell",
                    World = world,
                };
                world.Graph.AddVertex(breakShell);
                enemyRoot.Edges.Add(new Edge(enemyRoot, breakShell, new ItemCondition(world.GetItem("UseBomb"), 1)));
                enemyRoot.Edges.Add(new Edge(enemyRoot, breakShell, new ItemCondition(world.GetItem("Hammer"), 1)));

                var defeatHelmasaur = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = "DefeatHelmasaur",
                    World = world,
                    Item = world.GetItem("DefeatHelmasaur"),
                };
                world.Graph.AddVertex(defeatHelmasaur);
                foreach (var item in bosses["Helmasaur"])
                {
                    breakShell.Edges.Add(new Edge(breakShell, defeatHelmasaur, new ItemCondition(world.GetItem(item), 1)));
                }
            }

            {
                var breakShellDark = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = "DarkDefeatHelmasaurShell",
                    World = world,
                };
                world.Graph.AddVertex(breakShellDark);
                enemyRoot.Edges.Add(new Edge(enemyRoot, breakShellDark, new ItemCondition(world.GetItem(BunnyGraphifier.ToDarkItem("UseBomb")!), 1)));
                enemyRoot.Edges.Add(new Edge(enemyRoot, breakShellDark, new ItemCondition(world.GetItem(BunnyGraphifier.ToDarkItem("Hammer")!), 1)));

                var defeatHelmasaurDark = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = "DarkDefeatHelmasaur",
                    World = world,
                    Item = world.GetItem("DarkDefeatHelmasaur"),
                };
                world.Graph.AddVertex(defeatHelmasaurDark);
                foreach (var item in bosses["Helmasaur"])
                {
                    breakShellDark.Edges.Add(new Edge(breakShellDark, defeatHelmasaurDark, new ItemCondition(world.GetItem(BunnyGraphifier.ToDarkItem(item) ?? item), 1)));
                }
            }
        }

        {
            // Arrghus
            var enemyRoot = new Vertex
            {
                Type = VertexType.Meta,
                Name = "FightArrghus",
                World = world,
            };
            world.Graph.AddVertex(enemyRoot);
            enemiesRoot.Edges.Add(new Edge(enemiesRoot, enemyRoot, new ItemCondition(world.GetItem("fixed"), 1)));

            {
                var pullSpawn = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = "DefeatArrghusSpawn",
                    World = world,
                };
                world.Graph.AddVertex(pullSpawn);
                enemyRoot.Edges.Add(new Edge(enemyRoot, pullSpawn, new ItemCondition(world.GetItem("Hookshot"), 1)));

                var defeatArrghus = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = "DefeatArrghus",
                    World = world,
                    Item = world.GetItem("DefeatArrghus"),
                };
                world.Graph.AddVertex(defeatArrghus);
                // Arrghus defeat conditions are a subset of the ones for defeating a spawn, so we can ignore bombs
                foreach (var item in bosses["Arrghus"])
                {
                    pullSpawn.Edges.Add(new Edge(pullSpawn, defeatArrghus, new ItemCondition(world.GetItem(item), 1)));
                }
            }

            {
                var pullSpawnDark = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = "DarkDefeatArrghusSpawn",
                    World = world,
                };
                world.Graph.AddVertex(pullSpawnDark);
                enemyRoot.Edges.Add(new Edge(enemyRoot, pullSpawnDark, new ItemCondition(world.GetItem(BunnyGraphifier.ToDarkItem("Hookshot")!), 1)));

                var defeatArrghusDark = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = "DarkDefeatArrghus",
                    World = world,
                    Item = world.GetItem("DarkDefeatArrghus"),
                };
                world.Graph.AddVertex(defeatArrghusDark);
                foreach (var item in bosses["Arrghus"])
                {
                    pullSpawnDark.Edges.Add(new Edge(pullSpawnDark, defeatArrghusDark, new ItemCondition(world.GetItem(BunnyGraphifier.ToDarkItem(item) ?? item), 1)));
                }
            }
        }

        {
            // Kholdstare
            var enemyRoot = new Vertex
            {
                Type = VertexType.Meta,
                Name = "FightKholdstare",
                World = world,
            };
            world.Graph.AddVertex(enemyRoot);
            enemiesRoot.Edges.Add(new Edge(enemiesRoot, enemyRoot, new ItemCondition(world.GetItem("fixed"), 1)));

            {
                var defeatShell = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = "DefeatKholdstareShell",
                    World = world,
                };
                world.Graph.AddVertex(defeatShell);
                foreach (var item in bosses["KholdstareShell"])
                {
                    enemyRoot.Edges.Add(new Edge(enemyRoot, defeatShell, new ItemCondition(world.GetItem(item), 1)));
                }

                var defeatKholdstare = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = "DefeatKholdstare",
                    World = world,
                    Item = world.GetItem("DefeatKholdstare"),
                };
                world.Graph.AddVertex(defeatKholdstare);
                foreach (var item in bosses["Kholdstare"])
                {
                    defeatShell.Edges.Add(new Edge(defeatShell, defeatKholdstare, new ItemCondition(world.GetItem(item), 1)));
                }
            }

            {
                var defeatShellDark = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = "DarkDefeatKholdstareShell",
                    World = world,
                };
                world.Graph.AddVertex(defeatShellDark);
                foreach (var item in bosses["KholdstareShell"])
                {
                    enemyRoot.Edges.Add(new Edge(enemyRoot, defeatShellDark, new ItemCondition(world.GetItem(BunnyGraphifier.ToDarkItem(item) ?? item), 1)));
                }

                var defeatKholdstareDark = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = "DarkDefeatKholdstare",
                    World = world,
                    Item = world.GetItem("DarkDefeatKholdstare"),
                };
                world.Graph.AddVertex(defeatKholdstareDark);
                foreach (var item in bosses["Kholdstare"])
                {
                    defeatShellDark.Edges.Add(new Edge(defeatShellDark, defeatKholdstareDark, new ItemCondition(world.GetItem(BunnyGraphifier.ToDarkItem(item) ?? item), 1)));
                }
            }
        }

        {
            // Trinexx
            var enemyRoot = new Vertex
            {
                Type = VertexType.Meta,
                Name = "FightTrinexx",
                World = world,
            };
            world.Graph.AddVertex(enemyRoot);
            enemiesRoot.Edges.Add(new Edge(enemiesRoot, enemyRoot, new ItemCondition(world.GetItem("fixed"), 1)));

            {
                var defeatHeadFire = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = "DefeatTrinexxHeadFire",
                    World = world,
                };
                world.Graph.AddVertex(defeatHeadFire);
                enemyRoot.Edges.Add(new Edge(enemyRoot, defeatHeadFire, new ItemCondition(world.GetItem("FireRod"), 1)));

                var defeatHeadIce = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = "DefeatTrinexxHeadIce",
                    World = world,
                };
                world.Graph.AddVertex(defeatHeadIce);
                defeatHeadFire.Edges.Add(new Edge(defeatHeadFire, defeatHeadIce, new ItemCondition(world.GetItem("IceRod"), 1)));

                var defeatTrinexx = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = "DefeatTrinexx",
                    World = world,
                    Item = world.GetItem("DefeatTrinexx"),
                };
                world.Graph.AddVertex(defeatTrinexx);
                foreach (var item in bosses["Trinexx"])
                {
                    defeatHeadIce.Edges.Add(new Edge(defeatHeadIce, defeatTrinexx, new ItemCondition(world.GetItem(item), 1)));
                }
            }

            {
                var defeatHeadFireDark = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = "DarkDefeatTrinexxHeadFire",
                    World = world,
                };
                world.Graph.AddVertex(defeatHeadFireDark);
                enemyRoot.Edges.Add(new Edge(enemyRoot, defeatHeadFireDark, new ItemCondition(world.GetItem(BunnyGraphifier.ToDarkItem("FireRod")!), 1)));

                var defeatHeadIceDark = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = "DarkDefeatTrinexxHeadIce",
                    World = world,
                };
                world.Graph.AddVertex(defeatHeadIceDark);
                defeatHeadFireDark.Edges.Add(new Edge(defeatHeadFireDark, defeatHeadIceDark, new ItemCondition(world.GetItem(BunnyGraphifier.ToDarkItem("IceRod")!), 1)));

                var defeatTrinexxDark = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = "DarkDefeatTrinexx",
                    World = world,
                    Item = world.GetItem("DarkDefeatTrinexx"),
                };
                world.Graph.AddVertex(defeatTrinexxDark);
                foreach (var item in bosses["Trinexx"])
                {
                    defeatHeadIceDark.Edges.Add(new Edge(defeatHeadIceDark, defeatTrinexxDark, new ItemCondition(world.GetItem(BunnyGraphifier.ToDarkItem(item) ?? item), 1)));
                }
            }
        }
    }
}

/*
    // remove this and use the same alg for UW enemies
    const OW_MAP_SHEETS = [
        0x02 => [0x0F, null, 0x4A, null],
        0x03 => [null, null, 0x12, 0x10],
        0x14 => [0x0E, null, null, null],
        0x18 => [0x4F, 0x49, 0x4A, 0x50],
        0x1B => [null, null, null, 0x1D],
        0x30 => [null, null, 0x12, null],
        0x3A => [null, null, null, 0x11], // this should be handled?
        0x4F => [null, null, 0x18, null],
        0x5E => [null, null, null, 0x19],
    ];

    private readonly array $defeats;
    private readonly array $challenge_enemies;
    private readonly array $no_place_sprites;

    public function __construct(private World $world)
    {
        $this->defeats = Yaml::parse(file_get_contents(app_path('Graph/data/Enemizer/enemies.yml'))) ?? [];
        $this->challenge_enemies = Yaml::parse(file_get_contents(app_path('Graph/data/Enemizer/challenge.yml'))) ?? [];
        $this->no_place_sprites = Yaml::parse(file_get_contents(app_path('Graph/data/Enemizer/noplace.yml'))) ?? [];

        $world_id = $this->world->id;
        foreach (array_keys($this->defeats) as $token) {
            $this->world->graph->newVertex([
                'name' => "$token:$world_id",
                'type' => 'meta',
                'item' => Item::get($token, $world_id),
            ]);
        }

        $enemies = $world->getLocationsOfType('mob');

        $enemy_rooms = $enemies->groupBy(fn ($enemy) => $enemy->roomid);
        $enemy_ows = $enemies->groupBy(fn ($enemy) => $enemy->map);

        // Set up sprite sheets
        $sheetable_sprites = Sprite::all()->filter(
            fn ($s) => count(array_filter($s->sheets, fn ($v) => $v !== null)) !== 0
        );
        // sprites that can be moved to any room as they don't have any sheet
        // requirements
        $nosheet_sprites = Sprite::all()->filter(
            fn ($s) => count(array_filter($s->sheets, fn ($v) => $v !== null)) === 0
                && !in_array($s->name, $this->no_place_sprites)
        )->all();

        $sheet_sets = array_fill(0, 124, [null, null, null, null]);
        $sheets_to_sprites = array_fill(0, 124, $nosheet_sprites);

        $room_sheets = [];
        $ow_sheets = [];

        // deal with OW required sheet sets ($j carries over to next block, it's
        // important for filling the array properly)
        $j = 0;
        foreach (self::OW_MAP_SHEETS as $map => $ow_set) {
            $ow_sheets[$map] = $j;
            $sheet_sets[$j] = $ow_set;
            $j++;
        }

        if ($world->config('enemizer.enemyShuffle') === 'none') {
            for ($i = 0; $i < 0x80; $i++) {
                if (!isset($enemy_ows[$i]) || count($enemy_ows[$i]) === 0) {
                    $ow_sheets[$i] = 0xFF;
                    continue;
                }
                if (!isset($ow_sheets[$i])) {
                    $fixed_set = [];
                    $enemies = $enemy_ows[$i]->map(fn ($e) => $e->sprite)->all();
                    foreach ($enemies as $sprite) {
                        $filtered_sprite = array_filter($sprite->sheets, fn ($v) => $v !== null);
                        $filtered_set = array_filter($fixed_set, fn ($v) => $v !== null);
                        $fixed_set = array_replace([null, null, null, null], $filtered_set, $filtered_sprite);
                    }
                    if (empty(array_filter($fixed_set, fn ($v) => $v !== null))) {
                        continue;
                    }
                    for ($k = 0; $k < $j; ++$k) {
                        if (
                            ($fixed_set[0] === null || $sheet_sets[$k][0] === $fixed_set[0])
                            && ($fixed_set[1] === null || $sheet_sets[$k][1] === $fixed_set[1])
                            && ($fixed_set[2] === null || $sheet_sets[$k][2] === $fixed_set[2])
                            && ($fixed_set[3] === null || $sheet_sets[$k][3] === $fixed_set[3])
                        ) {
                            $ow_sheets[$i] = $k;
                            continue 2;
                        }
                    }
                    $sheet_sets[$j] = $fixed_set;
                    $ow_sheets[$i] = $j;
                    ++$j;
                }
            }
        }

        // force fixed room sets! If we have a few "no move" sprites in a room
        // we need to guarantee that a sheet set exists for that room to look
        // correct.
        for ($i = 0; $i < 0x180; $i++) {
            if (!isset($enemy_rooms[$i]) || count($enemy_rooms[$i]) === 0) {
                continue;
            }
            $filtered = $enemy_rooms[$i]->filter(
                fn ($e) => $world->config('enemizer.enemyShuffle') === 'none'
                    || in_array($e->sprite->name, $this->no_place_sprites)
            );
            if (count($filtered) === 0) {
                continue;
            }
            $fixed_set = [];
            $enemies = $filtered->map(fn ($e) => $e->sprite)->all();
            foreach ($enemies as $sprite) {
                $filtered_sprite = array_filter($sprite->sheets, fn ($v) => $v !== null);
                $filtered_set = array_filter($fixed_set, fn ($v) => $v !== null);
                $fixed_set = array_replace([null, null, null, null], $filtered_set, $filtered_sprite);
            }
            if (empty(array_filter($fixed_set, fn ($v) => $v !== null))) {
                continue;
            }
            // potential bug here where fixed set is full, we may end up making 2+ copies in table
            if ($world->config('enemizer.enemyShuffle') === 'none' || get_random_int(0, 1)) {
                for ($k = 0; $k < $j; ++$k) {
                    if (
                        ($fixed_set[0] === null || $sheet_sets[$k][0] === $fixed_set[0])
                        && ($fixed_set[1] === null || $sheet_sets[$k][1] === $fixed_set[1])
                        && ($fixed_set[2] === null || $sheet_sets[$k][2] === $fixed_set[2])
                        && ($fixed_set[3] === null || $sheet_sets[$k][3] === $fixed_set[3])
                    ) {
                        $room_sheets[$i] = $k;
                        continue 2;
                    }
                }
            }
            $sheet_sets[$j] = $fixed_set;
            $room_sheets[$i] = $j;
            ++$j;
        }

        // fill in all sheet sets with valid layouts for sprites
        for ($i = 0; $i < 124; ++$i) {
            while (in_array(null, $sheet_sets[$i], true)) {
                $sprite = $sheetable_sprites->random();
                if (
                    ($sprite->sheets[0] === null || $sheet_sets[$i][0] === null)
                    && ($sprite->sheets[1] === null || $sheet_sets[$i][1] === null)
                    && ($sprite->sheets[2] === null || $sheet_sets[$i][2] === null)
                    && ($sprite->sheets[3] === null || $sheet_sets[$i][3] === null)
                ) {
                    $filtered_sprite = array_filter($sprite->sheets, fn ($v) => $v !== null);
                    $filtered_set = array_filter($sheet_sets[$i], fn ($v) => $v !== null);
                    $sheet_sets[$i] = array_replace([null, null, null, null], $filtered_set, $filtered_sprite);
                }
            }
        }

        // find all the sprites that can be placed validly with a particular sheet set.
        foreach ($sheetable_sprites as $sprite) {
            foreach ($sheet_sets as $i => $set) {
                if (
                    $world->config('enemizer.enemyShuffle') !== 'none'
                    && ($sprite->sheets[0] === null || $set[0] === $sprite->sheets[0])
                    && ($sprite->sheets[1] === null || $set[1] === $sprite->sheets[1])
                    && ($sprite->sheets[2] === null || $set[2] === $sprite->sheets[2])
                    && ($sprite->sheets[3] === null || $set[3] === $sprite->sheets[3])
                    && !in_array($sprite->name, $this->no_place_sprites)
                ) {
                    $sheets_to_sprites[$i][$sprite->name] = $sprite;
                }
            }
        }

        $all_challenge_enemies = array_map(fn ($e) => "$e:{$this->world->id}", Arr::flatten(self::CHALLENGE_ROOMS));
        for ($i = 0; $i < 0x180; $i++) {
            if (!isset($enemy_rooms[$i]) || count($enemy_rooms[$i]) === 0) {
                $room_sheets[$i] = 0x00;
                continue;
            }
            if (!isset($room_sheets[$i])) {
                do {
                    $sheet = get_random_key($sheets_to_sprites);
                } while (count($sheets_to_sprites[$sheet]) === 0);
                $room_sheets[$i] = $sheet;
            }
            $sheet = $room_sheets[$i];
            $filtered_placable = $enemy_rooms[$i]->filter(
                fn ($e) => $world->config('enemizer.enemyShuffle') !== 'none'
                    && !in_array($e->sprite->name, $this->no_place_sprites)
            );
            if (count($filtered_placable) === 0) {
                continue;
            }
            foreach ($filtered_placable as $enemy) {
                if (in_array($enemy->name, $all_challenge_enemies)) {
                    $new = get_random_element(array_filter(
                        $sheets_to_sprites[$sheet],
                        fn ($sprite) => in_array($sprite->name, $this->challenge_enemies)
                    ));
                    if (!$new) {
                        throw new Exception('ugh');
                    }
                } else {
                    $new = get_random_element($sheets_to_sprites[$sheet]);
                }
                Log::debug(vsprintf('%s: placing %s', [
                    $enemy->name,
                    $new->getNiceName(),
                ]));
                $enemy->sprite = $new;
            }
        }

        for ($i = 0; $i < 0x80; $i++) {
            if (!isset($enemy_ows[$i]) || count($enemy_ows[$i]) === 0) {
                $ow_sheets[$i] = 0xFF;
                continue;
            }
            if (!isset($ow_sheets[$i])) {
                do {
                    $sheet = get_random_key($sheets_to_sprites);
                } while (count($sheets_to_sprites[$sheet]) === 0);
                $ow_sheets[$i] = $sheet;
            }
            $sheet = $ow_sheets[$i];
            $filtered_placable = $enemy_ows[$i]->filter(
                fn ($e) => $world->config('enemizer.enemyShuffle') !== 'none'
                    && !in_array($e->sprite->name, $this->no_place_sprites)
            );
            if (count($filtered_placable) === 0) {
                continue;
            }
            foreach ($filtered_placable as $enemy) {
                $new = get_random_element($sheets_to_sprites[$sheet]);
                Log::debug(vsprintf('%s: placing %s', [
                    $enemy->name,
                    $new->getNiceName(),
                ]));
                $enemy->sprite = $new;
            }
        }
        ksort($ow_sheets);
        $ow_sheets = array_merge(
            array_slice($ow_sheets, 0, 0x40),
            array_slice($ow_sheets, 0, 0x40),
            array_slice($ow_sheets, 0, 0x40),
            array_slice($ow_sheets, 0x40, 0x80),
        );

        // pick random sheets where we have options
        $sheet_sets = array_map(
            fn ($set) => array_map(
                fn ($sheet) => is_array($sheet) ? get_random_element($sheet) : $sheet,
                $set
            ),
            $sheet_sets
        );

        $world->sprite_sheets = [
            'underworld' => array_map(fn ($s) => $s - 0x40, $room_sheets),
            'overworld' => $ow_sheets,
            'sets' => $sheet_sets,
        ];
    }

    public function adjustEdges(): void
    {
        $from = $this->world->getLocation('Meta');
        $world_id = $this->world->id;
        foreach ($this->defeats as $token => $items) {
            $to = $this->world->graph->getVertex($token . ":$world_id");
            foreach ($items as $item) {
                $this->world->graph->addDirected($from, $to, "$item:$world_id");
            }
        }

        foreach (self::CHALLENGE_ROOMS as $room => $enemies) {
            $from = $this->world->getLocation($room);
            foreach ($enemies as $enemy) {
                $to = $this->world->getLocation($enemy);
                if (!$to) {
                    dd([$enemy, $to]);
                }
                $take = 'Defeat' . $to->sprite->name . ":$world_id";
                $this->world->graph->addDirected($from, $to, $take);
            }
        }
    }
}
*/
