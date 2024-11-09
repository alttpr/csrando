namespace Randomizer.Games.Zelda1;

public class Config
{
    public static readonly int[] RandomTriforces = [0, 1, 2, 3, 4, 5, 6, 7, 8];
    public int[] TriforceGoalChoices { get; set; } = RandomTriforces;

    public EntranceShuffleOption EntranceShuffle { get; init; } = EntranceShuffleOption.None;
    public List<string> StartingEquipment { get; init; } = new();

    private int? _triforces;
    public int Triforces
    {
        get => _triforces.GetValueOrDefault(8);
        set => _triforces = value;
    }
}

public enum EntranceShuffleOption { None, Overworld }
