namespace Randomizer.Games.Goonies2;

using Randomizer.Games;
using Randomizer.Graph;
using Randomizer.RomModifications;
using BaseGameRandomizer = Randomizer.Graph.GameRandomizer;

public sealed class GameRandomizer(WorldConfig[] randomizerConfigs, PRNG prng)
    : BaseGameRandomizer(randomizerConfigs, prng)
{
    protected override IItemPooler CreateItemPooler(IWorld[] worlds, PRNG prng)
    {
        throw new NotImplementedException();
    }

    protected override IWorld CreateWorld(int worldId, WorldConfig worldConfig, Graph graph, PRNG prng) => new World(worldId, worldConfig, graph, prng);

    public override void AppendSpoiler(SpoilerLog spoilerLog) { } // FIXME: implement a spoiler log

    protected override void WriteWorldToRom(IWorld world, IRom rom, PRNG prng)
    {
        // TODO: Implement ROM writing for Goonies 2
        throw new NotImplementedException();
    }
}
