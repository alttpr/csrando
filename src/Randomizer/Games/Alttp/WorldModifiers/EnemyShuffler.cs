using Microsoft.Extensions.Logging;
using Randomizer.Graph;

namespace Randomizer.Games.Alttp.WorldModifiers;

/// <summary>
/// Modify the edges of the graph to shuffle entrances.
/// </summary>
internal sealed class EnemyShuffler : IAlttpWorldModifier
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
    public void AdjustEdges(World world, PRNG prng)
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
                if (edge.To is not Vertex { Sprite: Sprite sprite })
                    continue;

                var item = world.GetExistingItem($"Defeat{sprite.DefeatName}");
                if (item != null)
                    edge.Condition = new ItemCondition(item, 1);
            }
        }
    }

    private static void CreateBosses(World world)
    {
        var bosses = YamlReader.LoadBosses();
        var enemiesRoot = world.GetLocation("Enemies");

        {
            // Ganon
            var fight = new Vertex
            {
                Type = VertexType.Meta,
                Name = "FightGanon",
                World = world,
            };
            world.Graph.AddVertex(fight);
            enemiesRoot.Edges.Add(new Edge(enemiesRoot, fight,
                new ItemCondition(world.GetItem("GanonVulnerable"), 1)));

            var phaseOne = new Vertex
            {
                Type = VertexType.Meta,
                Name = "GanonPhaseOneComplete",
                World = world,
            };
            world.Graph.AddVertex(phaseOne);
            foreach (string item in bosses["Ganon"])
                fight.Edges.Add(new Edge(fight, phaseOne, new ItemCondition(world.GetItem(item), 1)));

            var phaseTwo = new Vertex
            {
                Type = VertexType.Meta,
                Name = "GanonPhaseTwoComplete",
                World = world,
            };
            world.Graph.AddVertex(phaseTwo);
            foreach (string item in bosses["Ganon"])
                phaseOne.Edges.Add(new Edge(phaseOne, phaseTwo, new ItemCondition(world.GetItem(item), 1)));

            var phaseThree = new Vertex
            {
                Type = VertexType.Meta,
                Name = "GanonPhaseThreeComplete",
                World = world,
            };
            world.Graph.AddVertex(phaseThree);
            foreach (string item in bosses["Ganon"])
                phaseTwo.Edges.Add(new Edge(phaseTwo, phaseThree, new ItemCondition(world.GetItem(item), 1)));

            var torchesLit = new Vertex
            {
                Type = VertexType.Meta,
                Name = "GanonTorchesLit",
                World = world,
            };
            world.Graph.AddVertex(torchesLit);
            foreach (string item in bosses["GanonLightTorches"])
                phaseThree.Edges.Add(new Edge(phaseThree, torchesLit, new ItemCondition(world.GetItem(item), 1)));

            var defeated = new Vertex
            {
                Type = VertexType.Meta,
                Name = "DefeatGanon",
                World = world,
                Item = world.GetItem("DefeatGanon"),
            };
            world.Graph.AddVertex(defeated);
            foreach (string item in bosses["GanonInvisible"])
                torchesLit.Edges.Add(new Edge(torchesLit, defeated, new ItemCondition(world.GetItem(item), 1)));

            var darkDefeated = new Vertex
            {
                Type = VertexType.Meta,
                Name = "DarkDefeatGanon",
                World = world,
                Item = world.GetItem("DarkDefeatGanon"),
            };
            world.Graph.AddVertex(darkDefeated);
            defeated.Edges.Add(new Edge(defeated, darkDefeated,
                new ItemCondition(world.GetItem("MoonPearl"), 1)));
        }

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
                foreach (string item in bosses["Helmasaur"])
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
                foreach (string item in bosses["Helmasaur"])
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
                foreach (string item in bosses["Arrghus"])
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
                foreach (string item in bosses["Arrghus"])
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
                foreach (string item in bosses["KholdstareShell"])
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
                foreach (string item in bosses["Kholdstare"])
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
                foreach (string item in bosses["KholdstareShell"])
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
                foreach (string item in bosses["Kholdstare"])
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
                foreach (string item in bosses["Trinexx"])
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
                foreach (string item in bosses["Trinexx"])
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
    [System.Diagnostics.DebuggerDisplay("[ {_sheets[0]}, {_sheets[1]}, {_sheets[2]}, {_sheets[3]} ]")]
    private sealed class SheetSet(byte?[] sheets) : IEquatable<SheetSet>
    {
        private readonly byte?[] _sheets = sheets;

        public SheetSet() : this(new byte?[4]) { }
        public bool IsEmpty => _sheets.All(b => b == null);
        public bool IsFull => _sheets.All(b => b != null);
        public bool CanMergeWith(SheetSet other)
        {
            return CanMergeWith(other._sheets);
        }

        public bool CanMergeWith(byte?[]? otherSheets)
        {
            if (otherSheets == null)
                return true;

            for (int idx = 0; idx < Math.Min(_sheets.Length, otherSheets.Length); idx++)
            {
                if (_sheets[idx] == null || otherSheets[idx] == null)
                    continue;

                if (_sheets[idx] != otherSheets[idx])
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
        public void Merge(byte?[]? otherSheets)
        {
            if (otherSheets == null)
                return;

            for (int idx = 0; idx < Math.Min(_sheets.Length, otherSheets.Length); idx++)
            {
                if (otherSheets[idx] == null)
                    continue;
                _sheets[idx] = otherSheets[idx];
            }
        }
        public SheetSet Freeze(PRNG prng)
        {
            byte?[] finalSet = new byte?[4];
            for (int i = 0; i < finalSet.Length; i++)
            {
                byte? option = _sheets.ElementAtOrDefault(i) ?? null;
                byte sheet = option.GetValueOrDefault();
                finalSet[i] = sheet;
            }

            return new(finalSet);
        }
        public IEnumerable<byte> Flatten()
        {
#if DEBUG
            System.Diagnostics.Debug.Assert(_sheets is null || _sheets.Length is 0 or 4, "This SheetSet is not empty, but doesn't contain exactly 4 entries. Review initialization for errors.");
            System.Diagnostics.Debug.Assert(_sheets?.Length != 4 || _sheets.All(b => b is not null), "This SheetSet is not empty, but doesn't contain exactly 4 entries. Try Freezing it before use.");
#endif
            return _sheets?.Select(b => b.GetValueOrDefault()) ?? [];
        }

        public override int GetHashCode()
        {
            return 0; // force Equals to be used, since the contents change over time.
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as SheetSet);
        }

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
                if (_sheets[i] != other._sheets[i])
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
        var regionVertices = world.GetLocationsOfType(VertexType.Region).ToArray();
        var roomVertices = regionVertices.Where(v => v.RoomId.HasValue).GroupBy(v => v.RoomId).Select(v => (Key: v.Key!.Value, Value: v.First())).ToDictionary(k => k.Key, v => v.Value);
        var mapVertices = regionVertices.Where(v => v.Map.HasValue).GroupBy(v => v.Map).Select(v => (Key: v.Key!.Value, Value: v.First())).ToDictionary(k => k.Key, v => v.Value);
        var enemyVertices = world.GetLocationsOfType(VertexType.Mob);

        // remove the maiden if blind isn't in thieves town.
        var thievesTownBoss = world.GetLocation("Thieves' Town - Boss Room - Blind Active");
        var blindBossEdge = thievesTownBoss.Edges.FirstOrDefault(e => e.Condition.Item?.Name == "DefeatBlind");
        if (blindBossEdge == null)
            enemyVertices = enemyVertices.Where(v => v.Sprite?.Name != "BlindMaiden");

        var enemyRooms = enemyVertices.ToLookup(enemy => enemy.RoomId);
        var enemyOWs = enemyVertices.ToLookup(enemy => enemy.Map);

        var allEnemies = Sprite.All().Select(e => new EnemySprite(e)).ToArray();
        // Set up sprite sheets
        var sheetableSprites = allEnemies.Where(s => !s.Sheets.IsEmpty);
        // sprites that can be moved to any room as they don't have any sheet
        // requirements. ignoring subtype sprites since they are only variants.
        var placableSprites = allEnemies.Where(s => !s.Sprite.Flags.HasFlag(YamlSpriteFlags.NoPlace) && s.Sprite.SubType == 0x00).ToHashSet();
        // falling sprites sometimes have additional sheet requirements (including a falling sprite)
        // those need to be selected for rooms that have pits, and might limit the rest of the sprites that can go there.
        var fallingSprites = new Dictionary<EnemySprite, EnemySprite>();
        foreach (var fallingSprite in placableSprites.Where(s => !string.IsNullOrEmpty(s.Sprite.FallingSpriteFor)))
        {
            // this intentionally throws when no matching sprite is found. this is a data issue!
            var nonFallingSprite = allEnemies.First(e => e.Sprite.Name == fallingSprite.Sprite.FallingSpriteFor);
            fallingSprites[nonFallingSprite] = fallingSprite;
        }
        placableSprites.RemoveWhere(e => !string.IsNullOrEmpty(e.Sprite.FallingSpriteFor));
        var challengeSprites = placableSprites.Where(s => s.Sprite.Flags.HasFlag(YamlSpriteFlags.Challenge)).ToHashSet();

        var roomSheets = Enumerable.Range(0, 0x140).Select(roomId =>
        {
            if (roomVertices.TryGetValue(roomId, out var roomVertex) && roomVertex?.Sheets is not null)
                return new SheetSet((byte?[])roomVertex.Sheets.Clone());
            return new SheetSet();
        }).ToArray();
        // this is 3 times light world (rain state, zelda rescued, aga down) plus 1 times dark world
        // then +2 for the special overworld (master sword grove/hobo bridge and zoras domain)
        var owSheets = Enumerable.Range(0, 4 * 0x40 + 2).Select(owIdx =>
        {
            var (mapId, _) = IndexToMapState(owIdx);
            if (mapVertices.TryGetValue(mapId, out var mapVertex) && mapVertex?.Sheets is not null)
                return new SheetSet((byte?[])mapVertex.Sheets.Clone());
            return new SheetSet();
        }).ToArray();

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
                    world.Config.EnemyShuffle == EnemyShuffleOption.None || e.Sprite?.Flags.HasFlag(YamlSpriteFlags.NoPlace) == true);

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
            // rooms that have sprites we need to account for. commonly, that's pots/skulls but also large blocks.
            // they affect the available room budget and reduce it, attempting to stay within the underworld sprite limit.
            var roomsWithExtraSprites = world.GetLocationsOfType(VertexType.Pot).Where(v => v.RoomId.HasValue).Select(v => v.RoomId!.Value).ToHashSet();
            roomsWithExtraSprites.UnionWith([0x3f, 0x44, 0x45, 0x93, 0xce, 0x117]); // large blocks

            void place(
                SheetSet[] sheets,
                Func<int, IEnumerable<Vertex>> enemiesAtIndex,
                Func<IEnumerable<EnemySprite>, IEnumerable<EnemySprite>> adjustCandidates,
                Func<int, Vertex, string> buildFailureMessage,
                Func<int, int> roomBudgetReduction,
                int roomBudget = 0xFF)
            {
                for (int idx = 0; idx < sheets.Length; idx++)
                {
                    var enemiesInRoom = enemiesAtIndex(idx).ToArray();
                    var enemiesToPlace = enemiesInRoom.Where(e => e.Sprite?.Flags.HasFlag(YamlSpriteFlags.NoPlace) == false);
                    var roomDeny = enemiesInRoom.Except(enemiesToPlace).Select(e => e.Sprite?.NotWith).Where(a => a is not null).SelectMany(a => a!).ToHashSet();

                    // things go wrong if more than 16 sprites are on screen at the same time (at least for underworld).
                    int budget = roomBudget - enemiesInRoom.Except(enemiesToPlace).Sum(e => e.Sprite!.Weight) - roomBudgetReduction(idx);
                    // assume we place regular weighted enemies to get the minimum required budget.
                    int minimumBudgetRequired = enemiesToPlace.Count();

                    // trophy enemies first, they need to be there and we want variance.
                    // after that, limited locations first (allow/deny lists) to make sure we don't fill the sheet with incompatible stuff.
                    foreach (var enemy in enemiesToPlace.OrderByDescending(e => e.Trophy != null).ThenByDescending(e => e.Allow?.Length > 0).ThenByDescending(e => e.Deny?.Length > 0))
                    {
                        if (!alreadyRandomized.Add(enemy))
                        {
                            sheets[idx].Merge(enemy.Sprite!.Sheets);
                            continue;
                        }

                        // 2. build a list of suitable sprites based on what the room currently has in the sheet set
                        IEnumerable<EnemySprite> spriteSource = enemy.Trophy == null ? placableSprites : challengeSprites;
                        int availableBudget = budget - minimumBudgetRequired + 1;
                        spriteSource = spriteSource.Where(s => s.Sprite.Weight <= availableBudget);
                        if (enemy.Item != null)
                            spriteSource = spriteSource.Where(e => !e.Sprite.Flags.HasFlag(YamlSpriteFlags.NoDrop));
                        if (enemy.MightFall)
                            spriteSource = spriteSource.Select(e => fallingSprites.GetValueOrDefault(e, e));
                        spriteSource = adjustCandidates(spriteSource);
                        if (roomDeny.Count > 0)
                            spriteSource = spriteSource.Where(e => !roomDeny.Contains(e.Sprite.Name));

                        var viableSprites = spriteSource.Where(e => isAllowed(enemy, e) && !isDenied(enemy, e) && sheets[idx].CanMergeWith(e.Sprite?.Sheets)).ToArray();
                        if (viableSprites.Length == 0)
                            throw new Exception(buildFailureMessage(idx, enemy));

                        // 3. pick possible enemies from that list
                        var newEnemy = prng.GetRandomElement(viableSprites);
                        sheets[idx] = sheets[idx].Merge(newEnemy.Sheets);
                        _logger.LogInformation("{Location}: Placing {NewEnemy} (budget {RemainingBudget}, minimum {MinimumBudgetRequired}, weight {BudgetDeduction})", enemy.Name, newEnemy.Sprite.Name, budget, minimumBudgetRequired, newEnemy.Sprite.Weight);
                        enemy.Sprite = newEnemy.Sprite;
                        budget -= newEnemy.Sprite.Weight;
                        minimumBudgetRequired--;
                        if (newEnemy.Sprite.NotWith is { } denies)
                            roomDeny.UnionWith(denies);
                    }
                }
            }

            place(
                owSheets,
                owIdx =>
                {
                    var (mapId, state) = IndexToMapState(owIdx);
                    return enemyOWs[mapId].Where(e => e.State is null || e.State.Contains(state));
                },
                candidates => candidates,
                (idx, enemy) =>
                {
                    var (mapId, _) = IndexToMapState(idx);
                    return $"Cannot find a replacement for '{enemy.Sprite?.Name}' that fits on map 0x{mapId:x02}";
                },
                // overworld has a lot more forgiving limits due to how it spawns sprites, so we can skip most of this.
                idx => 0);
            place(
                roomSheets,
                roomId => enemyRooms[roomId],
                candidates => candidates.Where(s => !s.Sprite.Flags.HasFlag(YamlSpriteFlags.OverworldOnly)),
                (idx, enemy) => $"Cannot find a replacement for '{enemy.Sprite?.Name}' that fits in room 0x{idx:x04}",
                idx => roomsWithExtraSprites.Contains(idx) ? 1 : 0,
                // underworld has a sprite limit of about 16, we should try to stay below that.
                roomBudget: 16);
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
        var overworldSheets = owSheets.Select(s => s.Freeze(prng)).ToArray();
        // build a list of unique sheet sets. overworld only (vanilla storage), we write underworld directly to the room header (rando-specific).
        var uniqueSheets = overworldSheets.Distinct().ToList();

        // the last two are special overworld (master sword grove/hobo bridge and zoras domain) which go in a different location.
        var specialOverworldSheets = overworldSheets[^2..];
        overworldSheets = overworldSheets[..^2];
        var underworldSheets = roomSheets.Select(s => s.Freeze(prng)).ToArray();

        // grab indices for the map headers from that set.
        byte[] mapSheetBytes = overworldSheets.Select(s => (byte)uniqueSheets.IndexOf(s)).ToArray();
        byte[] specialSheetBytes = specialOverworldSheets.Select(s => (byte)uniqueSheets.IndexOf(s)).ToArray();

        specialSheetBytes = [
            specialSheetBytes[0], specialSheetBytes[0], specialSheetBytes[1], specialSheetBytes[1],
            0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF,
        ];

        // flatten at the end, since this is a contiguous table in ROM
        byte[] flatSheetSets = uniqueSheets.SelectMany(s => s.Flatten()).ToArray();
        byte[] roomSheetBytes4 = underworldSheets.SelectMany(s => s.Flatten()).ToArray();

        world.SpriteSheets = (
            Underworld: roomSheetBytes4,
            Overworld: mapSheetBytes,
            Special: specialSheetBytes,
            Sets: flatSheetSets
        );

        static (int MapId, int State) IndexToMapState(int owIdx)
        {
            int mapId = owIdx % 0x40;
            int state = owIdx / 0x40;
            if (state >= 3)
            {
                // this is considered dark world, up the map id and just pick a random state (we don't really switch sprites between pre-aga/post-aga there)
                // we also abuse this to get the special maps at 0x80 and beyond
                mapId += 0x40 * (state - 2);
                state = 2;
            }

            return (mapId, state);
        }
        static bool isAllowed(Vertex target, EnemySprite sprite) => target.Allow is null || target.Allow.Length == 0 || target.Allow.Contains(sprite.Sprite.DefeatName);
        static bool isDenied(Vertex target, EnemySprite sprite) => target.Deny is not null && target.Deny.Length > 0 && target.Deny.Contains(sprite.Sprite.DefeatName);
    }
}
