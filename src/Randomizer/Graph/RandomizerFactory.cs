namespace Randomizer.Graph;

using Alttp = Games.Alttp.GameRandomizer;
using Goonies2 = Games.Goonies2.GameRandomizer;
using Zelda1 = Games.Zelda1.GameRandomizer;
using Combo = Games.Combo.GameRandomizer; // Assuming Combo exists
using Metroid = Games.Metroid.GameRandomizer; // Assuming Metroid exists
using SuperMetroid = Games.SuperMetroid.GameRandomizer; // Assuming SuperMetroid exists
using Randomizer.RomModifications; // Add this

/// <summary>
/// Get the world one needs for randomization based on the config provided.
/// </summary>
public class RandomizerFactory
{
    // Update Create to accept IRomFactory
    public static GameRandomizer Create(WorldConfig[] configs, int? seed, IRomFactory romFactory)
    {
        var prng = new PRNG(seed);
        // TODO: this should probably also have a more explicit way of specifying the game
        //       (in case conflicting or overlapping options exist, such as multi-game randomizers
        //       or a multi-game config with a single-game randomization target)
        return configs switch
        {
            // Pass romFactory to constructors
            [{ Alttp: { } }] => new Alttp(configs, prng, romFactory),
            [{ Goonies2: { } }] => new Goonies2(configs, prng, romFactory),
            [{ Zelda1: { } }] => new Zelda1(configs, prng, romFactory),
            [{ Combo: { } }] => new Combo(configs, prng, romFactory), // Assuming Combo exists
            [{ Metroid: { } }] => new Metroid(configs, prng, romFactory), // Assuming Metroid exists
            [{ SuperMetroid: { } }] => new SuperMetroid(configs, prng, romFactory), // Assuming SuperMetroid exists
            _ => throw new Exception("Unknown or unsupported game configuration"),
        };
    }
}
