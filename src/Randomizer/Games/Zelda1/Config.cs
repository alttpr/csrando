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

    [Category("Gameplay")]
    public bool DungeonShuffle { get; init; } = false;

    [Category("Gameplay")]
    public DungeonStyleOption DungeonStyle { get; init; } = DungeonStyleOption.Progressive;

    [Category("Gameplay")]
    public EnemyPlacementOption EnemyPlacement { get; init; } = EnemyPlacementOption.Progressive;

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

/// <summary>
/// Controls overall dungeon generation style.
/// Progressive: vanilla-like scaling (small early, large late) with slight fuzz.
/// Wild: each dungeon gets randomized parameters independent of level order.
/// Megadungeon: all dungeons are large and complex with more segments and cellars.
/// Nightmare: maximum size, maximum complexity, maximum pain.
/// Minimal: smallest possible dungeons for quick testing (6 rooms, no locked doors, no cellars).
/// </summary>
public enum DungeonStyleOption { Progressive, Wild, Megadungeon, Nightmare, Minimal }

/// <summary>
/// Controls how enemies are placed in generated dungeons.
/// Vanilla: each dungeon only uses enemies from its matching vanilla level.
/// Progressive: enemies are bucketed by difficulty tier (1–3, 4–5, 6–8, 9) —
///   a dungeon can draw from any level in the same tier.
/// Random: any dungeon-valid enemy can appear in any dungeon.
/// </summary>
public enum EnemyPlacementOption { Vanilla, Progressive, Random }
