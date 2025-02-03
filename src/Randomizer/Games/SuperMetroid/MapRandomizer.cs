namespace Randomizer.Games.SuperMetroid;

using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class MapRandomizer
{
    JsonReader _reader;
    World _world;
    PRNG _prng;

    public MapRandomizer(JsonReader reader, World world, PRNG prng)
    {
        _world = world;
        _reader = reader;
        _prng = prng;
    }

    public void Randomize()
    {
        var map = LoadMap();
        if (map == null)
        {
            Console.WriteLine("Failed to load map");
            return;
        }

        var connections = CreateConnections(map);

        (string,string)[] keepDoors = [
            ("Bomb Torizo Room", "Left Door"),
        ];


        // Patch out all colored and gray doors
        foreach (var room in _reader.Rooms)
        {
            foreach (var node in room.Nodes)
            {
                if(keepDoors.Contains((room.Name, node.Name)))
                {
                    continue;
                }

                if (node.NodeType == "door" && node.NodeSubType != "blue" && node.NodeSubType != "elevator")
                {
                    node.NodeSubType = "blue";
                    node.Locks = null;
                }
            }
        }

        _reader.Connections = [new ConnectionCollection(connections.ToArray())];
        _world.Map = map;
    }

    private IEnumerable<Connection> CreateConnections(Map map)
    {
        var connections = new List<Connection>();
        foreach (var door in map.doors)
        {
            var fromRoom = _reader.Rooms.Find(r => r.Nodes.Any(n => int.Parse(n.NodeAddress?.Substring(2) ?? "0", System.Globalization.NumberStyles.HexNumber) == door.from.exit_ptr));
            var toRoom = _reader.Rooms.Find(r => r.Nodes.Any(n => int.Parse(n.NodeAddress?.Substring(2) ?? "0", System.Globalization.NumberStyles.HexNumber) == door.to.exit_ptr));

            if (fromRoom == null || toRoom == null)
            {
                // Check if sandfall
                if (door.bidirectional == false)
                {
                    // Try to match with room geometry data
                    var toRoomGeometry = _reader.RoomGeometries.Find(r => r.doors.Any(d => d.entrance_ptr == door.to.entrance_ptr));
                    if (toRoomGeometry != null)
                    {
                        toRoom = _reader.Rooms.Find(r => r.Name == toRoomGeometry.name);
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to find room for door {door.from} -> {door.to}");
                    continue;
                }
            }

            var fromNode = fromRoom.Nodes.Where(n => int.Parse(n.NodeAddress?.Substring(2) ?? "0", System.Globalization.NumberStyles.HexNumber) == door.from.exit_ptr).FirstOrDefault();
            var toNode = toRoom.Nodes.Where(n => int.Parse(n.NodeAddress?.Substring(2) ?? "0", System.Globalization.NumberStyles.HexNumber) == door.to.exit_ptr).FirstOrDefault();

            if (toNode == null && door.bidirectional == false)
            {
                // Try to get the first sandfall entrance
                toNode = toRoom.Nodes.Where(n => n.NodeType == "entrance" && n.NodeSubType == "sandpit").FirstOrDefault();
            }

            var fromConnection = new Connection
            (
                fromNode?.DoorOrientation ?? "" switch
                {
                    "left" => "HorizontalDoor",
                    "right" => "HorizontalDoor",
                    "up" => "VerticalDoor",
                    "down" => "VerticalDoor",
                    string direction => throw new Exception($"Unknown direction {direction}")
                },
                door.bidirectional ? "Bidirectional" : "Forward",
                $"From {fromRoom.Name}:{fromNode.Name} to {toRoom.Name}:{toNode.Name}",
                [
                    new(
                        fromRoom.Area,
                        fromRoom.SubArea,
                        fromRoom.Id,
                        fromRoom.Name,
                        fromNode.Id,
                        fromNode.Name,
                        fromNode?.DoorOrientation ?? "" switch
                        {
                            "left" => "left",
                            "right" => "right",
                            "up" => "top",
                            "down" => "bottom",
                            string direction => throw new Exception($"Unknown direction {direction}")
                        },
                        null,
                        null
                    ),
                    new(
                        toRoom.Area,
                        toRoom.SubArea,
                        toRoom.Id,
                        toRoom.Name,
                        toNode.Id,
                        toNode.Name,
                        fromNode?.DoorOrientation ?? "" switch
                        {
                            "left" => "left",
                            "right" => "right",
                            "up" => "top",
                            "down" => "bottom",
                            string direction => throw new Exception($"Unknown direction {direction}")
                        },
                        null,
                        null
                    ),
                ],
                null,
                null
            );

            connections.Add(fromConnection);


            Console.WriteLine($"Connecting {fromRoom.Name}:{fromNode.Name} to {toRoom.Name}:{toNode.Name}");
        }

        return connections;
    }

    private Map? LoadMap()
    {
        var path = Path.Combine(JsonReader.DataRoot, "maps");
        var allMaps = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories).Order();

        // Pick a map at random using the PRNG
        var mapPath = _prng.GetRandomElement(allMaps);

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var data = JsonSerializer.Deserialize<Map>(File.ReadAllText(mapPath), options);
            return data ?? default;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while deserializing: {path}, {e.Message} ({e.InnerException?.Source ?? ""}");
            return default;
        }
    }

}


