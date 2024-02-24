namespace RandomizerTests.Logic.Open.MajorGlitches;

using Randomizer.Graph;

[Ignore("Skipped until logic is implemented")]
public abstract class OpenMajorGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Glitches = GlitchesOption.Major,
        State = StateOption.Open,
    };
}
