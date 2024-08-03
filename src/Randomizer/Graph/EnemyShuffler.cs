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

    private sealed class EnemySprite(Sprite sprite)
    {
        public Sprite Sprite { get; } = sprite;
        public SheetSet Sheets { get; } = new SheetSet(sprite.Sheets);
    }
    private sealed class SheetSet(byte[]?[] sheets) : IEquatable<SheetSet>
    {
        private readonly byte[]?[] _sheets = sheets;

        public SheetSet() : this(new byte[4][]) { }
        public bool IsEmpty => _sheets.All(b => b == null);
        public bool IsFull => _sheets.All(b => b != null);
        public bool CanMergeWith(SheetSet other) => CanMergeWith(other._sheets);
        public bool CanMergeWith(byte[]?[]? otherSheets)
        {
            if (otherSheets == null)
                return true;

            for (int idx = 0; idx < Math.Min(_sheets.Length, otherSheets.Length); idx++)
            {
                if (_sheets[idx] == null || otherSheets[idx] == null)
                    continue;

                if (!_sheets[idx]!.Intersect(otherSheets[idx]!).Any())
                    return false;
            }

            return true;
        }
        public SheetSet Merge(SheetSet other)
        {
            var merged = new SheetSet([.. _sheets]);
            merged.Merge(other._sheets);
            return merged;
        }
        public void Merge(byte[]?[]? otherSheets)
        {
            if (otherSheets == null)
                return;

            for (int idx = 0; idx < Math.Min(_sheets.Length, otherSheets.Length); idx++)
            {
                if (otherSheets[idx] == null)
                    continue;
                if (_sheets[idx] == null)
                {
                    _sheets[idx] = otherSheets[idx];
                    continue;
                }

                _sheets[idx] = _sheets[idx]!.Intersect(otherSheets[idx]!).ToArray();
            }
        }
        public SheetSet Freeze(PRNG prng)
        {
            var finalSet = new byte[4][];
            for (int i = 0; i < finalSet.Length; i++)
            {
                byte[] options = _sheets.ElementAtOrDefault(i) ?? [];
                byte sheet = options.Length == 0 ? (byte)0 : prng.GetRandomElement(options);
                finalSet[i] = [sheet];
            }

            return new(finalSet);
        }
        public IEnumerable<byte> Flatten()
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(_sheets is null || _sheets.Length is 0 or 4, "This SheetSet is not empty, but doesn't contain exactly 4 entries. Review initialization for errors.");
            System.Diagnostics.Debug.Assert(_sheets?.Length != 4 || _sheets.All(b => b is null || b.Length == 1), "This SheetSet is not empty, but doesn't contain exactly 4 entries. Try Freezing it before use.");
#endif
            return _sheets?.SelectMany(b => b ?? []) ?? [];
        }

        public override int GetHashCode() => 0; // force Equals to be used, since the contents change over time.
        public override bool Equals(object? obj) => Equals(obj as SheetSet);
        public bool Equals(SheetSet? other)
        {
            if (other is null)
                return false;
            if (other._sheets is null)
                return _sheets is null;
            if (ReferenceEquals(other, this))
                return true;
            if (other._sheets.Length != _sheets.Length)
                return false;
            for (int i = 0; i < _sheets.Length; i++)
            {
                if (_sheets[i] is null && other._sheets[i] is null)
                    continue;
                if (_sheets[i] is null || other._sheets[i] is null)
                    return false;
                if (!_sheets[i]!.SequenceEqual(other._sheets[i]!))
                    return false;
            }
            return true;
        }
    }

    private static void UpdateSpriteSheets(World world, PRNG prng)
    {
        // a sprite sheet is a quarter of a square set. four of those are loaded at a time per screen.
        // we do one of two possible things here:
        // 1. enemies are already placed (including no enemization): read enemies, calculate sets for them.
        // 2. enemies are not placed: pick random sheets, match other enemies to them to create sets, write enemies.
        // the latter tries to maximize variability in placement (vs. placing enemies first then trying to match sets.)

        var enemyVertices = world.GetLocationsOfType(VertexType.Mob);

        var enemyRooms = enemyVertices.ToLookup(enemy => enemy.RoomId);
        var enemyOWs = enemyVertices.ToLookup(enemy => enemy.Map);

        var allEnemies = Sprite.All().Select(e => new EnemySprite(e)).ToArray();
        // Set up sprite sheets
        var sheetableSprites = allEnemies.Where(s => !s.Sheets.IsEmpty);
        // sprites that can be moved to any room as they don't have any sheet
        // requirements
        var placableSprites = allEnemies.Where(s => !s.Sprite.Flags.HasFlag(YamlSpriteFlags.NoPlace)).ToHashSet();
        var challengeSprites = placableSprites.Where(s => s.Sprite.Flags.HasFlag(YamlSpriteFlags.Challenge)).ToHashSet();

        var roomSheets = Enumerable.Range(0, 0x140).Select(_ => new SheetSet()).ToArray();
        // this is 3 times light world (rain state, zelda rescued, aga down) plus 1 times dark world
        var owSheets = Enumerable.Range(0, 4 * 0x40).Select(_ => new SheetSet()).ToArray();

        // 1. place sprites that aren't shuffled. this is either all of them, or NoPlace sprites (such as NPCs, statues and other fixed stuff)
        for (int owIdx = 0; owIdx < owSheets.Length; owIdx++)
        {
            var (mapId, state) = IndexToMapState(owIdx);
            var enemiesToPlace = enemyOWs[mapId]
                .Where(e =>
                    (e.State is null || e.State.Contains(state)) &&
                    (world.Config.EnemyShuffle == EnemyShuffleOption.None || e.Sprite?.Flags.HasFlag(YamlSpriteFlags.NoPlace) == true));

            foreach (var enemy in enemiesToPlace)
            {
                if (!owSheets[owIdx].CanMergeWith(enemy.Sprite?.Sheets))
                    throw new Exception($"Enemy '{enemy.Sprite?.Name}' does not fit on map 0x{mapId:x02}");

                owSheets[owIdx].Merge(enemy.Sprite?.Sheets);
            }
        }
        for (int roomId = 0; roomId < roomSheets.Length; roomId++)
        {
            var enemiesToPlace = enemyRooms[roomId].Where(e =>
                    (world.Config.EnemyShuffle == EnemyShuffleOption.None || e.Sprite?.Flags.HasFlag(YamlSpriteFlags.NoPlace) == true));

            foreach (var enemy in enemiesToPlace)
            {
                if (!roomSheets[roomId].CanMergeWith(enemy.Sprite?.Sheets))
                    throw new Exception($"Enemy '{enemy.Sprite?.Name}' does not fit in room 0x{roomId:x04}");

                roomSheets[roomId].Merge(enemy.Sprite?.Sheets);
            }
        }

        // in case shuffle is off, we're done. no need to worry about the actual shuffle code.
        if (world.Config.EnemyShuffle != EnemyShuffleOption.None)
        {
            // this cannot be a VertexHashSet, since the world isn't fully built yet at this point.
            var alreadyRandomized = new HashSet<Vertex>();
            for (int owIdx = 0; owIdx < owSheets.Length; owIdx++)
            {
                var (mapId, state) = IndexToMapState(owIdx);
                var enemiesToPlace = enemyOWs[mapId]
                    .Where(e =>
                        (e.State is null || e.State.Contains(state)) &&
                        (e.Sprite?.Flags.HasFlag(YamlSpriteFlags.NoPlace) == false));

                // trophy enemies first, they need to be there and we want variance.
                foreach (var enemy in enemiesToPlace.OrderByDescending(e => e.Trophy != null))
                {
                    if (!alreadyRandomized.Add(enemy))
                    {
                        owSheets[owIdx].Merge(enemy.Sprite!.Sheets);
                        continue;
                    }

                    // 2. build a list of suitable sprites based on what the room currently has in the sheet set
                    IEnumerable<EnemySprite> spriteSource = enemy.Trophy == null ? placableSprites : challengeSprites;
                    if (enemy.Item != null)
                        spriteSource = spriteSource.Where(e => !e.Sprite.Flags.HasFlag(YamlSpriteFlags.NoDrop));
                    var viableSprites = spriteSource.Where(e => owSheets[owIdx].CanMergeWith(e.Sprite?.Sheets)).ToArray();
                    if (viableSprites.Length == 0)
                        throw new Exception($"Cannot find a replacement for '{enemy.Sprite?.Name}' that fits on map 0x{mapId:x02}");

                    // 3. pick possible enemies from that list
                    var newEnemy = prng.GetRandomElement(viableSprites);
                    owSheets[owIdx] = owSheets[owIdx].Merge(newEnemy.Sheets);
                    _logger.LogInformation("{Location}: Placing {NewEnemy}", enemy.Name, newEnemy.Sprite.Name);
                    enemy.Sprite = newEnemy.Sprite;
                }
            }
            for (int roomId = 0; roomId < roomSheets.Length; roomId++)
            {
                var enemiesToPlace = enemyRooms[roomId].Where(e =>
                        (e.Sprite?.Flags.HasFlag(YamlSpriteFlags.NoPlace) == false));

                // trophy enemies first, they need to be there and we want variance.
                foreach (var enemy in enemiesToPlace.OrderByDescending(e => e.Trophy != null))
                {
                    if (!alreadyRandomized.Add(enemy))
                    {
                        roomSheets[roomId].Merge(enemy.Sprite!.Sheets);
                        continue;
                    }

                    // 2. build a list of suitable sprites based on what the room currently has in the sheet set
                    IEnumerable<EnemySprite> spriteSource = enemy.Trophy == null ? placableSprites : challengeSprites;
                    if (enemy.Item != null)
                        spriteSource = spriteSource.Where(e => !e.Sprite.Flags.HasFlag(YamlSpriteFlags.NoDrop));
                    var viableSprites = spriteSource.Where(e => !e.Sprite.Flags.HasFlag(YamlSpriteFlags.OverworldOnly) && roomSheets[roomId].CanMergeWith(e.Sprite?.Sheets)).ToArray();
                    if (viableSprites.Length == 0)
                        throw new Exception($"Cannot find a replacement for '{enemy.Sprite?.Name}' that fits in room 0x{roomId:x04}");

                    // 3. pick possible enemies from that list
                    var newEnemy = prng.GetRandomElement(viableSprites);
                    roomSheets[roomId] = roomSheets[roomId].Merge(newEnemy.Sheets);
                    _logger.LogInformation("{Location}: Placing {NewEnemy}", enemy.Name, newEnemy.Sprite.Name);
                    enemy.Sprite = newEnemy.Sprite;
                }
            }
        }

        // sprite sheets have 3 major locations:
        // 1. underworld:
        //    - vanilla room headers (room pointer tables plus room data, OAM and sprites in the room) 
        //    - randomizer room headers (sprite sheet)
        // 2. overworld:
        //    - vanilla map headers (map pointer tables per state plus map data, sprites on the map)
        //    - vanilla sprite sets (table, 1 sheet set per map)
        // 3. sheet set table (4 sheets each, referenced by the other two)
        //
        // based on room/map sprites, matching sheets need to be loaded.
        // 4 sheets are grouped into a sheet set and referenced by the room/map.
        //
        // to get there, we'll build a matching sheet set per room/map,
        // then find unique ones and write them to the set table.
        // for every usage, we refer to an entry in this table.
        //
        // however, because enemization put too many constraints on how enemies could be placed,
        // the randomizer moved the underworld room headers and extended them to directly contain sprite sheet ids.
        // rather than cramming them into the sheet set table at the end, we just write them directly to the room.

        // attempt to consolidate sheets first. not every sheet uses every slot, so we can combine those to save space.
        bool mergedSomething = true;
        while (mergedSomething)
        {
            mergedSomething = false;
            for (int i = 0; i < roomSheets.Length; i++)
            {
                var thisSheet = roomSheets[i];
                if (thisSheet.IsEmpty || thisSheet.IsFull)
                    continue;
                var matchingSheets = roomSheets.Where(s => !s.IsEmpty && s != thisSheet && !s.Equals(thisSheet) && s.CanMergeWith(thisSheet)).ToArray();
                if (matchingSheets.Length > 0)
                {
                    var mergeThat = prng.GetRandomElement(matchingSheets);
                    foreach (var (index, _) in roomSheets.Indexed(s => s.Equals(thisSheet) || s.Equals(mergeThat)))
                        roomSheets[index] = thisSheet.Merge(mergeThat);
                    mergedSomething = true;
                }
            }
        }

        // freeze the remaining values to lock in slots that still have choices left.
        SheetSet[] overworldSheets = owSheets.Select(s => s.Freeze(prng)).ToArray();
        SheetSet[] underworldSheets = roomSheets.Select(s => s.Freeze(prng)).ToArray();

        // build a list of unique sheet sets. overworld only (vanilla storage,) we write underworld directly to the room header (rando-specific.)
        var uniqueSheets = overworldSheets.Distinct().ToList();

        // grab indices for the map headers from that set.
        byte[] mapSheetBytes = overworldSheets.Select(s => (byte)uniqueSheets.IndexOf(s)).ToArray();

        // flatten at the end, since this is a contiguous table in ROM
        byte[] flatSheetSets = uniqueSheets.SelectMany(s => s.Flatten()).ToArray();
        byte[] roomSheetBytes4 = underworldSheets.SelectMany(s => s.Flatten()).ToArray();

        world.SpriteSheets = (
            Underworld: roomSheetBytes4,
            Overworld: mapSheetBytes,
            Sets: flatSheetSets
        );

        static (int MapId, int State) IndexToMapState(int owIdx)
        {
            int mapId = owIdx % 0x40;
            int state = owIdx / 0x40;
            if (state == 3)
            {
                // this is considered dark world, up the map id and just pick a random state (we don't really switch sprites between pre-aga/post-aga there)
                mapId += 0x40;
                state = 2;
            }

            return (mapId, state);
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
    private static void UpdateSpriteSheetsPHP(World world, PRNG prng)
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

        var roomSheets = new byte[0x140];
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
