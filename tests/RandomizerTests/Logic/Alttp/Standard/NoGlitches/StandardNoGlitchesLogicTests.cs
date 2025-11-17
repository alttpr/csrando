namespace RandomizerTests.Logic.Alttp.Standard.NoGlitches;

using Randomizer.Games;
using Randomizer.Games.Alttp;
using RandomizerTests.Logic.Alttp;

public abstract class StandardNoGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Alttp = new()
        {
            Glitches = GlitchesOption.None,
            State = StateOption.Standard,
        }
    };
}
