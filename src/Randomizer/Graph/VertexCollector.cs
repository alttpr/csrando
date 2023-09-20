namespace AlttpRandomizer.Graph;

/**
 * Container for all the vertices.
 */
internal class VertexCollector
{
    /**
     * This does not account for door rando.
     */
    private static readonly string[] BUNNY_REVIVE =
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
    public List<Dictionary<string, object>> LoadYmlData(World world)
    {
        var vertex_data = YamlReader.LoadVertices();

        int world_id = world.Id;
        bool inverted = world.Config.State == StateOption.Inverted;
        bool bunny_revive = world.Config.Techs.Contains(TechOption.DungeonBunnyRevival);
        var names = new HashSet<string>();
        var vertices = new List<Dictionary<string, object>>();

        // overworld
        foreach (var map in vertex_data.Maps)
        {
            var shared = new Dictionary<string, object>()
            {
                { "map", map.MapMap },
            };
            foreach (var meta in map.Nodes.Meta)
            {
                vertices.Add(shared
                    .MergeOne("type", VertexType.Meta)
                    .Merge(meta.AsDictionary()));
            }
            foreach (var prizepack in map.Nodes.Prizepacks)
            {
                vertices.Add(prizepack.AsDictionary()
                    .MergeOne("type", VertexType.PrizePack));
            }
            foreach (var region in map.Nodes.Regions)
            {
                vertices.Add(shared
                    .MergeOne("moonpearl", map.Moonpearl)
                    .MergeOne("type", VertexType.Region)
                    .Merge(region.AsDictionary()));
            }
            foreach (var warp in map.Nodes.Warps)
            {
                vertices.Add(shared
                    .MergeOne("moonpearl", map.Moonpearl)
                    .MergeOne("type", VertexType.Warp)
                    .Merge(warp.AsDictionary()));
            }
            foreach (var mob in map.Nodes.Mobs)
            {
                vertices.Add(shared
                    .MergeOne("type", VertexType.Mob)
                    .MergeOne("sprite", Sprite.Get(mob.Sprite))
                    .Merge(mob.AsDictionary()));
            }
            foreach (var item in map.Nodes.Items)
            {
                vertices.Add(shared
                    .MergeOne("type", VertexType.Item)
                    .Merge(item.AsDictionary()));
            }
            foreach (var entrance in map.Nodes.Entrances)
            {
                // TODO: the old code had conditional access to entranceid and outletid; are there entrances without them?
                vertices.Add(shared
                    .MergeOne("name", entrance.Name + " - In")
                    .MergeOne("entranceid", entrance.Entranceid)
                    .MergeOne("type", VertexType.Entrance));
                vertices.Add(shared
                    .MergeOne("name", entrance.Name + " - Out")
                    .MergeOne("outletid", entrance.Outletid)
                    .MergeOne("type", VertexType.Outlet));
            }
            // consider merging Holes into entrances
            foreach (var entrance in map.Nodes.Holes)
            {
                vertices.Add(shared
                    .MergeOne("name", entrance.Name)
                    .MergeOne("entranceids", entrance.Entranceids)
                    .MergeOne("type", VertexType.Hole));
            }
        }

        // underworld
        foreach (var room in vertex_data.Rooms)
        {
            var shared = new Dictionary<string, object>()
            {
                { "roomid", room.Roomid },
                { "group", room.Group.GetValueOrDefault(0) },
                { "dark", room.Dark },
            };
            foreach (var region in room.Nodes.Regions)
            {
                vertices.Add(shared
                    .MergeOne("type", VertexType.Region)
                    .Merge(region.AsDictionary()));
                if (region.Inletid.HasValue)
                {
                    vertices.Add(shared
                        .MergeOne("name", region.Name + " - Exit")
                        // TODO: Check this is the correct type
                        .MergeOne("type", VertexType.Entrance)
                        .MergeOne("inletid", region.Inletid.Value));
                }
            }
            foreach (var mob in room.Nodes.Mobs)
            {
                vertices.Add(shared
                    .MergeOne("type", VertexType.Mob)
                    .MergeOne("sprite", Sprite.Get(mob.Sprite))
                    .Merge(mob.AsDictionary()));
            }
            foreach (var item in room.Nodes.Items)
            {
                vertices.Add(shared
                    .MergeOne("type", VertexType.Item)
                    .Merge(item.AsDictionary()));
            }
            foreach (var keydoor in room.Nodes.Keydoors)
            {
                vertices.Add(shared
                    .MergeOne("type", VertexType.Keydoor)
                    .Merge(keydoor.AsDictionary()));
            }
            foreach (var bigkeydoor in room.Nodes.BigKeydoors)
            {
                vertices.Add(shared
                    .MergeOne("type", VertexType.BigKeydoor)
                    .Merge(bigkeydoor.AsDictionary()));
            }
            foreach (var shutter in room.Nodes.Shutters)
            {
                vertices.Add(shared
                    .MergeOne("type", VertexType.Shutter)
                    .Merge(shutter.AsDictionary()));
            }
            foreach (var pot in room.Nodes.Pots)
            {
                vertices.Add(shared
                    .MergeOne("type", VertexType.Pot)
                    .Merge(pot.AsDictionary()));
            }
            // TODO: how do we want to handle this?
            foreach (var item in room.Nodes.Inventory)
            {
                vertices.Add(shared
                    .Merge(item.AsDictionary()));
            }

            //if (room["bosses"] ?? false)
            //{
            //    foreach (var (from, sprites) in room["bosses"]) {
            //        // do stuff
            //    }
            //}
        }

        return vertices.Select((v) =>
        {
            if (v.TryGetValue("itemset", out object? itemSetO) && itemSetO is List<string> itemSet)
            {
                v["itemset"] = itemSet.Select((set) => $"{set}:{world_id}").ToArray();
            }
            if (v.TryGetValue("key", out object? keyO) && keyO is string key)
            {
                v["key"] = $"{key}:{world_id}";
            }
            if (inverted && v.TryGetValue("moonpearl", out object? moonpearlO) && moonpearlO is bool moonpearl)
            {
                v["moonpearl"] = !moonpearl;
            }

            if (bunny_revive && BUNNY_REVIVE.Contains(v["name"]))
            {
                v["moonpearl"] = false;
            }
            string new_name = $"{v["name"]}:{world_id}";
            if (!names.Add(new_name))
            {
                throw new Exception($"Vertex Name collision `{new_name}`");
            }

            v["name"] = $"{v["name"]}:{world_id}";
            return v;
        }).ToList();
    }
}
