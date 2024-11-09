namespace Randomizer.Graph;

using Alttp = global::Randomizer.Games.Alttp.World;
using Goonies2 = global::Randomizer.Games.Goonies2.World;
using Zelda1 = global::Randomizer.Games.Zelda1.World;

/// <summary>
/// Get the world one needs for randomization based on the config provided.
/// </summary>
public class WorldFactory
{
    public static IWorld CreateWorld(int worldId, WorldConfig config, Graph graph, PRNG prng)
    {
        return config switch
        {
            { Alttp: { } } => new Alttp(worldId, config, graph, prng),
            { Goonies2: { } } => new Goonies2(worldId, config, graph, prng),
            { Zelda1: { } } => new Zelda1(worldId, config, graph, prng),
            _ => throw new Exception("Unknown game"),
        };
    }
}
