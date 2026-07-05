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
    [Description("This is in early testing, be aware of potential bugs and balance issues!")]
    public bool DungeonShuffle { get; init; } = false;

    [Category("Gameplay")]
    [Description("Randomize the contents of shop caves. Junk: shops sell random consumables at random prices. Full: shop slots join the item pool and can hold progression (gated in logic behind a farmable weapon).")]
    [Default(ShopShuffleOption.Off)]
    public ShopShuffleOption ShopShuffle { get; init; } = ShopShuffleOption.Off;

    // Sub-options below only have an effect when DungeonShuffle is enabled, so they are grouped
    // under a "Dungeon Shuffle" subcategory and hidden in the UI until it is turned on.

    [Category("Gameplay")]
    [Subcategory("Dungeon Shuffle")]
    [DependsOn(nameof(DungeonShuffle), true)]
    [Default(DungeonStyleOption.Progressive)]
    public DungeonStyleOption DungeonStyle { get; init; } = DungeonStyleOption.Progressive;

    [Category("Gameplay")]
    [Subcategory("Dungeon Shuffle")]
    [DependsOn(nameof(DungeonShuffle), true)]
    [Default(EnemyPlacementOption.Progressive)]
    public EnemyPlacementOption EnemyPlacement { get; init; } = EnemyPlacementOption.Progressive;

    [Category("Gameplay")]
    [Subcategory("Dungeon Shuffle")]
    [DependsOn(nameof(DungeonShuffle), true)]
    [Default(HiddenItemsOption.Sometimes)]
    public HiddenItemsOption HiddenItems { get; init; } = HiddenItemsOption.Sometimes;

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
/// Controls randomization of shop cave contents.
/// Off: vanilla shop items and prices.
/// Junk: shops sell randomized consumables/filler at randomized prices; never gates progression.
/// Full: shop slots become real item locations in the main pool (progression eligible), gated in
///   logic behind cave reachability AND possession of a farmable weapon (since rupees are infinitely
///   farmable, the price itself is flavor, not a logic gate).
/// </summary>
public enum ShopShuffleOption { Off, Junk, Full }

/// <summary>
/// Controls overall dungeon generation style.
/// Progressive: vanilla-like scaling (small early, large late) with slight fuzz.
/// Wild: each dungeon gets randomized parameters independent of level order.
/// Megadungeon: all dungeons are large and complex with more segments and cellars.
/// Nightmare: maximum size, maximum complexity, maximum pain.
/// Minimal: smallest practical dungeons for quick testing (8 rooms, no locked doors, no cellars).
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

/// <summary>
/// Controls whether generated dungeon items are hidden until the room is cleared of enemies.
/// (Boss rooms always reveal their item on the boss's death regardless of this setting.)
/// Off: every item is visible on entry; rooms that would have needed a kill trigger get their
///   shutters opened instead, so nothing has to be cleared for the item.
/// Sometimes: item rooms that gate shutters behind a kill hide their item, plus a moderate chance
///   for open (non-shutter) item rooms with killable enemies.
/// Always: every item room that can use a kill trigger hides its item until the room is cleared.
/// </summary>
public enum HiddenItemsOption { Off, Sometimes, Always }
