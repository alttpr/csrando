namespace Randomizer.Games.SuperMetroid;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
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
    private static readonly ConditionalWeakTable<World, RequirementMaskCache>
        RequirementMasksByWorld = new();
    private readonly Dictionary<Vertex, List<ReverseState>> _states = [];
    private readonly World _world;
    private readonly Inventory _inventory;
    private readonly RequirementHandler _handler;
    private readonly HashSet<Weapon> _weapons;
    private readonly RequirementMaskCache _requirementMasks;
    private readonly Dictionary<RequirementEvaluationKey, RequirementResult>
        _requirementResults = [];
    private ReverseBacktrackSearch(World world, Inventory inventory)
    {
        _world = world;
        _inventory = inventory.Clone();
        _handler = world.RequirementHandler;
        _requirementMasks = RequirementMasksByWorld.GetValue(
            world, _ => new RequirementMaskCache());
        var capacity = new VisitedState
        {
            Energy = 99 + inventory.GetCount(world.GetItem("ETank")) * 100,
            Missiles = inventory.GetCount(world.GetItem("Missile")) * 5,
            SuperMissiles = inventory.GetCount(world.GetItem("Super")) * 5,
            PowerBombs = inventory.GetCount(world.GetItem("PowerBomb")) * 5,
        };
        _weapons = world.JsonData.Weapons.Weapons
            .Where(weapon => _handler.HandleRequirement(
                weapon.UseRequires, capacity, inventory, world, []).Met)
            .ToHashSet();
    }

    public static ReverseBacktrackSearch Build(
        BacktrackRegion region, Vertex target, Inventory inventory)
    {
        var search = new ReverseBacktrackSearch(
            (World)target.World, inventory);
        var queue = new Queue<(Vertex Vertex, ReverseState State)>();

        AddTerminal(target);
        foreach (var exit in region.Vertices.Where(vertex =>
                     vertex.Edges.Any(edge => edge.To.World != vertex.World)))
            AddTerminal(exit);

        while (queue.TryDequeue(out var pending))
        {
            var (to, downstream) = pending;
            if (!search._states.TryGetValue(to, out var current)
                || !current.Contains(downstream))
                continue;

            foreach (var edge in region.IncomingEdges[to])
            {
                var from = (Vertex)edge.From;
                if (!region.Vertices.Contains(from))
                    continue;

                foreach (var predecessor in search.Traverse(
                             from, to, edge, downstream))
                {
                    if (search.Add(from, predecessor))
                    {
                        queue.Enqueue((from, predecessor));
                    }
                }
            }
        }

        return search;

        void AddTerminal(Vertex vertex)
        {
            if (search.Add(vertex, ReverseState.Empty))
                queue.Enqueue((vertex, ReverseState.Empty));
        }
    }

    public bool CanReturn(Vertex vertex, VisitedState arrivalState) =>
        _states.TryGetValue(vertex, out var states)
        && states.Any(state => state.IsAffordableFrom(arrivalState));

    public bool UsesInventory(Inventory inventory)
    {
        int ownCount = _inventory.All().Count(pair =>
            ReferenceEquals(pair.Key.World, _world));
        int suppliedCount = inventory.All().Count(pair =>
            ReferenceEquals(pair.Key.World, _world));
        return ownCount == suppliedCount
            && _inventory.All().All(pair =>
                !ReferenceEquals(pair.Key.World, _world)
                || inventory.GetCount(pair.Key) == pair.Value);
    }

    public int FrontierVertexCount => _states.Count;
    public int FrontierEntryCount => _states.Values.Sum(states => states.Count);

    private IEnumerable<ReverseState> Traverse(
        Vertex from, Vertex to, Randomizer.Graph.Edge edge,
        ReverseState downstream)
    {
        var state = downstream;

        // Forward traversal resets room-local state after crossing a room edge.
        // Consequently, a destination-room route that requires an obstacle or
        // door to already be active cannot be entered from another room at all.
        // If the reset state is acceptable, start the predecessor room fresh.
        if (from.RoomId != to.RoomId)
        {
            if (state.RequiredObstacleBitFlags != 0
                || state.DoorUnlockedFlags != 0)
                yield break;
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

        var edgeStates = new List<ReverseState>();
        if (edge is not Edge smEdge)
        {
            edgeStates.Add(state);
        }
        else
        {
            foreach (var strategy in smEdge.Strats ?? [])
            {
                foreach (var result in ApplyStrategy(state, strategy))
                    edgeStates.Add(result);
            }
        }

        foreach (var edgeState in Prune(edgeStates))
        {
            foreach (var unlocked in ApplyUnlocks(from, edgeState))
            {
                yield return FinishVertex(unlocked, unlockedDoor);
            }
        }

        if (from.RoomId == to.RoomId
            && from.Node?.Locks is { Length: > 0 })
        {
            var lockedEdgeStates = new List<ReverseState>();
            if (edge is Edge lockedSmEdge)
            {
                foreach (var strategy in lockedSmEdge.Strats ?? [])
                {
                    foreach (var result in ApplyStrategy(
                        state with
                        {
                            DoorUnlockedFlags = state.DoorUnlockedFlags
                                & ~unlockedDoor,
                        }, strategy))
                        lockedEdgeStates.Add(result);
                }
            }
            else
            {
                lockedEdgeStates.Add(state with
                {
                    DoorUnlockedFlags = state.DoorUnlockedFlags & ~unlockedDoor,
                });
            }
            foreach (var lockedState in Prune(lockedEdgeStates))
            {
                // Forward traversal may move along an in-room edge while the
                // current door remains locked (for example, enter a gray-door
                // room, kill its enemies, then open the door on the way out).
                // Keep that predecessor distinct from the automatically
                // unlocked alternatives above.
                yield return FinishVertex(lockedState, unlockedDoor);
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

    private IEnumerable<ReverseState> ApplyUnlocks(
        Vertex vertex, ReverseState downstream)
    {
        IReadOnlyList<ReverseState> states = [downstream];
        foreach (var nodeLock in (vertex.Node?.Locks ?? []).Reverse())
        {
            var next = new List<ReverseState>();
            foreach (var state in states)
            {
                if (nodeLock.Lock != null
                    && !EvaluateRequirement(state, nodeLock.Lock).Met)
                {
                    next.Add(state);
                    continue;
                }

                foreach (var strategy in nodeLock.UnlockStrats ?? [])
                    next.AddRange(ApplyStrategy(state, strategy));
            }
            states = Prune(next);
            if (states.Count == 0)
                yield break;

        }

        foreach (var state in states)
            yield return state;
    }

    private IEnumerable<ReverseState> ApplyStrategy(
        ReverseState downstream, Strat strategy)
    {
        var predecessor = downstream.InvertObstacles(
            strategy.ClearsObstacles ?? [], strategy.ResetsObstacles ?? []);
        if (predecessor is null)
            yield break;

        foreach (var (state, result) in RequirementMatches(
                     predecessor.Value, strategy.Requires))
        {
            if (result.Cost is { } cost
                && state.ApplyCost(cost) is { } withCost)
                yield return withCost;
        }
    }

    private IEnumerable<(ReverseState State, RequirementResult Result)>
        RequirementMatches(ReverseState state, Requirement requirement)
    {
        var masks = _requirementMasks.Get(requirement, this);
        int obstacleMask = masks.Obstacles;
        int doorMask = masks.Doors;
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
                        var candidate = state with
                        {
                            RequiredObstacleBitFlags = state.RequiredObstacleBitFlags
                                | obstacleValues,
                            ForbiddenObstacleBitFlags = state.ForbiddenObstacleBitFlags
                                | (obstacleMask & ~obstacleValues),
                            DoorUnlockedFlags = state.DoorUnlockedFlags | doorValues,
                        };
                        var result = EvaluateRequirement(candidate, requirement);
                        if (result.Met)
                            yield return (candidate, result);
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
        ReverseState state, Requirement requirement)
    {
        var key = new RequirementEvaluationKey(
            requirement, state.RequiredObstacleBitFlags,
            state.DoorUnlockedFlags);
        if (_requirementResults.TryGetValue(key, out var cached))
            return cached;

        var result = _handler.HandleRequirement(
            requirement, AvailableState(state), _inventory,
            _world, _weapons);
        _requirementResults[key] = result;
        return result;
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

    private int ReferencedObstacles(Requirement requirement) => requirement switch
    {
        Requirement.ObstaclesCleared cleared =>
            RequirementHandler.ObstacleMaskFromArray(cleared.Obstacles),
        Requirement.ObstaclesNotCleared notCleared =>
            RequirementHandler.ObstacleMaskFromArray(notCleared.Obstacles),
        Requirement.And and => and.Reqs.Aggregate(
            0, (mask, child) => mask | ReferencedObstacles(child)),
        Requirement.Or or => or.Reqs.Aggregate(
            0, (mask, child) => mask | ReferencedObstacles(child)),
        Requirement.Not not => ReferencedObstacles(not.Req),
        Requirement.Single single
            when _handler.TryGetOptimizedRequirement(single.Req, out var helper) =>
            ReferencedObstacles(helper),
        Requirement.SingleItem item
            when _handler.TryGetOptimizedRequirement(item.Item.Name, out var helper) =>
            ReferencedObstacles(helper),
        Requirement.Tech tech
            when _handler.TryGetOptimizedRequirement(
                $"t_{tech.TechRequirement}", out var helper) =>
            ReferencedObstacles(helper),
        _ => 0,
    };

    private int ReferencedDoors(Requirement requirement) => requirement switch
    {
        Requirement.DoorUnlockedAtNode door => 1 << door.Node,
        Requirement.And and => and.Reqs.Aggregate(
            0, (mask, child) => mask | ReferencedDoors(child)),
        Requirement.Or or => or.Reqs.Aggregate(
            0, (mask, child) => mask | ReferencedDoors(child)),
        Requirement.Not not => ReferencedDoors(not.Req),
        Requirement.Single single
            when _handler.TryGetOptimizedRequirement(single.Req, out var helper) =>
            ReferencedDoors(helper),
        Requirement.SingleItem item
            when _handler.TryGetOptimizedRequirement(item.Item.Name, out var helper) =>
            ReferencedDoors(helper),
        Requirement.Tech tech
            when _handler.TryGetOptimizedRequirement(
                $"t_{tech.TechRequirement}", out var helper) =>
            ReferencedDoors(helper),
        _ => 0,
    };

    private static IReadOnlyList<ReverseState> Prune(
        IReadOnlyList<ReverseState> candidates)
    {
        var frontier = new List<ReverseState>(candidates.Count);
        for (int index = 0; index < candidates.Count; index++)
            AddToFrontier(frontier, candidates[index]);
        return frontier;
    }

    private bool Add(Vertex vertex, ReverseState state)
    {
        if (!_states.TryGetValue(vertex, out var frontier))
        {
            _states[vertex] = [state];
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
        int DoorUnlockedFlags)
    {
        public static ReverseState Empty => new(0, 0, 0, 0, 0, 0, 0);

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
            string[] clearsObstacles, string[] resetsObstacles)
        {
            int clears = RequirementHandler.ObstacleMaskFromArray(clearsObstacles);
            int resets = RequirementHandler.ObstacleMaskFromArray(resetsObstacles);
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
            && (state.DoorUnlockedFlags & DoorUnlockedFlags) == DoorUnlockedFlags;

        public bool Dominates(ReverseState other) =>
            Energy <= other.Energy
            && Missiles <= other.Missiles
            && SuperMissiles <= other.SuperMissiles
            && PowerBombs <= other.PowerBombs
            && (RequiredObstacleBitFlags & other.RequiredObstacleBitFlags)
                == RequiredObstacleBitFlags
            && (ForbiddenObstacleBitFlags & other.ForbiddenObstacleBitFlags)
                == ForbiddenObstacleBitFlags
            && (DoorUnlockedFlags & other.DoorUnlockedFlags) == DoorUnlockedFlags;

        public bool HasSameGraphConstraints(ReverseState other) =>
            RequiredObstacleBitFlags == other.RequiredObstacleBitFlags
            && ForbiddenObstacleBitFlags == other.ForbiddenObstacleBitFlags
            && DoorUnlockedFlags == other.DoorUnlockedFlags;

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
        Requirement Requirement,
        int ObstacleBitFlags,
        int DoorUnlockedFlags);

    private readonly record struct RequirementMasks(int Obstacles, int Doors);

    private sealed class RequirementMaskCache
    {
        private readonly ConcurrentDictionary<Requirement, RequirementMasks> _masks =
            new(ReferenceEqualityComparer.Instance);

        public RequirementMasks Get(
            Requirement requirement, ReverseBacktrackSearch search) =>
            _masks.GetOrAdd(requirement, static (candidate, state) =>
                new RequirementMasks(
                    state.ReferencedObstacles(candidate),
                    state.ReferencedDoors(candidate)), search);
    }

}
