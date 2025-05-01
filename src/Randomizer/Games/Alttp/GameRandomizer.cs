namespace Randomizer.Games.Alttp;

using Randomizer.Graph;
using Randomizer.RomModifications;
using BaseGameRandomizer = Graph.GameRandomizer;
using BaseSpoilerLog = Graph.SpoilerLog;
using GlobalConfig = Randomizer.Config;

public sealed class GameRandomizer(WorldConfig[] randomizerConfigs, PRNG prng) : BaseGameRandomizer(randomizerConfigs, prng)
{
    private const int RomSize = 2 * 1024 * 1024;

    protected override IItemPooler CreateItemPooler(IWorld[] worlds, PRNG prng) => new ItemPooler(worlds, prng);
    protected override IWorld CreateWorld(int worldId, WorldConfig worldConfig, Graph graph, PRNG prng) => new World(worldId, worldConfig, graph, prng);

    public override void AppendSpoiler(BaseSpoilerLog spoilerLog) => Spoiler.Log(Worlds, spoilerLog);

    public override void ApplyPatch(IRom rom, FileInfo baseBPS)
    {
        rom.Resize(RomSize);
        rom.ApplyBasePatch(baseBPS);
    }
    protected override void WriteWorldToRom(IWorld world, IRom rom, PRNG prng)
    {
        if (world is not World alttpWorld)
            throw new ArgumentException("Passed world is not for The Legend of Zelda: A Link to the Past.", nameof(world));

        RomWriter.Write(rom, alttpWorld, prng);
    }
    protected override string CreateFileName(IWorld world, PRNG prng, string? worldSuffix)
        => $"alttpr_{world.WorldConfig.Alttp!.Glitches}_{world.WorldConfig.Alttp.State}_{world.WorldConfig.Alttp.Goal}_{prng.Seed:x08}{worldSuffix}.sfc";
    public override FileInfo? ProvideBaseRom()
    {
        // FIXME: AssembleBaseRom is game-specific, and we should probably move this into here.
        //        we'd check for a cached base rom file (perhaps including a hash check if the source changed),
        //        build a new one if it is missing (or outdated), and return its path.
        if (File.Exists(GlobalConfig.BaseRomFile))
            return new FileInfo(GlobalConfig.BaseRomFile);

        return base.ProvideBaseRom();
    }
}
