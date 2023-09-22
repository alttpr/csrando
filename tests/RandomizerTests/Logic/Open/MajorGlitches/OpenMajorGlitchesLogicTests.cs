using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Open.MajorGlitches;

public abstract class OpenMajorGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Glitches = GlitchesOption.Major,
        State = StateOption.Open,
    };
}
