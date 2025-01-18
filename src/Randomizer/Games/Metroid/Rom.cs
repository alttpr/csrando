namespace Randomizer.Games.Metroid;

using Randomizer.Games.Alttp;
using Randomizer.Graph;
using Randomizer.RomModifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Rom : GameRom
{
    public Rom(RomModifications.Rom rom, int offset) : base(rom, offset) 
    {

    }

    public void WriteItems(World world)
    {
        foreach(var location in world.GetLocationsOfType(VertexType.Item).Where(l => l.Item != null))
        {
            if(location.Item!.Bytes == null)
                continue;

            if (location.Addresses == null)
                continue;
            
            Write((Address)location.Addresses[0], location.Item!.Bytes);
        }
    }
}
