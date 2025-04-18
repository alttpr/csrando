namespace Randomizer.Games.Alttp;

using System.IO;
using Randomizer.Graph;
using Randomizer.RomModifications;

using BaseGameRandomizer = Graph.GameRandomizer;
using BaseSpoilerLog = Graph.SpoilerLog;
using GlobalConfig = Randomizer.Config;

public sealed class GameRandomizer(WorldConfig[] randomizerConfigs, PRNG prng, IRomFactory romFactory)
    : BaseGameRandomizer(randomizerConfigs, prng, romFactory)
{
    private const int RomSize = 2 * 1024 * 1024;

    protected override IItemPooler CreateItemPooler(IWorld[] worlds, PRNG prng) => new ItemPooler(worlds, prng);
    protected override IWorld CreateWorld(int worldId, WorldConfig worldConfig, Graph graph, PRNG prng) => new World(worldId, worldConfig, graph, prng);

    public override void AppendSpoiler(BaseSpoilerLog spoilerLog) => Spoiler.Log(Worlds, spoilerLog);

    protected override void WriteWorldToRom(IWorld world, IRom rom, PRNG prng)
    {
        if (world is not World alttpWorld)
            throw new ArgumentException("Passed world is not for The Legend of Zelda: A Link to the Past.", nameof(world));

        // Pass the received IRom (which could be Rom or LoggedRom) directly to the writer.
        // The writer creates its own internal Alttp.Rom wrapper using this IRom.
        RomWriter.Write(rom, alttpWorld, prng);
    }
    protected override string CreateFileName(IWorld world, PRNG prng, string? worldSuffix)
        => $"alttpr_{world.WorldConfig.Alttp!.Glitches}_{world.WorldConfig.Alttp.State}_{world.WorldConfig.Alttp.Goal}_{prng.Seed:x08}{worldSuffix}.sfc";
    public override FileInfo? ProvideBaseRom()
    {
        if (File.Exists(GlobalConfig.BaseRomFile))
            return new FileInfo(GlobalConfig.BaseRomFile);

        return base.ProvideBaseRom();
    }
}
