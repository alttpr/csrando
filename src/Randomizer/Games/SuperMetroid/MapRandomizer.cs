namespace Randomizer.Games.SuperMetroid;

using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;
using SolTechnology.Avro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Randomizer.Games.Metroid.YamlReader;

public class MapRandomizer
{
    private readonly JsonReader _reader;
    private readonly World _world;
    private readonly PRNG _prng;

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

        (string, string)[] keepDoors = [
            ("Bomb Torizo Room", "Left Door"),
        ];


        // Patch out all colored and gray doors
        foreach (var room in _reader.Rooms)
        {
            foreach (var node in room.Nodes)
            {
                if (keepDoors.Contains((room.Name, node.Name)))
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
        for(int i = 0; i < map.conn_from_door_id.Count; i++)
        {
            var bidirectional = map.conn_bidirectional[i];
            var fromRoomId = map.conn_from_room_id[i];
            var toRoomId = map.conn_to_room_id[i];

            var fromRoom = _reader.Rooms.Find(r => r.Id == fromRoomId);
            var toRoom = _reader.Rooms.Find(r => r.Id == toRoomId);

            var fromDoorId = map.conn_from_door_id[i];
            var toDoorId = map.conn_to_door_id[i];

            var fromGeo = _reader.RoomGeometries.Where(r => r.room_id == fromRoomId).First();
            var toGeo = _reader.RoomGeometries.Where(r => r.room_id == toRoomId).First();

            var fromGeoDoor = fromGeo.doors[fromDoorId];
            var toGeoDoor = toGeo.doors[toDoorId];

            if (fromRoom == null || toRoom == null)
            {
                // Check if sandfall
                if (bidirectional == false)
                {
                    // Try to match with room geometry data
                    var toRoomGeometry = _reader.RoomGeometries.Find(r => r.doors.Any(d => d.entrance_ptr == toGeoDoor.entrance_ptr));
                    if (toRoomGeometry != null)
                    {
                        toRoom = _reader.Rooms.Find(r => r.Name == toRoomGeometry.name);
                    }
                }
            }

            if (fromRoom == null || toRoom == null)
            {
                Console.WriteLine($"Failed to find room for door {fromGeoDoor} -> {toGeoDoor}");
                continue;
            }

            if (!fromGeoDoor.exit_ptr.HasValue || !toGeoDoor.exit_ptr.HasValue)
            {
                Console.WriteLine($"Skipping door with missing exit pointers: {fromGeoDoor} -> {toGeoDoor}");
                continue;
            }

            var fromExitPtr = fromGeoDoor.exit_ptr.Value;
            var toExitPtr = toGeoDoor.exit_ptr.Value;

            var fromNode = fromRoom.Nodes.Where(n => int.Parse(n.NodeAddress?.Substring(2) ?? "0", System.Globalization.NumberStyles.HexNumber) == fromExitPtr).FirstOrDefault();
            var toNode = toRoom.Nodes.Where(n => int.Parse(n.NodeAddress?.Substring(2) ?? "0", System.Globalization.NumberStyles.HexNumber) == toExitPtr).FirstOrDefault();

            if (toNode == null && bidirectional == false)
            {
                // Try to get the first sandfall entrance
                toNode = toRoom.Nodes.Where(n => n.NodeType == "entrance" && n.NodeSubType == "sandpit").FirstOrDefault();
            }

            if (fromNode is null || toNode is null)
            {
                Console.WriteLine($"Failed to resolve nodes for door {fromGeoDoor} -> {toGeoDoor}");
                continue;
            }

            string fromOrientation = fromNode.DoorOrientation ?? string.Empty;
            string toOrientation = toNode.DoorOrientation ?? string.Empty;

            static string MapDoorType(string orientation) => orientation switch
            {
                "left" or "right" => "HorizontalDoor",
                "up" or "down" => "VerticalDoor",
                string direction => throw new Exception($"Unknown direction {direction}")
            };

            static string MapNodeOrientation(string orientation) => orientation switch
            {
                "left" => "left",
                "right" => "right",
                "up" => "top",
                "down" => "bottom",
                string direction => throw new Exception($"Unknown direction {direction}")
            };

            var fromConnection = new Connection
            (
                MapDoorType(fromOrientation),
                bidirectional ? "Bidirectional" : "Forward",
                $"From {fromRoom.Name}:{fromNode.Name} to {toRoom.Name}:{toNode.Name}",
                [
                    new(
                        fromRoom.Area,
                        fromRoom.SubArea,
                        fromRoom.Id,
                        fromRoom.Name,
                        fromNode.Id,
                        fromNode.Name,
                        MapNodeOrientation(fromOrientation),
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
                        MapNodeOrientation(toOrientation),
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
        var allMaps = Directory.GetFiles(path, "*.avro", SearchOption.AllDirectories).Order();
        var avroFile = _prng.GetRandomElement(allMaps);
        var maps = AvroConvert.Deserialize<IEnumerable<Map>>(File.ReadAllBytes(avroFile));
        var map = _prng.GetRandomElement(maps);
        return map;

    }

}
