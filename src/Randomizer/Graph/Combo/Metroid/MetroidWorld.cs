namespace Randomizer.Graph.Combo.Metroid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
                Name = name,
                Type = type,
                Item = item != null ? world.GetItem(item) : null,
                ItemSet = itemset ?? []
            };

            world.Graph.AddVertex(vertex);
        }

        var metroidEdges = yamlReader.GetForWorld(world);
        foreach(var edgeCollection in metroidEdges)
        {
            var edgeCollectionData = edgeCollection.Key.Split(":").First().Split('|');
            var requirement = world.GetItem(edgeCollectionData.First());
            var requirementCount = int.Parse(edgeCollectionData.Skip(1).FirstOrDefault() ?? "1");

            foreach(var edges in edgeCollection.Value.Directed)
            {
                var from = world.Graph.GetVertex(edges[0]);
                var to = world.Graph.GetVertex(edges[1]);
                if (from is null || to is null)
                {
                    throw new Exception("Name Connection Mismatch: " + $"({edges[0]}, {edges[1]}) => " + $"({from}, {to})");
                }

                world.Graph.AddDirected(from, to, requirement, requirementCount);
            }

            foreach (var edges in edgeCollection.Value.Undirected)
            {
                var from = world.Graph.GetVertex(edges[0]);
                var to = world.Graph.GetVertex(edges[1]);
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
        world.Graph.AddDirected(world.GetLocation("start"), world.GetLocation("Brinstar - Morph Room - Spawn Platform (2) - Spawn Platform"), world.GetItem("fixed"));
        world.Graph.AddDirected(world.GetLocation("start"), world.GetLocation("Meta - Metroid Meta Locations - Meta (0) - Meta"), world.GetItem("fixed"));

        // Set up temporary starting items so we can traverse the whole M1 world (more or less)
        world.StartingItems.AddItem(world.GetItem("Morph"));
        world.StartingItems.AddItem(world.GetItem("Bombs"));
        world.StartingItems.AddItem(world.GetItem("Varia"));
        world.StartingItems.AddItem(world.GetItem("HiJump"));
        world.StartingItems.AddItem(world.GetItem("IceBeam"));
        world.StartingItems.AddItem(world.GetItem("LongBeam"));
        world.StartingItems.AddItem(world.GetItem("Missile"), 20);

    }
}
