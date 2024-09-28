namespace Randomizer.Games.Alttp.WorldModifiers;

using Randomizer.Graph;

/// <summary>
/// Modify the edges of the graph to deal with MoonPearl/Bunny state.
/// </summary>
internal sealed class BunnyGraphifier : IAlttpWorldModifier
{
    private static readonly Dictionary<string, string> ITEM_MAP = new()
    {
        { "L1Sword", "DarkL1Sword" },
        { "FireRod", "DarkFireRod" },
        { "IceRod", "DarkIceRod" },
        { "Hammer", "DarkHammer" },
        { "Hookshot", "DarkHookshot" },
        { "BowAndArrows", "DarkBowAndArrows" },
        { "Boomerang", "DarkBoomerang" },
        { "PegasusBoots", "DarkPegasusBoots" },
        { "Powder", "DarkPowder" },
        { "ActiveBombos", "DarkActiveBombos" },
        { "ActiveEther", "DarkActiveEther" },
        { "ActiveQuake", "DarkActiveQuake" },
        { "Lamp", "DarkLamp" }, // Lamp for seeing, Lamp for lighting, ??
        { "Shovel", "DarkShovel" },
        { "OcarinaInactive", "DarkOcarinaInactive" },
        { "CaneOfSomaria", "DarkCaneOfSomaria" },
        { "CaneOfByrna", "DarkCaneOfByrna" },
        { "MirrorShield", "DarkMirrorShield" },
        { "Cape", "DarkCape" },
        { "PowerGlove", "DarkPowerGlove" },
        { "TitansMitt", "DarkTitansMitt" },
        { "Flippers", "DarkFlippers" },
        { "BugCatchingNet", "DarkBugCatchingNet" },
        { "RedBoomerang", "DarkRedBoomerang" },
        { "LiftBush", "DarkLiftBush" },
        { "LiftPot", "DarkLiftPot" },
        { "UseBomb", "DarkUseBomb" },
        { "OpenChest", "DarkOpenChest" },
    };

    /// <summary>
    /// Add edges for new dark items required based on dark world and moon pearl.
    /// </summary>
    public void AdjustEdges(World world, PRNG prng)
    {
        var graph = world.Graph;

        var moonpearl = graph.AddVertex(new Vertex
        {
            Type = VertexType.Meta,
            Name = "MoonPearl",
            World = world,
        });
        var meta = world.GetLocation("Meta");
        graph.AddDirected(meta, moonpearl, world.GetItem("MoonPearl"));

        foreach (var (lightItem, darkItem) in ITEM_MAP)
        {
            var darkVertex = graph.AddVertex(new Vertex
            {
                Type = VertexType.Meta,
                Name = darkItem,
                World = world,
                Item = world.GetItem(darkItem),
            });

            world.Graph.AddDirected(moonpearl, darkVertex, world.GetItem(lightItem));
        }

        var darkNodes = world.Graph.GetVertices().Where(v => v.World == world && v.MoonPearl == true);
        var workQueue = new Queue<Vertex>(darkNodes);
        var marked = new HashSet<Vertex>();

        while (workQueue.TryDequeue(out var node))
        {
            if (!marked.Add(node))
                continue;


            foreach (var edge in node.Edges)
            {
                var toNode = edge.To;
                if (toNode.MoonPearl != false)
                {
                    workQueue.Enqueue(toNode);

                    var darkItem = ToDarkItem(edge.Condition.Item.Name);
                    if (darkItem == null)
                        continue;

                    edge.Condition = new(world.GetItem(darkItem), edge.Condition.Count);
                }
            }
        }
    }

    public static string? ToDarkItem(string item)
    {
        if (ITEM_MAP.TryGetValue(item, out var darkItem))
            return darkItem;
        if (item.StartsWith("Defeat"))
            return $"Dark{item}";
        return null;
    }
}
