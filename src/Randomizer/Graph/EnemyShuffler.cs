namespace Randomizer.Graph;

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
