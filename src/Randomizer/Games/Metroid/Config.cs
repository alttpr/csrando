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
    [Name("Early Morph Ball")]
    [Description("Places Morph Ball early.")]
    public bool EarlyMorph { get; init; } = false;

    [Category("Gameplay")]
    [Description("Generates a completely new randomized world map. This is in early testing, be aware of potential bugs!")]
    public bool MapShuffle { get; init; } = false;

    [Category("Gameplay")]
    [Subcategory("Map Shuffle")]
    [DependsOn(nameof(MapShuffle), true)]
    [Default(MapSizeOption.Standard)]
    public MapSizeOption MapSize { get; init; } = MapSizeOption.Standard;

    /// <summary>The value of <see cref="StartArea"/> that keeps the vanilla start.</summary>
    public const string VanillaStartArea = "Brinstar";

    /// <summary>The <see cref="StartArea"/> value that resolves to a random area.</summary>
    public const string RandomStartArea = "Random";

    /// <summary>
    /// Every selectable start area (also the "Random" pool); per-seed viability is
    /// decided against the graph in Metroid.World.ResolveStartingArea. Tourian is
    /// absent structurally: the Silver Two statue bridge is gated in both directions
    /// and the Tourian elevator lands behind it, so its start reaches zero item
    /// locations even holding every item.
    /// </summary>
    public static readonly string[] StartAreaValues = ["Brinstar", "Norfair", "Kraid", "Ridley"];

    private string? _startArea;

    // "Random" is a literal value (not the omit-the-property convention) so configs
    // that never mention the setting keep the vanilla start.
    [Category("Gameplay")]
    [Name("Starting Area")]
    [Wip("Random start area selection is being play-tested")]
    [OnlyWithGames(Game.SuperMetroid, Game.Metroid)]
    [Description("The area Samus starts in: the vanilla elevator arrival cell, or a corridor generated inside the area "
        + "when Map Shuffle is on. Random picks any of the listed areas. A moved start makes Metroid the seed's "
        + "starting game (random tie-break when Super Metroid also moves its start).")]
    [Values("Brinstar", "Random", "Norfair", "Kraid", "Ridley", Default = "Brinstar")]
    public string StartArea
    {
        get => _startArea ?? VanillaStartArea;
        set => _startArea = value;
    }

    /// <summary>Whether the config asks to move the start; the combo world picks the
    /// seed's starting game from this.</summary>
    [Ignore("Derived from StartArea")]
    [System.Text.Json.Serialization.JsonIgnore]
    public bool StartAreaRequested => StartArea != VanillaStartArea;

    /// <summary>Cleared by the combo world on every game that did not win the
    /// starting-game selection, so only the seed's starting game moves its start.</summary>
    [Ignore("Internal starting-game selection state, not a setting")]
    [System.Text.Json.Serialization.JsonIgnore]
    public bool ApplyStartArea { get; set; } = true;

    [Category("Gameplay")]
    [Name("Tiered Items")]
    [Wip("Tiered item map icons are being play-tested")]
    [OnlyWithGames(Game.SuperMetroid, Game.Metroid)]
    [Description("Item cells on the map show their tier: major, medium, or minor items. Works on both vanilla and shuffled maps. Custom starts from all-minor and applies the override list.")]
    public TieredItemsSetting TieredItems { get; init; } = TieredItemsSetting.Off;

    [Category("Gameplay")]
    [Name("Custom Item Tiers")]
    [Wip("Tiered item map icons are being play-tested")]
    [OnlyWithGames(Game.SuperMetroid, Game.Metroid)]
    [DependsOn(nameof(TieredItems), TieredItemsSetting.Custom)]
    [Description("Comma-separated overrides: game:ItemName=tier (tier: minor, medium, major). Example: sm:Charge=medium, m1:IceBeam=major")]
    public string CustomItemTiers { get; init; } = "";
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
