namespace Randomizer.Games.Zelda1;

using Randomizer.Graph;
using BaseRom = RomModifications.Rom;

public static class RomWriter
{
    public static void Write(BaseRom baseRom, World world, PRNG prng, int offset = 0)
    {
        var rom = new Rom(baseRom, offset);
        var data = world.YamlData!;

        rom.WriteItems(world);

        if (world.Config.EntranceShuffle != EntranceShuffleOption.None)
        {
            rom.WriteOverworldMapData(world, prng, data);
            rom.WriteSpecial(world, prng, data);
        }
    }
}
