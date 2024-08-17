namespace Randomizer.Graph;

using Microsoft.Extensions.Logging;

/// <summary>
/// Modify the edges of the graph to shuffle entrances.
/// </summary>
internal sealed class EntranceShuffler : IWorldModifier
{
    private static readonly ILogger _logger = ClassLogger.Get();

    /// <summary>
    /// Connect Entrances, Exits, Outlets, and rooms based on World settings.
    /// </summary>
    public static void AdjustEdges(World world, PRNG prng)
    {
        string definitionName = world.Config.EntranceShuffle switch
        {
            EntranceShuffleOption.Simple => "simple",
            EntranceShuffleOption.Restricted => "restricted",
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

        // local logger, for things that only matters during entrance shuffle.
        var logger = world.Config.EntranceShuffle != EntranceShuffleOption.None ? _logger : null;

        var fixedItem = world.GetItem("fixed");

        foreach (var connection in definition.Fixed.Concat(definitionState.Fixed))
        {
            var from = world.GetLocation(connection[0]);
            var to = world.GetLocation(connection[1]);
            world.Graph.AddDirected(from, to, fixedItem);
        }

        var scopedGroups = definition.Scoped.Concat(definitionState.Scoped).GroupBy(x => x.Group);
        /// I apologize for the following code and data structure. It does allow
        /// for the most flexibility in the entrance shuffle.
        foreach (var connectionGroups in definition.Connections.Concat(definitionState.Connections).GroupBy(x => x.Group))
        {
            /// do scoped things
            /// TODO: figure out what happens when scoped collides with Symetric
            var connected = new List<string>();
            var scopedGroup = scopedGroups.FirstOrDefault(x => x.Key == connectionGroups.Key);
            if (scopedGroup != null)
            {
                var scopedIns = new Queue<List<List<string>>>(prng.Shuffle(scopedGroup.SelectMany(x => x.In)));
                var scopedOuts = new Queue<List<List<string>>>(prng.Shuffle(scopedGroup.SelectMany(x => x.Out)));
                if (scopedIns.Count > scopedOuts.Count)
                {
                    throw new Exception("Entrance count mismatch (scoped)");
                }

                while (scopedIns.Count > 0)
                {
                    var from_items = new Queue<List<string>>(prng.Shuffle(scopedIns.Dequeue()));
                    var to_items = new Queue<List<string>>(prng.Shuffle(scopedOuts.Dequeue()));

                    while (from_items.Count > 0)
                    {
                        var froms = from_items.Dequeue();
                        var tos = to_items.Dequeue();

                        connected.Add(froms[0]);
                        connected.Add(tos[0]);
                        for (var i = 0; i < froms.Count; i++)
                        {
                            var from = world.GetLocation(froms[i]);
                            var to = world.GetLocation(tos[i]);
                            world.Graph.AddDirected(from, to, fixedItem);
                            logger?.LogInformation("Scoped '{From}' -> '{To}' ({Condition})", from.Name, to.Name, fixedItem.Name);
                        }
                    }
                }
            }

            if (connectionGroups.First().Symmetric)
            {
                SymmetricShuffle(prng, connectionGroups, world, logger, connected);
            }
            else
            {
                Shuffle(prng, connectionGroups, world, logger, connected);
            }

        }
    }

    public static void SymmetricShuffle(PRNG prng, IGrouping<string, ConnectionGroup> connectionGroups, World world, ILogger? logger, List<string> connected)
    {
        var ins = new List<List<List<string>>>(connectionGroups.SelectMany(x => x.In));
        var outs = new List<List<List<string>>>(connectionGroups.SelectMany(x => x.Out));
        if (ins.Count != outs.Count)
        {
            throw new Exception("Entrance count mismatch");
        }

        while (ins.Count > 0)
        {
            Queue<List<string>> from_items;
            Queue<List<string>> to_items;
            var in_index = prng.GetRandomInt(ins.Count);
            var out_index = prng.GetRandomInt(outs.Count);

            if (in_index == out_index)
            {
                from_items = new(prng.Shuffle(ins.ElementAt(in_index)));
                to_items = new(outs.ElementAt(out_index));
                ins.RemoveAt(in_index);
                outs.RemoveAt(out_index);
            }
            else
            {
                var shuffled = prng.Shuffle(ins.ElementAt(in_index).Indexed()).ToArray();
                var unshuffled_outs = outs.ElementAt(in_index);
                var shuffled_outs = new List<List<string>>();
                foreach (var (index, _) in shuffled) shuffled_outs.Add(unshuffled_outs[index]);
                from_items = new(shuffled.Select(e => e.Value).Concat(ins.ElementAt(out_index)));
                to_items = new(outs.ElementAt(out_index).Concat(shuffled_outs));
                ins.RemoveAt(Math.Max(in_index, out_index));
                ins.RemoveAt(Math.Min(in_index, out_index));
                outs.RemoveAt(Math.Max(in_index, out_index));
                outs.RemoveAt(Math.Min(in_index, out_index));
            }

            while (from_items.Count > 0)
            {
                var froms = from_items.Dequeue();
                var tos = to_items.Dequeue();
                if (froms.Count != tos.Count)
                {
                    throw new Exception("Entrance sub-sub-count mismatch");
                }

                for (var i = 0; i < froms.Count; i++)
                {
                    var from = world.GetLocation(froms[i]);
                    var to = world.GetLocation(tos[i]);
                    world.Graph.AddDirected(from, to, world.GetItem("fixed"));
                    logger?.LogInformation("Linked '{From}' -> '{To}' ({Condition})", from.Name, to.Name, world.GetItem("fixed").Name);
                }
            }
        }
    }

    public static void Shuffle(PRNG prng, IGrouping<string, ConnectionGroup> connectionGroups, World world, ILogger? logger, List<string> connected)
    {
        var ins = new Queue<List<List<string>>>(prng.Shuffle(connectionGroups.SelectMany(x => x.In)));
        var outs = new Queue<List<List<string>>>(prng.Shuffle(connectionGroups.SelectMany(x => x.Out)));
        if (ins.Count != outs.Count)
        {
            throw new Exception("Entrance count mismatch");
        }

        while (ins.Count > 0)
        {
            if (connected.Contains(ins.Peek()[0][0]))
            {
                ins.Dequeue();
                continue;
            }
            if (connected.Contains(outs.Peek()[0][0]))
            {
                outs.Dequeue();
                continue;
            }

            Queue<List<string>> from_items;
            Queue<List<string>> to_items;
            from_items = new(prng.Shuffle(ins.Dequeue()));
            to_items = new(prng.Shuffle(outs.Dequeue()));
            if (from_items.Count != to_items.Count)
            {
                throw new Exception("Entrance sub-count mismatch");
            }

            while (from_items.Count > 0)
            {
                var froms = from_items.Dequeue();
                var tos = to_items.Dequeue();
                if (froms.Count != tos.Count)
                {
                    throw new Exception("Entrance sub-sub-count mismatch");
                }

                for (var i = 0; i < froms.Count; i++)
                {
                    var from = world.GetLocation(froms[i]);
                    var to = world.GetLocation(tos[i]);
                    world.Graph.AddDirected(from, to, world.GetItem("fixed"));
                    logger?.LogInformation("Linked '{From}' -> '{To}' ({Condition})", from.Name, to.Name, world.GetItem("fixed").Name);
                }
            }
        }
    }
}
