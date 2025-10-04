namespace Randomizer.Games;

using Randomizer.Games.Metadata;
using AlttpConfig = Alttp.Config;
using Goonies2Config = Goonies2.Config;
using Zelda1Config = Zelda1.Config;
using MetroidConfig = Metroid.Config;
using SuperMetroidConfig = SuperMetroid.Config;
using ComboConfig = Combo.Config;

public class WorldConfig
{
    // game/text language. english only at the moment.
    [Values("en")]
    public string Language { get; init; } = "en";
    // randomizer target (which determines the active settings that follow)
    public RandomizerTarget Game { get; init; } = RandomizerTarget.Alttpr;

    [UsableWith(RandomizerTarget.Alttpr, RandomizerTarget.Combo)]
    public AlttpConfig? Alttp { get; init; }
    [UsableWith(RandomizerTarget.G2R)]
    public Goonies2Config? Goonies2 { get; init; }
    [UsableWith(RandomizerTarget.Combo)]
    public Zelda1Config? Zelda1 { get; init; }
    [UsableWith(RandomizerTarget.Combo)]
    public SuperMetroidConfig? SuperMetroid { get; init; }
    [UsableWith(RandomizerTarget.Combo)]
    public MetroidConfig? Metroid { get; init; }
    [UsableWith(RandomizerTarget.Combo)]
    public ComboConfig? Combo { get; init; }
}

public enum RandomizerTarget
{
    [Description("The Legend of Zelda: A Link to the Past Randomizer")]
    Alttpr,
    [Description("The Legend of Zelda Randomizer")]
    Z1R,
    [Description("The Goonies II Randomizer")]
    G2R,
    [Description("Combo Randomizer")]
    Combo,
}
public enum Game
{
    [Description("The Goonies II: The Fratellis' Last Stand")]
    Goonies2,
    [Description("The Legend of Zelda")]
    Zelda1,
    [Description("The Legend of Zelda: A Link to the Past")]
    Zelda3,
    [Description("Super Metroid")]
    SuperMetroid,
    [Description("Metroid")]
    Metroid,
    [Description("Combo")]
    Combo,
}
