namespace Randomizer.Games;

using AlttpConfig = Alttp.Config;
using Goonies2Config = Goonies2.Config;
using Zelda1Config = Zelda1.Config;

public class WorldConfig
{
    // game/text language. english only at the moment.
    public string Language { get; init; } = "en";
    // randomizer target (which determines the active settings that follow)
    public RandomizerTarget Game { get; init; } = RandomizerTarget.Alttpr;
    public AlttpConfig? Alttp { get; init; }
    public Goonies2Config? Goonies2 { get; init; }
    public Zelda1Config? Zelda1 { get; init; }
}

public enum RandomizerTarget
{
    [Description("The Legend of Zelda: A Link to the Past Randomizer")]
    Alttpr,
    [Description("The Legend of Zelda Randomizer")]
    Z1R,
    [Description("The Goonies II Randomizer")]
    G2R,
    //Quad,
}
