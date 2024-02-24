namespace RandomizerTests.Logic.Open.OverworldGlitches;

using Randomizer.Graph;

[Ignore("Skipped until logic is implemented")]
public abstract class OpenOverworldGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Glitches = GlitchesOption.Overworld,
        State = StateOption.Open,
    };
}
