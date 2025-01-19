namespace Randomizer.Games.SuperMetroid;

using Randomizer.Graph;
using BaseRom = RomModifications.Rom;

public static class RomWriter
{
    public static void Write(BaseRom baseRom, World world, PRNG prng, int offset = 0)
    {
        var rom = new Rom(baseRom, offset);
        rom.WriteItems(world);
        rom.WriteBossesNeeded(world);
        rom.WriteKeycardFlag(world);
        rom.WritePlms(world);
    }
}
