namespace Randomizer.Graph.Combo.Metroid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using ItemSet = Dictionary<ItemSetName, /* WeightedSet */ Dictionary<int, List<Item>>>;
using WeightedSet = Dictionary<int, List<Item>>;

internal class MetroidWorld
{
    // Adjusts the world as needed to randomize Metroid
    public static void AdjustWorld(World world)
    {
        // Load the Metroid Yaml Data and hook up the world to the current world graph
        var yamlReader = new MetroidYamlReader();
        var metroidVertices = yamlReader.LoadYmlData(world);

        foreach (var vtx in metroidVertices)
        {
            var name = vtx.TryGetValue("name", out object? nameValue) ? (string)nameValue : throw new InvalidDataException("Metroid vertex without a name");
            var type = vtx.TryGetValue("type", out object? typeValue) ? (VertexType)typeValue : VertexType.Meta;
            var item = vtx.TryGetValue("item", out object? itemValue) ? (string)itemValue : null;
            var itemset = vtx.TryGetValue("itemset", out object? itemsetValue) ? (string[])itemsetValue : null;

            var vertex = new Vertex()
            {
                World = world,
                Name = name,
                Type = type,
                Item = item != null ? world.GetItem("M1" + item) : null,
                ItemSet = itemset?.Select(i => new ItemSetName(i, world)).ToArray() ?? []
            };

            world.Graph.AddVertex(vertex);
        }

        var metroidEdges = yamlReader.GetForWorld(world);
        foreach(var edgeCollection in metroidEdges)
        {
            var edgeCollectionData = edgeCollection.Key.Split(":").First().Split('|');
            var requirementName = edgeCollectionData.First();
            
            if (!requirementName.StartsWith("fixed"))
            {
                requirementName = "M1" + requirementName;
            }

            var requirement = world.GetItem(requirementName);
            var requirementCount = int.Parse(edgeCollectionData.Skip(1).FirstOrDefault() ?? "1");

            foreach(var edges in edgeCollection.Value.Directed)
            {
                var from = world.GetLocation(edges[0]);
                var to = world.GetLocation(edges[1]);
                if (from is null || to is null)
                {
                    throw new Exception("Name Connection Mismatch: " + $"({edges[0]}, {edges[1]}) => " + $"({from}, {to})");
                }

                world.Graph.AddDirected(from, to, requirement, requirementCount);
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
            }
        }

        // Connect the start edge to the start edge of the Metroid graph
        // This will have to change when we know how we actually want to connect portals and such
        world.Graph.AddDirected(world.GetLocation("start"), world.GetLocation("M1 - Brinstar - Morph Room - Spawn Platform (2) - Spawn Platform"), world.GetItem("fixed"));
        world.Graph.AddDirected(world.GetLocation("start"), world.GetLocation("M1 - Meta - Metroid Meta Locations - Meta (0) - Meta"), world.GetItem("fixed"));

    }

    public static ItemSet GetItemSet(World world)
    {
        return new ItemSet
        {
            { ItemSetName.DefaultSet, new WeightedSet
                {
                    { 3, [
                            world.GetItem("M1Morph"),
                            world.GetItem("M1Bombs"),
                            world.GetItem("M1Varia"),
                            world.GetItem("M1HiJump"),
                            world.GetItem("M1IceBeam"),
                            world.GetItem("M1LongBeam"),
                            world.GetItem("M1WaveBeam"),
                            world.GetItem("M1ScrewAttack"),
                            world.GetItem("M1EnergyTank"),
                            world.GetItem("M1Missile"),
                        ]
                    },
                    { 9001, [
                            .. Enumerable.Repeat(world.GetItem("M1Missile"), 20),
                            .. Enumerable.Repeat(world.GetItem("M1EnergyTank"), 7),
                        ]
                    }
                }
            }
        };
    }
}
