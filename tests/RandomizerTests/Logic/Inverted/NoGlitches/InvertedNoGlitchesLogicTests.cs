namespace RandomizerTests.Logic.Inverted.NoGlitches;

using Randomizer.Graph;

public abstract class InvertedNoGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Glitches = GlitchesOption.None,
        State = StateOption.Inverted,
    };
}
