namespace RandomizerTests.Logic.Open.NoGlitches;

using AlttpRandomizer.Graph;

public abstract class OpenNoGlitchesLogicTests : LogicTestBase
{
    protected override RandomizerConfig GetWorldConfig() => new()
    {
        Glitches = GlitchesOption.None,
        State = StateOption.Open,
    };
}
