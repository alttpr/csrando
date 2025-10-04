namespace Randomizer.Games.Combo;

using Randomizer.Games.Metadata;
using Randomizer.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[TargetGame(Game.Combo)]
public class Config
{
    [Ignore("We don't allow to set this yet")]
    public string InitialGame { get; init; } = "alttp";
    [Ignore("We don't allow to set this yet")]
    public bool Race { get; init; }
    [Ignore("We don't allow to set this yet")]
    public int Seed { get; init; }
}
