using Randomizer.Graph;

namespace RandomizerTests.Logic.Open.OverworldGlitches;

public abstract class OpenOverworldGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Glitches = GlitchesOption.Overworld,
        State = StateOption.Open,
    };
}
