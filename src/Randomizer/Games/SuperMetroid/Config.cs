namespace Randomizer.Games.SuperMetroid;

using Randomizer.Games.Metadata;
using Randomizer.Graph;

[TargetGame(Game.SuperMetroid)]
public class Config
{
    private static readonly string[] ImplicitTech = [
            "canStopOnADime",
            "canTrivialMidAirMorph",
            "canUseGrapple",
            "canTurnaroundSpinJump",
            "canUseEnemies",
            "canTrivialUseFrozenEnemies",
            "canEscapeEnemyGrab",
            "canSpecialBeamAttack",
            "canAwakenZebes",
        ];
    private static readonly string[] BasicTech = [
            "canMidAirMorph",
            "canWalljump",
            "canUseFrozenEnemies",
            "canShinespark",
        ];
    private static readonly string[] MediumTech = [
            "canHeatRun",
            "canSuitlessMaridia",
            "canSpaceJumpWaterBounce",
            "canDisableEquipment",
            "canDownGrab",
            "canCrouchJump",
            "canGravityJump",
            "canSpringBallJumpMidAir",
            "canCarefulJump",
            "canPreciseWalljump",
            "canConsecutiveWalljump",
            "canBombHorizontally",
            "canIBJ",
            "canNeutralDamageBoost",
            "canPseudoScrew",
            "canHorizontalShinespark",
            "canShinechargeMovement",
        ];
    private static readonly string[] HardTech = [
            "canSuitlessLavaDive",
            "canSunkenTileWideWallClimb",
            "canCrossRoomJumpIntoWater",
            "canPlayInSand",
            "canManageReserves",
            "canMoonwalk",
            "canResetFallSpeed",
            "canWallJumpInstantMorph",
            "canLateralMidAirMorph",
            "canSpringBallBounce",
            "canMockball",
            "canTwoTileSqueeze",
            "canPreciseGrapple",
            "canMorphTurnaround",
            "canUseIFrames",
            "canTrickySpringBallJump",
            "canDelayedWalljump",
            "canStaggeredWalljump",
            "canJumpIntoIBJ",
            "canPowerBombMidIBJ",
            "canSnailClimb",
            "canHorizontalDamageBoost",
            "canMochtroidIceClimb",
            "canTrickyUseFrozenEnemies",
            "canDodgeWhileShooting",
            "canHitbox",
            "canOffScreenSuperShot",
            "canGateGlitch",
            "canHeroShot",
            "canHyperGateShot",
            "canMidairShinespark",
            "canUseSpeedEchoes",
            "canWaterShineCharge",
            "canXRayWaitForIFrames",
            "canXRayStandUp",
            "canCeilingClip",
            "canXRayCeilingClip",
            "canCameraManip",
        ];

    public static SkillConfig BasicSkillConfig = new()
    {
        ShinechargeTiles = 33,
        HeatedShinechargeTiles = 33,
        SpeedballTiles = 65,
        ShinechargeLeniencyFrames = 120,
        HeatDamageMultiplier = 4.0m,
        EnemyDamageMultiplier = 1.5m,
    };
    public static SkillConfig MediumSkillConfig = new()
    {
        ShinechargeTiles = 25,
        HeatedShinechargeTiles = 29,
        SpeedballTiles = 50,
        ShinechargeLeniencyFrames = 90,
        HeatDamageMultiplier = 2.5m,
        EnemyDamageMultiplier = 1.2m,
    };
    public static SkillConfig HardSkillConfig = new()
    {
        ShinechargeTiles = 20,
        HeatedShinechargeTiles = 24,
        SpeedballTiles = 35,
        ShinechargeLeniencyFrames = 60,
        HeatDamageMultiplier = 1.5m,
        EnemyDamageMultiplier = 1.0m,
    };

    private static readonly string[] RandomBosses = ["0", "1", "2", "3", "4"];

    public Dictionary<Logic, string[]> LogicTechs = new()
    {
        [Logic.Basic] =
            [
                ..ImplicitTech,
                ..BasicTech,
            ],
        [Logic.Medium] =
            [
                ..ImplicitTech,
                ..BasicTech,
                ..MediumTech,
            ],
        [Logic.Hard] =
            [
                ..ImplicitTech,
                ..BasicTech,
                ..MediumTech,
                ..HardTech,
            ],
        [Logic.Custom] = [],
    };

    public Dictionary<Logic, SkillConfig> LogicSkillConfigs = new()
    {
        [Logic.Basic] = BasicSkillConfig,
        [Logic.Medium] = MediumSkillConfig,
        [Logic.Hard] = HardSkillConfig
    };

    [Ignore("Starting equipment is too advanced to be represented with simple attributes")]
    public List<string> StartingEquipment { get; init; } = new();

    [Category("Gameplay")]
    public Logic Logic { get; init; } = Logic.Basic;

    [Category("Gameplay")]
    [Name("Early Morph Ball")]
    [Description("Places Morph Ball early.")]
    public bool EarlyMorph { get; init; } = false;

    [Ignore("Skill config is too complex to be represented with simple attributes")]
    public string[] CustomTech { get; init; } = [];

    [Category("Goal")]
    public Keycards Keycards { get; init; } = Keycards.None;

    [Category("Goal")]
    [DependsOn(nameof(Keycards), Keycards.All)]
    [Description("If enabled, the key door before G4 is not placed")]
    public bool FastG4 { get; init; } = false;

    [Category("Gameplay")]
    public MapRandomizerSetting MapRandomizer { get; init; } = MapRandomizerSetting.None;

    [Category("Gameplay")]
    [Name("Tiered Items")]
    [Wip("Tiered item map icons are being play-tested")]
    [OnlyWithGames(Game.SuperMetroid, Game.Metroid)]
    [DependsOn(nameof(MapRandomizer), MapRandomizerSetting.Standard)]
    [Description("Item dots on the map show their tier: major, medium, or minor items. Custom starts from all-minor and applies the override list.")]
    public TieredItemsSetting TieredItems { get; init; } = TieredItemsSetting.Off;

    [Category("Gameplay")]
    [Name("Custom Item Tiers")]
    [Wip("Tiered item map icons are being play-tested")]
    [OnlyWithGames(Game.SuperMetroid, Game.Metroid)]
    [DependsOn(nameof(TieredItems), TieredItemsSetting.Custom)]
    [Description("Comma-separated overrides: game:ItemName=tier (tier: minor, medium, major). Example: sm:Charge=medium, m1:IceBeam=major")]
    public string CustomItemTiers { get; init; } = "";

    [Category("Gameplay")]
    [Description("Spawn all items directly no matter the state of events. (For example killing bosses like Phantoon)")]
    public bool SpawnAllItems { get; init; } = false;

    /// <summary>The value of <see cref="StartLocation"/> that keeps the vanilla start.</summary>
    public const string VanillaStartLocation = "Ship";

    /// <summary>The <see cref="StartLocation"/> value that resolves to a random station.</summary>
    public const string RandomStartLocation = "Random";

    /// <summary>
    /// Every selectable start location; the "Random" pool is this list without Ship.
    /// A test keeps it in sync with the data-derived
    /// <see cref="SaveStations.EligibleStartStations"/> and with the [Values] list on
    /// <see cref="StartLocation"/>, which must repeat the names literally.
    /// </summary>
    public static readonly string[] StartLocationValues =
    [
        VanillaStartLocation,
        "Crateria Save Room",
        "Big Pink Save Room",
        "Green Brinstar Main Shaft Save Room",
        "Etecoon Save Room",
        "Kraid Save Room",
        "Caterpillar Save Room",
        "Post Crocomire Save Room",
        "Bubble Mountain Save Room",
        "Frog Savestation",
        "Crocomire Save Room",
        "Lower Norfair Elevator Save Room",
        "Red Kihunter Shaft Save Room",
        "Wrecked Ship Save Room",
        "Glass Tunnel Save Room",
        "Forgotten Highway Save Room",
        "Aqueduct Save Room",
        "Draygon Save Room",
    ];

    private string? _startLocation;

    // "Random" is a literal value (not the omit-the-property convention) so configs
    // that never mention the setting keep the vanilla start.
    [Category("Gameplay")]
    [Name("Starting Location")]
    [Wip("Random starts are being play-tested")]
    [OnlyWithGames(Game.SuperMetroid, Game.Metroid)]
    [DependsOn(nameof(MapRandomizer), MapRandomizerSetting.Standard)]
    [Description("The save station Samus starts and initially respawns at; requires the Map Randomizer, which decides "
        + "per seed whether a station can reach items and a portal with an empty inventory (Random draws only from "
        + "stations that can). A moved start makes Super Metroid the seed's starting game (random tie-break when "
        + "Metroid also moves its start).")]
    [Values("Ship", "Random",
        "Crateria Save Room",
        "Big Pink Save Room",
        "Green Brinstar Main Shaft Save Room",
        "Etecoon Save Room",
        "Kraid Save Room",
        "Caterpillar Save Room",
        "Post Crocomire Save Room",
        "Bubble Mountain Save Room",
        "Frog Savestation",
        "Crocomire Save Room",
        "Lower Norfair Elevator Save Room",
        "Red Kihunter Shaft Save Room",
        "Wrecked Ship Save Room",
        "Glass Tunnel Save Room",
        "Forgotten Highway Save Room",
        "Aqueduct Save Room",
        "Draygon Save Room",
        Default = "Ship")]
    public string StartLocation
    {
        get => _startLocation ?? VanillaStartLocation;
        set => _startLocation = value;
    }

    /// <summary>Whether the config asks to move the start; the combo world picks the
    /// seed's starting game from this. Requires the map randomizer — on the vanilla
    /// layout no station passes the itemless-pocket viability check.</summary>
    [Ignore("Derived from StartLocation")]
    [System.Text.Json.Serialization.JsonIgnore]
    public bool StartLocationRequested =>
        MapRandomizer == MapRandomizerSetting.Standard && StartLocation != VanillaStartLocation;

    /// <summary>Cleared by the combo world on every game that did not win the
    /// starting-game selection, so only the seed's starting game moves its start.</summary>
    [Ignore("Internal starting-game selection state, not a setting")]
    [System.Text.Json.Serialization.JsonIgnore]
    public bool ApplyStartLocation { get; set; } = true;

    [RandomizedOptionsFor(nameof(Bosses))]
    [Values("0", "1", "2", "3", "4")]
    public string[] BossChoices { get; set; } = RandomBosses;

    private string? _bosses;

    [Category("Goal")]
    [Values("0", "1", "2", "3", "4", Default = "4")]
    public string Bosses
    {
        get => _bosses ?? "4";
        set => _bosses = value;
    }

    public void SelectRandomValues(PRNG prng)
    {
        if (_bosses is null)
            _bosses = prng.GetRandomElement(BossChoices);
    }
}

public enum Logic
{
    Basic,
    Medium,
    Hard,
    Custom
}

public enum Keycards
{
    None,
    //Regular,
    //Bosses,
    All
}

public enum MapRandomizerSetting
{
    None,
    Standard
}

public class SkillConfig
{
    public int ShinechargeTiles { get; init; } = 0;
    public int HeatedShinechargeTiles { get; init; } = 0;
    public int SpeedballTiles { get; init; } = 0;
    public int ShinechargeLeniencyFrames { get; init; } = 0;
    public decimal HeatDamageMultiplier { get; init; } = 0m;
    public decimal EnemyDamageMultiplier { get; init; } = 0m;
}

[PostGenSettingsFor("supermetroid", Target = RandomizerTarget.Combo)]
public class SuperMetroidPostGenSettings
{
    [Name("Separate Screw Attack Animation")]
    [Description("If enabled, Screw Attack will have its own separate animation.")]
    [OnPatch(0x3F0208, 0x01)]
    [OffPatch(0x3F0208, 0x00)]
    public bool SeparateScrewAttack { get; init; } = false;

    [Name("Low Energy Beep")]
    [Description("If enabled, a beep will sound when energy is low.")]
    [OffPatch(0x086A9B, 0x80)]
    [OffPatch(0x087337, 0x80)]
    [OffPatch(0x08E6D5, 0x80)]
    public bool LowEnergyBeep { get; init; } = true;
}
