namespace Randomizer.Games.Zelda1;

using Randomizer.Games.Metadata;

[TargetGame(Game.Zelda1)]
public class Config
{
    public static readonly int[] RandomTriforces = [0, 1, 2, 3, 4, 5, 6, 7, 8];
    [ValueRange(0, 8)]
    [RandomizedOptionsFor(nameof(Triforces))]
    public int[] TriforceGoalChoices { get; set; } = RandomTriforces;

    public EntranceShuffleOption EntranceShuffle { get; init; } = EntranceShuffleOption.None;
    [Ignore("Starting inventory is too advenced to be represented with simple attributes")]
    public List<string> StartingEquipment { get; init; } = [];

    private int? _triforces;
    [ValueRange(0, 8, Default = 8)]
    public int Triforces
    {
        get => _triforces.GetValueOrDefault(8);
        set => _triforces = value;
    }
}

public enum EntranceShuffleOption { None, Overworld }
