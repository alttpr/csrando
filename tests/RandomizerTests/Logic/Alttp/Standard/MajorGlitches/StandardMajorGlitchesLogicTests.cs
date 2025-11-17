namespace RandomizerTests.Logic.Alttp.Standard.MajorGlitches;

using Randomizer.Games;
using Randomizer.Games.Alttp;
using RandomizerTests.Logic.Alttp;

[Ignore("Skipped until logic is implemented")]
public abstract class StandardMajorGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Alttp = new()
        {
            Glitches = GlitchesOption.Major,
            State = StateOption.Standard,
        }
    };
}
