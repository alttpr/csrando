namespace Randomizer.Graph;

using Randomizer.Games;
using Alttp = Games.Alttp.GameRandomizer;
using Goonies2 = Games.Goonies2.GameRandomizer;
using Zelda1 = Games.Zelda1.GameRandomizer;
using Combo = Games.Combo.GameRandomizer; 
using Metroid = Games.Metroid.GameRandomizer;
using SuperMetroid = Games.SuperMetroid.GameRandomizer;
using Randomizer.RomModifications; // Add this

/// <summary>
/// Get the world one needs for randomization based on the config provided.
/// </summary>
public class RandomizerFactory
{
    public static GameRandomizer Create(WorldConfig[] configs, int? seed)
    {
        var prng = new PRNG(seed);
        // TODO: this doesn't currently handle multiworld
        return configs switch
        {
            [ { Game: Games.RandomizerTarget.Alttpr, Alttp: { } } ] => new Alttp(configs, prng),
            [ { Game: Games.RandomizerTarget.Combo, Combo: { } }] => new Combo(configs, prng),
            //[ { Game: Games.RandomizerTarget.G2R, Goonies2: { } } ] => new Goonies2(configs, prng),
            //[ { Game: Games.RandomizerTarget.Z1R, Zelda1: { } } ] => new Zelda1(configs, prng),
            _ => throw new Exception($"Unsupported configuration: {configs.Length} worlds for {string.Join(", ", configs.Select(c => c.Game).Distinct())}"),
        };
    }
}
