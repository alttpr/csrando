using Randomizer.Graph;

namespace Randomizer.Games.Alttp;


internal static class DataLoader
{
    /// <summary>
    /// Fills the passed <paramref name="world"/> with a graph describing its configuration.
    /// </summary>
    public static void Fill(World world)
    {
        var graph = world.Graph;

        foreach (var vertex in LoadVertices(world))
        {
            graph.AddVertex(vertex);
        }

        var edges = LoadEdges(world);
        foreach (var (condition, data) in edges)
        {
            foreach (var edgeData in data.Directed)
            {
                var from = world.GetLocation(edgeData[0]);
                var to = world.GetLocation(edgeData[1]);
                if (from is null || to is null)
                {
                    throw new Exception(
                        "Name Connection Mismatch: " +
                        $"({edgeData[0]}, {edgeData[1]}) => " +
                        $"({from}, {to})");
                }
                graph.AddDirected(from, to, condition);
            }
        }

        PruneConfigEdges(world);
    }

    private static void PruneConfigEdges(World world)
    {
        foreach (var v in world.GetLocations())
        {
            v.Edges = v.Edges.Where(e => !e.Condition.Item.Name.StartsWith("ConfigWorld") || world.StartingItems.Has(e.Condition.Item)).ToList();
        }
    }

    private static ItemCondition ConditionFrom(World world, string condition)
    {
        var conditionSplit = condition.Split("|");
        var itemCount = 1;

        if (conditionSplit.Length > 1)
            itemCount = int.Parse(conditionSplit[1]);

        return new ItemCondition(world.GetItem(conditionSplit[0]), itemCount);
    }

    // NOTE: This does not account for door rando.
    // TODO: world should likely be data rather than code.
    private static readonly HashSet<string> BUNNY_REVIVE =
    [
        "Eastern Palace - Entrance",
        "Desert Palace - Main Room - Center",
        "Desert Palace - Right Entrance",
        "Desert Palace - Beemos Torches",
        "Desert Palace - Beemos 2",
        "Agahnims Tower - Entrance",
        "Palace of Darkness - Lobby",
        "Skull Woods - Main Entrance",
        "Skull Woods - Pinball Room",
        "Skull Woods - Firebar Pits",
        "Skull Woods - Statue Puzzle",
        "Skull Woods - Bumper Buddy",
        "Skull Woods - Bridge Room",
        "Thieves' Town - Grand Room SW",
        "Ice Palace - Entrance",
        "Misery Mire - Entrance",
        "Turtle Rock - Big Chest Entrance",
        "Turtle Rock - Laser Entrance",
        "Turtle Rock - Eye Bridge",
        "Ganon's Tower - Lobby",
    ];

    /// <summary>
    /// Get all vertices for a world and map static items to that world. Also
    /// given the world config, we may invert the moonpearl requirements here.
    /// </summary>
    /// <param name="world">world to attach preset items to</param>
    private static IEnumerable<Vertex> LoadVertices(World world)
    {
        var vertexData = YamlReader.LoadVertices();
        var structuredVertices = new Dictionary<string, Vertex>();
        var fixedCondition = new ItemCondition(world.GetItem("fixed"), 1);
        var pendingConnections = new List<(Vertex, string, ItemCondition)>();

        bool moonPearlTransform(bool moonpearl, string name)
        {
            bool result = moonpearl;

            if (world.Config.State == StateOption.Inverted)
                result = !moonpearl;

            if (world.Config.Techs != null && world.Config.Techs.Contains(TechOption.DungeonBunnyRevival) && BUNNY_REVIVE.Contains(name))
                result = false;

            return result;
        }

        // overworld
        foreach (var map in vertexData.Maps)
        {
            var shared = new Dictionary<string, object>()
            {
                { "map", map.MapMap },
            };
            foreach (var meta in map.Nodes.Meta)
            {
                var metaVertex = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = meta.Name,
                    World = world,
                };
                structuredVertices.Add(meta.Name, metaVertex);

                foreach (var connection in meta.Connections)
                {
                    foreach (var target in connection.Value)
                    {
                        pendingConnections.Add((metaVertex, target, ConditionFrom(world, connection.Key)));
                    }
                }

                foreach (var (item, index) in meta.Items.Select((v, i) => (v, i)))
                {
                    var metaItemVertex = new Vertex
                    {
                        Type = VertexType.Meta,
                        Name = $"{meta.Name} - {index} - {item}",
                        World = world,
                        Item = world.GetItem(item),
                    };
                    structuredVertices.Add(metaItemVertex.Name, metaItemVertex);
                    metaVertex.Edges.Add(new Edge(metaVertex, metaItemVertex, fixedCondition));
                }
            }
            foreach (var prizepack in map.Nodes.Prizepacks)
            {
                structuredVertices.Add(prizepack.Name, new Vertex
                {
                    Type = VertexType.PrizePack,
                    Name = prizepack.Name,
                    World = world,
                    Addresses = prizepack.Addresses,
                    Sprite = Sprite.Get(prizepack.Sprite),
                    Deny = prizepack.Deny.ToArray(),
                    Allow = prizepack.Allow.ToArray(),
                });
            }
            foreach (var region in map.Nodes.Regions)
            {
                var regionVertex = new Vertex
                {
                    Type = region.Type ?? VertexType.Region,
                    Name = region.Name,
                    World = world,
                    Map = map.MapMap,
                    Sheets = map.Sheets,
                    InletId = region.InletId,
                    Shopkeeper = region.Shopkeeper,
                    ShopStyle = region.Shopstyle,
                    Switch = region.Switch ?? false,
                    MoonPearl = moonPearlTransform(map.Moonpearl, region.Name),
                };
                structuredVertices.Add(region.Name, regionVertex);

                foreach (var mob in region.Mobs)
                {
                    var mobVertex = new Vertex
                    {
                        Type = VertexType.Mob,
                        Name = mob.Name,
                        World = world,
                        Map = map.MapMap,
                        Sprite = Sprite.Get(mob.Sprite),
                        Item = world.GetItemOrNull(mob.Item),
                        State = mob.State.ToArray(),
                        ItemSet = mob.ItemSet.Select(v => new ItemSetName(v, world)).ToArray(),
                        Trophy = world.GetItemOrNull(mob.Trophy),
                        Position = mob.Position,
                        Deny = mob.Deny.ToArray(),
                        Allow = mob.Allow.ToArray(),
                        MightFall = region.Pit,
                    };
                    structuredVertices.Add(mob.Name, mobVertex);
                    regionVertex.Edges.Add(new Edge(regionVertex, mobVertex, fixedCondition));
                }

                foreach (var entrance in region.Entrances)
                {
                    // TODO: the old code had conditional access to entranceid and outletid; are there entrances without them?
                    string nameIn = $"{entrance.Name} - In";
                    string nameOut = $"{entrance.Name} - Out";
                    var entranceInVertex = new Vertex
                    {
                        Type = VertexType.Entrance,
                        Name = nameIn,
                        World = world,
                        Map = map.MapMap,
                        EntranceId = entrance.EntranceId,
                        MoonPearl = regionVertex.MoonPearl,
                    };
                    structuredVertices.Add(nameIn, entranceInVertex);
                    var entranceOutVertex = new Vertex
                    {
                        Type = VertexType.Outlet,
                        Name = nameOut,
                        World = world,
                        Map = map.MapMap,
                        OutletId = entrance.OutletId,
                        MoonPearl = regionVertex.MoonPearl,
                    };
                    structuredVertices.Add(nameOut, entranceOutVertex);
                    foreach (var condition in entrance.Conditions.DefaultIfEmpty("fixed"))
                    {
                        regionVertex.Edges.Add(new Edge(regionVertex, entranceInVertex, ConditionFrom(world, condition)));
                    }
                    entranceOutVertex.Edges.Add(new Edge(entranceOutVertex, regionVertex, fixedCondition));
                }

                foreach (var item in region.Items)
                {
                    var itemVertex = new Vertex
                    {
                        Type = VertexType.Item,
                        SubType = item.Type,
                        Name = item.Name,
                        World = world,
                        Map = map.MapMap,
                        Item = world.GetItemOrNull(item.Item),
                        ItemSet = item.ItemSet.Select(v => new ItemSetName(v, world)).ToArray(),
                        Addresses = item.Addresses.ToArray(),
                    };
                    structuredVertices.Add(item.Name, itemVertex);
                    foreach (var condition in item.Conditions.DefaultIfEmpty("fixed"))
                    {
                        regionVertex.Edges.Add(new Edge(regionVertex, itemVertex, ConditionFrom(world, condition)));
                    }
                }

                foreach (var hole in region.Holes)
                {
                    var holeVertex = new Vertex
                    {
                        Type = VertexType.Hole,
                        Name = hole.Name,
                        World = world,
                        Map = map.MapMap,
                        EntranceIds = hole.EntranceIds.ToArray(),
                        MoonPearl = regionVertex.MoonPearl,
                    };
                    structuredVertices.Add(hole.Name, holeVertex);
                    foreach (var condition in hole.Conditions.DefaultIfEmpty("fixed"))
                    {
                        regionVertex.Edges.Add(new Edge(regionVertex, holeVertex, ConditionFrom(world, condition)));
                    }
                }

                foreach (var warp in region.Warps)
                {
                    var warpVertex = new Vertex
                    {
                        Type = VertexType.Warp,
                        Name = warp.Name,
                        World = world,
                        Map = map.MapMap,
                        Position = warp.Position,
                        MoonPearl = moonPearlTransform(map.Moonpearl, warp.Name),
                    };
                    structuredVertices.Add(warp.Name, warpVertex);
                    regionVertex.Edges.Add(new Edge(regionVertex, warpVertex, fixedCondition));
                    warpVertex.Edges.Add(new Edge(warpVertex, regionVertex, fixedCondition));

                    foreach (var connection in warp.Connections)
                    {
                        foreach (var target in connection.Value)
                        {
                            pendingConnections.Add((warpVertex, target, ConditionFrom(world, connection.Key)));
                        }
                    }
                }

                foreach (var connection in region.Connections)
                {
                    foreach (var target in connection.Value)
                    {
                        pendingConnections.Add((regionVertex, target, ConditionFrom(world, connection.Key)));
                    }
                }
            }
        }

        // underworld
        foreach (var room in vertexData.Rooms)
        {
            foreach (var region in room.Nodes.Regions)
            {
                Vertex regionVertex;
                regionVertex = new Vertex
                {
                    Type = region.Type ?? VertexType.Region,
                    Name = region.Name,
                    World = world,
                    RoomId = room.Roomid,
                    RoomOAM = room.OAM,
                    Group = room.Group.GetValueOrDefault(0),
                    Dark = room.Dark,
                    Sheets = room.Sheets,
                    ExtraLight = room.ExtraLight,
                    InletId = region.InletId,
                    Shopkeeper = region.Shopkeeper,
                    ShopStyle = region.Shopstyle,
                    Switch = region.Switch ?? false,
                };
                structuredVertices.Add(region.Name, regionVertex);

                if (region.InletId.HasValue)
                {
                    string nameExit = $"{region.Name} - Exit";
                    var exitVertex = new Vertex
                    {
                        Type = VertexType.Entrance,
                        Name = nameExit,
                        World = world,
                        RoomId = room.Roomid,
                        Group = room.Group.GetValueOrDefault(0),
                        InletId = region.InletId,
                    };
                    structuredVertices.Add(nameExit, exitVertex);
                    regionVertex.Edges.Add(new Edge(regionVertex, exitVertex, fixedCondition));
                }

                foreach (var mob in region.Mobs)
                {
                    var mobVertex = new Vertex
                    {
                        Type = VertexType.Mob,
                        Name = mob.Name,
                        World = world,
                        RoomId = room.Roomid,
                        RoomOAM = room.OAM,
                        Group = room.Group.GetValueOrDefault(0),
                        Sprite = Sprite.Get(mob.Sprite),
                        Item = world.GetItemOrNull(mob.Item),
                        State = mob.State.ToArray(),
                        ItemSet = mob.ItemSet.Select(v => new ItemSetName(v, world)).ToArray(),
                        Trophy = world.GetItemOrNull(mob.Trophy),
                        Position = mob.Position,
                        Deny = mob.Deny.ToArray(),
                        Allow = mob.Allow.ToArray(),
                        MightFall = region.Pit,
                    };
                    structuredVertices.Add(mob.Name, mobVertex);
                    regionVertex.Edges.Add(new Edge(regionVertex, mobVertex, fixedCondition));
                }

                if (region.Entrances.Count != 0)
                    throw new Exception("Found old style entrance in underworld region node.");

                foreach (var item in region.Items)
                {
                    var itemVertex = new Vertex
                    {
                        Type = VertexType.Item,
                        SubType = item.Type,
                        Name = item.Name,
                        World = world,
                        RoomId = room.Roomid,
                        Group = room.Group.GetValueOrDefault(0),
                        Item = world.GetItemOrNull(item.Item),
                        ItemSet = item.ItemSet.Select(v => new ItemSetName(v, world)).ToArray(),
                        Addresses = item.Addresses.ToArray(),
                    };
                    structuredVertices.Add(item.Name, itemVertex);

                    foreach (var condition in item.Conditions.DefaultIfEmpty("fixed"))
                    {
                        regionVertex.Edges.Add(new Edge(regionVertex, itemVertex, ConditionFrom(world, condition)));
                    }
                }

                foreach (var item in region.Inventory)
                {
                    var inventoryVertex = new Vertex
                    {
                        Type = item.Type,
                        Name = item.Name,
                        World = world,
                        RoomId = room.Roomid,
                        Group = room.Group.GetValueOrDefault(0),
                        Item = world.GetItemOrNull(item.Item),
                        Cost = item.Cost,
                        ItemSet = item.ItemSet.Select(v => new ItemSetName(v, world)).ToArray(),
                    };
                    structuredVertices.Add(item.Name, inventoryVertex);
                    regionVertex.Edges.Add(new Edge(regionVertex, inventoryVertex, new ItemCondition(world.GetItem("BuyItem"), 1)));
                }

                foreach (var pot in region.Pots)
                {
                    var potVertex = new Vertex
                    {
                        Type = VertexType.Pot,
                        Name = pot.Name,
                        World = world,
                        RoomId = room.Roomid,
                        Group = room.Group.GetValueOrDefault(0),
                        Item = world.GetItemOrNull(pot.Item),
                        State = pot.State.ToArray(),
                        ItemSet = pot.ItemSet.Select(v => new ItemSetName(v, world)).ToArray(),
                        Trophy = world.GetItemOrNull(pot.Trophy),
                        Deny = pot.Deny.ToArray(),
                        Allow = pot.Allow.ToArray(),
                    };
                    structuredVertices.Add(pot.Name, potVertex);
                    regionVertex.Edges.Add(new Edge(regionVertex, potVertex, new ItemCondition(world.GetItem("LiftPot"), 1)));
                }

                foreach (var bosses in region.Bosses)
                {
                    foreach (var boss in bosses.Value)
                    {
                        var bossVertex = new Vertex
                        {
                            Type = VertexType.Boss,
                            Name = boss.Name,
                            World = world,
                            RoomId = room.Roomid,
                            RoomOAM = room.OAM,
                            Group = room.Group.GetValueOrDefault(0),
                            Sprite = Sprite.Get(boss.Sprite),
                            Item = world.GetItemOrNull(boss.Item),
                            State = boss.State.ToArray(),
                            ItemSet = boss.ItemSet.Select(v => new ItemSetName(v, world)).ToArray(),
                            Trophy = world.GetItemOrNull(boss.Trophy),
                            Deny = boss.Deny.ToArray(),
                            Allow = boss.Allow.ToArray(),
                        };
                        structuredVertices.Add(boss.Name, bossVertex);
                        regionVertex.Edges.Add(new Edge(regionVertex, bossVertex, new ItemCondition(world.GetItem(bosses.Key), 1)));
                    }
                }

                foreach (var connection in region.Connections)
                {
                    foreach (var target in connection.Value)
                    {
                        pendingConnections.Add((regionVertex, target, ConditionFrom(world, connection.Key)));
                    }
                }
            }

            foreach (var item in room.Nodes.Items)
            {
                structuredVertices.Add(item.Name, new Vertex
                {
                    Type = VertexType.Item,
                    SubType = item.Type,
                    Name = item.Name,
                    World = world,
                    RoomId = room.Roomid,
                    Group = room.Group.GetValueOrDefault(0),
                    Item = world.GetItemOrNull(item.Item),
                    ItemSet = item.ItemSet.Select(v => new ItemSetName(v, world)).ToArray(),
                    Addresses = item.Addresses.ToArray(),
                });
            }
        }

        // Link all the pending edges
        foreach (var pendingEdge in pendingConnections)
        {
            pendingEdge.Item1.Edges.Add(new Edge(pendingEdge.Item1, structuredVertices[pendingEdge.Item2], pendingEdge.Item3));
        }

        return structuredVertices.Values;
    }

    /// <summary>
    /// Given a particular world (configuration), read all the edge data files
    /// and create edges based on the world to connect the vertices.
    /// </summary>
    /// <param name="world">world to attach preset items to</param>
    private static Dictionary<ItemCondition, DirectedUndirectedPair> LoadEdges(World world)
    {
        var edgeData = new Dictionary<string, DirectedUndirectedPair>();
        YamlReader.MergeEdges(edgeData, YamlReader.LoadEdges("base"));

        switch (world.Config.State)
        {
            case StateOption.Standard:
                YamlReader.MergeEdges(edgeData, YamlReader.LoadEdges("normal"));
                edgeData["fixed"].Directed.Add(new() { "start", "Link's House - Bedroom" });
                break;
            case StateOption.Inverted:
                YamlReader.MergeEdges(edgeData, YamlReader.LoadEdges("inverted"));
                // @todo move these once we have the nodes made
                edgeData["fixed"].Directed.Add(new() { "start", "Link's House - Bedroom" });
                edgeData["fixed"].Directed.Add(new() { "start", "Dark Sanctuary" });
                break;
            case StateOption.Open:
            default:
                YamlReader.MergeEdges(edgeData, YamlReader.LoadEdges("normal"));
                edgeData["fixed"].Directed.Add(new() { "start", "Link's House - Bedroom" });
                edgeData["fixed"].Directed.Add(new() { "start", "Sanctuary Hall" });
                break;
        }

        if (world.Config.Techs != null)
        {
            foreach (var tech in world.Config.Techs)
            {
                var fileName = tech switch
                {
                    TechOption.DungeonBunnyRevival => "dungeon_bunny_revival",
                    _ => throw new Exception("Missing tech enum to file mapping for value: " + tech),
                };
                YamlReader.MergeEdges(edgeData, YamlReader.LoadEdgesFromTech(fileName));
            }
        }

        var returnData = new Dictionary<ItemCondition, DirectedUndirectedPair>();
        foreach (var (conditionString, edges) in edgeData)
        {
            var parts = conditionString.Split("|");
            var item = world.GetItem(parts[0]);
            var itemCountPair = new ItemCondition(item, parts.Length > 1 ? int.Parse(parts[1]) : 1);

            returnData[itemCountPair] = new DirectedUndirectedPair
            {
                Directed = edges.Directed,
            };
        }

        return returnData;
    }
}
