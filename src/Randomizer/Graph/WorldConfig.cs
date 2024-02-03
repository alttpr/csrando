namespace Randomizer.Graph;

using System.Collections.Generic;

public class WorldConfig
{
    public const int RandomCrystals = -1;

    // TODO: Align this with current website which broke it down to multiple settings.
    // See https://github.com/sporchia/alttp_vt_randomizer/pull/951
    public int RomHardMode { get; init; } = 0;

    // Use RandomizerConfig.RandomCrystals value for randomizing the number of crystals
    public int CrystalsGanon { get; set; } = 7;
    public int CrystalsTower { get; set; } = 7;

    public ushort TriforcePieces { get; set; } = 0;

    public GoalOption Goal { get; init; } = GoalOption.Ganon;

    public AccessibilityOption Accessibility { get; init; } = AccessibilityOption.Items;

    public StateOption State { get; init; } = StateOption.Standard;

    public GlitchesOption Glitches { get; init; } = GlitchesOption.None;

    public List<TechOption> Techs { get; init; } = new();

    public WeaponOption Weapon { get; init; } = WeaponOption.Randomized;

    public EntranceShuffleOption EntranceShuffle { get; init; } = EntranceShuffleOption.None;

    public EnemyShuffleOption EnemyShuffle { get; init; } = EnemyShuffleOption.None;

    public BossShuffleOption BossShuffle { get; init; } = BossShuffleOption.None;

    // TODO: Make it a bool? Do we have more planned there?
    public ShopSupplyOption RegionShopSupply { get; init; } = ShopSupplyOption.Normal;
    public bool RegionWildKeys { get; init; } = false;
    public bool RegionWildBigKeys { get; init; } = false;
    public bool RegionWildMaps { get; init; } = false;
    public bool RegionWildCompasses { get; init; } = false;
    public bool RomRupeeBow { get; init; } = false;

    // TODO: Add configuration for custom prize packs
    public bool CustomPrizePacks { get; init; } = false;

    public List<string> StartingEquipment { get; init; } = new();

    public bool MapOnPickup { get; init; } = false;
    public bool EscapeAssist { get; init; } = false;
    public bool PseudoBoots { get; init; } = false;
    public bool FastRom { get; init; } = true;
    public bool QuickSwap { get; init; } = false;
    public bool NoMusic { get; init; } = false;
    public byte CapeMagicUsageNormal { get; init; } = 0x04;
    public byte CapeMagicUsageHalf { get; init; } = 0x08;
    public byte CapeMagicUsageQuarter { get; init; } = 0x10;
    public bool CaneOfByrnaInvulnerability { get; init; } = true;
    public byte PowderedSpriteFairyPrize { get; init; } = 0xE3;
    public byte BottleFillHealth { get; init; } = 0xA0;
    public byte BottleFillMagic { get; init; } = 0x80;
    public bool CatchableFairies { get; init; } = true;
    public bool CatchableBees { get; init; } = true;
    public bool StunItemsHookshot { get; init; } = true;
    public bool StunItemsBoomerang { get; init; } = true;
    public bool SilversOnlyAtGanon { get; init; } = false;
    public bool GenericKeys { get; init; } = false;
    public bool HudItemCounter { get; init; } = false;
}

public enum GoalOption { Ganon, FastGanon, Dungeons, Pedestal, TriforceHunt }
public enum AccessibilityOption { Items, Locations, None }
public enum StateOption { Standard, Inverted, Open }
public enum GlitchesOption
{
    None,
    Overworld,
    Major,
    NoLogic,
}

public enum TechOption
{
    DungeonBunnyRevival,
}
public enum WeaponOption { Randomized, Assured, Vanilla, Swordless }
public enum EntranceShuffleOption { None, Simple, Restricted, Full, Crossed, Insanity }
public enum BossShuffleOption { None, Simple, Full, Random }
public enum ShopSupplyOption { Normal, Shuffled }
public enum EnemyShuffleOption { None, Shuffled, Random }
