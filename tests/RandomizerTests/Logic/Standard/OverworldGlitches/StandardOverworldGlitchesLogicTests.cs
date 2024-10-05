namespace RandomizerTests.Logic.Standard.OverworldGlitches;

using Randomizer.Games.Alttp;
using Randomizer.Graph;

[Ignore("Skipped until logic is implemented")]
public abstract class StandardOverworldGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Alttp = new()
        {
            Glitches = GlitchesOption.Overworld,
            State = StateOption.Standard,
        }
    };
}
