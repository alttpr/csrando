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

internal class SMJsonReader
{
    public List<Room> Rooms { get; set; } = new();
    public List<ConnectionCollection> Connections { get; set; } = new();
    public TechCollection Techs { get; set; } = new TechCollection([]);
    public HelperCollection Helpers { get; set; } = new HelperCollection([]);
    public List<EnemyCollection> Enemies { get; set; } = new();
    public List<BossScenarioCollection> BossScenarios { get; set; } = new();

    private Dictionary<string, Dictionary<string, object>> vertices = new Dictionary<string, Dictionary<string, object>>();
    private Dictionary<string, DirectedUndirectedPair> edges = new Dictionary<string, DirectedUndirectedPair>();

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

    private void AddEdge(Dictionary<string, object> from, Dictionary<string, object> to, string edgeGroup, bool undirected = false)
    {
        if (!edges.TryGetValue(edgeGroup, out var edgePair))
        {
            edgePair = new DirectedUndirectedPair();
            edges.Add(edgeGroup, edgePair);
        }

        // Check if the edge already exists
        var edgeExists = undirected ? edgePair.Undirected.Any(e => e.SequenceEqual(new string[] { (string)from["name"], (string)to["name"] })) :
                                      edgePair.Directed.Any(e => e.SequenceEqual(new string[] { (string)from["name"], (string)to["name"] }));

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
        }
    }

    private void AddDirectedEdge(Dictionary<string, object> from, Dictionary<string, object> to, string edgeGroup)
    {
        AddEdge(from, to, edgeGroup);
    }

    private void AddUndirectedEdge(Dictionary<string, object> from, Dictionary<string, object> to, string edgeGroup)
    {
        AddEdge(from, to, edgeGroup, true);
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

    public void BuildGraph()
    {
        BuildRoom(Rooms[0]);
    }

    private void BuildRoom(Room room)
    {
        // Create this rooms nodes
        foreach (var node in room.Nodes)
        {
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
                { "name", $"{room.Area} - {room.Name} - {node.Name}" },
                { "type", nodeType },
                { "subtype", nodeSubType! },
                { "address", Convert.ToInt32(node.NodeAddress ?? "0", 16) },
                { "itemset", (string[])["supermetroid"] },
            });
        }
    }
}
