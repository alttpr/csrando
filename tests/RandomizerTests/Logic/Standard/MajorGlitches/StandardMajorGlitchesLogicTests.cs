namespace RandomizerTests.Logic.Standard.MajorGlitches;

using Randomizer.Games.Alttp;
using Randomizer.Graph;

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
