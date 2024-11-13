namespace Randomizer.Games.Metroid;

using Randomizer.Graph;
using Randomizer.RomModifications;
using BaseGameRandomizer = Graph.GameRandomizer;

public sealed class GameRandomizer(WorldConfig[] randomizerConfigs, PRNG prng) : BaseGameRandomizer(randomizerConfigs, prng)
{
    protected override IItemPooler CreateItemPooler(IWorld[] worlds, PRNG prng) => new ItemPooler(worlds, prng);

    protected override IWorld CreateWorld(int worldId, WorldConfig worldConfig, Graph graph, PRNG prng) => new World(worldId, worldConfig, graph, prng);

    public override void AppendSpoiler(SpoilerLog spoilerLog) { } // FIXME: implement a spoiler log

    protected override void WriteWorldToRom(IWorld world, Rom rom, PRNG prng)
    {
        if (world is not World m1World)
            throw new ArgumentException("Passed world is not for Metroid.", nameof(world));

        RomWriter.Write(rom, m1World, prng);
    }
    protected override string CreateFileName(IWorld world, PRNG prng, string? worldSuffix)
        => $"m1r_{prng.Seed:x08}{worldSuffix}.nes";
}
