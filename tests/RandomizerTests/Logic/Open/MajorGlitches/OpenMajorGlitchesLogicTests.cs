namespace RandomizerTests.Logic.Open.MajorGlitches;

using Randomizer.Graph;

public abstract class OpenMajorGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Glitches = GlitchesOption.Major,
        State = StateOption.Open,
    };
}
