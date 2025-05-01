namespace Randomizer.Games.Zelda1;

using Randomizer.Graph;
using Randomizer.RomModifications;

public static class RomWriter
{
    public static void Write(IRom baseRom, World world, PRNG prng)
    {

        foreach (var location in world.GetLocationsOfType(VertexType.Item))
        {
            if (location.Item is Item item && location.Addresses != null)
            {
                baseRom.Write((int)location.Addresses[0], [(byte)item.Id]);
            }
        }

        //var startingEquipment = world.ComputeStartingItems();
        //foreach (var item in startingEquipment.All())
        //{
        //    // Write starting equipment to ROM
        //    baseRom.Write(0x123456, new byte[] { (byte)((Item)item.Key).Byte! }); // Replace 0x123456 with the actual address
        //}
    }
}
