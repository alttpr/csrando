namespace Randomizer.Games.SuperMetroid;

using MathNet.Numerics.Optimization;
using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;
using System;
using System.Collections.Generic;
using System.Data;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YamlDotNet.RepresentationModel;
using ExitCondition = Model.ExitCondition;

public class DoorPlmData
{
    public int RoomAddress { get; set; }
    public int DoorAddress { get; set; }
    public int RoomId { get; set; }
    public int NodeId { get; set; }
    public int XPosition { get; set; }
    public int YPosition { get; set; }
    public List<RoomPLM>? PLMs { get; set; }
}

public class GraphPreprocessor
{
    private JsonReader _reader;
    private World _world;
    private SmGraph _graph;
    private List<string> _allowedTechs;

    public GraphPreprocessor(JsonReader reader, World world)
    {
        _reader = reader;
        _graph = new();
        _world = world;
        _allowedTechs = world.AllowedTechs;
    }

    public void Preprocess()
    {
        PatchKeycards();
        BuildGraph();
        BuildRunways();
        PruneGraph();

        foreach (var vtx in _graph.Vertices)
        {
            _world.Graph.AddVertex(vtx);
            foreach(var edge in _graph.GetEdges(vtx))
            {
                vtx.Edges.Add(edge);
            }
        }

        //// Find all doors that are not blue
        //var doors = _graph.Vertices.OfType<SuperMetroid.Vertex>().Where(v => v.Node!.NodeType == "door" && v.Node!.NodeSubType != "elevator");
        //var doorData = DoorReader.ReadDoorData();

        //var roomNodePlmMap = new List<DoorPlmData>();

        //// For each door, find the door shell PLM
        //foreach (var door in doors)
        //{
        //    var otherDoor = door.Edges.Where(e => ((Vertex)e.To).RoomId != ((Vertex)e.From).RoomId).FirstOrDefault()?.To as SuperMetroid.Vertex;
        //    if (otherDoor != null)
        //    {
        //        var doorNodeAddress = int.Parse(otherDoor?.Node?.NodeAddress?.Substring(2) ?? "0", System.Globalization.NumberStyles.HexNumber);
        //        if (doorNodeAddress > 0)
        //        {
        //            var doorHeader = doorData.FirstOrDefault(d => d.ptr == doorNodeAddress);
        //            if (doorHeader != null)
        //            {
        //                DoorPlmData doorPlmData = new()
        //                {
        //                    RoomAddress = doorHeader.room,
        //                    DoorAddress = doorHeader.ptr,
        //                    RoomId = door.RoomId,
        //                    NodeId = door.NodeId,
        //                    XPosition = doorHeader.x_low,
        //                    YPosition = doorHeader.y_low,
        //                    PLMs = null
        //                };

        //                var doorPLMs = _reader.RoomPLMs.Where(r => r.Room == doorHeader.room && r.XPosition == doorHeader.x_low && r.YPosition == doorHeader.y_low);
        //                if (doorPLMs != null)
        //                {
        //                    Console.WriteLine($"Door: {door.Name}, PLM: {doorPLMs}");
        //                    doorPlmData.PLMs = doorPLMs.ToList();
        //                }

        //                roomNodePlmMap.Add(doorPlmData);
        //            }
        //            else
        //            {
        //                Console.WriteLine($"Could not find door header for {door.Name}");
        //            }
        //        }
        //        else
        //        {
        //            Console.WriteLine($"Could not find door address for {door.Name}");
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Could not find other door for {door.Name}");
        //    }
        //}

        //// Write the plm map to a file as serialized json
        //var plmMapJson = System.Text.Json.JsonSerializer.Serialize(roomNodePlmMap);
        //System.IO.File.WriteAllText(JsonReader.DataRoot + "\\plm_door_map.json", plmMapJson);


        //// Find a door PLM for a given door
        //var door = _graph.Vertices.OfType<SuperMetroid.Vertex>().Where(v => v.Node!.NodeSubType == "red").First();
        //var otherDoor = door.Edges.Where(e => ((Vertex)e.To).RoomId != ((Vertex)e.From).RoomId).First().To as SuperMetroid.Vertex;

        //var doorNodeAddress = int.Parse(otherDoor.Node.NodeAddress.Substring(2), System.Globalization.NumberStyles.HexNumber);
        //var doorData = DoorReader.ReadDoorData();
        //var doorHeader = doorData.First(d => d.ptr == doorNodeAddress);

        //var doorPLM = _reader.RoomPLMs.First(r => r.Room == doorHeader.room && r.XPosition == doorHeader.x_low && r.YPosition == doorHeader.y_low);

    }

    private void PatchKeycards()
    {
        if (_world.Config.Keycards == Keycards.All)
        {
            PatchKeyCard(_reader, "Crateria", "Landing Site", "Top Left Door", "CrateriaL1");
            PatchKeyCard(_reader, "Crateria", "Landing Site", "Top Right Door", "CrateriaL1");

            PatchKeyCard(_reader, "Crateria", "Crateria Kihunter Room", "Right Door", "CrateriaL2");

            PatchKeyCard(_reader, "Crateria", "Green Pirates Shaft", "Bottom Right Door", "CrateriaBoss");
            PatchKeyCard(_reader, "Crateria", "Flyway", "Right Door", "CrateriaBoss");

            PatchKeyCard(_reader, "Brinstar", "Construction Zone", "Right Door", "BrinstarL1");

            PatchKeyCard(_reader, "Brinstar", "Green Brinstar Main Shaft", "Below Power Bomb Blocks - Bottom Left Door", "BrinstarL2");
            PatchKeyCard(_reader, "Brinstar", "Pink Brinstar Hopper Room", "Top Right Door", "BrinstarL2");
            PatchKeyCard(_reader, "Brinstar", "Spore Spawn Farming Room", "Right Door", "BrinstarL2");

            PatchKeyCard(_reader, "Brinstar", "Spore Spawn Kihunter Room", "Top Right Door", "BrinstarBoss");
            PatchKeyCard(_reader, "Brinstar", "Kraid Eye Door Room", "Right Door", "BrinstarBoss");

            PatchKeyCard(_reader, "Norfair", "Business Center", "Top Left Door", "NorfairL1");
            PatchKeyCard(_reader, "Norfair", "Crocomire Speedway", "Top of the Shaft Left Door", "NorfairL1");

            PatchKeyCard(_reader, "Norfair", "Cathedral", "Right Door", "NorfairL2");
            PatchKeyCard(_reader, "Norfair", "Upper Norfair Farming Room", "Top Right Door", "NorfairL2");
            PatchKeyCard(_reader, "Norfair", "Purple Shaft", "Top Door", "NorfairL2");
            PatchKeyCard(_reader, "Norfair", "Single Chamber", "Left Shaft - Top Left Door", "NorfairL2");

            PatchKeyCard(_reader, "Norfair", "Crocomire Speedway", "Bottom Door", "NorfairBoss");

            PatchKeyCard(_reader, "Norfair", "The Worst Room In The Game", "Top Right Door", "LowerNorfairL1");
            PatchKeyCard(_reader, "Norfair", "Single Chamber", "Far Right Door", "LowerNorfairL1");

            PatchKeyCard(_reader, "Norfair", "Lower Norfair Farming Room", "Left Door", "LowerNorfairBoss");

            PatchKeyCard(_reader, "Maridia", "Mt. Everest", "Top Right Door", "MaridiaL1");
            PatchKeyCard(_reader, "Maridia", "Aqueduct", "Middle Left Door", "MaridiaL1");

            PatchKeyCard(_reader, "Maridia", "Botwoon Hallway", "Right Door", "MaridiaL2");
            PatchKeyCard(_reader, "Maridia", "Halfie Climb Room", "Bottom Left Door", "MaridiaL2");

            PatchKeyCard(_reader, "Maridia", "The Precious Room", "Bottom Left Door", "MaridiaBoss");

            PatchKeyCard(_reader, "Crateria", "West Ocean", "Upper Right Section - Bottom Right Door", "WreckedShipL1");
            PatchKeyCard(_reader, "Crateria", "Homing Geemer Room", "Right Door", "WreckedShipL1");
            PatchKeyCard(_reader, "Wrecked Ship", "Gravity Suit Room", "Right Door", "WreckedShipL1");

            PatchKeyCard(_reader, "Wrecked Ship", "Basement", "Right Door", "WreckedShipBoss");
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
                    _ => new Requirement.SingleItem(_world.GetItem(singleReq.Req))
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
                if (node.Locks?.SelectMany(l => l.UnlockStrats ?? [])?.Any() == true)
                {
                    foreach (var strat in node.Locks.SelectMany(l => l.UnlockStrats ?? []))
                    {
                        strat.Requires = OptimizeRequirement(strat.Requires);
                    }
                }


                var vertex = new Vertex()
                {
                    Name = $"{room.Area} - {room.Name} - {node.Name}",
                    Type = node.NodeItem != null ? VertexType.Item : VertexType.Meta,
                    World = _world,
                    RoomId = room.Id,
                    NodeId = node.Id,
                    Addresses = node.NodeAddress != null ? [long.Parse(node.NodeAddress.Substring(2), System.Globalization.NumberStyles.HexNumber)] : null,
                    Node = node
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
                        //var optimizedStrats = linkStrats.Select(s => s with { Requires = OptimizeRequirement(s.Requires) }).Where(s => s.Requires is not Requirement.Never);
                        foreach (var strat in linkStrats)
                        {
                            strat.Requires = OptimizeRequirement(strat.Requires);
                        }

                        _graph.AddDirected(fromVtx, toVtx, linkStrats);
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
                string kc when kc.StartsWith("keycard:") => new Requirement.Single(kc.Split(":")[1].Trim()),
                _ => null
            };
        }

        if ((fromNode.NodeType == "door" || fromNode.NodeType == "exit") && (fromNode.UseImplicitLeaveNormally == null || fromNode.UseImplicitLeaveNormally == true))
        {
            // Create implicit leave normally strat if one doesn't exist already            
            var strat = new Strat
            {
                Name = "Leave Normally",
                Link = [fromNode.Id, fromNode.Id],
                Requires = new Requirement.Always(),
                ExitCondition = new ExitCondition.LeaveNormally(),
            };


            fromStrats.Add(strat);
        }

        if ((toNode.NodeType == "door" || toNode.NodeType == "entrance" || toNode.NodeType == "utility") && (toNode.UseImplicitComeInNormally == null || toNode.UseImplicitComeInNormally == true))
        {
            // Create implicit come in normally strat if one doesn't exist already
            if (!toStrats.Any(s => s.EntranceCondition is EntranceCondition.ComeInNormally))
            {
                var strat = new Strat
                {
                    Name = "Come In Normally",
                    Link = [toNode.Id, toNode.Id],
                    Requires = new Requirement.Always(),
                    EntranceCondition = new EntranceCondition.ComeInNormally(),
                };
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
                var newFromStrat = (Strat)strat.Clone();
                newFromStrat.Requires = unlockReq == null ? OptimizeRequirement(strat.Requires) : OptimizeRequirement(new Requirement.And([strat.Requires, unlockReq]));

                var fromStratVtx = _graph.Vertices.First(v => v.RoomId == fromVtx.RoomId && v.Node!.Id == strat.Link![0]);
                var targetStratVtx = _graph.Vertices.First(v => v.RoomId == toVtx.RoomId && v.Node!.Id == targetStrat.Link![0]);

                _graph.AddDirected(fromStratVtx, targetStratVtx, [newFromStrat]);
            }
        }
    }

    public Node PatchNodeWithKey(Node node, string nameToPatch, string keycardName)
    {
        if (node.Name == nameToPatch)
        {
            node.NodeSubType = $"keycard: {keycardName}";
        }

        return node;
    }

    public void PatchKeyCard(JsonReader reader, string areaName, string roomName, string doorName, string keyCardName)
    {
        var room = reader.Rooms.First(x => x.Name == roomName && x.Area == areaName);
        var newNodes = room.Nodes.Select(x => PatchNodeWithKey(x, doorName, keyCardName));
        room.Nodes = newNodes.ToArray();
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
