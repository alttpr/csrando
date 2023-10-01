namespace RandomizerTests.Logic.Open.OverworldGlitches;

using Randomizer.Graph;

public abstract class OpenOverworldGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Glitches = GlitchesOption.Overworld,
        State = StateOption.Open,
    };
}
