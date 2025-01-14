namespace Randomizer.Games.SuperMetroid;

using MathNet.Numerics.Optimization;
using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YamlDotNet.RepresentationModel;
using ExitCondition = Model.ExitCondition;

public class GraphPreprocessor
{
    private JsonReader _reader;
    private World _world;
    private SmGraph _graph;
    private List<string> _allowedTechs;

    public GraphPreprocessor(JsonReader reader, World world, List<string> allowedTechs)
    {
        _reader = reader;
        _graph = new();
        _world = world;
        _allowedTechs = allowedTechs;
    }

    public void Preprocess()
    {
        BuildGraph();
        BuildRunways();
        PruneGraph();

        foreach(var vtx in _graph.Vertices)
        {
            _world.Graph.AddVertex(vtx);
            foreach(var edge in _graph.GetEdges(vtx))
            {
                vtx.Edges.Add(edge);
            }
        }
    }

    private void BuildRunways()
    {
        //TODO: Resolve runways to enable shinesparking requirements

    }

    private void PruneGraph()
    {
        //TODO: Prune edges that can't be fullfilled or have invalid strats or no strats

    }

    public Requirement OptimizeRequirement(Requirement req)
    {
        switch (req)
        {
            case Requirement.Single singleReq:
                return singleReq switch
                {
                    Requirement.Single s when s.Req.Contains("ArtificialMorph") => new Requirement.Never(),
                    Requirement.Single s when s.Req.Contains("XMode") => new Requirement.Never(),
                    Requirement.Single s when s.Req.Contains("CrystalFlash") => new Requirement.Never(),
                    Requirement.Single s when s.Req.StartsWith("can") && !_allowedTechs.Contains(s.Req) => new Requirement.Never(),
                    _ => singleReq
                };

            case Requirement.ObstaclesNotCleared onClear:
                if (onClear.Obstacles.Length == 0)
                    return new Requirement.Always();
                return onClear;

            case Requirement.ObstaclesCleared cleared:
                if (cleared.Obstacles.Length == 0)
                    return new Requirement.Always();
                return cleared;

            case Requirement.And andReq:
                return OptimizeAnd(andReq);

            case Requirement.Or orReq:
                return OptimizeOr(orReq);

            case Requirement.Not notReq:
                return OptimizeNot(notReq);

            case Requirement.GainFlashSuit gainFlash:
                return new Requirement.Always();

            case Requirement.UseFlashSuit useFlash:
                return new Requirement.Never();

            case Requirement.NoFlashSuit noFlash:
                return new Requirement.Always();

        }

        return req;
    }

    private Requirement OptimizeAnd(Requirement.And andReq)
    {
        var optimizedSubs = andReq.Reqs
            .Select(OptimizeRequirement)
            .ToList();

        var flattened = new List<Requirement>();
        foreach (var sub in optimizedSubs)
        {
            if (sub is Requirement.And innerAnd)
            {
                flattened.AddRange(innerAnd.Reqs);
            }
            else
            {
                flattened.Add(sub);
            }
        }
        optimizedSubs = flattened;

        optimizedSubs.RemoveAll(r => r is Requirement.Always);

        if (optimizedSubs.Any(r => r is Requirement.Never))
            return new Requirement.Never();

        if (optimizedSubs.Count == 0)
            return new Requirement.Always();

        if (optimizedSubs.Count == 1)
            return optimizedSubs[0];

        return new Requirement.And(optimizedSubs.ToArray());
    }

    private Requirement OptimizeOr(Requirement.Or orReq)
    {
        var optimizedSubs = orReq.Reqs
            .Select(OptimizeRequirement)
            .ToList();

        var flattened = new List<Requirement>();
        foreach (var sub in optimizedSubs)
        {
            if (sub is Requirement.Or innerOr)
            {
                flattened.AddRange(innerOr.Reqs);
            }
            else
            {
                flattened.Add(sub);
            }
        }
        optimizedSubs = flattened;

        optimizedSubs.RemoveAll(r => r is Requirement.Never);

        if (optimizedSubs.Any(r => r is Requirement.Always))
            return new Requirement.Always();

        if (optimizedSubs.Count == 0)
            return new Requirement.Never();

        if (optimizedSubs.Count == 1)
            return optimizedSubs[0];

        return new Requirement.Or(optimizedSubs.ToArray());
    }

    /// <summary>
    /// Optimize a Not requirement.
    /// </summary>
    private Requirement OptimizeNot(Requirement.Not notReq)
    {
        var child = OptimizeRequirement(notReq.Req);

        if (child is Requirement.Always)
            return new Requirement.Never();

        if (child is Requirement.Never)
            return new Requirement.Always();

        if (child is Requirement.Not doubleNot)
            return doubleNot.Req;

        return new Requirement.Not(child);
    }

    private void BuildGraph()
    {
        // Create room vertices
        foreach (var room in _reader.Rooms)
        {
            foreach (var node in room.Nodes)
            {
                var vertex = new Vertex()
                {
                    Name = $"{room.Area} - {room.Name} - {node.Name}",
                    Type = node.NodeItem != null ? VertexType.Item : VertexType.Meta,
                    World = _world,
                    RoomId = room.Id,
                    NodeId = node.Id,
                    Node = node with { Locks = node.Locks == null ? null : node.Locks.Select(l => l with { UnlockStrats = l.UnlockStrats.Select(s => s with { Requires = OptimizeRequirement(s.Requires) }).ToArray() } ).ToArray() },
                };
                _graph.AddVertex(vertex);
            }

            // Connect in-room links
            foreach (var link in room.Links)
            {
                var fromVtx = _graph.Vertices.First(v => v.RoomId == room.Id && v.Node!.Id == link.From);
                foreach (var linkTo in link.To)
                {
                    var toVtx = _graph.Vertices.First(v => v.RoomId == room.Id && v.Node!.Id == linkTo.Id);
                    var linkStrats = room.Strats.Where(s => s.Link![0] == link.From && s.Link![1] == linkTo.Id && (s.ExitCondition == null || s.ExitCondition is ExitCondition.LeaveNormally || s.ExitCondition is ExitCondition.LeaveWithRunway) &&
                        (s.EntranceCondition == null || 
                         s.EntranceCondition is EntranceCondition.ComeInNormally ||
                         s.EntranceCondition is EntranceCondition.ComeInRunning ||
                         s.EntranceCondition is EntranceCondition.ComeInJumping ||
                         s.EntranceCondition is EntranceCondition.ComeInSpinning ||
                         s.EntranceCondition is EntranceCondition.ComesThroughToilet));
                    
                    if (linkStrats.Any())
                    {
                        var optimizedStrats = linkStrats.Select(s => s with { Requires = OptimizeRequirement(s.Requires) }).Where(s => s.Requires is not Requirement.Never);
                        _graph.AddDirected(fromVtx, toVtx, optimizedStrats);
                    } 
                }
            }
        }

        // Connect rooms
        foreach (var connection in _reader.Connections.SelectMany(c => c.Connections))
        {
            var fromVtx = _graph.Vertices.First(v => v.RoomId == connection.Nodes[0].RoomId && v.Node!.Id == connection.Nodes[0].NodeId);
            var toVtx = _graph.Vertices.First(v => v.RoomId == connection.Nodes[1].RoomId && v.Node!.Id == connection.Nodes[1].NodeId);

            var fromNode = fromVtx.Node!;
            var toNode = toVtx.Node!;

            var fromRoom = _reader.RoomById[fromVtx.RoomId];
            var toRoom = _reader.RoomById[toVtx.RoomId];


            // Connect from to to
            ConnectNodes(fromVtx, toVtx, fromNode, toNode, fromRoom, toRoom);

            if (connection.Direction.ToLower() == "bidirectional")
            {
                // Connect to to from
                ConnectNodes(toVtx, fromVtx, toNode, fromNode, toRoom, fromRoom);
            }
        }
    }

    private void ConnectNodes(Vertex fromVtx, Vertex toVtx, Node fromNode, Node toNode, Room fromRoom, Room toRoom)
    {
        List<Strat> fromStrats = fromRoom.Strats.Where(s => s.Link![0] == fromNode.Id && s.ExitCondition != null).ToList();
        List<Strat> toStrats = toRoom.Strats.Where(s => s.Link![0] == toNode.Id && s.EntranceCondition != null).ToList();
        Requirement? unlockReq = null;

        if(fromNode.NodeType == "door" && fromNode.UseImplicitDoorUnlocks == null || fromNode.UseImplicitDoorUnlocks == true)
        {
            unlockReq = fromNode.NodeSubType switch
            {
                "blue" => null,
                "red" => new Requirement.Single("h_canOpenRedDoors"),
                "green" => new Requirement.Single("h_canOpenGreenDoors"),
                "yellow" => new Requirement.Single("h_canOpenYellowDoors"),
                "eye" => new Requirement.Single("h_canOpenEyeDoors"),
                _ => null
            };
        }

        if ((fromNode.NodeType == "door" || fromNode.NodeType == "exit") && (fromNode.UseImplicitLeaveNormally == null || fromNode.UseImplicitLeaveNormally == true))
        {
            // Create implicit leave normally strat if one doesn't exist already
            var strat = new Strat([fromNode.Id, fromNode.Id], "Leave Normally", null, null, null, new Requirement.Always(), new ExitCondition.LeaveNormally(), null, null, null, null, null, null, null, null, null);
            fromStrats.Add(strat);
        }

        if ((toNode.NodeType == "door" || toNode.NodeType == "entrance" || toNode.NodeType == "utility") && (toNode.UseImplicitComeInNormally == null || toNode.UseImplicitComeInNormally == true))
        {
            // Create implicit come in normally strat if one doesn't exist already
            if (!toStrats.Any(s => s.EntranceCondition is EntranceCondition.ComeInNormally))
            {
                var strat = new Strat([toNode.Id, toNode.Id], "Come In Normally", null, null, new EntranceCondition.ComeInNormally(), new Requirement.Always(), null, null, null, null, null, null, null, null, null, null);
                toStrats.Add(strat);
            }
        }

        foreach (var strat in fromStrats)
        {
            // TODO: Implement more of the entrance/exit condition pairs, but this will suffice for almost all "normal" cases
            var targetStrats = strat.ExitCondition switch
            {
                ExitCondition.LeaveNormally ln => toStrats.Where(s => s.EntranceCondition is EntranceCondition.ComeInNormally),
                ExitCondition.LeaveWithRunway lwr => toStrats.Where(s => (
                    s.EntranceCondition is EntranceCondition.ComeInNormally ||
                    s.EntranceCondition is EntranceCondition.ComeInJumping ||
                    s.EntranceCondition is EntranceCondition.ComeInRunning ||
                    s.EntranceCondition is EntranceCondition.ComeInSpinning)),
                _ => []
            };

            // Create edges between the exit node strat and the target room strats
            foreach (var targetStrat in targetStrats)
            {
                // Inject the door unlock requirement into the fromStrat
                var newFromStrat = unlockReq == null ? strat with { Requires = OptimizeRequirement(strat.Requires) } : strat with { Requires = OptimizeRequirement(new Requirement.And([strat.Requires, unlockReq]))};

                var fromStratVtx = _graph.Vertices.First(v => v.RoomId == fromVtx.RoomId && v.Node!.Id == strat.Link![0]);
                var targetStratVtx = _graph.Vertices.First(v => v.RoomId == toVtx.RoomId && v.Node!.Id == targetStrat.Link![0]);

                _graph.AddDirected(fromStratVtx, targetStratVtx, [newFromStrat]);
            }
        }
    }
}


public class SmGraph
{
    public List<Vertex> Vertices { get; } = new();
    public List<Edge> Edges { get; } = new();
    public Dictionary<Vertex, List<Edge>> AdjecencyList { get; } = new();

    public Vertex AddVertex(Vertex vertex)
    {
        Vertices.Add(vertex);
        AdjecencyList.Add(vertex, new());
        return vertex;
    }

    public void AddDirected(Vertex from, Vertex to, IEnumerable<Strat> strats)
    {
        var edge = new Edge(from, to, strats);
        Edges.Add(edge);
        AdjecencyList[from].Add(edge);
    }

    public void AddUndirected(Vertex from, Vertex to, IEnumerable<Strat> strats)
    {
        AddDirected(from, to, strats);
        AddDirected(to, from, strats);
    }

    public void AddDirected(Vertex from, Vertex to, ItemCondition condition)
    {
        var edge = new Edge(from, to, condition);
        Edges.Add(edge);
        AdjecencyList[from].Add(edge);
    }

    public void AddUndirected(Vertex from, Vertex to, ItemCondition condition)
    {
        AddDirected(from, to, condition);
        AddDirected(to, from, condition);
    }

    public IEnumerable<Edge> GetEdges(Vertex vertex)
    {
        return AdjecencyList[vertex];
    }

    public void RemoveVertex(Vertex vertex)
    {
        Vertices.Remove(vertex);
        foreach (var edge in AdjecencyList[vertex])
        {
            Edges.Remove(edge);
        }
        AdjecencyList.Remove(vertex);
    }
}
