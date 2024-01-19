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
                string name = $"{meta.Name}:{world.Id}";
                var metaVertex = new Vertex
                {
                    Type = VertexType.Meta,
                    Name = name,
                };
                structured_vertices.Add(name, metaVertex);

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
                        Name = $"{meta.Name} - {index} - {item}:{world.Id}",
                        Item = world.GetItem(item),
                    };
                    structured_vertices.Add(metaItemVertex.Name, metaItemVertex);
                    metaVertex.Edges.Add(new Edge(metaVertex, metaItemVertex, fixedCondition));
                }
            }
            foreach (var prizepack in map.Nodes.Prizepacks)
            {
                string name = $"{prizepack.Name}:{world.Id}";
                structured_vertices.Add(name, new Vertex
                {
                    Type = VertexType.PrizePack,
                    Name = name,
                    Offset = prizepack.Offset,
                    Sprite = Sprite.Get(prizepack.Sprite),
                });
            }
            foreach (var region in map.Nodes.Regions)
            {
                string name = $"{region.Name}:{world.Id}";
                var regionVertex = new Vertex
                {
                    Type = region.Type ?? VertexType.Region,
                    Name = name,
                    Map = map.MapMap,
                    InletId = region.InletId,
                    Shopkeeper = region.Shopkeeper,
                    ShopStyle = region.Shopstyle,
                    Switch = region.Switch ?? false,
                    MoonPearl = MoonPearlTransform(map.Moonpearl, name),
                };
                structured_vertices.Add(name, regionVertex);

                foreach (var mob in region.Mobs)
                {
                    string mobName = $"{mob.Name}:{world.Id}";
                    var mobVertex = new Vertex
                    {
                        Type = VertexType.Mob,
                        Name = mobName,
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
                    structured_vertices.Add(mobName, mobVertex);
                    regionVertex.Edges.Add(new Edge(regionVertex, mobVertex, fixedCondition));
                }

                foreach (var entrance in region.Entrances)
                {
                    // TODO: the old code had conditional access to entranceid and outletid; are there entrances without them?
                    string nameIn = $"{entrance.Name} - In:{world.Id}";
                    string nameOut = $"{entrance.Name} - Out:{world.Id}";
                    var entranceInVertex = new Vertex
                    {
                        Type = VertexType.Entrance,
                        Name = nameIn,
                        Map = map.MapMap,
                        EntranceId = entrance.EntranceId,
                    };
                    structured_vertices.Add(nameIn, entranceInVertex);
                    var entranceOutVertex = new Vertex
                    {
                        Type = VertexType.Outlet,
                        Name = nameOut,
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
                    string itemName = $"{item.Name}:{world.Id}";
                    var itemVertex = new Vertex
                    {
                        Type = VertexType.Item,
                        SubType = item.Type,
                        Name = itemName,
                        Map = map.MapMap,
                        Item = world.GetItemOrNull(item.Item),
                        ItemSet = item.ItemSet.Select(v => new ItemSetName(v, world)).ToArray(),
                        Addresses = item.Addresses.ToArray(),
                    };
                    structured_vertices.Add(itemName, itemVertex);
                    foreach (var condition in item.Conditions.DefaultIfEmpty("fixed"))
                    {
                        regionVertex.Edges.Add(new Edge(regionVertex, itemVertex, ConditionFrom(world, condition)));
                    }
                }

                foreach (var hole in region.Holes)
                {
                    string holeName = $"{hole.Name}:{world.Id}";
                    var holeVertex = new Vertex
                    {
                        Type = VertexType.Hole,
                        Name = holeName,
                        Map = map.MapMap,
                        EntranceIds = hole.EntranceIds.ToArray(),
                    };
                    structured_vertices.Add(holeName, holeVertex);
                    foreach (var condition in hole.Conditions.DefaultIfEmpty("fixed"))
                    {
                        regionVertex.Edges.Add(new Edge(regionVertex, holeVertex, ConditionFrom(world, condition)));
                    }
                }

                foreach (var warp in region.Warps)
                {
                    string warpName = $"{warp.Name}:{world.Id}";
                    var warpVertex = new Vertex
                    {
                        Type = VertexType.Warp,
                        Name = warpName,
                        Map = map.MapMap,
                        Position = warp.Position,
                        MoonPearl = MoonPearlTransform(map.Moonpearl, warpName),
                    };
                    structured_vertices.Add(warpName, warpVertex);
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
                string name = $"{region.Name}:{world.Id}";
                regionVertex = new Vertex
                {
                    Type = region.Type ?? VertexType.Region,
                    Name = name,
                    RoomId = room.Roomid,
                    Group = room.Group.GetValueOrDefault(0),
                    Dark = room.Dark,
                    ExtraLight = room.ExtraLight,
                    InletId = region.InletId,
                    Shopkeeper = region.Shopkeeper,
                    ShopStyle = region.Shopstyle,
                    Switch = region.Switch ?? false,
                };
                structured_vertices.Add(name, regionVertex);

                if (region.InletId.HasValue)
                {
                    string nameExit = $"{region.Name} - Exit:{world.Id}";
                    var exitVertex = new Vertex
                    {
                        Type = VertexType.Entrance,
                        Name = nameExit,
                        RoomId = room.Roomid,
                        Group = room.Group.GetValueOrDefault(0),
                        InletId = region.InletId,
                    };
                    structured_vertices.Add(nameExit, exitVertex);
                    regionVertex.Edges.Add(new Edge(regionVertex, exitVertex, fixedCondition));
                }

                foreach (var mob in region.Mobs)
                {
                    string mobName = $"{mob.Name}:{world.Id}";
                    var mobVertex = new Vertex
                    {
                        Type = VertexType.Mob,
                        Name = mobName,
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
                    structured_vertices.Add(mobName, mobVertex);
                    regionVertex.Edges.Add(new Edge(regionVertex, mobVertex, fixedCondition));
                }

                if (region.Entrances.Any())
                    throw new Exception("Found old style entrance in underworld region node.");

                foreach (var item in region.Items)
                {
                    string itemName = $"{item.Name}:{world.Id}";
                    var itemVertex = new Vertex
                    {
                        Type = VertexType.Item,
                        SubType = item.Type,
                        Name = itemName,
                        RoomId = room.Roomid,
                        Group = room.Group.GetValueOrDefault(0),
                        Item = world.GetItemOrNull(item.Item),
                        ItemSet = item.ItemSet.Select(v => new ItemSetName(v, world)).ToArray(),
                        Addresses = item.Addresses.ToArray(),
                    };
                    structured_vertices.Add(itemName, itemVertex);

                    foreach (var condition in item.Conditions.DefaultIfEmpty("fixed"))
                    {
                        regionVertex.Edges.Add(new Edge(regionVertex, itemVertex, ConditionFrom(world, condition)));
                    }
                }

                foreach (var item in region.Inventory)
                {
                    string inventoryName = $"{item.Name}:{world.Id}";
                    var inventoryVertex = new Vertex
                    {
                        Type = item.Type,
                        Name = inventoryName,
                        RoomId = room.Roomid,
                        Group = room.Group.GetValueOrDefault(0),
                        Item = world.GetItemOrNull(item.Item),
                        Cost = item.Cost,
                        ItemSet = item.ItemSet.Select(v => new ItemSetName(v, world)).ToArray(),
                    };
                    structured_vertices.Add(inventoryName, inventoryVertex);
                    regionVertex.Edges.Add(new Edge(regionVertex, inventoryVertex, new ItemCondition(world.GetItem("BuyItem"), 1)));
                }

                foreach (var pot in region.Pots)
                {
                    string potName = $"{pot.Name}:{world.Id}";
                    var potVertex = new Vertex
                    {
                        Type = VertexType.Pot,
                        Name = potName,
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
                    structured_vertices.Add(potName, potVertex);
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
                string name = $"{item.Name}:{world.Id}";
                structured_vertices.Add(name, new Vertex
                {
                    Type = VertexType.Item,
                    SubType = item.Type,
                    Name = name,
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
            pendingEdge.Item1.Edges.Add(new Edge(pendingEdge.Item1, structured_vertices[$"{pendingEdge.Item2}:{world.Id}"], pendingEdge.Item3));
        }

        return structured_vertices.Values;
    }
}
