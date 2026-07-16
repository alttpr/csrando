namespace Randomizer.Games.SuperMetroid;

using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;

/// <summary>
/// Runs the normal SM transition logic over the incoming-edge graph for one
/// concrete inventory. Resource expenditure is accumulated as debt instead of
/// being subtracted from a forward state. The resulting bounded frontier can be
/// compared directly with the states recorded when item and flag vertices were
/// reached by the forward search.
/// </summary>
internal sealed class ReverseBacktrackSearch
{
    private readonly List<ReverseState>?[] _states;
    private readonly List<int> _frontierVertexIds = [];
    private readonly SmSearchModel _model;
    private readonly World _world;
    private readonly Inventory _inventory;
    private readonly ulong _inventoryFlagMask;
    private readonly Dictionary<ulong, Inventory> _inventoriesByRequiredFlags = [];
    private readonly RequirementHandler _handler;
    private readonly HashSet<Weapon> _weapons;
    // Pre-sized to the typical settled entry count so build-time growth does
    // not repeatedly reallocate the backing arrays.
    private readonly Dictionary<RequirementEvaluationKey, RequirementResult>
        _requirementResults = new(4096);
    private readonly List<ReverseState> _edgeCandidates = [];
    private readonly List<ReverseState> _edgeFrontier = [];
    private readonly List<ReverseState> _unlockResults = [];
    private readonly List<ReverseState> _lockedEdgeCandidates = [];
    private readonly List<ReverseState> _lockedEdgeFrontier = [];
    private readonly List<ReverseState> _unlockCurrent = [];
    private readonly List<ReverseState> _unlockCandidates = [];
    private readonly List<ReverseState> _unlockPruned = [];
    private ReverseBacktrackSearch(
        World world, Inventory inventory, SmSearchModel model)
    {
        _world = world;
        _model = model;
        _states = new List<ReverseState>?[model.Capacity];
        _inventory = inventory.Clone();
        _inventoryFlagMask = model.InventoryFlagMask(inventory);
        _inventoriesByRequiredFlags[0] = _inventory;
        _handler = world.RequirementHandler;
        var capacity = new VisitedState
        {
            Energy = 99 + inventory.GetCount(world.GetItem("ETank")) * 100,
            Missiles = inventory.GetCount(world.GetItem("Missile")) * 5,
            SuperMissiles = inventory.GetCount(world.GetItem("Super")) * 5,
            PowerBombs = inventory.GetCount(world.GetItem("PowerBomb")) * 5,
        };
        _weapons = world.JsonData.Weapons.Weapons
            .Where(weapon => _handler.HandleRequirement(
                weapon.UseRequires, capacity, inventory, world, [],
                captureDetails: false).Met)
            .ToHashSet();
    }

    public static ReverseBacktrackSearch Build(
        BacktrackRegion region, Vertex target, Inventory inventory)
    {
        var search = new ReverseBacktrackSearch(
            (World)target.World, inventory, region.Model);
        var queue = new Queue<(int VertexId, ReverseState State)>();
        var predecessors = new List<ReverseState>();

        AddTerminal(target.Id);
        foreach (int exitId in region.Model.CrossWorldTerminalIds)
            AddTerminal(exitId);

        while (queue.TryDequeue(out var pending))
        {
            var (toId, downstream) = pending;
            var current = search._states[toId];
            if (current == null || !current.Contains(downstream))
                continue;
            var to = region.Model.VerticesById[toId]!;

            foreach (var edge in region.Model.IncomingEdgesById[toId])
            {
                var from = (Vertex)edge.From;
                if (!region.Contains(from))
                    continue;

                predecessors.Clear();
                search.Traverse(from, to, edge, downstream, predecessors);
                foreach (var predecessor in predecessors)
                {
                    if (search.Add(from, predecessor))
                    {
                        queue.Enqueue((from.Id, predecessor));
                    }
                }
            }
        }

        return search;

        void AddTerminal(int vertexId)
        {
            var vertex = region.Model.VerticesById[vertexId]!;
            if (region.Contains(vertex)
                && search.Add(vertex, ReverseState.Empty))
                queue.Enqueue((vertexId, ReverseState.Empty));
        }
    }

    public bool CanReturn(Vertex vertex, VisitedState arrivalState) =>
        (uint)vertex.Id < (uint)_states.Length
        && _states[vertex.Id] is { } states
        && states.Any(state => state.IsAffordableFrom(arrivalState));

    public int FrontierVertexCount => _frontierVertexIds.Count;
    public int FrontierEntryCount => _frontierVertexIds.Sum(
        vertexId => _states[vertexId]!.Count);

    private void Traverse(
        Vertex from, Vertex to, Randomizer.Graph.Edge edge,
        ReverseState downstream, List<ReverseState> output)
    {
        var state = downstream;

        // Forward traversal resets room-local state after crossing a room edge.
        // Consequently, a destination-room route that requires an obstacle or
        // door to already be active cannot be entered from another room at all.
        // If the reset state is acceptable, start the predecessor room fresh.
        if (from.RoomId != to.RoomId)
        {
            if (state.RequiredObstacleBitFlags != 0
                || state.DoorUnlockedFlags != 0
                || state.RequiredFlagMask != 0)
                return;
            state = state with
            {
                RequiredObstacleBitFlags = 0,
                ForbiddenObstacleBitFlags = 0,
                DoorUnlockedFlags = 0,
            };
        }

        int unlockedDoor = from.Node?.NodeType == "door"
            ? 1 << from.NodeId
            : 0;
        state = state with
        {
            DoorUnlockedFlags = state.DoorUnlockedFlags | unlockedDoor,
        };

        _edgeCandidates.Clear();
        if (edge is not Edge smEdge)
        {
            _edgeCandidates.Add(state);
        }
        else
        {
            foreach (var strategy in smEdge.Strats ?? [])
                ApplyStrategy(state, strategy, _edgeCandidates);
        }

        Prune(_edgeCandidates, _edgeFrontier);
        foreach (var edgeState in _edgeFrontier)
        {
            _unlockResults.Clear();
            ApplyUnlocks(from, edgeState, _unlockResults);
            foreach (var unlocked in _unlockResults)
                output.Add(FinishVertex(unlocked, unlockedDoor));
        }

        if (from.RoomId == to.RoomId
            && from.Node?.Locks is { Length: > 0 })
        {
            _lockedEdgeCandidates.Clear();
            if (edge is Edge lockedSmEdge)
            {
                foreach (var strategy in lockedSmEdge.Strats ?? [])
                {
                    ApplyStrategy(state with
                        {
                            DoorUnlockedFlags = state.DoorUnlockedFlags
                                & ~unlockedDoor,
                        }, strategy, _lockedEdgeCandidates);
                }
            }
            else
            {
                _lockedEdgeCandidates.Add(state with
                {
                    DoorUnlockedFlags = state.DoorUnlockedFlags & ~unlockedDoor,
                });
            }
            Prune(_lockedEdgeCandidates, _lockedEdgeFrontier);
            foreach (var lockedState in _lockedEdgeFrontier)
            {
                // Forward traversal may move along an in-room edge while the
                // current door remains locked (for example, enter a gray-door
                // room, kill its enemies, then open the door on the way out).
                // Keep that predecessor distinct from the automatically
                // unlocked alternatives above.
                output.Add(FinishVertex(lockedState, unlockedDoor));
            }
        }
    }

    private ReverseState FinishVertex(
        ReverseState state, int unlockedDoor)
    {
        state = state with
        {
            DoorUnlockedFlags = state.DoorUnlockedFlags & ~unlockedDoor,
        };
        return state;
    }

    private void ApplyUnlocks(
        Vertex vertex, ReverseState downstream, List<ReverseState> output)
    {
        _unlockCurrent.Clear();
        _unlockCurrent.Add(downstream);
        List<ReverseState> states = _unlockCurrent;
        List<ReverseState> pruned = _unlockPruned;
        var locks = vertex.Node?.Locks ?? [];
        for (int lockIndex = locks.Length - 1; lockIndex >= 0; lockIndex--)
        {
            var nodeLock = locks[lockIndex];
            _unlockCandidates.Clear();
            foreach (var state in states)
            {
                if (nodeLock.Lock != null
                    && !EvaluateRequirement(state, nodeLock.Lock,
                        _model.GetRequirementPlan(nodeLock.Lock, _handler)).Met)
                {
                    _unlockCandidates.Add(state);
                    continue;
                }

                foreach (var strategy in nodeLock.UnlockStrats ?? [])
                    ApplyStrategy(state, strategy, _unlockCandidates,
                        _model.FlagsProducedInRoom(vertex.RoomId));
            }
            Prune(_unlockCandidates, pruned);
            var oldStates = states;
            states = pruned;
            pruned = oldStates;
            if (states.Count == 0)
                return;

        }

        foreach (var state in states)
            output.Add(state);
    }

    private void ApplyStrategy(
        ReverseState downstream, Strat strategy,
        List<ReverseState> output, ulong allowedNewFlags = 0)
    {
        var plan = _model.GetStrategyPlan(strategy, _handler);
        var predecessor = (downstream with
        {
            RequiredFlagMask = downstream.RequiredFlagMask
                & ~plan.SetsFlagMask,
        }).InvertObstacles(
            plan.ClearsObstacleMask, plan.ResetsObstacleMask);
        if (predecessor is null)
            return;

        AddRequirementMatches(
            predecessor.Value, strategy.Requires, plan.Requirement,
            allowedNewFlags, output);
    }

    private void AddRequirementMatches(
        ReverseState state, Requirement requirement,
        in CompiledRequirementPlan plan,
        ulong allowedNewFlags, List<ReverseState> output)
    {
        int obstacleMask = plan.ObstacleMask;
        int doorMask = plan.DoorMask;
        ulong flagMask = plan.FlagMask & ~_inventoryFlagMask & allowedNewFlags;
        int obstacleValues = 0;
        while (true)
        {
            if ((obstacleValues & state.ForbiddenObstacleBitFlags) == 0
                && (state.RequiredObstacleBitFlags & obstacleMask
                    & ~obstacleValues) == 0)
            {
                int doorValues = 0;
                while (true)
                {
                    if ((state.DoorUnlockedFlags & doorMask & ~doorValues) == 0)
                    {
                        ulong flagValues = 0;
                        while (true)
                        {
                            if ((state.RequiredFlagMask & flagMask & ~flagValues) == 0)
                            {
                                var candidate = state with
                                {
                                    RequiredObstacleBitFlags = state.RequiredObstacleBitFlags
                                        | obstacleValues,
                                    ForbiddenObstacleBitFlags = state.ForbiddenObstacleBitFlags
                                        | (obstacleMask & ~obstacleValues),
                                    DoorUnlockedFlags = state.DoorUnlockedFlags | doorValues,
                                    RequiredFlagMask = state.RequiredFlagMask | flagValues,
                                };
                                var result = EvaluateRequirement(
                                    candidate, requirement, plan);
                                if (result.Met && result.Cost is { } cost
                                    && candidate.ApplyCost(cost) is { } withCost)
                                    output.Add(withCost);
                            }
                            if (flagValues == flagMask)
                                break;
                            flagValues = (flagValues - flagMask) & flagMask;
                        }
                    }
                    if (doorValues == doorMask)
                        break;
                    doorValues = (doorValues - doorMask) & doorMask;
                }
            }
            if (obstacleValues == obstacleMask)
                break;
            obstacleValues = (obstacleValues - obstacleMask) & obstacleMask;
        }
    }

    private RequirementResult EvaluateRequirement(
        ReverseState state, Requirement requirement,
        in CompiledRequirementPlan plan)
    {
        // The tree reads graph state and event flags only through the bits
        // named by its compiled masks, resources are constant in reverse
        // evaluation, and the weapon set and inventory are fixed per search,
        // so states agreeing on the masked bits share one result.
        var key = new RequirementEvaluationKey(
            plan.Id, state.RequiredObstacleBitFlags & plan.ObstacleMask,
            state.DoorUnlockedFlags & plan.DoorMask,
            state.RequiredFlagMask & plan.FlagMask);
        if (_requirementResults.TryGetValue(key, out var cached))
            return cached;

        var result = _handler.HandleRequirement(
            requirement, AvailableState(state),
            InventoryWithRequiredFlags(state.RequiredFlagMask),
            _world, _weapons, captureDetails: false);
        _requirementResults[key] = result;
        return result;
    }

    private Inventory InventoryWithRequiredFlags(ulong requiredFlags)
    {
        requiredFlags &= ~_inventoryFlagMask;
        if (_inventoriesByRequiredFlags.TryGetValue(
                requiredFlags, out var inventory))
            return inventory;

        inventory = _inventory.Clone();
        for (int index = 0; index < _model.FlagItems.Length; index++)
        {
            if ((requiredFlags & (1UL << index)) != 0)
                inventory.AddItem(_model.FlagItems[index]);
        }
        _inventoriesByRequiredFlags[requiredFlags] = inventory;
        return inventory;
    }

    private VisitedState AvailableState(ReverseState state) => new()
    {
        // Resource affordability is deliberately deferred until the completed
        // reverse debt is compared with an actual forward-arrival state.
        Energy = 1_000_000,
        Missiles = 1_000_000,
        SuperMissiles = 1_000_000,
        PowerBombs = 1_000_000,
        ObstacleBitFlags = state.RequiredObstacleBitFlags,
        DoorUnlockedFlags = state.DoorUnlockedFlags,
    };

    private static void Prune(
        IReadOnlyList<ReverseState> candidates, List<ReverseState> frontier)
    {
        frontier.Clear();
        if (frontier.Capacity < candidates.Count)
            frontier.Capacity = candidates.Count;
        for (int index = 0; index < candidates.Count; index++)
            AddToFrontier(frontier, candidates[index]);
    }

    private bool Add(Vertex vertex, ReverseState state)
    {
        var frontier = _states[vertex.Id];
        if (frontier == null)
        {
            _states[vertex.Id] = [state];
            _frontierVertexIds.Add(vertex.Id);
            return true;
        }
        return AddToFrontier(frontier, state);
    }

    private static bool AddToFrontier(
        List<ReverseState> frontier, ReverseState candidate)
    {
        for (int index = 0; index < frontier.Count; index++)
        {
            if (frontier[index].Dominates(candidate))
                return false;
        }

        for (int index = frontier.Count - 1; index >= 0; index--)
        {
            if (candidate.Dominates(frontier[index]))
                frontier.RemoveAt(index);
        }
        frontier.Add(candidate);

        // MapRandomizer keeps at most one state for each of four cost metrics.
        // Do the same for each concrete obstacle/door constraint group so the
        // resource frontier cannot grow without bound while preserving the
        // graph-state alternatives that our graph does not pre-expand.
        int sameConstraintCount = 0;
        ReverseState best0 = default;
        ReverseState best1 = default;
        ReverseState best2 = default;
        ReverseState best3 = default;
        long bestCost0 = long.MaxValue;
        long bestCost1 = long.MaxValue;
        long bestCost2 = long.MaxValue;
        long bestCost3 = long.MaxValue;
        for (int index = 0; index < frontier.Count; index++)
        {
            var state = frontier[index];
            if (!state.HasSameGraphConstraints(candidate))
                continue;

            sameConstraintCount++;
            long cost0 = state.Cost(0);
            long cost1 = state.Cost(1);
            long cost2 = state.Cost(2);
            long cost3 = state.Cost(3);
            if (cost0 < bestCost0)
            {
                bestCost0 = cost0;
                best0 = state;
            }
            if (cost1 < bestCost1)
            {
                bestCost1 = cost1;
                best1 = state;
            }
            if (cost2 < bestCost2)
            {
                bestCost2 = cost2;
                best2 = state;
            }
            if (cost3 < bestCost3)
            {
                bestCost3 = cost3;
                best3 = state;
            }
        }

        if (sameConstraintCount <= 4)
            return true;

        bool retainedCandidate = candidate.Equals(best0)
            || candidate.Equals(best1)
            || candidate.Equals(best2)
            || candidate.Equals(best3);
        for (int index = frontier.Count - 1; index >= 0; index--)
        {
            var state = frontier[index];
            if (state.HasSameGraphConstraints(candidate)
                && !state.Equals(best0)
                && !state.Equals(best1)
                && !state.Equals(best2)
                && !state.Equals(best3))
                frontier.RemoveAt(index);
        }
        return retainedCandidate;
    }

    private readonly record struct ReverseState(
        int Energy,
        int Missiles,
        int SuperMissiles,
        int PowerBombs,
        int RequiredObstacleBitFlags,
        int ForbiddenObstacleBitFlags,
        int DoorUnlockedFlags,
        ulong RequiredFlagMask)
    {
        public static ReverseState Empty => new(0, 0, 0, 0, 0, 0, 0, 0);

        public ReverseState? ApplyCost(RequirementCost cost)
        {
            int energy = ReverseResource(Energy, cost.Energy);
            int missiles = ReverseResource(Missiles, cost.Missiles);
            int supers = ReverseResource(SuperMissiles, cost.SuperMissiles);
            int powerBombs = ReverseResource(PowerBombs, cost.PowerBombs);
            return this with
            {
                Energy = energy,
                Missiles = missiles,
                SuperMissiles = supers,
                PowerBombs = powerBombs,
            };
        }

        public ReverseState? InvertObstacles(
            int clears, int resets)
        {
            if ((RequiredObstacleBitFlags & resets) != 0
                || (ForbiddenObstacleBitFlags & clears) != 0)
                return null;
            return this with
            {
                RequiredObstacleBitFlags = RequiredObstacleBitFlags & ~clears,
                ForbiddenObstacleBitFlags = ForbiddenObstacleBitFlags & ~resets,
            };
        }

        public bool IsAffordableFrom(VisitedState state) =>
            state.Energy >= Energy
            && state.Missiles >= Missiles
            && state.SuperMissiles >= SuperMissiles
            && state.PowerBombs >= PowerBombs
            && (state.ObstacleBitFlags & RequiredObstacleBitFlags)
                == RequiredObstacleBitFlags
            && (state.ObstacleBitFlags & ForbiddenObstacleBitFlags) == 0
            && (state.DoorUnlockedFlags & DoorUnlockedFlags) == DoorUnlockedFlags
            && RequiredFlagMask == 0;

        public bool Dominates(ReverseState other) =>
            Energy <= other.Energy
            && Missiles <= other.Missiles
            && SuperMissiles <= other.SuperMissiles
            && PowerBombs <= other.PowerBombs
            && (RequiredObstacleBitFlags & other.RequiredObstacleBitFlags)
                == RequiredObstacleBitFlags
            && (ForbiddenObstacleBitFlags & other.ForbiddenObstacleBitFlags)
                == ForbiddenObstacleBitFlags
            && (DoorUnlockedFlags & other.DoorUnlockedFlags) == DoorUnlockedFlags
            && (RequiredFlagMask & other.RequiredFlagMask) == RequiredFlagMask;

        public bool HasSameGraphConstraints(ReverseState other) =>
            RequiredObstacleBitFlags == other.RequiredObstacleBitFlags
            && ForbiddenObstacleBitFlags == other.ForbiddenObstacleBitFlags
            && DoorUnlockedFlags == other.DoorUnlockedFlags
            && RequiredFlagMask == other.RequiredFlagMask;

        public long Cost(int metric)
        {
            long weightedAmmo = Missiles + SuperMissiles * 3L + PowerBombs * 4L;
            return metric switch
            {
                0 => Energy * 100_000L + weightedAmmo,
                1 => weightedAmmo * 100_000L + Energy,
                2 => SuperMissiles * 100_000L
                    + Energy + Missiles + PowerBombs,
                _ => PowerBombs * 100_000L
                    + Energy + Missiles + SuperMissiles,
            };
        }

        private static int ReverseResource(int debt, int cost)
        {
            if (cost > 0x8000)
            {
                int drain = cost & 0x7fff;
                return debt == 0 ? 0 : checked(debt + drain);
            }
            return Math.Max(0, checked(debt + cost));
        }
    }

    private readonly record struct RequirementEvaluationKey(
        int RequirementPlanId,
        int ObstacleBitFlags,
        int DoorUnlockedFlags,
        ulong RequiredFlagMask);

}
