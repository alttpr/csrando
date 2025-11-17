namespace RandomizerTests.Logic.Alttp.Open.NoGlitches;

using Randomizer.Games;
using Randomizer.Games.Alttp;
using RandomizerTests.Logic.Alttp;

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
