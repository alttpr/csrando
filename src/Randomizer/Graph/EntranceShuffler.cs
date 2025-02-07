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

        var connected = new HashSet<string>();
        var scopedGroups = definition.Scoped.Concat(definitionState.Scoped).GroupBy(x => x.Group);
        foreach (var scopedGroup in scopedGroups)
        {
            // Create lists of all available pairs
            var allOverworldPairs = scopedGroup.SelectMany(x => x.Overworld).ToList();
            var allUnderworldPairs = scopedGroup.SelectMany(x => x.Underworld).ToList();

            // Shuffle both lists
            var shuffledOverworlds = new Queue<List<List<string>>>(prng.Shuffle(allOverworldPairs));
            var shuffledUnderworlds = new Queue<List<List<string>>>(prng.Shuffle(allUnderworldPairs));

            // Keep matching until one list is empty
            while (shuffledOverworlds.Count > 0 && shuffledUnderworlds.Count > 0)
            {
                var currentOverworld = shuffledOverworlds.Peek();
                var currentUnderworld = shuffledUnderworlds.Peek();

                // Skip if already connected
                if (connected.Contains(currentOverworld[0][0]))
                {
                    shuffledOverworlds.Dequeue();
                    continue;
                }
                if (connected.Contains(currentUnderworld[0][0]))
                {
                    shuffledUnderworlds.Dequeue();
                    continue;
                }

                // Get the pairs and shuffle their internal order
                var owPairs = new Queue<List<string>>(prng.Shuffle(shuffledOverworlds.Dequeue()));
                var uwPairs = new Queue<List<string>>(prng.Shuffle(shuffledUnderworlds.Dequeue()));

                // Connect all pairs in this set
                while (owPairs.Count > 0 && uwPairs.Count > 0)
                {
                    var overworlds = owPairs.Dequeue();
                    var underworlds = uwPairs.Dequeue();

                    // Mark as connected and establish connections
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

        // Handle remaining regular connections
        foreach (var connectionGroups in definition.Connections.Concat(definitionState.Connections).GroupBy(x => x.Group))
        {
            Shuffle(prng, connectionGroups, world, logger, connected);
        }
    }

    public static void Shuffle(PRNG prng, IGrouping<string, ConnectionGroup> connectionGroups, IWorld world, ILogger? logger, HashSet<string> connected)
    {
        var overworlds = new Queue<List<List<string>>>(prng.Shuffle(connectionGroups.SelectMany(x => x.Overworld)));
        var underworlds = new Queue<List<List<string>>>(prng.Shuffle(connectionGroups.SelectMany(x => x.Underworld)));

        while (underworlds.Count > 0 && overworlds.Count > 0)
        {
            // Skip already connected entrances
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

            var owGroup = overworlds.Dequeue();
            var uwGroup = underworlds.Dequeue();

            var ow_items = new Queue<List<string>>(prng.Shuffle(owGroup));
            var uw_items = new Queue<List<string>>(prng.Shuffle(uwGroup));

            while (ow_items.Count > 0 && uw_items.Count > 0)
            {
                var overworld = ow_items.Dequeue();
                var underworld = uw_items.Dequeue();

                // Mark as connected and establish connections
                connected.Add(overworld[0]);
                connected.Add(underworld[0]);

                for (int i = 0; i < Math.Min(overworld.Count, underworld.Count); i++)
                {
                    var ow = world.GetLocation(overworld[i]);
                    var uw = world.GetLocation(underworld[i]);

                    if (ow.Type is VertexType.Entrance or VertexType.Hole)
                    {
                        world.Graph.AddDirected(ow, uw, world.GetItem("fixed"));
                        logger?.LogInformation("Linked '{From}' -> '{To}' ({Condition})", ow.Name, uw.Name, "fixed");
                    }
                    else
                    {
                        world.Graph.AddDirected(uw, ow, world.GetItem("fixed"));
                        logger?.LogInformation("Linked '{From}' -> '{To}' ({Condition})", uw.Name, ow.Name, "fixed");
                    }
                }
            }
        }
    }
}
