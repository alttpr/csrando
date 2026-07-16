namespace Randomizer.Games.SuperMetroid;

using System.Runtime.CompilerServices;
using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;

/// <summary>
/// Immutable, vertex-ID-indexed topology shared by every SM traversal in one
/// world. It is built lazily after the graph has assigned stable vertex IDs.
/// </summary>
internal sealed class SmSearchModel
{
    private static readonly ConditionalWeakTable<Graph,
        Dictionary<IWorld, SmSearchModel>> ByGraph = new();

    private SmSearchModel(IWorld world, Graph graph)
    {
        int capacity = graph.GetVertices().Select(vertex => vertex.Id)
            .DefaultIfEmpty(-1).Max() + 1;
        VerticesById = new Vertex?[capacity];
        var incoming = new List<Randomizer.Graph.Edge>?[capacity];
        var vertexIds = new List<int>();
        var terminalIds = new List<int>();

        foreach (var vertex in graph.GetVertices().OfType<Vertex>()
                     .Where(vertex => ReferenceEquals(vertex.World, world)))
        {
            VerticesById[vertex.Id] = vertex;
            vertexIds.Add(vertex.Id);
            if (vertex.Edges.Any(edge => edge.To.World != vertex.World))
                terminalIds.Add(vertex.Id);
        }

        foreach (int fromId in vertexIds)
        {
            var from = VerticesById[fromId]!;
            foreach (var edge in from.Edges)
            {
                if (!ReferenceEquals(edge.To.World, world)
                    || edge.To is not Vertex to)
                    continue;
                (incoming[to.Id] ??= []).Add(edge);
            }
        }

        IncomingEdgesById = new Randomizer.Graph.Edge[capacity][];
        foreach (int vertexId in vertexIds)
            IncomingEdgesById[vertexId] = incoming[vertexId]?.ToArray() ?? [];
        VertexIds = vertexIds.ToArray();
        CrossWorldTerminalIds = terminalIds.ToArray();

        var flagNames = new HashSet<string>(
            graph.AllItems.Where(item => ReferenceEquals(item.World, world)
                    && item.Name.StartsWith("f_", StringComparison.Ordinal))
                .Select(item => item.Name), StringComparer.Ordinal);
        foreach (int vertexId in vertexIds)
        {
            var vertex = VerticesById[vertexId]!;
            foreach (var strategy in vertex.Edges.OfType<Edge>()
                         .SelectMany(edge => edge.Strats ?? []))
                flagNames.UnionWith(strategy.SetsFlags ?? []);
            foreach (var nodeLock in vertex.Node?.Locks ?? [])
            {
                flagNames.UnionWith(nodeLock.Yields ?? []);
                foreach (var strategy in nodeLock.UnlockStrats ?? [])
                    flagNames.UnionWith(strategy.SetsFlags ?? []);
            }
        }
        if (flagNames.Count > 64)
            throw new InvalidOperationException(
                "The compiled SM search model supports at most 64 event flags.");
        FlagItems = flagNames.OrderBy(name => name, StringComparer.Ordinal)
            .Select(world.GetItem).ToArray();
        _flagBitsByName = FlagItems.Select((item, index) => (item.Name, index))
            .ToDictionary(pair => pair.Name, pair => pair.index,
                StringComparer.Ordinal);
        foreach (int vertexId in vertexIds)
        {
            var vertex = VerticesById[vertexId]!;
            ulong produced = 0;
            foreach (var strategy in vertex.Edges.OfType<Edge>()
                         .SelectMany(edge => edge.Strats ?? []))
                produced |= FlagMask(strategy.SetsFlags);
            foreach (var nodeLock in vertex.Node?.Locks ?? [])
            {
                foreach (var strategy in nodeLock.UnlockStrats ?? [])
                    produced |= FlagMask(strategy.SetsFlags);
            }
            _flagsProducedByRoom[vertex.RoomId] =
                _flagsProducedByRoom.GetValueOrDefault(vertex.RoomId) | produced;
        }
    }

    public Vertex?[] VerticesById { get; }
    public Randomizer.Graph.Edge[][] IncomingEdgesById { get; }
    public int[] VertexIds { get; }
    public int[] CrossWorldTerminalIds { get; }
    public IItem[] FlagItems { get; }
    public int Capacity => VerticesById.Length;
    private readonly Dictionary<string, int> _flagBitsByName;
    private readonly Dictionary<int, ulong> _flagsProducedByRoom = [];
    private readonly Dictionary<Requirement, CompiledRequirementPlan>
        _requirements = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<Strat, CompiledStrategyPlan> _strategies =
        new(ReferenceEqualityComparer.Instance);

    public ulong FlagMask(IEnumerable<string>? flags)
    {
        ulong mask = 0;
        foreach (string flag in flags ?? [])
        {
            if (_flagBitsByName.TryGetValue(flag, out int bit))
                mask |= 1UL << bit;
        }
        return mask;
    }

    public ulong InventoryFlagMask(Inventory inventory)
    {
        ulong mask = 0;
        for (int index = 0; index < FlagItems.Length; index++)
        {
            if (inventory.Has(FlagItems[index]))
                mask |= 1UL << index;
        }
        return mask;
    }

    public ulong FlagsProducedInRoom(int roomId) =>
        _flagsProducedByRoom.GetValueOrDefault(roomId);

    public CompiledRequirementPlan GetRequirementPlan(
        Requirement requirement, RequirementHandler handler)
    {
        if (_requirements.TryGetValue(requirement, out var plan))
            return plan;

        var visiting = new HashSet<Requirement>(ReferenceEqualityComparer.Instance);
        plan = Compile(requirement);
        _requirements[requirement] = plan;
        return plan;

        CompiledRequirementPlan Compile(Requirement current)
        {
            if (_requirements.TryGetValue(current, out var cached))
                return cached;
            if (!visiting.Add(current))
                return default;

            var result = current switch
            {
                Requirement.ObstaclesCleared cleared => new(
                    RequirementHandler.ObstacleMaskFromArray(cleared.Obstacles), 0, 0, true),
                Requirement.ObstaclesNotCleared notCleared => new(
                    RequirementHandler.ObstacleMaskFromArray(notCleared.Obstacles), 0, 0, true),
                Requirement.DoorUnlockedAtNode door => new(0, 1 << door.Node, 0, true),
                Requirement.Single single when single.Req.StartsWith(
                    "f_", StringComparison.Ordinal) => new(
                    0, 0, FlagMask([single.Req])),
                Requirement.SingleItem item when item.Item.Name.StartsWith(
                    "f_", StringComparison.Ordinal) => new(
                    0, 0, FlagMask([item.Item.Name])),
                Requirement.And and => Combine(and.Reqs),
                Requirement.Or or => Combine(or.Reqs),
                Requirement.Not not => Compile(not.Req),
                Requirement.Single single
                    when handler.TryGetOptimizedRequirement(
                        single.Req, out var helper) => Compile(helper),
                Requirement.SingleItem item
                    when handler.TryGetOptimizedRequirement(
                        item.Item.Name, out var helper) => Compile(helper),
                Requirement.Tech tech
                    when handler.TryGetOptimizedRequirement(
                        $"t_{tech.TechRequirement}", out var helper) => Compile(helper),
                Requirement.ResourceAvailable
                    or Requirement.HeatFramesWithEnergyDrops
                    or Requirement.Shinespark
                    or Requirement.EnemyKill => new(0, 0, 0, true),
                _ => default,
            };
            visiting.Remove(current);
            _requirements[current] = result;
            return result;
        }

        CompiledRequirementPlan Combine(IEnumerable<Requirement> children)
        {
            var combined = default(CompiledRequirementPlan);
            foreach (var child in children)
                combined |= Compile(child);
            return combined;
        }
    }

    public CompiledStrategyPlan GetStrategyPlan(
        Strat strategy, RequirementHandler handler)
    {
        if (_strategies.TryGetValue(strategy, out var plan))
            return plan;
        plan = new CompiledStrategyPlan(
            GetRequirementPlan(strategy.Requires, handler),
            RequirementHandler.ObstacleMaskFromArray(
                strategy.ClearsObstacles ?? []),
            RequirementHandler.ObstacleMaskFromArray(
                strategy.ResetsObstacles ?? []),
            FlagMask(strategy.SetsFlags));
        _strategies[strategy] = plan;
        return plan;
    }

    public static SmSearchModel For(IWorld world, Graph graph)
    {
        var models = ByGraph.GetValue(graph, _ => []);
        lock (models)
        {
            if (!models.TryGetValue(world, out var model))
            {
                model = new SmSearchModel(world, graph);
                models[world] = model;
            }
            return model;
        }
    }
}

internal readonly record struct CompiledRequirementPlan(
    int ObstacleMask, int DoorMask, ulong FlagMask,
    bool StateDependent = false)
{
    public static CompiledRequirementPlan operator |(
        CompiledRequirementPlan left, CompiledRequirementPlan right) => new(
        left.ObstacleMask | right.ObstacleMask,
        left.DoorMask | right.DoorMask,
        left.FlagMask | right.FlagMask,
        left.StateDependent || right.StateDependent);
}

internal readonly record struct CompiledStrategyPlan(
    CompiledRequirementPlan Requirement,
    int ClearsObstacleMask,
    int ResetsObstacleMask,
    ulong SetsFlagMask);
