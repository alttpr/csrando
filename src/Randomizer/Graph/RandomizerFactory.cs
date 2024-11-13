namespace Randomizer.Graph;

using Alttp = Games.Alttp.GameRandomizer;
using Goonies2 = Games.Goonies2.GameRandomizer;
using Zelda1 = Games.Zelda1.GameRandomizer;
using Metroid = Games.Metroid.GameRandomizer;

/// <summary>
/// Get the world one needs for randomization based on the config provided.
/// </summary>
public class RandomizerFactory
{
    public static GameRandomizer Create(WorldConfig[] configs, int? seed)
    {
        var prng = new PRNG(seed);
        // TODO: this should probably also have a more explicit way of specifying the game
        //       (in case conflicting or overlapping options exist, such as multi-game randomizers
        //       or a multi-game config with a single-game randomization target)
        return configs switch
        {
            [ { Alttp: { } } ] => new Alttp(configs, prng),
            [ { Goonies2: { } } ] => new Goonies2(configs, prng),
            [ { Zelda1: { } } ] => new Zelda1(configs, prng),
            [ { Metroid: { } }] => new Metroid(configs, prng),

            _ => throw new Exception("Unknown game"),
        };
    }
}
