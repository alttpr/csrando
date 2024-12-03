namespace Randomizer.Games.SuperMetroid;

using Randomizer.Graph;
using Randomizer.Games.SuperMetroid.Model;

internal static class DataLoader
{
    public static Vertex Fill(World world)
    {
        var graph = world.Graph;
        var jsonReader = new JsonReader(world.Config);

        // Load the yaml data from files into memory
        jsonReader.Load();

        // Create vertices
        BuildGraph(jsonReader, world);


        // Create a starting vertex for the player to start at
        var startingVertex = new Vertex()
        {
            World = world,
            Name = "start",
            Type = VertexType.Meta,
        };

        graph.AddVertex(startingVertex);

        //// Connect the starting vertex to the starting location and meta location

        world.Graph.AddDirected(startingVertex, world.GetLocation("Crateria - Landing Site - Ship"), world.GetItem("fixed"));

        //world.Graph.AddDirected(startingVertex, world.GetLocation("Brinstar - Morph Room - Spawn Platform (2) - Spawn Platform"), world.GetItem("fixed"));
        //world.Graph.AddDirected(startingVertex, world.GetLocation("Meta - Metroid Meta Locations - Meta (0) - Meta"), world.GetItem("fixed"));

        //world.YamlData = yamlReader.Data!;
        
        world.JsonData = jsonReader;
        return startingVertex;
    }

    private static void BuildGraph(JsonReader jsonReader, World world)
    {
        foreach (var room in jsonReader.Rooms)
        {
            var roomName = $"{room.Area} - {room.Name}";

            // Create vertices
            foreach (var node in room.Nodes)
            {
                var nodeName = $"{roomName} - {node.Name}";
                var nodeType = node.NodeType switch
                {
                    // "door", "entrance", "exit", "event", "item", "junction", "utility"
                    "item" => VertexType.Item,
                    "entrance" => VertexType.Entrance,
                    "exit" => VertexType.Entrance,
                    "door" => VertexType.Entrance,
                    _ => VertexType.Meta
                };

                var vertex = new Vertex()
                {
                    World = world,
                    Name = nodeName,
                    Type = nodeType,
                    Node = node,
                    RoomId = room.Id,
                };

                world.Graph.AddVertex(vertex);
            }

            // Create edges within room
            foreach (var link in room.Links)
            {
                var fromNode = room.Nodes.Where(n => n.Id == link.From).First();
                var fromVtx = world.GetLocation($"{roomName} - {fromNode.Name}");
                foreach (var to in link.To)
                {
                    var toNode = room.Nodes.Where(n => n.Id == to.Id).First();
                    var toVtx = world.GetLocation($"{roomName} - {toNode.Name}");

                    foreach(var strat in room.Strats.Where(s => s.Link![0] == link.From && s.Link[1] == to.Id))
                    {
                        //var stratEdge = new Edge((Vertex)fromVtx, (Vertex)toVtx, new ItemCondition(world.GetItem("fixed"), 0), strat);
                        //fromVtx.Edges.Add(stratEdge);
                    }
                }
            }
        }

        // Create edges between rooms
        foreach(var connection in jsonReader.Connections.SelectMany(c => c.Connections))
        {
            var fromRoom = jsonReader.Rooms.Where(r => r.Id == connection.Nodes[0].RoomId).First();
            var fromNode = fromRoom.Nodes.Where(n => n.Id == connection.Nodes[0].NodeId).First();
            var fromVtx = world.GetLocation($"{fromRoom.Area} - {fromRoom.Name} - {fromNode.Name}");

            var toRoom = jsonReader.Rooms.Where(r => r.Id == connection.Nodes[1].RoomId).First();
            var toNode = toRoom.Nodes.Where(n => n.Id == connection.Nodes[1].NodeId).First();
            var toVtx = world.GetLocation($"{toRoom.Area} - {toRoom.Name} - {toNode.Name}");

            var firstEdge = new Edge((Vertex)fromVtx, (Vertex)toVtx, new ItemCondition(world.GetItem("fixed"), 0));
            fromVtx.Edges.Add(firstEdge);

            // TODO: Do we handle door colors here as a requirement by checking nodeSubType?

            if (connection.Direction.ToLower() == "bidirectional")
            {
                var secondEdge = new Edge((Vertex)toVtx, (Vertex)fromVtx, new ItemCondition(world.GetItem("fixed"), 0));
                toVtx.Edges.Add(secondEdge);
            }
        }
    }

}
