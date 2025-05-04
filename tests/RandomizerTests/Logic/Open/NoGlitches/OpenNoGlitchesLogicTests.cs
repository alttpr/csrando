namespace RandomizerTests.Logic.Open.NoGlitches;

using Randomizer.Games;
using Randomizer.Games.Alttp;

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
