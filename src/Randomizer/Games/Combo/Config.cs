namespace Randomizer.Games.Combo;

using Randomizer.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Config
{
    public WorldConfig? Games { get; init; }
    public bool Race { get; init; }
    public int Seed { get; init; }
}
