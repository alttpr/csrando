namespace RandomizerTests.Logic.Standard.NoGlitches;

using Randomizer.Games;
using Randomizer.Games.Alttp;

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
