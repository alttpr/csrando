namespace Randomizer.Games.SuperMetroid;

using Randomizer.Graph;
using Randomizer.RomModifications;
using BaseRom = RomModifications.Rom;

public static class RomWriter
{
    public static void Write(IRom baseRom, World world, PRNG prng, int offset = 0)
    {
        var rom = new Rom(baseRom, offset);
        rom.WriteItems(world);
        rom.WriteEventFlags(world);
        rom.WriteBossesNeeded(world);
        rom.WriteKeycardFlag(world);
        rom.WritePlms(world);
        rom.WriteMap(world);
    }
}
