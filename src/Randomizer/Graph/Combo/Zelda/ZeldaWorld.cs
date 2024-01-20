namespace Randomizer.Graph.Combo.Zelda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using ItemSet = Dictionary<ItemSetName, /* WeightedSet */ Dictionary<int, List<Item>>>;
using WeightedSet = Dictionary<int, List<Item>>;

internal class ZeldaWorld
{
    // Adjusts the world as needed to randomize Metroid
    public static void AdjustWorld(World world)
    {
        // Load the Metroid Yaml Data and hook up the world to the current world graph
        var yamlReader = new ZeldaYamlReader();
        var zeldaVertices = yamlReader.LoadYmlData(world);

        foreach (var vtx in zeldaVertices)
        {
            var name = vtx.TryGetValue("name", out object? nameValue) ? (string)nameValue : throw new InvalidDataException("Zelda vertex without a name");
            var type = vtx.TryGetValue("type", out object? typeValue) ? (VertexType)typeValue : VertexType.Meta;
            var item = vtx.TryGetValue("item", out object? itemValue) ? (string)itemValue : null;
            var itemset = vtx.TryGetValue("itemset", out object? itemsetValue) ? (string[])itemsetValue : null;

            var vertex = new Vertex()
            {
                World = world,
                Name = name,
                Type = type,
                Item = item != null ? world.GetItem("Z1" + item) : null,
                ItemSet = itemset?.Select(i => new ItemSetName(i, world)).ToArray() ?? []
            };

            world.Graph.AddVertex(vertex);
            //Console.WriteLine($"Added vertex {vertex.Name}");
        }

        var zeldaEdges = yamlReader.GetForWorld(world);
        foreach (var edgeCollection in zeldaEdges)
        {
            var edgeCollectionData = edgeCollection.Key.Split(":").First().Split('|');
            var requirementName = edgeCollectionData.First();
            if(!requirementName.StartsWith("fixed"))
            {
                requirementName = "Z1" + requirementName;
            }
            var requirement = world.GetItem(requirementName);
            var requirementCount = int.Parse(edgeCollectionData.Skip(1).FirstOrDefault() ?? "1");

            foreach (var edges in edgeCollection.Value.Directed)
            {
                var from = world.GetLocation(edges[0]);
                var to = world.GetLocation(edges[1]);
                if (from is null || to is null)
                {
                    throw new Exception("Name Connection Mismatch: " + $"({edges[0]}, {edges[1]}) => " + $"({from}, {to})");
                }

                world.Graph.AddDirected(from, to, requirement, requirementCount);
                //Console.WriteLine($"Added directed edge from {from.Name} to {to.Name}");
            }

            foreach (var edges in edgeCollection.Value.Undirected)
            {
                var from = world.GetLocation(edges[0]);
                var to = world.GetLocation(edges[1]);
                if (from is null || to is null)
                {
                    throw new Exception("Name Connection Mismatch: " + $"({edges[0]}, {edges[1]}) => " + $"({from}, {to})");
                }

                world.Graph.AddDirected(from, to, requirement, requirementCount);
                world.Graph.AddDirected(to, from, requirement, requirementCount);
                //Console.WriteLine($"Added undirected edge from {from.Name} to {to.Name}");
            }
        }

        // Connect the start edge to the start edge of the Metroid graph
        // This will have to change when we know how we actually want to connect portals and such
        //world.Graph.AddDirected(world.GetLocation("start"), world.GetLocation("Brinstar - Morph Room - Spawn Platform (2) - Spawn Platform"), world.GetItem("fixed"));
        //world.Graph.AddDirected(world.GetLocation("start"), world.GetLocation("Meta - Metroid Meta Locations - Meta (0) - Meta"), world.GetItem("fixed"));

        var startMap = yamlReader.GetStartMap();
        var formattedStartMap = startMap.ToString("X2");
        world.Graph.AddDirected(world.GetLocation("start"), world.GetLocation($"Z1 - Overworld - Map {formattedStartMap} - Left exit"), world.GetItem("fixed"));

    }

    public static ItemSet GetItemSet(World world)
    {
        return new ItemSet
        {
            //{
            //    new ItemSetName("z1triforce", world), new WeightedSet
            //    {
            //        { 1, [.. Enumerable.Repeat(world.GetItem("Z1Triforce"), 8)] }
            //    }
            //},
            { new ItemSetName("z1d1", world), new WeightedSet
                {
                    { 1, 
                        [
                            world.GetItem("Z1Key"),
                            world.GetItem("Z1Map"),
                            world.GetItem("Z1Compass"),
                        ]
                    }
                }
            },
            { new ItemSetName("z1d2", world), new WeightedSet
                {
                    { 1,
                        [
                            world.GetItem("Z1Key"),
                            world.GetItem("Z1Map"),
                            world.GetItem("Z1Compass"),
                        ]
                    }
                }
            },
            { new ItemSetName("z1d3", world), new WeightedSet
                {
                    { 1,
                        [
                            world.GetItem("Z1Key"),
                            world.GetItem("Z1Map"),
                            world.GetItem("Z1Compass"),
                        ]
                    }
                }
            },
            { new ItemSetName("z1d4", world), new WeightedSet
                {
                    { 1,
                        [
                            world.GetItem("Z1Key"),
                            world.GetItem("Z1Map"),
                            world.GetItem("Z1Compass"),
                        ]
                    }
                }
            },
            { new ItemSetName("z1d5", world), new WeightedSet
                {
                    { 1,
                        [
                            world.GetItem("Z1Key"),
                            world.GetItem("Z1Map"),
                            world.GetItem("Z1Compass"),
                        ]
                    }
                }
            },
            { new ItemSetName("z1d6", world), new WeightedSet
                {
                    { 1,
                        [
                            world.GetItem("Z1Key"),
                            world.GetItem("Z1Map"),
                            world.GetItem("Z1Compass"),
                        ]
                    }
                }
            },
            { new ItemSetName("z1d7", world), new WeightedSet
                {
                    { 1,
                        [
                            world.GetItem("Z1Key"),
                            world.GetItem("Z1Map"),
                            world.GetItem("Z1Compass"),
                        ]
                    }
                }
            },
            { new ItemSetName("z1d8", world), new WeightedSet
                {
                    { 1,
                        [
                            world.GetItem("Z1Key"),
                            world.GetItem("Z1Map"),
                            world.GetItem("Z1Compass"),
                        ]
                    }
                }
            },
            { new ItemSetName("z1d9", world), new WeightedSet
                {
                    { 1,
                        [
                            world.GetItem("Z1Key"),
                            world.GetItem("Z1Map"),
                            world.GetItem("Z1Compass"),
                        ]
                    }
                }
            },
            { ItemSetName.DefaultSet, new WeightedSet
                {
                    { 3, [
                            world.GetItem("Z1UseBombs"),
                            world.GetItem("Z1StepLadder"),
                            world.GetItem("Z1OpenGraves"),
                            world.GetItem("Z1Raft"),
                            world.GetItem("Z1BurnTrees"),
                            world.GetItem("Z1Recorder"),
                            world.GetItem("Z1Sword"),
                        ]
                    },
                }
            }
        };
    }
}
