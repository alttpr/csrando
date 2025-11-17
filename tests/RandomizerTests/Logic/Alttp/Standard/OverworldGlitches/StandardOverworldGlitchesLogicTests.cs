namespace RandomizerTests.Logic.Alttp.Standard.OverworldGlitches;

using Randomizer.Games;
using Randomizer.Games.Alttp;
using RandomizerTests.Logic.Alttp;

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
