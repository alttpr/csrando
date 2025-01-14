namespace Randomizer.Games.SuperMetroid;

using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;
using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

public class StatefulSearcher : ISearcher
{
    private Graph _graph;
    private Dictionary<Vertex, List<VisitedState>> _visitedStates;
    private Dictionary<Vertex, (HashSet<string>, List<VisitedState>)> _unvisitedStates;
    private Queue<(Vertex vertex, VisitedState state)> _queue;
    private Dictionary<Vertex, List<VisitedState>> _inQueue;
    private Dictionary<Vertex, (VisitedState, HashSet<string>)> _visitedItemLocations;
    private HashSet<Vertex> _visitedVertices;
    private HashSet<IItem> _foundItems;

    // Implement the same interface as the generic Searcher, but with a stateful implementation that can track
    // energy, ammo, and other stateful information during traversal of the graph.
    public StatefulSearcher(Graph graph, Vertex start, Inventory inventory, SetLocations? setLocations = null, Vertex? target = null, VisitedState? visitedState = null)
    {
        var oldInventory = inventory.Clone();

        var startState = (start, visitedState ?? new VisitedState
        {
            Energy = 99 + inventory.GetCount(start.World.GetItem("ETank")) * 100,
            Missiles = inventory.GetCount(start.World.GetItem("Missile")) * 5,
            SuperMissiles = inventory.GetCount(start.World.GetItem("Super")) * 5,
            PowerBombs = inventory.GetCount(start.World.GetItem("PowerBomb")) * 5,
            ObstacleBitFlags = 0
        });

        inventory = oldInventory.Clone();
        _visitedStates = new();
        _visitedVertices = new();
        _unvisitedStates = new();
        _visitedItemLocations = new();
        _graph = graph;

        var foundItems = new Dictionary<(Vertex, string), VisitedState>();
        var prevItems = new Dictionary<(Vertex, string), VisitedState>();
        var newItems = new Dictionary<(Vertex, string), VisitedState>();

        var stopWatch = System.Diagnostics.Stopwatch.StartNew();
        List<(Vertex, VisitedState)> startStates = [startState];
        int z = 0;

        do
        {
            z++;
            foundItems = InternalSearch(startStates, inventory, target);
            //newItems = new(foundItems);
            
            newItems = foundItems.Where(x => !prevItems.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Value);
            foreach(var foundItem in foundItems)
            {
                if (!prevItems.ContainsKey(foundItem.Key))
                {
                    prevItems.Add(foundItem.Key, foundItem.Value);
                } else
                {
                    prevItems[foundItem.Key] = foundItem.Value;
                }
            }


            //newItems.ExceptWith(prevItems);
            //prevItems.UnionWith(foundItems);
            _visitedVertices.UnionWith(_visitedStates.Select(x => x.Key));

            int newEnergy = (newItems.Count(x => x.Key.Item2 == "ETank") * 100);
            int newMissiles = (newItems.Count(x => x.Key.Item2 == "Missile") * 5);
            int newSupers = (newItems.Count(x => x.Key.Item2 == "Super") * 5);
            int newPowerBombs = (newItems.Count(x => x.Key.Item2 == "PowerBomb") * 5);

            foreach (var ((vtx, item), itemState) in newItems)
            {
                inventory.AddItem(vtx.World.GetItem(item));
                if (target == null && !vtx.Name.Contains("Tourian") && !(item.StartsWith("f_") && inventory.Has(vtx.World.GetItem(item))))
                {
                    var backtrackSearcher = new StatefulSearcher(graph, vtx, inventory, null, start, itemState);
                    if (!backtrackSearcher.HasVisited(start))
                    {
                        //Console.WriteLine($"Backtracking failed to find a path from {vtx.Name} to {start.Name}");
                        inventory.RemoveItem(vtx.World.GetItem(item));
                        newItems.Remove((vtx, item));

                        if (_unvisitedStates.TryGetValue(vtx, out var states))
                        {
                            states.Item1.Add("Backtrack");
                            _unvisitedStates[vtx] = states;
                        }
                        else
                        {
                            _unvisitedStates[vtx] = (new HashSet<string> { "Backtrack" }, [itemState with {
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


            startStates.Clear();
            var checkItems = newItems.Select(x => x.Key.Item2).ToHashSet();
            checkItems.Add("Backtrack");
            foreach (var (vertex, states) in _unvisitedStates)
            {
                if (states.Item1.Overlaps(checkItems))
                {
                    foreach (var state in states.Item2)
                    {
                        startStates.Add((vertex, state));
                    }
                }
            }

            // Update all visited states with new energy/ammo where the visited states is not in the start states
            var startStateKeys = startStates
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

        if (target == null)
        {
            //Console.WriteLine($"StatefulSearcher took {stopWatch.ElapsedMilliseconds}ms to complete, doing {z} passes");
        }

        _foundItems = new HashSet<IItem>(prevItems.Select(x => x.Key.Item2).Select(x => start.World.GetItem(x)));
    }

    private void AddUnvisited(Vertex vertex, VisitedState state, HashSet<string> missingItems)
    {
        //Console.WriteLine($"Adding unvisited state {state} to {vertex.Name} with missing items: {string.Join(",",missingItems)}");
        if (_unvisitedStates.TryGetValue(vertex, out var states))
        {
            var mergedItems = states.Item1.Union(missingItems);
            
            // If this state is a "best" state, add it
            if(states.Item2.Any(s => s.Dominates(state)))
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

    private Dictionary<(Vertex, string), VisitedState> InternalSearch(List<(Vertex, VisitedState)> starts, Inventory inventory, Vertex? target = null)
    {
        var foundItems = new Dictionary<(Vertex, string), VisitedState>();
        _queue = new Queue<(Vertex vertex, VisitedState state)>();
        _inQueue = new Dictionary<Vertex, List<VisitedState>>();

        foreach(var start in starts)
        {
            EnqueueState(start.Item1, start.Item2);
        }

        while (_queue.Count > 0)
        {

            var (current, state) = DequeueState()!.Value;

            //Console.WriteLine($"Visiting {current.Name} with state {state}");
                        
            // If we've already visited this vertex with a state that dominates the current state, skip it
            if (_visitedStates.TryGetValue(current, out var visitedStates) && visitedStates.Any(vs => vs.Dominates(state)))
            {
                continue;
            }

            // Add the current state to the visited states for this vertex
            if (!_visitedStates.TryGetValue(current, out visitedStates))
            {
                visitedStates = new();
                _visitedStates[current] = visitedStates;
            }
            else
            {
                _visitedStates[current].Add(state);
            }


            if (target != null && current == target)
            {
                return [];
            }

            //Console.WriteLine($"Visiting {current.Name} with state {state}");

            var currentNode = current.Node!;

            bool unlocked = false;
            var (unlockState, yields) = UnlockNode(inventory, current, state, currentNode);
            if (unlockState != null)
            {
                state = unlockState.Value;
                if(current.Node!.NodeType == "door")
                {
                    state = state.WithDoorUnlocked(current.NodeId);
                }

                unlocked = true;
                foreach(var yieldItem in yields)
                {
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
            }

            if(current.Type == VertexType.Item)
            {
                var currentInventory = inventory.All().Keys.Select(x => x.Name).ToHashSet();
                if (_visitedItemLocations.TryGetValue(current, out var visitedItemState))
                {
                    _visitedItemLocations[current] = (state, visitedItemState.Item2.Union(currentInventory).ToHashSet());
                }
                else
                {
                    _visitedItemLocations.Add(current, (state, currentInventory));
                }
            }


            if (unlocked && current.Type == VertexType.Item && current.Item != null)
            {
                if (foundItems.TryGetValue((current, current.Item.Name), out var existingState))
                {
                    if (state.Dominates(existingState))
                    {
                        foundItems[(current, current.Item.Name)] = state;
                    }
                }
                else
                {
                    foundItems.Add((current, current.Item.Name), state);
                }
            }
 
            foreach (var edge in current.Edges)
            {
                //Console.WriteLine($"Checking edge {current.Name} -> {edge.To.Name}");
                // Can't traverse doors if we couldn't unlock the node
                if (!unlocked && ((Vertex)edge.To).RoomId != current.RoomId)
                {
                    continue;
                }

                var stratStates = new List<(Strat, VisitedState)>();
                foreach (var strat in ((Edge)edge).Strats ?? [])
                {
                    //Console.WriteLine($"Checking strat {strat.Name} at {current.Name} with state {state}");
                    var result = RequirementHandler.HandleRequirement(strat.Requires, state, inventory, (World)current.World);
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

                    stratStates.Add((strat, finalState));
                }

                if (stratStates.Count == 0)
                {                     
                    continue;
                }

                // Find the best state (by comparing health and ammo) out of the completed strats and use that for enqueueing the next vertex
                var (bestStrat, bestState) = stratStates.First();
                foreach (var (strat, stratState) in stratStates)
                {
                    // If any of the strats we "could" use sets flags, add them to the found items
                    if (strat.SetsFlags != null)
                    {
                        foreach(var flag in strat.SetsFlags)
                        {
                            if (foundItems.TryGetValue((current, flag), out var existingState))
                            {
                                if (stratState.Dominates(existingState))
                                {
                                    foundItems[(current, flag)] = stratState;
                                }
                            }
                            else
                            {
                                foundItems.Add((current, flag), stratState);
                            }
                        }
                    }

                    if (stratState.Dominates(bestState))
                    {
                        bestState = stratState;
                        bestStrat = strat;
                    }
                }


                Vertex toVtx = (Vertex)edge.To;

                if(toVtx.RoomId != current.RoomId)
                {
                    bestState = bestState with { ObstacleBitFlags = 0, DoorUnlockedFlags = 0 };
                }

                EnqueueState(toVtx, bestState);
            }
        }

        return foundItems;
    }

    private void EnqueueState(Vertex v, VisitedState s)
    {
        if (!_inQueue.TryGetValue(v, out var list))
        {
            list = new List<VisitedState>();
            _inQueue[v] = list;
        }

        if (list.Any(existing => existing.Dominates(s)))
        {
            return;
        }

        list.RemoveAll(existing => s.Dominates(existing));
        list.Add(s);
        _queue.Enqueue((v, s));
    }

    private (Vertex, VisitedState)? DequeueState()
    {
        if (_queue.Count == 0)
            return null;

        var (v, s) = _queue.Dequeue();

        if (_inQueue.TryGetValue(v, out var list))
        {
            list.RemoveAll(x => x.Equals(s));
            if (list.Count == 0)
                _inQueue.Remove(v);
        }
        return (v, s);
    }

    private (VisitedState?, Dictionary<(Vertex, string), VisitedState>) UnlockNode(Inventory inventory, Vertex current, VisitedState lockState, Node currentNode)
    {
        var yields = new Dictionary<(Vertex, string), VisitedState>();

        if (currentNode.Locks != null)
        {
            foreach (var lck in currentNode.Locks)
            {
                // Check if this lock requires a specific item or flag to be locked, and if we don't fullfill the lock requirements, skip it
                if (lck.Lock != null)
                {
                    var lockResult = RequirementHandler.HandleRequirement(lck.Lock, lockState, inventory, (World)current.World);
                    if (!lockResult.Met)
                    {
                        AddUnvisited(current, lockState, lockResult.Missing ?? []);
                        continue;
                    }
                }

                var unlockStratStates = new List<VisitedState>();
                

                foreach (var unlockStrat in lck.UnlockStrats)
                {
                    var result = RequirementHandler.HandleRequirement(unlockStrat.Requires, lockState, inventory, (World)current.World);
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

                    unlockStratStates.Add(finalState);
                }

                if (unlockStratStates.Count == 0)
                {
                    
                    return (null, []);
                }

                var bestState = unlockStratStates.First();
                foreach (var unlockStratState in unlockStratStates)
                {
                    if (unlockStratState.Dominates(bestState))
                    {
                        bestState = unlockStratState;
                    }
                }

                lockState = bestState;
                foreach (var yield in lck.Yields ?? [])
                {
                    yields.Add((current, yield), lockState);
                }
            }
        }

        return (lockState, yields);
    }

    IEnumerable<Randomizer.Graph.Vertex> ISearcher.GetEmptyLocationsInSet(ItemSetName itemSet, Dictionary<ItemSetName, int>? itemSets, bool onlyReachable)
    {
        return _visitedVertices.Where(v => 
            v.Node != null &&
            v.Type == VertexType.Item &&
            v.Item == null
        );
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

    public bool BacktrackLocation(Vertex vertex, Inventory inventory, Vertex target, IItem itemToPlace)
    {
        var (startState, startFlags) = _visitedItemLocations[vertex];
        
        foreach(var flag in startFlags)
        {
            inventory.AddItem(vertex.World.GetItem(flag));
        }

        while(inventory.Has(itemToPlace))
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
        if (cost.Energy > 0x8000) { cost.Energy = Math.Min(Energy, cost.Energy & 0x7FFF); };
        if (cost.Missiles > 0x8000) { cost.Missiles = Math.Min(Missiles, cost.Missiles & 0x7FFF); };
        if (cost.SuperMissiles > 0x8000) { cost.SuperMissiles = Math.Min(SuperMissiles, cost.SuperMissiles & 0x7FFF); };
        if (cost.PowerBombs > 0x8000) { cost.PowerBombs = Math.Min(PowerBombs, cost.PowerBombs & 0x7FFF); };

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
