namespace Randomizer.Graph.Combo.SuperMetroid;

using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Text.Json;
using Combo.SuperMetroid.Model;
using static global::Randomizer.Graph.Combo.Zelda.ZeldaYamlReader;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security;

internal class SMJsonReader
{
    public List<Room> Rooms { get; set; } = new();
    public List<ConnectionCollection> Connections { get; set; } = new();
    public TechCollection Techs { get; set; } = new TechCollection([]);
    public HelperCollection Helpers { get; set; } = new HelperCollection([]);
    public List<EnemyCollection> Enemies { get; set; } = new();
    public List<BossScenarioCollection> BossScenarios { get; set; } = new();

    private Dictionary<string, Dictionary<string, object>> vertices = new Dictionary<string, Dictionary<string, object>>();
    private Dictionary<Requirement, DirectedUndirectedPair> edges = new Dictionary<Requirement, DirectedUndirectedPair>();

    private HashSet<(string, string, Requirement)> _edgeExists = new HashSet<(string, string, Requirement)>();

    private Dictionary<string, object> CreateNode(Dictionary<string, object> nodeData)
    {
        vertices.Add((string)nodeData["name"], nodeData);
        return nodeData;
    }

    private Dictionary<string, object>? FindNode(string name)
    {
        if (vertices.TryGetValue(name, out var vertexData))
        {
            return vertexData;
        }

        return null;
    }

    private Dictionary<string, object> FindOrCreateNode(string name, string? item = null)
    {
        if (!vertices.TryGetValue(name, out var vertexData))
        {
            vertexData = new Dictionary<string, object>
            {
                { "name", name },
                { "type", VertexType.Meta },
                { "item", item! },
            };
            vertices.Add(name, vertexData);
        }

        return vertexData;
    }

    private bool EdgeExists(Dictionary<string, object> from, Dictionary<string, object> to, Requirement requirement)
    {
        return _edgeExists.Contains(((string)from["name"], (string)to["name"], requirement));
    }

    private void AddEdge(Dictionary<string, object> from, Dictionary<string, object> to, Requirement requirement, bool undirected = false)
    {
        if (!edges.TryGetValue(requirement, out var edgePair))
        {
            edgePair = new DirectedUndirectedPair();
            edges.Add(requirement, edgePair);
        }

        // check if the edge already exists
        var edgeExists = _edgeExists.Contains(((string)from["name"], (string)to["name"], requirement));

        if (!edgeExists)
        {
            if (undirected)
            {
                edgePair.Undirected.Add([(string)from["name"], (string)to["name"]]);
            }
            else
            {
                edgePair.Directed.Add([(string)from["name"], (string)to["name"]]);
            }

            _edgeExists.Add(((string)from["name"], (string)to["name"], requirement));
        }
    }

    private void AddDirectedEdge(Dictionary<string, object> from, Dictionary<string, object> to, Requirement requirement)
    {
        AddEdge(from, to, requirement);
    }

    private void AddUndirectedEdge(Dictionary<string, object> from, Dictionary<string, object> to, Requirement requirement)
    {
        AddEdge(from, to, requirement, true);
    }

    private List<T> LoadFiles<T>(string path)
    {
        var jsonData = new List<T>();
        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();

        foreach (var fileName in Directory.GetFiles(path, "*.json", SearchOption.AllDirectories))
        {
            if (fileName.Contains("roomDiagram"))
            {
                continue;
            }

            var fileContents = File.ReadAllText(fileName);
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                var data = JsonSerializer.Deserialize<T>(fileContents, options);
                if (data != null)
                {
                    jsonData.Add(data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while deserializing: {fileName}, {e.Message} ({e.InnerException?.Source ?? ""}");
            }
        }

        return jsonData;
    }

    private T? LoadFile<T>(string path)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var data = JsonSerializer.Deserialize<T>(File.ReadAllText(path), options);
            return data ?? default;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while deserializing: {path}, {e.Message} ({e.InnerException?.Source ?? ""}");
            return default;
        }
    }

    public Dictionary<string, DirectedUndirectedPair> GetForWorld(World world)
    {
        if (Rooms.Count == 0)
        {
            Load();
        }

        return edges.Select(e => { e.Value.Directed = e.Value.Directed.Select(d => { d[0] = $"SM - {d[0]}"; d[1] = $"SM - {d[1]}"; return d; }).ToList(); e.Value.Undirected = e.Value.Undirected.Select(d => { d[0] = $"SM - {d[0]}"; d[1] = $"SM - {d[1]}"; return d; }).ToList(); return e; }).ToDictionary(e => $"{e.Key}", e => e.Value);
    }

    public List<Dictionary<string, object>> LoadYmlData(World world)
    {
        if (Rooms.Count == 0)
        {
            Load();
        }

        return vertices.Values.Select(v => { v["name"] = $"SM - {v["name"]}"; return v; }).ToList();
    }

    // Load all JSON data into memory
    public void Load()
    {
        var path = Path.Combine(YamlReader.DataRoot, "../Combo/Data/SuperMetroid/sm-json-data/");
        Rooms = LoadFiles<Room>(Path.Combine(path, "region"));
        Connections = LoadFiles<ConnectionCollection>(Path.Combine(path, "connection"));
        Techs = LoadFile<TechCollection>(Path.Combine(path, "tech.json")) ?? new TechCollection([]);
        Helpers = LoadFile<HelperCollection>(Path.Combine(path, "helpers.json")) ?? new HelperCollection([]);
        Enemies = LoadFiles<EnemyCollection>(Path.Combine(path, "enemies")).Where(x => x.Enemies != null).ToList();
        BossScenarios = LoadFiles<BossScenarioCollection>(Path.Combine(path, "enemies")).Where(x => x.Scenarios != null).ToList();
    }

    public void BuildGraph(World world)
    {
        foreach(var room in Rooms)
        {
            BuildRoom(room);
        }   
    }

    private void BuildRoom(Room room)
    {
        // For each obstacle in the room, created a copy of a node for that obstacle state (including a blank state)
        var obstacleCombinations = room.Obstacles?.Combinations().ToList() ?? [];
        string[] obstacleIdStrings = ["", ..obstacleCombinations.Select(c => string.Join(",", c.Select(o => o.Id).OrderBy(c => c))).OrderBy(c => c).ToList()];

        foreach (var obstacleIdString in obstacleIdStrings)
        {
            // Create this rooms nodes
            foreach (var node in room.Nodes)
            {
                var nodeName = obstacleIdString == "" ? $"{room.Area} - {room.Name} - {node.Name}" : $"{room.Area} - {room.Name} - {node.Name} - {obstacleIdString}";

                var nodeType = node.NodeType switch
                {
                    // "door", "entrance", "exit", "event", "item", "junction", "utility"
                    "item" => VertexType.Item,
                    "entrance" => VertexType.Entrance,
                    "exit" => VertexType.Entrance,
                    "door" => VertexType.Entrance,
                    _ => VertexType.Meta
                };

                VertexType? nodeSubType = node.NodeSubType switch
                {
                    "chozo" => VertexType.Chozo,
                    "hidden" => VertexType.Hidden,
                    "visible" => VertexType.Visible,
                    _ => null
                };

                var newNode = CreateNode(new()
                {
                    { "name", nodeName },
                    { "type", VertexType.Meta },
                    { "subtype", VertexType.Meta },
                });

                // If this node is an item, create a new item node that is common for all obstacle states
                if(nodeType == VertexType.Item)
                {
                    var itemNodeName = $"{room.Area} - {room.Name} - {node.Name} - Item";
                    if (FindNode(itemNodeName) == null)
                    {
                        var itemNode = CreateNode(new()
                        {
                            { "name", itemNodeName },
                            { "type", VertexType.Item },
                            { "subtype", nodeSubType! },
                            { "address", Convert.ToInt32(node.NodeAddress ?? "0", 16) },
                            { "itemset", (string[])["supermetroid"] },
                        });

                        AddDirectedEdge(newNode, itemNode, node.InteractionRequires ?? new Requirement.Always());
                    }
                }
            }
        }

        foreach (var node in room.Nodes)
        {
            ConnectNode(room, node, "");
        }

    }

    // This will recursively connect all the nodes in the room, switching obstacle states as needed
    public void ConnectNode(Room room, Node from, string obstacleState)
    {
        var fromNodeName = obstacleState == "" ? $"{room.Area} - {room.Name} - {from.Name}" : $"{room.Area} - {room.Name} - {from.Name} - {obstacleState}";
        var fromNodeData = FindNode(fromNodeName)!;

        // Get all the nodes that are connected from this one (all edges are directed)
        var links = room.Links.Where(l => l.From == from.Id).ToList();

        foreach (var link in links)
        {
            foreach (var linkTo in link.To)
            {
                var toNode = room.Nodes.Where(n => n.Id == linkTo.Id).FirstOrDefault()!;
                var linkStrats = room.Strats.Where(s => s.Link[0] == link.From && s.Link[1] == linkTo.Id).ToList();

                foreach (var strat in linkStrats)
                {
                    // Figure out the target obstacle state after executing this strat
                    string newObstacleState = obstacleState;
                    
                    if (strat.ClearsObstacles != null)
                    {
                        string[] currentObstacles = obstacleState.Split(",");
                        newObstacleState = string.Join(",", currentObstacles.Union(strat.ClearsObstacles).OrderBy(c => c)).Trim(',');
                    }

                    if (strat.ResetsObstacles != null)
                    {
                        string[] currentObstacles = newObstacleState.Split(",");
                        newObstacleState = string.Join(",", currentObstacles.Except(strat.ResetsObstacles).OrderBy(c => c)).Trim(',');
                    }

                    var stratNodeName = $"{fromNodeName} - {string.Join(",", strat.Link)} - Strat: {strat.Name}";
                    var stratNode = FindOrCreateNode(stratNodeName);

                    var toNodeName = newObstacleState == "" ? $"{room.Area} - {room.Name} - {toNode.Name}" : $"{room.Area} - {room.Name} - {toNode.Name} - {newObstacleState}";
                    var toNodeData = FindNode(toNodeName)!;

                    var requirement = strat.Requires == new Requirement.And([]) ? new Requirement.Always() : strat.Requires;

                    if (!EdgeExists(fromNodeData, stratNode, requirement))
                    {
                        AddDirectedEdge(fromNodeData, stratNode, requirement);
                        AddDirectedEdge(stratNode, toNodeData, new Requirement.Always());
                        //Console.WriteLine($"Added edge from {fromNodeName} to {stratNodeName} with requirement {requirement}");
                        ConnectNode(room, toNode, newObstacleState);
                    }

                }
            }
        }
    }
}
