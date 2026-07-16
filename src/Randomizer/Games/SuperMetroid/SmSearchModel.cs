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
    // Requirement evaluation is a pure function of the requirement's value, so
    // value-equal instances (records) can share one cache id and therefore one
    // cached result slot.
    private readonly Dictionary<Requirement, int> _requirementIdsByValue = [];
    private int _nextRequirementId;
    private RequirementResult[] _requirementResults = [];
    private int[] _requirementResultEpochs = [];
    private int _requirementCacheEpoch;
    private readonly Dictionary<StateRequirementKey,
        (int Epoch, RequirementResult Result)> _stateRequirementResults = [];

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
                    RequirementHandler.ObstacleMaskFromArray(cleared.Obstacles), 0, 0),
                Requirement.ObstaclesNotCleared notCleared => new(
                    RequirementHandler.ObstacleMaskFromArray(notCleared.Obstacles), 0, 0),
                Requirement.DoorUnlockedAtNode door => new(0, 1 << door.Node, 0),
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
                    or Requirement.Shinespark => new(0, 0, 0, true),
                // EnemyKill reads only the weapon set and inventory unless
                // details are captured, and result caching is disabled for
                // detail-capturing searches. Both inputs are fixed within one
                // requirement-cache epoch, so its results are cacheable.
                Requirement.EnemyKill => new(0, 0, 0),
                _ => default,
            };
            visiting.Remove(current);
            if (_requirementIdsByValue.TryGetValue(current, out int sharedId))
            {
                result = result with { Id = sharedId };
            }
            else
            {
                result = result with { Id = _nextRequirementId++ };
                _requirementIdsByValue[current] = result.Id;
            }
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
        // Every strategy belongs to exactly one world's data model, and each
        // (graph, world) pair owns exactly one SmSearchModel, so the compiled
        // plan can live on the strategy itself instead of a lookup table.
        if (strategy.CompiledPlan is { } plan)
            return plan;
        plan = new CompiledStrategyPlan(
            GetRequirementPlan(strategy.Requires, handler),
            RequirementHandler.ObstacleMaskFromArray(
                strategy.ClearsObstacles ?? []),
            RequirementHandler.ObstacleMaskFromArray(
                strategy.ResetsObstacles ?? []),
            FlagMask(strategy.SetsFlags));
        strategy.CompiledPlan = plan;
        return plan;
    }

    internal int BeginRequirementCacheEpoch()
    {
        if (++_requirementCacheEpoch == 0)
        {
            Array.Clear(_requirementResultEpochs);
            _stateRequirementResults.Clear();
            _requirementCacheEpoch = 1;
        }
        return _requirementCacheEpoch;
    }

    internal bool TryGetStateRequirementResult(
        int requirementId, int epoch, int obstacleBits, int doorBits,
        out RequirementResult result)
    {
        if (_stateRequirementResults.TryGetValue(
                new StateRequirementKey(requirementId, obstacleBits, doorBits),
                out var entry)
            && entry.Epoch == epoch)
        {
            result = entry.Result;
            return true;
        }
        result = default;
        return false;
    }

    internal void SetStateRequirementResult(
        int requirementId, int epoch, int obstacleBits, int doorBits,
        RequirementResult result)
    {
        if (requirementId < 0 || epoch != _requirementCacheEpoch)
            return;
        _stateRequirementResults[new StateRequirementKey(
            requirementId, obstacleBits, doorBits)] = (epoch, result);
    }

    private readonly record struct StateRequirementKey(
        int Id, int ObstacleBits, int DoorBits);

    internal bool TryGetRequirementResult(
        int requirementId, int epoch, out RequirementResult result)
    {
        if ((uint)requirementId < (uint)_requirementResultEpochs.Length
            && _requirementResultEpochs[requirementId] == epoch)
        {
            result = _requirementResults[requirementId];
            return true;
        }
        result = default;
        return false;
    }

    internal void SetRequirementResult(
        int requirementId, int epoch, RequirementResult result)
    {
        if (requirementId < 0 || epoch != _requirementCacheEpoch)
            return;
        if (requirementId >= _requirementResults.Length)
        {
            int capacity = Math.Max(
                requirementId + 1,
                Math.Max(256, _requirementResults.Length * 2));
            Array.Resize(ref _requirementResults, capacity);
            Array.Resize(ref _requirementResultEpochs, capacity);
        }
        _requirementResults[requirementId] = result;
        _requirementResultEpochs[requirementId] = epoch;
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
    bool ResourceDependent = false, int Id = -1)
{
    /// <summary>Reads traversal state, but only through the obstacle and door
    /// bits named by the masks, so results can be cached per masked value.</summary>
    public bool GraphStateDependent => (ObstacleMask | DoorMask) != 0;

    public static CompiledRequirementPlan operator |(
        CompiledRequirementPlan left, CompiledRequirementPlan right) => new(
        left.ObstacleMask | right.ObstacleMask,
        left.DoorMask | right.DoorMask,
        left.FlagMask | right.FlagMask,
        left.ResourceDependent || right.ResourceDependent,
        -1);
}

internal readonly record struct CompiledStrategyPlan(
    CompiledRequirementPlan Requirement,
    int ClearsObstacleMask,
    int ResetsObstacleMask,
    ulong SetsFlagMask);
