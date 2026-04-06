namespace Randomizer.Games.Alttp;

using System.Collections.Generic;
using Randomizer.Games.Metadata;
using Randomizer.Graph;

[TargetGame(Game.Zelda3)]
public class Config
{
    public static readonly string[] RandomCrystals = ["0", "1", "2", "3", "4", "5", "6", "7"];
    public const int DefaultMoldormEyeCount = 2;
    public const int DefaultMolderp = 2;

    // TODO: Align this with current website which broke it down to multiple settings.
    // See https://github.com/sporchia/alttp_vt_randomizer/pull/951
    [Ignore("This should really just be a boolean")]
    public int RomHardMode { get; init; } = 0;

    // Use an array of allowed random values for randomizing the number of crystals.
    // A single-element array acts as specific count to use.
    [RandomizedOptionsFor(nameof(CrystalsGanon))]
    [Values("0", "1", "2", "3", "4", "5", "6", "7", "Dungeons")]
    public string[] CrystalsGanonChoices { get; set; } = RandomCrystals;
    [RandomizedOptionsFor(nameof(CrystalsTower))]
    [Values("0", "1", "2", "3", "4", "5", "6", "7")]
    public string[] CrystalsTowerChoices { get; set; } = RandomCrystals;
    private int[] _moldormEyeCountChoices = [DefaultMoldormEyeCount];
    [RandomizedOptionsFor(nameof(MoldormEyeCount))]
    [ValueRange(0, 8)]
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
    [RandomizedOptionsFor(nameof(Molderp))]
    [ValueRange(1, 8)]
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
    [Category("Goal")]
    [Values("0", "1", "2", "3", "4", "5", "6", "7", "Dungeons", Default = "7")]
    public string CrystalsGanon
    {
        get => _crystalsGanon ?? "7";
        set => _crystalsGanon = value;
    }
    private string? _crystalsTower;
    [Category("Goal")]
    [Values("0", "1", "2", "3", "4", "5", "6", "7", Default = "7")]
    public string CrystalsTower
    {
        get => _crystalsTower ?? "7";
        set => _crystalsTower = value;
    }

    private int? _moldormEyeCount;
    [ValueRange(0, 8, Default = DefaultMoldormEyeCount)]
    [Category("Cosmetic", CategoryDisplay.Collapsed)]
    public int MoldormEyeCount
    {
        get => _moldormEyeCount.GetValueOrDefault(DefaultMoldormEyeCount);
        init => _moldormEyeCount = value;
    }
    private int? _molderp;
    [ValueRange(1, 8, Default = DefaultMolderp)]
    [Category("Cosmetic")]
    [Description("Everyone loves a derpy moldorm. Works best with 2 or more eyes.")]
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

    [Category("Goal")]
    [ValueRange(1, 150, Default = 50)]
    [DependsOn(nameof(Goal), GoalOption.TriforceHunt, GoalOption.Trifecta)]
    public ushort TriforcePieces { get; set; } = 50;

    [Category("Goal")]
    [ValueRange(1, 150, Default = 30)]
    [DependsOn(nameof(Goal), GoalOption.TriforceHunt, GoalOption.Trifecta)]
    public ushort GoalRequiredCount { get; init; } = 30; // default 30/50 triforce pieces


    [Category("Goal")]
    public GoalOption Goal { get; init; } = GoalOption.Ganon;

    [Category("Item Placement", CategoryDisplay.Collapsed)]
    public AccessibilityOption Accessibility { get; init; } = AccessibilityOption.Items;

    [Category("Gameplay")]
    [Default(StateOption.Open)]
    public StateOption State { get; init; } = StateOption.Open;

    [Category("Item Placement")]
    [Wip("Glitch logic is not yet fully implemented")]
    public GlitchesOption Glitches { get; init; } = GlitchesOption.None;

    [Ignore("We don't have enough tech options to make this worthwhile.")]
    public List<TechOption> Techs { get; init; } = [];

    public WeaponOption Weapon { get; init; } = WeaponOption.Randomized;

    [Category("Gameplay")]
    public EntranceShuffleOption EntranceShuffle { get; init; } = EntranceShuffleOption.None;

    [Category("Gameplay")]
    public EnemyShuffleOption EnemyShuffle { get; init; } = EnemyShuffleOption.None;

    public EnemyDamageOption EnemyDamage { get; init; } = EnemyDamageOption.Default;

    public EnemyHealthOption EnemyHealth { get; init; } = EnemyHealthOption.Default;

    [Category("Gameplay")]
    public BossShuffleOption BossShuffle { get; init; } = BossShuffleOption.None;

    [Category("Cosmetic")]
    public TileRoomPatternOption TileRoomPattern { get; init; } = TileRoomPatternOption.Default;

    [Category("Item Placement")]
    [Name("Shop inventory")]
    [Wip("Has no logical regard for money, and might require excessive rupee farming.")]
    public ShopSupplyOption RegionShopSupply { get; init; } = ShopSupplyOption.Normal;

    [Category("Item Placement")]
    [Subcategory("Dungeon Item Shuffle")]
    [Name("Small Keys shuffled outside dungeon")]
    [Description("If No, small keys that are randomly placed will be restricted to their respective dungeons. If Yes, they will be able to be randomly placed in any item location. This does not affect manually placed small keys.")]
    public bool RegionWildKeys { get; init; } = false;
    [Category("Item Placement")]
    [Subcategory("Dungeon Item Shuffle")]
    [Name("Big Keys shuffled outside dungeon")]
    [Description("If No, big keys that are randomly placed will be restricted to their respective dungeons. If Yes, they will be able to be randomly placed in any item location. This does not affect manually placed big keys.")]
    public bool RegionWildBigKeys { get; init; } = false;
    [Category("Item Placement")]
    [Subcategory("Dungeon Item Shuffle")]
    [Name("Maps shuffled outside dungeon")]
    [Description("If No, maps that are randomly placed will be restricted to their respective dungeons. If Yes, they will be able to be randomly placed in any item location. This does not affect manually placed maps.")]
    public bool RegionWildMaps { get; init; } = false;
    [Category("Item Placement")]
    [Subcategory("Dungeon Item Shuffle")]
    [Name("Compasses shuffled outside dungeon")]
    [Description("If No, compasses that are randomly placed will be restricted to their respective dungeons. If Yes, they will be able to be randomly placed in any item location. This does not affect manually placed compasses.")]
    public bool RegionWildCompasses { get; init; } = false;

    [Name("Retro bow")]
    [Description("Zelda 1 style Bow that consumes rupees, will also remove arrows as drops and replace them with blue rupees.")]
    public bool RomRupeeBow { get; init; } = false;

    // TODO: Add configuration for custom prize packs
    [Ignore("No custom prize packs yet")]
    public bool CustomPrizePacks { get; init; } = false;

    [Ignore("Starting inventory is too advenced to be represented with simple attributes")]
    public List<string> StartingEquipment { get; init; } = [];

    [Name("Only display Crystals/Pendants on Map Pickup")]
    [Description("If No, the overworld map will show uncollected crystals and pendants over their respective dungeons. If Yes, the overworld map will only display uncollected crystals and pendants if Link has collected their respective maps.")]
    public bool MapOnPickup { get; init; } = false;
    [DependsOn(nameof(State), StateOption.Standard)]
    [Description("Provides unlimited magic, arrows or bombs during escape if Uncle gives you a weapon requiring resources.")]
    public bool EscapeAssist { get; init; } = false;
    public bool PseudoBoots { get; init; } = false;
    [Ignore("Is there even a reason to turn this off?")]
    public bool FastRom { get; init; } = true;
    [Ignore("Exposed as a post-generation setting")]
    public bool QuickSwap { get; init; } = false;
    [Ignore("Exposed as a post-generation setting")]
    public bool NoMusic { get; init; } = false;
    [Advanced("Directly provides a ROM value")]
    [Default(0x04)]
    [Name("Cape magic usage (normal)")]
    public byte CapeMagicUsageNormal { get; init; } = 0x04;
    [Advanced("Directly provides a ROM value")]
    [Default(0x08)]
    [Name("Cape magic usage (1/2)")]
    public byte CapeMagicUsageHalf { get; init; } = 0x08;
    [Advanced("Directly provides a ROM value")]
    [Default(0x10)]
    [Name("Cape magic usage (1/4)")]
    public byte CapeMagicUsageQuarter { get; init; } = 0x10;
    [Default(true)]
    public bool CaneOfByrnaInvulnerability { get; init; } = true;
    [Advanced("Directly provides a ROM value")]
    [Default(0xE3)]
    [Name("Powdered sprite prize")]
    [Description("Set the sprite that spawns when powdered sprite that usually spawns a faerie is powdered.")]
    public byte PowderedSpriteFairyPrize { get; init; } = 0xE3;
    [Advanced("Directly provides a ROM value")]
    [Default(0xA0)]
    [Description("How much Health refills from Bottles")]
    public byte BottleFillHealth { get; init; } = 0xA0;
    [Advanced("Directly provides a ROM value")]
    [Default(0x80)]
    [Description("How much Magic refills from Bottles")]
    public byte BottleFillMagic { get; init; } = 0x80;
    [Default(true)]
    public bool CatchableFairies { get; init; } = true;
    [Default(true)]
    public bool CatchableBees { get; init; } = true;
    [Default(true)]
    [Name("Hookshot stuns enemies")]
    public bool StunItemsHookshot { get; init; } = true;
    [Default(true)]
    [Name("Boomerang stuns enemies")]
    public bool StunItemsBoomerang { get; init; } = true;
    public bool SilversOnlyAtGanon { get; init; } = false;
    [Name("Retro keys")]
    [Description("All Small keys will be converted to Generic keys.")]
    public bool GenericKeys { get; init; } = false;
    public bool HudItemCounter { get; init; } = false;
    [Ignore("Nobody cares about the goal icon")]
    public GoalIconOption GoalIcon { get; init; } = GoalIconOption.Triforce;
    [Ignore("Exposed as a post-generation setting")]
    public HeartColorOption HeartColor { get; init; } = HeartColorOption.Red;
    [Ignore("Exposed as a post-generation setting")]
    public HeartBeepSpeedOption HeartBeepSpeed { get; init; } = HeartBeepSpeedOption.Half;
    [Ignore("Exposed as a post-generation setting")]
    public MenuSpeedOption MenuSpeed { get; init; } = MenuSpeedOption.Normal;
    [Default(GanonAgahnimRngOption.Table)]
    public GanonAgahnimRngOption GanonAgahnimRNG { get; init; } = GanonAgahnimRngOption.Table;
    [Default(SilversEquipOption.Collection)]
    public SilversEquipOption SilversAutoEquip { get; init; } = SilversEquipOption.Collection;
    public CompassCounterOption CompassCounter { get; init; } = CompassCounterOption.Off;
    public bool RevealBootsLocation { get; init; } = false;
    [Category("Gameplay")]
    public bool EnableHints { get; init; } = false;
}

public enum TileRoomPatternOption
{
    Default,
    Random,
}

public enum GoalOption { Ganon, FastGanon, Dungeons, Pedestal, TriforceHunt, Trifecta }
public enum AccessibilityOption { Items, Locations, None }
public enum StateOption { Standard, Inverted, Open, /*Retro*/ }
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

[PostGenSettingsFor("alttp", Target = RandomizerTarget.Alttpr)]
[PostGenSettingsFor("alttp", Target = RandomizerTarget.Combo, AddressOffset = 0x400000)]
public sealed class PostGenConfig
{
    [Name("Quick Swap")]
    [Description("Enable item quick swap (L/R toggles items)")]
    [OnPatch(0x18004B, 0x01)]
    [OffPatch(0x18004B, 0x00)]
    public bool QuickSwap { get; init; } = true;

    [Name("Music")]
    [Description("Enable in-game music")]
    [OnPatch(0x18021A, 0x00)]
    [OffPatch(0x18021A, 0x01)]
    public bool EnableMusic { get; init; } = true;

    [Name("MSU-1 Resume")]
    [Description("Enable MSU-1 music resume")]
    [OffPatch(0x18021D, 0x00)]
    [OffPatch(0x18021E, 0x00)]
    public bool Msu1Resume { get; init; } = true;

    [Name("Menu Speed")]
    [Description("Set in-game menu menu scroll speed")]
    public MenuSpeed Menu { get; init; } = MenuSpeed.Normal;

    [Name("Heart Color")]
    [Description("Select heart HUD color")]
    public HeartColor HeartHudColor { get; init; } = HeartColor.Red;

    [Name("Heart Beep Speed")]
    [Description("Low-health beep frequency")]
    public HeartBeepSpeed HeartBeep { get; init; } = HeartBeepSpeed.Normal;

    [Name("Reduce Flashes")]
    [Description("Reduce flashing effects")]
    [OnPatch(0x18017F, 0x01)]
    [OffPatch(0x18017F, 0x00)]
    public bool ReduceFlashing { get; init; } = false;
}

public enum MenuSpeed
{
    // instant: writes main byte and sets 3 addresses to 0x20
    [Choice("instant", "Instant")]
    [ChoicePatch(0x180048, 0xE8)]
    [ChoicePatch(0x006DD9A, 0x20)]
    [ChoicePatch(0x006DF2A, 0x20)]
    [ChoicePatch(0x006E0E9, 0x20)]
    Instant,

    [Choice("fast", "Fast")]
    [ChoicePatch(0x180048, 0x10)]
    [ChoicePatch(0x006DD9A, 0x11)]
    [ChoicePatch(0x006DF2A, 0x12)]
    [ChoicePatch(0x006E0E9, 0x12)]
    Fast,

    [Choice("normal", "Normal")]
    [ChoicePatch(0x180048, 0x08)]
    [ChoicePatch(0x006DD9A, 0x11)]
    [ChoicePatch(0x006DF2A, 0x12)]
    [ChoicePatch(0x006E0E9, 0x12)]
    Normal,

    [Choice("slow", "Slow")]
    [ChoicePatch(0x180048, 0x04)]
    [ChoicePatch(0x006DD9A, 0x11)]
    [ChoicePatch(0x006DF2A, 0x12)]
    [ChoicePatch(0x006E0E9, 0x12)]
    Slow,
}

public enum HeartColor
{
    [Choice("red", "Red")]
    [ChoicePatch(0x187020, 0x00)]
    Red,

    [Choice("blue", "Blue")]
    [ChoicePatch(0x187020, 0x01)]
    Blue,

    [Choice("green", "Green")]
    [ChoicePatch(0x187020, 0x02)]
    Green,

    [Choice("yellow", "Yellow")]
    [ChoicePatch(0x187020, 0x03)]
    Yellow,
}

public enum HeartBeepSpeed
{
    [Choice("off", "Off")]
    [ChoicePatch(0x180033, 0x00)]
    Off,

    [Choice("normal", "Normal")]
    [ChoicePatch(0x180033, 0x20)]
    Normal,

    [Choice("half", "Half Speed")]
    [ChoicePatch(0x180033, 0x40)]
    Half,

    [Choice("quarter", "Quarter Speed")]
    [ChoicePatch(0x180033, 0x80)]
    Quarter,

    [Choice("double", "Double Speed")]
    [ChoicePatch(0x180033, 0x10)]
    Double,
}
