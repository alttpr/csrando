using Randomizer.Graph;

namespace RandomizerTests.Logic.Standard.MajorGlitches;

public abstract class StandardMajorGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Glitches = GlitchesOption.Major,
        State = StateOption.Standard,
    };
}
