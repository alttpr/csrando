namespace RandomizerTests.Logic.Open.OverworldGlitches;

using Randomizer.Games.Alttp;
using Randomizer.Graph;

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
