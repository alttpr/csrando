namespace Randomizer.Graph;

/// <summary>A spoiler playthrough reduced to the pickups needed for victory.</summary>
public sealed record PlaythroughData(
    bool Complete,
    IReadOnlyList<string> VictoryItems,
    IReadOnlyList<PlaythroughItem> StartingItems,
    IReadOnlyList<PlaythroughSphere> Spheres,
    IReadOnlyList<string> Warnings);

public sealed record PlaythroughSphere(int Number, IReadOnlyList<PlaythroughPickup> Pickups);

public sealed record PlaythroughPickup(
    PlaythroughVertex Location,
    PlaythroughItem Item,
    IReadOnlyList<PlaythroughItem> RequiredItems,
    IReadOnlyList<PlaythroughPathStep> Path,
    bool Meta,
    bool Required);

public sealed record PlaythroughVertex(string Name, string Game, int World, string Type);
public sealed record PlaythroughItem(
    string Name, string Game, int World, int Count = 1,
    bool Meta = false, bool Initial = false, bool Simple = true);
public sealed record PlaythroughResource(string Name, int Amount);
public sealed record PlaythroughPathStep(
    PlaythroughVertex From,
    PlaythroughVertex To,
    PlaythroughItem? Requirement,
    IReadOnlyList<PlaythroughItem> Requirements,
    IReadOnlyList<PlaythroughResource> ResourcesSpent,
    string Game,
    bool CrossGame,
    string? Strategy = null);

/// <summary>
/// Builds deterministic reachability spheres while retaining a BFS predecessor for every
/// vertex. It then walks dependencies backward from the victory items, removing filler
/// pickups from the user-facing playthrough without losing their route provenance.
/// </summary>
internal static class PlaythroughGenerator
{
    private sealed record RouteStep(
        Edge Edge,
        string? Strategy,
        IReadOnlyDictionary<IItem, int> Requirements,
        IReadOnlyDictionary<string, int> ResourcesSpent);

    private sealed record Reachability(
        HashSet<Vertex> Vertices,
        Dictionary<IWorld, Inventory> AvailableInventories,
        Dictionary<Vertex, Games.SuperMetroid.StatefulPathStep?> SuperMetroidPredecessors,
        Dictionary<Vertex, List<Games.SuperMetroid.StatefulPickup>> SuperMetroidPickups);

    private sealed record FoundPickup(
        Vertex Location,
        IItem Item,
        int Sphere,
        List<RouteStep> Path,
        bool IsAutomaticEvent);

    public static PlaythroughData Generate(GameRandomizer randomizer)
    {
        var inventory = randomizer.PlaythroughStartingItems;
        var goals = randomizer.Worlds.SelectMany(world => world.GetVictoryItems()).Distinct().ToList();
        var randomizedLocations = randomizer.PlaythroughRandomizedLocations;
        var assumedItems = randomizer.PlaythroughAssumedItems;
        var physicalItems = randomizedLocations.SelectMany(ItemsAt).Concat(goals)
            .SelectMany(item => new[] { item, item.LogicalItem }.OfType<IItem>())
            .ToHashSet();
        var graphMetaItems = randomizer.Graph.GetVertices()
            .Where(vertex => !randomizedLocations.Contains(vertex))
            .SelectMany(ItemsAt).ToHashSet();
        bool IsMetaItem(IItem item) => !physicalItems.Contains(item)
            && (graphMetaItems.Contains(item)
                || item.Name.StartsWith("Config", StringComparison.Ordinal)
                || item.Name.StartsWith("f_", StringComparison.Ordinal));
        var initialLogicItems = inventory.All().Select(pair => pair.Key).ToHashSet();
        bool IsInitialLogic(IItem item) => IsMetaItem(item)
            && (initialLogicItems.Contains(item)
                || item.Name.StartsWith("Config", StringComparison.Ordinal));
        var startingItems = inventory.All()
            .Where(pair => pair.Key.Name != "fixed")
            .OrderBy(pair => pair.Key.World.Id).ThenBy(pair => pair.Key.Name)
            .Select(pair => Item(pair.Key, pair.Value,
                IsMetaItem(pair.Key), IsInitialLogic(pair.Key))).ToList();
        var starts = randomizer.Worlds.Select(world => world.GetPlaythroughStart()).Distinct().ToList();
        var initiallyStartedWorlds = starts.Select(start => start.World).ToHashSet();
        var victoryLocations = randomizer.Graph.GetVertices()
            .Where(vertex => ItemsAt(vertex).Any(item => goals.Contains(item)))
            .ToHashSet();
        // ComboSearcher keeps a game's searcher alive after a portal first reaches it.
        // Do the equivalent for sphere searches, since later state changes (notably the
        // SM escape) can make the original portal edge unavailable without stranding
        // a player who already entered the other game.
        var persistentStarts = starts.ToHashSet();
        var persistentEntryPaths = new Dictionary<Vertex, List<RouteStep>>();
        var collectedPickups = new HashSet<(Vertex, IItem)>();
        var collectedAutomaticItems = new HashSet<IItem>();
        // Items where every copy matters: anything gating an edge by count, and small keys —
        // keys are consumed on doors, but DoorReplacer rewrites their edge conditions into
        // per-door unlock items, so they never appear with Count > 1 themselves. Deduplicating
        // them would key-starve the simulation relative to real play (e.g. two pots holding
        // the same dungeon key) and stall the playthrough on winnable seeds.
        var countableAutomaticItems = randomizer.Graph.GetVertices()
            .SelectMany(vertex => vertex.Edges)
            .Where(edge => edge.Condition.Count > 1)
            .Select(edge => edge.Condition.Item)
            .Concat(randomizer.Graph.Doors.Keys)
            .ToHashSet();
        var found = new List<FoundPickup>();

        for (int sphere = 0; sphere < randomizer.Graph.GetVertices().Count(); sphere++)
        {
            GenerationContext.ThrowIfCancellationRequested();
            int persistentStartsBeforeSphere = persistentStarts.Count;
            var reachability = FindReachable(randomizer.Graph, persistentStarts, inventory,
                [.. randomizedLocations, .. victoryLocations]);
            var paths = BuildPaths(starts, reachability, persistentEntryPaths);
            foreach (var edge in randomizer.Graph.GetVertices().SelectMany(vertex => vertex.Edges)
                         .Where(edge => edge.From.World != edge.To.World
                             && !initiallyStartedWorlds.Contains(edge.To.World)
                             && reachability.Vertices.Contains(edge.To)
                             && paths.ContainsKey(edge.To)))
            {
                if (persistentStarts.Add(edge.To))
                    persistentEntryPaths[edge.To] = BuildPath(edge.To, paths);
            }
            var pickups = paths.Keys
                .Where(reachability.Vertices.Contains)
                .SelectMany(vertex => AvailablePickups(vertex, reachability)
                    .Select(pickup => (vertex, pickup)))
                .Where(pair => !collectedPickups.Contains((pair.vertex, pair.pickup.Item)))
                .Where(pair => ReferenceEquals(pair.vertex.Item, pair.pickup.Item)
                    || countableAutomaticItems.Contains(pair.pickup.Item)
                    || !collectedAutomaticItems.Contains(pair.pickup.Item))
                .OrderBy(pair => pair.vertex.World.Id)
                .ThenBy(pair => pair.vertex.Name)
                .ThenBy(pair => pair.pickup.Item.Name)
                .ToList();

            if (pickups.Count == 0)
                break;

            var acceptedItems = new List<IItem>();
            foreach (var (location, pickup) in pickups)
            {
                var item = pickup.Item;
                bool automatic = !ReferenceEquals(location.Item, item)
                    || !randomizedLocations.Contains(location)
                        && !victoryLocations.Contains(location);
                if (automatic && !countableAutomaticItems.Contains(item)
                    && collectedAutomaticItems.Contains(item))
                {
                    // Record the skip: this pair can never be accepted in a later sphere
                    // (its item stays deduplicated), and leaving it unrecorded would re-list
                    // it every sphere, preventing the loop from ever running out of pickups.
                    collectedPickups.Add((location, item));
                    continue;
                }
                var path = BuildPath(location, paths, pickup);
                if (location.World is Games.SuperMetroid.World
                    && item.Name.StartsWith("f_", StringComparison.Ordinal)
                    && path.SelectMany(step => step.Requirements.Keys)
                        .Any(requirement => SameLogicalItem(requirement, item)))
                {
                    var viablePath = BuildSuperMetroidPathWithoutPickup(
                        randomizer.Graph, persistentStarts, inventory, location,
                        pickup, paths);
                    if (viablePath == null)
                        continue;
                    path = viablePath;
                }
                collectedPickups.Add((location, item));
                if (automatic && !countableAutomaticItems.Contains(item))
                    collectedAutomaticItems.Add(item);
                found.Add(new FoundPickup(
                    location,
                    item,
                    sphere,
                    path,
                    automatic));
                acceptedItems.Add(item);
            }
            // Keep sphere evaluation simultaneous: only recorded pickups enter the
            // inventory, and none can affect another pickup's path in this sphere.
            foreach (var item in acceptedItems)
                inventory.AddItem(item);

            if (goals.Count > 0 && goals.All(inventory.Has))
                break;

            // A sphere that accepted nothing and reached no new world entries cannot change
            // future reachability: the playthrough is stalled (incomplete), so stop instead
            // of re-evaluating identical spheres until the loop bound.
            if (acceptedItems.Count == 0 && persistentStarts.Count == persistentStartsBeforeSphere)
                break;
        }

        var pickupsByItem = IndexPickupsByItem(found);
        var victoryRoots = goals.SelectMany(goal =>
            found.Where(pickup => SameLogicalItem(pickup.Item, goal)).Take(1));
        var victoryRequired = RequiredPickups(
            pickupsByItem, victoryRoots, randomizer.PlaythroughStartingItems);
        var detailedRoots = victoryRoots.Concat(found.Where(pickup =>
            !pickup.IsAutomaticEvent && assumedItems.Contains(pickup.Item))).Distinct();
        var detailedRequired = RequiredPickups(
            pickupsByItem, detailedRoots, randomizer.PlaythroughStartingItems);
        var spheres = found.Where(detailedRequired.Contains)
            .GroupBy(pickup => pickup.Sphere)
            .OrderBy(group => group.Key)
            .Select((group, index) => new PlaythroughSphere(index, group.Select(pickup =>
                new PlaythroughPickup(
                    VertexInfo(pickup.Location),
                    Item(pickup.Item, meta: IsMetaItem(pickup.Item),
                        initial: IsInitialLogic(pickup.Item)),
                    CondensedRequirements(pickup, pickupsByItem, detailedRequired,
                        randomizer.PlaythroughStartingItems, IsMetaItem, IsInitialLogic),
                    pickup.Path.Select(step => PathStep(
                        step, IsMetaItem, IsInitialLogic)).ToList(),
                    pickup.IsAutomaticEvent,
                    victoryRequired.Contains(pickup))).ToList()))
            .ToList();

        return new PlaythroughData(
            goals.Count > 0 && goals.All(inventory.Has),
            goals.Select(goal => goal.Name).ToList(),
            startingItems,
            spheres,
            []);
    }

    private static Reachability FindReachable(Graph graph, IEnumerable<Vertex> initialStarts,
        Inventory inventory, HashSet<Vertex> manualPickupLocations)
    {
        var starts = initialStarts.ToHashSet();
        var reachable = new HashSet<Vertex>();
        var availableInventories = new Dictionary<IWorld, Inventory>();
        var smPredecessors = new Dictionary<Vertex, Games.SuperMetroid.StatefulPathStep?>();
        var smPickups = new Dictionary<Vertex, List<Games.SuperMetroid.StatefulPickup>>();
        int previousStartCount;
        do
        {
            previousStartCount = starts.Count;
            foreach (var group in starts.GroupBy(vertex => vertex.World).ToList())
            {
                var groupStarts = group.ToList();
                if (group.Key is Games.SuperMetroid.World)
                {
                    var smSearcher = new Games.SuperMetroid.StatefulSearcher(
                        graph,
                        (Games.SuperMetroid.Vertex)groupStarts[0],
                        inventory.Clone(),
                        collectItemAt: vertex => !manualPickupLocations.Contains(vertex),
                        capturePath: true);
                    if (groupStarts.Count > 1)
                        smSearcher.ResumeSearch(groupStarts.Skip(1), inventory);
                    reachable.UnionWith(((ISearcher)smSearcher).GetVisited());
                    starts.UnionWith(smSearcher.GetOtherWorld());
                    availableInventories[group.Key] = smSearcher.GetInventory();
                    foreach (var vertex in group.Key.GetLocations())
                    {
                        smPredecessors.Remove(vertex);
                        smPickups.Remove(vertex);
                    }
                    foreach (var (vertex, step) in smSearcher.GetPredecessors())
                        smPredecessors[vertex] = step;
                    foreach (var (location, pickup) in smSearcher.GetPlaythroughPickups())
                    {
                        if (!smPickups.TryGetValue(location, out var items))
                        {
                            items = [];
                            smPickups[location] = items;
                        }
                        if (!items.Any(item => ReferenceEquals(item.Item, pickup.Item)))
                            items.Add(pickup);
                    }
                    continue;
                }

                var searcher = new Searcher(graph, groupStarts[0], inventory.Clone(),
                    world: group.Key, collectItemAt: vertex => !manualPickupLocations.Contains(vertex));
                if (groupStarts.Count > 1)
                    searcher.ResumeSearch(groupStarts.Skip(1), inventory);
                reachable.UnionWith(searcher.GetVisited());
                starts.UnionWith(searcher.GetOtherWorld());
                availableInventories[group.Key] = searcher.GetInventory();
            }
        } while (starts.Count != previousStartCount);

        return new Reachability(reachable, availableInventories, smPredecessors, smPickups);
    }

    private static Dictionary<Vertex, RouteStep?> BuildPaths(
        IEnumerable<Vertex> starts, Reachability reachability,
        IReadOnlyDictionary<Vertex, List<RouteStep>> persistentEntryPaths)
    {
        var previous = new Dictionary<Vertex, RouteStep?>();
        var queue = new Queue<Vertex>();
        foreach (var start in starts)
        {
            previous[start] = null;
            queue.Enqueue(start);
        }
        foreach (var (entry, path) in persistentEntryPaths.OrderBy(pair => pair.Value.Count))
        {
            // Queue every retained route vertex. Merely marking it as known would stop
            // the BFS when the original search meets that route and hide its branches.
            foreach (var step in path)
            {
                if (previous.TryAdd(step.Edge.To, step))
                    queue.Enqueue(step.Edge.To);
            }
            if (!previous.ContainsKey(entry))
            {
                previous[entry] = null;
                queue.Enqueue(entry);
            }
        }
        while (queue.TryDequeue(out var vertex))
        {
            foreach (var edge in vertex.Edges
                         .OrderBy(edge => edge.To.World.Id)
                         .ThenBy(edge => edge.To.World.GameId)
                         .ThenBy(edge => edge.To.Name))
            {
                RouteStep step;
                if (edge.From.World is Games.SuperMetroid.World
                    && ReferenceEquals(edge.From.World, edge.To.World))
                {
                    if (!reachability.SuperMetroidPredecessors.TryGetValue(edge.To, out var smStep)
                        || smStep == null || !ReferenceEquals(smStep.Edge, edge))
                        continue;
                    step = new RouteStep(edge, smStep.Strategy, smStep.Requirements,
                        smStep.ResourcesSpent);
                }
                else
                {
                    if (!reachability.AvailableInventories.TryGetValue(
                            edge.From.World, out var availableInventory)
                        || !availableInventory.Has(edge.Condition))
                        continue;
                    var condition = LogicalCondition(edge);
                    var requirements = condition.IsUnconditional
                        ? new Dictionary<IItem, int>()
                        : new Dictionary<IItem, int> { [condition.Item] = condition.Count };
                    step = new RouteStep(edge, null, requirements,
                        new Dictionary<string, int>());
                }
                if (!reachability.Vertices.Contains(edge.To) || previous.ContainsKey(edge.To))
                    continue;
                previous[edge.To] = step;
                queue.Enqueue(edge.To);
            }
        }
        return previous;
    }

    private static IEnumerable<IItem> ItemsAt(Vertex vertex)
    {
        if (vertex.Item != null) yield return vertex.Item;
        if (vertex.Trophy != null) yield return vertex.Trophy;
    }

    private static IEnumerable<Games.SuperMetroid.StatefulPickup> AvailablePickups(
        Vertex vertex, Reachability reachability) =>
        reachability.SuperMetroidPickups.TryGetValue(vertex, out var items)
            ? items
            : ItemsAt(vertex).Select(item => new Games.SuperMetroid.StatefulPickup(
                item, null, new Dictionary<IItem, int>(), new Dictionary<string, int>()));

    private static List<RouteStep> BuildPath(
        Vertex target,
        Dictionary<Vertex, RouteStep?> previous)
    {
        var path = new List<RouteStep>();
        var visited = new HashSet<Vertex>();
        while (previous[target] is { } step)
        {
            if (!visited.Add(target))
                throw new InvalidOperationException(
                    $"Cycle detected while building a playthrough path at '{target.Name}'.");
            path.Add(step);
            target = step.Edge.From;
        }
        path.Reverse();
        return path;
    }

    private static List<RouteStep> BuildPath(
        Vertex target,
        Dictionary<Vertex, RouteStep?> previous,
        Games.SuperMetroid.StatefulPickup pickup)
    {
        var path = BuildPath(target, previous);
        return ApplyPickupDetails(path, pickup);
    }

    private static List<RouteStep>? BuildSuperMetroidPathWithoutPickup(
        Graph graph, IEnumerable<Vertex> starts, Inventory inventory, Vertex target,
        Games.SuperMetroid.StatefulPickup pickup,
        Dictionary<Vertex, RouteStep?> generalPrevious)
    {
        List<RouteStep>? best = null;
        foreach (var start in starts.Where(start => ReferenceEquals(start.World, target.World)))
        {
            var searcher = new Games.SuperMetroid.StatefulSearcher(
                graph,
                (Games.SuperMetroid.Vertex)start,
                inventory.Clone(),
                collectItemAt: _ => false,
                capturePath: true,
                excludedPickup: pickup.Item);
            if (!searcher.HasVisited(target))
                continue;

            var acquisition = searcher.GetPickupDetails(target, pickup.Item) ?? pickup;

            var previous = searcher.GetPredecessors();
            var suffix = new List<RouteStep>();
            var current = target;
            while (previous.TryGetValue(current, out var step) && step != null)
            {
                suffix.Add(new RouteStep(
                    step.Edge, step.Strategy, step.Requirements, step.ResourcesSpent));
                current = step.Edge.From;
            }
            suffix.Reverse();

            var path = generalPrevious.ContainsKey(current)
                ? BuildPath(current, generalPrevious)
                : [];
            path.AddRange(suffix);
            path = ApplyPickupDetails(path, acquisition);
            if (path.SelectMany(step => step.Requirements.Keys)
                .Any(requirement => SameLogicalItem(requirement, pickup.Item)))
                continue;
            if (best == null || path.Count < best.Count)
                best = path;
        }
        return best;
    }

    private static List<RouteStep> ApplyPickupDetails(
        List<RouteStep> path, Games.SuperMetroid.StatefulPickup pickup)
    {
        if (path.Count > 0 && (pickup.Strategy != null || pickup.Requirements.Count > 0
                || pickup.ResourcesSpent.Count > 0))
        {
            var last = path[^1];
            var requirements = last.Requirements.ToDictionary();
            foreach (var (item, count) in pickup.Requirements)
                requirements[item] = Math.Max(requirements.GetValueOrDefault(item), count);
            path[^1] = last with
            {
                Strategy = JoinStrategies(last.Strategy, pickup.Strategy),
                Requirements = requirements,
                ResourcesSpent = MergeResourceMaximum(
                    last.ResourcesSpent, pickup.ResourcesSpent),
            };
        }
        return path;
    }

    private static string? JoinStrategies(string? first, string? second) =>
        first == null ? second : second == null || second == first ? first : $"{first}; {second}";

    private static Dictionary<string, int> MergeResourceMaximum(
        IReadOnlyDictionary<string, int> first, IReadOnlyDictionary<string, int> second)
    {
        var merged = first.ToDictionary();
        foreach (var (resource, amount) in second)
            merged[resource] = Math.Max(merged.GetValueOrDefault(resource), amount);
        return merged;
    }

    private static Dictionary<IItem, List<FoundPickup>> IndexPickupsByItem(
        IEnumerable<FoundPickup> found)
    {
        var index = new Dictionary<IItem, List<FoundPickup>>(
            ReferenceEqualityComparer.Instance);
        foreach (var pickup in found)
        {
            Add(pickup.Item, pickup);
            if (pickup.Item.LogicalItem is { } logicalItem
                && !ReferenceEquals(logicalItem, pickup.Item))
                Add(logicalItem, pickup);
        }
        return index;

        void Add(IItem item, FoundPickup pickup)
        {
            if (!index.TryGetValue(item, out var pickups))
                index[item] = pickups = [];
            pickups.Add(pickup);
        }
    }

    private static HashSet<FoundPickup> RequiredPickups(
        IReadOnlyDictionary<IItem, List<FoundPickup>> pickupsByItem,
        IEnumerable<FoundPickup> roots,
        Inventory startingInventory)
    {
        var required = new HashSet<FoundPickup>();
        var queue = new Queue<FoundPickup>(roots);
        while (queue.TryDequeue(out var pickup))
        {
            if (!required.Add(pickup)) continue;
            foreach (var condition in pickup.Path.SelectMany(step => step.Requirements)
                         .Select(pair => new ItemCondition(pair.Key, pair.Value)))
            {
                int alreadyStarting = startingInventory.GetCount(condition.Item);
                int needed = Math.Max(0, condition.Count - alreadyStarting);
                if (!pickupsByItem.TryGetValue(condition.Item, out var candidates))
                    continue;
                foreach (var dependency in candidates.Where(candidate =>
                             CanBeDependency(candidate, pickup)).Take(needed))
                    queue.Enqueue(dependency);
            }
        }
        return required;
    }

    private static IReadOnlyList<PlaythroughItem> CondensedRequirements(
        FoundPickup pickup,
        IReadOnlyDictionary<IItem, List<FoundPickup>> pickupsByItem,
        HashSet<FoundPickup> required,
        Inventory startingInventory, Func<IItem, bool> isMetaItem,
        Func<IItem, bool> isInitialLogic)
    {
        var items = new Dictionary<IItem, int>();
        var visited = new HashSet<FoundPickup>();

        void AddPathRequirements(FoundPickup current)
        {
            if (!visited.Add(current)) return;
            foreach (var condition in current.Path.SelectMany(step => step.Requirements)
                         .Select(pair => new ItemCondition(pair.Key, pair.Value)))
            {
                items[condition.Item] = Math.Max(items.GetValueOrDefault(condition.Item), condition.Count);
                int needed = Math.Max(0, condition.Count - startingInventory.GetCount(condition.Item));
                if (!pickupsByItem.TryGetValue(condition.Item, out var candidates))
                    continue;
                foreach (var dependency in candidates.Where(candidate => required.Contains(candidate)
                             && CanBeDependency(candidate, current))
                         .Take(needed))
                {
                    items[dependency.Item] = Math.Max(items.GetValueOrDefault(dependency.Item), 1);
                    AddPathRequirements(dependency);
                }
            }
        }

        AddPathRequirements(pickup);
        return items.OrderBy(pair => pair.Key.World.Id).ThenBy(pair => pair.Key.Name)
            .Select(pair => Item(pair.Key, pair.Value,
                isMetaItem(pair.Key) && pair.Key.Name is not "Crystal" and not "Pendant",
                isInitialLogic(pair.Key)))
            .ToList();
    }

    private static bool SameLogicalItem(IItem actual, IItem required) =>
        ReferenceEquals(actual, required) || ReferenceEquals(actual.LogicalItem, required);

    /// <summary>
    /// Fixed graph events are resolved transitively by Searcher and can therefore be
    /// discovered in the same sphere as the location that consumes them. Randomized
    /// pickups remain strictly limited to earlier spheres.
    /// </summary>
    private static bool CanBeDependency(FoundPickup candidate, FoundPickup pickup) =>
        !ReferenceEquals(candidate, pickup)
        && (candidate.Sphere < pickup.Sphere
            || candidate.Sphere == pickup.Sphere && candidate.IsAutomaticEvent);

    private static PlaythroughPathStep PathStep(
        RouteStep step, Func<IItem, bool> isMetaItem, Func<IItem, bool> isInitialLogic)
    {
        var requirements = step.Requirements
            .OrderBy(pair => pair.Key.World.Id).ThenBy(pair => pair.Key.Name)
            .Select(pair => Item(pair.Key, pair.Value,
                isMetaItem(pair.Key) && pair.Key.Name is not "Crystal" and not "Pendant",
                isInitialLogic(pair.Key))).ToList();
        var resourcesSpent = step.ResourcesSpent
            .Where(pair => pair.Value > 0)
            .OrderBy(pair => pair.Key)
            .Select(pair => new PlaythroughResource(pair.Key, pair.Value)).ToList();
        return new PlaythroughPathStep(
            VertexInfo(step.Edge.From),
            VertexInfo(step.Edge.To),
            requirements.FirstOrDefault(),
            requirements,
            resourcesSpent,
            step.Edge.From.World.GameId,
            !ReferenceEquals(step.Edge.From.World, step.Edge.To.World),
            step.Strategy);
    }

    /// <summary>Door edges use a synthetic per-door unlock item internally. Spoilers
    /// should name the small key the player actually spends.</summary>
    private static ItemCondition LogicalCondition(Edge edge)
    {
        foreach (var (key, doors) in edge.From.World.Graph.Doors)
        {
            if (doors.ContainsKey(edge.Condition.Item))
                return new ItemCondition(key, 1);
        }
        return edge.Condition;
    }

    private static PlaythroughVertex VertexInfo(Vertex vertex) =>
        new(vertex is Games.SuperMetroid.Vertex { LogicalName: { } logicalName }
                ? logicalName
                : vertex.Name,
            vertex.World.GameId, vertex.World.Id, vertex.Type.ToString());

    private static PlaythroughItem Item(
        IItem item, int count = 1, bool meta = false, bool initial = false) =>
        new(item.Name, item.World.GameId, item.World.Id, count, meta, initial,
            !initial && !IsInternalTraversalState(item.Name));

    private static bool IsInternalTraversalState(string name) =>
        name is "LostKiki" or "LightHole";
}
