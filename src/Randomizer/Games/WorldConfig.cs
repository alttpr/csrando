namespace Randomizer.Games;

using Randomizer.Games.Metadata;
using AlttpConfig = Alttp.Config;
using Goonies2Config = Goonies2.Config;
using Zelda1Config = Zelda1.Config;

public class WorldConfig
{
    // game/text language. english only at the moment.
    [Values("en")]
    public string Language { get; init; } = "en";
    // randomizer target (which determines the active settings that follow)
    public RandomizerTarget Game { get; init; } = RandomizerTarget.Alttpr;

    [UsableWith(RandomizerTarget.Alttpr /*, GameRandomizer.Quad */)]
    public AlttpConfig? Alttp { get; init; }
    [UsableWith(RandomizerTarget.G2R)]
    public Goonies2Config? Goonies2 { get; init; }
    [UsableWith(RandomizerTarget.Z1R /*, GameRandomizer.Quad */)]
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
public enum Game
{
    [Description("The Goonies II: The Fratellis' Last Stand")]
    Goonies2,
    [Description("The Legend of Zelda")]
    Zelda1,
    [Description("The Legend of Zelda: A Link to the Past")]
    Zelda3,
}
