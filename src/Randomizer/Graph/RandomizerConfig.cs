namespace AlttpRandomizer.Graph;

using System.Collections.Generic;

public class RandomizerConfig
{
    // TODO: Align this with current website which broke it down to multiple settings.
    // See https://github.com/sporchia/alttp_vt_randomizer/pull/951
    public int RomHardMode { get; init; } = 0;

    public int CrystalsGanon { get; init; } = 7;
    public int CrystalsTower { get; init; } = 7;

    public GoalOption Goal { get; init; } = GoalOption.Ganon;

    public AccessibilityOption Accessibility { get; init; } = AccessibilityOption.Items;

    public StateOption State { get; init; } = StateOption.Standard;

    public GlitchesOption Glitches { get; init; } = GlitchesOption.None;

    public List<TechOption> Techs { get; init; } = new();

    public WeaponOption Weapon { get; init; } = WeaponOption.Randomized;

    public EntranceShuffleOption EntranceShuffle { get; init; } = EntranceShuffleOption.None;

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

    public List<string> StartingEquipment { get; init; } = new()
    {
        "BossHeartContainer",
        "BossHeartContainer",
        "BossHeartContainer",
    };
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
