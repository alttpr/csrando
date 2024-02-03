namespace Randomizer.Graph.Combo.SuperMetroid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class SMWorld
{
    public static void AdjustWorld(World world)
    {
        var jsonReader = new SMJsonReader();
        jsonReader.Load();
        jsonReader.BuildGraph(world);
    }
}
