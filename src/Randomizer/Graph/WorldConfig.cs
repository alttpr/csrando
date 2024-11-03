namespace Randomizer.Graph;

using AlttpConfig = global::Randomizer.Games.Alttp.Config;
using Goonies2Config = global::Randomizer.Games.Goonies2.Config;
using Zelda1Config = global::Randomizer.Games.Zelda1.Config;

public class WorldConfig
{
    // game/text language. english only at the moment.
    public string Language { get; init; } = "en";
    public AlttpConfig? Alttp { get; init; }
    public Goonies2Config? Goonies2 { get; init; }
    public Zelda1Config? Zelda1 { get; init; }
}
