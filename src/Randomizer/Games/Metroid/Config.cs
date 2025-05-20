namespace Randomizer.Games.Metroid;

using Randomizer.Games.Metadata;

[TargetGame(Game.Metroid)]
public class Config
{
    public List<string> StartingEquipment { get; init; } = new();
}
