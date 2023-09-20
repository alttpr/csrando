namespace RandomizerTests.Logic.Standard.NoGlitches;

using AlttpRandomizer.Graph;

public abstract class StandardNoGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Glitches = GlitchesOption.None,
        State = StateOption.Standard,
    };
}
