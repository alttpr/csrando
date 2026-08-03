namespace Randomizer.Games.Metroid;

using System.Linq;
using Randomizer.Graph;
using Randomizer.RomModifications;


public class Rom : GameRom
{
    public Rom(IRom rom, int offset) : base(rom, offset)
    {

    }

    public void WriteItems(World world)
    {
        foreach (var location in world.GetLocationsOfType(VertexType.Item).Where(l => l.Item != null))
        {
            if (location.Item!.Bytes == null)
                continue;

            if (location.Addresses == null)
                continue;

            Write((Address)location.Addresses[0], location.Item!.Bytes);
        }
    }

    public void WritePatchData(World world)
    {
        // Tiered map icons must be resolved after the filler but before the automap
        // patch data reaches the ROM; this is the single point that is post-fill on
        // both the standalone and combo paths (combo also nulls location addresses
        // only after this runs).
        MapGen.MapIconPatcher.Apply(world);

        foreach (var (address, data) in world.PatchData ?? [])
        {
            Write((Address)address, data);
        }
    }
}
