namespace Randomizer.Games.Metroid;

using Randomizer.Graph;
using Randomizer.RomModifications;
using BaseGameRandomizer = Randomizer.Graph.GameRandomizer;

public sealed class GameRandomizer(WorldConfig[] randomizerConfigs, PRNG prng)
    : BaseGameRandomizer(randomizerConfigs, prng)
{
    protected override IItemPooler CreateItemPooler(IWorld[] worlds, PRNG prng) => new ItemPooler(worlds, prng);

    protected override IWorld CreateWorld(int worldId, WorldConfig worldConfig, Graph graph, PRNG prng) => new World(worldId, worldConfig, graph, prng);

    public override void AppendSpoiler(SpoilerLog spoilerLog)
    {
        // FIXME: item locations are still missing here; the combo spoiler covers them in
        // combo mode, where this randomizer is not used.
        foreach (var world in Worlds)
        {
            if (world is World m1World)
                Spoiler.AppendMap(spoilerLog.Spoiler, m1World);
        }
    }

    protected override void WriteWorldToRom(IWorld world, IRom rom, PRNG prng)
    {
        if (world is not World m1World)
            throw new ArgumentException("Passed world is not for Metroid.", nameof(world));

        RomWriter.Write(rom, m1World, prng);

        // Standalone seeds have no cross-game transitions; terminate the table so a door
        // landing on the vanilla portal coordinate (possible under map shuffle) cannot
        // trigger the base patch's transition entry. The combo writer rewrites the table.
        rom.Write(RomWriter.TransitionTableAddress, [0x00, 0x00]);
    }
    protected override string CreateFileName(IWorld world, PRNG prng, string? worldSuffix)
        => $"m1r_{prng.Seed:x08}{worldSuffix}.nes";
}
