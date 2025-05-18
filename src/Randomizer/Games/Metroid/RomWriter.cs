namespace Randomizer.Games.Metroid;

using Randomizer.Graph;
using Randomizer.RomModifications;

public static class RomWriter
{
    public static void Write(IRom baseRom, World world, PRNG prng, int offset = 0)
    {
        var rom = new Rom(baseRom, offset);

        rom.WriteItems(world);
        rom.WritePatchData(world);
    }
}
