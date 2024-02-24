namespace RandomizerTests.Logic.Standard.OverworldGlitches;

using Randomizer.Graph;

[Ignore("Skipped until logic is implemented")]
public abstract class StandardOverworldGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Glitches = GlitchesOption.Overworld,
        State = StateOption.Standard,
    };
}
