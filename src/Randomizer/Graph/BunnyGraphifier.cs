namespace Randomizer.Graph;
/**
 * Modify the edges of the graph to deal with MoonPearl/Bunny state.
 */
internal sealed class BunnyGraphifier : IWorldModifier
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

    /**
     * Add edges for new dark items required based on dark world and moon pearl.
     */
    public static void AdjustEdges(World world, PRNG prng)
    {
        var graph = world.Graph;

        var moonpearl = graph.AddVertex(new Vertex
        {
            Type = VertexType.Meta,
            Name = "MoonPearl",
            World = world,
        });
        var meta = world.GetLocation("Meta");
        graph.AddDirected(meta!, moonpearl, world.GetItem("MoonPearl"));

        foreach (var (light_item, dark_item) in ITEM_MAP)
        {
            var dark_vertex = graph.AddVertex(new Vertex
            {
                Type = VertexType.Meta,
                Name = dark_item,
                World = world,
                Item = world.GetItem(dark_item),
            });

            world.Graph.AddDirected(moonpearl, dark_vertex, world.GetItem(light_item));
        }

        var dark_nodes =
            from vertex in world.Graph.GetVertices()
            where vertex.MoonPearl == true
            select vertex;

        var work_queue = new Queue<Vertex>(dark_nodes);
        var marked = new HashSet<Vertex>();

        while (work_queue.TryDequeue(out var node))
        {
            if (node.Name.StartsWith("Lake of Bad Omen"))
                node.ToString();
            if (marked.Contains(node))
            {
                continue;
            }
            marked.Add(node);

            foreach (var edge in node.Edges)
            {
                var toNode = edge.To;
                if (toNode.MoonPearl != false)
                {
                    work_queue.Enqueue(toNode);
                    if (!ITEM_MAP.ContainsKey(edge.Condition.Item.Name))
                    {
                        continue;
                    }

                    edge.Condition = new(world.GetItem(ITEM_MAP[edge.Condition.Item.Name]), edge.Condition.Count);
                }
            }
        }
    }
}
