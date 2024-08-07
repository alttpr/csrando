using System.ComponentModel;

namespace RandomizerWeb.Server.Wrapper;

[DefaultValue(Normal)]
public enum GameMode
{
    [Description("Single player")]
    Normal,
    [Description("Multiworld")]
    Multiworld,
}

[DefaultValue(Normal)]
public enum Z3Logic
{
    [Description("Normal")]
    Normal,
    [Description("No major glitches")]
    Nmg,
    [Description("Overworld glitches")]
    Owg,
}

[DefaultValue(Normal)]
public enum SMLogic
{
    [Description("Normal")]
    Normal,
    [Description("Hard")]
    Hard,
}

[DefaultValue(Randomized)]
public enum SwordLocation
{
    [Description("Randomized")]
    Randomized,
    [Description("Early")]
    Early,
    [Description("Uncle assured")]
    Uncle,
}

[DefaultValue(Randomized)]
public enum MorphLocation
{
    [Description("Randomized")]
    Randomized,
    [Description("Early")]
    Early,
    [Description("Original location")]
    Original,
}

[DefaultValue(DefeatAll)]
public enum Goal
{
    [Description("Defeat all four end-bosses")]
    DefeatAll,
    //[Description("Fast Ganon and Defeat Mother Brain")]
    //FastGanonDefeatMotherBrain,
    //[Description("All Dungeons and Defeat Mother Brain")]
    //AllDungeonsDefeatMotherBrain,
    [Description("Triforce hunt")]
    TriforceHunt,
}

[DefaultValue(None)]
public enum KeyShuffle
{
    [Description("None")]
    None,
    [Description("ALTTP Keys and SM Keycards")]
    Keysanity,
    [Description("ALTTP Keys")]
    Z3Keys,
    [Description("SM Keycards")]
    SMKeycards
}

[DefaultValue(SevenCrystals)]
public enum OpenTower
{
    [Description("Random")]
    Random = -1,
    [Description("No Crystals")]
    NoCrystals = 0,
    [Description("One Crystal")]
    OneCrystal = 1,
    [Description("Two Crystals")]
    TwoCrystals = 2,
    [Description("Three Crystals")]
    ThreeCrystals = 3,
    [Description("Four Crystals")]
    FourCrystals = 4,
    [Description("Five Crystals")]
    FiveCrystals = 5,
    [Description("Six Crystals")]
    SixCrystals = 6,
    [Description("Seven Crystals")]
    SevenCrystals = 7,
}

[DefaultValue(SevenCrystals)]
public enum GanonVulnerable
{
    [Description("Random")]
    Random = -1,
    [Description("No Crystals")]
    NoCrystals = 0,
    [Description("One Crystal")]
    OneCrystal = 1,
    [Description("Two Crystals")]
    TwoCrystals = 2,
    [Description("Three Crystals")]
    ThreeCrystals = 3,
    [Description("Four Crystals")]
    FourCrystals = 4,
    [Description("Five Crystals")]
    FiveCrystals = 5,
    [Description("Six Crystals")]
    SixCrystals = 6,
    [Description("Seven Crystals")]
    SevenCrystals = 7,
}

[DefaultValue(FourBosses)]
public enum OpenSMTourian
{
    [Description("Random")]
    Random = -1,
    [Description("No Bosses")]
    NoBosses = 0,
    [Description("One Boss")]
    OneBoss = 1,
    [Description("Two Bosses")]
    TwoBosses = 2,
    [Description("Three Bosses")]
    ThreeBosses = 3,
    [Description("Four Bosses")]
    FourBosses = 4,
}

[DefaultValue(EightTriforces)]
public enum Z1Triforces
{
    [Description("Random")]
    Random = -1,
    [Description("No Triforces")]
    NoTriforces = 0,
    [Description("One Triforce")]
    OneTriforce = 1,
    [Description("Two Triforces")]
    TwoTriforces = 2,
    [Description("Three Triforces")]
    ThreeTriforces = 3,
    [Description("Four Triforces")]
    FourTriforces = 4,
    [Description("Five Triforces")]
    FiveTriforces = 5,
    [Description("Six Triforces")]
    SixTriforces = 6,
    [Description("Seven Triforces")]
    SevenTriforces = 7,
    [Description("Eight Triforces")]
    EightTriforces = 8
}

[DefaultValue(None)]
public enum Z1EntranceShuffle
{
    [Description("None")]
    None,
    [Description("Overworld")]
    Overworld,
}

public class Config
{

    public GameMode GameMode { get; set; } = GameMode.Normal;
    public Goal Goal { get; set; } = Goal.DefeatAll;
    public bool Race { get; set; } = false;
    public OpenTower OpenTower { get; set; } = OpenTower.SevenCrystals;
    public GanonVulnerable GanonVulnerable { get; set; } = GanonVulnerable.SevenCrystals;
    public OpenSMTourian OpenSMTourian { get; set; } = OpenSMTourian.FourBosses;
    public Z1Triforces Z1Triforces { get; set; } = Z1Triforces.EightTriforces;
    public Z1EntranceShuffle Z1EntranceShuffle { get; set; } = Z1EntranceShuffle.None;
    public KeyShuffle KeyShuffle { get; set; } = KeyShuffle.None;


    public bool SingleWorld => GameMode == GameMode.Normal;
    public bool MultiWorld => GameMode == GameMode.Multiworld;
    public bool Keysanity => KeyShuffle != KeyShuffle.None;

}
