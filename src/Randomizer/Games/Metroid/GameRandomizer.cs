namespace Randomizer.Games.Metroid;

using Randomizer.Graph;
using Randomizer.RomModifications;
using BaseGameRandomizer = Randomizer.Graph.GameRandomizer; // Corrected using alias

public sealed class GameRandomizer(WorldConfig[] randomizerConfigs, PRNG prng, IRomFactory romFactory) // Add romFactory
    : BaseGameRandomizer(randomizerConfigs, prng, romFactory) // Pass romFactory to base
{
    protected override IItemPooler CreateItemPooler(IWorld[] worlds, PRNG prng) => new ItemPooler(worlds, prng);

    protected override IWorld CreateWorld(int worldId, WorldConfig worldConfig, Graph graph, PRNG prng) => new World(worldId, worldConfig, graph, prng);

    public override void AppendSpoiler(SpoilerLog spoilerLog) { } // FIXME: implement a spoiler log

    protected override void WriteWorldToRom(IWorld world, IRom rom, PRNG prng)
    {
        if (world is not World m1World)
            throw new ArgumentException("Passed world is not for Metroid.", nameof(world));

        // Cast IRom to RomModifications.Rom as RomWriter expects it
        // Needs careful handling like other GameRandomizers
        if (rom is not RomModifications.Rom romImpl)
            throw new NotSupportedException($"Writing with ROM type {rom.GetType().Name} is not yet supported for Metroid.");

        RomWriter.Write(romImpl, m1World, prng);
    }
    protected override string CreateFileName(IWorld world, PRNG prng, string? worldSuffix)
        => $"m1r_{prng.Seed:x08}{worldSuffix}.nes";
}
