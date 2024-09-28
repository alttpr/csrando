namespace Randomizer.Graph;

/// <summary>
/// Updates the graph to remove small key usage from edges.
/// Small keys are replaced with a unique item identifying when the door has been open.
/// The Graph.Doors and Graph.FixedKeys are caching the list of doors and fixed dungeon
/// keys here.
/// </summary>
internal sealed class DoorReplacer : IWorldModifier
{
    public void AdjustEdges(World world, PRNG rng)
    {
        var doors = world.Graph.Doors;

        foreach (var edge in world.Graph.GetVertices()
            .SelectMany(v => v.Edges)
            .Where(e => e.From.World == world && e.Condition.Item.Type == ItemType.SmallKey))
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
        FindKeyForKey(world);
    }

    static void FindFixedKeys(World world)
    {
        var fixedKeys = world.Graph.FixedKeys;

        var worldKeys = world.Graph.GetVertices()
                    .Where(v => v.World == world && v.Item?.Type == ItemType.SmallKey)
                    .GroupBy(v => v.Item!)
                    .ToDictionary(k => k.Key, v => v.ToHashSet());

        foreach (var key in world.Graph.Doors.Where(k => k.Key.World == world))
        {
            worldKeys.TryAdd(key.Key, new HashSet<Vertex>());
        }

        foreach (var e in worldKeys)
        {
            fixedKeys.Add(e.Key, e.Value);
        }
    }

    static void FindKeyForKey(World world)
    {
        if (world.Config.Accessibility == AccessibilityOption.Locations)
            return;

        VertexHashSet allVisited = new(world.Graph);
        {
            Queue<Vertex> vertexQueue = new();
            vertexQueue.Enqueue(world.GetLocation("start"));

            while (vertexQueue.Count != 0)
            {
                Vertex v = vertexQueue.Dequeue();
                allVisited.Add(v);

                foreach (var edge in v.Edges)
                {
                    if (!allVisited.Contains(edge.To))
                    {
                        allVisited.Add(edge.To);
                        vertexQueue.Enqueue(edge.To);
                    }
                }
            }
        }

        foreach (var keyset in world.Graph.Doors)
        {
            foreach (var door in keyset.Value)
            {
                if (door.Key.World != world)
                    continue;

                Queue<Vertex> vertexQueue = new();

                VertexHashSet visitedWithoutDoor = new(world.Graph);
                vertexQueue.Enqueue(world.GetLocation("start"));

                while (vertexQueue.Count != 0)
                {
                    Vertex v = vertexQueue.Dequeue();
                    visitedWithoutDoor.Add(v);

                    foreach (var edge in v.Edges)
                    {
                        // Visit everything except for what's behind the door
                        if (edge.Condition.Item == door.Key)
                            continue;

                        if (!visitedWithoutDoor.Contains(edge.To))
                        {
                            visitedWithoutDoor.Add(edge.To);
                            vertexQueue.Enqueue(edge.To);
                        }
                    }
                }

                visitedWithoutDoor.SymmetricExceptWith(allVisited);
                var behindDoor = visitedWithoutDoor.ToList();
                // Find all the locations behind a door that have a single empty chest and no other item drop
                // Those are "KeyForKey" chest targets and in "Accessibility.Items" mode are eligible to receive
                // a key.
                // TODO: Check behavior when we add pot and enemies as item targets
                if (behindDoor.Count(v => v.Type == VertexType.Item && v.SubType == VertexType.Chest && v.Item == null) == 1 && !behindDoor.Any(v => v.Item != null))
                {
                    List<(Vertex Chest, List<Vertex> Regions)> keyForKeys;
                    if (!world.Graph.KeyForKeys.TryGetValue(keyset.Key, out keyForKeys!))
                    {
                        keyForKeys = new();
                        world.Graph.KeyForKeys.Add(keyset.Key, keyForKeys);
                    }
                    var chest = behindDoor.Where(v => v.Type == VertexType.Item && v.SubType == VertexType.Chest && v.Item == null).First();
                    keyForKeys.Add((chest, door.Value.SelectMany(v => new Vertex[] { v.A, v.B }).ToList()));
                }
            }
        }

        foreach (var bigkey in world.GetAllItems().Where(i => i.Type == ItemType.BigKey))
        {
            Queue<Vertex> vertexQueue = new();

            VertexHashSet visitedWithoutDoor = new(world.Graph);
            vertexQueue.Enqueue(world.GetLocation("start"));

            while (vertexQueue.Count != 0)
            {
                Vertex v = vertexQueue.Dequeue();
                visitedWithoutDoor.Add(v);

                foreach (var edge in v.Edges)
                {
                    // Visit everything except for what's behind the bigkey
                    if (edge.Condition.Item == bigkey)
                        continue;

                    if (!visitedWithoutDoor.Contains(edge.To))
                    {
                        visitedWithoutDoor.Add(edge.To);
                        vertexQueue.Enqueue(edge.To);
                    }
                }
            }

            visitedWithoutDoor.SymmetricExceptWith(allVisited);
            var behindDoor = visitedWithoutDoor.ToList();

            // Find all the locations behind a door that have a single empty chest and no other item drop
            // Those are "KeyForKey" chest targets and in "Accessibility.Items" mode are eligible to receive
            // a key.
            // TODO: Check behavior when we add pot and enemies as item targets
            if (behindDoor.Count(v => v.Type == VertexType.Item && v.SubType == VertexType.BigChest && v.Item == null) == 1 && behindDoor.Count(v => v.Item != null || v.Type == VertexType.Item) == 1)
            {
                List<(Vertex Chest, List<Vertex> Regions)> keyForKeys;
                if (!world.Graph.KeyForKeys.TryGetValue(bigkey, out keyForKeys!))
                {
                    keyForKeys = new();
                    world.Graph.KeyForKeys.Add(bigkey, keyForKeys);
                }
                var chest = behindDoor.Where(v => v.Type == VertexType.Item && v.SubType == VertexType.BigChest && v.Item == null).First();
                keyForKeys.Add((chest, []));
            }
        }
    }

    static Item ItemForDoorUnlock(World world, Vertex a, Vertex b)
    {
        string name = $"UnlockDoor: {a.Name} / {b.Name}";
        return world.GetItem(name);
    }
}
