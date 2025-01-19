namespace Randomizer.Games.Alttp;

using System.Collections.Generic;
using Randomizer.Graph;

public class Config
{
    public static readonly string[] RandomCrystals = ["0", "1", "2", "3", "4", "5", "6", "7"];
    public static readonly int DefaultMoldormEyeCount = 2;
    public static readonly int DefaultMolderp = 2;

    // TODO: Align this with current website which broke it down to multiple settings.
    // See https://github.com/sporchia/alttp_vt_randomizer/pull/951
    public int RomHardMode { get; init; } = 0;

    // Use an array of allowed random values for randomizing the number of crystals.
    // A single-element array acts as specific count to use.
    public string[] CrystalsGanonChoices { get; set; } = RandomCrystals;
    public string[] CrystalsTowerChoices { get; set; } = RandomCrystals;
    private int[] _moldormEyeCountChoices = [DefaultMoldormEyeCount];
    public int[] MoldormEyeCountChoices
    {
        get => _moldormEyeCountChoices;
        set
        {
            var choices = value.Where(i => i is >= 0 and <= 8).ToList();
            if (choices.Count == 0)
                choices.Add(DefaultMoldormEyeCount);
            _moldormEyeCountChoices = [.. choices];
        }
    }
    private int[] _molderpChoices = [DefaultMolderp];
    public int[] MolderpChoices
    {
        get => _molderpChoices;
        set
        {
            var choices = value.Where(i => i is >= 1 and <= 8).ToList();
            if (choices.Count == 0)
                choices.Add(DefaultMolderp);
            _molderpChoices = [.. choices];
        }
    }


    private string? _crystalsGanon;
    public string CrystalsGanon
    {
        get => _crystalsGanon ?? "7";
        set => _crystalsGanon = value;
    }
    private string? _crystalsTower;
    public string CrystalsTower
    {
        get => _crystalsTower ?? "7";
        set => _crystalsTower = value;
    }

    private int? _moldormEyeCount;
    public int MoldormEyeCount
    {
        get => _moldormEyeCount.GetValueOrDefault(DefaultMoldormEyeCount);
        init => _moldormEyeCount = value;
    }
    private int? _molderp;
    public int Molderp
    {
        get => _molderp.GetValueOrDefault(DefaultMolderp);
        init => _molderp = value;
    }

    public void SelectRandomValues(PRNG prng)
    {
        if (Goal == GoalOption.Dungeons)
            _crystalsGanon = "Dungeons";

        _crystalsGanon ??= prng.GetRandomElement(CrystalsGanonChoices);
        _crystalsTower ??= prng.GetRandomElement(CrystalsTowerChoices);
        _moldormEyeCount ??= prng.GetRandomElement(MoldormEyeCountChoices);
        _molderp ??= prng.GetRandomElement(MolderpChoices);
    }

    public ushort TriforcePieces { get; set; } = 50;

    public GoalOption Goal { get; init; } = GoalOption.Ganon;

    public AccessibilityOption Accessibility { get; init; } = AccessibilityOption.Items;

    public StateOption State { get; init; } = StateOption.Open;

    public GlitchesOption Glitches { get; init; } = GlitchesOption.None;

    public List<TechOption> Techs { get; init; } = new();

    public WeaponOption Weapon { get; init; } = WeaponOption.Randomized;

    public EntranceShuffleOption EntranceShuffle { get; init; } = EntranceShuffleOption.None;

    public EnemyShuffleOption EnemyShuffle { get; init; } = EnemyShuffleOption.None;

    public EnemyDamageOption EnemyDamage { get; init; } = EnemyDamageOption.Default;

    public EnemyHealthOption EnemyHealth { get; init; } = EnemyHealthOption.Default;

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
    public GoalIconOption GoalIcon { get; init; } = GoalIconOption.Triforce;
    public ushort GoalRequiredCount { get; init; } = 0; // default 30/50 triforce pieces
    public HeartColorOption HeartColor { get; init; } = HeartColorOption.Red;
    public HeartBeepSpeedOption HeartBeepSpeed { get; init; } = HeartBeepSpeedOption.Half;
    public MenuSpeedOption MenuSpeed { get; init; } = MenuSpeedOption.Normal;
    public GanonAgahnimRngOption GanonAgahnimRNG { get; init; } = GanonAgahnimRngOption.Table;
    public SilversEquipOption SilversAutoEquip { get; init; } = SilversEquipOption.Collection;
    public CompassCounterOption CompassCounter { get; init; } = CompassCounterOption.Off;
    public bool RevealBootsLocation { get; init; } = false;
    public bool EnableHints { get; init; } = false;
}

public enum GoalOption { Ganon, FastGanon, Dungeons, Pedestal, TriforceHunt, Trifecta }
public enum AccessibilityOption { Items, Locations, None }
public enum StateOption { Standard, Inverted, Open, Retro }
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
public enum EnemyDamageOption { Default, Shuffled, Random }
public enum EnemyHealthOption { Default, Easy, Medium, Hard, Expert }
public enum HeartColorOption { Red, Blue, Green, Yellow, Random }
public enum HeartBeepSpeedOption { Off = 0x00, Double = 0x10, Normal = 0x20, Half = 0x40, Quarter = 0x80 }
public enum MenuSpeedOption { Slow = 0x04, Normal = 0x08, Fast = 0x10, Instant = 0xE8 }
public enum GoalIconOption { Triforce, Star }
public enum GanonAgahnimRngOption { None = 0x01, Table = 0x00, Vanilla = Table }
public enum SilversEquipOption { Off = 0x00, Collection = 0x01, Ganon = 0x02, Both = 0x03 }
public enum CompassCounterOption { Off = 0x00, Pickup = 0x01, On = 0x02 }
