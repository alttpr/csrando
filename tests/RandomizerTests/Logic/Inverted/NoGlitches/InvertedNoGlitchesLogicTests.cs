namespace RandomizerTests.Logic.Inverted.NoGlitches;

using Randomizer.Games.Alttp;
using Randomizer.Graph;

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
