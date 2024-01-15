namespace RandomizerTests.Logic.Metroid.NoGlitches;

using Randomizer.Graph;

public abstract class NoGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Glitches = GlitchesOption.None,
        State = StateOption.Open,
    };
}
