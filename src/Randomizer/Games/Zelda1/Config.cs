namespace Randomizer.Games.Zelda1;

using Randomizer.Games.Metadata;
using Randomizer.Graph;

[TargetGame(Game.Zelda1)]
public class Config
{
    public static readonly string[] RandomTriforces = ["0", "1", "2", "3", "4", "5", "6", "7", "8"];

    [RandomizedOptionsFor(nameof(Triforces))]
    [Values("0", "1", "2", "3", "4", "5", "6", "7", "8", Default = "8")]
    public string[] TriforceGoalChoices { get; set; } = RandomTriforces;

    [Category("Gameplay")]
    public EntranceShuffleOption EntranceShuffle { get; init; } = EntranceShuffleOption.None;

    [Ignore("Starting inventory is too advenced to be represented with simple attributes")]
    public List<string> StartingEquipment { get; init; } = [];

    private string? _triforces;
    [Category("Goal")]
    [Values("0", "1", "2", "3", "4", "5", "6", "7", "8", Default = "8")]
    public string Triforces
    {
        get => _triforces ?? "8";
        set => _triforces = value;
    }

    public void SelectRandomValues(PRNG prng)
    {
        _triforces ??= prng.GetRandomElement(TriforceGoalChoices);
    }
}

public enum EntranceShuffleOption { None, Overworld }
