namespace Randomizer.Graph;

/// <summary>
/// Modify the edges of the graph to shuffle entrances.
/// </summary>
internal sealed class EntranceShuffler : IWorldModifier
{
    /// <summary>
    /// Connect Entrances, Exits, Outlets, and rooms based on World settings.
    /// </summary>
    public static void AdjustEdges(World world, PRNG prng)
    {
        string definitionName = world.Config.EntranceShuffle switch
        {
            EntranceShuffleOption.Simple => "simple",
            EntranceShuffleOption.Restricted => "vanilla",
            EntranceShuffleOption.Full => "vanilla",
            EntranceShuffleOption.Crossed => "vanilla",
            EntranceShuffleOption.Insanity => "insanity",
            EntranceShuffleOption.None => "vanilla",
            _ => throw new ArgumentException("Unknown EntranceShuffle option: " + world.Config.EntranceShuffle)
        };
        string definitionStateName = world.Config.State switch
        {
            StateOption.Standard => "normal",
            StateOption.Open => "normal",
            StateOption.Retro => "normal",
            StateOption.Inverted => "inverted",
            _ => throw new ArgumentException("Unknown State option: " + world.Config.State)
        } + "/" + definitionName;

        var definition = YamlReader.LoadEntrances(definitionName);
        var definitionState = YamlReader.EntranceDataFileExists(definitionStateName)
            ? YamlReader.LoadEntrances(definitionStateName)
            : new Entrances();

        var fixedItem = world.GetItem("fixed");

        foreach (var connection in definition.Fixed.Concat(definitionState.Fixed))
        {
            var from = world.GetLocation(connection[0]);
            var to = world.GetLocation(connection[1]);
            world.Graph.AddDirected(from, to, fixedItem);
        }

        foreach (var group in definition.Connections.Concat(definitionState.Connections))
        {
            var ins = new Queue<List<string>>(prng.Shuffle(group.In));
            var outs = new Queue<List<string>>(prng.Shuffle(group.Out));
            if (ins.Count != outs.Count)
            {
                throw new Exception("Entrance count mismatch");
            }

            while (ins.Count > 0)
            {
                var from_items = ins.Dequeue();
                var to_items = outs.Dequeue();
                if (from_items.Count != to_items.Count)
                {
                    throw new Exception("Entrance sub-count mismatch");
                }

                for (var i = 0; i < from_items.Count; i++)
                {
                    var from = world.GetLocation(from_items[i]);
                    var to = world.GetLocation(to_items[i]);
                    world.Graph.AddDirected(from, to, fixedItem);
                }
            }
        }

        // Deal with multi-entrances. unfortunately this is a bit of a mess.
        var multiIns = new Queue<List<string>>(prng.Shuffle(definition.Multi.In.Concat(definitionState.Multi.In)));
        var multiOuts = new Queue<List<string>>(prng.Shuffle(definition.Multi.Out.Concat(definitionState.Multi.Out)));
        if (multiIns.Count != multiOuts.Count)
        {
            throw new Exception("Entrance count mismatch");
        }

        while (multiIns.Count > 0)
        {
            var from_items = multiIns.Dequeue();
            var to_items = multiOuts.Dequeue();
            if (from_items.Count != to_items.Count)
            {
                throw new Exception("Entrance sub-count mismatch");
            }

            // Shuffle pairs of froms, so that entrances and exits if
            // multientra are not in the same order.
            var from_pairs = prng.Shuffle(from_items.Chunk(2)).SelectMany(x => x).ToList();

            for (var i = 0; i < from_items.Count; i++)
            {
                var from = world.GetLocation(from_pairs[i]);
                var to = world.GetLocation(to_items[i]);
                world.Graph.AddDirected(from, to, fixedItem);
            }
        }
    }
}
