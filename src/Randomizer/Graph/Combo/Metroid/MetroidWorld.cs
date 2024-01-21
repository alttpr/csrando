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
    public static Dictionary<World, MetroidYamlReader.YamlData?> Data = new Dictionary<World, MetroidYamlReader.YamlData?>();

    // Adjusts the world as needed to randomize Metroid
    public static void AdjustWorld(World world)
    {
        // Load the Metroid Yaml Data and hook up the world to the current world graph
        var yamlReader = new MetroidYamlReader();
        var metroidVertices = yamlReader.LoadYmlData(world);
        Data[world] = yamlReader.Data;

        foreach (var vtx in metroidVertices)
        {
            var name = vtx.TryGetValue("name", out object? nameValue) ? (string)nameValue : throw new InvalidDataException("Metroid vertex without a name");
            var type = vtx.TryGetValue("type", out object? typeValue) ? (VertexType)typeValue : VertexType.Meta;
            var item = vtx.TryGetValue("item", out object? itemValue) ? (string)itemValue : null;
            var itemset = vtx.TryGetValue("itemset", out object? itemsetValue) ? (string[])itemsetValue : null;

            var address = type == VertexType.Standing ? GetItemLocationAddress(world, name) : null;

            var vertex = new Vertex()
            {
                World = world,
                Name = name,
                Type = type,
                Item = item != null ? world.GetItem("M1" + item) : null,
                ItemSet = itemset?.Select(i => new ItemSetName(i, world)).ToArray() ?? [],
                Addresses = address != null ? [ address.Value, address.Value + 1] : null,
                Game = Game.Metroid
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

        var morphItem = world.GetLocation("M1 - Brinstar - Morph Room - Morph Pedestal (1) - Morph Ball");
        morphItem.Item = world.GetItem("OneRupee");

    }

    public static ItemSet GetItemSet(World world)
    {
        return new ItemSet
        {
            { ItemSetName.DefaultSet, new WeightedSet
                {
                    { 3, [
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
            },
            { new ItemSetName("lw", world), new WeightedSet
                {
                    { 2, [ world.GetItem("M1Morph")] }                            
                }
            }
        };
    }

    private static readonly Dictionary<(int, int), int> CoordToAddressMap = new()
    {
        
        // Brinstar
        { (0x02, 0x0F), 0x74000A },
        { (0x03, 0x18), 0x740013 },
        { (0x03, 0x1B), 0x740019 },
        { (0x05, 0x07), 0x740022 },
        { (0x05, 0x19), 0x740028 },
        { (0x07, 0x19), 0x740035 },
        { (0x09, 0x13), 0x74003E },
        { (0x0B, 0x12), 0x74004B },
        { (0x0E, 0x02), 0x740059 },
        { (0x0E, 0x09), 0x74005F },
        
        // Norfair
        { (0x0A, 0x1B), 0x740205 },
        { (0x0A, 0x1C), 0x74020B },
        { (0x0B, 0x1A), 0x740219 },
        { (0x0B, 0x1B), 0x74021F },
        { (0x0B, 0x1C), 0x740225 },
        { (0x0C, 0x1A), 0x74022E },
        { (0x0E, 0x12), 0x74023F },
        { (0x0F, 0x11), 0x740248 },
        { (0x0F, 0x13), 0x74024F },
        { (0x0F, 0x14), 0x740255 },
        { (0x10, 0x0F), 0x740267 },
        { (0x11, 0x1B), 0x740286 },
        { (0x13, 0x1A), 0x740299 },
        { (0x14, 0x1C), 0x7402AC },
        { (0x15, 0x12), 0x7402B5 },
        { (0x16, 0x13), 0x7402C3 },
        { (0x16, 0x14), 0x7402C9 },
        
        // Kraid
        { (0x15, 0x04), 0x740615 },
        { (0x15, 0x09), 0x74061B },
        { (0x16, 0x0A), 0x740624 },
        { (0x19, 0x0A), 0x74062D },
        { (0x1B, 0x05), 0x740636 },
        { (0x1D, 0x08), 0x740646 },
        
        // Ridley
        { (0x18, 0x12), 0x740805 },
        { (0x19, 0x11), 0x740813 },
        { (0x1B, 0x18), 0x74081C },
        { (0x1D, 0x0F), 0x740825 },
        { (0x1E, 0x14), 0x74082E },

    };

    // TODO: Ideally the randomizer should write its own complete sprite table to the rom instead of looking up hardcoded data
    // But we'll do this for now to get things working
    private static int? GetItemLocationAddress(World world, string vertexName)
    {
        var data = Data[world];

        if (data is null)
        {
            throw new Exception("No Metroid Data for world " + world.Id);
        }

        var room = data.rooms.Where(r => vertexName.Contains(r.name)).First();
        var sprite = room.sprites.Where(s => vertexName.Contains(s.name)).First();
        var screen = data.screens.Where(s => s.screen == room.screens[sprite.screen]).First();

        var x = room.position[0] + sprite.screen;
        var y = room.position[1];

        int? address = CoordToAddressMap.TryGetValue((y, x), out var addr) ? addr : null;
        return address;
        
    }

}
