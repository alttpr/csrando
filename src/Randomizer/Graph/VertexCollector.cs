namespace Randomizer.Graph;

/**
 * Container for all the vertices.
 */
internal class VertexCollector
{
    static ItemCondition ConditionFrom(World world, string condition)
    {
        var conditionSplit = condition.Split("|");
        var itemCount = 1;

        if (conditionSplit.Length > 1)
            itemCount = int.Parse(conditionSplit[1]);

        return new ItemCondition(world.GetItem(conditionSplit[0]), itemCount);
    }

    /**
     * This does not account for door rando.
     */
    private static readonly HashSet<string> BUNNY_REVIVE = new()
    {
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
    };

    /**
     * Get all vertices for a world and map static items to that world. Also
     * given the world config, we may invert the moonpearl requirements here.
     *
     * @param World world world to attach preset items to
     *
     * @throws Exception if unable to read data files
     */
    // TODO: this really needs to return typed data already...
    public static IEnumerable<Vertex> LoadYmlData(World world)
    {
        var vertex_data = YamlReader.LoadVertices();
        var structured_vertices = new Dictionary<string, Vertex>();
        var fixedCondition = new ItemCondition(world.GetItem("fixed"), 1);
        var pendingConnections = new List<(Vertex, string, ItemCondition)>();

        var MoonPearlTransform = (bool moonpearl, string name) =>
        {
            bool result = moonpearl;
            if (world.Config.State == StateOption.Inverted)
            {
                result = !moonpearl;
            }

            if (world.Config.Techs.Contains(TechOption.DungeonBunnyRevival) && BUNNY_REVIVE.Contains(name))
            {
                result = false;
            }
            return result;
        };

        // overworld
        foreach (var map in vertex_data.Maps)
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
                structured_vertices.Add(meta.Name, metaVertex);

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
                    structured_vertices.Add(metaItemVertex.Name, metaItemVertex);
                    metaVertex.Edges.Add(new Edge(metaVertex, metaItemVertex, fixedCondition));
                }
            }
            foreach (var prizepack in map.Nodes.Prizepacks)
            {
                structured_vertices.Add(prizepack.Name, new Vertex
                {
                    Type = VertexType.PrizePack,
                    Name = prizepack.Name,
                    World = world,
                    Offset = prizepack.Offset,
                    Sprite = Sprite.Get(prizepack.Sprite),
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
                    InletId = region.InletId,
                    Shopkeeper = region.Shopkeeper,
                    ShopStyle = region.Shopstyle,
                    Switch = region.Switch ?? false,
                    MoonPearl = MoonPearlTransform(map.Moonpearl, region.Name),
                };
                structured_vertices.Add(region.Name, regionVertex);

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
                        // TODO: Add deny, allow to Vertex.
                        // Deny = mob.Deny,
                        // Allow = mob.Allow,
                    };
                    structured_vertices.Add(mob.Name, mobVertex);
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
                    };
                    structured_vertices.Add(nameIn, entranceInVertex);
                    var entranceOutVertex = new Vertex
                    {
                        Type = VertexType.Outlet,
                        Name = nameOut,
                        World = world,
                        Map = map.MapMap,
                        OutletId = entrance.OutletId,
                    };
                    structured_vertices.Add(nameOut, entranceOutVertex);
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
                    structured_vertices.Add(item.Name, itemVertex);
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
                    };
                    structured_vertices.Add(hole.Name, holeVertex);
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
                        MoonPearl = MoonPearlTransform(map.Moonpearl, warp.Name),
                    };
                    structured_vertices.Add(warp.Name, warpVertex);
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
        foreach (var room in vertex_data.Rooms)
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
                    Group = room.Group.GetValueOrDefault(0),
                    Dark = room.Dark,
                    ExtraLight = room.ExtraLight,
                    InletId = region.InletId,
                    Shopkeeper = region.Shopkeeper,
                    ShopStyle = region.Shopstyle,
                    Switch = region.Switch ?? false,
                };
                structured_vertices.Add(region.Name, regionVertex);

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
                    structured_vertices.Add(nameExit, exitVertex);
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
                        Group = room.Group.GetValueOrDefault(0),
                        Sprite = Sprite.Get(mob.Sprite),
                        Item = world.GetItemOrNull(mob.Item),
                        State = mob.State.ToArray(),
                        ItemSet = mob.ItemSet.Select(v => new ItemSetName(v, world)).ToArray(),
                        Trophy = world.GetItemOrNull(mob.Trophy),
                        // TODO: Add deny, allow to Vertex.
                        // Deny = mob.Deny,
                        // Allow = mob.Allow,
                    };
                    structured_vertices.Add(mob.Name, mobVertex);
                    regionVertex.Edges.Add(new Edge(regionVertex, mobVertex, fixedCondition));
                }

                if (region.Entrances.Any())
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
                    structured_vertices.Add(item.Name, itemVertex);

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
                    structured_vertices.Add(item.Name, inventoryVertex);
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
                        // TODO: Add deny, allow to Vertex.
                        // Deny = pot.Deny,
                        // Allow = pot.Allow,
                    };
                    structured_vertices.Add(pot.Name, potVertex);
                    regionVertex.Edges.Add(new Edge(regionVertex, potVertex, new ItemCondition(world.GetItem("LiftPot"), 1)));
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
                structured_vertices.Add(item.Name, new Vertex
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

            //if (room["bosses"] ?? false)
            //{
            //    foreach (var (from, sprites) in room["bosses"]) {
            //        // do stuff
            //    }
            //}
        }

        // Link all the pending edges
        foreach (var pendingEdge in pendingConnections)
        {
            pendingEdge.Item1.Edges.Add(new Edge(pendingEdge.Item1, structured_vertices[pendingEdge.Item2], pendingEdge.Item3));
        }

        return structured_vertices.Values;
    }
}
