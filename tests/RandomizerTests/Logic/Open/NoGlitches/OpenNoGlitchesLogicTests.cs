namespace RandomizerTests.Logic.Open.NoGlitches;

using Randomizer.Games.Alttp;
using Randomizer.Graph;

public abstract class OpenNoGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Alttp = new()
        {
            Glitches = GlitchesOption.None,
            State = StateOption.Open,
        }
    };
}
