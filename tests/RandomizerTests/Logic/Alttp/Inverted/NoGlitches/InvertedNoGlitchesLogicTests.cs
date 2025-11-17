namespace RandomizerTests.Logic.Alttp.Inverted.NoGlitches;

using Randomizer.Games;
using Randomizer.Games.Alttp;
using RandomizerTests.Logic.Alttp;

public abstract class InvertedNoGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Alttp = new()
        {
            Glitches = GlitchesOption.None,
            State = StateOption.Inverted,
        }
    };
}
