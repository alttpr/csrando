namespace Randomizer.Games.SuperMetroid;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;

public sealed record StatefulPathStep(
    Randomizer.Graph.Edge Edge,
    string? Strategy,
    IReadOnlyDictionary<IItem, int> Requirements,
    IReadOnlyDictionary<string, int> ResourcesSpent);

public sealed record StatefulPickup(
    IItem Item,
    string? Strategy,
    IReadOnlyDictionary<IItem, int> Requirements,
    IReadOnlyDictionary<string, int> ResourcesSpent);

public class StatefulSearcher : ISearcher
{
    // Shared instances for path steps and pickups that carry no requirement or
    // resource data. Exposed only through read-only interfaces; never mutated.
    private static readonly Dictionary<IItem, int> EmptyRequirements = [];
    private static readonly Dictionary<string, int> EmptyResources = [];
    private static readonly HashSet<string> EmptyMissing = [];

    private readonly Graph _graph;
    private readonly ForwardStateFrontier[] _visitedStates;
    private readonly List<int> _visitedStateIds = [];
    private readonly bool[] _visitedStateTouched;
    private readonly UnvisitedFrontier?[] _unvisitedStates;
    private readonly List<int> _unvisitedStateIds = [];
    private readonly Queue<(Vertex vertex, ForwardState state, StatefulPathStep? step)> _queue = new();
    private readonly ForwardStateFrontier[] _inQueue;
    private readonly List<int> _inQueueIds = [];
    private readonly bool[] _inQueueTouched;
    private readonly Dictionary<Vertex, List<ForwardArrival>> _visitedItemLocations;
    private readonly HashSet<Vertex> _visitedVertices;
    private readonly HashSet<IItem> _foundItems = [];
    private readonly Inventory _inventory;
    private readonly Dictionary<IItem, int> _restartFlags;
    private readonly Dictionary<(Vertex, IItem), ForwardState> _prevItems;
    private readonly Vertex? _target;
    private readonly Vertex _start;
    private readonly List<(Vertex, ForwardState)> _startStates;
    private readonly HashSet<Vertex> _persistentStarts;
    private readonly SetLocations? _setLocations;
    private readonly List<Randomizer.Graph.Vertex> _otherWorldLocations = [];
    private readonly HashSet<Weapon> _currentWeapons = [];
    private readonly RequirementHandler _requirementHandler;
    private readonly SmSearchModel _searchModel;
    private readonly Func<Randomizer.Graph.Vertex, bool> _collectItemAt;
    private readonly Dictionary<Randomizer.Graph.Vertex, StatefulPathStep?> _predecessors = [];
    private readonly HashSet<Randomizer.Graph.Vertex> _recordedUnlocks = [];
    private readonly Dictionary<(Randomizer.Graph.Vertex, IItem), StatefulPickup> _pickupDetails = [];
    private readonly Dictionary<(Vertex Vertex, IItem Item), ForwardState>
        _deferredBacktrackValidation = [];
    private readonly bool _capturePath;
    private readonly BacktrackRegion? _targetRegion;
    private readonly BacktrackCache _backtrackCache;
    private readonly BacktrackMetrics _backtrackMetrics;
    private readonly bool _trackMetrics;
    private readonly bool _isBacktrackSearch;
    private bool _settledRestartActive;
    private int _currentWeaponsInventoryVersion = -1;
    private Inventory? _reverseInventory;
    private int _reverseInventoryVersion = -1;
    private SearchContext _searchContext = null!;
    private int _dequeuedStateCount;
    private long _metricDequeuedStates;
    private long _metricRequirementEvaluations;
    private long _metricEnqueueAttempts;
    private long _metricEnqueues;
    private long _metricStructurallyPrunedEnqueues;
    private int _requirementCacheEpoch;
    // Path-only searches can suppress the event they are trying to explain so a
    // later, post-event route cannot be mistaken for the acquisition route.
    private readonly IItem? _excludedPickup;

    // Implement the same interface as the generic Searcher, but with a stateful implementation that can track
    // energy, ammo, and other stateful information during traversal of the graph.
    public StatefulSearcher(Graph graph, Vertex start, Inventory inventory, SetLocations? setLocations = null,
        Vertex? target = null, VisitedState? visitedState = null,
        Func<Randomizer.Graph.Vertex, bool>? collectItemAt = null,
        bool capturePath = false, IItem? excludedPickup = null)
        : this(graph, start, inventory, setLocations, target, visitedState,
            collectItemAt, capturePath, excludedPickup, null)
    {
    }

    internal StatefulSearcher(Graph graph, Vertex start, Inventory inventory,
        SetLocations? setLocations, Vertex? target, VisitedState? visitedState,
        Func<Randomizer.Graph.Vertex, bool>? collectItemAt,
        bool capturePath, IItem? excludedPickup, BacktrackRegion? targetRegion)
    {
        _inventory = inventory;
        _target = target;
        _backtrackMetrics = BacktrackMetrics.ForGraph(graph);
        _trackMetrics = _backtrackMetrics.Enabled;
        _backtrackCache = BacktrackCache.ForGraph(graph);
        _isBacktrackSearch = targetRegion != null;
        _targetRegion = target == null || _backtrackMetrics.Mode == BacktrackBenchmarkMode.Legacy
            ? null
            : targetRegion ?? BacktrackRegion.Build(graph, target);
        _start = start;
        _restartFlags = inventory.All()
            .Where(pair => IsReplayableFlag(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        _setLocations = setLocations;
        _requirementHandler = ((World)start.World).RequirementHandler;
        _searchModel = SmSearchModel.For(start.World, graph);
        _collectItemAt = collectItemAt ?? (_ => true);
        _capturePath = capturePath;
        _excludedPickup = excludedPickup;
        _otherWorldLocations.Clear();

        var startState = (start, visitedState ?? new VisitedState
        {
            Energy = 99 + inventory.GetCount(start.World.GetItem("ETank")) * 100,
            Missiles = inventory.GetCount(start.World.GetItem("Missile")) * 5,
            SuperMissiles = inventory.GetCount(start.World.GetItem("Super")) * 5,
            PowerBombs = inventory.GetCount(start.World.GetItem("PowerBomb")) * 5,
            ObstacleBitFlags = 0
        });

        _prevItems = [];
        _visitedStates = new ForwardStateFrontier[_searchModel.Capacity];
        _visitedStateTouched = new bool[_searchModel.Capacity];
        _inQueue = new ForwardStateFrontier[_searchModel.Capacity];
        _inQueueTouched = new bool[_searchModel.Capacity];
        _visitedVertices = new(1024);
        _unvisitedStates = new UnvisitedFrontier?[_searchModel.Capacity];
        _visitedItemLocations = new(128);
        _graph = graph;
        _startStates = [];
        _persistentStarts = [start];

        List<(Vertex, VisitedState)> startStates = [startState];
        Search(startStates);

    }

    public void Search(List<(Vertex, VisitedState)> initialStates)
        => SearchForward(initialStates.Select(start => (
            start.Item1,
            ForwardState.FromVisited(start.Item2, _inventory,
                (World)start.Item1.World))).ToList());

    private void SearchForward(List<(Vertex, ForwardState)> initialStates)
    {
        var foundItems = new Dictionary<(Vertex, IItem), ForwardState>();
        var newItems = new Dictionary<(Vertex, IItem), ForwardState>();
        bool promotedAnyItems = false;

        foreach (var (vertex, state) in initialStates)
        {
            _startStates.Add((vertex, state));
        }

        do
        {
            foundItems = RunSearchPass(_startStates, _inventory, _target);

            newItems = foundItems.Where(x => !_prevItems.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Value);
            foreach (var foundItem in foundItems)
            {
                if (!_prevItems.ContainsKey(foundItem.Key))
                {
                    _prevItems.Add(foundItem.Key, foundItem.Value);
                }
                else
                {
                    _prevItems[foundItem.Key] = foundItem.Value;
                }
            }
            foreach (int vertexId in _visitedStateIds)
            {
                if (!_visitedStates[vertexId].IsEmpty)
                    _visitedVertices.Add(_searchModel.VerticesById[vertexId]!);
            }

            if (_backtrackMetrics.Mode == BacktrackBenchmarkMode.Legacy)
            {
                // Preserve the old sequential behavior as the benchmark baseline.
                foreach (var ((vtx, item), itemState) in newItems.ToArray())
                {
                    _inventory.AddItem(item);
                    if (_target == null && !vtx.Name.Contains("Tourian")
                        && !item.Name.StartsWith("f_", StringComparison.Ordinal)
                        && !EvaluateBacktrack(
                            vtx, itemState, _inventory.Clone(), _start))
                    {
                        RejectNewItem(vtx, item, itemState);
                    }
                }
            }
            else
            {
                // Forward-produced flags are global graph state, including
                // flags produced on a side branch rather than the final return
                // route. Promote those first, then resolve the item locations
                // against one shared reverse pass.
                var unconditional = newItems
                    .Where(pair => _target != null
                        || pair.Key.Item1.Name.Contains("Tourian")
                        || pair.Key.Item2.Name.StartsWith(
                            "f_", StringComparison.Ordinal))
                    .ToArray();
                foreach (var ((_, item), _) in unconditional)
                    _inventory.AddItem(item);

                var backtrackCandidates = newItems
                    .Where(pair => !pair.Key.Item2.Name.StartsWith(
                        "f_", StringComparison.Ordinal)
                        && _target == null
                        && !pair.Key.Item1.Name.Contains("Tourian"))
                    .Select(pair => (pair.Key.Item1, pair.Key.Item2, pair.Value))
                    .ToList();
                foreach (var (_, item, _) in backtrackCandidates)
                    _inventory.AddItem(item);

                // All pickups in the forward sphere are concrete actions that
                // can be collected before returning. Resolve them together,
                // remove failures, and rebuild only when that removal changes
                // the shared reverse inventory.
                while (backtrackCandidates.Count > 0)
                {
                    var reverseInventory = GetReverseInventory();
                    var rejected = backtrackCandidates
                        .Where(candidate => !EvaluateBacktrack(
                            candidate.Item1, candidate.Value,
                            reverseInventory, _start,
                            deferNegativeValidation: true))
                        .ToArray();
                    if (rejected.Length == 0)
                        break;

                    foreach (var (vtx, item, itemState) in rejected)
                    {
                        _deferredBacktrackValidation[(vtx, item)] = itemState;
                        RejectNewItem(vtx, item, itemState);
                    }
                    var rejectedKeys = rejected
                        .Select(candidate => (candidate.Item1, candidate.Item2))
                        .ToHashSet();
                    backtrackCandidates.RemoveAll(candidate =>
                        rejectedKeys.Contains((candidate.Item1, candidate.Item2)));
                }

                foreach (var (vtx, item, _) in backtrackCandidates)
                    _deferredBacktrackValidation.Remove((vtx, item));
            }

            void RejectNewItem(
                Vertex vtx, IItem item, ForwardState itemState)
            {
                _inventory.RemoveItem(item);
                newItems.Remove((vtx, item));
                _prevItems.Remove((vtx, item));

                var states = _unvisitedStates[vtx.Id];
                if (states != null)
                {
                    states.MissingItems.Add("Backtrack");
                }
                else
                {
                    _unvisitedStates[vtx.Id] = new UnvisitedFrontier(
                        ["Backtrack"], [itemState]);
                    _unvisitedStateIds.Add(vtx.Id);
                }
            }

            promotedAnyItems |= newItems.Count > 0;

            _startStates.Clear();
            var checkItems = newItems.Select(x => x.Key.Item2.Name).ToHashSet();
            checkItems.Add("Backtrack");
            foreach (int vertexId in _unvisitedStateIds)
            {
                var states = _unvisitedStates[vertexId];
                if (states != null && SetsOverlap(states.MissingItems, checkItems))
                {
                    var vertex = _searchModel.VerticesById[vertexId]!;
                    foreach (var state in states.States)
                    {
                        _startStates.Add((vertex, state));
                    }
                }
            }

            // Debt states are independent of capacity. Only vertices selected for
            // dependency re-evaluation need to leave the settled frontier.
            foreach (var (vertex, _) in _startStates)
                _visitedStates[vertex.Id].Clear();
        } while (newItems.Count > 0);

        if (_target == null && !_capturePath && !_settledRestartActive
            && promotedAnyItems)
        {
            bool foundNewPhysicalItem;
            do
            {
                var previousPhysicalItems = _prevItems.Keys
                    .Where(key => !IsReplayableFlag(key.Item2))
                    .ToHashSet();
                ResetTraversalForSettledRestart();
                _settledRestartActive = true;
                try
                {
                    SearchForward(_persistentStarts
                        .Select(vertex => (vertex, ForwardState.Empty))
                        .ToList());
                }
                finally
                {
                    _settledRestartActive = false;
                }
                foundNewPhysicalItem = _prevItems.Keys.Any(key =>
                    !IsReplayableFlag(key.Item2)
                    && !previousPhysicalItems.Contains(key));
            } while (foundNewPhysicalItem);
        }

        ValidateSettledBacktrackRejects();

        _foundItems.Clear();
        _foundItems.UnionWith(_prevItems.Select(x => x.Key.Item2));
    }

    private void ResetTraversalForSettledRestart()
    {
        foreach (var flag in _inventory.All()
                     .Where(pair => IsReplayableFlag(pair.Key))
                     .ToArray())
            _inventory.RemoveItem(flag.Key, flag.Value);
        foreach (var (flag, count) in _restartFlags)
            _inventory.AddItem(flag, count);
        foreach (var key in _prevItems.Keys
                     .Where(key => IsReplayableFlag(key.Item2))
                     .ToArray())
            _prevItems.Remove(key);

        foreach (int vertexId in _visitedStateIds)
        {
            _visitedStates[vertexId].Clear();
            _visitedStateTouched[vertexId] = false;
        }
        _visitedStateIds.Clear();
        foreach (int vertexId in _unvisitedStateIds)
            _unvisitedStates[vertexId] = null;
        _unvisitedStateIds.Clear();
        _visitedVertices.Clear();
        _visitedItemLocations.Clear();
        _startStates.Clear();
        _queue.Clear();
        foreach (int vertexId in _inQueueIds)
        {
            _inQueue[vertexId].Clear();
            _inQueueTouched[vertexId] = false;
        }
        _inQueueIds.Clear();
        _otherWorldLocations.Clear();
        _currentWeapons.Clear();
        _currentWeaponsInventoryVersion = -1;
        _deferredBacktrackValidation.Clear();
        _predecessors.Clear();
        _recordedUnlocks.Clear();
        _pickupDetails.Clear();
    }

    private bool IsReplayableFlag(IItem item) =>
        ReferenceEquals(item.World, _start.World)
        && item.Name.StartsWith("f_", StringComparison.Ordinal);

    private void AddUnvisited(Vertex vertex, ForwardState state, HashSet<string> missingItems)
    {
        if (missingItems.Count == 0)
        {
            return;
        }

        var states = _unvisitedStates[vertex.Id];
        if (states != null)
        {
            states.MissingItems.UnionWith(missingItems);
            AddNondominated(states.States, state);
        }
        else
        {
            _unvisitedStates[vertex.Id] = new UnvisitedFrontier(
                new HashSet<string>(missingItems, StringComparer.Ordinal), [state]);
            _unvisitedStateIds.Add(vertex.Id);
        }
    }

    private Dictionary<(Vertex, IItem), ForwardState> InternalSearch(List<(Vertex, ForwardState)> starts, Inventory inventory, Vertex? target = null)
    {
        var foundItems = new Dictionary<(Vertex, IItem), ForwardState>();
        _queue.Clear();
        foreach (int vertexId in _inQueueIds)
        {
            _inQueue[vertexId].Clear();
            _inQueueTouched[vertexId] = false;
        }
        _inQueueIds.Clear();
        // The usable weapon set depends only on the inventory, which does not
        // change within a pass, so recompute it only when the inventory has.
        if (_currentWeaponsInventoryVersion != inventory.Version)
        {
            _currentWeaponsInventoryVersion = inventory.Version;
            _currentWeapons.Clear();
            _currentWeapons.UnionWith(((World)_start.World).JsonData.Weapons.Weapons
                .Where(weapon =>
                {
                    if (_trackMetrics)
                        _metricRequirementEvaluations++;
                    return _requirementHandler.HandleRequirement(
                        weapon.UseRequires, new VisitedState(), inventory,
                        (World)_start.World, [], captureDetails: false).Met;
                }));
        }

        foreach (var start in starts)
        {
            EnqueueState(start.Item1, start.Item2, null);
        }

        while (_queue.Count > 0)
        {
            var (current, state, incomingStep) = DequeueState()!.Value;
            if (_trackMetrics)
                _metricDequeuedStates++;
            _dequeuedStateCount++;

            // If we've already visited this vertex with a state that dominates the current state, skip it
            ref var visitedStates = ref _visitedStates[current.Id];
            bool firstStateAtVertex = visitedStates.IsEmpty;
            if (!visitedStates.AddNondominated(state))
                continue;

            // Add the current state to the visited states for this vertex
            if (firstStateAtVertex)
            {
                if (!_visitedStateTouched[current.Id])
                {
                    _visitedStateTouched[current.Id] = true;
                    _visitedStateIds.Add(current.Id);
                }
            }
            if (_capturePath)
                _predecessors.TryAdd(current, incomingStep);


            if (target != null && current == target)
            {
                return [];
            }
            var currentNode = current.Node;
            if (currentNode == null)
            {
                // This is some kind of meta node, just resolve edges without any strats
                foreach (var edge in current.Edges)
                {
                    if (edge.To.World != current.World)
                    {
                        if (target != null)
                        {
                            if (!_visitedStateTouched[target.Id])
                            {
                                _visitedStateTouched[target.Id] = true;
                                _visitedStateIds.Add(target.Id);
                            }
                            _visitedStates[target.Id].SetSingle(state);
                            return [];
                        }

                        _otherWorldLocations.Add(edge.To);
                        continue;
                    }

                    var toVtx = (Vertex)edge.To;
                    EnqueueState(toVtx, state, _capturePath
                        ? new StatefulPathStep(edge, null, EmptyRequirements,
                            EmptyResources)
                        : null);
                }
                continue;
            }


            bool unlocked = false;
            var (unlockState, yields, unlockStrategy, unlockResult) =
                UnlockNode(inventory, current, state, currentNode);
            Dictionary<IItem, int>? unlockRequirements = null;
            Dictionary<string, int>? unlockResources = null;
            bool unlockRecordedOnIncoming = false;
            if (unlockState != null)
            {
                if (_capturePath)
                    unlockResources = ResourcesSpent(state, unlockState.Value);
                state = unlockState.Value;
                if (_capturePath && unlockResult != null)
                {
                    AppendStepRequirements(ref unlockRequirements,
                        unlockResult.Value, current.World);
                }
                if (_capturePath && _predecessors[current] is { } predecessor
                    && _recordedUnlocks.Add(current)
                    && (unlockStrategy != null
                        || unlockRequirements is { Count: > 0 }))
                {
                    _predecessors[current] = MergePathStep(
                        predecessor, unlockStrategy,
                        unlockRequirements ?? EmptyRequirements,
                        unlockResources ?? EmptyResources);
                    unlockRecordedOnIncoming = true;
                }
                else if (_capturePath && _recordedUnlocks.Contains(current)
                    && _predecessors[current] != null)
                    unlockRecordedOnIncoming = true;
                if (current.Node!.NodeType == "door")
                {
                    state = state.WithDoorUnlocked(current.NodeId);
                }

                unlocked = true;
                if (yields != null)
                {
                    foreach (var yieldItem in yields)
                    {
                        if (ReferenceEquals(yieldItem.Key.Item2, _excludedPickup))
                            continue;
                        if (foundItems.TryGetValue(
                                yieldItem.Key, out var existingState))
                        {
                            if (yieldItem.Value.Dominates(existingState))
                                foundItems[yieldItem.Key] = yieldItem.Value;
                        }
                        else
                        {
                            foundItems.Add(yieldItem.Key, yieldItem.Value);
                        }
                    }
                }
                if (_capturePath && yields != null)
                {
                    foreach (var (yieldLocation, yieldItem) in yields.Keys)
                    {
                        _pickupDetails.TryAdd((yieldLocation, yieldItem),
                            new StatefulPickup(yieldItem, unlockStrategy,
                                unlockRequirements ?? EmptyRequirements,
                                unlockResources ?? EmptyResources));
                    }
                }
            }

            if (unlocked && current.Type == VertexType.Item)
            {
                RecordForwardArrival(current, state);

                if (_capturePath && current.Item != null)
                    _pickupDetails.TryAdd((current, current.Item),
                        new StatefulPickup(current.Item, unlockStrategy,
                            unlockRequirements ?? EmptyRequirements,
                            unlockResources ?? EmptyResources));

                if (current.Item != null && !ReferenceEquals(current.Item, _excludedPickup)
                    && _collectItemAt(current))
                {
                    if (foundItems.TryGetValue((current, current.Item), out var existingState))
                    {
                        if (state.Dominates(existingState))
                        {
                            foundItems[(current, current.Item)] = state;
                        }
                    }
                    else
                    {
                        foundItems.Add((current, current.Item), state);
                    }
                }
            }

            foreach (var edge in current.Edges)
            {
                if (edge.To.World != current.World)
                {
                    // TODO: Maybe this should be a bit more sophisticated, but for now it makes sense to skip backtracking when we change worlds
                    if (target != null)
                    {
                        if (!_visitedStateTouched[target.Id])
                        {
                            _visitedStateTouched[target.Id] = true;
                            _visitedStateIds.Add(target.Id);
                        }
                        _visitedStates[target.Id].SetSingle(state);
                        return [];
                    }

                    _otherWorldLocations.Add(edge.To);
                    continue;
                }

                // Can't traverse doors if we couldn't unlock the node
                if (!unlocked && ((Vertex)edge.To).RoomId != current.RoomId)
                {
                    continue;
                }

                if (edge is not SuperMetroid.Edge)
                {
                    // This is a regular base edge, so no strats to consider, just enqueue the next vertex
                    EnqueueState((Vertex)edge.To, state, _capturePath
                        ? new StatefulPathStep(edge,
                            unlockRecordedOnIncoming ? null : unlockStrategy,
                            unlockRecordedOnIncoming
                                ? EmptyRequirements
                                : unlockRequirements ?? EmptyRequirements,
                            unlockRecordedOnIncoming
                                ? EmptyResources
                                : unlockResources ?? EmptyResources)
                        : null);
                    continue;
                }

                bool foundStrategy = false;
                Strat bestStrat = null!;
                ForwardState bestState = default;
                RequirementResult bestResult = default;
                foreach (var strat in ((Edge)edge).Strats ?? [])
                {
                    var strategyPlan = _searchModel.GetStrategyPlan(
                        strat, _requirementHandler);
                    var result = HandleRequirement(
                        strat.Requires, strategyPlan.Requirement, state,
                        inventory, (World)current.World, _currentWeapons);
                    if (!result.Met)
                    {
                        AddUnvisited(current, state, result.Missing ?? EmptyMissing);
                        continue;
                    }

                    var cost = result.Cost!;

                    var newState = state.ApplyCost(cost.Value, _searchContext);
                    if (newState == null)
                    {
                        // Figure out what costs we're missing
                        var missingCostItems = new HashSet<string>(result.Missing ?? EmptyMissing);
                        var available = state.ToVisited(_searchContext);
                        if (available.Energy - cost.Value.Energy <= 0) { missingCostItems.Add("ETank"); }
                        if (available.Missiles - cost.Value.Missiles <= 0) { missingCostItems.Add("Missile"); }
                        if (available.SuperMissiles - cost.Value.SuperMissiles <= 0) { missingCostItems.Add("Super"); }
                        if (available.PowerBombs - cost.Value.PowerBombs <= 0) { missingCostItems.Add("PowerBomb"); }


                        AddUnvisited(current, state, missingCostItems);
                        continue;
                    }

                    var finalState = newState.Value
                        .WithObstacles(strategyPlan.ClearsObstacleMask)
                        .WithoutObstacles(strategyPlan.ResetsObstacleMask);

                    if (_capturePath)
                    {
                        Dictionary<IItem, int>? stratRequirements = null;
                        AppendStepRequirements(
                            ref stratRequirements, result, current.World);
                        foreach (var flag in strat.SetsFlags ?? [])
                        {
                            var flagItem = current.World.GetItem(flag);
                            _pickupDetails.TryAdd((current, flagItem),
                                new StatefulPickup(flagItem, strat.Name,
                                    stratRequirements ?? EmptyRequirements,
                                    ResourcesSpent(state, finalState)));
                        }
                    }

                    // If any of the strats we "could" use sets flags, add them to the found items
                    if (strat.SetsFlags != null)
                    {
                        foreach (var flag in strat.SetsFlags)
                        {
                            var flagItem = current.World.GetItem(flag);
                            if (ReferenceEquals(flagItem, _excludedPickup))
                                continue;
                            if (foundItems.TryGetValue((current, flagItem), out var existingState))
                            {
                                if (finalState.Dominates(existingState))
                                {
                                    foundItems[(current, flagItem)] = finalState;
                                }
                            }
                            else
                            {
                                foundItems.Add((current, flagItem), finalState);
                            }
                        }
                    }

                    // Preserve strategy order and the established behavior where
                    // a later strategy replaces the current choice only when its
                    // resulting state fully dominates it (including equality).
                    if (!foundStrategy || finalState.Dominates(bestState))
                    {
                        bestState = finalState;
                        bestStrat = strat;
                        bestResult = result;
                    }
                    foundStrategy = true;
                }

                if (!foundStrategy)
                    continue;

                Vertex toVtx = (Vertex)edge.To;
                if (toVtx.RoomId != current.RoomId)
                {
                    bestState = bestState with
                    {
                        ObstacleBitFlags = 0,
                        DoorUnlockedFlags = 0,
                    };
                }

                StatefulPathStep? pathStep = null;
                if (_capturePath)
                {
                    Dictionary<IItem, int>? requirements = null;
                    AppendStepRequirements(
                        ref requirements, bestResult, current.World);
                    if (!unlockRecordedOnIncoming)
                    {
                        foreach (var (item, count) in
                                 unlockRequirements ?? EmptyRequirements)
                        {
                            requirements ??= [];
                            requirements[item] = Math.Max(
                                requirements.GetValueOrDefault(item), count);
                        }
                    }
                    pathStep = new StatefulPathStep(edge,
                        JoinStrategies(
                            unlockRecordedOnIncoming ? null : unlockStrategy,
                            bestStrat.Name), requirements ?? EmptyRequirements,
                        MergeResources(
                            unlockRecordedOnIncoming
                                ? EmptyResources
                                : unlockResources ?? EmptyResources,
                            ResourcesSpent(state, bestState)));
                }
                EnqueueState(toVtx, bestState, pathStep);
            }
        }

        return foundItems;
    }

    private RequirementResult HandleRequirement(
        Requirement requirement, ForwardState state, Inventory inventory,
        World world, HashSet<Weapon> weapons)
        => HandleRequirement(
            requirement,
            _searchModel.GetRequirementPlan(requirement, _requirementHandler),
            state, inventory, world, weapons);

    private RequirementResult HandleRequirement(
        Requirement requirement, in CompiledRequirementPlan plan,
        ForwardState state, Inventory inventory,
        World world, HashSet<Weapon> weapons)
    {
        if (_trackMetrics)
            _metricRequirementEvaluations++;
        // Results are cacheable per epoch unless they read resources: the
        // inventory, flag set, and weapon set are all fixed within one epoch,
        // and graph-state readers depend only on the masked obstacle/door bits
        // that become part of the cache key.
        bool cacheable = !_capturePath && !plan.ResourceDependent;
        if (cacheable)
        {
            if (!plan.GraphStateDependent)
            {
                if (_searchModel.TryGetRequirementResult(
                        plan.Id, _requirementCacheEpoch, out var cached))
                    return cached;
            }
            else if (_searchModel.TryGetStateRequirementResult(
                         plan.Id, _requirementCacheEpoch,
                         state.ObstacleBitFlags & plan.ObstacleMask,
                         state.DoorUnlockedFlags & plan.DoorMask,
                         out var cachedState))
            {
                return cachedState;
            }
        }

        var result = _requirementHandler.HandleRequirement(
            requirement, state.ToVisited(_searchContext), inventory, world,
            weapons, captureDetails: _capturePath);
        if (cacheable)
        {
            if (!plan.GraphStateDependent)
                _searchModel.SetRequirementResult(
                    plan.Id, _requirementCacheEpoch, result);
            else
                _searchModel.SetStateRequirementResult(
                    plan.Id, _requirementCacheEpoch,
                    state.ObstacleBitFlags & plan.ObstacleMask,
                    state.DoorUnlockedFlags & plan.DoorMask, result);
        }
        return result;
    }

    private void EnqueueState(Vertex v, ForwardState s, StatefulPathStep? step)
    {
        if (_trackMetrics)
            _metricEnqueueAttempts++;
        // A target search cannot succeed from a vertex with no directed path to
        // either its target or a cross-world exit. This reverse-graph filter is
        // state-agnostic and therefore cannot remove a valid route.
        if (_targetRegion != null && !_targetRegion.Contains(v))
        {
            if (_trackMetrics && _isBacktrackSearch)
                _metricStructurallyPrunedEnqueues++;
            return;
        }

        ref var list = ref _inQueue[v.Id];
        if (list.IsEmpty)
        {
            if (!_inQueueTouched[v.Id])
            {
                _inQueueTouched[v.Id] = true;
                _inQueueIds.Add(v.Id);
            }
        }

        if (!list.AddNondominated(s))
            return;
        if (_trackMetrics)
            _metricEnqueues++;
        _queue.Enqueue((v, s, step));
    }

    private Dictionary<(Vertex, IItem), ForwardState> RunSearchPass(
        List<(Vertex, ForwardState)> starts, Inventory inventory, Vertex? target)
    {
        _metricDequeuedStates = 0;
        _metricRequirementEvaluations = 0;
        _metricEnqueueAttempts = 0;
        _metricEnqueues = 0;
        _metricStructurallyPrunedEnqueues = 0;
        _requirementCacheEpoch = _searchModel.BeginRequirementCacheEpoch();
        _searchContext = new SearchContext(inventory, (World)_start.World);

        var result = InternalSearch(starts, inventory, target);
        if (_trackMetrics)
        {
            _backtrackMetrics.RecordForwardWork(
                _metricDequeuedStates,
                _metricRequirementEvaluations,
                _metricEnqueueAttempts,
                _metricEnqueues,
                _metricStructurallyPrunedEnqueues);
        }
        return result;
    }

    private (Vertex, ForwardState, StatefulPathStep?)? DequeueState()
    {
        if (!_queue.TryDequeue(out var queued))
            return null;

        var (vertex, state, step) = queued;
        ref var list = ref _inQueue[vertex.Id];
        if (!list.IsEmpty)
        {
            list.RemoveEqual(state);
        }
        return (vertex, state, step);
    }

    /// <summary>Equivalent to <see cref="HashSet{T}.Overlaps(IEnumerable{T})"/>
    /// for two hash sets, without boxing a struct enumerator.</summary>
    private static bool SetsOverlap(HashSet<string> first, HashSet<string> second)
    {
        var (smaller, larger) = first.Count <= second.Count
            ? (first, second)
            : (second, first);
        foreach (var item in smaller)
        {
            if (larger.Contains(item))
                return true;
        }
        return false;
    }

    private static bool AddNondominated(
        List<ForwardState> frontier, ForwardState candidate)
    {
        for (int index = 0; index < frontier.Count; index++)
        {
            if (frontier[index].Dominates(candidate))
                return false;
        }

        int writeIndex = 0;
        for (int index = 0; index < frontier.Count; index++)
        {
            var existing = frontier[index];
            if (!candidate.Dominates(existing))
                frontier[writeIndex++] = existing;
        }
        if (writeIndex < frontier.Count)
            frontier.RemoveRange(writeIndex, frontier.Count - writeIndex);
        frontier.Add(candidate);
        return true;
    }

    /// <summary>Fold the requirement result's used items and resource costs
    /// into a step requirements dictionary, allocating it only when there is
    /// something to record.</summary>
    private static void AppendStepRequirements(
        ref Dictionary<IItem, int>? requirements, in RequirementResult result,
        IWorld world)
    {
        foreach (var (item, count) in result.UsedItems ?? EmptyResources)
        {
            requirements ??= [];
            requirements[world.GetItem(item)] = count;
        }
        AddResourceRequirements(ref requirements, result.Cost!.Value, world);
    }

    private static void AddResourceRequirements(
        ref Dictionary<IItem, int>? requirements, RequirementCost cost,
        IWorld world)
    {
        AddResourceRequirement(ref requirements, "Energy", cost.Energy, world);
        AddResourceRequirement(ref requirements, "Missile", cost.Missiles, world);
        AddResourceRequirement(
            ref requirements, "Super", cost.SuperMissiles, world);
        AddResourceRequirement(
            ref requirements, "PowerBomb", cost.PowerBombs, world);
    }

    private static void AddResourceRequirement(
        ref Dictionary<IItem, int>? requirements, string type, int amount,
        IWorld world)
    {
        // AmmoDrain stores a drain marker in bit 15. It consumes whatever
        // resource is present (up to the encoded amount), so it is not a
        // capacity requirement and must not be converted into item packs.
        if (amount > 0x8000)
            return;
        var (itemName, count) = RequirementHandler.RequiredExpansion(type, amount);
        if (count <= 0)
            return;
        var item = world.GetItem(itemName);
        requirements ??= [];
        requirements[item] = Math.Max(requirements.GetValueOrDefault(item), count);
    }

    private static StatefulPathStep MergePathStep(
        StatefulPathStep step, string? strategy, IReadOnlyDictionary<IItem, int> requirements,
        IReadOnlyDictionary<string, int> resourcesSpent)
    {
        var mergedRequirements = step.Requirements.ToDictionary();
        foreach (var (item, count) in requirements)
            mergedRequirements[item] = Math.Max(mergedRequirements.GetValueOrDefault(item), count);
        return step with
        {
            Strategy = JoinStrategies(step.Strategy, strategy),
            Requirements = mergedRequirements,
            ResourcesSpent = MergeResources(step.ResourcesSpent, resourcesSpent),
        };
    }

    private Dictionary<string, int> ResourcesSpent(ForwardState before, ForwardState after)
    {
        var beforeVisited = before.ToVisited(_searchContext);
        var afterVisited = after.ToVisited(_searchContext);
        Dictionary<string, int>? resources = null;
        if (beforeVisited.Energy > afterVisited.Energy)
            (resources ??= [])["Energy"] = beforeVisited.Energy - afterVisited.Energy;
        if (beforeVisited.Missiles > afterVisited.Missiles)
            (resources ??= [])["Missiles"] = beforeVisited.Missiles - afterVisited.Missiles;
        if (beforeVisited.SuperMissiles > afterVisited.SuperMissiles)
            (resources ??= [])["Super Missiles"] = beforeVisited.SuperMissiles - afterVisited.SuperMissiles;
        if (beforeVisited.PowerBombs > afterVisited.PowerBombs)
            (resources ??= [])["Power Bombs"] = beforeVisited.PowerBombs - afterVisited.PowerBombs;
        return resources ?? EmptyResources;
    }

    private static Dictionary<string, int> MergeResources(
        IReadOnlyDictionary<string, int> first, IReadOnlyDictionary<string, int> second)
    {
        if (first.Count == 0 && second.Count == 0)
            return EmptyResources;
        var merged = first.ToDictionary();
        foreach (var (resource, amount) in second)
            merged[resource] = merged.GetValueOrDefault(resource) + amount;
        return merged;
    }

    private static string? JoinStrategies(string? first, string? second) =>
        first == null ? second : second == null || second == first ? first : $"{first}; {second}";

    private (ForwardState?, Dictionary<(Vertex, IItem), ForwardState>?, string?, RequirementResult?)
        UnlockNode(Inventory inventory, Vertex current, ForwardState lockState, Node currentNode)
    {
        Dictionary<(Vertex, IItem), ForwardState>? yields = null;
        string? usedStrategies = null;
        var combinedResult = RequirementResult.Success(
            RequirementCost.ZeroCost);

        if (currentNode.Locks != null)
        {
            foreach (var lck in currentNode.Locks)
            {
                // Check if this lock requires a specific item or flag to be locked, and if we don't fullfill the lock requirements, skip it
                if (lck.Lock != null)
                {
                    var lockResult = HandleRequirement(
                        lck.Lock, lockState, inventory,
                        (World)current.World, _currentWeapons);
                    if (!lockResult.Met)
                    {
                        AddUnvisited(current, lockState, lockResult.Missing ?? EmptyMissing);
                        continue;
                    }
                }

                bool foundStrategy = false;
                Strat bestStrat = null!;
                ForwardState bestState = default;
                RequirementResult bestResult = default;

                foreach (var unlockStrat in lck.UnlockStrats ?? [])
                {
                    var strategyPlan = _searchModel.GetStrategyPlan(
                        unlockStrat, _requirementHandler);
                    var result = HandleRequirement(
                        unlockStrat.Requires, strategyPlan.Requirement,
                        lockState, inventory,
                        (World)current.World, _currentWeapons);
                    if (!result.Met)
                    {
                        AddUnvisited(current, lockState, result.Missing ?? EmptyMissing);
                        continue;
                    }

                    var cost = result.Cost!;

                    var newState = lockState.ApplyCost(cost.Value, _searchContext);
                    if (newState == null)
                    {
                        var missingCostItems = new HashSet<string>(result.Missing ?? EmptyMissing);
                        var available = lockState.ToVisited(_searchContext);
                        if (available.Energy - cost.Value.Energy <= 0) { missingCostItems.Add("ETank"); }
                        if (available.Missiles - cost.Value.Missiles <= 0) { missingCostItems.Add("Missile"); }
                        if (available.SuperMissiles - cost.Value.SuperMissiles <= 0) { missingCostItems.Add("Super"); }
                        if (available.PowerBombs - cost.Value.PowerBombs <= 0) { missingCostItems.Add("PowerBomb"); }
                        AddUnvisited(current, lockState, missingCostItems);
                        continue;
                    }

                    var finalState = newState.Value
                        .WithObstacles(strategyPlan.ClearsObstacleMask)
                        .WithoutObstacles(strategyPlan.ResetsObstacleMask);

                    if (!foundStrategy || finalState.Dominates(bestState))
                    {
                        bestState = finalState;
                        bestStrat = unlockStrat;
                        bestResult = result;
                    }
                    foundStrategy = true;
                }

                if (!foundStrategy)
                    return (null, null, null, null);

                lockState = bestState;
                if (_capturePath)
                {
                    usedStrategies = JoinStrategies(
                        usedStrategies, bestStrat.Name);
                    combinedResult.Cost += bestResult.Cost!.Value;
                    combinedResult.MergeSuccess(bestResult);
                }
                foreach (var yield in lck.Yields ?? [])
                {
                    var yieldItem = current.World.GetItem(yield);
                    (yields ??= []).Add((current, yieldItem), lockState);
                }
            }
        }

        return (lockState, yields, usedStrategies,
            usedStrategies == null ? null : combinedResult);
    }

    IEnumerable<Randomizer.Graph.Vertex> ISearcher.GetEmptyLocationsInSet(ItemSetName itemSet, Dictionary<ItemSetName, int>? itemSets, bool onlyReachable)
    {
        if (_setLocations is null)
            throw new InvalidOperationException("Set locations are not available for this searcher instance.");

        var emptyLocations = _setLocations[itemSet].Where((vertex) =>
        {
            return (!onlyReachable || (_visitedVertices.Contains(vertex) && _visitedItemLocations.ContainsKey((Vertex)vertex))) && vertex.Item == null;
        }).OrderBy(v => v.Name).ToList();

        itemSets ??= [];
        foreach (var (setName, setCount) in itemSets)
        {
            if (setName.World == null)
                continue;

            var setLocations = _setLocations[setName].Where(static (location) => location.Item == null);
            if (setLocations.Count() < setCount)
                throw new Exception($"Not enough set locations available: {setName}");
            // if a set has the same number of items to place as set locations
            // left, remove it from this return.
            if (itemSet != setName && setLocations.Count() == setCount)
                emptyLocations.RemoveAll(setLocations.Contains);
        }

        return emptyLocations.ToArray();
    }

    IEnumerable<Randomizer.Graph.Vertex> ISearcher.GetVisited()
    {
        return _visitedVertices;
    }

    public bool HasVisited(Randomizer.Graph.Vertex vertex)
    {
        return _visitedVertices.Contains(vertex);
    }

    public bool HasFound(IItem item)
    {
        return _foundItems.Contains(item);
    }

    /// <summary>Get the inventory resolved by this search.</summary>
    public Inventory GetInventory()
    {
        return _inventory.Clone();
    }

    public bool BacktrackLocation(
        Vertex vertex, Vertex target, IItem itemToPlace)
    {
        // A location this searcher never reached (e.g. one only reachable with items
        // found in other games) has no state to backtrack from.
        if (!_visitedItemLocations.TryGetValue(vertex, out var arrivals))
            return false;

        // Use the inventory fixed point resolved by the forward search for every
        // reverse query from this searcher. This mirrors the paired traversal
        // model: flags and fixed pickups that became safely collectable are global
        // input to both passes, while only the local resource states differ by
        // vertex. Using each arrival's intermediate inventory here caused an
        // almost unique reverse traversal to be built for every candidate.
        var reverseInventory = GetReverseInventory();

        foreach (var arrival in arrivals)
        {
            if (EvaluateBacktrack(
                    vertex, arrival.State, reverseInventory, target))
                return true;
        }

        return false;
    }

    // Kept for source compatibility with callers written against the old API.
    // Reverse traversal uses the searcher's settled inventory, not this snapshot.
    public bool BacktrackLocation(
        Vertex vertex, Inventory inventory, Vertex target, IItem itemToPlace) =>
        BacktrackLocation(vertex, target, itemToPlace);

    /// <summary>A read-only snapshot of the searcher's inventory for reverse
    /// queries, reused until the inventory changes. Callers must not mutate it.</summary>
    private Inventory GetReverseInventory()
    {
        if (_reverseInventory == null
            || _reverseInventoryVersion != _inventory.Version)
        {
            _reverseInventory = _inventory.Clone();
            _reverseInventoryVersion = _inventory.Version;
        }
        return _reverseInventory;
    }

    private void RecordForwardArrival(
        Vertex vertex, ForwardState state)
    {
        if (!_visitedItemLocations.TryGetValue(vertex, out var arrivals))
        {
            _visitedItemLocations[vertex] =
                [new ForwardArrival(state)];
            return;
        }

        for (int index = 0; index < arrivals.Count; index++)
        {
            if (arrivals[index].State.Dominates(state))
                return;
        }

        int writeIndex = 0;
        for (int index = 0; index < arrivals.Count; index++)
        {
            var arrival = arrivals[index];
            if (!state.Dominates(arrival.State))
                arrivals[writeIndex++] = arrival;
        }
        if (writeIndex < arrivals.Count)
            arrivals.RemoveRange(writeIndex, arrivals.Count - writeIndex);
        arrivals.Add(new ForwardArrival(state));
    }

    private bool EvaluateBacktrack(
        Vertex vertex, ForwardState startState, Inventory inventory, Vertex target,
        bool deferNegativeValidation = false)
    {
        _backtrackMetrics.RecordCheck();
        var arrivalState = startState.ToVisited(inventory, (World)vertex.World);
        if (_backtrackMetrics.Mode == BacktrackBenchmarkMode.Legacy)
            return RunLegacyBacktrack(
                vertex, arrivalState, inventory, target, null, out _);

        var targetRegion = GetBacktrackRegion(target);
        if (!targetRegion.Contains(vertex))
        {
            _backtrackMetrics.RecordStructuralReject();
            return false;
        }

        var reverseSearch = GetReverseBacktrackSearch(
            target, targetRegion, inventory);
        bool canReturn = reverseSearch.CanReturn(vertex, arrivalState);
        if (canReturn || !deferNegativeValidation)
            ValidateReverseResult(
                canReturn, vertex, arrivalState, inventory, target, targetRegion);
        if (canReturn)
        {
            _backtrackMetrics.RecordReverseProof();
            return true;
        }
        _backtrackMetrics.RecordReverseReject();
        return false;
    }

    private void ValidateSettledBacktrackRejects()
    {
        if (_target != null
            || _backtrackMetrics.Mode != BacktrackBenchmarkMode.Validate)
            return;

        foreach (var ((vertex, item), state) in
                 _deferredBacktrackValidation.ToArray())
        {
            var inventory = _inventory.Clone();
            inventory.AddItem(item);
            if (EvaluateBacktrack(vertex, state, inventory, _start))
                throw new InvalidOperationException(
                    $"Settled reverse backtracking proved '{vertex.Name}' "
                    + "after it had been left rejected by the forward/reverse "
                    + "fixed point.");
        }
    }

    private void ValidateReverseResult(
        bool expected, Vertex vertex, VisitedState startState,
        Inventory inventory, Vertex target, BacktrackRegion targetRegion)
    {
        if (_backtrackMetrics.Mode != BacktrackBenchmarkMode.Validate)
            return;

        bool actual = RunLegacyBacktrack(
            vertex, startState, inventory, target, targetRegion, out _);
        if (actual != expected)
        {
            throw new InvalidOperationException(
                $"Reverse backtracking returned {expected} for '{vertex.Name}' "
                + $"to '{target.Name}', but legacy search returned {actual}.");
        }
    }

    private bool RunLegacyBacktrack(
        Vertex vertex, VisitedState startState, Inventory inventory, Vertex target,
        BacktrackRegion? targetRegion, out int states)
    {
        long started = Stopwatch.GetTimestamp();
        var fallback = new StatefulSearcher(
            _graph, vertex, inventory.Clone(), null, target, startState,
            null, false, null, targetRegion);
        bool success = fallback.HasVisited(target);
        states = fallback._dequeuedStateCount;
        _backtrackMetrics.RecordFallback(
            success, Stopwatch.GetTimestamp() - started, states);
        return success;
    }

    private BacktrackRegion GetBacktrackRegion(Vertex target)
        => _backtrackCache.GetRegion(_graph, target, _backtrackMetrics);

    private ReverseBacktrackSearch GetReverseBacktrackSearch(
        Vertex target, BacktrackRegion region, Inventory inventory)
        => _backtrackCache.GetReverseSearch(
            target, region, inventory, _backtrackMetrics);

    public void ResumeSearch(IEnumerable<Randomizer.Graph.Vertex> startAt, Inventory prevInventory)
    {
        var newStarts = startAt.Cast<Vertex>().ToList();
        _persistentStarts.UnionWith(newStarts);
        var diffItems = _inventory.All().Except(prevInventory.All()).Where(x => x.Key.World == _start.World).Select(x => x.Key.Name).ToHashSet();


        foreach (int vertexId in _unvisitedStateIds)
        {
            var states = _unvisitedStates[vertexId];
            if (states != null && SetsOverlap(states.MissingItems, diffItems))
            {
                var vertex = _searchModel.VerticesById[vertexId]!;
                foreach (var state in states.States)
                {
                    _startStates.Add((vertex, state));
                }
            }
        }

        foreach (Vertex start in newStarts)
        {
            if (!_visitedVertices.Contains(start))
            {
                _startStates.Add((start, ForwardState.Empty));
            }
        }

        // Debt states do not change when inventory capacity increases.
        foreach (var (vertex, _) in _startStates)
            _visitedStates[vertex.Id].Clear();

        if (_startStates.Count > 0)
        {
            SearchForward([]);
        }
    }

    public IEnumerable<Randomizer.Graph.Vertex> GetOtherWorld() => _otherWorldLocations;

    public IReadOnlyDictionary<Randomizer.Graph.Vertex, StatefulPathStep?> GetPredecessors() =>
        _predecessors;

    internal StatefulPickup? GetPickupDetails(Randomizer.Graph.Vertex location, IItem item) =>
        _pickupDetails.GetValueOrDefault((location, item));

    private sealed class UnvisitedFrontier(
        HashSet<string> missingItems, List<ForwardState> states)
    {
        public HashSet<string> MissingItems { get; } = missingItems;
        public List<ForwardState> States { get; } = states;
    }

    private readonly record struct ForwardArrival(ForwardState State);

    /// <summary>Item locations that can actually be opened, plus fixed flags yielded
    /// by strategies. Randomized item contents are included even when collection was
    /// disabled for sphere generation.</summary>
    public IEnumerable<(Randomizer.Graph.Vertex Location, StatefulPickup Pickup)> GetPlaythroughPickups()
    {
        foreach (var location in _visitedItemLocations.Keys)
        {
            if (location.Item != null)
                yield return (location, _pickupDetails.GetValueOrDefault(
                    (location, location.Item),
                    new StatefulPickup(location.Item, null, EmptyRequirements,
                        EmptyResources)));
        }

        foreach (var ((location, item), _) in _prevItems)
        {
            if (!ReferenceEquals(location.Item, item))
                yield return (location, _pickupDetails.GetValueOrDefault(
                    (location, item),
                    new StatefulPickup(item, null, EmptyRequirements,
                        EmptyResources)));
        }
    }
}

internal struct ForwardStateFrontier
{
    private const int InlineCapacity = 4;
    private ForwardState _state0;
    private ForwardState _state1;
    private ForwardState _state2;
    private ForwardState _state3;
    private List<ForwardState>? _overflow;

    public int Count { get; private set; }
    public readonly bool IsEmpty => Count == 0;

    public bool AddNondominated(ForwardState candidate)
    {
        for (int index = 0; index < Count; index++)
        {
            if (Get(index).Dominates(candidate))
                return false;
        }

        int writeIndex = 0;
        int originalCount = Count;
        for (int index = 0; index < originalCount; index++)
        {
            var existing = Get(index);
            if (!candidate.Dominates(existing))
                Set(writeIndex++, existing);
        }
        Truncate(writeIndex);
        AddUnchecked(candidate);
        return true;
    }

    public void RemoveEqual(ForwardState state)
    {
        int writeIndex = 0;
        int originalCount = Count;
        for (int index = 0; index < originalCount; index++)
        {
            var candidate = Get(index);
            if (!candidate.Equals(state))
                Set(writeIndex++, candidate);
        }
        Truncate(writeIndex);
    }

    public void SetSingle(ForwardState state)
    {
        Clear();
        AddUnchecked(state);
    }

    public void Clear()
    {
        _state0 = default;
        _state1 = default;
        _state2 = default;
        _state3 = default;
        _overflow?.Clear();
        Count = 0;
    }

    private readonly ForwardState Get(int index) => index switch
    {
        0 => _state0,
        1 => _state1,
        2 => _state2,
        3 => _state3,
        _ => _overflow![index - InlineCapacity],
    };

    private void Set(int index, ForwardState state)
    {
        switch (index)
        {
            case 0:
                _state0 = state;
                break;
            case 1:
                _state1 = state;
                break;
            case 2:
                _state2 = state;
                break;
            case 3:
                _state3 = state;
                break;
            default:
                _overflow![index - InlineCapacity] = state;
                break;
        }
    }

    private void AddUnchecked(ForwardState state)
    {
        switch (Count)
        {
            case 0:
                _state0 = state;
                break;
            case 1:
                _state1 = state;
                break;
            case 2:
                _state2 = state;
                break;
            case 3:
                _state3 = state;
                break;
            default:
                (_overflow ??= new List<ForwardState>(InlineCapacity)).Add(state);
                break;
        }
        Count++;
    }

    private void Truncate(int count)
    {
        int retainedOverflow = Math.Max(0, count - InlineCapacity);
        if (_overflow != null && _overflow.Count > retainedOverflow)
            _overflow.RemoveRange(
                retainedOverflow, _overflow.Count - retainedOverflow);

        if (count < 4)
            _state3 = default;
        if (count < 3)
            _state2 = default;
        if (count < 2)
            _state1 = default;
        if (count < 1)
            _state0 = default;
        Count = count;
    }
}

internal sealed class SearchContext
{
    public SearchContext(Inventory inventory, World world)
    {
        EnergyCapacity = 99 + inventory.GetCount(world.GetItem("ETank")) * 100;
        MissileCapacity = inventory.GetCount(world.GetItem("Missile")) * 5;
        SuperMissileCapacity = inventory.GetCount(world.GetItem("Super")) * 5;
        PowerBombCapacity = inventory.GetCount(world.GetItem("PowerBomb")) * 5;
    }

    public int EnergyCapacity { get; }
    public int MissileCapacity { get; }
    public int SuperMissileCapacity { get; }
    public int PowerBombCapacity { get; }
}

internal readonly record struct ForwardState(
    int EnergyDebt,
    int MissileDebt,
    int SuperMissileDebt,
    int PowerBombDebt,
    int ObstacleBitFlags,
    int DoorUnlockedFlags)
{
    public static ForwardState Empty => new(0, 0, 0, 0, 0, 0);

    public static ForwardState FromVisited(
        VisitedState state, Inventory inventory, World world) => new(
        Math.Max(0, EnergyCapacity(inventory, world) - state.Energy),
        Math.Max(0, MissileCapacity(inventory, world) - state.Missiles),
        Math.Max(0, SuperCapacity(inventory, world) - state.SuperMissiles),
        Math.Max(0, PowerBombCapacity(inventory, world) - state.PowerBombs),
        state.ObstacleBitFlags,
        state.DoorUnlockedFlags);

    public VisitedState ToVisited(Inventory inventory, World world) => new()
    {
        Energy = Math.Max(0, EnergyCapacity(inventory, world) - EnergyDebt),
        Missiles = Math.Max(0, MissileCapacity(inventory, world) - MissileDebt),
        SuperMissiles = Math.Max(0,
            SuperCapacity(inventory, world) - SuperMissileDebt),
        PowerBombs = Math.Max(0,
            PowerBombCapacity(inventory, world) - PowerBombDebt),
        ObstacleBitFlags = ObstacleBitFlags,
        DoorUnlockedFlags = DoorUnlockedFlags,
    };

    public VisitedState ToVisited(SearchContext context) => new()
    {
        Energy = Math.Max(0, context.EnergyCapacity - EnergyDebt),
        Missiles = Math.Max(0, context.MissileCapacity - MissileDebt),
        SuperMissiles = Math.Max(0,
            context.SuperMissileCapacity - SuperMissileDebt),
        PowerBombs = Math.Max(0,
            context.PowerBombCapacity - PowerBombDebt),
        ObstacleBitFlags = ObstacleBitFlags,
        DoorUnlockedFlags = DoorUnlockedFlags,
    };

    public ForwardState? ApplyCost(RequirementCost cost, SearchContext context)
    {
        int energy = context.EnergyCapacity - EnergyDebt;
        int missiles = context.MissileCapacity - MissileDebt;
        int supers = context.SuperMissileCapacity - SuperMissileDebt;
        int powerBombs = context.PowerBombCapacity - PowerBombDebt;

        if (cost.Energy > 0x8000)
            cost.Energy = Math.Min(energy, cost.Energy & 0x7fff);
        if (cost.Missiles > 0x8000)
            cost.Missiles = Math.Min(missiles, cost.Missiles & 0x7fff);
        if (cost.SuperMissiles > 0x8000)
            cost.SuperMissiles = Math.Min(supers, cost.SuperMissiles & 0x7fff);
        if (cost.PowerBombs > 0x8000)
            cost.PowerBombs = Math.Min(powerBombs, cost.PowerBombs & 0x7fff);

        if (missiles - cost.Missiles < 0)
        {
            int difference = cost.Missiles - missiles;
            cost.Missiles = missiles;
            cost.SuperMissiles += (difference + 2) / 3;
        }

        if (energy - cost.Energy < 0
            || missiles - cost.Missiles < 0
            || supers - cost.SuperMissiles < 0
            || powerBombs - cost.PowerBombs < 0)
            return null;

        return this with
        {
            EnergyDebt = Math.Max(0, EnergyDebt + cost.Energy),
            MissileDebt = Math.Max(0, MissileDebt + cost.Missiles),
            SuperMissileDebt = Math.Max(
                0, SuperMissileDebt + cost.SuperMissiles),
            PowerBombDebt = Math.Max(0, PowerBombDebt + cost.PowerBombs),
        };
    }

    public bool Dominates(ForwardState other) =>
        EnergyDebt <= other.EnergyDebt
        && SuperMissileDebt <= other.SuperMissileDebt
        && MissileDebt + SuperMissileDebt * 3
            <= other.MissileDebt + other.SuperMissileDebt * 3
        && PowerBombDebt <= other.PowerBombDebt
        && (ObstacleBitFlags & other.ObstacleBitFlags)
            == other.ObstacleBitFlags;

    public ForwardState WithDoorUnlocked(int node) =>
        this with { DoorUnlockedFlags = DoorUnlockedFlags | (1 << node) };

    public ForwardState WithObstacles(int obstacleMask) =>
        this with { ObstacleBitFlags = ObstacleBitFlags | obstacleMask };

    public ForwardState WithoutObstacles(int obstacleMask) =>
        this with { ObstacleBitFlags = ObstacleBitFlags & ~obstacleMask };

    private static int EnergyCapacity(Inventory inventory, World world) =>
        99 + inventory.GetCount(world.GetItem("ETank")) * 100;
    private static int MissileCapacity(Inventory inventory, World world) =>
        inventory.GetCount(world.GetItem("Missile")) * 5;
    private static int SuperCapacity(Inventory inventory, World world) =>
        inventory.GetCount(world.GetItem("Super")) * 5;
    private static int PowerBombCapacity(Inventory inventory, World world) =>
        inventory.GetCount(world.GetItem("PowerBomb")) * 5;
}

public struct VisitedState
{
    public int Energy;
    public int Missiles;
    public int SuperMissiles;
    public int PowerBombs;
    public int ObstacleBitFlags;
    public int DoorUnlockedFlags;

    public bool Dominates(VisitedState other)
    {
        return Energy >= other.Energy
            && SuperMissiles >= other.SuperMissiles
            && (Missiles + SuperMissiles * 3) >= (other.Missiles + other.SuperMissiles * 3)
            && PowerBombs >= other.PowerBombs
            && (ObstacleBitFlags & other.ObstacleBitFlags) == other.ObstacleBitFlags;
    }

    public VisitedState? ApplyCost(RequirementCost cost, Inventory inventory, World world)
    {
        // Handle drain costs (> 0x8000)
        if (cost.Energy > 0x8000) { cost.Energy = Math.Min(Energy, cost.Energy & 0x7FFF); }
        ;
        if (cost.Missiles > 0x8000) { cost.Missiles = Math.Min(Missiles, cost.Missiles & 0x7FFF); }
        ;
        if (cost.SuperMissiles > 0x8000) { cost.SuperMissiles = Math.Min(SuperMissiles, cost.SuperMissiles & 0x7FFF); }
        ;
        if (cost.PowerBombs > 0x8000) { cost.PowerBombs = Math.Min(PowerBombs, cost.PowerBombs & 0x7FFF); }
        ;

        // Redistribute missile cost to super cost if we run out of missiles (at a rate of 3:1)
        if (Missiles - cost.Missiles < 0)
        {
            var diff = Math.Abs(Missiles - cost.Missiles);
            cost.Missiles = Missiles;
            cost.SuperMissiles += (diff + 2) / 3;
        }

        // Check if we have enough resources to apply the cost, and also handle refilling (negative numbers by checking against the max values in our inventory)
        if (Energy - cost.Energy < 0 || Missiles - cost.Missiles < 0 || SuperMissiles - cost.SuperMissiles < 0 || PowerBombs - cost.PowerBombs < 0)
        {
            return null;
        }

        return new VisitedState
        {
            Energy = cost.Energy < 0 ? Math.Min(Energy - cost.Energy, 99 + inventory.GetCount(world.GetItem("ETank")) * 100) : Energy - cost.Energy,
            Missiles = cost.Missiles < 0 ? Math.Min(Missiles - cost.Missiles, inventory.GetCount(world.GetItem("Missile")) * 5) : Missiles - cost.Missiles,
            SuperMissiles = cost.SuperMissiles < 0 ? Math.Min(SuperMissiles - cost.SuperMissiles, inventory.GetCount(world.GetItem("Super")) * 5) : SuperMissiles - cost.SuperMissiles,
            PowerBombs = cost.PowerBombs < 0 ? Math.Min(PowerBombs - cost.PowerBombs, inventory.GetCount(world.GetItem("PowerBomb")) * 5) : PowerBombs - cost.PowerBombs,
            ObstacleBitFlags = ObstacleBitFlags,
            DoorUnlockedFlags = DoorUnlockedFlags
        };
    }

    public VisitedState WithDoorUnlocked(int node)
        => this with { DoorUnlockedFlags = DoorUnlockedFlags | (1 << node) };

    public bool HasDoorUnlocked(int node)
        => (DoorUnlockedFlags & (1 << node)) != 0;

    /// <summary>
    /// Return a new state with the specified obstacles "cleared" (bits set).
    /// </summary>
    public VisitedState WithObstacles(string[] obstacles)
        => this with { ObstacleBitFlags = ObstacleBitFlags | RequirementHandler.ObstacleMaskFromArray(obstacles) };

    /// <summary>
    /// Return a new state with the specified obstacles "uncleared" (bits unset).
    /// </summary>
    public VisitedState WithoutObstacles(string[] obstacles)
        => this with { ObstacleBitFlags = ObstacleBitFlags & ~RequirementHandler.ObstacleMaskFromArray(obstacles) };


    public override string ToString()
    {
        return $"Energy: {Energy}, Missiles: {Missiles}, SuperMissiles: {SuperMissiles}, PowerBombs: {PowerBombs}, ObstacleBitFlags: {ObstacleBitFlags}";
    }
}
