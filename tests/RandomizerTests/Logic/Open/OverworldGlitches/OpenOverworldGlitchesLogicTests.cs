namespace RandomizerTests.Logic.Open.OverworldGlitches;

using Randomizer.Games;
using Randomizer.Games.Alttp;

[Ignore("Skipped until logic is implemented")]
public abstract class OpenOverworldGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Alttp = new()
        {
            Glitches = GlitchesOption.Overworld,
            State = StateOption.Open,
        }
    };
}
