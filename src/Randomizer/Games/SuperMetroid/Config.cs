namespace Randomizer.Games.SuperMetroid;
using Randomizer.Graph;

public class Config
{
    private static string[] ImplicitTech = [
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
    private static string[] BasicTech = [
            "canMidAirMorph",
            "canWalljump",
            "canUseFrozenEnemies",
            "canShinespark",
        ];
    private static string[] MediumTech = [
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
    private static string[] HardTech = [
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

    public List<string> StartingEquipment { get; init; } = new();
    public Logic Logic { get; init; } = Logic.Basic;
    public string[] CustomTech { get; init; } = [];
    public Keycards Keycards { get; init; } = Keycards.None;


    public string[] BossChoices { get; set; } = RandomBosses;

    private string? _bosses;
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
    Regular,
    Bosses,
    All
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
