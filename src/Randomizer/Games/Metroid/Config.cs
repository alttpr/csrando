namespace Randomizer.Games.Metroid;

using Microsoft.VisualBasic;
using Randomizer.Games.Metadata;

[TargetGame(Game.Metroid)]
public class Config
{
    [Ignore("Starting equipment is too advanced to be represented with simple attributes")]
    public List<string> StartingEquipment { get; init; } = new();

    [Category("Gameplay")]
    public Logic Logic { get; init; } = Logic.Basic;

}
public enum Logic
{
    Basic
}
