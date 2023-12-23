namespace Randomizer.Graph;

/// <summary>
/// Updates the graph to remove small key usage from edges.
/// Small keys are replaced with a unique item identifying when the door has been open.
/// The Graph.Doors and Graph.FixedKeys are caching the list of doors and fixed dungeon
/// keys here.
/// </summary>
internal sealed class DoorReplacer : IWorldModifier
{
    public static void AdjustEdges(World world, PRNG rng)
    {
        var doors = world.Graph.Doors;

        foreach (var edge in world.Graph.GetVertices()
            .SelectMany(v => v.Edges)
            .Where(e => e.From.Name.EndsWith($":{world.Id}") && e.Condition.Item.Type == ItemType.SmallKey))
        {
            var first = edge.From;
            var second = edge.To;

            if (edge.From.Name.CompareTo(edge.To.Name) > 0)
                (first, second) = (second, first);

            if (!doors.TryGetValue(edge.Condition.Item, out var doorsForKey))
            {
                doorsForKey = new();
                doors.Add(edge.Condition.Item, doorsForKey);
            }

            var unlockItem = ItemForDoorUnlock(world, first, second);
            edge.Condition = new ItemCondition(unlockItem, 1);

            if (!doorsForKey.TryGetValue(unlockItem, out var vertices))
            {
                vertices = new();
                doorsForKey.Add(unlockItem, vertices);
            }
            vertices.Add((first, second));
        }

        FindFixedKeys(world);
    }

    static void FindFixedKeys(World world)
    {
        var fixedKeys = world.Graph.FixedKeys;

        var worldKeys = world.Graph.GetVertices()
                    .Where(v => v.Name.EndsWith($":{world.Id}") && v.Item?.Type == ItemType.SmallKey)
                    .GroupBy(v => v.Item!)
                    .ToDictionary(k => k.Key, v => v.ToHashSet());

        foreach (var key in world.Graph.Doors)
        {
            worldKeys.TryAdd(key.Key, new HashSet<Vertex>());
        }

        foreach (var e in worldKeys)
        {
            fixedKeys.Add(e.Key, e.Value);
        }
    }

    static Item ItemForDoorUnlock(World world, Vertex a, Vertex b)
    {
        string name = $"UnlockDoor: {a.Name} / {b.Name}";
        return world.GetItem(name);
    }
}
