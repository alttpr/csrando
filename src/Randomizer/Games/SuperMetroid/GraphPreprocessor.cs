namespace Randomizer.Games.SuperMetroid;

using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;
using BaseEdge = Randomizer.Graph.Edge;
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
    private readonly JsonReader _reader;
    private readonly World _world;
    private readonly SmGraph _graph;
    private readonly List<string> _allowedTechs;
    private readonly Dictionary<Strat, Vertex> _entranceVertices = [];
    private int _entranceVertexId;

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
        PatchVanillaMapPreopenedDoors();
        PatchBossGate();

        // Patch morph PLM
        var morphRoom = _reader.Rooms.First(r => r.Name == "Morph Ball Room");
        var morphNode = morphRoom.Nodes.First(n => n.Name == "Right Item");
        morphNode.NodeAddress = "0x786EC";

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

    private void PatchBossGate()
    {
        if (_world.Map != null)
            return;

        var statuesRoom = _reader.Rooms.First(room => room.Name == "Statues Room");
        var bossRequirement = BossRequirement(int.Parse(_world.Config.Bosses));
        foreach (var statuesCutscene in statuesRoom.Strats.Where(strat =>
                     strat.Name == "Statues Cutscene"))
            statuesCutscene.Requires = bossRequirement;
    }

    private static Requirement BossRequirement(int requiredBosses)
    {
        string[] bossFlags =
        [
            "f_DefeatedKraid",
            "f_DefeatedPhantoon",
            "f_DefeatedDraygon",
            "f_DefeatedRidley",
        ];
        if (requiredBosses <= 0)
            return new Requirement.Always();

        var alternatives = Enumerable.Range(0, 1 << bossFlags.Length)
            .Where(mask => Enumerable.Range(0, bossFlags.Length)
                .Count(index => (mask & (1 << index)) != 0) == requiredBosses)
            .Select(mask => new Requirement.And(Enumerable.Range(0, bossFlags.Length)
                .Where(index => (mask & (1 << index)) != 0)
                .Select(index => (Requirement)new Requirement.Single(bossFlags[index]))
                .ToArray()))
            .Cast<Requirement>()
            .ToArray();

        return alternatives.Length == 1
            ? alternatives[0]
            : new Requirement.Or(alternatives);
    }

    private void PatchVanillaMapPreopenedDoors()
    {
        if (_world.Map != null)
            return;

        (string Room, string Door)[] preopenedDoors =
        [
            ("Red Brinstar Elevator Room", "Top Door"),
            ("Business Center", "Middle Left Door"),
            ("Construction Zone", "Right Door"),
        ];

        foreach (var (roomName, doorName) in preopenedDoors)
        {
            var door = _reader.Rooms.First(r => r.Name == roomName).Nodes.First(n => n.Name == doorName);

            // Keycard mode may already have replaced one of these doors; preserve that
            // replacement rather than turning it into a free blue door.
            if (door.NodeSubType.StartsWith("keycard:"))
                continue;

            door.NodeSubType = "blue";
            door.Locks = null;
        }
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

            // Entrance conditions describe transient state at a door. Keep the
            // ordinary node as the normal state and create a private state vertex
            // for each supported conditioned strat. A vertex is per-strat rather
            // than merely per condition type so a weak runway cannot unlock a
            // second strat with stricter speed constraints.
            foreach (var strat in room.Strats.Where(s =>
                         s.Link is { Length: >= 2 } &&
                         IsSupportedRunwayEntrance(s.EntranceCondition)))
            {
                var node = room.Nodes.First(n => n.Id == strat.Link![0]);
                var logicalName = $"{room.Area} - {room.Name} - {node.Name}";
                var entranceVertex = new Vertex
                {
                    Name = $"{logicalName} [entrance-state:{++_entranceVertexId}]",
                    LogicalName = logicalName,
                    Type = VertexType.Meta,
                    World = _world,
                    RoomId = room.Id,
                    NodeId = node.Id,
                    Node = node,
                };
                _graph.AddVertex(entranceVertex);
                _entranceVertices.Add(strat, entranceVertex);

                // Samus may discard the incoming momentum and continue from the
                // door normally. This is deliberately a plain graph edge so it
                // does not add a synthetic strategy to the spoiler path.
                var normalVertex = _graph.Vertices.First(v =>
                    v.RoomId == room.Id && v.NodeId == node.Id && v.LogicalName == null);
                _graph.AddUnconditional(entranceVertex, normalVertex,
                    new ItemCondition(_world.GetItem("fixed"), 1));
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

                    // A supported entrance-conditioned strat can only begin at
                    // the state vertex reached by matching it across the door.
                    foreach (var strat in room.Strats.Where(s =>
                                 s.Link![0] == link.From && s.Link![1] == linkTo.Id &&
                                 IsSupportedRunwayEntrance(s.EntranceCondition) &&
                                 (s.ExitCondition == null ||
                                  s.ExitCondition is ExitCondition.LeaveNormally ||
                                  s.ExitCondition is ExitCondition.LeaveWithRunway)))
                    {
                        strat.Requires = OptimizeRequirement(strat.Requires);
                        _graph.AddDirected(_entranceVertices[strat], toVtx, [strat]);
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
        // The ROM writer still needs this edge as shuffled-connection metadata,
        // even though map-rando ROMs permanently block it in gameplay.
        bool blockedMotherBrainConnection = _world.Map != null
            && (fromRoom.Name == "Mother Brain Room" && fromNode.Name == "Left Blast Door"
                || toRoom.Name == "Mother Brain Room" && toNode.Name == "Left Blast Door");

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
                // Inject door and goal requirements into the traversal out of this node.
                var newFromStrat = (Strat)strat.Clone();
                var requirements = new List<Requirement> { strat.Requires };
                if (strat.ExitCondition is ExitCondition.LeaveWithRunway runway)
                    requirements.Add(RunwayEntranceRequirement(
                        runway, targetStrat.EntranceCondition!, fromRoom, fromNode));
                if (unlockReq != null)
                    requirements.Add(unlockReq);
                if (blockedMotherBrainConnection)
                    requirements.Add(new Requirement.Never());
                // Map rando pre-opens G4 and moves the configurable boss-count gate to
                // the grey door installed on the shuffled connection into Mother Brain.
                if (_world.Map != null
                    && toRoom.Name == "Mother Brain Room"
                    && toNode.Name == "Right Door")
                {
                    requirements.Add(BossRequirement(int.Parse(_world.Config.Bosses)));
                }
                newFromStrat.Requires = OptimizeRequirement(
                    new Requirement.And(requirements.ToArray()));

                var fromStratVtx = _entranceVertices.GetValueOrDefault(strat) ?? fromVtx;
                var targetStratVtx = _entranceVertices.GetValueOrDefault(targetStrat) ?? toVtx;

                _graph.AddDirected(fromStratVtx, targetStratVtx, [newFromStrat]);
            }
        }
    }

    private static bool IsSupportedRunwayEntrance(EntranceCondition? condition) =>
        condition is EntranceCondition.ComeInRunning or
            EntranceCondition.ComeInJumping or
            EntranceCondition.ComeInSpinning;

    private Requirement RunwayEntranceRequirement(ExitCondition.LeaveWithRunway runway,
        EntranceCondition entrance, Room fromRoom, Node fromNode)
    {
        var requirements = new List<Requirement>();
        decimal effectiveLength = EffectiveRunwayLength(runway);
        string speedBooster;

        switch (entrance)
        {
            case EntranceCondition.ComeInNormally:
                return new Requirement.Always();

            case EntranceCondition.ComeInRunning running:
                if (effectiveLength < running.MinTiles ||
                    running.MaxTiles is { } runningMax && runningMax < running.MinTiles)
                    return new Requirement.Never();
                speedBooster = running.SpeedBooster;
                break;

            case EntranceCondition.ComeInJumping jumping:
                if (effectiveLength < jumping.MinTiles ||
                    jumping.MaxTiles is { } jumpingMax && jumpingMax < jumping.MinTiles)
                    return new Requirement.Never();
                speedBooster = jumping.SpeedBooster;
                break;

            case EntranceCondition.ComeInSpinning spinning:
                decimal usableLength = effectiveLength - spinning.UnusableTiles;
                int maximumSpeed = MaximumExtraRunSpeed(usableLength);
                speedBooster = spinning.SpeedBooster;
                if (speedBooster.Equals("false", StringComparison.OrdinalIgnoreCase))
                    maximumSpeed = Math.Min(maximumSpeed, 0x20);

                int minimum = ParseExtraRunSpeed(spinning.MinExtraRunSpeed) ?? 0;
                int maximum = ParseExtraRunSpeed(spinning.MaxExtraRunSpeed) ?? int.MaxValue;
                if (minimum > maximum || minimum > maximumSpeed)
                    return new Requirement.Never();
                if (!speedBooster.Equals("false", StringComparison.OrdinalIgnoreCase) &&
                    minimum > 0x20)
                    requirements.Add(new Requirement.Single("SpeedBooster"));
                break;

            default:
                return new Requirement.Never();
        }

        if (speedBooster.Equals("true", StringComparison.OrdinalIgnoreCase))
            requirements.Add(new Requirement.Single("SpeedBooster"));
        else if (speedBooster.Equals("false", StringComparison.OrdinalIgnoreCase))
            requirements.Add(new Requirement.Single("canDisableEquipment"));

        if (fromNode.DoorEnvironments?.Any(environment =>
                environment.Physics.Equals("water", StringComparison.OrdinalIgnoreCase)) == true)
            requirements.Add(new Requirement.Single("Gravity"));

        // Implicit heat cost depends on runway use and whether the exit strat
        // starts at this door. Until that calculation is modeled, require Varia
        // so heated runway use remains possible without granting a free hellrun.
        if (fromRoom.RoomEnvironments?.Any(environment => environment.Heated) == true)
            requirements.Add(new Requirement.Single("Varia"));

        return requirements.Count switch
        {
            0 => new Requirement.Always(),
            1 => requirements[0],
            _ => new Requirement.And(requirements.ToArray()),
        };
    }

    private static decimal EffectiveRunwayLength(ExitCondition.LeaveWithRunway runway) =>
        runway.Length - (runway.StartingDownTiles ?? 0) -
        9m / 16m * (1 - runway.OpenEnd) +
        1m / 3m * (runway.SteepUpTiles ?? 0) +
        1m / 7m * (runway.SteepDownTiles ?? 0) +
        5m / 27m * (runway.GentleUpTiles ?? 0) +
        5m / 59m * (runway.GentleDownTiles ?? 0);

    private static int? ParseExtraRunSpeed(string? value)
    {
        if (value == null)
            return null;
        string hexadecimal = value.TrimStart('$').Replace(".", "");
        return int.Parse(hexadecimal, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
    }

    private static int MaximumExtraRunSpeed(decimal runwayLength)
    {
        int[] speeds =
        [
            0x00, 0x0A, 0x0E, 0x12, 0x16, 0x1A, 0x1E, 0x21, 0x24, 0x27,
            0x2A, 0x2D, 0x30, 0x33, 0x35, 0x38, 0x3A, 0x3D, 0x3F, 0x42,
            0x44, 0x46, 0x48, 0x4A, 0x4D, 0x4F, 0x51, 0x53, 0x55, 0x57,
            0x59, 0x5B, 0x5C, 0x5E, 0x60, 0x62, 0x64, 0x65, 0x67, 0x69,
            0x6B, 0x6C, 0x6E, 0x70,
        ];
        int tiles = Math.Clamp(decimal.ToInt32(decimal.Floor(runwayLength)), 0, 43);
        return speeds[tiles];
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
    public List<BaseEdge> Edges { get; } = new();
    public Dictionary<Vertex, List<BaseEdge>> AdjecencyList { get; } = new();

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

    public void AddUnconditional(Vertex from, Vertex to, ItemCondition condition)
    {
        var edge = new BaseEdge(from, to, condition);
        Edges.Add(edge);
        AdjecencyList[from].Add(edge);
    }

    public IEnumerable<BaseEdge> GetEdges(Vertex vertex)
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
