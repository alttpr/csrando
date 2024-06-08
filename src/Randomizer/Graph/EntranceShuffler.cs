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

        var definition = YamlReader.LoadEntrances(definitionName);
        var fixedItem = world.GetItem("fixed");

        foreach (var connection in definition.Fixed)
        {
            var from = world.GetLocation(connection[0]);
            var to = world.GetLocation(connection[1]);
            world.Graph.AddDirected(from, to, fixedItem);
        }

        foreach (var group in definition.Connections)
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
    }
}
