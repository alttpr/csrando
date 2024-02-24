using Randomizer.Graph;

namespace RandomizerTests.Logic.Inverted.OverworldGlitches;

[Ignore("Skipped until logic is implemented")]
public abstract class InvertedOverworldGlitchesLogicTests : LogicTestBase
{
    protected override WorldConfig GetWorldConfig() => new()
    {
        Glitches = GlitchesOption.Overworld,
        State = StateOption.Inverted,
    };
}
