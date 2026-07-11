namespace Randomizer.Games.Metroid;

using Randomizer.Games.Metadata;

[TargetGame(Game.Metroid)]
public class Config
{
    [Ignore("Starting equipment is too advanced to be represented with simple attributes")]
    public List<string> StartingEquipment { get; init; } = new();

    [Category("Gameplay")]
    public Logic Logic { get; init; } = Logic.Basic;

    [Category("Gameplay")]
    [Description("Generates a completely new randomized world map. This is in early testing, be aware of potential bugs!")]
    public bool MapShuffle { get; init; } = false;

    [Category("Gameplay")]
    [Subcategory("Map Shuffle")]
    [DependsOn(nameof(MapShuffle), true)]
    [Default(MapSizeOption.Standard)]
    public MapSizeOption MapSize { get; init; } = MapSizeOption.Standard;
}

public enum Logic
{
    Basic
}

public enum MapSizeOption
{
    Small,
    Standard,
    Large,
    /// <summary>Saturates the grid: every area grows until nothing fits anymore.</summary>
    Nightmare
}
