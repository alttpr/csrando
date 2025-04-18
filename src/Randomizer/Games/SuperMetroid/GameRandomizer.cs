namespace Randomizer.Games.SuperMetroid;

using Randomizer.Graph;
using Randomizer.RomModifications;
using BaseGameRandomizer = Randomizer.Graph.GameRandomizer;

public sealed class GameRandomizer : BaseGameRandomizer // Changed to inherit explicitly
{
    // Added traditional constructor
    public GameRandomizer(WorldConfig[] randomizerConfigs, PRNG prng, IRomFactory romFactory)
        : base(randomizerConfigs, prng, romFactory)
    {
    }

    protected override IItemPooler CreateItemPooler(IWorld[] worlds, PRNG prng) => new ItemPooler(worlds, prng);

    protected override IWorld CreateWorld(int worldId, WorldConfig worldConfig, Graph graph, PRNG prng) => new World(worldId, worldConfig, graph, prng);

    public override void AppendSpoiler(SpoilerLog spoilerLog) { } // FIXME: implement a spoiler log

    protected override void WriteWorldToRom(IWorld world, IRom rom, PRNG prng)
    {
        if (world is not World smWorld)
            throw new ArgumentException("Passed world is not for Super Metroid.", nameof(world));

        // Pass the received IRom directly to the writer.
        RomWriter.Write(rom, smWorld, prng);
    }
    protected override string CreateFileName(IWorld world, PRNG prng, string? worldSuffix)
        => $"smr_{prng.Seed:x08}{worldSuffix}.sfc";
}
