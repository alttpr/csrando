namespace RandomizerTests.Logic.Open.NoGlitches;

using Randomizer.Graph;

public abstract class OpenNoGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Glitches = GlitchesOption.None,
        State = StateOption.Open,
    };
}
