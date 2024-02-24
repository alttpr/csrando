using Randomizer.Graph;

namespace RandomizerTests.Logic.Inverted.MajorGlitches;

[Ignore("Skipped until logic is implemented")]
public abstract class InvertedMajorGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Glitches = GlitchesOption.Major,
        State = StateOption.Inverted,
    };
}
