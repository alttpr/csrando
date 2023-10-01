namespace RandomizerTests.Logic.Standard.OverworldGlitches;

using Randomizer.Graph;

public abstract class StandardOverworldGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Glitches = GlitchesOption.Overworld,
        State = StateOption.Standard,
    };
}
