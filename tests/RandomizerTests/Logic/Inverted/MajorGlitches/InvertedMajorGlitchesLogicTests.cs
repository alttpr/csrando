using AlttpRandomizer.Graph;

namespace RandomizerTests.Logic.Inverted.MajorGlitches;

public abstract class InvertedMajorGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Glitches = GlitchesOption.Major,
        State = StateOption.Inverted,
    };
}
