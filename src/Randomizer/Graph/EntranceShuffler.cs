namespace AlttpRandomizer.Graph;

/**
 * Modify the edges of the graph to shuffle entrances.
 */
internal sealed class EntranceShuffler
{
    private readonly Entrances _definition;
    private readonly World _world;

    /**
     * Add all the vertices to the graph for this region.
     *
     * @param World world world to reduce graph for
     *
     * @return void
     */
    public EntranceShuffler(World world)
    {
        _world = world;
        string definition_name = world.Config<string>("entrances") switch
        {
            "simple" => "simple",
            "restricted" => "vanilla",
            "full" => "vanilla",
            "crossed" => "vanilla",
            "insanity" => "vanilla",
            "none" => "vanilla",
            _ => world.Config<string>("entrances") ?? "vanilla",
        };

        _definition = YamlReader.LoadEntrances(definition_name);
    }

    /**
     * Connect Entrances, Exits, Outlets, and rooms based on World settings.
     */
    public void AdjustEdges()
    {
        int world_id = _world.Id;
        foreach (var connection in _definition.Fixed)
        {
            var from = _world.Graph.GetVertex($"{connection[0]}:{world_id}");
            var to = _world.Graph.GetVertex($"{connection[1]}:{world_id}");
            _world.Graph.AddDirected(from, to, $"fixed:{world_id}");
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
