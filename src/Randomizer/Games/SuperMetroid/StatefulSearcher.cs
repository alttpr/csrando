namespace Randomizer.Games.SuperMetroid;

using System;
using System.Collections.Generic;
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
    private readonly Graph _graph;
    private Dictionary<Vertex, List<VisitedState>> _visitedStates = null!;
    private Dictionary<Vertex, (HashSet<string>, List<VisitedState>)> _unvisitedStates = null!;
    private readonly Queue<(Vertex vertex, VisitedState state, StatefulPathStep? step)> _queue = new();
    private readonly Dictionary<Vertex, List<VisitedState>> _inQueue = [];
    private readonly Dictionary<Vertex, (VisitedState, Inventory)> _visitedItemLocations;
    private readonly HashSet<Vertex> _visitedVertices;
    private readonly HashSet<IItem> _foundItems = [];
    private readonly Inventory _inventory;
    private readonly Dictionary<(Vertex, IItem), VisitedState> _prevItems;
    private readonly Vertex? _target;
    private readonly Vertex _start;
    private readonly List<(Vertex, VisitedState)> _startStates;
    private readonly SetLocations? _setLocations;
    private readonly List<Randomizer.Graph.Vertex> _otherWorldLocations = [];
    private readonly HashSet<Weapon> _currentWeapons = [];
    private readonly RequirementHandler _requirementHandler;
    private readonly Func<Randomizer.Graph.Vertex, bool> _collectItemAt;
    private readonly Dictionary<Randomizer.Graph.Vertex, StatefulPathStep?> _predecessors = [];
    private readonly HashSet<Randomizer.Graph.Vertex> _recordedUnlocks = [];
    private readonly Dictionary<(Randomizer.Graph.Vertex, IItem), StatefulPickup> _pickupDetails = [];
    private readonly bool _capturePath;
    // Path-only searches can suppress the event they are trying to explain so a
    // later, post-event route cannot be mistaken for the acquisition route.
    private readonly IItem? _excludedPickup;
    private readonly CancellationToken _cancellationToken;

    // Implement the same interface as the generic Searcher, but with a stateful implementation that can track
    // energy, ammo, and other stateful information during traversal of the graph.
    public StatefulSearcher(Graph graph, Vertex start, Inventory inventory, SetLocations? setLocations = null,
        Vertex? target = null, VisitedState? visitedState = null,
        Func<Randomizer.Graph.Vertex, bool>? collectItemAt = null,
        bool capturePath = false, IItem? excludedPickup = null)
    {
        _cancellationToken = GenerationContext.CancellationToken;
        _inventory = inventory;
        _target = target;
        _start = start;
        _setLocations = setLocations;
        _requirementHandler = ((World)start.World).RequirementHandler;
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
        _visitedStates = new(1024);
        _visitedVertices = new(1024);
        _unvisitedStates = new(1024);
        _visitedItemLocations = new(128);
        _graph = graph;
        _startStates = [];

        List<(Vertex, VisitedState)> startStates = [startState];
        Search(startStates);

    }

    public void Search(List<(Vertex, VisitedState)> initialStates)
    {
        var foundItems = new Dictionary<(Vertex, IItem), VisitedState>();
        var newItems = new Dictionary<(Vertex, IItem), VisitedState>();
        int z = 0;

        foreach (var (vertex, state) in initialStates)
        {
            _startStates.Add((vertex, state));
        }

        do
        {
            _cancellationToken.ThrowIfCancellationRequested();
            z++;
            foundItems = InternalSearch(_startStates, _inventory, _target);
            //newItems = new(foundItems);

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


            //newItems.ExceptWith(prevItems);
            //prevItems.UnionWith(foundItems);
            _visitedVertices.UnionWith(_visitedStates.Select(x => x.Key));

            int newEnergy = (newItems.Count(x => x.Key.Item2.Name == "ETank") * 100);
            int newMissiles = (newItems.Count(x => x.Key.Item2.Name == "Missile") * 5);
            int newSupers = (newItems.Count(x => x.Key.Item2.Name == "Super") * 5);
            int newPowerBombs = (newItems.Count(x => x.Key.Item2.Name == "PowerBomb") * 5);

            foreach (var ((vtx, item), itemState) in newItems)
            {
                _inventory.AddItem(item);
                if (_target == null && !vtx.Name.Contains("Tourian") && !(item.Name.StartsWith("f_") && _inventory.Has(item)))
                {
                    var backtrackSearcher = new StatefulSearcher(_graph, vtx, _inventory.Clone(), null, _start, itemState);
                    if (!backtrackSearcher.HasVisited(_start))
                    {
                        //Console.WriteLine($"Backtracking failed to find a path from {vtx.Name} to {start.Name}");
                        _inventory.RemoveItem(item);
                        newItems.Remove((vtx, item));
                        _prevItems.Remove((vtx, item));

                        if (_unvisitedStates.TryGetValue(vtx, out var states))
                        {
                            states.Item1.Add("Backtrack");
                            _unvisitedStates[vtx] = states;
                        }
                        else
                        {
                            _unvisitedStates[vtx] = (["Backtrack"], [itemState with {
                            Energy = itemState.Energy + newEnergy,
                            Missiles = itemState.Missiles + newMissiles,
                            SuperMissiles = itemState.SuperMissiles + newSupers,
                            PowerBombs = itemState.PowerBombs + newPowerBombs
                        }]);
                        }
                    }
                }
            }

            // Add new items to all unvisited states
            _unvisitedStates = _unvisitedStates.ToDictionary(x => x.Key, x => (x.Value.Item1, x.Value.Item2.Select(s => s with
            {
                Energy = s.Energy + newEnergy,
                Missiles = s.Missiles + newMissiles,
                SuperMissiles = s.SuperMissiles + newSupers,
                PowerBombs = s.PowerBombs + newPowerBombs
            }).ToList()));


            _startStates.Clear();
            var checkItems = newItems.Select(x => x.Key.Item2.Name).ToHashSet();
            checkItems.Add("Backtrack");
            foreach (var (vertex, states) in _unvisitedStates)
            {
                if (states.Item1.Overlaps(checkItems))
                {
                    foreach (var state in states.Item2)
                    {
                        _startStates.Add((vertex, state));
                    }
                }
            }

            // Update all visited states with new energy/ammo where the visited states is not in the start states
            var startStateKeys = _startStates
                .Select(s => s.Item1)
                .ToHashSet();

            _visitedStates = _visitedStates
                .Where(kvp => !startStateKeys.Contains(kvp.Key))
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value
                        .Select(s => s with
                        {
                            Energy = s.Energy + newEnergy,
                            Missiles = s.Missiles + newMissiles,
                            SuperMissiles = s.SuperMissiles + newSupers,
                            PowerBombs = s.PowerBombs + newPowerBombs
                        })
                        .ToList()
                );

            //Console.WriteLine($"-- Found {newItems.Count} new items, {foundItems.Count} total items, {startStates.Count} new start states, {z} passes --");
        } while (newItems.Count > 0);

        if (_target == null)
        {
            //Console.WriteLine($"StatefulSearcher took {stopWatch.ElapsedMilliseconds}ms to complete, doing {z} passes");
        }

        _foundItems.Clear();
        _foundItems.UnionWith(_prevItems.Select(x => x.Key.Item2));
    }

    private void AddUnvisited(Vertex vertex, VisitedState state, HashSet<string> missingItems)
    {
        if (missingItems.Count == 0)
        {
            return;
        }

        //Console.WriteLine($"Adding unvisited state {state} to {vertex.Name} with missing items: {string.Join(",",missingItems)}");
        if (_unvisitedStates.TryGetValue(vertex, out var states))
        {
            var mergedItems = states.Item1.Union(missingItems);

            // If this state is a "best" state, add it
            if (states.Item2.Any(s => s.Dominates(state)))
            {
                // There is a better state already in the list, only update the missing items
                states.Item1 = mergedItems.ToHashSet();
            }
            else
            {
                states.Item2.RemoveAll(s => state.Dominates(s));
                states.Item2.Add(state);
                states.Item1 = mergedItems.ToHashSet();
            }
            _unvisitedStates[vertex] = states;
        }
        else
        {
            _unvisitedStates[vertex] = (missingItems, new List<VisitedState> { state });
        }
    }

    private Dictionary<(Vertex, IItem), VisitedState> InternalSearch(List<(Vertex, VisitedState)> starts, Inventory inventory, Vertex? target = null)
    {
        var foundItems = new Dictionary<(Vertex, IItem), VisitedState>();
        _queue.Clear();
        _inQueue.Clear();
        _currentWeapons.Clear();
        _currentWeapons.UnionWith(((World)_start.World).JsonData.Weapons.Weapons
            .Where(w => _requirementHandler.HandleRequirement(w.UseRequires, new VisitedState(), inventory, (World)_start.World, []).Met));

        foreach (var start in starts)
        {
            EnqueueState(start.Item1, start.Item2, null);
        }

        while (_queue.Count > 0)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            var (current, state, incomingStep) = DequeueState()!.Value;

            //Console.WriteLine($"Visiting {current.Name} with state {state}");

            // If we've already visited this vertex with a state that dominates the current state, skip it
            if (_visitedStates.TryGetValue(current, out var visitedStates) && visitedStates.Any(vs => vs.Dominates(state)))
            {
                continue;
            }

            // Add the current state to the visited states for this vertex
            if (!_visitedStates.TryGetValue(current, out visitedStates))
            {
                visitedStates = [];
                _visitedStates[current] = visitedStates;
            }

            else
            {
                _visitedStates[current].Add(state);
            }
            if (_capturePath)
                _predecessors.TryAdd(current, incomingStep);


            if (target != null && current == target)
            {
                return [];
            }

            //Console.WriteLine($"Visiting {current.Name} with state {state}");

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
                            _visitedStates[target] = [state];
                            return [];
                        }

                        _otherWorldLocations.Add(edge.To);
                        continue;
                    }

                    var toVtx = (Vertex)edge.To;
                    EnqueueState(toVtx, state, _capturePath
                        ? new StatefulPathStep(edge, null, new Dictionary<IItem, int>(),
                            new Dictionary<string, int>())
                        : null);
                }
                continue;
            }


            bool unlocked = false;
            var (unlockState, yields, unlockStrategy, unlockResult) =
                UnlockNode(inventory, current, state, currentNode);
            Dictionary<IItem, int>? unlockRequirements =
                _capturePath ? new Dictionary<IItem, int>() : null;
            Dictionary<string, int>? unlockResources =
                _capturePath ? new Dictionary<string, int>() : null;
            bool unlockRecordedOnIncoming = false;
            if (unlockState != null)
            {
                if (_capturePath)
                    unlockResources = ResourcesSpent(state, unlockState.Value);
                state = unlockState.Value;
                if (_capturePath && unlockResult != null)
                {
                    foreach (var (item, count) in unlockResult.UsedItems ?? [])
                        unlockRequirements![current.World.GetItem(item)] = count;
                    AddResourceRequirements(
                        unlockRequirements!, unlockResult.Cost!.Value, current.World);
                }
                if (_capturePath && _predecessors[current] is { } predecessor
                    && _recordedUnlocks.Add(current)
                    && (unlockStrategy != null || unlockRequirements!.Count > 0))
                {
                    _predecessors[current] = MergePathStep(
                        predecessor, unlockStrategy, unlockRequirements!, unlockResources!);
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
                foreach (var yieldItem in yields)
                {
                    if (ReferenceEquals(yieldItem.Key.Item2, _excludedPickup))
                        continue;
                    if (foundItems.TryGetValue(yieldItem.Key, out var existingState))
                    {
                        if (yieldItem.Value.Dominates(existingState))
                        {
                            foundItems[yieldItem.Key] = yieldItem.Value;
                        }
                    }
                    else
                    {
                        foundItems.Add(yieldItem.Key, yieldItem.Value);
                    }
                }
                if (_capturePath)
                {
                    foreach (var (yieldLocation, yieldItem) in yields.Keys)
                    {
                        _pickupDetails.TryAdd((yieldLocation, yieldItem),
                            new StatefulPickup(yieldItem, unlockStrategy,
                                unlockRequirements!, unlockResources!));
                    }
                }
            }

            if (unlocked && current.Type == VertexType.Item)
            {
                if (_visitedItemLocations.TryGetValue(current, out var visitedItemState))
                {
                    _visitedItemLocations[current] = (state, visitedItemState.Item2.Merge(inventory));
                }
                else
                {
                    _visitedItemLocations.Add(current, (state, inventory.Clone()));
                }

                if (_capturePath && current.Item != null)
                    _pickupDetails.TryAdd((current, current.Item),
                        new StatefulPickup(current.Item, unlockStrategy,
                            unlockRequirements!, unlockResources!));

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
                        _visitedStates[target] = [state];
                        return [];
                    }

                    _otherWorldLocations.Add(edge.To);
                    continue;
                }

                //Console.WriteLine($"Checking edge {current.Name} -> {edge.To.Name}");
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
                                ? new Dictionary<IItem, int>()
                                : unlockRequirements!,
                            unlockRecordedOnIncoming
                                ? new Dictionary<string, int>()
                                : unlockResources!)
                        : null);
                    continue;
                }

                var stratStates = new List<(Strat Strat, VisitedState State, RequirementResult Result)>();
                foreach (var strat in ((Edge)edge).Strats ?? [])
                {
                    //Console.WriteLine($"Checking strat {strat.Name} at {current.Name} with state {state}");
                    var result = _requirementHandler.HandleRequirement(strat.Requires, state, inventory, (World)current.World, _currentWeapons);
                    if (!result.Met)
                    {
                        //Console.WriteLine($"Failed to handle strat {strat.Name} at {current.Name} with state {state}");
                        AddUnvisited(current, state, result.Missing ?? []);
                        continue;
                    }

                    var cost = result.Cost!;

                    var newState = state.ApplyCost(cost.Value, inventory, (World)current.World);
                    if (newState == null)
                    {
                        //Console.WriteLine($"Failed to apply cost {cost} at {current.Name} with state {state}");
                        // Figure out what costs we're missing
                        var missingCostItems = new HashSet<string>(result.Missing ?? []);
                        if (state.Energy - cost.Value.Energy <= 0) { missingCostItems.Add("ETank"); }
                        if (state.Missiles - cost.Value.Missiles <= 0) { missingCostItems.Add("Missile"); }
                        if (state.SuperMissiles - cost.Value.SuperMissiles <= 0) { missingCostItems.Add("Super"); }
                        if (state.PowerBombs - cost.Value.PowerBombs <= 0) { missingCostItems.Add("PowerBomb"); }


                        AddUnvisited(current, state, missingCostItems);
                        continue;
                    }

                    var finalState = newState.Value
                        .WithObstacles(strat.ClearsObstacles ?? [])
                        .WithoutObstacles(strat.ResetsObstacles ?? []);

                    stratStates.Add((strat, finalState, result));

                    if (_capturePath)
                    {
                        var stratRequirements = (result.UsedItems ?? []).ToDictionary(
                            pair => (IItem)current.World.GetItem(pair.Key), pair => pair.Value);
                        AddResourceRequirements(stratRequirements, result.Cost!.Value, current.World);
                        foreach (var flag in strat.SetsFlags ?? [])
                        {
                            var flagItem = current.World.GetItem(flag);
                            _pickupDetails.TryAdd((current, flagItem),
                                new StatefulPickup(flagItem, strat.Name, stratRequirements,
                                    ResourcesSpent(state, finalState)));
                        }
                    }
                }

                if (stratStates.Count == 0)
                {
                    continue;
                }

                // Find the best state (by comparing health and ammo) out of the completed strats and use that for enqueueing the next vertex
                var (bestStrat, bestState, bestResult) = stratStates.First();
                foreach (var (strat, stratState, result) in stratStates)
                {
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
                                if (stratState.Dominates(existingState))
                                {
                                    foundItems[(current, flagItem)] = stratState;
                                }
                            }
                            else
                            {
                                foundItems.Add((current, flagItem), stratState);
                            }
                        }
                    }

                    if (stratState.Dominates(bestState))
                    {
                        bestState = stratState;
                        bestStrat = strat;
                        bestResult = result;
                    }
                }


                Vertex toVtx = (Vertex)edge.To;

                if (toVtx.RoomId != current.RoomId)
                {
                    bestState = bestState with { ObstacleBitFlags = 0, DoorUnlockedFlags = 0 };
                }

                StatefulPathStep? pathStep = null;
                if (_capturePath)
                {
                    var requirements = (bestResult.UsedItems ?? []).ToDictionary(
                        pair => (IItem)current.World.GetItem(pair.Key), pair => pair.Value);
                    AddResourceRequirements(requirements, bestResult.Cost!.Value, current.World);
                    if (!unlockRecordedOnIncoming)
                    {
                        foreach (var (item, count) in unlockRequirements!)
                            requirements[item] = Math.Max(requirements.GetValueOrDefault(item), count);
                    }
                    pathStep = new StatefulPathStep(edge,
                        JoinStrategies(unlockRecordedOnIncoming ? null : unlockStrategy,
                            bestStrat.Name), requirements,
                        MergeResources(
                            unlockRecordedOnIncoming
                                ? new Dictionary<string, int>()
                                : unlockResources!,
                            ResourcesSpent(state, bestState)));
                }
                EnqueueState(toVtx, bestState, pathStep);
            }
        }

        return foundItems;
    }

    private void EnqueueState(Vertex v, VisitedState s, StatefulPathStep? step)
    {
        if (!_inQueue.TryGetValue(v, out var list))
        {
            list = [];
            _inQueue[v] = list;
        }

        if (list.Any(existing => existing.Dominates(s)))
        {
            return;
        }

        list.RemoveAll(existing => s.Dominates(existing));
        list.Add(s);
        _queue.Enqueue((v, s, step));
    }

    private (Vertex, VisitedState, StatefulPathStep?)? DequeueState()
    {
        if (_queue.Count == 0)
            return null;

        var (v, s, step) = _queue.Dequeue();

        if (_inQueue.TryGetValue(v, out var list))
        {
            list.RemoveAll(x => x.Equals(s));
            if (list.Count == 0)
                _inQueue.Remove(v);
        }
        return (v, s, step);
    }

    private static void AddResourceRequirements(
        Dictionary<IItem, int> requirements, RequirementCost cost, IWorld world)
    {
        (string Type, int Amount)[] resources =
        [
            ("Energy", cost.Energy),
            ("Missile", cost.Missiles),
            ("Super", cost.SuperMissiles),
            ("PowerBomb", cost.PowerBombs),
        ];
        foreach (var (type, amount) in resources)
        {
            // AmmoDrain stores a drain marker in bit 15. It consumes whatever
            // resource is present (up to the encoded amount), so it is not a
            // capacity requirement and must not be converted into item packs.
            if (amount > 0x8000)
                continue;
            var (itemName, count) = RequirementHandler.RequiredExpansion(type, amount);
            if (count <= 0)
                continue;
            var item = world.GetItem(itemName);
            requirements[item] = Math.Max(requirements.GetValueOrDefault(item), count);
        }
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

    private static Dictionary<string, int> ResourcesSpent(VisitedState before, VisitedState after)
    {
        var resources = new Dictionary<string, int>();
        if (before.Energy > after.Energy)
            resources["Energy"] = before.Energy - after.Energy;
        if (before.Missiles > after.Missiles)
            resources["Missiles"] = before.Missiles - after.Missiles;
        if (before.SuperMissiles > after.SuperMissiles)
            resources["Super Missiles"] = before.SuperMissiles - after.SuperMissiles;
        if (before.PowerBombs > after.PowerBombs)
            resources["Power Bombs"] = before.PowerBombs - after.PowerBombs;
        return resources;
    }

    private static Dictionary<string, int> MergeResources(
        IReadOnlyDictionary<string, int> first, IReadOnlyDictionary<string, int> second)
    {
        var merged = first.ToDictionary();
        foreach (var (resource, amount) in second)
            merged[resource] = merged.GetValueOrDefault(resource) + amount;
        return merged;
    }

    private static string? JoinStrategies(string? first, string? second) =>
        first == null ? second : second == null || second == first ? first : $"{first}; {second}";

    private (VisitedState?, Dictionary<(Vertex, IItem), VisitedState>, string?, RequirementResult?)
        UnlockNode(Inventory inventory, Vertex current, VisitedState lockState, Node currentNode)
    {
        var yields = new Dictionary<(Vertex, IItem), VisitedState>();
        var usedStrategies = new List<string>();
        var combinedResult = RequirementResult.Success(RequirementCost.ZeroCost);

        if (currentNode.Locks != null)
        {
            foreach (var lck in currentNode.Locks)
            {
                // Check if this lock requires a specific item or flag to be locked, and if we don't fullfill the lock requirements, skip it
                if (lck.Lock != null)
                {
                    var lockResult = _requirementHandler.HandleRequirement(lck.Lock, lockState, inventory, (World)current.World, _currentWeapons);
                    if (!lockResult.Met)
                    {
                        AddUnvisited(current, lockState, lockResult.Missing ?? []);
                        continue;
                    }
                }

                var unlockStratStates = new List<(Strat Strat, VisitedState State, RequirementResult Result)>();


                foreach (var unlockStrat in lck.UnlockStrats ?? [])
                {
                    var result = _requirementHandler.HandleRequirement(unlockStrat.Requires, lockState, inventory, (World)current.World, _currentWeapons);
                    if (!result.Met)
                    {
                        AddUnvisited(current, lockState, result.Missing ?? []);
                        continue;
                    }

                    var cost = result.Cost!;

                    var newState = lockState.ApplyCost(cost.Value, inventory, (World)current.World);
                    if (newState == null)
                    {
                        var missingCostItems = new HashSet<string>(result.Missing ?? []);
                        if (lockState.Energy - cost.Value.Energy <= 0) { missingCostItems.Add("ETank"); }
                        if (lockState.Missiles - cost.Value.Missiles <= 0) { missingCostItems.Add("Missile"); }
                        if (lockState.SuperMissiles - cost.Value.SuperMissiles <= 0) { missingCostItems.Add("Super"); }
                        if (lockState.PowerBombs - cost.Value.PowerBombs <= 0) { missingCostItems.Add("PowerBomb"); }
                        AddUnvisited(current, lockState, missingCostItems);
                        continue;
                    }

                    var finalState = newState.Value
                        .WithObstacles(unlockStrat.ClearsObstacles ?? [])
                        .WithoutObstacles(unlockStrat.ResetsObstacles ?? []);

                    unlockStratStates.Add((unlockStrat, finalState, result));
                }

                if (unlockStratStates.Count == 0)
                {

                    return (null, [], null, null);
                }

                var (bestStrat, bestState, bestResult) = unlockStratStates.First();
                foreach (var (unlockStrat, unlockStratState, result) in unlockStratStates)
                {
                    if (unlockStratState.Dominates(bestState))
                    {
                        bestState = unlockStratState;
                        bestStrat = unlockStrat;
                        bestResult = result;
                    }
                }

                lockState = bestState;
                if (_capturePath)
                {
                    usedStrategies.Add(bestStrat.Name);
                    combinedResult.Cost += bestResult.Cost!.Value;
                    combinedResult.MergeSuccess(bestResult);
                }
                foreach (var yield in lck.Yields ?? [])
                {
                    var yieldItem = current.World.GetItem(yield);
                    yields.Add((current, yieldItem), lockState);
                }
            }
        }

        return (lockState, yields,
            usedStrategies.Count == 0 ? null : string.Join("; ", usedStrategies),
            usedStrategies.Count == 0 ? null : combinedResult);
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

    public bool BacktrackLocation(Vertex vertex, Inventory inventory, Vertex target, IItem itemToPlace)
    {
        // A location this searcher never reached (e.g. one only reachable with items
        // found in other games) has no state to backtrack from.
        if (!_visitedItemLocations.TryGetValue(vertex, out var visited))
            return false;

        var (startState, startFlags) = visited;

        foreach (var flag in startFlags.All())
        {
            if (!inventory.Has(flag.Key))
                inventory.AddItem(flag.Key);
        }

        while (inventory.Has(itemToPlace))
        {
            inventory.RemoveItem(itemToPlace);
        }

        var backtrackSearcher = new StatefulSearcher(_graph, vertex, inventory, null, target, startState);
        if (!backtrackSearcher.HasVisited(target))
        {
            return false;
        }

        return true;
    }

    public void ResumeSearch(IEnumerable<Randomizer.Graph.Vertex> startAt, Inventory prevInventory)
    {
        var diffItems = _inventory.All().Except(prevInventory.All()).Where(x => x.Key.World == _start.World).Select(x => x.Key.Name).ToHashSet();

        var newEnergy = (_inventory.GetCount(_start.World.GetItem("ETank")) - prevInventory.GetCount(_start.World.GetItem("ETank"))) * 100;
        var newMissiles = (_inventory.GetCount(_start.World.GetItem("Missile")) - prevInventory.GetCount(_start.World.GetItem("Missile"))) * 5;
        var newSupers = (_inventory.GetCount(_start.World.GetItem("Super")) - prevInventory.GetCount(_start.World.GetItem("Super"))) * 5;
        var newPowerBombs = (_inventory.GetCount(_start.World.GetItem("PowerBomb")) - prevInventory.GetCount(_start.World.GetItem("PowerBomb"))) * 5;

        // Add new items to all unvisited states
        _unvisitedStates = _unvisitedStates.ToDictionary(x => x.Key, x => (x.Value.Item1, x.Value.Item2.Select(s => s with
        {
            Energy = s.Energy + newEnergy,
            Missiles = s.Missiles + newMissiles,
            SuperMissiles = s.SuperMissiles + newSupers,
            PowerBombs = s.PowerBombs + newPowerBombs
        }).ToList()));


        foreach (var (vertex, states) in _unvisitedStates)
        {
            if (states.Item1.Overlaps(diffItems))
            {
                foreach (var state in states.Item2)
                {
                    _startStates.Add((vertex, state));
                }
            }
        }

        foreach (Vertex start in startAt)
        {
            if (!_visitedVertices.Contains(start))
            {
                _startStates.Add((start, new VisitedState
                {
                    Energy = 99 + _inventory.GetCount(start.World.GetItem("ETank")) * 100,
                    Missiles = _inventory.GetCount(start.World.GetItem("Missile")) * 5,
                    SuperMissiles = _inventory.GetCount(start.World.GetItem("Super")) * 5,
                    PowerBombs = _inventory.GetCount(start.World.GetItem("PowerBomb")) * 5,
                    ObstacleBitFlags = 0
                }));
            }
        }

        // Update all visited states with new energy/ammo where the visited states is not in the start states
        var startStateKeys = _startStates
            .Select(s => s.Item1)
            .ToHashSet();

        _visitedStates = _visitedStates
            .Where(kvp => !startStateKeys.Contains(kvp.Key))
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value
                    .Select(s => s with
                    {
                        Energy = s.Energy + newEnergy,
                        Missiles = s.Missiles + newMissiles,
                        SuperMissiles = s.SuperMissiles + newSupers,
                        PowerBombs = s.PowerBombs + newPowerBombs
                    })
                    .ToList()
            );

        if (_startStates.Count > 0)
        {
            Search([]);
        }
    }

    public IEnumerable<Randomizer.Graph.Vertex> GetOtherWorld() => _otherWorldLocations;

    public IReadOnlyDictionary<Randomizer.Graph.Vertex, StatefulPathStep?> GetPredecessors() =>
        _predecessors;

    internal StatefulPickup? GetPickupDetails(Randomizer.Graph.Vertex location, IItem item) =>
        _pickupDetails.GetValueOrDefault((location, item));

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
                    new StatefulPickup(location.Item, null, new Dictionary<IItem, int>(),
                        new Dictionary<string, int>())));
        }

        foreach (var ((location, item), _) in _prevItems)
        {
            if (!ReferenceEquals(location.Item, item))
                yield return (location, _pickupDetails.GetValueOrDefault(
                    (location, item),
                    new StatefulPickup(item, null, new Dictionary<IItem, int>(),
                        new Dictionary<string, int>())));
        }
    }
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
            cost.SuperMissiles += (int)Math.Ceiling(diff / 3.0m);
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
