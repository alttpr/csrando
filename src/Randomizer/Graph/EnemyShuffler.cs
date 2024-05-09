namespace Randomizer.Graph;

using Microsoft.Extensions.Logging;

/// <summary>
/// Modify the edges of the graph to shuffle entrances.
/// </summary>
internal sealed class EnemyShuffler : IWorldModifier
{
    private static readonly ILogger _logger = ClassLogger.Get();

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

        UpdateSpriteSheets(world, prng);

        // Replace the fixed condition to mobs with a Defeat condition
        // if one exists.
        foreach (var vertex in world.GetLocations())
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

    // this dictionary forces certain sheets onto certain maps,
    // which is required for those screens to work.
    // TODO: remove this and use the same alg for UW enemies
    private static readonly Dictionary<byte, byte[]?[]> OW_MAP_SHEETS = new()
    {
        [0x02] = [[0x0F], null, [0x4A], null],
        [0x03] = [null, null, [0x12], [0x10]],
        [0x14] = [[0x0E], null, null, null],
        [0x18] = [[0x4F], [0x49], [0x4A], [0x50]],
        [0x1B] = [null, null, null, [0x1D]],
        [0x30] = [null, null, [0x12], null],
        [0x3A] = [null, null, null, [0x11]], // this should be handled?
        [0x4F] = [null, null, [0x18], null],
        [0x5E] = [null, null, null, [0x19]],
    };
    private static void UpdateSpriteSheets(World world, PRNG prng)
    {
        var enemyVertices = world.GetLocationsOfType(VertexType.Mob);

        var enemyRooms = enemyVertices.ToLookup(enemy => enemy.RoomId);
        var enemyOWs = enemyVertices.ToLookup(enemy => enemy.Map);

        // Set up sprite sheets
        var sheetableSprites = Sprite.All().Where(
            s => s.Sheets.Any(v => v != null)
        );
        // sprites that can be moved to any room as they don't have any sheet
        // requirements
        var nosheetSprites = Sprite.All().Where(
            s => s.Sheets.All(v => v == null)
                && !s.Flags.HasFlag(YamlSpriteFlags.NoPlace)
        ).ToHashSet();

        const int MandatorySheets = 124;
        // NOTE: this cannot be Enumerable.Repeat, we need a distinct copy for every entry, since we modify them later.
        // those are available sheet sets (of 4 sprite sheets) that may be used later
        var sheetSets = Enumerable.Range(0, MandatorySheets).Select(_ => (new byte[]?[] { null, null, null, null })).ToList();
        // list of sprites possible when using a particular sprite sheet
        var sheetsToSprites = Enumerable.Range(0, MandatorySheets).Select(_ => nosheetSprites.ToHashSet()).ToArray();

        var roomSheets = new byte[0x180];
        Array.Fill(roomSheets, (byte)0x00);
        var owSheets = new byte[0x80];
        Array.Fill(owSheets, (byte)0xFF);

        // deal with OW required sheet sets (set index carries over to next block, it's
        // important for filling the array properly)
        int sheetSetIndex = 0;
        foreach (var (map, owSet) in OW_MAP_SHEETS)
        {
            owSheets[map] = (byte)sheetSetIndex;
            sheetSets[sheetSetIndex] = owSet;
            sheetSetIndex++;
        }

        if (world.Config.EnemyShuffle == EnemyShuffleOption.None)
        {
            for (int i = 0; i < owSheets.Length; i++)
            {
                if (!enemyOWs[i].Any())
                {
                    owSheets[i] = 0xFF;
                    continue;
                }

                if (owSheets[i] == 0xFF)
                {
                    var fixedSet = new byte[]?[4];
                    var enemies = enemyOWs[i].Select(e => e.Sprite!).ToList();
                    foreach (var sprite in enemies)
                    {
                        foreach (var (idx, value) in sprite.Sheets.Indexed(v => v != null))
                            fixedSet[idx] = value;
                    }
                    if (!fixedSet.Any(v => v != null))
                        continue;

                    bool next = false;
                    for (byte k = 0; k < sheetSetIndex; ++k)
                    {
                        byte[]?[] kSet = sheetSets[k];
                        if (
                            (fixedSet[0] == null || fixedSet[0]!.SequenceEqual(kSet[0] ?? []))
                            && (fixedSet[1] == null || fixedSet[1]!.SequenceEqual(kSet[1] ?? []))
                            && (fixedSet[2] == null || fixedSet[2]!.SequenceEqual(kSet[2] ?? []))
                            && (fixedSet[3] == null || fixedSet[3]!.SequenceEqual(kSet[3] ?? []))
                        )
                        {
                            owSheets[i] = k;
                            next = true;
                            break;
                        }
                    }
                    if (next)
                        continue;
                    sheetSets[sheetSetIndex] = fixedSet;
                    owSheets[i] = (byte)sheetSetIndex;
                    ++sheetSetIndex;
                }
            }
        }

        // force fixed room sets! If we have a few "no move" sprites in a room
        // we need to guarantee that a sheet set exists for that room to look
        // correct.
        for (int i = 0; i < roomSheets.Length; i++)
        {
            if (!enemyRooms[i].Any())
                continue;

            var filtered = enemyRooms[i].Where(
                e => world.Config.EnemyShuffle == EnemyShuffleOption.None
                    || e.Sprite!.Flags.HasFlag(YamlSpriteFlags.NoPlace)
            );
            if (!filtered.Any())
                continue;

            var fixedSet = new byte[]?[4];
            var enemies = filtered.Select(e => e.Sprite).ToList();
            foreach (var sprite in enemies)
            {
                foreach (var (idx, value) in sprite!.Sheets.Indexed(v => v != null))
                    fixedSet[idx] = value;
            }
            if (!fixedSet.Any(v => v != null))
                continue;

            bool next = false;
            // potential bug here where fixed set is full, we may end up making 2+ copies in table
            if (world.Config.EnemyShuffle == EnemyShuffleOption.None || prng.GetRandomInt(0..1) == 1)
            {
                for (byte k = 0; k < sheetSetIndex; ++k)
                {
                    if (
                        (fixedSet[0] == null || fixedSet[0]!.SequenceEqual(sheetSets[k][0] ?? []))
                        && (fixedSet[1] == null || fixedSet[1]!.SequenceEqual(sheetSets[k][1] ?? []))
                        && (fixedSet[2] == null || fixedSet[2]!.SequenceEqual(sheetSets[k][2] ?? []))
                        && (fixedSet[3] == null || fixedSet[3]!.SequenceEqual(sheetSets[k][3] ?? []))
                    )
                    {
                        roomSheets[i] = k;
                        next = true;
                        break;
                    }
                }
            }
            if (next)
                continue;
            // FIXME: this looks terribad, can we just start with an empty collection all the time?
            while (sheetSetIndex >= sheetSets.Count)
                sheetSets.Add([]);
            sheetSets[sheetSetIndex] = fixedSet;

            roomSheets[i] = (byte)sheetSetIndex;
            ++sheetSetIndex;
        }

        // fill in all sheet sets with valid layouts for sprites
        // those first 124 (0x7C) entries are necessary for the game to work
        // (items, title screen gfx, overworld terrain, etc.)
        for (int i = 0; i < MandatorySheets; ++i)
        {
            while (sheetSets[i].Any(s => s == null))
            {
                var sprite = prng.GetRandomElement(sheetableSprites);
                if (
                    (sprite.Sheets[0] == null || sheetSets[i][0] == null)
                    && (sprite.Sheets[1] == null || sheetSets[i][1] == null)
                    && (sprite.Sheets[2] == null || sheetSets[i][2] == null)
                    && (sprite.Sheets[3] == null || sheetSets[i][3] == null)
                )
                {
                    foreach (var (idx, value) in sprite.Sheets.Indexed(v => v != null))
                        sheetSets[i][idx] = value;
                }
            }
        }

        // find all the sprites that can be placed validly with a particular sheet set.
        foreach (var sprite in sheetableSprites)
        {
            foreach (var (i, set) in sheetSets.Indexed())
            {
                if (
                    world.Config.EnemyShuffle != EnemyShuffleOption.None
                    && (sprite.Sheets[0] == null || sprite.Sheets[0]!.SequenceEqual(set[0]))
                    && (sprite.Sheets[1] == null || sprite.Sheets[1]!.SequenceEqual(set[1]))
                    && (sprite.Sheets[2] == null || sprite.Sheets[2]!.SequenceEqual(set[2]))
                    && (sprite.Sheets[3] == null || sprite.Sheets[3]!.SequenceEqual(set[3]))
                    && !sprite.Flags.HasFlag(YamlSpriteFlags.NoPlace)
                )
                {
                    sheetsToSprites[i].Add(sprite);
                }
            }
        }

        for (int i = 0; i < roomSheets.Length; i++)
        {
            if (!enemyRooms[i].Any())
            {
                roomSheets[i] = 0x00;
                continue;
            }

            if (roomSheets[i] == 0x00)
            {
                roomSheets[i] = (byte)prng.GetRandomElement(sheetsToSprites.Indexed(sprites => sprites.Count != 0)).Index;
                //do
                //{
                //    sheet = get_random_key(sheets_to_sprites);
                //} while (count(sheets_to_sprites[sheet]) == 0);
                //room_sheets[i] = sheet;
            }
            byte sheet = roomSheets[i];
            var filteredPlacable = enemyRooms[i].Where(
                e => world.Config.EnemyShuffle != EnemyShuffleOption.None
                    && !e.Sprite!.Flags.HasFlag(YamlSpriteFlags.NoPlace)
            );
            if (!filteredPlacable.Any())
                continue;

            foreach (var enemy in filteredPlacable)
            {
                Sprite newEnemy;
                if (enemy.Trophy != null)
                {
                    newEnemy = prng.GetRandomElement(sheetsToSprites[sheet].Where(sprite => sprite.Flags.HasFlag(YamlSpriteFlags.Challenge)))
                        ?? throw new Exception("Cannot find challenge enemy");
                }
                else
                {
                    newEnemy = prng.GetRandomElement(sheetsToSprites[sheet]);
                }
                _logger.LogDebug("{Location}: placing {Enemy}", enemy.Name, newEnemy.Name);
                enemy.Sprite = newEnemy;
            }
        }

        for (int i = 0; i < owSheets.Length; i++)
        {
            if (!enemyOWs[i].Any())
            {
                owSheets[i] = 0xFF;
                continue;
            }

            if (owSheets[i] == 0xFF)
            {
                owSheets[i] = (byte)prng.GetRandomElement(sheetsToSprites.Indexed(sprites => sprites.Count != 0)).Index;
                //do
                //{
                //    sheet = get_random_key(sheets_to_sprites);
                //} while (count(sheets_to_sprites[sheet]) == 0);
                //ow_sheets[i] = sheet;
            }
            byte sheet = owSheets[i];
            var filteredPlacable = enemyOWs[i].Where(
                e => world.Config.EnemyShuffle != EnemyShuffleOption.None
                    && !e.Sprite!.Flags.HasFlag(YamlSpriteFlags.NoPlace)
            );
            if (!filteredPlacable.Any())
                continue;

            foreach (var enemy in filteredPlacable)
            {
                var newEnemy = prng.GetRandomElement(sheetsToSprites[sheet]);
                _logger.LogDebug("{Location}: placing {Enemy}", enemy.Name, newEnemy.Name);
                enemy.Sprite = newEnemy;
            }
        }

        byte[] finalOverworldSheets = [
            .. owSheets[0x00..0x40], // light world rain state (0)
            .. owSheets[0x00..0x40], // light world before aga (1)
            .. owSheets[0x00..0x40], // light world after aga (2)
            .. owSheets[0x40..0x80], // dark world
        ];

        // pick random sheets where we have options
        // flatten at the end, since this is a contiguous table in ROM
        byte[] flatSheetSets = sheetSets
            .SelectMany(set => set
                .Select(sheets => sheets != null ? prng.GetRandomElement(sheets) : (byte)0))
            .ToArray();

        for (int i = 0; i < roomSheets.Length; i++)
            roomSheets[i] -= 0x40;

        world.SpriteSheets = (
            Underworld: roomSheets, // array_map(fn (s) => s - 0x40, room_sheets)
            Overworld: finalOverworldSheets,
            Sets: flatSheetSets
        );
    }
}
