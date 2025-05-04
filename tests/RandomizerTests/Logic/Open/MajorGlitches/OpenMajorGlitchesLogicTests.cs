namespace RandomizerTests.Logic.Open.MajorGlitches;

using Randomizer.Games;
using Randomizer.Games.Alttp;

[Ignore("Skipped until logic is implemented")]
public abstract class OpenMajorGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Alttp = new()
        {
            Glitches = GlitchesOption.Major,
            State = StateOption.Open,
        }
    };
}
