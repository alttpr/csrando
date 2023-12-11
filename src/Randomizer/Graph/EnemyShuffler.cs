namespace Randomizer.Graph;

/// <summary>
/// Modify the edges of the graph to shuffle entrances.
/// </summary>
internal sealed class EnemyShuffler : IWorldModifier
{
    /// <summary>
    /// @todo move this to YAML by adding trophy to the region.
    /// </summary>
    private static readonly Dictionary<string, string[]> CHALLENGE_ROOMS = new()
    {
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
        { "Misery Mire - Sluggula Cross", new[] {
            "Misery Mire - Sluggula Cross - Sluggula TL",
            "Misery Mire - Sluggula Cross - Sluggula TR",
            "Misery Mire - Sluggula Cross - Antifairy",
            "Misery Mire - Sluggula Cross - Sluggula BL",
            "Misery Mire - Sluggula Cross - Sluggula BR",
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
    };

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
            string vertexName = $"{token}:{world.Id}";
            if (!world.Graph.HasVertex(vertexName))
            {
                world.Graph.AddVertex(new Vertex
                {
                    Name = vertexName,
                    Type = VertexType.Meta,
                    Item = world.GetItem(token),
                });
            }
        }

        var from = world.GetLocation("Meta");
        foreach (var (token, items) in defeats)
        {
            var to = world.Graph.GetVertex($"{token}:{world.Id}");
            foreach (string item in items)
            {
                world.Graph.AddDirected(from, to, world.GetItem(item));
            }
        }

        // Replaces the fixed condition to mobs with a Defeat condition
        // if one exists.
        foreach (var vertex in world.Graph.GetVertices())
        {
            foreach (var edge in vertex.Edges)
            {
                if (edge.To.Type != VertexType.Mob)
                    continue;
                if (!edge.Condition.IsUnconditional)
                    continue;
                if (edge.To.Sprite == null)
                    continue;

                if (defeats.ContainsKey($"Defeat{edge.To.Sprite!.Name}"))
                    edge.Condition = new ItemCondition(world.GetItem($"Defeat{edge.To.Sprite!.Name}"), 1);
            }
        }

        foreach (var (room, enemies) in CHALLENGE_ROOMS)
        {
            from = world.GetLocation(room);
            foreach (string enemy in enemies)
            {
                var to = world.GetLocation(enemy);
                if (to.Sprite == null)
                    throw new Exception($"No sprite for {enemy}");

                world.Graph.AddDirected(from, to, world.GetItem($"Defeat{to.Sprite.Name}"));
            }
        }
    }
}
