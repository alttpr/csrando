namespace RandomizerTests.Logic.Standard.MajorGlitches;

using Randomizer.Graph;

[Ignore("Skipped until logic is implemented")]
public abstract class StandardMajorGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Glitches = GlitchesOption.Major,
        State = StateOption.Standard,
    };
}
