namespace Randomizer.Games.Goonies2;

using Randomizer.Graph;
using Randomizer.RomModifications;
using BaseGameRandomizer = Randomizer.Graph.GameRandomizer; // Corrected using alias

public sealed class GameRandomizer(WorldConfig[] randomizerConfigs, PRNG prng, IRomFactory romFactory) // Add romFactory
    : BaseGameRandomizer(randomizerConfigs, prng, romFactory) // Pass romFactory to base
{
    protected override IItemPooler CreateItemPooler(IWorld[] worlds, PRNG prng)
    {
        throw new NotImplementedException();
    }

    protected override IWorld CreateWorld(int worldId, WorldConfig worldConfig, Graph graph, PRNG prng) => new World(worldId, worldConfig, graph, prng);

    public override void AppendSpoiler(SpoilerLog spoilerLog) { } // FIXME: implement a spoiler log

    protected override void WriteWorldToRom(IWorld world, IRom rom, PRNG prng)
    {
        // This needs implementation and handling for IRom types
        throw new NotImplementedException();
    }
}
