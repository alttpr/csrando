namespace RandomizerTests.Logic.Standard.MajorGlitches;

using Randomizer.Graph;

public abstract class StandardMajorGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Glitches = GlitchesOption.Major,
        State = StateOption.Standard,
    };
}
