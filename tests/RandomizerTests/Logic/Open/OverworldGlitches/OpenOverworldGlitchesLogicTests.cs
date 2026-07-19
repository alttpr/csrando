namespace RandomizerTests.Logic.Open.OverworldGlitches;

using Randomizer.Games;
using Randomizer.Games.Alttp;

public abstract class OpenOverworldGlitchesLogicTests : LogicTestBase
{
    [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
    public static void ClassInit(TestContext testContext)
    {
        Assert.Inconclusive("Skipped until logic is implemented");
    }
    protected override WorldConfig GetWorldConfig() => new()
    {
        Alttp = new()
        {
            Glitches = GlitchesOption.Overworld,
            State = StateOption.Open,
        }
    };
}
