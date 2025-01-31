namespace Randomizer.Graph;

using Randomizer.Games.Alttp;
using Microsoft.Extensions.Logging;

using AlttpVertex = Games.Alttp.Vertex;

/// <summary>
/// Modify the edges of the graph to shuffle entrances.
/// </summary>
internal sealed class EntranceShuffler : IWorldModifier
{
    private static readonly ILogger _logger = ClassLogger.Get();

    /// <summary>
    /// Connect Entrances, Exits, Outlets, and rooms based on World settings.
    /// </summary>
    public void AdjustEdges(IWorld world, PRNG prng)
    {
        // FIXME: fetching the entrance config is game specific, but the logic is generic enough to work with the right files.
        //        split this in a way that the world (or something world-related) returns the edge connections instead.
        string definitionName = world.WorldConfig.Alttp!.EntranceShuffle switch
        {
            EntranceShuffleOption.Simple => "simple",
            EntranceShuffleOption.Restricted => "restricted",
            EntranceShuffleOption.Full => "vanilla",
            EntranceShuffleOption.Crossed => "crossed",
            EntranceShuffleOption.Insanity => "insanity",
            EntranceShuffleOption.None => "vanilla",
            _ => throw new ArgumentException("Unknown EntranceShuffle option: " + world.WorldConfig.Alttp.EntranceShuffle)
        };
        string definitionStateName = world.WorldConfig.Alttp.State switch
        {
            StateOption.Standard => "normal",
            StateOption.Open => "normal",
            StateOption.Retro => "retro",
            StateOption.Inverted => "inverted",
            _ => throw new ArgumentException("Unknown State option: " + world.WorldConfig.Alttp.State)
        } + "/" + definitionName;

        var definition = YamlReader.LoadEntrances(definitionName);
        var definitionState = YamlReader.EntranceDataFileExists(definitionStateName)
            ? YamlReader.LoadEntrances(definitionStateName)
            : new Entrances();

        // local logger, for things that only matters during entrance shuffle.
        var logger = world.WorldConfig.Alttp.EntranceShuffle != EntranceShuffleOption.None ? _logger : null;

        var fixedItem = world.GetItem("fixed");

        foreach (var connection in definition.Fixed.Concat(definitionState.Fixed))
        {
            var from = world.GetLocation(connection[0]);
            var to = world.GetLocation(connection[1]);
            world.Graph.AddDirected(from, to, fixedItem);
        }
        
        var connected = new List<string>();
        var scopedGroups = definition.Scoped.Concat(definitionState.Scoped).GroupBy(x => x.Group);
        foreach(var scopedGroup in scopedGroups)
        {
            var scopedOverworlds = new Queue<List<List<string>>>(prng.Shuffle(scopedGroup.SelectMany(x => x.Overworld)));
            var scopedUnderworlds = new Queue<List<List<string>>>(prng.Shuffle(scopedGroup.SelectMany(x => x.Underworld)));

            while (scopedOverworlds.Count > 0 && scopedUnderworlds.Count > 0)
            {

                if (connected.Contains(scopedOverworlds.Peek()[0][0]))
                {
                    scopedOverworlds.Dequeue();
                    continue;
                }
                if (connected.Contains(scopedUnderworlds.Peek()[0][0]))
                {
                    scopedUnderworlds.Dequeue();
                    continue;
                }

                var ow_items = new Queue<List<string>>(prng.Shuffle(scopedOverworlds.Dequeue()));
                var uw_items = new Queue<List<string>>(prng.Shuffle(scopedUnderworlds.Dequeue()));

                while (ow_items.Count > 0)
                {
                    var overworlds = ow_items.Dequeue();
                    var underworlds = uw_items.Dequeue();

                    connected.Add(overworlds[0]);
                    connected.Add(underworlds[0]);
                    for (int i = 0; i < overworlds.Count; i++)
                    {
                        var overworld = world.GetLocation(overworlds[i]);
                        var underworld = world.GetLocation(underworlds[i]);
                        if (overworld.Type is VertexType.Entrance or VertexType.Hole)
                        {
                            world.Graph.AddDirected(overworld, underworld, fixedItem);
                            logger?.LogInformation("Scoped '{From}' -> '{To}' ({Condition})", overworld.Name, underworld.Name, fixedItem.Name);
                        }
                        else
                        {
                            world.Graph.AddDirected(underworld, overworld, fixedItem);
                            logger?.LogInformation("Scoped '{From}' -> '{To}' ({Condition})", underworld.Name, overworld.Name, fixedItem.Name);
                        }
                    }
                }
            }
        }

        /// I apologize for the following code and data structure. It does allow
        /// for the most flexibility in the entrance shuffle.
        foreach (var connectionGroups in definition.Connections.Concat(definitionState.Connections).GroupBy(x => x.Group))
        {
            Shuffle(prng, connectionGroups, world, logger, connected);
        }
    }


    public static void Shuffle(PRNG prng, IGrouping<string, ConnectionGroup> connectionGroups, IWorld world, ILogger? logger, List<string> connected)
    {
        var overworlds = new Queue<List<List<string>>>(prng.Shuffle(connectionGroups.SelectMany(x => x.Overworld)));
        var underworlds = new Queue<List<List<string>>>(prng.Shuffle(connectionGroups.SelectMany(x => x.Underworld)));

        while (underworlds.Count > 0 && overworlds.Count > 0)
        {
            if (connected.Contains(overworlds.Peek()[0][0]))
            {
                overworlds.Dequeue();
                continue;
            }
            if (connected.Contains(underworlds.Peek()[0][0]))
            {
                underworlds.Dequeue();
                continue;
            }

            var ow_items = new Queue<List<string>>(prng.Shuffle(overworlds.Dequeue()));
            var uw_items = new Queue<List<string>>(prng.Shuffle(underworlds.Dequeue()));
            // FIXME: this overworld handling is game specific for ALTTP
            var firstOverworld = (AlttpVertex)world.GetLocation(ow_items.Peek()[0]);
            while (ow_items.Count != uw_items.Count)
            {
                if (ow_items.Count < uw_items.Count)
                {
                    var owCollection = overworlds.Dequeue();
                    foreach (var entry in owCollection)
                    {
                        // TODO: this has potential to be a bug, if the data has overworld
                        // locations grouped that are both moon pearl and not moon pearl.
                        if (((AlttpVertex)world.GetLocation(entry[0])).MoonPearl != firstOverworld.MoonPearl)
                        {
                            overworlds.Enqueue(owCollection);
                            break;
                        }
                        ow_items.Enqueue(entry);
                    }
                }
                else
                {
                    foreach (var entry in underworlds.Dequeue())
                    {
                        uw_items.Enqueue(entry);
                    }
                }
            }

            while (ow_items.Count > 0)
            {
                var overworld = ow_items.Dequeue();
                var underworld = uw_items.Dequeue();
                if (overworld.Count != underworld.Count)
                {
                    throw new Exception("Entrance sub-sub-count mismatch");
                }

                for (int i = 0; i < overworld.Count; i++)
                {
                    var ow = world.GetLocation(overworld[i]);
                    var uw = world.GetLocation(underworld[i]);
                    if (ow.Type is VertexType.Entrance or VertexType.Hole)
                    {
                        world.Graph.AddDirected(ow, uw, world.GetItem("fixed"));
                        logger?.LogInformation("Linked '{From}' -> '{To}' ({Condition})", ow.Name, uw.Name, world.GetItem("fixed").Name);
                    }
                    else
                    {
                        world.Graph.AddDirected(uw, ow, world.GetItem("fixed"));
                        logger?.LogInformation("Linked '{From}' -> '{To}' ({Condition})", uw.Name, ow.Name, world.GetItem("fixed").Name);
                    }
                }
            }
        }
    }
}
