namespace Randomizer.Games.Zelda1;

using Randomizer.Graph;
using Randomizer.RomModifications;

public static class RomWriter
{
    public static void Write(IRom baseRom, World world, PRNG prng, int offset = 0)
    {
        var rom = new Rom(baseRom, offset);
        var data = world.YamlData!;

        rom.WriteTriforceGoal(world);
        rom.WriteItems(world);

        if (world.Config.EntranceShuffle != EntranceShuffleOption.None)
        {
            rom.WriteOverworldMapData(world, prng, data);
            rom.WriteSpecial(world, prng, data);
        }

        if (world.Config.DungeonShuffle)
        {
            rom.WriteUnderworldMapData(data);
            rom.WriteLevelData(data);
        }
    }
}
