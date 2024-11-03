namespace Randomizer.Games.Zelda1;

using Randomizer.Graph;
using BaseRom = RomModifications.Rom;

public static class RomWriter
{
    public static void Write(BaseRom baseRom, World world, PRNG prng)
    {

        foreach (var location in world.GetLocationsOfType(VertexType.Item))
        {
            var item = location.Item as Item;
            if (item != null && location.Addresses != null)
            {
                baseRom.Write((int)location.Addresses[0], new byte[] { (byte)item.Id });
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
