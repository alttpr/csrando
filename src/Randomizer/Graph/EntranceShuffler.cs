namespace Randomizer.Graph;

/**
 * Modify the edges of the graph to shuffle entrances.
 */
internal sealed class EntranceShuffler: IWorldModifier
{
    /**
     * Connect Entrances, Exits, Outlets, and rooms based on World settings.
     */
    public static void AdjustEdges(World world, PRNG prng)
    {
        string definition_name = world.Config.EntranceShuffle switch
        {
            EntranceShuffleOption.Simple => "simple",
            EntranceShuffleOption.Restricted => "vanilla",
            EntranceShuffleOption.Full => "vanilla",
            EntranceShuffleOption.Crossed => "vanilla",
            EntranceShuffleOption.Insanity => "vanilla",
            EntranceShuffleOption.None => "vanilla",
            _ => throw new ArgumentException("Unknown EntranceShuffle option: " + world.Config.EntranceShuffle)
        };

        var definition = YamlReader.LoadEntrances(definition_name);

        int world_id = world.Id;
        foreach (var connection in definition.Fixed)
        {
            var from = world.Graph.GetVertex($"{connection[0]}:{world_id}");
            var to = world.Graph.GetVertex($"{connection[1]}:{world_id}");
            world.Graph.AddDirected(from, to, world.GetItem("fixed"));
        }
        /* TODO: Let's only do vanilla in the meantime...
        foreach (var group in this.definition.Connections) {
            var ins = PHP.fy_shuffle(group.In.ToArray());
            var outs = PHP.fy_shuffle(group.Out.ToArray());
            if (ins.Length != outs.Length) {
                throw new Exception("Entrance count mismatch");
            }

            while (ins.Length > 0) {
                in_items = Arr.wrap(array_pop(ins));
                out_items = Arr.wrap(array_pop(outs));
                if (count(in_items) != count(out_items)) {
                    throw new Exception("Entrance sub-count mismatch");
                }
                foreach (var offset => in in in_items) {
                    out = out_items[offset];
                    from = this.world.graph.getVertex(${in}:{world_id}");
                    to = this.world.graph.getVertex($"{out}:{world_id}");
                    this.world.graph.addDirected(from, to, $"fixed:{world_id}");
                }
            }
        }
        */
    }
}
